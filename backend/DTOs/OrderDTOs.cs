using EcommerceAPI.Models;

namespace EcommerceAPI.DTOs;

public class OrderDto
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public ShippingAddressDto ShippingAddress { get; set; } = new();
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateOrderRequest
{
    public ShippingAddressDto ShippingAddress { get; set; } = new();
    public string PaymentMethod { get; set; } = "Stripe";
    public string? CouponCode { get; set; }
}

public class ShippingAddressDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
    public string? TrackingNumber { get; set; }
    public string? EstimatedDelivery { get; set; }
}

public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
}
