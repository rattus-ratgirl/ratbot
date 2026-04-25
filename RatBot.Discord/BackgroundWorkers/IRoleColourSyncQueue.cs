using System.Threading.Channels;

namespace RatBot.Discord.BackgroundWorkers;

public interface IRoleColourSyncQueue
{
    public readonly record struct WorkItem(ulong GuildId, ulong UserId);

    public sealed record Status(int Pending, int InFlight, double? PerSecond, TimeSpan? Eta);

    bool Enqueue(ulong guildId, ulong userId);
    ValueTask EnqueueAsync(ulong guildId, ulong userId, CancellationToken ct);

    ChannelReader<WorkItem> Reader { get; }

    void OnWorkStarted(WorkItem item);
    void OnWorkCompleted(WorkItem item);

    Status GetStatus();
}