using Microsoft.EntityFrameworkCore;
using RatBot.Domain.Emoji;

namespace RatBot.Application.Reactions;

public interface IEmojiRepository
{
    DbSet<EmojiUsageCount> EmojiUsageCounts { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
