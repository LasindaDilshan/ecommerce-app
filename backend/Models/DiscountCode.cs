using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public enum DiscountType
    {
        Percentage = 1,
        FixedAmount = 2,
        FreeShipping = 3,
        BuyXGetY = 4
    }

    public class DiscountCode
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public DiscountType DiscountType { get; set; }

        // Discount value (percentage or fixed amount)
        public decimal Value { get; set; }

        // Minimum purchase amount required to use this coupon
        public decimal? MinimumPurchase { get; set; }

        // Maximum discount amount (useful for percentage discounts)
        public decimal? MaximumDiscount { get; set; }

        // Validity period
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
        public DateTime ValidTo { get; set; }

        // Usage limits
        public int? TotalUsageLimit { get; set; } // null = unlimited
        public int UsedCount { get; set; } = 0;
        public int? PerUserLimit { get; set; } // null = unlimited per user

        // Buy X Get Y promotion fields
        public int? BuyQuantity { get; set; } // Required quantity to buy
        public int? GetQuantity { get; set; } // Quantity given free
        public int? TargetProductId { get; set; } // Specific product for Buy X Get Y

        // Status
        public bool IsActive { get; set; } = true;

        // Metadata
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Relationships for product/category restrictions
        public virtual ICollection<DiscountCodeProduct> ApplicableProducts { get; set; } = new List<DiscountCodeProduct>();
        public virtual ICollection<DiscountCodeCategory> ApplicableCategories { get; set; } = new List<DiscountCodeCategory>();
        public virtual ICollection<DiscountCodeUsage> Usages { get; set; } = new List<DiscountCodeUsage>();

        // Helper properties
        public bool IsExpired => DateTime.UtcNow > ValidTo || DateTime.UtcNow < ValidFrom;
        public bool IsUsageLimitReached => TotalUsageLimit.HasValue && UsedCount >= TotalUsageLimit.Value;
        public bool IsValid => IsActive && !IsExpired && !IsUsageLimitReached;
    }

    // Junction table for DiscountCode-Product many-to-many relationship
    public class DiscountCodeProduct
    {
        public int DiscountCodeId { get; set; }
        public int ProductId { get; set; }

        public virtual DiscountCode DiscountCode { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }

    // Junction table for DiscountCode-Category many-to-many relationship
    public class DiscountCodeCategory
    {
        public int DiscountCodeId { get; set; }
        public int CategoryId { get; set; }

        public virtual DiscountCode DiscountCode { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
    }
}
