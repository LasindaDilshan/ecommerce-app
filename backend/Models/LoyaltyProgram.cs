using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models;

public class LoyaltyAccount
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    public int CurrentPoints { get; set; } = 0;

    public int LifetimePoints { get; set; } = 0;

    public LoyaltyTier Tier { get; set; } = LoyaltyTier.Bronze;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
}

public class LoyaltyTransaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int LoyaltyAccountId { get; set; }

    [ForeignKey("LoyaltyAccountId")]
    public LoyaltyAccount LoyaltyAccount { get; set; } = null!;

    public int? OrderId { get; set; }

    [ForeignKey("OrderId")]
    public Order? Order { get; set; }

    [Required]
    public LoyaltyTransactionType Type { get; set; }

    [Required]
    public int Points { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LoyaltyReward
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int PointsCost { get; set; }

    public RewardType Type { get; set; }

    // For percentage discount
    public decimal? DiscountPercentage { get; set; }

    // For fixed amount discount
    public decimal? DiscountAmount { get; set; }

    // For free shipping
    public bool IsFreeShipping { get; set; }

    public bool IsActive { get; set; } = true;

    public LoyaltyTier? MinimumTier { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RedeemedReward
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int LoyaltyAccountId { get; set; }

    [ForeignKey("LoyaltyAccountId")]
    public LoyaltyAccount LoyaltyAccount { get; set; } = null!;

    [Required]
    public int LoyaltyRewardId { get; set; }

    [ForeignKey("LoyaltyRewardId")]
    public LoyaltyReward LoyaltyReward { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string RedemptionCode { get; set; } = string.Empty;

    public int PointsSpent { get; set; }

    public bool IsUsed { get; set; } = false;

    public int? UsedOnOrderId { get; set; }

    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }
}

public enum LoyaltyTier
{
    Bronze = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3
}

public enum LoyaltyTransactionType
{
    Earned = 0,
    Redeemed = 1,
    Expired = 2,
    Bonus = 3,
    Adjustment = 4,
    Refund = 5
}

public enum RewardType
{
    PercentageDiscount = 0,
    FixedDiscount = 1,
    FreeShipping = 2,
    FreeProduct = 3
}
