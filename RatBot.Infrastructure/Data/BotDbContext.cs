namespace RatBot.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for RatBot persistence.
/// </summary>
public sealed class BotDbContext : DbContext, IBotDataContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BotDbContext"/> class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    public BotDbContext(DbContextOptions<BotDbContext> options)
        : base(options) { }

    public DbSet<EmojiUsageCount> EmojiUsageCounts => Set<EmojiUsageCount>();

    public DbSet<Configuration.Quorum.QuorumScopeConfigEntity> QuorumScopeConfigs =>
        Set<Configuration.Quorum.QuorumScopeConfigEntity>();

    public DbSet<Configuration.Quorum.QuorumScopeConfigRoleEntity> QuorumScopeConfigRoles =>
        Set<Configuration.Quorum.QuorumScopeConfigRoleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmojiUsageCount>(b =>
        {
            b.HasKey(x => x.EmojiId);

            b.Property(x => x.EmojiId).HasMaxLength(128);
            b.Property(x => x.UsageCount).HasColumnType("int");
        });

        modelBuilder.Entity<Configuration.Quorum.QuorumScopeConfigEntity>(b =>
        {
            b.ToTable("QuorumScopeConfigs");
            b.HasKey(x => new { x.GuildId, x.ScopeType, x.ScopeId });

            b.Property(x => x.GuildId).HasColumnType("numeric(20,0)");
            b.Property(x => x.ScopeType).HasColumnType("integer");
            b.Property(x => x.ScopeId).HasColumnType("numeric(20,0)");
            b.Property(x => x.QuorumProportion).HasColumnType("double precision").HasPrecision(6, 4);

            b.HasIndex(x => x.GuildId);
            b.HasIndex(x => new { x.GuildId, x.ScopeType });

            b.HasMany(x => x.Roles)
                .WithOne(x => x.Config)
                .HasForeignKey(x => new { x.GuildId, x.ScopeType, x.ScopeId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Configuration.Quorum.QuorumScopeConfigRoleEntity>(b =>
        {
            b.ToTable("QuorumScopeConfigRoles");
            b.HasKey(x => new { x.GuildId, x.ScopeType, x.ScopeId, x.RoleId });

            b.Property(x => x.GuildId).HasColumnType("numeric(20,0)");
            b.Property(x => x.ScopeType).HasColumnType("integer");
            b.Property(x => x.ScopeId).HasColumnType("numeric(20,0)");
            b.Property(x => x.RoleId).HasColumnType("numeric(20,0)");
        });
    }
}
