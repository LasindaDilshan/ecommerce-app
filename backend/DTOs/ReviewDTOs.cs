using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs
{
    public class CreateReviewRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 2000 characters")]
        public string Comment { get; set; } = string.Empty;

        public int? OrderId { get; set; } // Optional - to link review to a specific order
    }

    public class UpdateReviewRequest
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 2000 characters")]
        public string Comment { get; set; } = string.Empty;
    }

    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        public bool IsFeatured { get; set; }
        public int HelpfulVotes { get; set; }
        public int UnhelpfulVotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? CurrentUserVote { get; set; } // null if not voted, true if helpful, false if unhelpful
    }

    public class ReviewSummaryDto
    {
        public int ReviewId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public int HelpfulVotes { get; set; }
        public int TotalVotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductRatingDto
    {
        public int ProductId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public RatingDistribution Distribution { get; set; } = new();
    }

    public class RatingDistribution
    {
        public int FiveStars { get; set; }
        public int FourStars { get; set; }
        public int ThreeStars { get; set; }
        public int TwoStars { get; set; }
        public int OneStar { get; set; }

        public double FiveStarsPercentage => TotalCount > 0 ? (double)FiveStars / TotalCount * 100 : 0;
        public double FourStarsPercentage => TotalCount > 0 ? (double)FourStars / TotalCount * 100 : 0;
        public double ThreeStarsPercentage => TotalCount > 0 ? (double)ThreeStars / TotalCount * 100 : 0;
        public double TwoStarsPercentage => TotalCount > 0 ? (double)TwoStars / TotalCount * 100 : 0;
        public double OneStarPercentage => TotalCount > 0 ? (double)OneStar / TotalCount * 100 : 0;

        private int TotalCount => FiveStars + FourStars + ThreeStars + TwoStars + OneStar;
    }

    public class ReviewVoteRequest
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        public bool IsHelpful { get; set; }
    }

    public class ReviewModerationRequest
    {
        [Required]
        public bool IsApproved { get; set; }

        public bool IsFeatured { get; set; }
    }

    public class ReviewFilterRequest
    {
        public int? Rating { get; set; }
        public bool? VerifiedPurchasesOnly { get; set; }
        public string? SortBy { get; set; } = "MostRecent"; // MostRecent, MostHelpful, HighestRating, LowestRating
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}