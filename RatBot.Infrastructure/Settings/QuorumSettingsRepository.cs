using RatBot.Application.Features.Quorum;
using RatBot.Infrastructure.Data;

namespace RatBot.Infrastructure.Settings;

public sealed class QuorumSettingsRepository(BotDbContext dbContext) : IQuorumSettingsRepository
{
    private readonly QuorumSettingsMapper _mapper = new QuorumSettingsMapper();

    public async Task<ErrorOr<QuorumSettings>> GetAsync(ulong guildId, QuorumSettingsType targetType, ulong targetId)
    {
        QuorumSettingsEntity? entity = await dbContext.Set<QuorumSettingsEntity>()
            .AsNoTracking()
            .Include(config => config.Roles)
            .SingleOrDefaultAsync(config =>
                config.GuildId == guildId && config.TargetType == targetType && config.TargetId == targetId);

        return entity is null
            ? Error.NotFound(description: "Quorum settings not found")
            : _mapper.Map(entity);
    }

    public async Task<ErrorOr<Success>> UpsertAsync(QuorumSettings config)
    {
        ArgumentNullException.ThrowIfNull(config);

        QuorumSettingsEntity? entity = await dbContext.Set<QuorumSettingsEntity>()
            .Include(existing => existing.Roles)
            .SingleOrDefaultAsync(existing =>
                existing.GuildId == config.GuildId
                && existing.TargetType == config.TargetType
                && existing.TargetId == config.TargetId);

        if (entity is null)
        {
            dbContext.Add(_mapper.Map(config));
            await dbContext.SaveChangesAsync();
            return Result.Success;
        }

        dbContext.RemoveRange(entity.Roles);

        entity = entity with
        {
            QuorumProportion = config.QuorumProportion,
            Roles = config.RoleIds.Select(roleId => new RoleEntity(roleId)).ToList()
        };

        dbContext.Update(entity);

        await dbContext.SaveChangesAsync();
        return Result.Success;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(ulong guildId, QuorumSettingsType targetType, ulong targetId)
    {
        QuorumSettingsEntity? entity = await dbContext.Set<QuorumSettingsEntity>()
            .Include(existing => existing.Roles)
            .SingleOrDefaultAsync(existing =>
                existing.GuildId == guildId && existing.TargetType == targetType && existing.TargetId == targetId);

        if (entity is null)
            return Error.NotFound(description: "Quorum settings not found");

        dbContext.Remove(entity);
        await dbContext.SaveChangesAsync();
        return Result.Deleted;
    }
}