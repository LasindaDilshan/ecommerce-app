using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TwoFactorController : ControllerBase
    {
        private readonly ITwoFactorAuthService _twoFactorService;
        private readonly IAuthService _authService;
        private readonly ILogger<TwoFactorController> _logger;

        public TwoFactorController(
            ITwoFactorAuthService twoFactorService,
            IAuthService authService,
            ILogger<TwoFactorController> logger)
        {
            _twoFactorService = twoFactorService;
            _authService = authService;
            _logger = logger;
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

        // Enable 2FA - Generate QR code and recovery codes
        [HttpPost("enable")]
        public async Task<IActionResult> EnableTwoFactor()
        {
            try
            {
                var result = await _twoFactorService.EnableTwoFactorAsync(GetUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling 2FA");
                return StatusCode(500, new { message = "An error occurred while enabling two-factor authentication" });
            }
        }

        // Verify 2FA setup with initial code
        [HttpPost("verify-setup")]
        public async Task<IActionResult> VerifyTwoFactorSetup([FromBody] Verify2FARequest request)
        {
            try
            {
                var success = await _twoFactorService.VerifyTwoFactorSetupAsync(GetUserId(), request.Code);
                if (success)
                {
                    return Ok(new { message = "Two-factor authentication has been enabled successfully" });
                }
                return BadRequest(new { message = "Invalid verification code" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying 2FA setup");
                return StatusCode(500, new { message = "An error occurred while verifying two-factor authentication" });
            }
        }

        // Disable 2FA
        [HttpPost("disable")]
        public async Task<IActionResult> DisableTwoFactor([FromBody] Disable2FARequest request)
        {
            try
            {
                var success = await _twoFactorService.DisableTwoFactorAsync(GetUserId(), request.Code, request.Password);
                if (success)
                {
                    return Ok(new { message = "Two-factor authentication has been disabled" });
                }
                return BadRequest(new { message = "Invalid credentials or verification code" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA");
                return StatusCode(500, new { message = "An error occurred while disabling two-factor authentication" });
            }
        }

        // Get 2FA status
        [HttpGet("status")]
        public async Task<IActionResult> GetTwoFactorStatus()
        {
            try
            {
                var status = await _twoFactorService.GetTwoFactorStatusAsync(GetUserId());
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting 2FA status");
                return StatusCode(500, new { message = "An error occurred while retrieving two-factor authentication status" });
            }
        }

        // Regenerate recovery codes
        [HttpPost("recovery-codes/regenerate")]
        public async Task<IActionResult> RegenerateRecoveryCodes([FromBody] RegenerateRecoveryCodesRequest request)
        {
            try
            {
                var codes = await _twoFactorService.RegenerateRecoveryCodesAsync(GetUserId(), request.TwoFactorCode);
                return Ok(new { recoveryCodes = codes });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating recovery codes");
                return StatusCode(500, new { message = "An error occurred while regenerating recovery codes" });
            }
        }

        // Login with 2FA
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginWithTwoFactor([FromBody] LoginWith2FARequest request)
        {
            try
            {
                // First validate credentials and check if user has 2FA enabled
                var loginRequest = new LoginRequest
                {
                    Email = request.Email,
                    Password = request.Password
                };

                // This will throw if credentials are invalid
                var authResult = await _authService.LoginAsync(loginRequest);

                if (!authResult.RequiresTwoFactor)
                {
                    return Ok(authResult);
                }

                // Validate 2FA code
                bool isValidCode = await _twoFactorService.ValidateTwoFactorCodeAsync(authResult.UserId, request.TwoFactorCode);

                if (!isValidCode)
                {
                    await _twoFactorService.RecordFailedLoginAttemptAsync(authResult.UserId);
                    return Unauthorized(new { message = "Invalid two-factor authentication code" });
                }

                // Reset failed attempts on successful 2FA
                await _twoFactorService.ResetFailedLoginAttemptsAsync(authResult.UserId);

                // Return full auth response with tokens
                authResult.RequiresTwoFactor = false;
                return Ok(authResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during 2FA login");
                return Unauthorized(new { message = "Authentication failed" });
            }
        }

        // Login with recovery code
        [AllowAnonymous]
        [HttpPost("login-recovery")]
        public async Task<IActionResult> LoginWithRecoveryCode([FromBody] RecoveryCodeRequest request)
        {
            try
            {
                var loginRequest = new LoginRequest
                {
                    Email = request.Email,
                    Password = request.Password
                };

                // Validate credentials
                var authResult = await _authService.LoginAsync(loginRequest);

                if (!authResult.RequiresTwoFactor)
                {
                    return Ok(authResult);
                }

                // Validate recovery code
                bool isValidCode = await _twoFactorService.ValidateRecoveryCodeAsync(authResult.UserId, request.RecoveryCode);

                if (!isValidCode)
                {
                    await _twoFactorService.RecordFailedLoginAttemptAsync(authResult.UserId);
                    return Unauthorized(new { message = "Invalid recovery code" });
                }

                // Reset failed attempts
                await _twoFactorService.ResetFailedLoginAttemptsAsync(authResult.UserId);

                authResult.RequiresTwoFactor = false;
                return Ok(authResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during recovery code login");
                return Unauthorized(new { message = "Authentication failed" });
            }
        }

        // Email verification endpoints
        [AllowAnonymous]
        [HttpPost("email/verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] EmailVerificationRequest request)
        {
            try
            {
                var success = await _twoFactorService.VerifyEmailAsync(request.Token);
                if (success)
                {
                    return Ok(new { message = "Email verified successfully" });
                }
                return BadRequest(new { message = "Invalid or expired verification token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email");
                return StatusCode(500, new { message = "An error occurred while verifying email" });
            }
        }

        [HttpPost("email/resend")]
        public async Task<IActionResult> ResendVerificationEmail()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { message = "User email not found" });
                }

                await _twoFactorService.ResendVerificationEmailAsync(userEmail);
                return Ok(new { message = "Verification email sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification email");
                return StatusCode(500, new { message = "An error occurred while sending verification email" });
            }
        }

        // Password reset endpoints
        [AllowAnonymous]
        [HttpPost("password/forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] PasswordResetRequest request)
        {
            try
            {
                await _twoFactorService.GeneratePasswordResetTokenAsync(request.Email);
                // Always return success to prevent email enumeration
                return Ok(new { message = "If the email exists, a password reset link has been sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset request");
                // Still return success to prevent enumeration
                return Ok(new { message = "If the email exists, a password reset link has been sent" });
            }
        }

        [AllowAnonymous]
        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var success = await _twoFactorService.ResetPasswordAsync(request.Token, request.NewPassword);
                if (success)
                {
                    return Ok(new { message = "Password has been reset successfully" });
                }
                return BadRequest(new { message = "Invalid or expired reset token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, new { message = "An error occurred while resetting password" });
            }
        }
    }
}