namespace RatBot.Domain.Features.Rps;

public sealed record RpsGameSession(
    string GameId,
    ulong ChallengerId,
    ulong OpponentId,
    DateTimeOffset ExpiresAt,
    RpsPick? ChallengerPick,
    RpsPick? OpponentPick);