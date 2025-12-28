using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models;

public class ChatSession
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ChatRoomId { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public int? SupportAgentId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Waiting"; // Waiting, Active, Closed

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    [MaxLength(500)]
    public string? Subject { get; set; }

    public int? Rating { get; set; } // 1-5 stars

    [MaxLength(1000)]
    public string? FeedbackComment { get; set; }

    // Navigation properties
    [ForeignKey("CustomerId")]
    public User Customer { get; set; } = null!;

    [ForeignKey("SupportAgentId")]
    public User? SupportAgent { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
