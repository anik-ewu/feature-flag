using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FeatureFlags.Application.Features.Projects.Commands.CreateProject;

public record CreateProjectCommand(string Name, Guid TenantId) : IRequest<Guid>;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;

    public CreateProjectCommandHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = Project.Create(request.TenantId, request.Name);
        
        await _projectRepository.AddAsync(project, cancellationToken);

        return project.Id; // Return new Project ID
    }
}
