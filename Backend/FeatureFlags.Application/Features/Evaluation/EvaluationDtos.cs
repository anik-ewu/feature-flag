namespace FeatureFlags.Application.Features.Evaluation;

public record EvaluationRequestDto(
    string UserId,
    Dictionary<string, string> Attributes
);

public record EvaluationResponseDto(
    Dictionary<string, bool> Flags
);
