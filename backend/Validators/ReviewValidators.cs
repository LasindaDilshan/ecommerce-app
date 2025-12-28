using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators;

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product must be selected.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MinimumLength(10).WithMessage("Comment must be at least 10 characters.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");

        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Invalid order ID.")
            .When(x => x.OrderId.HasValue);
    }
}

public class UpdateReviewRequestValidator : AbstractValidator<UpdateReviewRequest>
{
    public UpdateReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MinimumLength(10).WithMessage("Comment must be at least 10 characters.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");
    }
}

public class ReviewVoteRequestValidator : AbstractValidator<ReviewVoteRequest>
{
    public ReviewVoteRequestValidator()
    {
        RuleFor(x => x.ReviewId)
            .GreaterThan(0).WithMessage("Valid review must be selected.");
    }
}

public class ReviewModerationRequestValidator : AbstractValidator<ReviewModerationRequest>
{
    public ReviewModerationRequestValidator()
    {
        // All fields are valid booleans, no additional validation needed
    }
}

public class ReviewFilterRequestValidator : AbstractValidator<ReviewFilterRequest>
{
    public ReviewFilterRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.SortBy)
            .Must(x => new[] { "MostRecent", "MostHelpful", "HighestRating", "LowestRating" }.Contains(x))
            .WithMessage("SortBy must be one of: MostRecent, MostHelpful, HighestRating, LowestRating.")
            .When(x => !string.IsNullOrEmpty(x.SortBy));

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
    }
}
