using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IEmailService emailService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);

            // Send welcome email
            try
            {
                var userName = $"{request.FirstName} {request.LastName}";
                await _emailService.SendWelcomeEmailAsync(request.Email, userName);
                _logger.LogInformation($"Welcome email sent to {request.Email}");
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, $"Failed to send welcome email to {request.Email}");
                // Don't fail registration if email fails
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RevokeTokenAsync(request.RefreshToken);
            return Ok(new { success = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
