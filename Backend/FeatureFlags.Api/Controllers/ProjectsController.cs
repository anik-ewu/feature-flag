using FeatureFlags.Application.Common.Interfaces;
using FeatureFlags.Application.Features.Projects.Commands.CreateProject;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMediator _mediator;

    public ProjectsController(IProjectRepository projectRepository, IMediator mediator)
    {
        _projectRepository = projectRepository;
        _mediator = mediator;
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId)
    {
        var projects = await _projectRepository.GetByTenantAsync(tenantId);
        return Ok(projects);
    }
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command)
    {
        var projectId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetByTenant), new { tenantId = command.TenantId }, new { id = projectId });
    }
}
