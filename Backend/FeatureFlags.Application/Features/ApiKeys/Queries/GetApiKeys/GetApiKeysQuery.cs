using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Application.Features.ApiKeys.DTOs;
using MediatR;

namespace FeatureFlags.Application.Features.ApiKeys.Queries.GetApiKeys;

public record GetApiKeysQuery(Guid ProjectId) : IRequest<IEnumerable<ApiKeyDto>>;

public class GetApiKeysQueryHandler : IRequestHandler<GetApiKeysQuery, IEnumerable<ApiKeyDto>>
{
    private readonly IEnvironmentApiKeyRepository _apiKeyRepository;

    public GetApiKeysQueryHandler(IEnvironmentApiKeyRepository apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<IEnumerable<ApiKeyDto>> Handle(GetApiKeysQuery request, CancellationToken cancellationToken)
    {
        var keys = await _apiKeyRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);
        
        return keys.Select(k => new ApiKeyDto(
            k.Id,
            k.ProjectId,
            k.Environment.ToString(),
            k.Name,
            k.Key,
            k.CreatedAtUtc
        ));
    }
}
