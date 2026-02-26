using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Application.Features.FeatureFlags.DTOs;
using MediatR;

namespace FeatureFlags.Application.Features.FeatureFlags.Queries.GetFeatureFlagsByProject;

public record GetFeatureFlagsByProjectQuery(Guid ProjectId) : IRequest<IEnumerable<FeatureFlagDto>>;

public class GetFeatureFlagsByProjectQueryHandler : IRequestHandler<GetFeatureFlagsByProjectQuery, IEnumerable<FeatureFlagDto>>
{
    private readonly IFeatureFlagRepository _featureFlagRepository;

    public GetFeatureFlagsByProjectQueryHandler(IFeatureFlagRepository featureFlagRepository)
    {
        _featureFlagRepository = featureFlagRepository;
    }

    public async Task<IEnumerable<FeatureFlagDto>> Handle(GetFeatureFlagsByProjectQuery request, CancellationToken cancellationToken)
    {
        var featureFlags = await _featureFlagRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        return featureFlags.Select(f => new FeatureFlagDto(
            f.Id,
            f.ProjectId,
            f.Key,
            f.Description,
            f.IsEnabled,
            f.RolloutPercentage.Value,
            f.Environment.ToString(),
            f.TargetingRules.Select(r => new TargetingRuleDto(
                r.Id,
                r.Type.ToString(),
                r.Operator.ToString(),
                r.Value
            ))
        ));
    }
}
