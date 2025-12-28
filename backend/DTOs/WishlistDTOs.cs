using Backend.DTOs;

namespace Backend.DTOs;

public class WishlistDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<WishlistItemDto> Items { get; set; } = new();
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class WishlistItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public decimal? ProductDiscountPrice { get; set; }
    public string? ProductImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsInStock { get; set; }
    public DateTime AddedAt { get; set; }
}

public class AddToWishlistRequest
{
    public int ProductId { get; set; }
}

public class MoveToCartRequest
{
    public int WishlistItemId { get; set; }
    public int Quantity { get; set; } = 1;
}
