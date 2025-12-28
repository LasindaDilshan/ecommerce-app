using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public interface ILoyaltyService
{
    // Account Management
    Task<LoyaltyAccountDto> GetAccountAsync(int userId);
    Task<LoyaltySummaryDto> GetSummaryAsync(int userId);

    // Points Management
    Task<bool> AdjustPointsAsync(int userId, int points, string reason);

    // Transactions
    Task<List<LoyaltyTransactionDto>> GetTransactionsAsync(int userId, int page = 1, int pageSize = 20);

    // Rewards
    Task<List<LoyaltyRewardDto>> GetAvailableRewardsAsync(int userId);
    Task<RedeemRewardResponse> RedeemRewardAsync(int userId, int rewardId);
    Task<List<RedeemedRewardDto>> GetRedeemedRewardsAsync(int userId);
    Task<RedeemedRewardDto?> ValidateRedemptionCodeAsync(string code);

    // Admin
    Task<LoyaltyRewardDto> CreateRewardAsync(CreateRewardRequest request);
    Task<LoyaltyRewardDto> UpdateRewardAsync(int rewardId, CreateRewardRequest request);
    Task<bool> DeleteRewardAsync(int rewardId);
    Task<List<LoyaltyRewardDto>> GetAllRewardsAsync();
}
