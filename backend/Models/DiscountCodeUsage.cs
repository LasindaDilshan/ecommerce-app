using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    /// <summary>
    /// Tracks individual usage of discount codes for per-user limits
    /// Supports both authenticated users and guest users (via email)
    /// </summary>
    public class DiscountCodeUsage
    {
        public int Id { get; set; }

        public int DiscountCodeId { get; set; }

        // Either UserId or GuestEmail will be set (not both)
        public int? UserId { get; set; } // For authenticated users

        [MaxLength(255)]
        public string? GuestEmail { get; set; } // For guest users

        public int? OrderId { get; set; } // Link to the order where coupon was used

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        public decimal DiscountApplied { get; set; } // Amount of discount applied

        // Navigation properties
        public virtual DiscountCode DiscountCode { get; set; } = null!;
        public virtual User? User { get; set; }
        public virtual Order? Order { get; set; }

        // Helper property
        public bool IsGuestUsage => UserId == null && !string.IsNullOrEmpty(GuestEmail);
    }
}
