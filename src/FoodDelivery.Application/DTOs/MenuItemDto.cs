namespace FoodDelivery.Application.DTOs;

public record MenuItemDto(
    Guid Id,
    string Name,
    string Description,
    string ImageUrl,
    decimal Price,
    string Category,
    bool IsAvailable,
    DateTime CreatedAtUtc
);
