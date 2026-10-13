using FoodDelivery.Domain.Enums;

namespace FoodDelivery.Application.DTOs;

public record UpdateOrderStatusRequest(
    OrderStatus Status
);

public record UpdateOrderDetailsRequest(
    string CustomerName,
    string DeliveryAddress,
    string PhoneNumber
);
