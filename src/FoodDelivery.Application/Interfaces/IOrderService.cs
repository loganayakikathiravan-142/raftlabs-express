using FoodDelivery.Application.Common.Models;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderDto>> GetOrdersAsync(int page = 1, int pageSize = 10, OrderStatus? status = null, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatus newStatus, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateDetailsAsync(Guid id, UpdateOrderDetailsRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default);
}
