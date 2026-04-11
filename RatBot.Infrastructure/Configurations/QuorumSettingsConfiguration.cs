using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuorumSettings = RatBot.Infrastructure.Settings.QuorumSettings;

namespace RatBot.Infrastructure.Configurations;

public sealed class QuorumSettingsConfiguration : IEntityTypeConfiguration<QuorumSettings>
{
    public void Configure(EntityTypeBuilder<QuorumSettings> builder)
    {
        builder.ToTable("QuorumSettings");
        builder.HasKey(x => new { x.GuildId, x.TargetType, x.TargetId });

        builder.Property(x => x.GuildId).HasColumnType("bigint").HasConversion<long>();
        builder.Property(x => x.TargetType).HasColumnType("integer");
        builder.Property(x => x.TargetId).HasColumnType("bigint").HasConversion<long>();
        builder.Property(x => x.QuorumProportion).HasColumnType("double precision").HasPrecision(6, 4);

        builder.HasIndex(x => x.GuildId);
        builder.HasIndex(x => new { x.GuildId, x.TargetType });

        builder.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey("GuildId", "TargetType", "TargetId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
