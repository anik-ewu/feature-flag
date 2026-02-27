using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Domain.Entities;
using FeatureFlags.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
