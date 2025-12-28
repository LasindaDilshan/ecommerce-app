using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user identifier");
            }
            return userId;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // Create a new review
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            try
            {
                var review = await _reviewService.CreateReviewAsync(GetUserId(), request);
                return CreatedAtAction(nameof(GetReview), new { id = review.ReviewId }, review);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, new { message = "An error occurred while creating the review" });
            }
        }

        // Get a specific review
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(int id)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                return Ok(review);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting review {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the review" });
            }
        }

        // Update a review
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewRequest request)
        {
            try
            {
                var review = await _reviewService.UpdateReviewAsync(id, GetUserId(), request);
                return Ok(review);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating review {id}");
                return StatusCode(500, new { message = "An error occurred while updating the review" });
            }
        }

        // Delete a review
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var result = await _reviewService.DeleteReviewAsync(id, GetUserId(), IsAdmin());
                if (!result)
                {
                    return NotFound(new { message = "Review not found" });
                }
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting review {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the review" });
            }
        }

        // Get reviews for a product
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId, [FromQuery] ReviewFilterRequest filter)
        {
            try
            {
                var (reviews, totalCount) = await _reviewService.GetProductReviewsAsync(productId, filter);
                return Ok(new
                {
                    reviews,
                    totalCount,
                    pageNumber = filter.PageNumber,
                    pageSize = filter.PageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting reviews for product {productId}");
                return StatusCode(500, new { message = "An error occurred while retrieving reviews" });
            }
        }

        // Get product rating summary
        [HttpGet("product/{productId}/rating")]
        public async Task<IActionResult> GetProductRating(int productId)
        {
            try
            {
                var rating = await _reviewService.GetProductRatingAsync(productId);
                return Ok(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting rating for product {productId}");
                return StatusCode(500, new { message = "An error occurred while retrieving the rating" });
            }
        }

        // Get reviews by current user
        [Authorize]
        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews()
        {
            try
            {
                var reviews = await _reviewService.GetUserReviewsAsync(GetUserId());
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user reviews");
                return StatusCode(500, new { message = "An error occurred while retrieving your reviews" });
            }
        }

        // Check if user has reviewed a product
        [Authorize]
        [HttpGet("product/{productId}/has-reviewed")]
        public async Task<IActionResult> HasUserReviewedProduct(int productId)
        {
            try
            {
                var hasReviewed = await _reviewService.HasUserReviewedProductAsync(GetUserId(), productId);
                return Ok(new { hasReviewed });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user reviewed product {productId}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // Vote on a review (helpful/unhelpful)
        [Authorize]
        [HttpPost("vote")]
        public async Task<IActionResult> VoteReview([FromBody] ReviewVoteRequest request)
        {
            try
            {
                await _reviewService.VoteReviewAsync(GetUserId(), request);
                return Ok(new { message = "Vote recorded successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting on review");
                return StatusCode(500, new { message = "An error occurred while recording your vote" });
            }
        }

        // Remove vote on a review
        [Authorize]
        [HttpDelete("vote/{reviewId}")]
        public async Task<IActionResult> RemoveVote(int reviewId)
        {
            try
            {
                var result = await _reviewService.RemoveVoteAsync(GetUserId(), reviewId);
                if (!result)
                {
                    return NotFound(new { message = "Vote not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing vote on review {reviewId}");
                return StatusCode(500, new { message = "An error occurred while removing your vote" });
            }
        }

        // Admin: Get all reviews with optional filter
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllReviews([FromQuery] bool? approved, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (reviews, totalCount) = await _reviewService.GetAllReviewsAsync(approved, pageNumber, pageSize);
                return Ok(new
                {
                    reviews,
                    totalCount,
                    pageNumber,
                    pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reviews");
                return StatusCode(500, new { message = "An error occurred while retrieving reviews" });
            }
        }

        // Admin: Get pending reviews
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("admin/pending")]
        public async Task<IActionResult> GetPendingReviews()
        {
            try
            {
                var reviews = await _reviewService.GetPendingReviewsAsync();
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending reviews");
                return StatusCode(500, new { message = "An error occurred while retrieving pending reviews" });
            }
        }

        // Admin: Moderate a review (approve/reject, feature)
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}/moderate")]
        public async Task<IActionResult> ModerateReview(int id, [FromBody] ReviewModerationRequest request)
        {
            try
            {
                var review = await _reviewService.ModerateReviewAsync(id, request);
                return Ok(review);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error moderating review {id}");
                return StatusCode(500, new { message = "An error occurred while moderating the review" });
            }
        }
    }
}