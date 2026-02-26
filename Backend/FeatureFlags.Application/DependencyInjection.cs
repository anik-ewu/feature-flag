using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        services.AddMediatR(config => 
        {
            config.RegisterServicesFromAssembly(assembly);
            // Add Behaviors later (e.g. Validation, Logging)
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
