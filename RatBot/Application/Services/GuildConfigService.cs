using RatBot.Infrastructure.Data;

namespace RatBot.Application.Services;

public sealed class GuildConfigService
{
    private readonly BotDbContext _dbContext;
    private readonly IConfiguration _config;

    public GuildConfigService(BotDbContext dbContext, IConfiguration config)
    {
        _dbContext = dbContext;
        _config = config;
    }

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

    public async Task SetPrefixAsync(ulong guildId, string prefix)
    {
        GuildConfig guildConfig = await GetOrCreateAsync(guildId);
        guildConfig.Prefix = prefix;
        await _dbContext.SaveChangesAsync();
    }
}
