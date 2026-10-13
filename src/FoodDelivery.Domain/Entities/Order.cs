using FoodDelivery.Domain.Enums;
using FoodDelivery.Domain.Exceptions;

namespace FoodDelivery.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.OrderReceived;
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();

    public Order()
    {
    }

    public static Order Create(
        string customerName,
        string deliveryAddress,
        string phoneNumber,
        decimal deliveryFee,
        IEnumerable<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));

        if (string.IsNullOrWhiteSpace(deliveryAddress))
            throw new ArgumentException("Delivery address is required.", nameof(deliveryAddress));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        var itemList = items?.ToList() ?? new List<OrderItem>();
        if (itemList.Count == 0)
            throw new InvalidOrderOperationException("An order must contain at least one item.");

        if (deliveryFee < 0)
            throw new ArgumentException("Delivery fee cannot be negative.", nameof(deliveryFee));

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            CustomerName = customerName.Trim(),
            DeliveryAddress = deliveryAddress.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            Status = OrderStatus.OrderReceived,
            DeliveryFee = Math.Round(deliveryFee, 2, MidpointRounding.AwayFromZero),
            CreatedAtUtc = DateTime.UtcNow,
            OrderItems = itemList
        };

        foreach (var item in order.OrderItems)
        {
            item.OrderId = order.Id;
        }

        order.RecalculateTotals();
        return order;
    }

    public void RecalculateTotals()
    {
        Subtotal = Math.Round(OrderItems.Sum(i => i.LineTotal), 2, MidpointRounding.AwayFromZero);
        TotalAmount = Math.Round(Subtotal + DeliveryFee, 2, MidpointRounding.AwayFromZero);
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (Status == newStatus)
            return;

        if (IsTerminal())
            throw new InvalidOrderOperationException($"Cannot transition status from terminal state '{Status}' to '{newStatus}'.");

        bool isValidTransition = (Status, newStatus) switch
        {
            (OrderStatus.OrderReceived, OrderStatus.Preparing) => true,
            (OrderStatus.OrderReceived, OrderStatus.Cancelled) => true,
            (OrderStatus.Preparing, OrderStatus.OutForDelivery) => true,
            (OrderStatus.Preparing, OrderStatus.Cancelled) => true,
            (OrderStatus.OutForDelivery, OrderStatus.Delivered) => true,
            _ => false
        };

        if (!isValidTransition)
        {
            throw new InvalidOrderOperationException(
                $"Invalid order status transition from '{Status}' to '{newStatus}'. Permitted transitions from '{Status}' are: {GetAllowedTransitionsDescription(Status)}.");
        }

        Status = newStatus;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        UpdateStatus(OrderStatus.Cancelled);
    }

    public bool IsTerminal() => Status is OrderStatus.Delivered or OrderStatus.Cancelled;

    private static string GetAllowedTransitionsDescription(OrderStatus status) => status switch
    {
        OrderStatus.OrderReceived => $"{OrderStatus.Preparing}, {OrderStatus.Cancelled}",
        OrderStatus.Preparing => $"{OrderStatus.OutForDelivery}, {OrderStatus.Cancelled}",
        OrderStatus.OutForDelivery => $"{OrderStatus.Delivered}",
        OrderStatus.Delivered => "None (terminal state)",
        OrderStatus.Cancelled => "None (terminal state)",
        _ => "None"
    };

    public static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomSuffix = Random.Shared.Next(1000, 9999);
        return $"ORD-{timestamp}-{randomSuffix}";
    }
}
