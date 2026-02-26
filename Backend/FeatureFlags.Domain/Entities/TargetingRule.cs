using FeatureFlags.Domain.Common;
using FeatureFlags.Domain.Enums;

namespace FeatureFlags.Domain.Entities;

public class TargetingRule : BaseEntity
{
    public Guid FeatureFlagId { get; private set; }
    public RuleType Type { get; private set; }
    public RuleOperator Operator { get; private set; }
    public string Value { get; private set; }

    private TargetingRule(Guid featureFlagId, RuleType type, RuleOperator @operator, string value)
    {
        FeatureFlagId = featureFlagId;
        Type = type;
        Operator = @operator;
        Value = value;
    }

    internal static TargetingRule Create(Guid featureFlagId, RuleType type, RuleOperator @operator, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Rule value cannot be empty.", nameof(value));

        return new TargetingRule(featureFlagId, type, @operator, value);
    }

    internal void Update(RuleType type, RuleOperator @operator, string value)
    {
         if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Rule value cannot be empty.", nameof(value));

        Type = type;
        Operator = @operator;
        Value = value;
        UpdateTimestamp();
    }
}
