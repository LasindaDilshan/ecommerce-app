namespace EcommerceAPI.Models;

public class Cart
{
    public int Id { get; set; }
    public int? UserId { get; set; } // Nullable for guest carts
    public string? SessionId { get; set; } // For guest cart identification
    public string? GuestEmail { get; set; } // For abandoned cart recovery
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Discount/Coupon tracking
    public string? AppliedCouponCode { get; set; }
    public decimal DiscountAmount { get; set; } = 0;

    // Helper property
    public bool IsGuestCart => UserId == null && !string.IsNullOrEmpty(SessionId);

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Cart Cart { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
