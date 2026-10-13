using FluentAssertions;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Validators;
using Xunit;

namespace FoodDelivery.UnitTests.Validators;

public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator = new();

    private static CreateOrderRequest CreateValidRequest() => new(
        CustomerName: "John Doe",
        DeliveryAddress: "456 Oak Avenue, Apt 2B",
        PhoneNumber: "+1-555-0199",
        Items: new List<OrderItemRequest>
        {
            new(Guid.NewGuid(), 2)
        }
    );

    [Fact]
    public async Task Validate_WithValidRequest_ReturnsValidResult()
    {
        var request = CreateValidRequest();
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_WithEmptyCustomerName_FailsValidation(string? invalidName)
    {
        var request = CreateValidRequest() with { CustomerName = invalidName! };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOrderRequest.CustomerName));
    }

    [Fact]
    public async Task Validate_WithCustomerNameExceeding100Chars_FailsValidation()
    {
        var request = CreateValidRequest() with { CustomerName = new string('A', 101) };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOrderRequest.CustomerName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")] // less than 5 characters
    [InlineData(null)]
    public async Task Validate_WithInvalidDeliveryAddress_FailsValidation(string? invalidAddress)
    {
        var request = CreateValidRequest() with { DeliveryAddress = invalidAddress! };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOrderRequest.DeliveryAddress));
    }

    [Theory]
    [InlineData("+1-555-0199")]
    [InlineData("9876543210")]
    [InlineData("(555) 123-4567")]
    [InlineData("+91 98765 43210")]
    public async Task Validate_WithValidPhoneFormats_PassesValidation(string validPhone)
    {
        var request = CreateValidRequest() with { PhoneNumber = validPhone };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("phone")]
    [InlineData("123")]
    [InlineData("abcdefghij")]
    public async Task Validate_WithInvalidPhoneFormats_FailsValidation(string invalidPhone)
    {
        var request = CreateValidRequest() with { PhoneNumber = invalidPhone };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOrderRequest.PhoneNumber));
    }

    [Fact]
    public async Task Validate_WithEmptyItemsList_FailsValidation()
    {
        var request = CreateValidRequest() with { Items = new List<OrderItemRequest>() };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOrderRequest.Items));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public async Task Validate_WithInvalidItemQuantity_FailsValidation(int invalidQty)
    {
        var request = CreateValidRequest() with
        {
            Items = new List<OrderItemRequest> { new(Guid.NewGuid(), invalidQty) }
        };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.StartsWith("Items[0].Quantity"));
    }
}
