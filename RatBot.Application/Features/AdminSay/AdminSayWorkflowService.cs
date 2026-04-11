namespace RatBot.Application.Features.AdminSay;

public sealed class AdminSayWorkflowService(IAdminSaySessionRepository sessionRepository, ILogger logger)
{
    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(5);

    private readonly ILogger _logger = logger.ForContext<AdminSayWorkflowService>();

    public async Task<AdminSaySession> CreateSessionAsync(
        ulong guildId,
        ulong userId,
        ulong channelId,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfZero(guildId);
        ArgumentOutOfRangeException.ThrowIfZero(userId);
        ArgumentOutOfRangeException.ThrowIfZero(channelId);

        DateTimeOffset createdAt = DateTimeOffset.UtcNow;

        AdminSaySession session = new AdminSaySession(
            Guid.NewGuid().ToString("N"),
            guildId,
            userId,
            channelId,
            createdAt,
            createdAt.Add(SessionTtl));

        await sessionRepository.StoreAsync(session, ct);

        _logger.Information(
            "Created admin-say session {SessionId} for guild {GuildId}, user {UserId}, channel {ChannelId}.",
            session.SessionId,
            session.GuildId,
            session.UserId,
            session.ChannelId);

        return session;
    }

    public Task<AdminSaySession?> ConsumeSessionAsync(
        string sessionId,
        ulong guildId,
        ulong userId,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentOutOfRangeException.ThrowIfZero(guildId);
        ArgumentOutOfRangeException.ThrowIfZero(userId);

        return sessionRepository.TryConsumeActiveAsync(sessionId, guildId, userId, DateTimeOffset.UtcNow, ct);
    }
}