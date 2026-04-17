using RatBot.Domain.Features.Moderation;
using RatBot.Domain.Primitives;

namespace RatBot.Application.Features.Moderation.Interfaces;

public interface IModerationService
{
    Task<ErrorOr<AutobannedUser>> RegisterAutobanAsync(
        GuildSnowflake guildId,
        UserSnowflake bannedUser,
        UserSnowflake moderator,
        CancellationToken ct = default);

    Task<AutobannedUser?> GetAutobanAsync(
        GuildSnowflake guildId,
        UserSnowflake userId,
        CancellationToken ct = default);
}
