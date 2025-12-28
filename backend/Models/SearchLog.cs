using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models;

public class SearchLog
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; } // Null for guest users

    [Required]
    [MaxLength(500)]
    public string SearchTerm { get; set; } = string.Empty;

    public int ResultsCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? Category { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    [MaxLength(50)]
    public string? SortBy { get; set; }

    [MaxLength(20)]
    public string? SortOrder { get; set; }

    public bool HasFilters { get; set; } = false;

    [MaxLength(100)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    // For autocomplete and search suggestions
    public int ClickedResultPosition { get; set; } = -1; // -1 means no click

    public int? ClickedProductId { get; set; }
}
