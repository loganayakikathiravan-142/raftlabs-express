using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FoodDelivery.Infrastructure.Hubs;

public class OrderHub : Hub
{
    private readonly ILogger<OrderHub> _logger;

    public OrderHub(ILogger<OrderHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinOrderGroup(string orderNumber)
    {
        if (!string.IsNullOrWhiteSpace(orderNumber))
        {
            var groupName = $"order-{orderNumber.Trim()}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} joined real-time group {GroupName}", Context.ConnectionId, groupName);
        }
    }

    public async Task LeaveOrderGroup(string orderNumber)
    {
        if (!string.IsNullOrWhiteSpace(orderNumber))
        {
            var groupName = $"order-{orderNumber.Trim()}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
        }
    }

    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "orders-admin");
        _logger.LogInformation("Connection {ConnectionId} joined admin tracking group", Context.ConnectionId);
    }
}
