using RatBot.Application.Features.Moderation.Interfaces;
using RatBot.Domain.Features.Moderation;
using RatBot.Domain.Primitives;

namespace RatBot.Interactions.Modules;

[UsedImplicitly]
public static class ModerationModule
{
    [Group("smod", "Senior moderation commands.")]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    public sealed class SeniorModeration(ILogger logger, IModerationService moderationService) : SlashCommandBase
    {
        [SlashCommand("autoban", "Register a user to be banned if they join.")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task AutobanAsync([Summary("User ID", "The Discord user ID to autoban.")] string user)
        {
            if (!ulong.TryParse(user, out ulong parsedUserId) || parsedUserId == 0)
            {
                await RespondAsync("Enter a valid Discord user ID or mention.", ephemeral: true);
                return;
            }

            UserSnowflake bannedUser = parsedUserId;

            if (Context.Guild.GetUser(bannedUser) is not null)
            {
                await RespondAsync(
                    "That user is currently in the server. Use a regular ban instead.",
                    ephemeral: true);

                return;
            }

            GuildSnowflake guildId = Context.Guild.Id;
            UserSnowflake moderator = Context.User.Id;

            ErrorOr<AutobannedUser> result = await moderationService.RegisterAutobanAsync(
                guildId,
                bannedUser,
                moderator);

            if (result.IsError)
            {
                await RespondAsync(result.FirstError.Description, ephemeral: true);
                return;
            }

            logger.Information(
                "User {User} ({UserId}) registered autoban for {TargetId} in guild {GuildId}.",
                Context.User.Username,
                Context.User.Id,
                bannedUser,
                guildId);

            await RespondAsync($"Registered {bannedUser.ToMention()} for autoban.", ephemeral: true);
        }
    }

    [Group("mod", "Moderation commands.")]
    [DefaultMemberPermissions(GuildPermission.MuteMembers)]
    public sealed class Moderation : SlashCommandBase
    {
    }
}