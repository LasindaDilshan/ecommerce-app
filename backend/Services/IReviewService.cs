using System.Collections.Generic;
using System.Threading.Tasks;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services
{
    public interface IReviewService
    {
        // Review CRUD operations
        Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewRequest request);
        Task<ReviewDto> GetReviewByIdAsync(int reviewId);
        Task<ReviewDto> UpdateReviewAsync(int reviewId, int userId, UpdateReviewRequest request);
        Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin = false);

        // Get reviews
        Task<(IEnumerable<ReviewSummaryDto> Reviews, int TotalCount)> GetProductReviewsAsync(int productId, ReviewFilterRequest filter);
        Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(int userId);
        Task<bool> HasUserReviewedProductAsync(int userId, int productId);

        // Voting
        Task<bool> VoteReviewAsync(int userId, ReviewVoteRequest request);
        Task<bool> RemoveVoteAsync(int userId, int reviewId);

        // Product ratings
        Task<ProductRatingDto> GetProductRatingAsync(int productId);
        Task UpdateProductRatingAsync(int productId);

        // Admin operations
        Task<ReviewDto> ModerateReviewAsync(int reviewId, ReviewModerationRequest request);
        Task<(IEnumerable<ReviewDto> Reviews, int TotalCount)> GetAllReviewsAsync(bool? approved, int pageNumber, int pageSize);
        Task<IEnumerable<ReviewDto>> GetPendingReviewsAsync();

        // Verification
        Task<bool> VerifyPurchaseAsync(int userId, int productId, int? orderId = null);
    }
}