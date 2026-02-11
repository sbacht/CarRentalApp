using CarRental.Application.Common.Behaviours;
using CarRental.Application.Common.Services;
using CarRental.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddAutoMapper(_ => { }, typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        services.AddScoped<IPriceService, PriceService>();
        return services;
    }
}
