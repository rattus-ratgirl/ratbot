namespace RatBot.Interactions.Common.Discord;

/// <summary>
/// Splits message text into chunks that respect Discord message length limits.
/// </summary>
public static class DiscordMessageChunker
{
    /// <summary>
    /// Discord's current maximum number of characters per message.
    /// </summary>
    public const int DiscordMessageCharacterLimit = 2000;

    /// <summary>
    /// Splits a message into chunks no longer than <paramref name="chunkSize"/>.
    /// Prefers newline boundaries when possible.
    /// </summary>
    public static IReadOnlyList<string> SplitForMessageLimit(string message, int chunkSize = DiscordMessageCharacterLimit)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be greater than zero.");

        List<string> chunks = [];
        int index = 0;

        while (index < message.Length)
        {
            int remainingLength = message.Length - index;
            if (remainingLength <= chunkSize)
            {
                chunks.Add(message[index..]);
                break;
            }

            string window = message.Substring(index, chunkSize);
            int splitAt = window.LastIndexOf('\n');
            int chunkLength = splitAt > 0 ? splitAt + 1 : chunkSize;

            chunks.Add(message.Substring(index, chunkLength));
            index += chunkLength;
        }

        return chunks;
    }
}
