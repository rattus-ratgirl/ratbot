using System.Collections.Concurrent;
using RatBot.Application.Features.AdminSay;

namespace RatBot.Infrastructure.Persistence;

public sealed class AdminSaySessionStore : IAdminSaySessionStore
{
    private readonly ConcurrentDictionary<string, AdminSaySession> _sessions =
        new ConcurrentDictionary<string, AdminSaySession>(StringComparer.Ordinal);

    public Task StoreAsync(AdminSaySession session, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        _sessions[session.SessionId] = session;
        return Task.CompletedTask;
    }

    public Task<AdminSaySession?> TryConsumeActiveAsync(
        string sessionId,
        ulong guildId,
        ulong userId,
        DateTimeOffset utcNow,
        CancellationToken ct = default)
    {
        bool sessionNotFound = !_sessions.TryRemove(sessionId, out AdminSaySession? session);

        if (sessionNotFound)
            return Task.FromResult<AdminSaySession?>(null);

        bool sessionIsInValid = session!.ExpiresAt > utcNow && session.GuildId == guildId && session.UserId == userId;

        return !sessionIsInValid
            ? Task.FromResult<AdminSaySession?>(null)
            : Task.FromResult<AdminSaySession?>(session);
    }
}
