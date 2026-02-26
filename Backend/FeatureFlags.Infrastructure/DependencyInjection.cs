using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Infrastructure.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Database Context
        // services.AddDbContext<ApplicationDbContext>(...);

        // Add Redis Distributed Caching
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "FeatureFlagsSaaS_";
        });

        // Add Services
        services.AddScoped<IFeatureFlagCacheService, FeatureFlagCacheService>();
        // services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();

        return services;
    }
}
