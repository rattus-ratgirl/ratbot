using RatBot.Application.Features.Moderation;
using RatBot.Application.Features.Moderation.Interfaces;
using RatBot.Domain.Features.Moderation;
using RatBot.Domain.Primitives;

namespace RatBot.Application.Features.Moderation.Services;

public sealed class ModerationService(IAutobannedUserRepository autobannedUsers, ILogger logger) : IModerationService
{
    private readonly ILogger _logger = logger.ForContext<ModerationService>();

    public async Task<ErrorOr<AutobannedUser>> RegisterAutobanAsync(
        GuildSnowflake guildId,
        UserSnowflake bannedUser,
        UserSnowflake moderator,
        CancellationToken ct = default)
    {
        AutobannedUser? existing = await autobannedUsers.GetAsync(guildId, bannedUser, ct);

        if (existing is not null)
            return ModerationErrors.UserAlreadyAutobanned(bannedUser);

        AutobannedUser autobannedUser = AutobannedUser.Create(
            guildId,
            bannedUser,
            moderator,
            DateTimeOffset.UtcNow);

        await autobannedUsers.AddAsync(autobannedUser, ct);

        _logger.Information(
            "Registered user {BannedUserId} for autoban in guild {GuildId} by moderator {ModeratorId}.",
            bannedUser,
            guildId,
            moderator);

        return autobannedUser;
    }

    public Task<AutobannedUser?> GetAutobanAsync(
        GuildSnowflake guildId,
        UserSnowflake userId,
        CancellationToken ct = default) =>
        autobannedUsers.GetAsync(guildId, userId, ct);
}
