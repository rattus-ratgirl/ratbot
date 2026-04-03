using System.Collections.Concurrent;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

// ReSharper disable ClassNeverInstantiated.Global

namespace RatBot.Interactions;

/// <summary>
/// Defines administrative slash commands.
/// </summary>
[Group("admin", "Administrative commands.")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public sealed class AdminModule : SlashCommandBase
{
    internal const int DiscordMessageLimit = 2000;
    internal const string SayModalCustomId = "admin-say";
    private const int ModalMessageLimit = 4000;
    private static readonly TimeSpan PendingRequestTtl = TimeSpan.FromMinutes(15);
    private static readonly ConcurrentDictionary<string, PendingAdminSayRequest> PendingRequests =
        new ConcurrentDictionary<string, PendingAdminSayRequest>();

    internal static bool TryTakePendingChannelId(ulong guildId, ulong userId, out ulong channelId)
    {
        string pendingKey = GetPendingRequestKey(guildId, userId);
        bool found = PendingRequests.TryRemove(pendingKey, out PendingAdminSayRequest? pendingRequest);

        channelId = pendingRequest?.ChannelId ?? 0;
        return found;
    }

    internal static IReadOnlyList<string> SplitIntoChunks(string message, int chunkSize)
    {
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

    private static string GetPendingRequestKey(ulong guildId, ulong userId)
    {
        return $"{guildId}:{userId}";
    }

    private static void PurgeExpiredPendingRequests()
    {
        DateTimeOffset threshold = DateTimeOffset.UtcNow.Subtract(PendingRequestTtl);

        foreach ((string key, PendingAdminSayRequest pendingRequest) in PendingRequests)
            if (pendingRequest.CreatedAt < threshold)
                PendingRequests.TryRemove(key, out _);
    }

    /// <summary>
    /// Opens a modal to send a multi-line bot-authored message to a target channel.
    /// </summary>
    /// <param name="channel">The destination channel.</param>
    [SlashCommand("say", "Send a multiline message as the bot to a specific channel.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SayAsync(ITextChannel channel)
    {
        SocketGuild guild = Context.Guild!;
        ChannelPermissions botPermissions = guild.CurrentUser.GetPermissions(channel);
        if (!botPermissions.ViewChannel || !botPermissions.SendMessages)
        {
            await RespondAsync($"I don't have permission to post in {channel.Mention}.", ephemeral: true);
            return;
        }

        PurgeExpiredPendingRequests();
        PendingRequests[GetPendingRequestKey(guild.Id, Context.User.Id)] = new PendingAdminSayRequest(channel.Id, DateTimeOffset.UtcNow);

        await RespondWithModalAsync<AdminSayModal>(SayModalCustomId);
    }

    /// <summary>
    /// Modal payload for the admin say command.
    /// </summary>
    public sealed class AdminSayModal : IModal
    {
        /// <summary>
        /// Gets the modal title displayed to the user.
        /// </summary>
        public string Title => "Send Message";

        /// <summary>
        /// Gets or sets the message body entered by the user.
        /// </summary>
        [InputLabel("Message")]
        [ModalTextInput(
            "message",
            TextInputStyle.Paragraph,
            placeholder: "Write the message exactly as it should be posted.",
            maxLength: ModalMessageLimit
        )]
        public string Message { get; set; } = string.Empty;
    }

    private sealed record PendingAdminSayRequest(ulong ChannelId, DateTimeOffset CreatedAt);
}
