using FeatureFlags.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace FeatureFlags.Application.Features.ApiKeys.Commands.DeleteApiKey;

public record DeleteApiKeyCommand(Guid ProjectId, Guid Id) : IRequest;

public class DeleteApiKeyCommandValidator : AbstractValidator<DeleteApiKeyCommand>
{
    public DeleteApiKeyCommandValidator()
    {
        RuleFor(v => v.ProjectId).NotEmpty();
        RuleFor(v => v.Id).NotEmpty();
    }
}

public class DeleteApiKeyCommandHandler : IRequestHandler<DeleteApiKeyCommand>
{
    private readonly IEnvironmentApiKeyRepository _apiKeyRepository;

    public DeleteApiKeyCommandHandler(IEnvironmentApiKeyRepository apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task Handle(DeleteApiKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(request.Id, request.ProjectId, cancellationToken);
            
        if (apiKey == null)
            throw new KeyNotFoundException($"API Key with ID {request.Id} not found in this project.");

        await _apiKeyRepository.DeleteAsync(apiKey, cancellationToken);
    }
}
