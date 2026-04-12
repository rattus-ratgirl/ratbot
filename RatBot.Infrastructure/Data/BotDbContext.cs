using RatBot.Infrastructure.Settings;

namespace RatBot.Infrastructure.Data;

/// <summary>
///     Entity Framework Core database context for RatBot persistence.
/// </summary>
public sealed class BotDbContext : DbContext, IBotDataContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BotDbContext" /> class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
    {
    }

    public DbSet<QuorumSettingsEntity> QuorumSettings =>
        Set<QuorumSettingsEntity>();

    public DbSet<RoleEntity> QuorumSettingsRoles =>
        Set<RoleEntity>();

    public DbSet<EmojiUsageCount> EmojiUsageCounts => Set<EmojiUsageCount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BotDbContext).Assembly);
}