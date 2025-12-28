using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var totalRevenue = await _context.Orders
            .Where(o => o.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(o => o.TotalAmount);

        var totalOrders = await _context.Orders.CountAsync();
        var totalCustomers = await _context.Users.CountAsync(u => u.Role == "User");
        var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
        var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
        var lowStockProducts = await _context.Products.CountAsync(p => p.StockQuantity < 10 && p.IsActive);

        var recentOrders = await _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .Select(o => new RecentOrderDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.User != null ? $"{o.User.FirstName} {o.User.LastName}" : "Guest",
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                OrderDate = o.OrderDate
            })
            .ToListAsync();

        var topProducts = await _context.OrderItems
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                TotalSold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(tp => tp.Revenue)
            .Take(5)
            .ToListAsync();

        var revenueByMonth = await _context.Orders
            .Where(o => o.PaymentStatus == PaymentStatus.Paid && o.OrderDate >= DateTime.UtcNow.AddMonths(-6))
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new RevenueByMonthDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:00}",
                Revenue = g.Sum(o => o.TotalAmount),
                Orders = g.Count()
            })
            .OrderBy(r => r.Month)
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalCustomers = totalCustomers,
            TotalProducts = totalProducts,
            PendingOrders = pendingOrders,
            LowStockProducts = lowStockProducts,
            RecentOrders = recentOrders,
            TopProducts = topProducts,
            RevenueByMonth = revenueByMonth
        };
    }
}
