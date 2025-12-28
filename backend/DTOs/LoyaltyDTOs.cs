using EcommerceAPI.Models;

namespace EcommerceAPI.DTOs;

public class LoyaltyAccountDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int CurrentPoints { get; set; }
    public int LifetimePoints { get; set; }
    public string Tier { get; set; } = string.Empty;
    public string TierBenefits { get; set; } = string.Empty;
    public int PointsToNextTier { get; set; }
    public double EarningMultiplier { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoyaltyTransactionDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoyaltyRewardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public bool IsFreeShipping { get; set; }
    public string? MinimumTier { get; set; }
    public bool CanRedeem { get; set; }
}

public class RedeemedRewardDto
{
    public int Id { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public string RedemptionCode { get; set; } = string.Empty;
    public int PointsSpent { get; set; }
    public bool IsUsed { get; set; }
    public DateTime RedeemedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class RedeemRewardRequest
{
    public int RewardId { get; set; }
}

public class RedeemRewardResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RedemptionCode { get; set; }
    public int PointsSpent { get; set; }
    public int RemainingPoints { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class LoyaltySummaryDto
{
    public int CurrentPoints { get; set; }
    public int LifetimePoints { get; set; }
    public string Tier { get; set; } = string.Empty;
    public int PointsToNextTier { get; set; }
    public string NextTier { get; set; } = string.Empty;
    public double EarningMultiplier { get; set; }
    public List<string> TierBenefits { get; set; } = new();
    public List<LoyaltyTransactionDto> RecentTransactions { get; set; } = new();
    public List<RedeemedRewardDto> ActiveRewards { get; set; } = new();
}

public class EarnPointsRequest
{
    public int UserId { get; set; }
    public int OrderId { get; set; }
    public decimal OrderTotal { get; set; }
}

public class AdjustPointsRequest
{
    public int UserId { get; set; }
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CreateRewardRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public RewardType Type { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public bool IsFreeShipping { get; set; }
    public LoyaltyTier? MinimumTier { get; set; }
}
