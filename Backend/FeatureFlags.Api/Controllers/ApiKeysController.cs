using FeatureFlags.Application.Features.ApiKeys.Commands.CreateApiKey;
using FeatureFlags.Application.Features.ApiKeys.Commands.DeleteApiKey;
using FeatureFlags.Application.Features.ApiKeys.DTOs;
using FeatureFlags.Application.Features.ApiKeys.Queries.GetApiKeys;
using FeatureFlags.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/apikeys")]
public class ApiKeysController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApiKeysController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApiKeyDto>>> GetApiKeys(Guid projectId)
    {
        var keys = await _mediator.Send(new GetApiKeysQuery(projectId));
        return Ok(keys);
    }

    public record CreateApiKeyRequest(EnvironmentType Environment, string Name);

    [HttpPost]
    public async Task<ActionResult<ApiKeyDto>> CreateApiKey(Guid projectId, [FromBody] CreateApiKeyRequest request)
    {
        var key = await _mediator.Send(new CreateApiKeyCommand(projectId, request.Environment, request.Name));
        return CreatedAtAction(nameof(GetApiKeys), new { projectId }, key);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApiKey(Guid projectId, Guid id)
    {
        await _mediator.Send(new DeleteApiKeyCommand(projectId, id));
        return NoContent();
    }
}
