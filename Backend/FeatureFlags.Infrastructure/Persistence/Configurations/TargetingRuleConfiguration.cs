using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FeatureFlags.Domain.Entities;

namespace FeatureFlags.Infrastructure.Persistence.Configurations;

public class TargetingRuleConfiguration : IEntityTypeConfiguration<TargetingRule>
{
    public void Configure(EntityTypeBuilder<TargetingRule> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne<FeatureFlag>()
            .WithMany(f => f.TargetingRules)
            .HasForeignKey(t => t.FeatureFlagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
