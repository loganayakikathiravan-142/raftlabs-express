using FluentValidation;
using FoodDelivery.Application.DTOs;

namespace FoodDelivery.Application.Validators;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status value provided.");
    }
}
