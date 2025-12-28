using System.Threading.Tasks;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services
{
    public interface ITwoFactorAuthService
    {
        // Two-Factor Authentication
        Task<Enable2FAResponse> EnableTwoFactorAsync(int userId);
        Task<bool> VerifyTwoFactorSetupAsync(int userId, string code);
        Task<bool> DisableTwoFactorAsync(int userId, string code, string password);
        Task<TwoFactorStatusResponse> GetTwoFactorStatusAsync(int userId);
        Task<bool> ValidateTwoFactorCodeAsync(int userId, string code);
        Task<bool> ValidateRecoveryCodeAsync(int userId, string recoveryCode);
        Task<List<string>> RegenerateRecoveryCodesAsync(int userId, string code);

        // Email Verification
        Task<string> GenerateEmailVerificationTokenAsync(int userId);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ResendVerificationEmailAsync(string email);

        // Password Reset
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);

        // Account Security
        Task<bool> IsAccountLockedAsync(int userId);
        Task RecordFailedLoginAttemptAsync(int userId);
        Task ResetFailedLoginAttemptsAsync(int userId);
    }
}