using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Authenticator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using BCrypt.Net;

namespace EcommerceAPI.Services
{
    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<TwoFactorAuthService> _logger;
        private readonly string _issuer;
        private readonly int _lockoutMinutes = 15;
        private readonly int _maxFailedAttempts = 5;

        public TwoFactorAuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<TwoFactorAuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
            _issuer = _configuration["AppSettings:AppName"] ?? "E-Commerce App";
        }

        public async Task<Enable2FAResponse> EnableTwoFactorAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            if (user.TwoFactorEnabled)
            {
                throw new InvalidOperationException("Two-factor authentication is already enabled");
            }

            // Generate a new secret key
            var secret = GenerateSecretKey();
            user.TwoFactorSecret = secret;

            // Generate recovery codes
            var recoveryCodes = GenerateRecoveryCodes(8);
            user.RecoveryCodes = JsonSerializer.Serialize(recoveryCodes);

            await _context.SaveChangesAsync();

            // Generate QR code
            var tfa = new TwoFactorAuthenticator();
            var setupInfo = tfa.GenerateSetupCode(
                _issuer,
                user.Email,
                secret,
                false,
                3);

            _logger.LogInformation($"2FA setup initiated for user {userId}");

            return new Enable2FAResponse
            {
                QrCodeUrl = setupInfo.QrCodeSetupImageUrl,
                ManualEntryKey = setupInfo.ManualEntryKey,
                RecoveryCodes = recoveryCodes
            };
        }

        public async Task<bool> VerifyTwoFactorSetupAsync(int userId, string code)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                return false;
            }

            var tfa = new TwoFactorAuthenticator();
            bool isValid = tfa.ValidateTwoFactorPIN(user.TwoFactorSecret, code);

            if (isValid && !user.TwoFactorEnabled)
            {
                user.TwoFactorEnabled = true;
                user.TwoFactorEnabledAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"2FA enabled for user {userId}");

                // Send confirmation email
                try
                {
                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Two-Factor Authentication Enabled",
                        $"<p>Two-factor authentication has been successfully enabled for your account.</p>" +
                        $"<p>Please keep your recovery codes safe. You'll need them if you lose access to your authenticator app.</p>",
                        "Two-factor authentication has been successfully enabled for your account."
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send 2FA confirmation email");
                }
            }

            return isValid;
        }

        public async Task<bool> DisableTwoFactorAsync(int userId, string code, string password)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.TwoFactorEnabled)
            {
                return false;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return false;
            }

            // Verify 2FA code
            var tfa = new TwoFactorAuthenticator();
            if (!tfa.ValidateTwoFactorPIN(user.TwoFactorSecret, code))
            {
                return false;
            }

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            user.TwoFactorEnabledAt = null;
            user.RecoveryCodes = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"2FA disabled for user {userId}");

            // Send notification email
            try
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Two-Factor Authentication Disabled",
                    $"<p>Two-factor authentication has been disabled for your account.</p>" +
                    $"<p>If you didn't make this change, please contact support immediately.</p>",
                    "Two-factor authentication has been disabled for your account."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send 2FA disabled notification email");
            }

            return true;
        }

        public async Task<TwoFactorStatusResponse> GetTwoFactorStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            return new TwoFactorStatusResponse
            {
                IsEnabled = user.TwoFactorEnabled,
                EnabledAt = user.TwoFactorEnabledAt,
                HasRecoveryCodes = !string.IsNullOrEmpty(user.RecoveryCodes)
            };
        }

        public async Task<bool> ValidateTwoFactorCodeAsync(int userId, string code)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                return false;
            }

            var tfa = new TwoFactorAuthenticator();
            return tfa.ValidateTwoFactorPIN(user.TwoFactorSecret, code);
        }

        public async Task<bool> ValidateRecoveryCodeAsync(int userId, string recoveryCode)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.RecoveryCodes))
            {
                return false;
            }

            var codes = JsonSerializer.Deserialize<List<string>>(user.RecoveryCodes);
            if (codes == null || !codes.Contains(recoveryCode))
            {
                return false;
            }

            // Remove used recovery code
            codes.Remove(recoveryCode);
            user.RecoveryCodes = codes.Any() ? JsonSerializer.Serialize(codes) : null;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Recovery code used for user {userId}");

            return true;
        }

        public async Task<List<string>> RegenerateRecoveryCodesAsync(int userId, string code)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.TwoFactorEnabled)
            {
                throw new InvalidOperationException("Two-factor authentication is not enabled");
            }

            // Verify 2FA code
            if (!await ValidateTwoFactorCodeAsync(userId, code))
            {
                throw new UnauthorizedAccessException("Invalid two-factor authentication code");
            }

            var recoveryCodes = GenerateRecoveryCodes(8);
            user.RecoveryCodes = JsonSerializer.Serialize(recoveryCodes);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Recovery codes regenerated for user {userId}");

            return recoveryCodes;
        }

        public async Task<string> GenerateEmailVerificationTokenAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            if (user.EmailVerified)
            {
                throw new InvalidOperationException("Email is already verified");
            }

            var token = GenerateRandomToken();
            user.EmailVerificationToken = token;
            user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);

            await _context.SaveChangesAsync();

            // Send verification email
            await _emailService.SendAccountVerificationEmailAsync(user.Email, token);

            _logger.LogInformation($"Email verification token generated for user {userId}");

            return token;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
            {
                return false;
            }

            if (user.EmailVerificationTokenExpires < DateTime.UtcNow)
            {
                return false;
            }

            user.EmailVerified = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpires = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Email verified for user {user.Id}");

            return true;
        }

        public async Task<bool> ResendVerificationEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null || user.EmailVerified)
            {
                return false;
            }

            await GenerateEmailVerificationTokenAsync(user.Id);
            return true;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                // Don't reveal if email exists
                return string.Empty;
            }

            var token = GenerateRandomToken();

            // Store token temporarily (in production, use a separate table or cache)
            user.EmailVerificationToken = token; // Reusing field for simplicity
            user.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            // Send password reset email
            await _emailService.SendPasswordResetEmailAsync(user.Email, token);

            _logger.LogInformation($"Password reset token generated for user {user.Id}");

            return token;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

            if (user == null)
            {
                return false;
            }

            if (user.EmailVerificationTokenExpires < DateTime.UtcNow)
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpires = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Password reset for user {user.Id}");

            // Send confirmation email
            try
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Password Changed",
                    "<p>Your password has been successfully changed.</p>" +
                    "<p>If you didn't make this change, please contact support immediately.</p>",
                    "Your password has been successfully changed."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password change notification");
            }

            return true;
        }

        public async Task<bool> IsAccountLockedAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return true;
            }

            return user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow;
        }

        public async Task RecordFailedLoginAttemptAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return;
            }

            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= _maxFailedAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(_lockoutMinutes);
                _logger.LogWarning($"Account locked for user {userId} after {_maxFailedAttempts} failed attempts");
            }

            await _context.SaveChangesAsync();
        }

        public async Task ResetFailedLoginAttemptsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return;
            }

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            await _context.SaveChangesAsync();
        }

        private string GenerateSecretKey()
        {
            var key = Guid.NewGuid().ToString("N").Substring(0, 16);
            return Base32Encode(Encoding.UTF8.GetBytes(key));
        }

        private string GenerateRandomToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");
        }

        private List<string> GenerateRecoveryCodes(int count)
        {
            var codes = new List<string>();
            for (int i = 0; i < count; i++)
            {
                codes.Add(GenerateRecoveryCode());
            }
            return codes;
        }

        private string GenerateRecoveryCode()
        {
            var randomBytes = new byte[6];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "").ToUpper();
        }

        private string Base32Encode(byte[] data)
        {
            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new StringBuilder();
            int buffer = 0;
            int bitsInBuffer = 0;

            foreach (byte b in data)
            {
                buffer = (buffer << 8) | b;
                bitsInBuffer += 8;

                while (bitsInBuffer >= 5)
                {
                    int index = (buffer >> (bitsInBuffer - 5)) & 0x1F;
                    result.Append(base32Chars[index]);
                    bitsInBuffer -= 5;
                }
            }

            if (bitsInBuffer > 0)
            {
                int index = (buffer << (5 - bitsInBuffer)) & 0x1F;
                result.Append(base32Chars[index]);
            }

            return result.ToString();
        }
    }
}