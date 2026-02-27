using FeatureFlags.Domain.Common;
using FeatureFlags.Domain.Enums;
using System.Security.Cryptography;

namespace FeatureFlags.Domain.Entities;

public class EnvironmentApiKey : BaseEntity
{
    public Guid ProjectId { get; private set; }
    public EnvironmentType Environment { get; private set; }
    public string Name { get; private set; }
    public string Key { get; private set; }

    private EnvironmentApiKey(Guid projectId, EnvironmentType environment, string name, string key)
    {
        ProjectId = projectId;
        Environment = environment;
        Name = name;
        Key = key;
    }

    public static EnvironmentApiKey Create(Guid projectId, EnvironmentType environment, string name)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("ProjectId cannot be empty.", nameof(projectId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Api Key name cannot be empty.", nameof(name));

        // Generate a secure random token format like `env_{envType}_{randomBase64}`
        var randomBytes = new byte[32];
        RandomNumberGenerator.Fill(randomBytes);
        var randomToken = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var prefix = environment switch
        {
            EnvironmentType.Development => "dev",
            EnvironmentType.Staging => "stg",
            EnvironmentType.Production => "prod",
            _ => "key"
        };

        var finalKey = $"{prefix}_{randomToken}";

        return new EnvironmentApiKey(projectId, environment, name, finalKey);
    }
}
