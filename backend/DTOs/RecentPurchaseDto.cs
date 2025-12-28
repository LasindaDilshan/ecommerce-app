namespace EcommerceAPI.DTOs;

public class RecentPurchaseDto
{
    public string ProductName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime PurchaseTime { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

public class ProductSocialProofDto
{
    public int ProductId { get; set; }
    public int TotalSold { get; set; }
    public int SoldLast24Hours { get; set; }
    public int CurrentViewers { get; set; }
    public List<RecentPurchaseDto> RecentPurchases { get; set; } = new();
}
