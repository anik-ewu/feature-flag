using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Infrastructure.Persistence.Repositories;

public class FeatureFlagRepository : IFeatureFlagRepository
{
    private readonly ApplicationDbContext _context;

    public FeatureFlagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FeatureFlag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.TargetingRules)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<FeatureFlag?> GetByKeyAsync(Guid projectId, string key, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.TargetingRules)
            .FirstOrDefaultAsync(f => f.ProjectId == projectId && f.Key == key, cancellationToken);
    }

    public async Task<IEnumerable<FeatureFlag>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.TargetingRules)
            .Where(f => f.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        await _context.FeatureFlags.AddAsync(featureFlag, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        _context.FeatureFlags.Update(featureFlag);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        _context.FeatureFlags.Remove(featureFlag);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
