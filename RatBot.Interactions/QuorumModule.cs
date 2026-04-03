using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using RatBot.Domain.Entities;
using RatBot.Domain.Enums;
using RatBot.Infrastructure.Services;
using Serilog;

namespace RatBot.Interactions;

/// <summary>
/// Defines quorum-related interactions.
/// </summary>
/// <param name="logger">The logger instance.</param>
[Group("quorum", "Quorum commands.")]
public sealed class QuorumModule(ILogger logger, QuorumConfigService quorumConfigService) : SlashCommandBase
{
    private static ulong[] ParseRoleIds(string roleIds)
    {
        if (string.IsNullOrWhiteSpace(roleIds))
            return [];

        string[] parts = roleIds.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        Stack<ulong> parsedRoleIds = new Stack<ulong>(parts.Length);

        foreach (string part in parts)
        {
            if (!ulong.TryParse(part, out ulong roleId))
                return [];

            parsedRoleIds.Push(roleId);
        }

        return parsedRoleIds.Distinct().ToArray();
    }

    /// <summary>
    /// Adds a quorum configuration for a channel or category scope.
    /// </summary>
    /// <param name="scopeId">The channel or category identifier.</param>
    /// <param name="roleIds">The comma-separated role identifiers used for quorum counting.</param>
    /// <param name="proportion">The quorum proportion in decimal form.</param>
    [SlashCommand("add", "Add a quorum config for a channel or category. The Scope ID must be a channel or category ID.")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public Task AddAsync(string scopeId, string roleIds, double proportion)
    {
        AddQuorumConfigArgs args = new AddQuorumConfigArgs(scopeId, roleIds, proportion);
        return ReplyAsync(args, AddResponseAsync);
    }

    /// <summary>
    /// Counts members required to satisfy the current channel or category quorum configuration.
    /// </summary>
    [SlashCommand("count", "Count the number of members needed for quorum.")]
    [RequireUserPermission(GuildPermission.SendPolls)]
    public Task CountAsync()
    {
        return ReplyAsync(CountResponse);
    }

    private async Task<string> AddResponseAsync(AddQuorumConfigArgs args)
    {
        SocketGuild guild = Context.Guild!;

        if (!ulong.TryParse(args.ScopeId, out ulong parsedScopeId))
            return "Invalid scope ID provided. Please provide a valid ID.";

        ulong[] parsedRoleIds = ParseRoleIds(args.RoleIds);
        if (parsedRoleIds.Length == 0)
            return "Invalid role IDs provided. Please provide a comma-separated list of valid role IDs.";

        SocketGuildChannel? scope = guild.Channels.FirstOrDefault(channel => channel.Id == parsedScopeId);
        if (scope is null)
            return "Invalid scope ID provided.";

        SocketRole[] roles = parsedRoleIds.Select(guild.GetRole).Where(role => role is not null).ToArray();

        if (roles.Length != parsedRoleIds.Length)
            return "One or more role IDs are invalid for this guild.";

        QuorumScopeType? scopeType = scope.ChannelType switch
        {
            ChannelType.Text => QuorumScopeType.Channel,
            ChannelType.Category => QuorumScopeType.Category,
            _ => null,
        };

        if (scopeType is null)
            return "Invalid channel type for quorum config.";

        await quorumConfigService.CreateAsync(guild.Id, scopeType.Value, scope.Id, parsedRoleIds, args.Proportion);

        string roleSummary = string.Join(", ", roles.Select(role => role.Mention));

        return scope switch
        {
            SocketTextChannel textChannel =>
                $"Quorum config added in channel {textChannel.Mention} with roles {roleSummary} and proportion {args.Proportion}.",
            SocketCategoryChannel categoryChannel =>
                $"Quorum config added in category \"{categoryChannel.Name}\" with roles {roleSummary} and proportion {args.Proportion}.",
            _ => "Invalid channel type for quorum config.",
        };
    }

    private async Task<InteractionResponse> CountResponse()
    {
        if (Context.Channel is not ITextChannel currentChannel)
            return InteractionResponse.Ephemeral("This command can only be used in a text channel.");

        ICategoryChannel? category = await currentChannel.GetCategoryAsync();

        QuorumScopeConfig? config = await quorumConfigService.GetAsync(currentChannel.GuildId, QuorumScopeType.Channel, currentChannel.Id);

        if (config is null && category?.Id is { } categoryId)
            config = await quorumConfigService.GetAsync(currentChannel.GuildId, QuorumScopeType.Category, categoryId);

        if (config is null)
            return InteractionResponse.Ephemeral("No quorum config found for this channel or category. Please use `/quorum config add` to add one.");

        SocketGuild guild = Context.Guild!;

        SocketRole[] roles = config.RoleIds.Select(guild.GetRole).Where(role => role is not null).ToArray();

        int membersWithRole = roles.SelectMany(role => role.Members).Select(member => member.Id).Distinct().Count();

        int quorumCount = (int)Math.Ceiling(membersWithRole * config.QuorumProportion);

        logger.Debug(
            "Members with roles {RoleIds}: {MembersWithRole}, quorum count: {QuorumCount}, proportion: {ConfigQuorumProportion}",
            config.RoleIds,
            membersWithRole,
            quorumCount,
            config.QuorumProportion
        );

        return InteractionResponse.Public($"Quorum count for {currentChannel.Mention}: {quorumCount}");
    }

    private sealed record AddQuorumConfigArgs(string ScopeId, string RoleIds, double Proportion);
}
