namespace RatBot.Domain.Features.Rps;

public sealed record RpsPickSubmissionResult(
    RpsPickSubmissionStatus Status,
    RpsGameSession? Game,
    RpsGameOutcome? Outcome);
