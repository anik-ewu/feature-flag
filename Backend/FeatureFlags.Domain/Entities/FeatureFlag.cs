using FeatureFlags.Domain.Common;
using FeatureFlags.Domain.Enums;
using FeatureFlags.Domain.ValueObjects;

namespace FeatureFlags.Domain.Entities;

public class FeatureFlag : BaseEntity
{
    public Guid ProjectId { get; private set; }
    public string Key { get; private set; }
    public string Description { get; private set; }
    public bool IsEnabled { get; private set; }
    public RolloutPercentage RolloutPercentage { get; private set; }
    public EnvironmentType Environment { get; private set; }

    private readonly List<TargetingRule> _targetingRules = new();
    public IReadOnlyCollection<TargetingRule> TargetingRules => _targetingRules.AsReadOnly();

    private FeatureFlag(
        Guid projectId, 
        string key, 
        string description, 
        EnvironmentType environment)
    {
        ProjectId = projectId;
        Key = key;
        Description = description;
        Environment = environment;
        IsEnabled = false;
        RolloutPercentage = RolloutPercentage.Create(0);
    }

    public static FeatureFlag Create(
        Guid projectId, 
        string key, 
        string description, 
        EnvironmentType environment)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Feature flag key cannot be empty.", nameof(key));

        // Format key systematically (e.g. lowercase, no spaces)
        var formattedKey = key.Trim().ToLowerInvariant().Replace(" ", "-");

        return new FeatureFlag(projectId, formattedKey, description, environment);
    }

    public void Toggle(bool isEnabled)
    {
        IsEnabled = isEnabled;
        UpdateTimestamp();
    }

    public void UpdateRolloutPercentage(int percentage)
    {
        RolloutPercentage = RolloutPercentage.Create(percentage);
        UpdateTimestamp();
    }

    public void UpdateDetails(string description)
    {
        Description = description;
        UpdateTimestamp();
    }

    public void AddTargetingRule(RuleType type, RuleOperator op, string value)
    {
        // Protect invariant: prevent exact duplicate rules
        var existingRule = _targetingRules.FirstOrDefault(r => 
            r.Type == type && r.Operator == op && r.Value == value);
            
        if (existingRule != null)
            throw new InvalidOperationException("An identical targeting rule already exists for this flag.");

        _targetingRules.Add(TargetingRule.Create(Id, type, op, value));
        UpdateTimestamp();
    }

    public void RemoveTargetingRule(Guid ruleId)
    {
        var rule = _targetingRules.FirstOrDefault(r => r.Id == ruleId);
        if (rule != null)
        {
            _targetingRules.Remove(rule);
            UpdateTimestamp();
        }
    }

    public void ClearTargetingRules()
    {
        _targetingRules.Clear();
        UpdateTimestamp();
    }
}
