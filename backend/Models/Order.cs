namespace EcommerceAPI.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int? UserId { get; set; } // Nullable for guest orders

    // Guest Information (for orders placed without account)
    public string? GuestEmail { get; set; }
    public string? GuestFirstName { get; set; }
    public string? GuestLastName { get; set; }

    // Price breakdown
    public decimal SubTotal { get; set; } // SubTotal after product discounts (if any)
    public decimal DiscountAmount { get; set; } = 0; // Coupon discount amount
    public string? CouponCode { get; set; } // Applied coupon code
    public decimal ShippingCost { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? PaymentIntentId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }

    // Shipping Address
    public string ShippingFirstName { get; set; } = string.Empty;
    public string ShippingLastName { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingZipCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string? ShippingPhone { get; set; }

    // Helper properties
    public bool IsGuestOrder => UserId == null && !string.IsNullOrEmpty(GuestEmail);
    public string CustomerEmail => GuestEmail ?? User?.Email ?? string.Empty;
    public string CustomerFullName => IsGuestOrder
        ? $"{GuestFirstName} {GuestLastName}"
        : $"{User?.FirstName} {User?.LastName}";

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual Payment? Payment { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded
}
