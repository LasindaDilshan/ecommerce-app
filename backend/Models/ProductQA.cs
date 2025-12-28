using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models;

public class ProductQuestion
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    public bool IsApproved { get; set; } = false;

    public bool IsAnswered { get; set; } = false;

    public int UpvoteCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProductAnswer> Answers { get; set; } = new List<ProductAnswer>();

    public ICollection<QuestionVote> Votes { get; set; } = new List<QuestionVote>();
}

public class ProductAnswer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int QuestionId { get; set; }

    [ForeignKey("QuestionId")]
    public ProductQuestion Question { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Required]
    [MaxLength(2000)]
    public string AnswerText { get; set; } = string.Empty;

    public bool IsApproved { get; set; } = false;

    public bool IsVerifiedPurchase { get; set; } = false;

    public bool IsSellerAnswer { get; set; } = false;

    public bool IsAccepted { get; set; } = false;

    public int HelpfulCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<AnswerVote> Votes { get; set; } = new List<AnswerVote>();
}

public class QuestionVote
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int QuestionId { get; set; }

    [ForeignKey("QuestionId")]
    public ProductQuestion Question { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AnswerVote
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AnswerId { get; set; }

    [ForeignKey("AnswerId")]
    public ProductAnswer Answer { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    public bool IsHelpful { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
