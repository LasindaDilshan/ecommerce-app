using EcommerceAPI.Models;

namespace Backend.Models;

public class Wishlist
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
