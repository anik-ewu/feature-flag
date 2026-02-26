using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Domain.Entities;
using FeatureFlags.Domain.Enums;
using FluentValidation;
using MediatR;

namespace FeatureFlags.Application.Features.FeatureFlags.Commands.CreateFeatureFlag;

public record CreateFeatureFlagCommand(
    Guid ProjectId,
    string Key,
    string Description,
    EnvironmentType Environment) : IRequest<Guid>;

public class CreateFeatureFlagCommandValidator : AbstractValidator<CreateFeatureFlagCommand>
{
    public CreateFeatureFlagCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Key).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Environment).IsInEnum();
    }
}

public class CreateFeatureFlagCommandHandler : IRequestHandler<CreateFeatureFlagCommand, Guid>
{
    private readonly IFeatureFlagRepository _featureFlagRepository;
    private readonly IProjectRepository _projectRepository;

    public CreateFeatureFlagCommandHandler(
        IFeatureFlagRepository featureFlagRepository,
        IProjectRepository projectRepository)
    {
        _featureFlagRepository = featureFlagRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Guid> Handle(CreateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with Id {request.ProjectId} was not found.");
        }

        var existingFlag = await _featureFlagRepository.GetByKeyAsync(request.ProjectId, request.Key, cancellationToken);
        if (existingFlag != null)
        {
            throw new InvalidOperationException($"A feature flag with the key '{request.Key}' already exists in this project.");
        }

        var featureFlag = FeatureFlag.Create(
            request.ProjectId,
            request.Key,
            request.Description ?? string.Empty,
            request.Environment);

        await _featureFlagRepository.AddAsync(featureFlag, cancellationToken);

        return featureFlag.Id;
    }
}
