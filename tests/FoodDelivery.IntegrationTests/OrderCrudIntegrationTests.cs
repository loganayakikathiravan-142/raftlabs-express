using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FoodDelivery.Application.Common.Models;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Domain.Enums;
using Xunit;

namespace FoodDelivery.IntegrationTests;

public class OrderCrudIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public OrderCrudIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<List<MenuItemDto>> GetAvailableMenuItemsAsync()
    {
        var response = await _client.GetAsync("/api/v1/menu");
        var items = await response.Content.ReadFromJsonAsync<List<MenuItemDto>>(JsonOptions);
        return items!.Where(i => i.IsAvailable).ToList();
    }

    private async Task<OrderDto> CreateTestOrderAsync(string customerName = "Test Customer")
    {
        var menuItems = await GetAvailableMenuItemsAsync();
        var request = new CreateOrderRequest(
            CustomerName: customerName,
            DeliveryAddress: "123 Culinary Boulevard, Suite 100",
            PhoneNumber: "+1-555-0199",
            Items: new List<OrderItemRequest> { new(menuItems[0].Id, 1) }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        order.Should().NotBeNull();
        return order!;
    }

    #region 1. CREATE Operations

    [Fact]
    public async Task CreateOrder_WithMultipleItems_CalculatesAuthoritativePricingAndStoresSnapshots()
    {
        // Arrange
        var menuItems = await GetAvailableMenuItemsAsync();
        var item1 = menuItems[0];
        var item2 = menuItems[1];

        var request = new CreateOrderRequest(
            CustomerName: "Ada Lovelace",
            DeliveryAddress: "10 Computing Lane, London",
            PhoneNumber: "+44-20-7946-0912",
            Items: new List<OrderItemRequest>
            {
                new(item1.Id, 3),
                new(item2.Id, 2)
            }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var created = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        created.Should().NotBeNull();
        created!.OrderNumber.Should().StartWith("ORD-");
        created.CustomerName.Should().Be("Ada Lovelace");
        created.Status.Should().Be(OrderStatus.OrderReceived);

        var expectedSubtotal = (item1.Price * 3) + (item2.Price * 2);
        created.Subtotal.Should().Be(expectedSubtotal);
        created.DeliveryFee.Should().Be(3.99m);
        created.TotalAmount.Should().Be(expectedSubtotal + 3.99m);

        created.Items.Should().HaveCount(2);
        created.Items.Should().Contain(i => i.ItemNameSnapshot == item1.Name && i.UnitPriceSnapshot == item1.Price && i.Quantity == 3);
        created.Items.Should().Contain(i => i.ItemNameSnapshot == item2.Name && i.UnitPriceSnapshot == item2.Price && i.Quantity == 2);
    }

    [Fact]
    public async Task CreateOrder_WithNonExistentMenuItem_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateOrderRequest(
            CustomerName: "John Doe",
            DeliveryAddress: "404 Street, Null Island",
            PhoneNumber: "+1-555-0100",
            Items: new List<OrderItemRequest> { new(Guid.NewGuid(), 1) }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateOrder_WithOutOfStockMenuItem_ReturnsUnprocessableEntity()
    {
        // Arrange
        var allItemsResponse = await _client.GetAsync("/api/v1/menu");
        var allItems = await allItemsResponse.Content.ReadFromJsonAsync<List<MenuItemDto>>(JsonOptions);
        var outOfStockItem = allItems!.First(i => !i.IsAvailable);

        var request = new CreateOrderRequest(
            CustomerName: "John Doe",
            DeliveryAddress: "500 Stock Road",
            PhoneNumber: "+1-555-0100",
            Items: new List<OrderItemRequest> { new(outOfStockItem.Id, 1) }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("out of stock");
    }

    #endregion

    #region 2. READ Operations

    [Fact]
    public async Task GetOrderById_WhenOrderExists_Returns200WithOrderDetails()
    {
        // Arrange
        var created = await CreateTestOrderAsync("Alan Turing");

        // Act
        var response = await _client.GetAsync($"/api/v1/orders/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        order.Should().NotBeNull();
        order!.Id.Should().Be(created.Id);
        order.OrderNumber.Should().Be(created.OrderNumber);
        order.CustomerName.Should().Be("Alan Turing");
    }

    [Fact]
    public async Task GetOrderById_WhenOrderDoesNotExist_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrderByNumber_WhenOrderExists_Returns200WithMatchingOrder()
    {
        // Arrange
        var created = await CreateTestOrderAsync("Grace Hopper");

        // Act
        var response = await _client.GetAsync($"/api/v1/orders/by-number/{created.OrderNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        order.Should().NotBeNull();
        order!.OrderNumber.Should().Be(created.OrderNumber);
        order.CustomerName.Should().Be("Grace Hopper");
    }

    [Fact]
    public async Task GetOrderByNumber_WhenOrderDoesNotExist_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/orders/by-number/ORD-99999999999999-9999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrders_WithPagination_ReturnsPaginatedResultWithMetadata()
    {
        // Arrange: ensure multiple orders exist
        await CreateTestOrderAsync("Pagination Order One");
        await CreateTestOrderAsync("Pagination Order Two");

        // Act
        var response = await _client.GetAsync("/api/v1/orders?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResult<OrderDto>>(JsonOptions);
        paged.Should().NotBeNull();
        paged!.Items.Should().NotBeEmpty();
        paged.Page.Should().Be(1);
        paged.PageSize.Should().Be(5);
        paged.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetOrders_WithStatusFilter_ReturnsOnlyMatchingStatusOrders()
    {
        // Arrange
        var created = await CreateTestOrderAsync("Filtered Order");
        await _client.PatchAsJsonAsync($"/api/v1/orders/{created.Id}/status", new UpdateOrderStatusRequest(OrderStatus.Preparing));

        // Act
        var response = await _client.GetAsync("/api/v1/orders?status=Preparing");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResult<OrderDto>>(JsonOptions);
        paged.Should().NotBeNull();
        paged!.Items.All(o => o.Status == OrderStatus.Preparing).Should().BeTrue();
    }

    #endregion

    #region 3. UPDATE Operations (Customer Details & Status)

    [Fact]
    public async Task UpdateDetails_WhenOrderReceived_UpdatesAddressAndContactSuccessfully()
    {
        // Arrange
        var created = await CreateTestOrderAsync("Original Name");

        var updateRequest = new UpdateOrderDetailsRequest(
            CustomerName: "Updated Customer Name",
            DeliveryAddress: "999 New Gourmet Avenue, Penthouse 1",
            PhoneNumber: "+1-555-9999"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/orders/{created.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        updated.Should().NotBeNull();
        updated!.CustomerName.Should().Be("Updated Customer Name");
        updated.DeliveryAddress.Should().Be("999 New Gourmet Avenue, Penthouse 1");
        updated.PhoneNumber.Should().Be("+1-555-9999");
        updated.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateDetails_WhenOrderIsPreparing_RejectsWith422UnprocessableEntity()
    {
        // Arrange
        var created = await CreateTestOrderAsync("Late Customer");
        await _client.PatchAsJsonAsync($"/api/v1/orders/{created.Id}/status", new UpdateOrderStatusRequest(OrderStatus.Preparing));

        var updateRequest = new UpdateOrderDetailsRequest(
            CustomerName: "Too Late Name",
            DeliveryAddress: "Cannot Change Address Anymore",
            PhoneNumber: "+1-555-1111"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/orders/{created.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("only permitted while");
    }

    #endregion

    #region 4. DELETE Operations

    [Fact]
    public async Task DeleteOrder_WhenOrderReceived_SucceedsWith204AndRemovesOrder()
    {
        // Arrange
        var created = await CreateTestOrderAsync("Order To Delete");

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/v1/orders/{created.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it is no longer retrievable
        var getResponse = await _client.GetAsync($"/api/v1/orders/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOrder_WhenOrderIsPreparing_RejectsWith422UnprocessableEntity()
    {
        // Arrange
        var created = await CreateTestOrderAsync("Kitchen Cooking Order");
        await _client.PatchAsJsonAsync($"/api/v1/orders/{created.Id}/status", new UpdateOrderStatusRequest(OrderStatus.Preparing));

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/v1/orders/{created.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var content = await deleteResponse.Content.ReadAsStringAsync();
        content.Should().Contain("cannot be removed");
    }

    [Fact]
    public async Task DeleteOrder_WhenOrderDoesNotExist_Returns404NotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/v1/orders/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
