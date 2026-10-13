using FluentValidation;
using FoodDelivery.Application.DTOs;

namespace FoodDelivery.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    private const string PhoneRegexPattern = @"^(\+?[0-9]{1,4}[\s\-]?)?(\([0-9]{1,4}\)[\s\-]?)?[0-9\s\-]{6,16}$";

    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(100).WithMessage("Customer name must not exceed 100 characters.")
            .Matches(@"^[a-zA-Z\s\.\'\-]+$").WithMessage("Customer name contains invalid characters.");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("Delivery address is required.")
            .MinimumLength(5).WithMessage("Delivery address must be at least 5 characters.")
            .MaximumLength(250).WithMessage("Delivery address must not exceed 250 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(PhoneRegexPattern).WithMessage("Please enter a valid phone number (e.g., +1-555-0199 or 9876543210).");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("An order must contain at least one item.")
            .Must(items => items != null && items.Count > 0).WithMessage("An order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MenuItemId)
                .NotEmpty().WithMessage("Menu item identifier is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.")
                .LessThanOrEqualTo(99).WithMessage("Quantity cannot exceed 99 items per entry.");
        });
    }
}
