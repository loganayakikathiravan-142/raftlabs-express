using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FoodDelivery.Application.DTOs;
using Xunit;

namespace FoodDelivery.IntegrationTests;

public class MenuEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MenuEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMenu_ReturnsOkWithSeededItems()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/menu");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<MenuItemDto>>();
        items.Should().NotBeNull();
        items!.Count.Should().BeGreaterThan(5);
        items.All(i => !string.IsNullOrEmpty(i.Name)).Should().BeTrue();
        items.All(i => i.Price > 0).Should().BeTrue();
    }

    [Fact]
    public async Task GetMenu_FilteredByCategory_ReturnsOnlyCategoryItems()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/menu?category=Pizza");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<MenuItemDto>>();
        items.Should().NotBeNull();
        items!.Should().NotBeEmpty();
        items.All(i => i.Category.Equals("Pizza", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task GetMenu_FilteredBySearch_ReturnsMatchingItems()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/menu?search=Burger");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<MenuItemDto>>();
        items.Should().NotBeNull();
        items!.Should().NotBeEmpty();
        items.All(i => i.Name.Contains("Burger", StringComparison.OrdinalIgnoreCase) ||
                       i.Description.Contains("Burger", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task GetMenuItemById_WhenItemExists_ReturnsOk()
    {
        // Arrange
        var allItemsResponse = await _client.GetAsync("/api/v1/menu");
        var allItems = await allItemsResponse.Content.ReadFromJsonAsync<List<MenuItemDto>>();
        var targetItem = allItems!.First();

        // Act
        var response = await _client.GetAsync($"/api/v1/menu/{targetItem.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await response.Content.ReadFromJsonAsync<MenuItemDto>();
        item.Should().NotBeNull();
        item!.Id.Should().Be(targetItem.Id);
        item.Name.Should().Be(targetItem.Name);
    }

    [Fact]
    public async Task GetMenuItemById_WhenNonExistent_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/menu/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
