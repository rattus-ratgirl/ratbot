using RatBot.Infrastructure.Settings;
using QuorumSettings = RatBot.Infrastructure.Settings.QuorumSettings;

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

    public DbSet<QuorumSettings> QuorumSettings =>
        Set<QuorumSettings>();

    public DbSet<Role> QuorumSettingsRoles =>
        Set<Role>();

    public DbSet<EmojiUsageCount> EmojiUsageCounts => Set<EmojiUsageCount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BotDbContext).Assembly);
    }
}