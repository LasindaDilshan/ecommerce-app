using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(ApplicationDbContext context, ILogger<ReviewService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewRequest request)
        {
            // Check if user has already reviewed this product
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == request.ProductId);

            if (existingReview != null)
            {
                throw new InvalidOperationException("You have already reviewed this product");
            }

            // Verify the product exists
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new ArgumentException("Product not found");
            }

            // Check if this is a verified purchase
            bool isVerifiedPurchase = await VerifyPurchaseAsync(userId, request.ProductId, request.OrderId);

            var review = new Review
            {
                ProductId = request.ProductId,
                UserId = userId,
                OrderId = request.OrderId,
                Rating = request.Rating,
                Title = request.Title,
                Comment = request.Comment,
                IsVerifiedPurchase = isVerifiedPurchase,
                IsApproved = true, // Auto-approve, can be changed to require moderation
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update product rating
            await UpdateProductRatingAsync(request.ProductId);

            _logger.LogInformation($"Review created for product {request.ProductId} by user {userId}");

            return await GetReviewByIdAsync(review.ReviewId);
        }

        public async Task<ReviewDto> GetReviewByIdAsync(int reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            if (review == null)
            {
                throw new ArgumentException("Review not found");
            }

            return MapToReviewDto(review);
        }

        public async Task<ReviewDto> UpdateReviewAsync(int reviewId, int userId, UpdateReviewRequest request)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == userId);

            if (review == null)
            {
                throw new ArgumentException("Review not found or you don't have permission to edit it");
            }

            review.Rating = request.Rating;
            review.Title = request.Title;
            review.Comment = request.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update product rating
            await UpdateProductRatingAsync(review.ProductId);

            _logger.LogInformation($"Review {reviewId} updated by user {userId}");

            return await GetReviewByIdAsync(reviewId);
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin = false)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
            {
                return false;
            }

            if (!isAdmin && review.UserId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this review");
            }

            var productId = review.ProductId;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // Update product rating
            await UpdateProductRatingAsync(productId);

            _logger.LogInformation($"Review {reviewId} deleted");

            return true;
        }

        public async Task<(IEnumerable<ReviewSummaryDto> Reviews, int TotalCount)> GetProductReviewsAsync(
            int productId, ReviewFilterRequest filter)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId && r.IsApproved);

            // Apply filters
            if (filter.Rating.HasValue)
            {
                query = query.Where(r => r.Rating == filter.Rating.Value);
            }

            if (filter.VerifiedPurchasesOnly == true)
            {
                query = query.Where(r => r.IsVerifiedPurchase);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "mosthelpful" => query.OrderByDescending(r => r.HelpfulVotes)
                                     .ThenByDescending(r => r.CreatedAt),
                "highestrating" => query.OrderByDescending(r => r.Rating)
                                       .ThenByDescending(r => r.CreatedAt),
                "lowestrating" => query.OrderBy(r => r.Rating)
                                      .ThenByDescending(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt) // Most recent by default
            };

            // Apply pagination
            var reviews = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new ReviewSummaryDto
                {
                    ReviewId = r.ReviewId,
                    UserName = $"{r.User.FirstName} {r.User.LastName}",
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    IsVerifiedPurchase = r.IsVerifiedPurchase,
                    HelpfulVotes = r.HelpfulVotes,
                    TotalVotes = r.HelpfulVotes + r.UnhelpfulVotes,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return (reviews, totalCount);
        }

        public async Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(int userId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(MapToReviewDto);
        }

        public async Task<bool> HasUserReviewedProductAsync(int userId, int productId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        }

        public async Task<bool> VoteReviewAsync(int userId, ReviewVoteRequest request)
        {
            var review = await _context.Reviews.FindAsync(request.ReviewId);
            if (review == null)
            {
                throw new ArgumentException("Review not found");
            }

            // Check if user is trying to vote on their own review
            if (review.UserId == userId)
            {
                throw new InvalidOperationException("You cannot vote on your own review");
            }

            // Check if user has already voted
            var existingVote = await _context.ReviewVotes
                .FirstOrDefaultAsync(v => v.ReviewId == request.ReviewId && v.UserId == userId);

            if (existingVote != null)
            {
                // Update existing vote if different
                if (existingVote.IsHelpful != request.IsHelpful)
                {
                    // Update vote counts
                    if (request.IsHelpful)
                    {
                        review.HelpfulVotes++;
                        review.UnhelpfulVotes--;
                    }
                    else
                    {
                        review.HelpfulVotes--;
                        review.UnhelpfulVotes++;
                    }

                    existingVote.IsHelpful = request.IsHelpful;
                    existingVote.VotedAt = DateTime.UtcNow;
                }
            }
            else
            {
                // Create new vote
                var vote = new ReviewVote
                {
                    ReviewId = request.ReviewId,
                    UserId = userId,
                    IsHelpful = request.IsHelpful,
                    VotedAt = DateTime.UtcNow
                };

                _context.ReviewVotes.Add(vote);

                // Update vote counts
                if (request.IsHelpful)
                {
                    review.HelpfulVotes++;
                }
                else
                {
                    review.UnhelpfulVotes++;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveVoteAsync(int userId, int reviewId)
        {
            var vote = await _context.ReviewVotes
                .FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == userId);

            if (vote == null)
            {
                return false;
            }

            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                if (vote.IsHelpful)
                {
                    review.HelpfulVotes--;
                }
                else
                {
                    review.UnhelpfulVotes--;
                }
            }

            _context.ReviewVotes.Remove(vote);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ProductRatingDto> GetProductRatingAsync(int productId)
        {
            var rating = await _context.ProductRatings
                .FirstOrDefaultAsync(pr => pr.ProductId == productId);

            if (rating == null)
            {
                // Calculate rating if not cached
                await UpdateProductRatingAsync(productId);
                rating = await _context.ProductRatings.FirstOrDefaultAsync(pr => pr.ProductId == productId);
            }

            if (rating == null)
            {
                return new ProductRatingDto
                {
                    ProductId = productId,
                    AverageRating = 0,
                    TotalReviews = 0,
                    Distribution = new RatingDistribution()
                };
            }

            return new ProductRatingDto
            {
                ProductId = rating.ProductId,
                AverageRating = Math.Round(rating.AverageRating, 1),
                TotalReviews = rating.TotalReviews,
                Distribution = new RatingDistribution
                {
                    FiveStars = rating.FiveStarCount,
                    FourStars = rating.FourStarCount,
                    ThreeStars = rating.ThreeStarCount,
                    TwoStars = rating.TwoStarCount,
                    OneStar = rating.OneStarCount
                }
            };
        }

        public async Task UpdateProductRatingAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .ToListAsync();

            if (!reviews.Any())
            {
                // Remove rating if no reviews
                var existingRating = await _context.ProductRatings.FindAsync(productId);
                if (existingRating != null)
                {
                    _context.ProductRatings.Remove(existingRating);
                    await _context.SaveChangesAsync();
                }
                return;
            }

            var rating = await _context.ProductRatings.FindAsync(productId);
            if (rating == null)
            {
                rating = new ProductRating { ProductId = productId };
                _context.ProductRatings.Add(rating);
            }

            rating.TotalReviews = reviews.Count;
            rating.AverageRating = reviews.Average(r => r.Rating);
            rating.FiveStarCount = reviews.Count(r => r.Rating == 5);
            rating.FourStarCount = reviews.Count(r => r.Rating == 4);
            rating.ThreeStarCount = reviews.Count(r => r.Rating == 3);
            rating.TwoStarCount = reviews.Count(r => r.Rating == 2);
            rating.OneStarCount = reviews.Count(r => r.Rating == 1);
            rating.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<ReviewDto> ModerateReviewAsync(int reviewId, ReviewModerationRequest request)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                throw new ArgumentException("Review not found");
            }

            bool ratingNeedsUpdate = review.IsApproved != request.IsApproved;

            review.IsApproved = request.IsApproved;
            review.IsFeatured = request.IsFeatured;

            await _context.SaveChangesAsync();

            if (ratingNeedsUpdate)
            {
                await UpdateProductRatingAsync(review.ProductId);
            }

            _logger.LogInformation($"Review {reviewId} moderated - Approved: {request.IsApproved}, Featured: {request.IsFeatured}");

            return await GetReviewByIdAsync(reviewId);
        }

        public async Task<(IEnumerable<ReviewDto> Reviews, int TotalCount)> GetAllReviewsAsync(
            bool? approved, int pageNumber, int pageSize)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .AsQueryable();

            if (approved.HasValue)
            {
                query = query.Where(r => r.IsApproved == approved.Value);
            }

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews.Select(MapToReviewDto), totalCount);
        }

        public async Task<IEnumerable<ReviewDto>> GetPendingReviewsAsync()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => !r.IsApproved)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(MapToReviewDto);
        }

        public async Task<bool> VerifyPurchaseAsync(int userId, int productId, int? orderId = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId &&
                           o.OrderItems.Any(oi => oi.ProductId == productId) &&
                           o.Status != OrderStatus.Cancelled);

            if (orderId.HasValue)
            {
                query = query.Where(o => o.Id == orderId.Value);
            }

            return await query.AnyAsync();
        }

        private ReviewDto MapToReviewDto(Review review)
        {
            return new ReviewDto
            {
                ReviewId = review.ReviewId,
                ProductId = review.ProductId,
                ProductName = review.Product?.Name ?? "",
                UserId = review.UserId,
                UserName = review.User != null ? $"{review.User.FirstName} {review.User.LastName}" : "",
                Rating = review.Rating,
                Title = review.Title,
                Comment = review.Comment,
                IsVerifiedPurchase = review.IsVerifiedPurchase,
                IsApproved = review.IsApproved,
                IsFeatured = review.IsFeatured,
                HelpfulVotes = review.HelpfulVotes,
                UnhelpfulVotes = review.UnhelpfulVotes,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }
    }
}