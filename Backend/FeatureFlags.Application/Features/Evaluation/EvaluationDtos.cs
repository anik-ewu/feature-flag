namespace FeatureFlags.Application.Features.Evaluation;

public record EvaluationRequestDto(
    Guid TenantId,
    string ProjectKey, // Usually passed as string in APIs, we'll need to resolve ProjectId via cache
    string Environment,
    string UserId,
    Dictionary<string, string> Attributes
);

public record EvaluationResponseDto(
    Dictionary<string, bool> Flags
);
