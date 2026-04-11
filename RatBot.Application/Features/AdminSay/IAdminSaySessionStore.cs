namespace RatBot.Application.Features.AdminSay;

public interface IAdminSaySessionStore
{
    Task StoreAsync(AdminSaySession session, CancellationToken ct = default);

    Task<AdminSaySession?> TryConsumeActiveAsync(
        string sessionId,
        ulong guildId,
        ulong userId,
        DateTimeOffset utcNow,
        CancellationToken ct = default);
}
