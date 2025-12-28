using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators;

public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters.");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200).WithMessage("Address line 2 must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.AddressLine2));

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(100).WithMessage("State must not exceed 100 characters.");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.")
            .MaximumLength(20).WithMessage("Zip code must not exceed 20 characters.")
            .Matches(@"^[A-Z0-9\-\s]+$").WithMessage("Zip code contains invalid characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .Matches(@"^[\+\d\s\-\(\)]+$").WithMessage("Phone number contains invalid characters.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

public class UpdateAddressRequestValidator : AbstractValidator<UpdateAddressRequest>
{
    public UpdateAddressRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.AddressLine1)
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.AddressLine1));

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200).WithMessage("Address line 2 must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.AddressLine2));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.State));

        RuleFor(x => x.ZipCode)
            .MaximumLength(20).WithMessage("Zip code must not exceed 20 characters.")
            .Matches(@"^[A-Z0-9\-\s]+$").WithMessage("Zip code contains invalid characters.")
            .When(x => !string.IsNullOrEmpty(x.ZipCode));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .Matches(@"^[\+\d\s\-\(\)]+$").WithMessage("Phone number contains invalid characters.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
