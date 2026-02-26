using FeatureFlags.Domain.Entities;

namespace FeatureFlags.Application.Common.Interfaces;

public interface IFeatureFlagRepository
{
    Task<FeatureFlag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeatureFlag?> GetByKeyAsync(Guid projectId, string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeatureFlag>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task AddAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default);
    Task UpdateAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default);
    Task DeleteAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default);
}
