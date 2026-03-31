using RatBot.Domain.Entities;
using RatBot.Infrastructure.Data;

namespace RatBot.Infrastructure.Services;

public sealed class VirtueRoleTierConfigService
{
    private readonly BotDbContext _dbContext;

    public VirtueRoleTierConfigService(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<VirtueRoleTierConfig>> ListAsync(ulong guildId)
    {
        return _dbContext
            .VirtueRoleTierConfigs.AsNoTracking()
            .Where(x => x.GuildId == guildId)
            .OrderBy(x => x.TierIndex)
            .ToListAsync();
    }

    public async Task UpsertAsync(ulong guildId, int tierIndex, ulong roleId, int minVirtue, int maxVirtue)
    {
        VirtueRoleTierConfig? config = await _dbContext.VirtueRoleTierConfigs.FindAsync(guildId, tierIndex);

        if (config is null)
        {
            _dbContext.VirtueRoleTierConfigs.Add(
                new VirtueRoleTierConfig
                {
                    GuildId = guildId,
                    TierIndex = tierIndex,
                    RoleId = roleId,
                    MinVirtue = minVirtue,
                    MaxVirtue = maxVirtue,
                }
            );
        }
        else
        {
            config.RoleId = roleId;
            config.MinVirtue = minVirtue;
            config.MaxVirtue = maxVirtue;
        }

        await _dbContext.SaveChangesAsync();
    }
}
