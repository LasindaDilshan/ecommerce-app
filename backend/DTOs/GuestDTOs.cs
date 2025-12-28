using Backend.DTOs;
using EcommerceAPI.DTOs;

namespace Backend.DTOs;

// Guest Cart DTOs
public class GuestCartRequest
{
    public string SessionId { get; set; } = string.Empty;
}

public class AddToGuestCartRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateGuestCartItemRequest
{
    public int Quantity { get; set; }
}

// Guest Checkout DTOs
public class GuestCheckoutRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public ShippingAddressDto ShippingAddress { get; set; } = new();
    public string? CouponCode { get; set; }
}

public class GuestOrderResponse
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
}

public class GuestOrderTrackingRequest
{
    public string OrderNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class MergeGuestCartRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int UserId { get; set; }
}
