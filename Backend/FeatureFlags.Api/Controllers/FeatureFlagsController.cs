using FeatureFlags.Application.Features.FeatureFlags.Commands.CreateFeatureFlag;
using FeatureFlags.Application.Features.FeatureFlags.Commands.DeleteFeatureFlag;
using FeatureFlags.Application.Features.FeatureFlags.Commands.UpdateFeatureFlag;
using FeatureFlags.Application.Features.FeatureFlags.Queries.GetFeatureFlagById;
using FeatureFlags.Application.Features.FeatureFlags.Queries.GetFeatureFlagsByProject;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeatureFlagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeatureFlagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetByProject(Guid projectId)
    {
        var query = new GetFeatureFlagsByProjectQuery(projectId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid projectId)
    {
        var query = new GetFeatureFlagByIdQuery(id, projectId);
        var result = await _mediator.Send(query);
        
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeatureFlagCommand command)
    {
        // Enforce implicit tenant context eventually, right now trusting command
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id, projectId = command.ProjectId }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFeatureFlagCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid projectId)
    {
        var command = new DeleteFeatureFlagCommand(id, projectId);
        await _mediator.Send(command);
        return NoContent();
    }
}
