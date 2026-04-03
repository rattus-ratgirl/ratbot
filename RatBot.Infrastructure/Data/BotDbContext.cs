using RatBot.Domain.Entities;

namespace RatBot.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for RatBot persistence.
/// </summary>
public sealed class BotDbContext : DbContext
{
    /// <summary>
    /// Gets the guild configuration set.
    /// </summary>
    public DbSet<GuildConfig> GuildConfigs => Set<GuildConfig>();

    /// <summary>
    /// Gets the quorum scope configuration set.
    /// </summary>
    public DbSet<QuorumScopeConfig> QuorumScopeConfigs => Set<QuorumScopeConfig>();

    /// <summary>
    /// Gets the user virtue set.
    /// </summary>
    public DbSet<UserVirtue> UserVirtues => Set<UserVirtue>();

    /// <summary>
    /// Gets the emoji usage set.
    /// </summary>
    public DbSet<EmojiUsageCount> EmojiUsageCounts => Set<EmojiUsageCount>();

    /// <summary>
    /// Initializes a new instance of the <see cref="BotDbContext"/> class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    public BotDbContext(DbContextOptions<BotDbContext> options)
        : base(options) { }

    /// <summary>
    /// Configures entity mappings.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildConfig>(b =>
        {
            b.HasKey(x => x.GuildId);
            b.Property(x => x.Prefix).HasMaxLength(2);
        });

        modelBuilder.Entity<QuorumScopeConfig>(b =>
        {
            b.HasKey(x => new
            {
                x.GuildId,
                x.ScopeType,
                x.ScopeId,
            });

            b.Property(x => x.GuildId).HasColumnType("bigint unsigned");
            b.Property(x => x.ScopeId).HasColumnType("bigint unsigned");
            b.Property(x => x.RoleId).HasColumnType("bigint unsigned");
            b.Property(x => x.ScopeType).HasConversion<int>();
            b.Property(x => x.QuorumProportion).HasPrecision(6, 4);

            b.HasIndex(x => x.GuildId);
            b.HasIndex(x => new { x.GuildId, x.ScopeType });
        });

        modelBuilder.Entity<UserVirtue>(b =>
        {
            b.HasKey(x => x.UserId);

            b.Property(x => x.UserId).ValueGeneratedNever().HasColumnType("bigint unsigned");
            b.Property(x => x.Virtue).HasColumnName("Score").HasColumnType("int");
        });

        modelBuilder.Entity<EmojiUsageCount>(b =>
        {
            b.HasKey(x => x.EmojiId);

            b.Property(x => x.EmojiId).HasMaxLength(128);
            b.Property(x => x.UsageCount).HasColumnType("int");
        });
    }
}
