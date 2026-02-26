using FeatureFlags.Domain.Common;

namespace FeatureFlags.Domain.Entities;

public class Project : BaseEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }

    private Project(Guid tenantId, string name)
    {
        TenantId = tenantId;
        Name = name;
    }

    public static Project Create(Guid tenantId, string name)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty.", nameof(name));

        return new Project(tenantId, name);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty.", nameof(name));

        Name = name;
        UpdateTimestamp();
    }
}
