using RatBot.Domain.Entities;
using RatBot.Infrastructure.Data;

namespace RatBot.Infrastructure.Services;

/// <summary>
/// Provides persistence operations for emoji usage analytics.
/// </summary>
public sealed class EmojiUsageService
{
    private readonly BotDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmojiUsageService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public EmojiUsageService(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static bool IsDuplicateKey(DbUpdateException ex)
    {
        string message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Increments usage count for the provided emoji identifier.
    /// </summary>
    /// <param name="emojiId">The emoji identifier.</param>
    /// <returns>A task that completes when the increment is persisted.</returns>
    public async Task IncrementUsageAsync(string emojiId)
    {
        int updatedRowCount = await _dbContext
            .EmojiUsageCounts.Where(x => x.EmojiId == emojiId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.UsageCount, x => x.UsageCount + 1));

        if (updatedRowCount != 0)
            return;

        _dbContext.EmojiUsageCounts.Add(new EmojiUsageCount { EmojiId = emojiId, UsageCount = 1 });

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsDuplicateKey(ex))
        {
            // A concurrent insert won the race; increment the now-existing row.
            _dbContext.ChangeTracker.Clear();
            await _dbContext
                .EmojiUsageCounts.Where(x => x.EmojiId == emojiId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.UsageCount, x => x.UsageCount + 1));
        }
    }

    /// <summary>
    /// Gets top emoji usage rows ordered by usage descending.
    /// </summary>
    /// <param name="limit">The maximum number of rows to return.</param>
    /// <returns>The ordered usage rows.</returns>
    public Task<List<EmojiUsageCount>> GetTopUsageAsync(int limit = 25)
    {
        int clampedLimit = Math.Clamp(limit, 1, 100);

        return _dbContext
            .EmojiUsageCounts.AsNoTracking()
            .OrderByDescending(x => x.UsageCount)
            .ThenBy(x => x.EmojiId)
            .Take(clampedLimit)
            .ToListAsync();
    }
}
