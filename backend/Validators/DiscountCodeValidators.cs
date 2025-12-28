using FluentValidation;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Validators;

public class CreateDiscountCodeRequestValidator : AbstractValidator<CreateDiscountCodeRequest>
{
    public CreateDiscountCodeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Discount code is required.")
            .MinimumLength(3).WithMessage("Code must be at least 3 characters long.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("Code can only contain uppercase letters, numbers, and hyphens.");

        RuleFor(x => x.DiscountType)
            .IsInEnum().WithMessage("Invalid discount type.");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0.")
            .LessThanOrEqualTo(1000000).WithMessage("Discount value must not exceed 1,000,000.")
            .Must((request, value) => {
                // For percentage discounts, value should be between 1 and 100
                if (request.DiscountType == DiscountType.Percentage)
                    return value >= 1 && value <= 100;
                return true;
            }).WithMessage("Percentage discount value must be between 1 and 100.");

        RuleFor(x => x.MinimumPurchase)
            .GreaterThan(0).WithMessage("Minimum purchase must be greater than 0.")
            .LessThanOrEqualTo(1000000).WithMessage("Minimum purchase must not exceed 1,000,000.")
            .When(x => x.MinimumPurchase.HasValue);

        RuleFor(x => x.MaximumDiscount)
            .GreaterThan(0).WithMessage("Maximum discount must be greater than 0.")
            .LessThanOrEqualTo(1000000).WithMessage("Maximum discount must not exceed 1,000,000.")
            .When(x => x.MaximumDiscount.HasValue);

        RuleFor(x => x.ValidFrom)
            .NotEmpty().WithMessage("Valid from date is required.");

        RuleFor(x => x.ValidTo)
            .NotEmpty().WithMessage("Valid to date is required.")
            .GreaterThan(x => x.ValidFrom).WithMessage("Valid to date must be after valid from date.");

        RuleFor(x => x.TotalUsageLimit)
            .GreaterThanOrEqualTo(1).WithMessage("Total usage limit must be at least 1.")
            .LessThanOrEqualTo(1000000).WithMessage("Total usage limit must not exceed 1,000,000.")
            .When(x => x.TotalUsageLimit.HasValue);

        RuleFor(x => x.PerUserLimit)
            .GreaterThanOrEqualTo(1).WithMessage("Per user limit must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Per user limit must not exceed 100.")
            .When(x => x.PerUserLimit.HasValue);

        // Buy X Get Y specific validation
        RuleFor(x => x.BuyQuantity)
            .GreaterThanOrEqualTo(1).WithMessage("Buy quantity must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Buy quantity must not exceed 100.")
            .NotEmpty().WithMessage("Buy quantity is required for Buy X Get Y discount type.")
            .When(x => x.DiscountType == DiscountType.BuyXGetY);

        RuleFor(x => x.GetQuantity)
            .GreaterThanOrEqualTo(1).WithMessage("Get quantity must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Get quantity must not exceed 100.")
            .NotEmpty().WithMessage("Get quantity is required for Buy X Get Y discount type.")
            .When(x => x.DiscountType == DiscountType.BuyXGetY);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public class UpdateDiscountCodeRequestValidator : AbstractValidator<UpdateDiscountCodeRequest>
{
    public UpdateDiscountCodeRequestValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom.GetValueOrDefault())
            .WithMessage("Valid to date must be after valid from date.")
            .When(x => x.ValidTo.HasValue && x.ValidFrom.HasValue);

        RuleFor(x => x.TotalUsageLimit)
            .GreaterThanOrEqualTo(1).WithMessage("Total usage limit must be at least 1.")
            .LessThanOrEqualTo(1000000).WithMessage("Total usage limit must not exceed 1,000,000.")
            .When(x => x.TotalUsageLimit.HasValue);
    }
}

public class ApplyCouponRequestValidator : AbstractValidator<ApplyCouponRequest>
{
    public ApplyCouponRequestValidator()
    {
        RuleFor(x => x.CouponCode)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Coupon code must not exceed 50 characters.");

        RuleFor(x => x.SessionId)
            .MaximumLength(100).WithMessage("Session ID must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SessionId));
    }
}
