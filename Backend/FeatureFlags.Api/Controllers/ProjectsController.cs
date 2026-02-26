using FeatureFlags.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;

    public ProjectsController(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId)
    {
        var projects = await _projectRepository.GetByTenantAsync(tenantId);
        return Ok(projects);
    }
}
