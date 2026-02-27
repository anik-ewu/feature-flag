using Microsoft.EntityFrameworkCore;
using FeatureFlags.Domain.Entities;

namespace FeatureFlags.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<TargetingRule> TargetingRules => Set<TargetingRule>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EnvironmentApiKey> ApiKeys => Set<EnvironmentApiKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
