using FeatureFlags.Domain.Common;

namespace FeatureFlags.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; }
    public string ApiKey { get; private set; }
    public bool IsActive { get; private set; }

    private Tenant(string name, string apiKey)
    {
        Name = name;
        ApiKey = apiKey;
        IsActive = true;
    }

    public static Tenant Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty.", nameof(name));

        var apiKey = GenerateApiKey();
        return new Tenant(name, apiKey);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty.", nameof(name));

        Name = name;
        UpdateTimestamp();
    }

    public void RegenerateApiKey()
    {
        ApiKey = GenerateApiKey();
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    private static string GenerateApiKey()
    {
        return $"sk_live_{Guid.NewGuid():N}";
    }
}
