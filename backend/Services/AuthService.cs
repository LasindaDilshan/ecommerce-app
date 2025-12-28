using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using EcommerceAPI.Exceptions;

namespace EcommerceAPI.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new EmailAlreadyExistsException(request.Email);
        }

        // Create new user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Create cart for user (EF Core will handle the relationship after save)
        var cart = new Cart
        {
            User = user,
            CreatedAt = DateTime.UtcNow
        };
        _context.Carts.Add(cart);

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            User = user,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"]!)),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshTokenEntity);

        // Single SaveChangesAsync for all entities
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:ExpiryMinutes"]!)),
            RequiresTwoFactor = false,
            EmailVerified = user.EmailVerified
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new AccountDeactivatedException();
        }

        // Check if account is locked
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            var remainingTime = (user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
            throw new AccountLockedException(remainingTime);
        }

        // Check if 2FA is enabled
        if (user.TwoFactorEnabled)
        {
            // Return partial response indicating 2FA is required
            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                AccessToken = string.Empty, // Don't provide token yet
                RefreshToken = string.Empty, // Don't provide token yet
                ExpiresAt = DateTime.UtcNow,
                RequiresTwoFactor = true,
                EmailVerified = user.EmailVerified
            };
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Revoke old refresh tokens
        var oldTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in oldTokens)
        {
            token.IsRevoked = true;
        }

        // Save new refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"]!)),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:ExpiryMinutes"]!)),
            RequiresTwoFactor = false,
            EmailVerified = user.EmailVerified
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity == null || tokenEntity.IsRevoked)
        {
            throw new InvalidRefreshTokenException();
        }

        if (tokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidRefreshTokenException("Refresh token has expired");
        }

        var user = tokenEntity.User;

        if (!user.IsActive)
        {
            throw new AccountDeactivatedException();
        }

        // Generate new tokens
        var accessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Revoke old refresh token
        tokenEntity.IsRevoked = true;

        // Save new refresh token
        var newTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"]!)),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:ExpiryMinutes"]!))
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity == null)
        {
            return false;
        }

        tokenEntity.IsRevoked = true;
        await _context.SaveChangesAsync();

        return true;
    }

    private string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"]!)),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKey),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
