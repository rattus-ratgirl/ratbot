namespace RatBot.Application.Features.AdminSay;

public interface IAdminSaySessionRepository
{
    Task StoreAsync(AdminSaySession session, CancellationToken ct = default);

    Task<AdminSaySession?> TryConsumeActiveAsync(
        string sessionId,
        ulong guildId,
        ulong userId,
        DateTimeOffset utcNow,
        CancellationToken ct = default);
}