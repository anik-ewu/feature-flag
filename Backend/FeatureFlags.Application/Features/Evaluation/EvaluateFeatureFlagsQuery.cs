using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Application.Common.Utils;
using FeatureFlags.Application.Features.FeatureFlags.DTOs;
using FeatureFlags.Domain.Enums;
using FluentValidation;
using MediatR;

namespace FeatureFlags.Application.Features.Evaluation;

public record EvaluateFeatureFlagsQuery(EvaluationRequestDto Request) : IRequest<EvaluationResponseDto>;

public class EvaluateFeatureFlagsQueryValidator : AbstractValidator<EvaluateFeatureFlagsQuery>
{
    public EvaluateFeatureFlagsQueryValidator()
    {
        RuleFor(x => x.Request.TenantId).NotEmpty();
        RuleFor(x => x.Request.ProjectKey).NotEmpty();
        RuleFor(x => x.Request.Environment).NotEmpty();
        RuleFor(x => x.Request.UserId).NotEmpty(); // Required for percentage rollout
        RuleFor(x => x.Request.Attributes).NotNull();
    }
}

public class EvaluateFeatureFlagsQueryHandler : IRequestHandler<EvaluateFeatureFlagsQuery, EvaluationResponseDto>
{
    private readonly IFeatureFlagCacheService _cacheService;
    private readonly IProjectRepository _projectRepository; // To resolve ProjectKey -> ProjectId

    public EvaluateFeatureFlagsQueryHandler(
        IFeatureFlagCacheService cacheService,
        IProjectRepository projectRepository)
    {
        _cacheService = cacheService;
        _projectRepository = projectRepository;
    }

    public async Task<EvaluationResponseDto> Handle(EvaluateFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        // 1. Resolve Project (In production, this should also be cached via a ProjectCacheService)
        // Note: The user prompt asked for ProjectKey in the request payload, but the Project entity only has Name and Id.
        // For SaaS, "Name" or "Key" acts as the unique identifier for the tenant's project.
        // Assuming we resolve the ProjectId from the cache or DB based on TenantId + ProjectKey.
        var projectId = await ResolveProjectIdAsync(request.Request.TenantId, request.Request.ProjectKey, cancellationToken);
        
        if (projectId == Guid.Empty)
        {
            return new EvaluationResponseDto(new Dictionary<string, bool>());
        }

        // 2. Fetch all flags from high-performance cache
        var flags = await _cacheService.GetFlagsAsync(projectId, request.Request.Environment, cancellationToken);

        // 3. Evaluate each flag
        var result = new Dictionary<string, bool>();

        foreach (var flag in flags)
        {
            bool isEnabled = EvaluateFlag(flag, request.Request.UserId, request.Request.Attributes);
            result.Add(flag.Key, isEnabled);
        }

        // 4. Return Evaluation Result
        return new EvaluationResponseDto(result);
    }

    private bool EvaluateFlag(FeatureFlagDto flag, string userId, Dictionary<string, string> attributes)
    {
        // Global toggle takes precedence. If false, it's always false.
        if (!flag.IsEnabled)
            return false;

        // Apply Targeting Rules
        if (flag.TargetingRules.Any())
        {
            // If there are rules, the user MUST match at least ONE (OR logic) or ALL (AND logic).
            // Usually, standard FF systems use OR for multiple targeting rules of the same type, but let's do a strict match.
            bool ruleMatched = false;
            foreach (var rule in flag.TargetingRules)
            {
                if (EvaluateRule(rule, userId, attributes))
                {
                    ruleMatched = true;
                    break; 
                }
            }

            if (!ruleMatched)
                return false; // User didn't match the required rules
        }

        // Apply Rollout Percentage using Consistent Hashing
        if (flag.RolloutPercentage < 100)
        {
            if (flag.RolloutPercentage == 0)
                return false;

            var userHash = HashingUtils.GetRolloutPercentage(userId, flag.Key);
            return userHash <= flag.RolloutPercentage;
        }

        return true;
    }

    private bool EvaluateRule(TargetingRuleDto rule, string userId, Dictionary<string, string> attributes)
    {
        if (!Enum.TryParse<RuleType>(rule.Type, out var ruleType))
            return false;

        if (!Enum.TryParse<RuleOperator>(rule.Operator, out var ruleOperator))
            return false;

        string? targetValue = ruleType switch
        {
            RuleType.UserId => userId,
            RuleType.Email => attributes.GetValueOrDefault("email"),
            RuleType.Country => attributes.GetValueOrDefault("country"),
            RuleType.CustomProperty => attributes.GetValueOrDefault(rule.Type), // Requires careful property mapping in advanced setups
            _ => null
        };

        if (string.IsNullOrEmpty(targetValue))
            return false;

        return ruleOperator switch
        {
            RuleOperator.Equals => string.Equals(targetValue, rule.Value, StringComparison.OrdinalIgnoreCase),
            RuleOperator.NotEquals => !string.Equals(targetValue, rule.Value, StringComparison.OrdinalIgnoreCase),
            RuleOperator.Contains => targetValue.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
            RuleOperator.In => rule.Value.Split(',').Select(x => x.Trim()).Contains(targetValue, StringComparer.OrdinalIgnoreCase),
            RuleOperator.NotIn => !rule.Value.Split(',').Select(x => x.Trim()).Contains(targetValue, StringComparer.OrdinalIgnoreCase),
            _ => false
        };
    }

    private async Task<Guid> ResolveProjectIdAsync(Guid tenantId, string projectKey, CancellationToken cancellationToken)
    {
        // THIS MUST BE CACHED IN A REAL APP to prevent DB Hit per request.
        // For now, we simulate resolving it.
        // In the upcoming Infrastructure layer, we will implement this via a Cache-Aside pattern.
        
        // Return dummy/simulated ID for the sake of the interface building
        return Guid.NewGuid(); // Placeholder: to be replaced with actual Cache lookup
    }
}
