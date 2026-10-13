using FoodDelivery.Application.DTOs;

namespace FoodDelivery.Application.Interfaces;

public interface IMenuService
{
    Task<IReadOnlyList<MenuItemDto>> GetAllAsync(string? category = null, string? search = null, CancellationToken cancellationToken = default);
    Task<MenuItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
