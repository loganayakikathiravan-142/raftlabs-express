namespace FoodDelivery.Domain.Enums;

public enum OrderStatus
{
    OrderReceived = 1,
    Preparing = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Cancelled = 5
}
