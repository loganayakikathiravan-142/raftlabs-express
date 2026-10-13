using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Application.DTOs;

public record OrderItemDto(
    Guid Id,
    Guid MenuItemId,
    string ItemNameSnapshot,
    decimal UnitPriceSnapshot,
    int Quantity,
    decimal LineTotal
);

public record OrderDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string DeliveryAddress,
    string PhoneNumber,
    OrderStatus Status,
    string StatusText,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    List<OrderItemDto> Items
);
