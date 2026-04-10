namespace RatBot.Domain.Features.AdminSay;

public sealed record AdminSaySession(
    string SessionId,
    ulong GuildId,
    ulong UserId,
    ulong ChannelId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);
