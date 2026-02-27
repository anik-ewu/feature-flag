using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Application.Features.ApiKeys.DTOs;
using FeatureFlags.Domain.Entities;
using FeatureFlags.Domain.Enums;
using FluentValidation;
using MediatR;

namespace FeatureFlags.Application.Features.ApiKeys.Commands.CreateApiKey;

public record CreateApiKeyCommand(Guid ProjectId, EnvironmentType Environment, string Name) : IRequest<ApiKeyDto>;

public class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(v => v.ProjectId).NotEmpty();
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Environment).IsInEnum();
    }
}

public class CreateApiKeyCommandHandler : IRequestHandler<CreateApiKeyCommand, ApiKeyDto>
{
    private readonly IEnvironmentApiKeyRepository _apiKeyRepository;
    private readonly IProjectRepository _projectRepository;

    public CreateApiKeyCommandHandler(
        IEnvironmentApiKeyRepository apiKeyRepository,
        IProjectRepository projectRepository)
    {
        _apiKeyRepository = apiKeyRepository;
        _projectRepository = projectRepository;
    }

    public async Task<ApiKeyDto> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {request.ProjectId} was not found.");
        }

        var apiKey = EnvironmentApiKey.Create(request.ProjectId, request.Environment, request.Name);

        await _apiKeyRepository.AddAsync(apiKey, cancellationToken);

        return new ApiKeyDto(
            apiKey.Id,
            apiKey.ProjectId,
            apiKey.Environment.ToString(),
            apiKey.Name,
            apiKey.Key,
            apiKey.CreatedAtUtc
        );
    }
}
