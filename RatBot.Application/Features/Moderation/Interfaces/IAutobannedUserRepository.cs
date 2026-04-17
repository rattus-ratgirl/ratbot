using RatBot.Domain.Features.Moderation;
using RatBot.Domain.Primitives;

namespace RatBot.Application.Features.Moderation.Interfaces;

public interface IAutobannedUserRepository
{
    Task<AutobannedUser?> GetAsync(GuildSnowflake guildId, UserSnowflake userId, CancellationToken ct = default);
    Task AddAsync(AutobannedUser user, CancellationToken ct = default);
}
