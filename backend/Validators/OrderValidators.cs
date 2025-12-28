using FluentValidation;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.")
            .SetValidator(new ShippingAddressDtoValidator());

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required.")
            .Must(x => new[] { "Stripe", "PayPal", "CreditCard" }.Contains(x))
            .WithMessage("Invalid payment method. Must be one of: Stripe, PayPal, CreditCard.");

        RuleFor(x => x.CouponCode)
            .MaximumLength(50).WithMessage("Coupon code must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CouponCode));
    }
}

public class ShippingAddressDtoValidator : AbstractValidator<ShippingAddressDto>
{
    public ShippingAddressDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.")
            .MinimumLength(5).WithMessage("Address must be at least 5 characters long.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("City can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(100).WithMessage("State must not exceed 100 characters.");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.")
            .MaximumLength(20).WithMessage("Zip code must not exceed 20 characters.")
            .Matches(@"^[A-Za-z0-9\s\-]+$").WithMessage("Invalid zip code format.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Country can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.Phone)
            .Matches(@"^[\d\s\(\)\+\-]+$").WithMessage("Invalid phone number format.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status.");
    }
}
