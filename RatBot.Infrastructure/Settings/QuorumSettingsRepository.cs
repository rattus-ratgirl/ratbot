using RatBot.Application.Features.Quorum;
using RatBot.Infrastructure.Data;

namespace RatBot.Infrastructure.Settings;

public sealed class QuorumSettingsRepository(BotDbContext dbContext) : IQuorumSettingsRepository
{
    private static Domain.Features.Quorum.QuorumSettings Map(QuorumSettings entity) =>
        new Domain.Features.Quorum.QuorumSettings(
            entity.GuildId,
            entity.TargetType,
            entity.TargetId,
            entity.Roles.Select(role => role.RoleId).ToArray(),
            entity.QuorumProportion);

    private static QuorumSettings Map(Domain.Features.Quorum.QuorumSettings config) =>
        new QuorumSettings
        {
            GuildId = config.GuildId,
            TargetType = config.TargetType,
            TargetId = config.TargetId,
            QuorumProportion = config.QuorumProportion,
            Roles = config.RoleIds.Select(roleId => new Role(roleId)).ToList()
        };
    public async Task<Domain.Features.Quorum.QuorumSettings?> GetAsync(
        ulong guildId,
        QuorumSettingsType targetType,
        ulong targetId,
        CancellationToken ct = default)
    {
        QuorumSettings? entity = await dbContext.Set<QuorumSettings>()
            .AsNoTracking()
            .Include(config => config.Roles)
            .SingleOrDefaultAsync(
                config => config.GuildId == guildId && config.TargetType == targetType && config.TargetId == targetId,
                ct);

        return entity is null
            ? null
            : Map(entity);
    }

    public async Task<bool> UpsertAsync(Domain.Features.Quorum.QuorumSettings config, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        QuorumSettings? entity = await dbContext.Set<QuorumSettings>()
            .Include(existing => existing.Roles)
            .SingleOrDefaultAsync(
                existing => existing.GuildId == config.GuildId && existing.TargetType == config.TargetType &&
                            existing.TargetId == config.TargetId,
                ct);

        if (entity is null)
        {
            dbContext.Add(Map(config));
            await dbContext.SaveChangesAsync(ct);
            return true;
        }

        entity = entity with { QuorumProportion = config.QuorumProportion };
        dbContext.RemoveRange(entity.Roles);

        entity = entity with { Roles = config.RoleIds.Select(roleId => new Role(roleId)).ToList() };
        dbContext.Update(entity);

        await dbContext.SaveChangesAsync(ct);
        return false;
    }

    public async Task<bool> DeleteAsync(
        ulong guildId,
        QuorumSettingsType targetType,
        ulong targetId,
        CancellationToken ct = default)
    {
        QuorumSettings? entity = await dbContext.Set<QuorumSettings>()
            .Include(existing => existing.Roles)
            .SingleOrDefaultAsync(
                existing => existing.GuildId == guildId && existing.TargetType == targetType &&
                            existing.TargetId == targetId,
                ct);

        if (entity is null)
            return false;

        dbContext.Remove(entity);
        await dbContext.SaveChangesAsync(ct);
        return true;
    }
}
