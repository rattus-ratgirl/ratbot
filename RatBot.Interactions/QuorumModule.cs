using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using RatBot.Domain.Entities;
using RatBot.Domain.Enums;
using RatBot.Infrastructure.Services;

namespace RatBot.Interactions;

[Group("quorum", "Quorum commands.")]
public class QuorumModule(QuorumConfigService quorumConfigService) : SlashCommandBase
{
    public class Config(QuorumConfigService quorumConfigService) : SlashCommandBase
    {
        [SlashCommand(
            "add",
            "Add a quorum config for a channel or category. The Scope ID must be a channel or category ID."
        )]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddAsync(string scopeId, string roleId, double proportion)
        {
            if (!ulong.TryParse(scopeId, out ulong parsedScopeId) || !ulong.TryParse(roleId, out ulong parsedRoleId))
            {
                await RespondAsync("Invalid scope or role ID provided. Please provide valid IDs.");
                return;
            }

            if (Context.Guild.Channels.All(c => c.Id != parsedScopeId))
            {
                await RespondAsync("Invalid scope ID provided.");
                return;
            }

            if (Context.Guild.Roles.All(r => r.Id != parsedRoleId))
            {
                await RespondAsync("Invalid role ID provided.");
                return;
            }

            try
            {
                SocketGuildChannel scope = Context.Guild.Channels.FirstOrDefault(c => c.Id == parsedScopeId)!;
                QuorumScopeType scopeType = scope.ChannelType switch
                {
                    ChannelType.Text => QuorumScopeType.Channel,
                    ChannelType.Category => QuorumScopeType.Category,
                    _ => throw new ArgumentOutOfRangeException(),
                };

                await quorumConfigService.CreateAsync(
                    Context.Guild.Id,
                    scopeType,
                    scope.Id,
                    parsedRoleId,
                    proportion
                );

                SocketRole? role = Context.Guild.GetRole(parsedRoleId);
                SocketTextChannel? channel = Context.Guild.GetTextChannel(scope.Id);

                if (channel is not null)
                    await RespondAsync(
                        $"Quorum config added in channel {channel.Mention} with role {role.Mention} and proportion {proportion}.",
                        ephemeral: true
                    );

                SocketCategoryChannel? category = Context.Guild.GetCategoryChannel(scope.Id);
                if (category is not null)
                    await RespondAsync(
                        $"Quorum config added in category \"{category.Name}\" with role {role.Mention} and proportion {proportion}.",
                        ephemeral: true
                    );
            }
            catch (ArgumentOutOfRangeException)
            {
                await RespondAsync("Invalid channel type for quorum config.");
            }
        }
    }

    [SlashCommand("count", "Count the number of members needed for quorum.")]
    [RequireUserPermission(GuildPermission.SendPolls)]
    public async Task CountAsync()
    {
        if (Context.Channel is not ITextChannel currentChannel)
        {
            await RespondAsync("This command can only be used in a text channel.", ephemeral: true);
            return;
        }

        ulong channelId = currentChannel.Id;
        ulong categoryId = (await currentChannel.GetCategoryAsync()).Id;

        QuorumScopeConfig? config =
            await quorumConfigService.GetAsync(currentChannel.GuildId, QuorumScopeType.Channel, channelId)
            ?? await quorumConfigService.GetAsync(currentChannel.GuildId, QuorumScopeType.Category, categoryId);

        if (config is not null)
        {
            ulong roleId = config.RoleId;
            int membersWithRole = Context.Guild.GetRole(roleId)?.Members?.Count() ?? 0;
            int quorumCount = (int)Math.Ceiling(membersWithRole * config.QuorumProportion);

            await RespondAsync($"Quorum count for {currentChannel.Mention}: {quorumCount}");

            return;
        }

        await RespondAsync(
            "No quorum config found for this channel or category. Please use `/quorum config add` to add one.",
            ephemeral: true
        );
    }
}
