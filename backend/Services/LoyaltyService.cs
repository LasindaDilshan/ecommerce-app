using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services;

public class LoyaltyService : ILoyaltyService
{
    private readonly ApplicationDbContext _context;

    // Points per dollar spent
    private const int BasePointsPerDollar = 1;

    // Tier thresholds (lifetime points)
    private static readonly Dictionary<LoyaltyTier, int> TierThresholds = new()
    {
        { LoyaltyTier.Bronze, 0 },
        { LoyaltyTier.Silver, 500 },
        { LoyaltyTier.Gold, 2000 },
        { LoyaltyTier.Platinum, 5000 }
    };

    // Tier earning multipliers
    private static readonly Dictionary<LoyaltyTier, double> TierMultipliers = new()
    {
        { LoyaltyTier.Bronze, 1.0 },
        { LoyaltyTier.Silver, 1.25 },
        { LoyaltyTier.Gold, 1.5 },
        { LoyaltyTier.Platinum, 2.0 }
    };

    // Tier benefits descriptions
    private static readonly Dictionary<LoyaltyTier, List<string>> TierBenefits = new()
    {
        { LoyaltyTier.Bronze, new List<string> { "Earn 1 point per $1 spent", "Access to member-only deals" } },
        { LoyaltyTier.Silver, new List<string> { "Earn 1.25 points per $1 spent", "Free shipping on orders over $50", "Early access to sales" } },
        { LoyaltyTier.Gold, new List<string> { "Earn 1.5 points per $1 spent", "Free shipping on all orders", "Birthday bonus points", "Priority customer support" } },
        { LoyaltyTier.Platinum, new List<string> { "Earn 2 points per $1 spent", "Free express shipping", "Exclusive platinum rewards", "Personal shopping assistant", "Double points events" } }
    };

    public LoyaltyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LoyaltyAccountDto> GetAccountAsync(int userId)
    {
        var account = await GetOrCreateAccountAsync(userId);
        return MapToAccountDto(account);
    }

    public async Task<LoyaltySummaryDto> GetSummaryAsync(int userId)
    {
        var account = await GetOrCreateAccountAsync(userId);

        var recentTransactions = await _context.Set<LoyaltyTransaction>()
            .Where(t => t.LoyaltyAccountId == account.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new LoyaltyTransactionDto
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Points = t.Points,
                Description = t.Description,
                OrderId = t.OrderId,
                OrderNumber = t.Order != null ? t.Order.OrderNumber : null,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        var activeRewards = await _context.Set<RedeemedReward>()
            .Include(r => r.LoyaltyReward)
            .Where(r => r.LoyaltyAccountId == account.Id && !r.IsUsed && r.ExpiresAt > DateTime.UtcNow)
            .Select(r => new RedeemedRewardDto
            {
                Id = r.Id,
                RewardName = r.LoyaltyReward.Name,
                RedemptionCode = r.RedemptionCode,
                PointsSpent = r.PointsSpent,
                IsUsed = r.IsUsed,
                RedeemedAt = r.RedeemedAt,
                ExpiresAt = r.ExpiresAt
            })
            .ToListAsync();

        var (pointsToNextTier, nextTier) = CalculateNextTier(account.LifetimePoints, account.Tier);

        return new LoyaltySummaryDto
        {
            CurrentPoints = account.CurrentPoints,
            LifetimePoints = account.LifetimePoints,
            Tier = account.Tier.ToString(),
            PointsToNextTier = pointsToNextTier,
            NextTier = nextTier,
            EarningMultiplier = TierMultipliers[account.Tier],
            TierBenefits = TierBenefits[account.Tier],
            RecentTransactions = recentTransactions,
            ActiveRewards = activeRewards
        };
    }

    public async Task<bool> AdjustPointsAsync(int userId, int points, string reason)
    {
        var account = await GetOrCreateAccountAsync(userId);

        account.CurrentPoints = Math.Max(0, account.CurrentPoints + points);
        if (points > 0)
        {
            account.LifetimePoints += points;
        }
        account.UpdatedAt = DateTime.UtcNow;

        UpdateTier(account);

        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            Type = points > 0 ? LoyaltyTransactionType.Bonus : LoyaltyTransactionType.Adjustment,
            Points = points,
            Description = reason,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<LoyaltyTransaction>().Add(transaction);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<LoyaltyTransactionDto>> GetTransactionsAsync(int userId, int page = 1, int pageSize = 20)
    {
        var account = await _context.Set<LoyaltyAccount>()
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (account == null) return new List<LoyaltyTransactionDto>();

        return await _context.Set<LoyaltyTransaction>()
            .Include(t => t.Order)
            .Where(t => t.LoyaltyAccountId == account.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new LoyaltyTransactionDto
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Points = t.Points,
                Description = t.Description,
                OrderId = t.OrderId,
                OrderNumber = t.Order != null ? t.Order.OrderNumber : null,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<LoyaltyRewardDto>> GetAvailableRewardsAsync(int userId)
    {
        var account = await GetOrCreateAccountAsync(userId);

        return await _context.Set<LoyaltyReward>()
            .Where(r => r.IsActive)
            .Select(r => new LoyaltyRewardDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                PointsCost = r.PointsCost,
                Type = r.Type.ToString(),
                DiscountPercentage = r.DiscountPercentage,
                DiscountAmount = r.DiscountAmount,
                IsFreeShipping = r.IsFreeShipping,
                MinimumTier = r.MinimumTier.HasValue ? r.MinimumTier.Value.ToString() : null,
                CanRedeem = account.CurrentPoints >= r.PointsCost &&
                           (!r.MinimumTier.HasValue || account.Tier >= r.MinimumTier.Value)
            })
            .ToListAsync();
    }

    public async Task<RedeemRewardResponse> RedeemRewardAsync(int userId, int rewardId)
    {
        var account = await GetOrCreateAccountAsync(userId);
        var reward = await _context.Set<LoyaltyReward>().FindAsync(rewardId);

        if (reward == null || !reward.IsActive)
        {
            return new RedeemRewardResponse
            {
                Success = false,
                Message = "Reward not found or is no longer available"
            };
        }

        if (account.CurrentPoints < reward.PointsCost)
        {
            return new RedeemRewardResponse
            {
                Success = false,
                Message = $"Insufficient points. You need {reward.PointsCost} points but have {account.CurrentPoints}"
            };
        }

        if (reward.MinimumTier.HasValue && account.Tier < reward.MinimumTier.Value)
        {
            return new RedeemRewardResponse
            {
                Success = false,
                Message = $"This reward requires {reward.MinimumTier.Value} tier or higher"
            };
        }

        // Deduct points
        account.CurrentPoints -= reward.PointsCost;
        account.UpdatedAt = DateTime.UtcNow;

        // Generate redemption code
        var redemptionCode = GenerateRedemptionCode();
        var expiresAt = DateTime.UtcNow.AddDays(30);

        // Create redeemed reward
        var redeemedReward = new RedeemedReward
        {
            LoyaltyAccountId = account.Id,
            LoyaltyRewardId = rewardId,
            RedemptionCode = redemptionCode,
            PointsSpent = reward.PointsCost,
            IsUsed = false,
            RedeemedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };

        _context.Set<RedeemedReward>().Add(redeemedReward);

        // Record transaction
        var transaction = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.Id,
            Type = LoyaltyTransactionType.Redeemed,
            Points = -reward.PointsCost,
            Description = $"Redeemed: {reward.Name}",
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<LoyaltyTransaction>().Add(transaction);
        await _context.SaveChangesAsync();

        return new RedeemRewardResponse
        {
            Success = true,
            Message = "Reward redeemed successfully!",
            RedemptionCode = redemptionCode,
            PointsSpent = reward.PointsCost,
            RemainingPoints = account.CurrentPoints,
            ExpiresAt = expiresAt
        };
    }

    public async Task<List<RedeemedRewardDto>> GetRedeemedRewardsAsync(int userId)
    {
        var account = await _context.Set<LoyaltyAccount>()
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (account == null) return new List<RedeemedRewardDto>();

        return await _context.Set<RedeemedReward>()
            .Include(r => r.LoyaltyReward)
            .Where(r => r.LoyaltyAccountId == account.Id)
            .OrderByDescending(r => r.RedeemedAt)
            .Select(r => new RedeemedRewardDto
            {
                Id = r.Id,
                RewardName = r.LoyaltyReward.Name,
                RedemptionCode = r.RedemptionCode,
                PointsSpent = r.PointsSpent,
                IsUsed = r.IsUsed,
                RedeemedAt = r.RedeemedAt,
                ExpiresAt = r.ExpiresAt
            })
            .ToListAsync();
    }

    public async Task<RedeemedRewardDto?> ValidateRedemptionCodeAsync(string code)
    {
        var redeemedReward = await _context.Set<RedeemedReward>()
            .Include(r => r.LoyaltyReward)
            .FirstOrDefaultAsync(r => r.RedemptionCode == code && !r.IsUsed && r.ExpiresAt > DateTime.UtcNow);

        if (redeemedReward == null) return null;

        return new RedeemedRewardDto
        {
            Id = redeemedReward.Id,
            RewardName = redeemedReward.LoyaltyReward.Name,
            RedemptionCode = redeemedReward.RedemptionCode,
            PointsSpent = redeemedReward.PointsSpent,
            IsUsed = redeemedReward.IsUsed,
            RedeemedAt = redeemedReward.RedeemedAt,
            ExpiresAt = redeemedReward.ExpiresAt
        };
    }

    public async Task<LoyaltyRewardDto> CreateRewardAsync(CreateRewardRequest request)
    {
        var reward = new LoyaltyReward
        {
            Name = request.Name,
            Description = request.Description,
            PointsCost = request.PointsCost,
            Type = request.Type,
            DiscountPercentage = request.DiscountPercentage,
            DiscountAmount = request.DiscountAmount,
            IsFreeShipping = request.IsFreeShipping,
            MinimumTier = request.MinimumTier,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<LoyaltyReward>().Add(reward);
        await _context.SaveChangesAsync();

        return MapToRewardDto(reward);
    }

    public async Task<LoyaltyRewardDto> UpdateRewardAsync(int rewardId, CreateRewardRequest request)
    {
        var reward = await _context.Set<LoyaltyReward>().FindAsync(rewardId);
        if (reward == null)
        {
            throw new Exception("Reward not found");
        }

        reward.Name = request.Name;
        reward.Description = request.Description;
        reward.PointsCost = request.PointsCost;
        reward.Type = request.Type;
        reward.DiscountPercentage = request.DiscountPercentage;
        reward.DiscountAmount = request.DiscountAmount;
        reward.IsFreeShipping = request.IsFreeShipping;
        reward.MinimumTier = request.MinimumTier;

        await _context.SaveChangesAsync();

        return MapToRewardDto(reward);
    }

    public async Task<bool> DeleteRewardAsync(int rewardId)
    {
        var reward = await _context.Set<LoyaltyReward>().FindAsync(rewardId);
        if (reward == null) return false;

        reward.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<LoyaltyRewardDto>> GetAllRewardsAsync()
    {
        return await _context.Set<LoyaltyReward>()
            .Select(r => MapToRewardDto(r))
            .ToListAsync();
    }

    #region Private Helpers

    private async Task<LoyaltyAccount> GetOrCreateAccountAsync(int userId)
    {
        var account = await _context.Set<LoyaltyAccount>()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (account == null)
        {
            account = new LoyaltyAccount
            {
                UserId = userId,
                CurrentPoints = 0,
                LifetimePoints = 0,
                Tier = LoyaltyTier.Bronze,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<LoyaltyAccount>().Add(account);
            await _context.SaveChangesAsync();

            // Reload with user
            account = await _context.Set<LoyaltyAccount>()
                .Include(a => a.User)
                .FirstAsync(a => a.Id == account.Id);
        }

        return account;
    }

    private void UpdateTier(LoyaltyAccount account)
    {
        var newTier = LoyaltyTier.Bronze;

        foreach (var tier in TierThresholds.OrderByDescending(t => t.Value))
        {
            if (account.LifetimePoints >= tier.Value)
            {
                newTier = tier.Key;
                break;
            }
        }

        account.Tier = newTier;
    }

    private (int pointsNeeded, string nextTier) CalculateNextTier(int lifetimePoints, LoyaltyTier currentTier)
    {
        var nextTierValue = (int)currentTier + 1;
        if (nextTierValue > (int)LoyaltyTier.Platinum)
        {
            return (0, "Max tier reached");
        }

        var nextTier = (LoyaltyTier)nextTierValue;
        var pointsNeeded = TierThresholds[nextTier] - lifetimePoints;

        return (Math.Max(0, pointsNeeded), nextTier.ToString());
    }

    private LoyaltyAccountDto MapToAccountDto(LoyaltyAccount account)
    {
        var (pointsToNextTier, _) = CalculateNextTier(account.LifetimePoints, account.Tier);

        return new LoyaltyAccountDto
        {
            Id = account.Id,
            UserId = account.UserId,
            UserName = account.User != null ? $"{account.User.FirstName} {account.User.LastName}" : "",
            CurrentPoints = account.CurrentPoints,
            LifetimePoints = account.LifetimePoints,
            Tier = account.Tier.ToString(),
            TierBenefits = string.Join(", ", TierBenefits[account.Tier]),
            PointsToNextTier = pointsToNextTier,
            EarningMultiplier = TierMultipliers[account.Tier],
            CreatedAt = account.CreatedAt
        };
    }

    private static LoyaltyRewardDto MapToRewardDto(LoyaltyReward reward)
    {
        return new LoyaltyRewardDto
        {
            Id = reward.Id,
            Name = reward.Name,
            Description = reward.Description,
            PointsCost = reward.PointsCost,
            Type = reward.Type.ToString(),
            DiscountPercentage = reward.DiscountPercentage,
            DiscountAmount = reward.DiscountAmount,
            IsFreeShipping = reward.IsFreeShipping,
            MinimumTier = reward.MinimumTier?.ToString()
        };
    }

    private string GenerateRedemptionCode()
    {
        return $"REWARD-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }

    #endregion
}
