using FluentValidation;
using Backend.DTOs;

namespace EcommerceAPI.Validators;

public class AddToWishlistRequestValidator : AbstractValidator<AddToWishlistRequest>
{
    public AddToWishlistRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product must be selected.");
    }
}

public class MoveToCartRequestValidator : AbstractValidator<MoveToCartRequest>
{
    public MoveToCartRequestValidator()
    {
        RuleFor(x => x.WishlistItemId)
            .GreaterThan(0).WithMessage("Valid wishlist item must be selected.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Quantity must not exceed 100 per order.");
    }
}
