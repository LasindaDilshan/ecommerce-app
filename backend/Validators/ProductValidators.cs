using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.")
            .MinimumLength(3).WithMessage("Product name must be at least 3 characters long.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThanOrEqualTo(1000000).WithMessage("Price must not exceed 1,000,000.")
            .PrecisionScale(10, 2, false).WithMessage("Price can have at most 2 decimal places.");

        RuleFor(x => x.DiscountPrice)
            .LessThan(x => x.Price).WithMessage("Discount price must be less than regular price.")
            .GreaterThan(0).WithMessage("Discount price must be greater than 0.")
            .When(x => x.DiscountPrice.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
            .LessThanOrEqualTo(100000).WithMessage("Stock quantity must not exceed 100,000.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("SKU can only contain uppercase letters, numbers, and hyphens.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Valid category must be selected.");
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.")
            .MinimumLength(3).WithMessage("Product name must be at least 3 characters long.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThanOrEqualTo(1000000).WithMessage("Price must not exceed 1,000,000.")
            .PrecisionScale(10, 2, false).WithMessage("Price can have at most 2 decimal places.");

        RuleFor(x => x.DiscountPrice)
            .LessThan(x => x.Price).WithMessage("Discount price must be less than regular price.")
            .GreaterThan(0).WithMessage("Discount price must be greater than 0.")
            .When(x => x.DiscountPrice.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
            .LessThanOrEqualTo(100000).WithMessage("Stock quantity must not exceed 100,000.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Valid category must be selected.");
    }
}

public class ProductQueryParametersValidator : AbstractValidator<ProductQueryParameters>
{
    public ProductQueryParametersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum price cannot be negative.")
            .LessThan(x => x.MaxPrice).WithMessage("Minimum price must be less than maximum price.")
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThan(0).WithMessage("Maximum price must be greater than 0.")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x.SortBy)
            .Must(x => new[] { "Name", "Price", "CreatedAt", "StockQuantity" }.Contains(x))
            .WithMessage("SortBy must be one of: Name, Price, CreatedAt, StockQuantity.");

        RuleFor(x => x.SortOrder)
            .Must(x => new[] { "asc", "desc" }.Contains(x.ToLower()))
            .WithMessage("SortOrder must be 'asc' or 'desc'.");
    }
}
