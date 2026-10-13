using FluentValidation;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FoodDelivery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
