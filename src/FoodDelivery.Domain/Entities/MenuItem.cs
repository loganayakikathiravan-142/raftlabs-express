namespace FoodDelivery.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public MenuItem()
    {
    }

    public MenuItem(string name, string description, decimal price, string category, string imageUrl, bool isAvailable = true)
    {
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        ImageUrl = imageUrl;
        IsAvailable = isAvailable;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
