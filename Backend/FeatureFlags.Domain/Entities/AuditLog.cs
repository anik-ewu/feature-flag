using FeatureFlags.Domain.Common;
using FeatureFlags.Domain.Enums;

namespace FeatureFlags.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public AuditAction Action { get; private set; }
    public string ChangedBy { get; private set; }

    // Stores serialized old/new state if needed
    public string? Details { get; private set; }

    private AuditLog(
        string entityType, 
        Guid entityId, 
        AuditAction action, 
        string changedBy,
        string? details)
    {
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        ChangedBy = changedBy;
        Details = details;
    }

    public static AuditLog Create(
        string entityType, 
        Guid entityId, 
        AuditAction action, 
        string changedBy,
        string? details = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType cannot be empty.", nameof(entityType));

        if (string.IsNullOrWhiteSpace(changedBy))
            throw new ArgumentException("ChangedBy cannot be empty.", nameof(changedBy));

        return new AuditLog(entityType, entityId, action, changedBy, details);
    }
}
