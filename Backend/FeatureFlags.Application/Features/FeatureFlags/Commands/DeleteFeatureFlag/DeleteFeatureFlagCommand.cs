using FeatureFlags.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace FeatureFlags.Application.Features.FeatureFlags.Commands.DeleteFeatureFlag;

public record DeleteFeatureFlagCommand(Guid Id, Guid ProjectId) : IRequest;

public class DeleteFeatureFlagCommandValidator : AbstractValidator<DeleteFeatureFlagCommand>
{
    public DeleteFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}

public class DeleteFeatureFlagCommandHandler : IRequestHandler<DeleteFeatureFlagCommand>
{
    private readonly IFeatureFlagRepository _featureFlagRepository;

    public DeleteFeatureFlagCommandHandler(IFeatureFlagRepository featureFlagRepository)
    {
        _featureFlagRepository = featureFlagRepository;
    }

    public async Task Handle(DeleteFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var featureFlag = await _featureFlagRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (featureFlag == null || featureFlag.ProjectId != request.ProjectId)
        {
            throw new KeyNotFoundException($"Feature flag with Id {request.Id} was not found for this project.");
        }

        await _featureFlagRepository.DeleteAsync(featureFlag, cancellationToken);
    }
}
