using FluentAssertions;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Domain.Enums;
using FoodDelivery.Domain.Exceptions;
using Xunit;

namespace FoodDelivery.UnitTests.Domain;

public class OrderDomainTests
{
    private static OrderItem CreateValidItem(decimal unitPrice = 12.50m, int quantity = 2)
    {
        return new OrderItem(Guid.NewGuid(), "Margherita Pizza", unitPrice, quantity);
    }

    [Fact]
    public void CreateOrder_WithValidData_CalculatesCorrectSubtotalAndTotal()
    {
        // Arrange
        var item1 = new OrderItem(Guid.NewGuid(), "Margherita Pizza", 12.50m, 2); // 25.00
        var item2 = new OrderItem(Guid.NewGuid(), "Garlic Bread", 4.25m, 1);     // 4.25
        var deliveryFee = 3.99m;

        // Act
        var order = Order.Create(
            "Jane Doe",
            "123 Flavor Street, Gourmet City",
            "+1-555-0199",
            deliveryFee,
            new[] { item1, item2 });

        // Assert
        order.Should().NotBeNull();
        order.CustomerName.Should().Be("Jane Doe");
        order.DeliveryAddress.Should().Be("123 Flavor Street, Gourmet City");
        order.PhoneNumber.Should().Be("+1-555-0199");
        order.Status.Should().Be(OrderStatus.OrderReceived);
        order.DeliveryFee.Should().Be(3.99m);
        order.Subtotal.Should().Be(29.25m);
        order.TotalAmount.Should().Be(33.24m);
        order.OrderNumber.Should().StartWith("ORD-");
        order.OrderItems.Should().HaveCount(2);
        order.OrderItems.All(i => i.OrderId == order.Id).Should().BeTrue();
    }

    [Fact]
    public void CreateOrder_WithNoItems_ThrowsInvalidOrderOperationException()
    {
        // Act
        var action = () => Order.Create(
            "Jane Doe",
            "123 Flavor Street",
            "+1-555-0199",
            2.99m,
            Array.Empty<OrderItem>());

        // Assert
        action.Should().Throw<InvalidOrderOperationException>()
            .WithMessage("*at least one item*");
    }

    [Theory]
    [InlineData("", "123 Street", "1234567890")]
    [InlineData("   ", "123 Street", "1234567890")]
    [InlineData("Jane Doe", "", "1234567890")]
    [InlineData("Jane Doe", "123 Street", "")]
    public void CreateOrder_WithMissingCustomerDetails_ThrowsArgumentException(
        string customerName, string deliveryAddress, string phoneNumber)
    {
        // Arrange
        var items = new[] { CreateValidItem() };

        // Act
        var action = () => Order.Create(customerName, deliveryAddress, phoneNumber, 2.99m, items);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateOrder_WithNegativeDeliveryFee_ThrowsArgumentException()
    {
        // Arrange
        var items = new[] { CreateValidItem() };

        // Act
        var action = () => Order.Create("Jane Doe", "123 Street", "1234567890", -1.00m, items);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void UpdateStatus_ThroughValidLifecyclePath_Succeeds()
    {
        // Arrange
        var order = Order.Create("Jane Doe", "123 Street", "1234567890", 2.00m, new[] { CreateValidItem() });
        order.Status.Should().Be(OrderStatus.OrderReceived);

        // Act & Assert 1: Received -> Preparing
        order.UpdateStatus(OrderStatus.Preparing);
        order.Status.Should().Be(OrderStatus.Preparing);
        order.UpdatedAtUtc.Should().NotBeNull();

        // Act & Assert 2: Preparing -> OutForDelivery
        order.UpdateStatus(OrderStatus.OutForDelivery);
        order.Status.Should().Be(OrderStatus.OutForDelivery);

        // Act & Assert 3: OutForDelivery -> Delivered
        order.UpdateStatus(OrderStatus.Delivered);
        order.Status.Should().Be(OrderStatus.Delivered);
        order.IsTerminal().Should().BeTrue();
    }

    [Fact]
    public void UpdateStatus_FromDelivered_ThrowsInvalidOrderOperationException()
    {
        // Arrange
        var order = Order.Create("Jane Doe", "123 Street", "1234567890", 2.00m, new[] { CreateValidItem() });
        order.UpdateStatus(OrderStatus.Preparing);
        order.UpdateStatus(OrderStatus.OutForDelivery);
        order.UpdateStatus(OrderStatus.Delivered);

        // Act
        var action = () => order.UpdateStatus(OrderStatus.Preparing);

        // Assert
        action.Should().Throw<InvalidOrderOperationException>()
            .WithMessage("*terminal state*");
    }

    [Fact]
    public void UpdateStatus_WithIllegalTransition_ThrowsInvalidOrderOperationException()
    {
        // Arrange
        var order = Order.Create("Jane Doe", "123 Street", "1234567890", 2.00m, new[] { CreateValidItem() });

        // Act - Jumping directly from OrderReceived to Delivered is illegal
        var action = () => order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        action.Should().Throw<InvalidOrderOperationException>()
            .WithMessage("*Invalid order status transition*");
    }

    [Fact]
    public void Cancel_WhenOrderReceived_TransitionsToCancelled()
    {
        // Arrange
        var order = Order.Create("Jane Doe", "123 Street", "1234567890", 2.00m, new[] { CreateValidItem() });

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.IsTerminal().Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenDelivered_ThrowsInvalidOrderOperationException()
    {
        // Arrange
        var order = Order.Create("Jane Doe", "123 Street", "1234567890", 2.00m, new[] { CreateValidItem() });
        order.UpdateStatus(OrderStatus.Preparing);
        order.UpdateStatus(OrderStatus.OutForDelivery);
        order.UpdateStatus(OrderStatus.Delivered);

        // Act
        var action = () => order.Cancel();

        // Assert
        action.Should().Throw<InvalidOrderOperationException>();
    }
}
