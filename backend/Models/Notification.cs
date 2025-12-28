using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models;

public class Notification
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // OrderUpdate, LowStock, System, Promotion

    public bool IsRead { get; set; } = false;

    [MaxLength(500)]
    public string? Link { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    // Navigation property
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
