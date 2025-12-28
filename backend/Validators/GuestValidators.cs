using FluentValidation;
using Backend.DTOs;

namespace EcommerceAPI.Validators;

public class GuestCartRequestValidator : AbstractValidator<GuestCartRequest>
{
    public GuestCartRequestValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.")
            .MaximumLength(100).WithMessage("Session ID must not exceed 100 characters.");
    }
}

public class AddToGuestCartRequestValidator : AbstractValidator<AddToGuestCartRequest>
{
    public AddToGuestCartRequestValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.")
            .MaximumLength(100).WithMessage("Session ID must not exceed 100 characters.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product must be selected.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Quantity must not exceed 100 per order.");
    }
}

public class UpdateGuestCartItemRequestValidator : AbstractValidator<UpdateGuestCartItemRequest>
{
    public UpdateGuestCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Quantity must not exceed 100 per order.");
    }
}

public class GuestCheckoutRequestValidator : AbstractValidator<GuestCheckoutRequest>
{
    public GuestCheckoutRequestValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.")
            .MaximumLength(100).WithMessage("Session ID must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^[\d\s\(\)\+\-]+$").WithMessage("Invalid phone number format.");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.")
            .SetValidator(new ShippingAddressDtoValidator());

        RuleFor(x => x.CouponCode)
            .MaximumLength(50).WithMessage("Coupon code must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CouponCode));
    }
}

public class GuestOrderTrackingRequestValidator : AbstractValidator<GuestOrderTrackingRequest>
{
    public GuestOrderTrackingRequestValidator()
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Order number is required.")
            .MaximumLength(50).WithMessage("Order number must not exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}

public class MergeGuestCartRequestValidator : AbstractValidator<MergeGuestCartRequest>
{
    public MergeGuestCartRequestValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.")
            .MaximumLength(100).WithMessage("Session ID must not exceed 100 characters.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid user ID is required.");
    }
}
