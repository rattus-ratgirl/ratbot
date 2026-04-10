using Microsoft.EntityFrameworkCore;
using RatBot.Application.Features.Quorum;
using RatBot.Infrastructure.Data;

namespace RatBot.Infrastructure.Configuration.Quorum;

public sealed class EfQuorumConfigurationRepository(BotDbContext dbContext) : IQuorumConfigurationRepository
{
    public async Task<QuorumScopeConfig?> GetAsync(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        CancellationToken ct = default)
    {
        QuorumScopeConfigEntity? entity = await dbContext.Set<QuorumScopeConfigEntity>()
            .AsNoTracking()
            .Include(config => config.Roles)
            .SingleOrDefaultAsync(
                config => config.GuildId == guildId && config.ScopeType == scopeType && config.ScopeId == scopeId,
                ct);

        return entity is null ? null : Map(entity);
    }

    public async Task<bool> UpsertAsync(QuorumScopeConfig config, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        QuorumScopeConfigEntity? entity = await dbContext.Set<QuorumScopeConfigEntity>()
            .Include(existing => existing.Roles)
            .SingleOrDefaultAsync(
                existing =>
                    existing.GuildId == config.GuildId
                    && existing.ScopeType == config.ScopeType
                    && existing.ScopeId == config.ScopeId,
                ct);

        if (entity is null)
        {
            dbContext.Add(Map(config));
            await dbContext.SaveChangesAsync(ct);
            return true;
        }

        entity.QuorumProportion = config.QuorumProportion;
        dbContext.RemoveRange(entity.Roles);
        entity.Roles = config.RoleIds
            .Select(roleId => new QuorumScopeConfigRoleEntity
            {
                GuildId = config.GuildId,
                ScopeType = config.ScopeType,
                ScopeId = config.ScopeId,
                RoleId = roleId
            })
            .ToList();

        await dbContext.SaveChangesAsync(ct);
        return false;
    }

    public async Task<bool> DeleteAsync(
        ulong guildId,
        QuorumScopeType scopeType,
        ulong scopeId,
        CancellationToken ct = default)
    {
        QuorumScopeConfigEntity? entity = await dbContext.Set<QuorumScopeConfigEntity>()
            .Include(existing => existing.Roles)
            .SingleOrDefaultAsync(
                existing => existing.GuildId == guildId && existing.ScopeType == scopeType && existing.ScopeId == scopeId,
                ct);

        if (entity is null)
            return false;

        dbContext.Remove(entity);
        await dbContext.SaveChangesAsync(ct);
        return true;
    }

    private static QuorumScopeConfig Map(QuorumScopeConfigEntity entity) =>
        new QuorumScopeConfig(
            entity.GuildId,
            entity.ScopeType,
            entity.ScopeId,
            entity.Roles.Select(role => role.RoleId).ToArray(),
            entity.QuorumProportion);

    private static QuorumScopeConfigEntity Map(QuorumScopeConfig config) =>
        new QuorumScopeConfigEntity
        {
            GuildId = config.GuildId,
            ScopeType = config.ScopeType,
            ScopeId = config.ScopeId,
            QuorumProportion = config.QuorumProportion,
            Roles = config.RoleIds
                .Select(roleId => new QuorumScopeConfigRoleEntity
                {
                    GuildId = config.GuildId,
                    ScopeType = config.ScopeType,
                    ScopeId = config.ScopeId,
                    RoleId = roleId
                })
                .ToList()
        };
}
