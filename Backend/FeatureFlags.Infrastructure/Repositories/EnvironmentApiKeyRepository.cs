using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Domain.Entities;
using FeatureFlags.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Infrastructure.Repositories;

public class EnvironmentApiKeyRepository : IEnvironmentApiKeyRepository
{
    private readonly ApplicationDbContext _context;

    public EnvironmentApiKeyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnvironmentApiKey?> GetByIdAsync(Guid id, Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == id && k.ProjectId == projectId, cancellationToken);
    }

    public async Task<EnvironmentApiKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .FirstOrDefaultAsync(k => k.Key == key, cancellationToken);
    }

    public async Task<IEnumerable<EnvironmentApiKey>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.ApiKeys
            .Where(k => k.ProjectId == projectId)
            .OrderByDescending(k => k.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EnvironmentApiKey apiKey, CancellationToken cancellationToken = default)
    {
        await _context.ApiKeys.AddAsync(apiKey, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(EnvironmentApiKey apiKey, CancellationToken cancellationToken = default)
    {
        _context.ApiKeys.Remove(apiKey);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
