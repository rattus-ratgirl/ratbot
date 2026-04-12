using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RatBot.Infrastructure.Settings;

namespace RatBot.Infrastructure.Configurations;

public sealed class QuorumSettingsRoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("QuorumSettingsRoles");

        // Shadow properties for composite key from parent
        builder.Property<ulong>("GuildId").HasColumnType("bigint").HasConversion<long>();
        builder.Property<QuorumSettingsType>("TargetType").HasColumnType("integer");
        builder.Property<ulong>("TargetId").HasColumnType("bigint").HasConversion<long>();

        builder.Property(x => x.RoleId).HasColumnType("bigint").HasConversion<long>();

        builder.HasKey("GuildId", "TargetType", "TargetId", "RoleId");
    }
}