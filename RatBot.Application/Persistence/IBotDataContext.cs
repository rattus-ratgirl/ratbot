using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RatBot.Domain.Features.Emoji;

namespace RatBot.Application.Persistence;

public interface IBotDataContext
{
    DbSet<EmojiUsageCount> EmojiUsageCounts { get; }

    ChangeTracker ChangeTracker { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}