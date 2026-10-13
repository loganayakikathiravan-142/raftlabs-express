using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FoodDelivery.Infrastructure.Services;

public class OrderNotificationService : IOrderNotificationService
{
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly ILogger<OrderNotificationService> _logger;

    public OrderNotificationService(IHubContext<OrderHub> hubContext, ILogger<OrderNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyOrderStatusChangedAsync(OrderDto order, CancellationToken cancellationToken = default)
    {
        try
        {
            var groupName = $"order-{order.OrderNumber.Trim()}";
            _logger.LogInformation("Broadcasting order status update for Order {OrderNumber} (Status: {Status}) to group {GroupName}",
                order.OrderNumber, order.Status, groupName);

            // Notify specific customer tracking the order
            await _hubContext.Clients.Group(groupName).SendAsync("OrderStatusChanged", order, cancellationToken);

            // Notify admin monitoring dashboard
            await _hubContext.Clients.Group("orders-admin").SendAsync("OrderStatusChanged", order, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast order status notification for Order {OrderNumber}", order.OrderNumber);
        }
    }

    public async Task NotifyOrderCreatedAsync(OrderDto order, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Broadcasting new order notification for Order {OrderNumber} to admin group", order.OrderNumber);
            await _hubContext.Clients.Group("orders-admin").SendAsync("OrderCreated", order, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast order created notification for Order {OrderNumber}", order.OrderNumber);
        }
    }
}
