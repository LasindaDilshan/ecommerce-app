using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<PagedResult<UserDto>> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        // Validate and sanitize pagination parameters to prevent DoS
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100); // Max 100 items per page

        var query = _context.Users.AsQueryable();

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<UserDto>
        {
            Items = users,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;

        await _context.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return false;
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new Exception("Current password is incorrect");
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateUserRoleAsync(int id, UpdateUserRoleRequest request)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return false;
        }

        if (request.Role != "Admin" && request.Role != "User")
        {
            throw new Exception("Invalid role");
        }

        user.Role = request.Role;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return false;
        }

        user.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }
}
