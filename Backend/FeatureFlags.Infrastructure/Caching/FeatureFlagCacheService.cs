using System.Collections.Concurrent;
using System.Text.Json;
using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Application.Features.FeatureFlags.DTOs;
using FeatureFlags.Domain.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.Infrastructure.Caching;

public class FeatureFlagCacheService : IFeatureFlagCacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IFeatureFlagRepository _featureFlagRepository;
    private readonly ILogger<FeatureFlagCacheService> _logger;
    
    // Memory cache for anti-stampede (locking mechanism)
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public FeatureFlagCacheService(
        IDistributedCache distributedCache,
        IFeatureFlagRepository featureFlagRepository,
        ILogger<FeatureFlagCacheService> logger)
    {
        _distributedCache = distributedCache;
        _featureFlagRepository = featureFlagRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<FeatureFlagDto>> GetFlagsAsync(Guid projectId, string environment, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(projectId, environment);

        // 1. Try to fetch from Redis
        var cachedData = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogDebug("Cache HIT for key: {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<IEnumerable<FeatureFlagDto>>(cachedData) ?? Enumerable.Empty<FeatureFlagDto>();
        }

        _logger.LogDebug("Cache MISS for key: {CacheKey}. Fetching from DB...", cacheKey);

        // 2. Cache Stampede Protection (Double-Checked Locking with Semaphore)
        var myLock = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        
        await myLock.WaitAsync(cancellationToken);
        try
        {
            // 3. Double-check cache inside the lock in case another thread already populated it
            cachedData = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<IEnumerable<FeatureFlagDto>>(cachedData) ?? Enumerable.Empty<FeatureFlagDto>();
            }

            // 4. Fetch from database (repository)
            var flagsFromDb = await _featureFlagRepository.GetByProjectIdAsync(projectId, cancellationToken);
            
            // Filter by environment and map to DTOs
            if (!Enum.TryParse<EnvironmentType>(environment, true, out var envType))
            {
                _logger.LogWarning("Invalid environment requested: {Environment}", environment);
                return Enumerable.Empty<FeatureFlagDto>();
            }

            var dtos = flagsFromDb
                .Where(f => f.Environment == envType)
                .Select(f => new FeatureFlagDto(
                    f.Id,
                    f.ProjectId,
                    f.Key,
                    f.Description,
                    f.IsEnabled,
                    f.RolloutPercentage.Value,
                    f.Environment.ToString(),
                    f.TargetingRules.Select(r => new TargetingRuleDto(
                        r.Id,
                        r.Type.ToString(),
                        r.Operator.ToString(),
                        r.Value
                    ))
                )).ToList();

            // 5. Populate cache
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30), // Prevent stale data indefinitely
                SlidingExpiration = TimeSpan.FromMinutes(10) // Keep hot data warm
            };

            var serializedData = JsonSerializer.Serialize(dtos);
            await _distributedCache.SetStringAsync(cacheKey, serializedData, cacheOptions, cancellationToken);

            return dtos;
        }
        finally
        {
            myLock.Release();
        }
    }

    public async Task InvalidateProjectCacheAsync(Guid projectId, string environment, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(projectId, environment);
        
        _logger.LogInformation("Invalidating cache for key: {CacheKey}", cacheKey);
        
        await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
    }

    private static string GetCacheKey(Guid projectId, string environment)
    {
        // Structured Cache Keys for predictable invalidation
        return $"FeatureFlags:Project:{projectId}:Env:{environment.ToLowerInvariant()}";
    }
}
