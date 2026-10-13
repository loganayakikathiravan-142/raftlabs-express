using FoodDelivery.Application.DTOs;

namespace FoodDelivery.Application.Interfaces;

public interface IOrderNotificationService
{
    Task NotifyOrderStatusChangedAsync(OrderDto order, CancellationToken cancellationToken = default);
    Task NotifyOrderCreatedAsync(OrderDto order, CancellationToken cancellationToken = default);
}
