using System.ComponentModel.DataAnnotations;
using EcommerceAPI.Models;

namespace EcommerceAPI.DTOs
{
    /// <summary>
    /// DTO for returning discount code information
    /// </summary>
    public class DiscountCodeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; }
        public decimal Value { get; set; }
        public decimal? MinimumPurchase { get; set; }
        public decimal? MaximumDiscount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int? TotalUsageLimit { get; set; }
        public int UsedCount { get; set; }
        public int? PerUserLimit { get; set; }
        public int? BuyQuantity { get; set; }
        public int? GetQuantity { get; set; }
        public int? TargetProductId { get; set; }
        public string? TargetProductName { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<int> ApplicableProductIds { get; set; } = new();
        public List<int> ApplicableCategoryIds { get; set; } = new();

        // Computed properties
        public bool IsExpired { get; set; }
        public bool IsUsageLimitReached { get; set; }
        public bool IsValid { get; set; }
    }

    /// <summary>
    /// Request for creating a new discount code
    /// </summary>
    public class CreateDiscountCodeRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public DiscountType DiscountType { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal Value { get; set; }

        [Range(0, 1000000)]
        public decimal? MinimumPurchase { get; set; }

        [Range(0, 1000000)]
        public decimal? MaximumDiscount { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        [Range(1, 1000000)]
        public int? TotalUsageLimit { get; set; }

        [Range(1, 100)]
        public int? PerUserLimit { get; set; }

        // Buy X Get Y fields
        [Range(1, 100)]
        public int? BuyQuantity { get; set; }

        [Range(1, 100)]
        public int? GetQuantity { get; set; }

        public int? TargetProductId { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }

        // Product/Category restrictions
        public List<int> ApplicableProductIds { get; set; } = new();
        public List<int> ApplicableCategoryIds { get; set; } = new();
    }

    /// <summary>
    /// Request for updating an existing discount code
    /// </summary>
    public class UpdateDiscountCodeRequest
    {
        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public bool? IsActive { get; set; }

        [Range(1, 1000000)]
        public int? TotalUsageLimit { get; set; }

        public List<int>? ApplicableProductIds { get; set; }
        public List<int>? ApplicableCategoryIds { get; set; }
    }

    /// <summary>
    /// Request for applying a coupon to a cart
    /// </summary>
    public class ApplyCouponRequest
    {
        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;

        // For guest users
        public string? SessionId { get; set; }
    }

    /// <summary>
    /// Response for coupon validation with discount details
    /// </summary>
    public class CouponValidationResponse
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public DiscountType? DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; } // Calculated discount
        public decimal CartSubTotal { get; set; }
        public decimal FinalTotal { get; set; }
        public List<int> EligibleProductIds { get; set; } = new();
        public string? SuccessMessage { get; set; }

        // Buy X Get Y details
        public int? FreeItemsAdded { get; set; }
        public string? FreeItemProductName { get; set; }
    }

    /// <summary>
    /// Request for removing a coupon from cart
    /// </summary>
    public class RemoveCouponRequest
    {
        public string? SessionId { get; set; } // For guest users
    }

    /// <summary>
    /// Statistics for discount code usage
    /// </summary>
    public class DiscountCodeStatsDto
    {
        public int TotalCodes { get; set; }
        public int ActiveCodes { get; set; }
        public int ExpiredCodes { get; set; }
        public int TotalUsages { get; set; }
        public decimal TotalDiscountGiven { get; set; }
    }
}
