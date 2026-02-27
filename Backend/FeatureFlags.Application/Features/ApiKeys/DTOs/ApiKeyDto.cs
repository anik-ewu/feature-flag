namespace FeatureFlags.Application.Features.ApiKeys.DTOs;

public record ApiKeyDto(
    Guid Id,
    Guid ProjectId,
    string Environment,
    string Name,
    string Key,
    DateTime CreatedAt
);
