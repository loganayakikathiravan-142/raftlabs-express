using FluentAssertions;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Application.Services;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Domain.Enums;
using FoodDelivery.Domain.Exceptions;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace FoodDelivery.UnitTests.Services;

public class OrderServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IOrderNotificationService> _mockNotificationService;
    private readonly IConfiguration _configuration;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();

        _mockNotificationService = new Mock<IOrderNotificationService>();

        var inMemoryConfig = new Dictionary<string, string?>
        {
            {"OrderSettings:DeliveryFee", "4.50"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        _orderService = new OrderService(_dbContext, _mockNotificationService.Object, _configuration);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CreateOrderAsync_WithAvailableItems_CreatesOrderWithAuthoritativePrices()
    {
        // Arrange
        var item1 = new MenuItem("Truffle Burger", "Delicious burger", 15.00m, "Burgers", "url1", true);
        var item2 = new MenuItem("French Fries", "Crispy fries", 5.00m, "Sides", "url2", true);
        _dbContext.MenuItems.AddRange(item1, item2);
        await _dbContext.SaveChangesAsync();

        var request = new CreateOrderRequest(
            CustomerName: "Alice Smith",
            DeliveryAddress: "789 Pine Street",
            PhoneNumber: "+1-555-4321",
            Items: new List<OrderItemRequest>
            {
                new(item1.Id, 2), // 30.00
                new(item2.Id, 1)  // 5.00
            }
        );

        // Act
        var result = await _orderService.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Subtotal.Should().Be(35.00m);
        result.DeliveryFee.Should().Be(4.50m);
        result.TotalAmount.Should().Be(39.50m);
        result.Status.Should().Be(OrderStatus.OrderReceived);
        result.Items.Should().HaveCount(2);

        // Verify persisted snapshot
        var persisted = await _dbContext.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == result.Id);
        persisted.Should().NotBeNull();
        persisted!.CustomerName.Should().Be("Alice Smith");
        persisted.OrderItems.Should().Contain(i => i.ItemNameSnapshot == "Truffle Burger" && i.UnitPriceSnapshot == 15.00m);

        // Verify notification was dispatched
        _mockNotificationService.Verify(n => n.NotifyOrderCreatedAsync(It.IsAny<OrderDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithNonExistentMenuItem_ThrowsNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            "Bob", "123 Main St", "1234567890",
            new List<OrderItemRequest> { new(missingId, 1) });

        // Act
        var action = () => _orderService.CreateOrderAsync(request);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateOrderAsync_WithUnavailableMenuItem_ThrowsInvalidOrderOperationException()
    {
        // Arrange
        var outOfStockItem = new MenuItem("Sold Out Pasta", "Not available", 20.00m, "Pasta", "url", false);
        _dbContext.MenuItems.Add(outOfStockItem);
        await _dbContext.SaveChangesAsync();

        var request = new CreateOrderRequest(
            "Bob", "123 Main St", "1234567890",
            new List<OrderItemRequest> { new(outOfStockItem.Id, 1) });

        // Act
        var action = () => _orderService.CreateOrderAsync(request);

        // Assert
        await action.Should().ThrowAsync<InvalidOrderOperationException>()
            .WithMessage("*out of stock*");
    }

    [Fact]
    public async Task UpdateStatusAsync_FromReceivedToPreparing_UpdatesAndNotifies()
    {
        // Arrange
        var item = new MenuItem("Pizza", "Cheesy", 12.00m, "Pizza", "url", true);
        _dbContext.MenuItems.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await _orderService.CreateOrderAsync(new CreateOrderRequest(
            "Charlie", "100 Elm", "1234567890", new List<OrderItemRequest> { new(item.Id, 1) }));

        // Act
        var updated = await _orderService.UpdateStatusAsync(created.Id, OrderStatus.Preparing);

        // Assert
        updated.Status.Should().Be(OrderStatus.Preparing);
        _mockNotificationService.Verify(n => n.NotifyOrderStatusChangedAsync(It.IsAny<OrderDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenOrderReceived_DeletesOrderSuccessfully()
    {
        // Arrange
        var item = new MenuItem("Salad", "Fresh", 8.00m, "Salads", "url", true);
        _dbContext.MenuItems.Add(item);
        await _dbContext.SaveChangesAsync();

        var created = await _orderService.CreateOrderAsync(new CreateOrderRequest(
            "Dave", "200 Maple", "1234567890", new List<OrderItemRequest> { new(item.Id, 1) }));

        // Act
        var deleted = await _orderService.DeleteOrderAsync(created.Id);

        // Assert
        deleted.Should().BeTrue();
        (await _dbContext.Orders.FindAsync(created.Id)).Should().BeNull();
    }
}
