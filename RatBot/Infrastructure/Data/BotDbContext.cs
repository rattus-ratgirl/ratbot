namespace RatBot.Infrastructure.Data;

public sealed class BotDbContext : DbContext
{
    public DbSet<GuildConfig> GuildConfigs => Set<GuildConfig>();

    public BotDbContext(DbContextOptions<BotDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildConfig>(b =>
        {
            b.HasKey(x => x.GuildId);
            b.Property(x => x.Prefix).HasMaxLength(2);
        });
    }
}
