using RatBot.Application.Emoji;

namespace RatBot.Discord.BackgroundWorkers;

public sealed class EmojiAnalyticsBackgroundWorker(
    ReactionQueue buffer,
    IServiceScopeFactory scopeFactory,
    ILogger logger) : BackgroundService
{
    private readonly ILogger _logger = logger.ForContext<EmojiAnalyticsBackgroundWorker>();

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("Emoji analytics background worker started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Wait for data to be available
                if (!await buffer.Reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false))
                    break;

                // Collect a batch of items
                Queue<string> emojiBatch = new Queue<string>();

                while (buffer.Reader.TryRead(out string? emojiId))
                {
                    emojiBatch.Enqueue(emojiId);

                    // Stop if batch size is reached (e.g., 100)
                    if (emojiBatch.Count >= 100)
                        break;
                }

                if (emojiBatch.Count > 0)
                    await ProcessBatchAsync(emojiBatch, stoppingToken).ConfigureAwait(false);

                // Add a small delay if the buffer was empty to avoid tight loop,
                // but only if we didn't just process a full batch.
                if (emojiBatch.Count < 100)
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Emoji analytics background worker is stopping.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Emoji analytics background worker encountered an error.");
        }
    }

    private async Task ProcessBatchAsync(Queue<string> emojiBatch, CancellationToken ct)
    {
        try
        {
            AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

            await using (scope.ConfigureAwait(false))
            {
                ReactionUsageTracker reactionUsageTracker =
                    scope.ServiceProvider.GetRequiredService<ReactionUsageTracker>();

                await reactionUsageTracker.RecordBatchUsageAsync(emojiBatch, ct).ConfigureAwait(false);

                _logger.Debug("Processed {Count} emoji usage events from channel.", emojiBatch.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to process emoji analytics batch.");
        }
    }
}