using FluentValidation;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Validators;

public class CreateWarehouseRequestValidator : AbstractValidator<CreateWarehouseRequest>
{
    public CreateWarehouseRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(100).WithMessage("Warehouse name must not exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Warehouse code is required.")
            .MaximumLength(20).WithMessage("Warehouse code must not exceed 20 characters.")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("Code can only contain uppercase letters, numbers, and hyphens.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(100).WithMessage("State must not exceed 100 characters.");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.")
            .MaximumLength(20).WithMessage("Zip code must not exceed 20 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
    }
}

public class UpdateStockRequestValidator : AbstractValidator<UpdateStockRequest>
{
    public UpdateStockRequestValidator()
    {
        RuleFor(x => x.StockItemId)
            .GreaterThan(0).WithMessage("Valid stock item must be selected.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(100000).WithMessage("Quantity must not exceed 100,000.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid stock movement type.");

        RuleFor(x => x.Reference)
            .MaximumLength(100).WithMessage("Reference must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reference));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class StockAdjustmentRequestValidator : AbstractValidator<StockAdjustmentRequest>
{
    public StockAdjustmentRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product must be selected.");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0).WithMessage("Valid warehouse must be selected.");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.")
            .LessThanOrEqualTo(100000).WithMessage("Quantity must not exceed 100,000.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(200).WithMessage("Reason must not exceed 200 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class CreateBatchRequestValidator : AbstractValidator<CreateBatchRequest>
{
    public CreateBatchRequestValidator()
    {
        RuleFor(x => x.StockItemId)
            .GreaterThan(0).WithMessage("Valid stock item must be selected.");

        RuleFor(x => x.BatchNumber)
            .NotEmpty().WithMessage("Batch number is required.")
            .MaximumLength(50).WithMessage("Batch number must not exceed 50 characters.");

        RuleFor(x => x.LotNumber)
            .MaximumLength(50).WithMessage("Lot number must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.LotNumber));

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(100000).WithMessage("Quantity must not exceed 100,000.");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.ManufactureDate).WithMessage("Expiry date must be after manufacture date.")
            .When(x => x.ManufactureDate.HasValue && x.ExpiryDate.HasValue);

        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Invalid supplier ID.")
            .When(x => x.SupplierId.HasValue);

        RuleFor(x => x.PurchaseCost)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase cost cannot be negative.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class CreateStockTransferRequestValidator : AbstractValidator<CreateStockTransferRequest>
{
    public CreateStockTransferRequestValidator()
    {
        RuleFor(x => x.FromWarehouseId)
            .GreaterThan(0).WithMessage("Source warehouse must be selected.");

        RuleFor(x => x.ToWarehouseId)
            .GreaterThan(0).WithMessage("Destination warehouse must be selected.")
            .NotEqual(x => x.FromWarehouseId).WithMessage("Source and destination warehouse must be different.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item must be included.")
            .Must(x => x.Count <= 100).WithMessage("Cannot transfer more than 100 items at once.");

        RuleForEach(x => x.Items).SetValidator(new StockTransferItemRequestValidator());

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class StockTransferItemRequestValidator : AbstractValidator<StockTransferItemRequest>
{
    public StockTransferItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product must be selected.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(10000).WithMessage("Quantity must not exceed 10,000.");

        RuleFor(x => x.BatchNumber)
            .MaximumLength(50).WithMessage("Batch number must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.BatchNumber));
    }
}

public class CreateSupplierRequestValidator : AbstractValidator<CreateSupplierRequest>
{
    public CreateSupplierRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Supplier code is required.")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters.")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("Code can only contain uppercase letters, numbers, and hyphens.");

        RuleFor(x => x.ContactPerson)
            .NotEmpty().WithMessage("Contact person is required.")
            .MaximumLength(100).WithMessage("Contact person must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.");

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("Website must not exceed 200 characters.")
            .Must(BeAValidUrl).WithMessage("Invalid website URL.")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(100).WithMessage("State must not exceed 100 characters.");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.")
            .MaximumLength(20).WithMessage("Zip code must not exceed 20 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

        RuleFor(x => x.TaxId)
            .MaximumLength(50).WithMessage("Tax ID must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.TaxId));

        RuleFor(x => x.PaymentTermsDays)
            .InclusiveBetween(0, 365).WithMessage("Payment terms must be between 0 and 365 days.");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100.")
            .When(x => x.DiscountPercentage.HasValue);
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

public class CreatePurchaseOrderRequestValidator : AbstractValidator<CreatePurchaseOrderRequest>
{
    public CreatePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier must be selected.");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0).WithMessage("Warehouse must be selected.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item must be included.")
            .Must(x => x.Count <= 100).WithMessage("Cannot order more than 100 items at once.");

        RuleForEach(x => x.Items).SetValidator(new PurchaseOrderItemRequestValidator());

        RuleFor(x => x.ExpectedDeliveryDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expected delivery date must be in the future.")
            .When(x => x.ExpectedDeliveryDate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class PurchaseOrderItemRequestValidator : AbstractValidator<PurchaseOrderItemRequest>
{
    public PurchaseOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product must be selected.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(10000).WithMessage("Quantity must not exceed 10,000.");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0).WithMessage("Unit cost cannot be negative.")
            .LessThanOrEqualTo(1000000).WithMessage("Unit cost must not exceed 1,000,000.");
    }
}

public class StockReservationRequestValidator : AbstractValidator<StockReservationRequest>
{
    public StockReservationRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product must be selected.");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0).WithMessage("Valid warehouse must be selected.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(10000).WithMessage("Quantity must not exceed 10,000.");

        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Invalid order ID.")
            .When(x => x.OrderId.HasValue);

        RuleFor(x => x.CartItemId)
            .GreaterThan(0).WithMessage("Invalid cart item ID.")
            .When(x => x.CartItemId.HasValue);

        RuleFor(x => x.ExpirationMinutes)
            .InclusiveBetween(1, 1440).WithMessage("Expiration must be between 1 and 1440 minutes (24 hours).");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
