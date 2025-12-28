using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators;

public class SearchRequestValidator : AbstractValidator<SearchRequest>
{
    public SearchRequestValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(200).WithMessage("Search term must not exceed 200 characters.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Invalid category ID.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum price cannot be negative.")
            .LessThan(x => x.MaxPrice).WithMessage("Minimum price must be less than maximum price.")
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThan(0).WithMessage("Maximum price must be greater than 0.")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 5).WithMessage("Rating must be between 0 and 5.")
            .When(x => x.MinRating.HasValue);

        RuleFor(x => x.SortBy)
            .Must(x => new[] { "Relevance", "Name", "PriceAsc", "PriceDesc", "Rating", "Newest" }.Contains(x))
            .WithMessage("SortBy must be one of: Relevance, Name, PriceAsc, PriceDesc, Rating, Newest.")
            .When(x => !string.IsNullOrEmpty(x.SortBy));

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
    }
}

public class AutocompleteRequestValidator : AbstractValidator<AutocompleteRequest>
{
    public AutocompleteRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query is required.")
            .MaximumLength(100).WithMessage("Query must not exceed 100 characters.");

        RuleFor(x => x.Limit)
            .GreaterThanOrEqualTo(1).WithMessage("Limit must be at least 1.")
            .LessThanOrEqualTo(50).WithMessage("Limit must not exceed 50.");
    }
}
