using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators;

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product must be selected.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Quantity must not exceed 100 per order.");
    }
}

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Quantity must not exceed 100 per order.");
    }
}
