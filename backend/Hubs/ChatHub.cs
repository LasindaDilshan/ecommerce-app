using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EcommerceAPI.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private static readonly Dictionary<string, string> UserConnections = new();
    private static readonly Dictionary<string, List<string>> ActiveChats = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("userId")?.Value;
        var userRole = Context.User?.FindFirst("role")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            UserConnections[userId] = Context.ConnectionId;

            // Add admins to support group
            if (userRole == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "support-team");
            }

            // Notify about online status
            await Clients.All.SendAsync("UserOnline", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("userId")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            UserConnections.Remove(userId);

            // Remove from active chats
            if (ActiveChats.ContainsKey(userId))
            {
                var chatPartners = ActiveChats[userId];
                foreach (var partnerId in chatPartners)
                {
                    await Clients.User(partnerId).SendAsync("UserOffline", userId);
                }
                ActiveChats.Remove(userId);
            }

            // Notify about offline status
            await Clients.All.SendAsync("UserOffline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Customer initiates chat with support
    public async Task StartSupportChat(string message)
    {
        var userId = Context.User?.FindFirst("userId")?.Value;
        var userName = Context.User?.FindFirst("email")?.Value ?? "User";

        if (!string.IsNullOrEmpty(userId))
        {
            // Notify support team
            await Clients.Group("support-team").SendAsync("NewSupportChatRequest", new
            {
                UserId = userId,
                UserName = userName,
                Message = message,
                Timestamp = DateTime.UtcNow
            });

            // Confirm to user
            await Clients.Caller.SendAsync("SupportChatInitiated", new
            {
                Message = "Your chat request has been sent to our support team. An agent will be with you shortly.",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    // Admin accepts support chat
    public async Task AcceptSupportChat(string customerId)
    {
        var adminId = Context.User?.FindFirst("userId")?.Value;
        var adminName = Context.User?.FindFirst("email")?.Value ?? "Support Agent";

        if (!string.IsNullOrEmpty(adminId))
        {
            // Create chat room
            var chatRoomId = $"chat-{customerId}-{adminId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);

            if (UserConnections.TryGetValue(customerId, out var customerConnectionId))
            {
                await Groups.AddToGroupAsync(customerConnectionId, chatRoomId);

                // Track active chat
                if (!ActiveChats.ContainsKey(adminId))
                    ActiveChats[adminId] = new List<string>();

                if (!ActiveChats.ContainsKey(customerId))
                    ActiveChats[customerId] = new List<string>();

                ActiveChats[adminId].Add(customerId);
                ActiveChats[customerId].Add(adminId);

                // Notify customer
                await Clients.Client(customerConnectionId).SendAsync("SupportAgentJoined", new
                {
                    AgentId = adminId,
                    AgentName = adminName,
                    ChatRoomId = chatRoomId,
                    Message = $"{adminName} has joined the chat.",
                    Timestamp = DateTime.UtcNow
                });

                // Confirm to admin
                await Clients.Caller.SendAsync("ChatAccepted", new
                {
                    CustomerId = customerId,
                    ChatRoomId = chatRoomId,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }

    // Send message in chat
    public async Task SendChatMessage(string chatRoomId, string message)
    {
        var userId = Context.User?.FindFirst("userId")?.Value;
        var userName = Context.User?.FindFirst("email")?.Value ?? "User";
        var userRole = Context.User?.FindFirst("role")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.Group(chatRoomId).SendAsync("ReceiveChatMessage", new
            {
                ChatRoomId = chatRoomId,
                SenderId = userId,
                SenderName = userName,
                SenderRole = userRole,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    // User is typing indicator
    public async Task UserTyping(string chatRoomId)
    {
        var userId = Context.User?.FindFirst("userId")?.Value;
        var userName = Context.User?.FindFirst("email")?.Value ?? "User";

        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.OthersInGroup(chatRoomId).SendAsync("UserIsTyping", new
            {
                UserId = userId,
                UserName = userName,
                ChatRoomId = chatRoomId
            });
        }
    }

    // End chat session
    public async Task EndChat(string chatRoomId)
    {
        var userId = Context.User?.FindFirst("userId")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.Group(chatRoomId).SendAsync("ChatEnded", new
            {
                ChatRoomId = chatRoomId,
                EndedBy = userId,
                Timestamp = DateTime.UtcNow
            });

            // Clean up active chats
            if (ActiveChats.ContainsKey(userId))
            {
                ActiveChats[userId].Clear();
            }
        }
    }

    // Get online support agents count
    public async Task GetOnlineSupportAgents()
    {
        var connections = Clients.Group("support-team");
        // This is simplified; in production you'd track this in a service
        await Clients.Caller.SendAsync("OnlineSupportAgentsCount", new
        {
            Count = UserConnections.Count(u => u.Value != null), // Simplified
            Timestamp = DateTime.UtcNow
        });
    }
}
