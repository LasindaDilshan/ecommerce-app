using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators;

public class Verify2FARequestValidator : AbstractValidator<Verify2FARequest>
{
    public Verify2FARequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Code must be exactly 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Code must contain only digits.");
    }
}

public class Disable2FARequestValidator : AbstractValidator<Disable2FARequest>
{
    public Disable2FARequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Code must be exactly 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Code must contain only digits.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginWith2FARequestValidator : AbstractValidator<LoginWith2FARequest>
{
    public LoginWith2FARequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.TwoFactorCode)
            .NotEmpty().WithMessage("Two-factor code is required.")
            .Length(6).WithMessage("Code must be exactly 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Code must contain only digits.");
    }
}

public class RecoveryCodeRequestValidator : AbstractValidator<RecoveryCodeRequest>
{
    public RecoveryCodeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.RecoveryCode)
            .NotEmpty().WithMessage("Recovery code is required.")
            .MaximumLength(20).WithMessage("Recovery code must not exceed 20 characters.");
    }
}

public class RegenerateRecoveryCodesRequestValidator : AbstractValidator<RegenerateRecoveryCodesRequest>
{
    public RegenerateRecoveryCodesRequestValidator()
    {
        RuleFor(x => x.TwoFactorCode)
            .NotEmpty().WithMessage("Two-factor code is required.")
            .Length(6).WithMessage("Code must be exactly 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Code must contain only digits.");
    }
}

public class EmailVerificationRequestValidator : AbstractValidator<EmailVerificationRequest>
{
    public EmailVerificationRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification token is required.")
            .MaximumLength(500).WithMessage("Token is too long.");
    }
}

public class ResendEmailVerificationRequestValidator : AbstractValidator<ResendEmailVerificationRequest>
{
    public ResendEmailVerificationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");
    }
}

public class PasswordResetRequestValidator : AbstractValidator<PasswordResetRequest>
{
    public PasswordResetRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.")
            .MaximumLength(500).WithMessage("Token is too long.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}
