using FeatureFlags.Domain.Entities;

namespace FeatureFlags.Application.Common.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
