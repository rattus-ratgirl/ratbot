using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RatBot.Infrastructure.Persistence.Configurations;

public sealed class MetaSuggestionConfiguration : IEntityTypeConfiguration<MetaSuggestion>
{
    public void Configure(EntityTypeBuilder<MetaSuggestion> builder)
    {
        builder.ToTable("MetaSuggestions");

        builder.Property(x => x.Title).HasMaxLength(75);
        builder.Property(x => x.Summary).HasMaxLength(1500);
        builder.Property(x => x.Motivation).HasMaxLength(1950);
        builder.Property(x => x.Specification).HasMaxLength(1950);

        builder.HasIndex(x => x.GuildId);
        builder.HasIndex(x => new { x.GuildId, x.State });
        builder.HasIndex(x => x.ThreadChannelId).IsUnique();
    }
}