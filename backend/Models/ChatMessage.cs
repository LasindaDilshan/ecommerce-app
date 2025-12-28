using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }

    public int ChatSessionId { get; set; }

    public int SenderId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    [MaxLength(50)]
    public string? MessageType { get; set; } = "Text"; // Text, Image, File, System

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    // Navigation properties
    [ForeignKey("ChatSessionId")]
    public ChatSession ChatSession { get; set; } = null!;

    [ForeignKey("SenderId")]
    public User Sender { get; set; } = null!;
}
