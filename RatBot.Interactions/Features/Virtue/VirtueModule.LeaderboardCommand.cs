using System.Text;
using Discord.Interactions;
using RatBot.Domain.Entities;

namespace RatBot.Interactions.Features.Virtue;

public sealed partial class VirtueModule
{
    /// <summary>
    /// Shows the virtue leaderboard for the current guild.
    /// </summary>
    [SlashCommand("leaderboard", "Show the top 20 users by virtue.")]
    public Task LeaderboardAsync()
    {
        return ReplyAsync(GetLeaderboardResponseAsync, defer: true);
    }

    private async Task<string> GetLeaderboardResponseAsync()
    {
        List<UserVirtue> topUsers = await _userVirtueService.GetTopVirtuesAsync(20);
        if (topUsers.Count == 0)
            return "No virtue entries found yet.";

        StringBuilder text = new StringBuilder("Virtue leaderboard:\n");
        int rank = 1;

        foreach (UserVirtue entry in topUsers)
        {
            string mention = $"<@{entry.UserId}>";
            text.AppendLine($"{rank}. {mention}: {entry.Virtue}");
            rank++;
        }

        return text.ToString();
    }
}
