using JetBrains.Annotations;
using LanguageExt;
using RatBot.Domain.Enums;
using RatBot.Infrastructure.Services;
using RatBot.Interactions.Features.Quorum;

namespace RatBot.Interactions.Features.Config;

public sealed partial class ConfigModule
{
    /// <summary>
    /// Defines quorum configuration interactions.
    /// </summary>
    [Group("quorum", "Quorum configuration.")]
    public sealed class QuorumConfigModule(IConfigRepository configRepository) : SlashCommandBase
    {
        private static QuorumScopeType? GetScopeType(SocketGuildChannel scope) =>
            scope.ChannelType switch
            {
                ChannelType.Text => QuorumScopeType.Channel,
                ChannelType.Category => QuorumScopeType.Category,
                _ => null,
            };

        private static Arr<ulong> ParseRoleIds(string roleIds)
        {
            if (string.IsNullOrWhiteSpace(roleIds))
                return Arr<ulong>.Empty;

            List<ulong> parsedRoleIds = [];

            foreach (string part in roleIds.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (!ulong.TryParse(part, out ulong roleId))
                    return Arr<ulong>.Empty;

                parsedRoleIds.Add(roleId);
            }

            return new Arr<ulong>(parsedRoleIds.Distinct());
        }

        /// <summary>
        /// Creates or updates a quorum configuration for a channel or category scope.
        /// </summary>
        /// <param name="scopeId">The channel or category identifier.</param>
        /// <param name="roleIds">The comma-separated role identifiers used for quorum counting.</param>
        /// <param name="proportion">The quorum proportion in decimal form.</param>
        [SlashCommand("set", "Create or update a quorum config for a channel or category scope ID.")]
        [UsedImplicitly, RequireUserPermission(GuildPermission.Administrator)]
        public Task SetAsync(string scopeId, string roleIds, double proportion)
        {
            SetQuorumConfigArgs args = new SetQuorumConfigArgs(scopeId, roleIds, proportion);
            return ReplyAsync(args, SetResponseAsync);
        }

        private async Task<string> SetResponseAsync(SetQuorumConfigArgs args)
        {
            SocketGuild guild = Context.Guild!;

            if (!ulong.TryParse(args.ScopeId, out ulong parsedScopeId))
                return "Invalid scope ID provided. Please provide a valid ID.";

            Arr<ulong> parsedRoleIds = ParseRoleIds(args.RoleIds);

            if (parsedRoleIds.Length == 0)
                return "Invalid role IDs provided. Please provide a comma-separated list of valid role IDs.";

            SocketGuildChannel? scope = guild.Channels.FirstOrDefault(channel => channel.Id == parsedScopeId);

            if (scope is null)
                return "Invalid scope ID provided.";

            Arr<SocketRole> roles = parsedRoleIds.Select(guild.GetRole);

            if (roles.Length != parsedRoleIds.Length)
                return "One or more role IDs are invalid for this guild.";

            QuorumScopeType? scopeType = GetScopeType(scope);

            if (scopeType is null)
                return "Invalid channel type for quorum config.";

            bool created = await QuorumConfigs.SetAsync(
                configRepository,
                QuorumScopeConfig.Create(guild.Id, scopeType.Value, scope.Id, parsedRoleIds, args.Proportion)
            );

            string action = created
                ? "created"
                : "updated";

            string roleSummary = string.Join(", ", roles.Select(role => role.Mention));

            return scope switch
            {
                SocketTextChannel textChannel =>
                    $"Quorum config {action} for channel {textChannel.Mention} with roles {roleSummary} and proportion {args.Proportion}.",
                SocketCategoryChannel categoryChannel =>
                    $"Quorum config {action} for category \"{categoryChannel.Name}\" with roles {roleSummary} and proportion {args.Proportion}.",
                _ => "Invalid channel type for quorum config.",
            };
        }

        /// <summary>
        /// Removes a quorum configuration for a channel or category scope.
        /// </summary>
        /// <param name="scopeId">The channel or category identifier.</param>
        [SlashCommand("unset", "Remove a quorum config for a channel or category. The scope ID must be a channel or category ID.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [UsedImplicitly]
        public Task UnsetAsync(string scopeId)
        {
            UnsetQuorumConfigArgs args = new UnsetQuorumConfigArgs(scopeId);
            return ReplyAsync(args, UnsetResponseAsync);
        }

        private async Task<string> UnsetResponseAsync(UnsetQuorumConfigArgs args)
        {
            SocketGuild guild = Context.Guild!;

            if (!ulong.TryParse(args.ScopeId, out ulong parsedScopeId))
                return "Invalid scope ID provided. Please provide a valid ID.";

            SocketGuildChannel? scope = guild.Channels.FirstOrDefault(channel => channel.Id == parsedScopeId);

            if (scope is null)
                return "Invalid scope ID provided.";

            QuorumScopeType? scopeType = GetScopeType(scope);

            if (scopeType is null)
                return "Invalid channel type for quorum config.";

            bool deleted = await QuorumConfigs.DeleteAsync(configRepository, guild.Id, scopeType.Value, scope.Id);

            if (!deleted)
                return "No quorum config exists for that scope.";

            return scope switch
            {
                SocketTextChannel textChannel => $"Quorum config removed for channel {textChannel.Mention}.",
                SocketCategoryChannel categoryChannel => $"Quorum config removed for category \"{categoryChannel.Name}\".",
                _ => "Invalid channel type for quorum config.",
            };
        }

        private sealed record SetQuorumConfigArgs(string ScopeId, string RoleIds, double Proportion);

        private sealed record UnsetQuorumConfigArgs(string ScopeId);
    }
}
