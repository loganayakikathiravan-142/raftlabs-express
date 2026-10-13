using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Domain.Enums;
using Xunit;

namespace FoodDelivery.IntegrationTests;

public class OrderStatusUpdatesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public OrderStatusUpdatesIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<OrderDto> CreateTestOrderAsync()
    {
        var menuResponse = await _client.GetAsync("/api/v1/menu");
        var items = await menuResponse.Content.ReadFromJsonAsync<List<MenuItemDto>>(JsonOptions);
        var availableItem = items!.First(i => i.IsAvailable);

        var request = new CreateOrderRequest(
            CustomerName: "Status Lifecycle User",
            DeliveryAddress: "777 Progress Way, Suite 4",
            PhoneNumber: "+1-555-7777",
            Items: new List<OrderItemRequest> { new(availableItem.Id, 1) }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions))!;
    }

    [Fact]
    public async Task StatusUpdate_ThroughStandardHappyPath_TransitionsSequentiallyAndUpdatesTimestamps()
    {
        // 1. Initial State: OrderReceived
        var order = await CreateTestOrderAsync();
        order.Status.Should().Be(OrderStatus.OrderReceived);
        order.UpdatedAtUtc.Should().BeNull();

        // 2. Transition: OrderReceived -> Preparing
        var prepResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Preparing));
        prepResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var prepOrder = await prepResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        prepOrder!.Status.Should().Be(OrderStatus.Preparing);
        prepOrder.StatusText.Should().Be("Preparing");
        prepOrder.UpdatedAtUtc.Should().NotBeNull();

        // 3. Transition: Preparing -> OutForDelivery
        var dispatchResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.OutForDelivery));
        dispatchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dispatchOrder = await dispatchResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        dispatchOrder!.Status.Should().Be(OrderStatus.OutForDelivery);
        dispatchOrder.StatusText.Should().Be("Out for Delivery");

        // 4. Transition: OutForDelivery -> Delivered
        var deliverResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Delivered));
        deliverResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var deliverOrder = await deliverResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        deliverOrder!.Status.Should().Be(OrderStatus.Delivered);
        deliverOrder.StatusText.Should().Be("Delivered");
    }

    [Fact]
    public async Task StatusUpdate_CancellationFromOrderReceived_TransitionsToCancelled()
    {
        // Arrange
        var order = await CreateTestOrderAsync();

        // Act
        var cancelResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Cancelled));

        // Assert
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelledOrder = await cancelResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        cancelledOrder!.Status.Should().Be(OrderStatus.Cancelled);
        cancelledOrder.StatusText.Should().Be("Cancelled");
    }

    [Fact]
    public async Task StatusUpdate_CancellationFromPreparing_TransitionsToCancelled()
    {
        // Arrange
        var order = await CreateTestOrderAsync();
        await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status", new UpdateOrderStatusRequest(OrderStatus.Preparing));

        // Act
        var cancelResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Cancelled));

        // Assert
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelledOrder = await cancelResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        cancelledOrder!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Theory]
    [InlineData(OrderStatus.OutForDelivery)] // Skipping Preparing
    [InlineData(OrderStatus.Delivered)]      // Skipping Preparing & OutForDelivery
    public async Task StatusUpdate_IllegalSkipFromOrderReceived_Returns422UnprocessableEntity(OrderStatus invalidTargetStatus)
    {
        // Arrange
        var order = await CreateTestOrderAsync();

        // Act
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(invalidTargetStatus));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid order status transition");
    }

    [Fact]
    public async Task StatusUpdate_MovingBackwardFromPreparingToOrderReceived_Returns422UnprocessableEntity()
    {
        // Arrange
        var order = await CreateTestOrderAsync();
        await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status", new UpdateOrderStatusRequest(OrderStatus.Preparing));

        // Act: attempt to move backward
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.OrderReceived));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid order status transition");
    }

    [Fact]
    public async Task StatusUpdate_WhenOrderIsDelivered_RejectsAnySubsequentMutation()
    {
        // Arrange: advance order to terminal Delivered state
        var order = await CreateTestOrderAsync();
        await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status", new UpdateOrderStatusRequest(OrderStatus.Preparing));
        await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status", new UpdateOrderStatusRequest(OrderStatus.OutForDelivery));
        await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status", new UpdateOrderStatusRequest(OrderStatus.Delivered));

        // Act: attempt to change status of delivered order
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Preparing));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("terminal state");
    }

    [Fact]
    public async Task StatusUpdate_WhenOrderIsCancelled_RejectsAnySubsequentMutation()
    {
        // Arrange: cancel the order
        var order = await CreateTestOrderAsync();
        await _client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status", new UpdateOrderStatusRequest(OrderStatus.Cancelled));

        // Act: attempt to revive or alter cancelled order
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Preparing));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("terminal state");
    }

    [Fact]
    public async Task StatusUpdate_WithNonExistentOrderId_Returns404NotFound()
    {
        // Act
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{Guid.NewGuid()}/status",
            new UpdateOrderStatusRequest(OrderStatus.Preparing));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
