using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.")
            .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Category description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.");

        RuleFor(x => x.ParentCategoryId)
            .GreaterThan(0).WithMessage("Invalid parent category.")
            .When(x => x.ParentCategoryId.HasValue);
    }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.")
            .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Category description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.");

        RuleFor(x => x.ParentCategoryId)
            .GreaterThan(0).WithMessage("Invalid parent category.")
            .When(x => x.ParentCategoryId.HasValue);
    }
}
