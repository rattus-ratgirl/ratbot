using RatBot.Domain.Entities;
using RatBot.Infrastructure.Data;

namespace RatBot.Infrastructure.Services;

/// <summary>
/// Provides persistence operations for user virtue scores.
/// </summary>
public sealed class UserVirtueService
{
    private readonly BotDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserVirtueService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public UserVirtueService(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets the highest virtue users ordered by score descending.
    /// </summary>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <returns>The ordered top virtue entries.</returns>
    public Task<List<UserVirtue>> GetTopVirtuesAsync(int limit = 20)
    {
        int clampedLimit = Math.Clamp(limit, 1, 100);

        return _dbContext
            .UserVirtues.AsNoTracking()
            .OrderByDescending(x => x.Virtue)
            .ThenBy(x => x.UserId)
            .Take(clampedLimit)
            .ToListAsync();
    }
}
