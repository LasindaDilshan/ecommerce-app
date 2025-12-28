namespace EcommerceAPI.DTOs;

public class DashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockProducts { get; set; }
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<RevenueByMonthDto> RevenueByMonth { get; set; } = new();
}

public class RecentOrderDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public decimal Revenue { get; set; }
}

public class RevenueByMonthDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}
