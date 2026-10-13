using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Application.Services;

public class MenuService : IMenuService
{
    private readonly IApplicationDbContext _context;

    public MenuService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MenuItemDto>> GetAllAsync(
        string? category = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MenuItems.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cleanCategory = category.Trim().ToLower();
            query = query.Where(m => m.Category.ToLower() == cleanCategory);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var cleanSearch = search.Trim().ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(cleanSearch) ||
                m.Description.ToLower().Contains(cleanSearch));
        }

        var items = await query
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync(cancellationToken);

        return items.Select(MapToDto).ToList();
    }

    public async Task<MenuItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        return item == null ? null : MapToDto(item);
    }

    private static MenuItemDto MapToDto(Domain.Entities.MenuItem m) =>
        new(m.Id, m.Name, m.Description, m.ImageUrl, m.Price, m.Category, m.IsAvailable, m.CreatedAtUtc);
}
