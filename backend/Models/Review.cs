using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public int? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public bool IsVerifiedPurchase { get; set; } = false;

        public bool IsApproved { get; set; } = true; // Auto-approve by default, admin can moderate

        public bool IsFeatured { get; set; } = false; // Admin can feature helpful reviews

        public int HelpfulVotes { get; set; } = 0;

        public int UnhelpfulVotes { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property for review votes
        public ICollection<ReviewVote> ReviewVotes { get; set; } = new List<ReviewVote>();

        // Calculated property
        [NotMapped]
        public int TotalVotes => HelpfulVotes + UnhelpfulVotes;

        [NotMapped]
        public double HelpfulnessScore => TotalVotes > 0 ? (double)HelpfulVotes / TotalVotes : 0;
    }

    public class ReviewVote
    {
        [Key]
        public int VoteId { get; set; }

        [Required]
        public int ReviewId { get; set; }

        [ForeignKey("ReviewId")]
        public Review Review { get; set; } = null!;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public bool IsHelpful { get; set; }

        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProductRating
    {
        [Key]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public double AverageRating { get; set; } = 0;

        public int TotalReviews { get; set; } = 0;

        public int FiveStarCount { get; set; } = 0;

        public int FourStarCount { get; set; } = 0;

        public int ThreeStarCount { get; set; } = 0;

        public int TwoStarCount { get; set; } = 0;

        public int OneStarCount { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}