using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<PagedResult<UserDto>> GetAllUsersAsync(int pageNumber, int pageSize);
    Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request);
    Task<bool> UpdateUserRoleAsync(int id, UpdateUserRoleRequest request);
    Task<bool> DeactivateUserAsync(int id);
}
