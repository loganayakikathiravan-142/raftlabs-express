using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Domain.Enums;
using Xunit;

namespace FoodDelivery.IntegrationTests;

public class FullCustomerJourneyEndToEndTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public FullCustomerJourneyEndToEndTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CustomerJourney_FromMenuBrowsingToFinalDelivery_ExecutesSeamlessly()
    {
        // 1. Browsing the Food Menu
        var menuResponse = await _client.GetAsync("/api/v1/menu");
        menuResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var menuItems = await menuResponse.Content.ReadFromJsonAsync<List<MenuItemDto>>(JsonOptions);
        menuItems.Should().NotBeNull();
        menuItems!.Count.Should().BeGreaterThanOrEqualTo(10);

        // Filter by Category
        var pizzaResponse = await _client.GetAsync("/api/v1/menu?category=Pizza");
        var pizzaItems = await pizzaResponse.Content.ReadFromJsonAsync<List<MenuItemDto>>(JsonOptions);
        pizzaItems.Should().NotBeNull();
        pizzaItems!.All(p => p.Category == "Pizza").Should().BeTrue();

        // 2. Select Items to Purchase
        var margherita = pizzaItems.First(p => p.IsAvailable);
        var burgerResponse = await _client.GetAsync("/api/v1/menu?category=Burgers");
        var burgerItems = await burgerResponse.Content.ReadFromJsonAsync<List<MenuItemDto>>(JsonOptions);
        var burger = burgerItems!.First(b => b.IsAvailable);

        // 3. Attempt Checkout with Invalid Input (Negative Validation)
        var invalidOrderRequest = new CreateOrderRequest(
            CustomerName: "", // Missing
            DeliveryAddress: "Short", // Too short
            PhoneNumber: "not-a-number", // Invalid format
            Items: new List<OrderItemRequest> { new(margherita.Id, 0) } // Invalid quantity
        );

        var badRequestResponse = await _client.PostAsJsonAsync("/api/v1/orders", invalidOrderRequest);
        badRequestResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 4. Place Valid Order through Real REST API
        var validOrderRequest = new CreateOrderRequest(
            CustomerName: "Dr. Gregory House",
            DeliveryAddress: "221 Princeton-Plainsboro Boulevard, Princeton, NJ",
            PhoneNumber: "+1-555-0199",
            Items: new List<OrderItemRequest>
            {
                new(margherita.Id, 2), // 2 Pizzas
                new(burger.Id, 1)      // 1 Burger
            }
        );

        var createResponse = await _client.PostAsJsonAsync("/api/v1/orders", validOrderRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Headers.Location.Should().NotBeNull();

        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        order.Should().NotBeNull();
        order!.OrderNumber.Should().StartWith("ORD-");
        order.Status.Should().Be(OrderStatus.OrderReceived);
        order.CustomerName.Should().Be("Dr. Gregory House");

        // Authoritative Server-side Price Calculations Check
        var expectedSubtotal = (margherita.Price * 2) + (burger.Price * 1);
        order.Subtotal.Should().Be(expectedSubtotal);
        order.DeliveryFee.Should().Be(3.99m);
        order.TotalAmount.Should().Be(expectedSubtotal + 3.99m);

        // 5. Track Order via Public Order Number
        var trackResponse = await _client.GetAsync($"/api/v1/orders/by-number/{order.OrderNumber}");
        trackResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var trackedOrder = await trackResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions);
        trackedOrder!.Id.Should().Be(order.Id);

        // 6. Kitchen Operations: Advance Through Full Lifecycle
        // Step A: OrderReceived -> Preparing
        var prepResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Preparing));
        prepResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await prepResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions))!.Status.Should().Be(OrderStatus.Preparing);

        // Step B: Preparing -> OutForDelivery
        var dispatchResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.OutForDelivery));
        dispatchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await dispatchResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions))!.Status.Should().Be(OrderStatus.OutForDelivery);

        // Step C: OutForDelivery -> Delivered
        var deliveredResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Delivered));
        deliveredResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await deliveredResponse.Content.ReadFromJsonAsync<OrderDto>(JsonOptions))!.Status.Should().Be(OrderStatus.Delivered);

        // 7. Verify Invariants on Terminal Order
        // Attempting to move backwards from Delivered to Preparing is rejected
        var illegalTransitionResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/orders/{order.Id}/status",
            new UpdateOrderStatusRequest(OrderStatus.Preparing));
        illegalTransitionResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        // Attempting to delete a delivered order is rejected
        var deleteResponse = await _client.DeleteAsync($"/api/v1/orders/{order.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
