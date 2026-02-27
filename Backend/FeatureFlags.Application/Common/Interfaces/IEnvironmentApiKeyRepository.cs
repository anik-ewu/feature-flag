using FeatureFlags.Domain.Entities;
using FeatureFlags.Domain.Enums;

namespace FeatureFlags.Application.Common.Interfaces;

public interface IEnvironmentApiKeyRepository
{
    Task<EnvironmentApiKey?> GetByIdAsync(Guid id, Guid projectId, CancellationToken cancellationToken = default);
    Task<EnvironmentApiKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<EnvironmentApiKey>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task AddAsync(EnvironmentApiKey apiKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(EnvironmentApiKey apiKey, CancellationToken cancellationToken = default);
}
