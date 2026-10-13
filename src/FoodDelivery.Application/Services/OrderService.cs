using FoodDelivery.Application.Common.Models;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Domain.Enums;
using FoodDelivery.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FoodDelivery.Application.Services;

public class OrderService : IOrderService
{
    private readonly IApplicationDbContext _context;
    private readonly IOrderNotificationService _notificationService;
    private readonly decimal _defaultDeliveryFee;

    public OrderService(
        IApplicationDbContext context,
        IOrderNotificationService notificationService,
        IConfiguration configuration)
    {
        _context = context;
        _notificationService = notificationService;
        var configuredFee = configuration.GetValue<decimal?>("OrderSettings:DeliveryFee");
        _defaultDeliveryFee = configuredFee ?? 3.99m;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOrderOperationException("An order must contain at least one item.");

        var requestedItemIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();

        // Fetch authoritative menu items from database
        var menuItems = await _context.MenuItems
            .Where(m => requestedItemIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        // Verify all requested menu items exist
        foreach (var id in requestedItemIds)
        {
            if (!menuItems.ContainsKey(id))
            {
                throw new NotFoundException("MenuItem", id);
            }
        }

        // Verify all requested items are currently available
        foreach (var itemReq in request.Items)
        {
            var menuItem = menuItems[itemReq.MenuItemId];
            if (!menuItem.IsAvailable)
            {
                throw new InvalidOrderOperationException($"The item '{menuItem.Name}' is currently out of stock and cannot be ordered.");
            }
        }

        // Build domain order items with frozen price and name snapshots
        var orderItems = new List<OrderItem>();
        foreach (var itemReq in request.Items)
        {
            var menuItem = menuItems[itemReq.MenuItemId];
            orderItems.Add(new OrderItem(
                menuItemId: menuItem.Id,
                itemNameSnapshot: menuItem.Name,
                unitPriceSnapshot: menuItem.Price,
                quantity: itemReq.Quantity
            ));
        }

        // Server-side authoritative calculation
        var order = Order.Create(
            customerName: request.CustomerName,
            deliveryAddress: request.DeliveryAddress,
            phoneNumber: request.PhoneNumber,
            deliveryFee: _defaultDeliveryFee,
            items: orderItems
        );

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(order);

        // Fire-and-forget or background notify real-time listeners
        await _notificationService.NotifyOrderCreatedAsync(dto, cancellationToken);

        return dto;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return null;

        var cleanNumber = orderNumber.Trim();
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderNumber == cleanNumber, cancellationToken);

        return order == null ? null : MapToDto(order);
    }

    public async Task<PagedResult<OrderDto>> GetOrdersAsync(
        int page = 1,
        int pageSize = 10,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = orders.Select(MapToDto).ToList();
        return new PagedResult<OrderDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, OrderStatus newStatus, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            throw new NotFoundException("Order", id);

        order.UpdateStatus(newStatus);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(order);
        await _notificationService.NotifyOrderStatusChangedAsync(dto, cancellationToken);

        return dto;
    }

    public async Task<OrderDto> UpdateDetailsAsync(Guid id, UpdateOrderDetailsRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            throw new NotFoundException("Order", id);

        if (order.Status != OrderStatus.OrderReceived)
        {
            throw new InvalidOrderOperationException(
                $"Customer details cannot be updated because the order is currently '{order.Status}'. Updates are only permitted while '{OrderStatus.OrderReceived}'.");
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerName))
            order.CustomerName = request.CustomerName.Trim();

        if (!string.IsNullOrWhiteSpace(request.DeliveryAddress))
            order.DeliveryAddress = request.DeliveryAddress.Trim();

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            order.PhoneNumber = request.PhoneNumber.Trim();

        order.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(order);
        await _notificationService.NotifyOrderStatusChangedAsync(dto, cancellationToken);

        return dto;
    }

    public async Task<bool> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            return false;

        if (order.Status != OrderStatus.OrderReceived && order.Status != OrderStatus.Cancelled)
        {
            throw new InvalidOrderOperationException(
                $"Order cannot be removed because it is in active preparation or delivery status '{order.Status}'.");
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public static OrderDto MapToDto(Order o) => new(
        Id: o.Id,
        OrderNumber: o.OrderNumber,
        CustomerName: o.CustomerName,
        DeliveryAddress: o.DeliveryAddress,
        PhoneNumber: o.PhoneNumber,
        Status: o.Status,
        StatusText: FormatStatus(o.Status),
        Subtotal: o.Subtotal,
        DeliveryFee: o.DeliveryFee,
        TotalAmount: o.TotalAmount,
        CreatedAtUtc: o.CreatedAtUtc,
        UpdatedAtUtc: o.UpdatedAtUtc,
        Items: o.OrderItems.Select(i => new OrderItemDto(
            Id: i.Id,
            MenuItemId: i.MenuItemId,
            ItemNameSnapshot: i.ItemNameSnapshot,
            UnitPriceSnapshot: i.UnitPriceSnapshot,
            Quantity: i.Quantity,
            LineTotal: i.LineTotal
        )).ToList()
    );

    private static string FormatStatus(OrderStatus status) => status switch
    {
        OrderStatus.OrderReceived => "Order Received",
        OrderStatus.Preparing => "Preparing",
        OrderStatus.OutForDelivery => "Out for Delivery",
        OrderStatus.Delivered => "Delivered",
        OrderStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };
}
