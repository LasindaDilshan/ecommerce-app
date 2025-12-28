namespace EcommerceAPI.DTOs;

public class CartDto
{
    public int CartId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal FinalTotal { get; set; } // SubTotal - DiscountAmount
    public int TotalItems { get; set; }
}

public class CartItemDto
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public int AvailableStock { get; set; }
}

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}
