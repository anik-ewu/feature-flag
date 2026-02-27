using FeatureFlags.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

using FeatureFlags.Domain.Enums;

namespace FeatureFlags.Application.Features.FeatureFlags.Commands.UpdateFeatureFlag;

public record TargetingRuleDto(RuleType Type, RuleOperator Operator, string Value);

public record UpdateFeatureFlagCommand(
    Guid Id,
    Guid ProjectId, // Required to ensure Tenant Isolation (Verify flag belongs to this project/tenant context)
    string Description,
    bool IsEnabled,
    int RolloutPercentage,
    List<TargetingRuleDto>? TargetingRules = null) : IRequest;

public class UpdateFeatureFlagCommandValidator : AbstractValidator<UpdateFeatureFlagCommand>
{
    public UpdateFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.RolloutPercentage).InclusiveBetween(0, 100);
    }
}

public class UpdateFeatureFlagCommandHandler : IRequestHandler<UpdateFeatureFlagCommand>
{
    private readonly IFeatureFlagRepository _featureFlagRepository;

    public UpdateFeatureFlagCommandHandler(IFeatureFlagRepository featureFlagRepository)
    {
        _featureFlagRepository = featureFlagRepository;
    }

    public async Task Handle(UpdateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var featureFlag = await _featureFlagRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (featureFlag == null || featureFlag.ProjectId != request.ProjectId)
        {
            throw new KeyNotFoundException($"Feature flag with Id {request.Id} was not found for this project.");
        }

        featureFlag.UpdateDetails(request.Description ?? string.Empty);
        featureFlag.Toggle(request.IsEnabled);
        featureFlag.UpdateRolloutPercentage(request.RolloutPercentage);

        // Synchronize Targeting Rules
        var rulesToKeep = new HashSet<Guid>();
        
        if (request.TargetingRules != null)
        {
            foreach (var ruleDto in request.TargetingRules)
            {
                var existingRule = featureFlag.TargetingRules
                    .FirstOrDefault(r => r.Type == ruleDto.Type && r.Operator == ruleDto.Operator && r.Value == ruleDto.Value);

                if (existingRule != null)
                {
                    rulesToKeep.Add(existingRule.Id);
                }
                else
                {
                    featureFlag.AddTargetingRule(ruleDto.Type, ruleDto.Operator, ruleDto.Value);
                    // The freshly added rule is the last one in the list
                    var newRule = featureFlag.TargetingRules.Last();
                    rulesToKeep.Add(newRule.Id);
                }
            }
        }

        var rulesToRemove = featureFlag.TargetingRules
            .Where(r => !rulesToKeep.Contains(r.Id))
            .Select(r => r.Id)
            .ToList();

        foreach (var ruleId in rulesToRemove)
        {
            featureFlag.RemoveTargetingRule(ruleId);
        }

        await _featureFlagRepository.UpdateAsync(featureFlag, cancellationToken);
    }
}
