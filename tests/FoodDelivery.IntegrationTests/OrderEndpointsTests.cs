using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using FoodDelivery.Application.Common.Models;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Domain.Enums;
using Xunit;

namespace FoodDelivery.IntegrationTests;

public class OrderEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public OrderEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<List<MenuItemDto>> GetAvailableMenuItemsAsync()
    {
        var response = await _client.GetAsync("/api/v1/menu");
        var items = await response.Content.ReadFromJsonAsync<List<MenuItemDto>>(JsonOptions);
        return items!.Where(i => i.IsAvailable).ToList();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task CreateOrder_WithValidData_Returns201CreatedAndCalculatesTotals()
    {
        // Arrange
        var menuItems = await GetAvailableMenuItemsAsync();
        var item1 = menuItems[0];
        var item2 = menuItems[1];

        var createRequest = new CreateOrderRequest(
            CustomerName: "Eleanor Vance",
            DeliveryAddress: "42 Hill House Lane, Boston, MA",
            PhoneNumber: "+1-555-8921",
            Items: new List<OrderItemRequest>
            {
                new(item1.Id, 2),
                new(item2.Id, 1)
            }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var createdOrder = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        createdOrder.Should().NotBeNull();
        createdOrder!.CustomerName.Should().Be("Eleanor Vance");
        createdOrder.Status.Should().Be(OrderStatus.OrderReceived);
        createdOrder.OrderNumber.Should().StartWith("ORD-");

        var expectedSubtotal = (item1.Price * 2) + (item2.Price * 1);
        createdOrder.Subtotal.Should().Be(expectedSubtotal);
        createdOrder.DeliveryFee.Should().Be(3.99m);
        createdOrder.TotalAmount.Should().Be(expectedSubtotal + 3.99m);
        createdOrder.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidPhoneNumber_ReturnsBadRequestWithProblemDetails()
    {
        // Arrange
        var menuItems = await GetAvailableMenuItemsAsync();
        var createRequest = new CreateOrderRequest(
            CustomerName: "John",
            DeliveryAddress: "123 Main St",
            PhoneNumber: "not-a-phone-number",
            Items: new List<OrderItemRequest> { new(menuItems[0].Id, 1) }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("validation");
    }

    [Fact]
    public async Task CreateOrder_WithUnavailableItem_ReturnsUnprocessableEntity()
    {
        // Arrange
        var menuResponse = await _client.GetAsync("/api/v1/menu");
        var allItems = await menuResponse.Content.ReadFromJsonAsync<List<MenuItemDto>>(JsonOptions);
        var unavailableItem = allItems!.First(i => !i.IsAvailable);

        var createRequest = new CreateOrderRequest(
            CustomerName: "John Doe",
            DeliveryAddress: "123 Main St",
            PhoneNumber: "+1-555-0199",
            Items: new List<OrderItemRequest> { new(unavailableItem.Id, 1) }
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("out of stock");
    }

    [Fact]
    public async Task OrderLifecycle_TrackAndUpdateStatus_FollowsExpectedFlow()
    {
        // Arrange - Create Order
        var menuItems = await GetAvailableMenuItemsAsync();
        var createRequest = new CreateOrderRequest(
            CustomerName: "Marcus Aurelius",
            DeliveryAddress: "Palatine Hill, Rome",
            PhoneNumber: "+1-555-0001",
            Items: new List<OrderItemRequest> { new(menuItems[0].Id, 1) }
        );

        var createResponse = await _client.PostAsJsonAsync("/api/v1/orders", createRequest);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);

        // Act 1: Get by Order Number
        var byNumberResponse = await _client.GetAsync($"/api/v1/orders/by-number/{order!.OrderNumber}");
        byNumberResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act 2: Transition to Preparing
        var prepResponse = await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status", new UpdateOrderStatusRequest(OrderStatus.Preparing));
        prepResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var prepOrder = await prepResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        prepOrder!.Status.Should().Be(OrderStatus.Preparing);

        // Act 3: Transition to OutForDelivery
        var dispatchResponse = await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status", new UpdateOrderStatusRequest(OrderStatus.OutForDelivery));
        dispatchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dispatchOrder = await dispatchResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        dispatchOrder!.Status.Should().Be(OrderStatus.OutForDelivery);

        // Act 4: Transition to Delivered
        var deliverResponse = await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status", new UpdateOrderStatusRequest(OrderStatus.Delivered));
        deliverResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var deliverOrder = await deliverResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        deliverOrder!.Status.Should().Be(OrderStatus.Delivered);

        // Act 5: Disallowed transition after Delivered
        var illegalResponse = await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status", new UpdateOrderStatusRequest(OrderStatus.Preparing));
        illegalResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
