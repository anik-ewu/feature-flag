using System.Collections.Concurrent;
using FeatureFlags.Application.Features.FeatureFlags.DTOs;

namespace FeatureFlags.Application.Common.Interfaces;

public interface IFeatureFlagCacheService
{
    /// <summary>
    /// Gets all feature flags for a specific project and environment. 
    /// Internally uses Redis + MemoryCache for <10ms response times.
    /// </summary>
    Task<IEnumerable<FeatureFlagDto>> GetFlagsAsync(Guid projectId, string environment, CancellationToken cancellationToken);
    
    /// <summary>
    /// Invalidates the cache when a flag is updated/created/deleted.
    /// </summary>
    Task InvalidateProjectCacheAsync(Guid projectId, string environment, CancellationToken cancellationToken);
}
