namespace FeatureFlags.Application.Features.FeatureFlags.DTOs;

public record FeatureFlagDto(
    Guid Id,
    Guid ProjectId,
    string Key,
    string Description,
    bool IsEnabled,
    int RolloutPercentage,
    string Environment,
    IEnumerable<TargetingRuleDto> TargetingRules,
    DateTime? UpdatedAtUtc
);

public record TargetingRuleDto(
    Guid Id,
    string Type,
    string Operator,
    string Value
);
