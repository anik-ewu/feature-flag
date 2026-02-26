using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FeatureFlags.Domain.Entities;

namespace FeatureFlags.Infrastructure.Persistence.Configurations;

public class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Key)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(f => f.Description)
            .HasMaxLength(500);

        builder.OwnsOne(f => f.RolloutPercentage, p =>
        {
            p.Property(r => r.Value).HasColumnName("RolloutPercentage");
        });

        // Ensure key is unique per project
        builder.HasIndex(f => new { f.ProjectId, f.Key }).IsUnique();

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(FeatureFlag.TargetingRules))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
