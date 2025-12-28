using System;
using System.Threading.Tasks;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Exceptions;
using EcommerceAPI.Models;
using EcommerceAPI.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace EcommerceAPI.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly Mock<IConfiguration> _configurationMock;

        public AuthServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Setup configuration mock
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(x => x["JwtSettings:SecretKey"])
                .Returns("ThisIsATestSecretKeyWith256BitsLength!@#$%^&*()");
            _configurationMock.Setup(x => x["JwtSettings:Issuer"])
                .Returns("TestIssuer");
            _configurationMock.Setup(x => x["JwtSettings:Audience"])
                .Returns("TestAudience");
            _configurationMock.Setup(x => x["JwtSettings:ExpiryMinutes"])
                .Returns("60");
            _configurationMock.Setup(x => x["JwtSettings:RefreshTokenExpiryDays"])
                .Returns("7");

            _authService = new AuthService(_context, _configurationMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateNewUser()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Test123!",
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                PhoneNumber = "1234567890"
            };

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(registerDto.Email);
            result.FirstName.Should().Be(registerDto.FirstName);
            result.LastName.Should().Be(registerDto.LastName);

            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            userInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowEmailAlreadyExistsException()
        {
            // Arrange
            var existingUser = new User
            {
                Email = "existing@example.com",
                PasswordHash = "hash",
                FirstName = "Existing",
                LastName = "User",
                Role = UserRole.Customer
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var registerDto = new RegisterDto
            {
                Email = "existing@example.com",
                Password = "Test123!",
                FirstName = "New",
                LastName = "User"
            };

            // Act & Assert
            await _authService.Invoking(s => s.RegisterAsync(registerDto))
                .Should().ThrowAsync<EmailAlreadyExistsException>()
                .WithMessage("*existing@example.com*");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
        {
            // Arrange
            var password = "Test123!";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = passwordHash,
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.Customer
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = password
            };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.Email.Should().Be(user.Email);
            result.Role.Should().Be(user.Role.ToString());
        }

        [Fact]
        public async Task LoginAsync_WithInvalidEmail_ShouldThrowAuthenticationException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "Test123!"
            };

            // Act & Assert
            await _authService.Invoking(s => s.LoginAsync(loginDto))
                .Should().ThrowAsync<AuthenticationException>()
                .WithMessage("Invalid email or password");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldThrowAuthenticationException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.Customer
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            // Act & Assert
            await _authService.Invoking(s => s.LoginAsync(loginDto))
                .Should().ThrowAsync<AuthenticationException>()
                .WithMessage("Invalid email or password");
        }

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "hash",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.Customer,
                RefreshToken = "valid-refresh-token",
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.RefreshTokenAsync("valid-refresh-token");

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBe("valid-refresh-token"); // Should be a new token
        }

        [Fact]
        public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowInvalidRefreshTokenException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "hash",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.Customer,
                RefreshToken = "expired-refresh-token",
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1) // Expired yesterday
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act & Assert
            await _authService.Invoking(s => s.RefreshTokenAsync("expired-refresh-token"))
                .Should().ThrowAsync<InvalidRefreshTokenException>();
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInvalidToken_ShouldThrowInvalidRefreshTokenException()
        {
            // Act & Assert
            await _authService.Invoking(s => s.RefreshTokenAsync("non-existent-token"))
                .Should().ThrowAsync<InvalidRefreshTokenException>();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}