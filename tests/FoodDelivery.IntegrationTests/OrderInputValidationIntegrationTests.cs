using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FoodDelivery.Application.DTOs;
using Xunit;

namespace FoodDelivery.IntegrationTests;

public class OrderInputValidationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public OrderInputValidationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> GetValidMenuItemIdAsync()
    {
        var response = await _client.GetAsync("/api/v1/menu");
        var items = await response.Content.ReadFromJsonAsync<List<MenuItemDto>>(JsonOptions);
        return items!.First(i => i.IsAvailable).Id;
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateOrder_WithEmptyCustomerName_Returns400BadRequestWithProblemDetails(string invalidName)
    {
        var menuItemId = await GetValidMenuItemIdAsync();
        var request = new CreateOrderRequest(
            CustomerName: invalidName,
            DeliveryAddress: "123 Valid Street Address",
            PhoneNumber: "+1-555-0199",
            Items: new List<OrderItemRequest> { new(menuItemId, 1) }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Customer name is required");
    }

    [Fact]
    public async Task CreateOrder_WithCustomerNameExceeding100Chars_Returns400BadRequest()
    {
        var menuItemId = await GetValidMenuItemIdAsync();
        var request = new CreateOrderRequest(
            CustomerName: new string('A', 101),
            DeliveryAddress: "123 Valid Street Address",
            PhoneNumber: "+1-555-0199",
            Items: new List<OrderItemRequest> { new(menuItemId, 1) }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("exceed 100 characters");
    }

    [Theory]
    [InlineData("John123")]
    [InlineData("User@!#$")]
    public async Task CreateOrder_WithInvalidCharactersInName_Returns400BadRequest(string invalidName)
    {
        var menuItemId = await GetValidMenuItemIdAsync();
        var request = new CreateOrderRequest(
            CustomerName: invalidName,
            DeliveryAddress: "123 Valid Street Address",
            PhoneNumber: "+1-555-0199",
            Items: new List<OrderItemRequest> { new(menuItemId, 1) }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("invalid characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")] // shorter than 5 chars
    public async Task CreateOrder_WithInvalidAddress_Returns400BadRequest(string invalidAddress)
    {
        var menuItemId = await GetValidMenuItemIdAsync();
        var request = new CreateOrderRequest(
            CustomerName: "Valid Name",
            DeliveryAddress: invalidAddress,
            PhoneNumber: "+1-555-0199",
            Items: new List<OrderItemRequest> { new(menuItemId, 1) }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Delivery address");
    }

    [Fact]
    public async Task CreateOrder_WithAddressExceeding250Chars_Returns400BadRequest()
    {
        var menuItemId = await GetValidMenuItemIdAsync();
        var request = new CreateOrderRequest(
            CustomerName: "Valid Name",
            DeliveryAddress: new string('D', 251),
            PhoneNumber: "+1-555-0199",
            Items: new List<OrderItemRequest> { new(menuItemId, 1) }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("exceed 250 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-number")]
    [InlineData("123")]
    [InlineData("abcdefghij")]
    public async Task CreateOrder_WithInvalidPhoneFormat_Returns400BadRequest(string invalidPhone)
    {
        var menuItemId = await GetValidMenuItemIdAsync();
        var request = new CreateOrderRequest(
            CustomerName: "Valid Name",
            DeliveryAddress: "123 Valid Street Address",
            PhoneNumber: invalidPhone,
            Items: new List<OrderItemRequest> { new(menuItemId, 1) }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("phone number");
    }

    [Theory]
    [InlineData("+1-555-0199")]
    [InlineData("9876543210")]
    [InlineData("(555) 123-4567")]
    [InlineData("+91 98765 43210")]
    public async Task CreateOrder_WithValidPhoneFormats_PassesValidation(string validPhone)
    {
        var menuItemId = await GetValidMenuItemIdAsync();
        var request = new CreateOrderRequest(
            CustomerName: "Phone Test User",
            DeliveryAddress: "123 Valid Street Address",
            PhoneNumber: validPhone,
            Items: new List<OrderItemRequest> { new(menuItemId, 1) }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateOrder_WithEmptyItemsList_Returns400BadRequest()
    {
        var request = new CreateOrderRequest(
            CustomerName: "Valid Name",
            DeliveryAddress: "123 Valid Street Address",
            PhoneNumber: "+1-555-0199",
            Items: new List<OrderItemRequest>()
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("at least one item");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public async Task CreateOrder_WithOutOfRangeQuantity_Returns400BadRequest(int invalidQty)
    {
        var menuItemId = await GetValidMenuItemIdAsync();
        var request = new CreateOrderRequest(
            CustomerName: "Valid Name",
            DeliveryAddress: "123 Valid Street Address",
            PhoneNumber: "+1-555-0199",
            Items: new List<OrderItemRequest> { new(menuItemId, invalidQty) }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Quantity");
    }

    [Fact]
    public async Task CreateOrder_WithEmptyMenuItemGuid_Returns400BadRequest()
    {
        var request = new CreateOrderRequest(
            CustomerName: "Valid Name",
            DeliveryAddress: "123 Valid Street Address",
            PhoneNumber: "+1-555-0199",
            Items: new List<OrderItemRequest> { new(Guid.Empty, 1) }
        );

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Menu item identifier is required");
    }
}
