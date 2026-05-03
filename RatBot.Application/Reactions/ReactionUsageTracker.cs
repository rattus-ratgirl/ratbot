using Microsoft.EntityFrameworkCore;
using RatBot.Application.Common.Interfaces;
using RatBot.Domain.Emoji;

namespace RatBot.Application.Reactions;

public sealed class ReactionUsageTracker(IEmojiRepository emojiRepository, ILogger logger)
{
    private readonly ILogger _logger = logger.ForContext<ReactionUsageTracker>();

    public async Task RecordBatchUsageAsync(IEnumerable<string> emojiIds, CancellationToken ct = default)
    {
        List<(string Id, int N)> usages = emojiIds
            .GroupBy(x => x, StringComparer.Ordinal)
            .Select(g => (EmojiId: g.Key, Count: g.Count()))
            .ToList();

        foreach ((string emojiId, int count) in usages)
        {
            int updatedRowCount = await emojiRepository.EmojiUsageCounts
                .Where(x => x.EmojiId == emojiId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        x => x.ReactionUsageCount,
                        x => x.ReactionUsageCount + count),
                    ct)
                .ConfigureAwait(false);

            if (updatedRowCount != 0)
                continue;

            emojiRepository.EmojiUsageCounts.Add(new EmojiUsageCount
            {
                EmojiId = emojiId,
                ReactionUsageCount = count,
                MessageUsageCount = 0,
            });

            await emojiRepository.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        foreach ((string Id, int N) usage in usages)
            _logger.Verbose("Recorded {EmojiUsageCount} usages for emoji {EmojiId}.", usage.N, usage.Id);
    }

    public async Task<ErrorOr<List<EmojiUsageCount>>> GetTopUsageAsync(int limit = 25, CancellationToken ct = default)
    {
        int clampedLimit = Math.Clamp(limit, 1, 100);

        List<EmojiUsageCount> topUsage = await emojiRepository.EmojiUsageCounts
            .AsNoTracking()
            .OrderByDescending(x => x.ReactionUsageCount)
            .ThenBy(x => x.EmojiId)
            .Take(clampedLimit)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (topUsage.Count == 0)
            return Error.NotFound(description: "No emoji usage has been recorded yet.");

        return topUsage;
    }
}
