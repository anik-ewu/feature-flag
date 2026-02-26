using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Application.Features.FeatureFlags.DTOs;
using MediatR;

namespace FeatureFlags.Application.Features.FeatureFlags.Queries.GetFeatureFlagById;

public record GetFeatureFlagByIdQuery(Guid Id, Guid ProjectId) : IRequest<FeatureFlagDto?>;

public class GetFeatureFlagByIdQueryHandler : IRequestHandler<GetFeatureFlagByIdQuery, FeatureFlagDto?>
{
    private readonly IFeatureFlagRepository _featureFlagRepository;

    public GetFeatureFlagByIdQueryHandler(IFeatureFlagRepository featureFlagRepository)
    {
        _featureFlagRepository = featureFlagRepository;
    }

    public async Task<FeatureFlagDto?> Handle(GetFeatureFlagByIdQuery request, CancellationToken cancellationToken)
    {
        var featureFlag = await _featureFlagRepository.GetByIdAsync(request.Id, cancellationToken);

        if (featureFlag == null || featureFlag.ProjectId != request.ProjectId)
        {
            return null; // Return null instead of throwing for query endpoints
        }

        return new FeatureFlagDto(
            featureFlag.Id,
            featureFlag.ProjectId,
            featureFlag.Key,
            featureFlag.Description,
            featureFlag.IsEnabled,
            featureFlag.RolloutPercentage.Value,
            featureFlag.Environment.ToString(),
            featureFlag.TargetingRules.Select(r => new TargetingRuleDto(
                r.Id,
                r.Type.ToString(),
                r.Operator.ToString(),
                r.Value
            ))
        );
    }
}
