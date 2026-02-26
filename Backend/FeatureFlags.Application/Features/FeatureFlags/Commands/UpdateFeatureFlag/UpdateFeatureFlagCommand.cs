using FeatureFlags.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace FeatureFlags.Application.Features.FeatureFlags.Commands.UpdateFeatureFlag;

public record UpdateFeatureFlagCommand(
    Guid Id,
    Guid ProjectId, // Required to ensure Tenant Isolation (Verify flag belongs to this project/tenant context)
    string Description,
    bool IsEnabled,
    int RolloutPercentage) : IRequest;

public class UpdateFeatureFlagCommandValidator : AbstractValidator<UpdateFeatureFlagCommand>
{
    public UpdateFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty();
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

        await _featureFlagRepository.UpdateAsync(featureFlag, cancellationToken);
    }
}
