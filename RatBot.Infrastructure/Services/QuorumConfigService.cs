using RatBot.Domain.Entities;
using RatBot.Domain.Enums;
using RatBot.Infrastructure.Data;

namespace RatBot.Infrastructure.Services;

public sealed class QuorumConfigService
{
    private readonly BotDbContext _dbContext;

    public QuorumConfigService(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<QuorumScopeConfig?> GetAsync(ulong guildId, QuorumScopeType scopeType, ulong scopeId)
    {
        return _dbContext.QuorumScopeConfigs.FirstOrDefaultAsync(x =>
            x.GuildId == guildId && x.ScopeType == scopeType && x.ScopeId == scopeId
        );
    }

    public Task<List<QuorumScopeConfig>> ListAsync(ulong guildId)
    {
        return _dbContext.QuorumScopeConfigs
            .Where(x => x.GuildId == guildId)
            .OrderBy(x => x.ScopeType)
            .ThenBy(x => x.ScopeId)
            .ToListAsync();
    }

    public async Task CreateAsync(ulong guildId, QuorumScopeType scopeType, ulong scopeId, ulong roleId,
        double proportion)
    {
        QuorumScopeConfig? existing = await GetAsync(guildId, scopeType, scopeId);

        if (existing is not null) return;

        _dbContext.QuorumScopeConfigs.Add(
            new QuorumScopeConfig
            {
                GuildId = guildId,
                ScopeType = scopeType,
                ScopeId = scopeId,
                RoleId = roleId,
                QuorumProportion = proportion,
            }
        );

        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(ulong guildId, QuorumScopeType scopeType, ulong scopeId, ulong roleId,
        double proportion)
    {
        QuorumScopeConfig? existing = await GetAsync(guildId, scopeType, scopeId);
        if (existing is null)
            return false;

        existing.RoleId = roleId;
        existing.QuorumProportion = proportion;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(ulong guildId, QuorumScopeType scopeType, ulong scopeId)
    {
        QuorumScopeConfig? existing = await GetAsync(guildId, scopeType, scopeId);
        if (existing is null)
            return false;

        _dbContext.QuorumScopeConfigs.Remove(existing);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
