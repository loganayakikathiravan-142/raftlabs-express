using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.Api.Controllers;

[ApiController]
[Route("api/v1/menu")]
[Produces("application/json")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    /// <summary>
    /// Retrieves all food menu items, optionally filtered by category or search keyword.
    /// </summary>
    /// <param name="category">Optional food category filter (e.g. Pizza, Burgers, Pasta, Salads, Desserts, Beverages).</param>
    /// <param name="search">Optional text query matching food name or description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of food menu items.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MenuItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MenuItemDto>>> GetMenu(
        [FromQuery] string? category,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var items = await _menuService.GetAllAsync(category, search, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Retrieves a specific menu item by its unique identifier.
    /// </summary>
    /// <param name="id">Menu item ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The menu item details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenuItemDto>> GetMenuItemById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _menuService.GetByIdAsync(id, cancellationToken);
        if (item == null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Menu Item Not Found",
                Detail = $"No menu item was found matching identifier '{id}'."
            });
        }

        return Ok(item);
    }
}
