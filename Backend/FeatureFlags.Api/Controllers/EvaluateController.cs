using FeatureFlags.Application.Features.Evaluation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EvaluateController : ControllerBase
{
    private readonly IMediator _mediator;

    public EvaluateController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<EvaluationResponseDto>> Evaluate([FromBody] EvaluationRequestDto request)
    {
        if (!Request.Headers.TryGetValue("X-Environment-Key", out var apiKeyHeader))
        {
            return Unauthorized("X-Environment-Key header is missing.");
        }

        var apiKey = apiKeyHeader.ToString();

        // Fast path query via MediatR -> Core Logic -> Cache
        var response = await _mediator.Send(new EvaluateFeatureFlagsQuery(apiKey, request));
        
        return Ok(response.Flags);
    }
}
