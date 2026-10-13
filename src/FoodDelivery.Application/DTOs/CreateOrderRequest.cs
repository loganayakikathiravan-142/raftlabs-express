namespace FoodDelivery.Application.DTOs;

public record OrderItemRequest(
    Guid MenuItemId,
    int Quantity
);

public record CreateOrderRequest(
    string CustomerName,
    string DeliveryAddress,
    string PhoneNumber,
    List<OrderItemRequest> Items
);
