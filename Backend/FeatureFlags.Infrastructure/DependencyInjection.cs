using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext, Redis, etc. here later
        // services.AddDbContext<ApplicationDbContext>(...);
        // services.AddStackExchangeRedisCache(...);

        return services;
    }
}
