using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FeatureFlags.Domain.Entities;

namespace FeatureFlags.Infrastructure.Persistence.Configurations;

public class EnvironmentApiKeyConfiguration : IEntityTypeConfiguration<EnvironmentApiKey>
{
    public void Configure(EntityTypeBuilder<EnvironmentApiKey> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Environment)
            .HasConversion<string>()
            .IsRequired();

        // Ensure keys are unique across the system
        builder.HasIndex(e => e.Key).IsUnique();

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
