using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace EcommerceAPI.Hubs;

public class ActivityHub : Hub
{
    private static readonly ConcurrentDictionary<string, int> ActiveUsers = new();
    private static readonly ConcurrentDictionary<int, HashSet<string>> ProductViewers = new();
    private static readonly object _lock = new();

    public override async Task OnConnectedAsync()
    {
        // Track active user
        ActiveUsers.TryAdd(Context.ConnectionId, 0);

        // Broadcast current active users count
        await Clients.All.SendAsync("ActiveUsersUpdate", ActiveUsers.Count);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove from active users
        ActiveUsers.TryRemove(Context.ConnectionId, out _);

        // Remove from all product viewers
        lock (_lock)
        {
            foreach (var productId in ProductViewers.Keys.ToList())
            {
                if (ProductViewers[productId].Remove(Context.ConnectionId))
                {
                    // Notify about viewer count update
                    Clients.Group($"product-{productId}").SendAsync("ProductViewersUpdate", new
                    {
                        ProductId = productId,
                        ViewersCount = ProductViewers[productId].Count
                    });

                    // Clean up if no viewers
                    if (ProductViewers[productId].Count == 0)
                    {
                        ProductViewers.TryRemove(productId, out _);
                    }
                }
            }
        }

        // Broadcast active users count
        await Clients.All.SendAsync("ActiveUsersUpdate", ActiveUsers.Count);

        await base.OnDisconnectedAsync(exception);
    }

    // Track product view
    public async Task ViewProduct(int productId)
    {
        // Add to product viewers group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"product-{productId}");

        lock (_lock)
        {
            if (!ProductViewers.ContainsKey(productId))
            {
                ProductViewers[productId] = new HashSet<string>();
            }
            ProductViewers[productId].Add(Context.ConnectionId);
        }

        // Broadcast viewer count to all viewing this product
        var viewersCount = ProductViewers[productId].Count;
        await Clients.Group($"product-{productId}").SendAsync("ProductViewersUpdate", new
        {
            ProductId = productId,
            ViewersCount = viewersCount,
            Timestamp = DateTime.UtcNow
        });
    }

    // Stop viewing product
    public async Task LeaveProduct(int productId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"product-{productId}");

        lock (_lock)
        {
            if (ProductViewers.ContainsKey(productId))
            {
                ProductViewers[productId].Remove(Context.ConnectionId);

                var viewersCount = ProductViewers[productId].Count;

                // Broadcast updated viewer count
                Clients.Group($"product-{productId}").SendAsync("ProductViewersUpdate", new
                {
                    ProductId = productId,
                    ViewersCount = viewersCount,
                    Timestamp = DateTime.UtcNow
                });

                // Clean up if no viewers
                if (viewersCount == 0)
                {
                    ProductViewers.TryRemove(productId, out _);
                }
            }
        }
    }

    // Broadcast when someone makes a purchase
    public async Task NotifyPurchase(int productId, string productName, string location = "Unknown")
    {
        await Clients.All.SendAsync("RecentPurchase", new
        {
            ProductId = productId,
            ProductName = productName,
            Location = location,
            Timestamp = DateTime.UtcNow
        });
    }

    // Broadcast low stock alert
    public async Task NotifyLowStock(int productId, string productName, int stockQuantity)
    {
        await Clients.All.SendAsync("LowStockAlert", new
        {
            ProductId = productId,
            ProductName = productName,
            StockQuantity = stockQuantity,
            Timestamp = DateTime.UtcNow
        });
    }

    // Track cart additions (for analytics)
    public async Task TrackCartAddition(int productId, string productName)
    {
        // Broadcast to admins for real-time analytics
        await Clients.Group("admin").SendAsync("CartAdditionTracked", new
        {
            ProductId = productId,
            ProductName = productName,
            Timestamp = DateTime.UtcNow
        });
    }

    // Get current active users count
    public async Task GetActiveUsersCount()
    {
        await Clients.Caller.SendAsync("ActiveUsersUpdate", ActiveUsers.Count);
    }

    // Get product viewers count
    public async Task GetProductViewersCount(int productId)
    {
        var viewersCount = ProductViewers.ContainsKey(productId)
            ? ProductViewers[productId].Count
            : 0;

        await Clients.Caller.SendAsync("ProductViewersUpdate", new
        {
            ProductId = productId,
            ViewersCount = viewersCount,
            Timestamp = DateTime.UtcNow
        });
    }

    // Admin dashboard - broadcast real-time metrics
    public async Task SubscribeToAdminMetrics()
    {
        var userRole = Context.User?.FindFirst("role")?.Value;

        if (userRole == "Admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin");

            // Send current metrics
            await Clients.Caller.SendAsync("AdminMetricsUpdate", new
            {
                ActiveUsers = ActiveUsers.Count,
                ActiveProductViews = ProductViewers.Sum(p => p.Value.Count),
                MostViewedProducts = ProductViewers
                    .OrderByDescending(p => p.Value.Count)
                    .Take(5)
                    .Select(p => new { ProductId = p.Key, Viewers = p.Value.Count }),
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public async Task UnsubscribeFromAdminMetrics()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
    }
}
