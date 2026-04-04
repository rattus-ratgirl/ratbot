using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace RatBot.Interactions.Features.Games;

/// <summary>
/// Defines rock-paper-scissors interactions launched from a user context menu.
/// </summary>
[UsedImplicitly]
public sealed partial class RpsModule : SlashCommandBase
{
    private static readonly TimeSpan GameTtl = TimeSpan.FromMinutes(10);

    private static readonly ConcurrentDictionary<string, RpsGameState> Games = new ConcurrentDictionary<string, RpsGameState>();

    private enum RpsPick
    {
        Rock,
        Paper,
        Scissors,
    }

    private static void PurgeExpiredGames()
    {
        DateTimeOffset threshold = DateTimeOffset.UtcNow.Subtract(GameTtl);

        foreach ((string key, RpsGameState state) in Games)
            if (state.CreatedAt < threshold)
                Games.TryRemove(key, out _);
    }

    private sealed record RpsGameState(
        ulong ChallengerId,
        ulong OpponentId,
        DateTimeOffset CreatedAt,
        RpsPick? ChallengerPick,
        RpsPick? OpponentPick
    );
}
