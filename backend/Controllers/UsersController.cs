using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user identifier");
        }
        return userId;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userService.GetUserByIdAsync(GetUserId());

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(GetUserId(), request);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            await _userService.ChangePasswordAsync(GetUserId(), request);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _userService.GetAllUsersAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            await _userService.UpdateUserRoleAsync(id, request);
            return Ok(new { message = "User role updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var result = await _userService.DeactivateUserAsync(id);

        if (!result)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(new { message = "User deactivated successfully" });
    }
}
