using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RatBot.Application.Common.Interfaces;
using RatBot.Domain.Emoji;

namespace RatBot.Application.MessageContent;

public sealed class EmojiUsageTracker(IEmojiRepository emojiRepository, ILogger logger)
{
    private static readonly Regex EmojiRegex = new Regex(
        @"(?:<(?<animated>a)?:(?<name>\w{2,32}):)?(?<id>\d{17,21})>?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    private readonly ILogger _logger = logger.ForContext<EmojiUsageTracker>();

    public async Task RecordMessageBatchUsageAsync(IEnumerable<string> messageContents, CancellationToken ct = default)
    {
        List<(string Id, int N)> usages = messageContents
            .SelectMany(ExtractEmojiIds)
            .GroupBy(x => x, StringComparer.Ordinal)
            .Select(g => (EmojiId: g.Key, Count: g.Count()))
            .ToList();

        foreach ((string emojiId, int count) in usages)
        {
            int updatedRowCount = await emojiRepository.EmojiUsageCounts
                .Where(x => x.EmojiId == emojiId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        x => x.MessageUsageCount,
                        x => x.MessageUsageCount + count),
                    ct)
                .ConfigureAwait(false);

            if (updatedRowCount != 0)
                continue;

            emojiRepository.EmojiUsageCounts.Add(new EmojiUsageCount
            {
                EmojiId = emojiId,
                ReactionUsageCount = 0,
                MessageUsageCount = count,
            });

            await emojiRepository.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        foreach ((string Id, int N) usage in usages)
            _logger.Verbose("Recorded {EmojiUsageCount} message usages for emoji {EmojiId}.", usage.N, usage.Id);
    }

    private static IEnumerable<string> ExtractEmojiIds(string messageContent) =>
        EmojiRegex
            .Matches(messageContent)
            .Select(match => match.Groups["id"].Value);
}
