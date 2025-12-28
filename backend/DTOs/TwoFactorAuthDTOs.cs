using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs
{
    public class Enable2FARequest
    {
        // No input needed, just user authentication
    }

    public class Enable2FAResponse
    {
        public string QrCodeUrl { get; set; } = string.Empty;
        public string ManualEntryKey { get; set; } = string.Empty;
        public List<string> RecoveryCodes { get; set; } = new();
    }

    public class Verify2FARequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
        public string Code { get; set; } = string.Empty;
    }

    public class Disable2FARequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginWith2FARequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
        public string TwoFactorCode { get; set; } = string.Empty;
    }

    public class RecoveryCodeRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string RecoveryCode { get; set; } = string.Empty;
    }

    public class RegenerateRecoveryCodesRequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
        public string TwoFactorCode { get; set; } = string.Empty;
    }

    public class TwoFactorStatusResponse
    {
        public bool IsEnabled { get; set; }
        public DateTime? EnabledAt { get; set; }
        public bool HasRecoveryCodes { get; set; }
    }

    public class EmailVerificationRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }

    public class ResendEmailVerificationRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class PasswordResetRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}