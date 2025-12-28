using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators;

public class CreateSubscriptionPlanRequestValidator : AbstractValidator<CreateSubscriptionPlanRequest>
{
    public CreateSubscriptionPlanRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Plan code is required.")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters.")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("Code can only contain uppercase letters, numbers, and hyphens.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.BillingInterval)
            .IsInEnum().WithMessage("Invalid billing interval.");

        RuleFor(x => x.BillingIntervalCount)
            .GreaterThan(0).WithMessage("Billing interval count must be greater than 0.")
            .LessThanOrEqualTo(12).WithMessage("Billing interval count must not exceed 12.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThanOrEqualTo(10000).WithMessage("Price must not exceed 10,000.");

        RuleFor(x => x.SetupFee)
            .GreaterThanOrEqualTo(0).WithMessage("Setup fee cannot be negative.")
            .LessThanOrEqualTo(1000).WithMessage("Setup fee must not exceed 1,000.")
            .When(x => x.SetupFee.HasValue);

        RuleFor(x => x.TrialPeriodDays)
            .InclusiveBetween(1, 90).WithMessage("Trial period must be between 1 and 90 days.")
            .When(x => x.TrialPeriodDays.HasValue);

        RuleFor(x => x.Features)
            .Must(x => x.Count <= 20).WithMessage("Cannot have more than 20 features.");

        RuleForEach(x => x.Features)
            .NotEmpty().WithMessage("Feature cannot be empty.")
            .MaximumLength(200).WithMessage("Feature must not exceed 200 characters.");

        RuleForEach(x => x.Products).SetValidator(new AddProductToPlanRequestValidator());
    }
}

public class AddProductToPlanRequestValidator : AbstractValidator<AddProductToPlanRequest>
{
    public AddProductToPlanRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product must be selected.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Quantity must not exceed 100.");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100.")
            .When(x => x.DiscountPercentage.HasValue);
    }
}

public class CreateSubscriptionRequestValidator : AbstractValidator<CreateSubscriptionRequest>
{
    public CreateSubscriptionRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid user must be selected.");

        RuleFor(x => x.PlanId)
            .GreaterThan(0).WithMessage("Valid plan must be selected.");

        RuleFor(x => x.PaymentMethodId)
            .MaximumLength(100).WithMessage("Payment method ID must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethodId));

        RuleFor(x => x.ShippingAddressId)
            .GreaterThan(0).WithMessage("Invalid shipping address ID.")
            .When(x => x.ShippingAddressId.HasValue);
    }
}

public class UpdateSubscriptionRequestValidator : AbstractValidator<UpdateSubscriptionRequest>
{
    public UpdateSubscriptionRequestValidator()
    {
        RuleFor(x => x.NewPlanId)
            .GreaterThan(0).WithMessage("Invalid plan ID.")
            .When(x => x.NewPlanId.HasValue);

        RuleFor(x => x.ShippingAddressId)
            .GreaterThan(0).WithMessage("Invalid shipping address ID.")
            .When(x => x.ShippingAddressId.HasValue);

        RuleFor(x => x.PaymentMethodId)
            .MaximumLength(100).WithMessage("Payment method ID must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethodId));
    }
}

public class PauseSubscriptionRequestValidator : AbstractValidator<PauseSubscriptionRequest>
{
    public PauseSubscriptionRequestValidator()
    {
        RuleFor(x => x.PauseUntil)
            .GreaterThan(DateTime.UtcNow).WithMessage("Pause date must be in the future.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddMonths(3)).WithMessage("Cannot pause subscription for more than 3 months.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}

public class CancelSubscriptionRequestValidator : AbstractValidator<CancelSubscriptionRequest>
{
    public CancelSubscriptionRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}

public class CreateReturnRequestValidator : AbstractValidator<CreateReturnRequest>
{
    public CreateReturnRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Valid order must be selected.");

        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("Invalid return reason.");

        RuleFor(x => x.Comments)
            .MaximumLength(1000).WithMessage("Comments must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Comments));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item must be included.");

        RuleForEach(x => x.Items).SetValidator(new ReturnItemRequestValidator());
    }
}

public class ReturnItemRequestValidator : AbstractValidator<ReturnItemRequest>
{
    public ReturnItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product must be selected.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Quantity must not exceed 100.");

        RuleFor(x => x.Condition)
            .IsInEnum().WithMessage("Invalid item condition.");
    }
}

public class ProcessReturnRequestValidator : AbstractValidator<ProcessReturnRequest>
{
    public ProcessReturnRequestValidator()
    {
        RuleFor(x => x.Comments)
            .MaximumLength(1000).WithMessage("Comments must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Comments));

        RuleFor(x => x.RestockingFee)
            .GreaterThanOrEqualTo(0).WithMessage("Restocking fee cannot be negative.")
            .LessThanOrEqualTo(1000).WithMessage("Restocking fee must not exceed 1,000.")
            .When(x => x.RestockingFee.HasValue);

        RuleFor(x => x.RefundMethod)
            .IsInEnum().WithMessage("Invalid refund method.");
    }
}

public class RecoverAbandonedCartRequestValidator : AbstractValidator<RecoverAbandonedCartRequest>
{
    public RecoverAbandonedCartRequestValidator()
    {
        RuleFor(x => x.AbandonedCartId)
            .GreaterThan(0).WithMessage("Valid abandoned cart must be selected.");

        RuleFor(x => x.EmailTemplate)
            .MaximumLength(100).WithMessage("Email template must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.EmailTemplate));

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 50).WithMessage("Discount percentage must be between 0 and 50.")
            .When(x => x.DiscountPercentage.HasValue);
    }
}

public class CreateGiftCardRequestValidator : AbstractValidator<CreateGiftCardRequest>
{
    public CreateGiftCardRequestValidator()
    {
        RuleFor(x => x.Value)
            .InclusiveBetween(1, 1000).WithMessage("Gift card value must be between 1 and 1000.");

        RuleFor(x => x.RecipientEmail)
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.RecipientEmail));

        RuleFor(x => x.RecipientName)
            .MaximumLength(100).WithMessage("Recipient name must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.RecipientName));

        RuleFor(x => x.Message)
            .MaximumLength(500).WithMessage("Message must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Message));

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future.")
            .When(x => x.ExpiresAt.HasValue);
    }
}

public class RedeemGiftCardRequestValidator : AbstractValidator<RedeemGiftCardRequest>
{
    public RedeemGiftCardRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Gift card code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Invalid order ID.")
            .When(x => x.OrderId.HasValue);
    }
}

public class GiftCardBalanceRequestValidator : AbstractValidator<GiftCardBalanceRequest>
{
    public GiftCardBalanceRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Gift card code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");
    }
}
