using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EcommerceAPI.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("userId")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal notification group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            // Check if user is admin and add to admin group
            var userRole = Context.User?.FindFirst("role")?.Value;
            if (userRole == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("userId")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");

            var userRole = Context.User?.FindFirst("role")?.Value;
            if (userRole == "Admin")
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Client can request to join specific notification channels
    public async Task JoinNotificationChannel(string channel)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"channel-{channel}");
    }

    public async Task LeaveNotificationChannel(string channel)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"channel-{channel}");
    }

    // Mark notification as read
    public async Task MarkNotificationAsRead(int notificationId)
    {
        // This would be handled by a service that updates the database
        // Then broadcast the update to the user
        await Clients.Caller.SendAsync("NotificationRead", notificationId);
    }

    // Mark all notifications as read
    public async Task MarkAllNotificationsAsRead()
    {
        var userId = Context.User?.FindFirst("userId")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("AllNotificationsRead");
        }
    }
}
