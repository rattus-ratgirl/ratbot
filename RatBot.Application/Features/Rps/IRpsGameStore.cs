namespace RatBot.Application.Features.Rps;

public interface IRpsGameStore
{
    Task CreateAsync(RpsGameSession game, CancellationToken ct = default);

    Task<RpsPickSubmissionResult> SubmitPickAsync(
        string gameId,
        ulong userId,
        RpsPick pick,
        DateTimeOffset utcNow,
        CancellationToken ct = default);
}
