using Microsoft.Extensions.Configuration;
using RatBot.Domain.Entities;
using RatBot.Infrastructure.Data;

namespace RatBot.Infrastructure.Services;

/// <summary>
/// Provides guild configuration persistence operations.
/// </summary>
public sealed class GuildConfigService
{
    private readonly BotDbContext _dbContext;
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuildConfigService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="config">The application configuration.</param>
    public GuildConfigService(BotDbContext dbContext, IConfiguration config)
    {
        _dbContext = dbContext;
        _config = config;
    }

    /// <summary>
    /// Gets the guild configuration if it exists; otherwise creates it with defaults.
    /// </summary>
    /// <param name="guildId">The guild identifier.</param>
    /// <returns>The existing or newly created guild configuration.</returns>
    public async Task<GuildConfig> GetOrCreateAsync(ulong guildId)
    {
        GuildConfig? foundConfig = await _dbContext.GuildConfigs.FindAsync(guildId);

        if (foundConfig is not null)
            return foundConfig;

        GuildConfig guildConfig = new GuildConfig { GuildId = guildId, Prefix = _config["Bot:Prefix"] ?? "?" };
        _dbContext.GuildConfigs.Add(guildConfig);
        await _dbContext.SaveChangesAsync();
        return guildConfig;
    }

    /// <summary>
    /// Sets the command prefix for a guild.
    /// </summary>
    /// <param name="guildId">The guild identifier.</param>
    /// <param name="prefix">The new prefix value.</param>
    /// <returns>A task that completes when changes are persisted.</returns>
    public async Task SetPrefixAsync(ulong guildId, string prefix)
    {
        GuildConfig guildConfig = await GetOrCreateAsync(guildId);
        guildConfig.Prefix = prefix;
        await _dbContext.SaveChangesAsync();
    }
}
