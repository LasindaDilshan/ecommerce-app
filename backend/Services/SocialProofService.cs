using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public class SocialProofService : ISocialProofService
{
    private readonly ApplicationDbContext _context;
    private static readonly string[] Cities = { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose", "Austin", "Jacksonville", "Seattle", "Denver", "Boston", "Portland", "Las Vegas", "Miami", "Atlanta", "San Francisco" };

    public SocialProofService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecentPurchaseDto>> GetRecentPurchasesAsync(int limit = 10)
    {
        var recentOrders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.OrderDate >= DateTime.UtcNow.AddHours(-24))
            .OrderByDescending(o => o.OrderDate)
            .Take(limit)
            .ToListAsync();

        var purchases = new List<RecentPurchaseDto>();

        foreach (var order in recentOrders)
        {
            foreach (var item in order.OrderItems)
            {
                if (item.Product == null) continue;

                purchases.Add(new RecentPurchaseDto
                {
                    ProductName = item.Product.Name,
                    CustomerName = GetAnonymizedName(order.User?.Email ?? order.GuestEmail ?? ""),
                    Location = GetRandomCity(),
                    PurchaseTime = order.OrderDate,
                    TimeAgo = GetTimeAgo(order.OrderDate)
                });
            }
        }

        return purchases.Take(limit).ToList();
    }

    public async Task<ProductSocialProofDto> GetProductSocialProofAsync(int productId)
    {
        var now = DateTime.UtcNow;
        var last24Hours = now.AddHours(-24);

        // Get total sold count
        var totalSold = await _context.OrderItems
            .Where(oi => oi.ProductId == productId)
            .SumAsync(oi => oi.Quantity);

        // Get sold in last 24 hours
        var soldLast24Hours = await _context.OrderItems
            .Where(oi => oi.ProductId == productId && oi.Order.OrderDate >= last24Hours)
            .SumAsync(oi => oi.Quantity);

        // Get recent purchases for this product
        var recentOrders = await _context.OrderItems
            .Include(oi => oi.Order)
            .ThenInclude(o => o.User)
            .Include(oi => oi.Product)
            .Where(oi => oi.ProductId == productId && oi.Order.OrderDate >= last24Hours)
            .OrderByDescending(oi => oi.Order.OrderDate)
            .Take(5)
            .ToListAsync();

        var recentPurchases = recentOrders
            .Where(oi => oi.Product != null)
            .Select(oi => new RecentPurchaseDto
            {
                ProductName = oi.Product.Name,
                CustomerName = GetAnonymizedName(oi.Order.User?.Email ?? oi.Order.GuestEmail ?? ""),
                Location = GetRandomCity(),
                PurchaseTime = oi.Order.OrderDate,
                TimeAgo = GetTimeAgo(oi.Order.OrderDate)
            }).ToList();

        // Generate random current viewers (2-15)
        var currentViewers = new Random().Next(2, 16);

        return new ProductSocialProofDto
        {
            ProductId = productId,
            TotalSold = totalSold,
            SoldLast24Hours = soldLast24Hours,
            CurrentViewers = currentViewers,
            RecentPurchases = recentPurchases
        };
    }

    private string GetAnonymizedName(string email)
    {
        if (string.IsNullOrEmpty(email))
            return "Someone";

        var parts = email.Split('@');
        if (parts.Length == 0)
            return "Someone";

        var username = parts[0];
        if (username.Length <= 2)
            return username[0] + "***";

        return username[0] + new string('*', username.Length - 1);
    }

    private string GetRandomCity()
    {
        var random = new Random();
        return Cities[random.Next(Cities.Length)];
    }

    private string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minutes ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hours ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} days ago";

        return dateTime.ToShortDateString();
    }
}
