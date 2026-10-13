namespace FoodDelivery.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid MenuItemId { get; set; }
    public string ItemNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => Math.Round(UnitPriceSnapshot * Quantity, 2, MidpointRounding.AwayFromZero);

    public OrderItem()
    {
    }

    public OrderItem(Guid menuItemId, string itemNameSnapshot, decimal unitPriceSnapshot, int quantity)
    {
        MenuItemId = menuItemId;
        ItemNameSnapshot = itemNameSnapshot;
        UnitPriceSnapshot = unitPriceSnapshot;
        Quantity = quantity;
    }
}
