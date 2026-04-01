using RatBot.Domain.Entities;
using RatBot.Infrastructure.Data;

namespace RatBot.Infrastructure.Services;

public sealed class UserVirtueService
{
    private readonly BotDbContext _dbContext;

    public UserVirtueService(BotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> AddVirtueDeltaAsync(ulong userId, int delta)
    {
        if (delta == 0)
            return await GetVirtueAsync(userId);

        int updatedRowCount = await _dbContext
            .UserVirtues.Where(x => x.UserId == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Virtue, x => x.Virtue + delta));

        if (updatedRowCount == 0)
        {
            _dbContext.UserVirtues.Add(new UserVirtue { UserId = userId, Virtue = delta });

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsDuplicateKey(ex))
            {
                // A concurrent writer inserted first; apply this delta via update instead.
                _dbContext.ChangeTracker.Clear();
                await _dbContext
                    .UserVirtues.Where(x => x.UserId == userId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Virtue, x => x.Virtue + delta));
            }
        }

        int updatedVirtue = await _dbContext
            .UserVirtues.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Virtue)
            .SingleAsync();

        return updatedVirtue;
    }

    private static bool IsDuplicateKey(DbUpdateException ex)
    {
        string message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<int> GetVirtueAsync(ulong userId)
    {
        UserVirtue? userVirtue = await _dbContext.UserVirtues.FindAsync(userId);
        return userVirtue?.Virtue ?? 0;
    }

    public async Task<int?> TryGetVirtueAsync(ulong userId)
    {
        UserVirtue? userVirtue = await _dbContext.UserVirtues.FindAsync(userId);
        return userVirtue?.Virtue;
    }

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

    public Task<UserVirtue?> GetLowestVirtueAsync()
    {
        return _dbContext
            .UserVirtues.AsNoTracking()
            .OrderBy(x => x.Virtue)
            .ThenBy(x => x.UserId)
            .FirstOrDefaultAsync();
    }
}
