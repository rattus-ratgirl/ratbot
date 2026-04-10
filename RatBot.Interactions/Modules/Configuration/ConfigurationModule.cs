using JetBrains.Annotations;

namespace RatBot.Interactions.Modules.Configuration;

[Group("config", "Configuration commands.")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
[UsedImplicitly]
public sealed class ConfigurationModule : SlashCommandBase
{
    [Group("quorum", "Quorum configuration.")]
    public sealed class QuorumConfigurationModule(QuorumConfigurationService quorumConfigurationService)
        : GuildConfigurationModuleBase
    {
        [SlashCommand("set", "Create or update a quorum config for a channel or category scope ID.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task SetAsync(string scopeId, string roleIds, double proportion) =>
            ReplyAsync(() => SetResponseAsync(scopeId, roleIds, proportion));

        [SlashCommand("unset", "Remove a quorum config for a channel or category. The scope ID must be a channel or category ID.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task UnsetAsync(string scopeId) => ReplyAsync(() => UnsetResponseAsync(scopeId));

        private async Task<string> SetResponseAsync(string scopeId, string roleIds, double proportion)
        {
            if (!TryResolveQuorumScope(scopeId, out ResolvedQuorumScope resolvedScope, out string errorMessage))
                return errorMessage;

            if (!TryResolveRoles(roleIds, out SocketRole[] roles, out errorMessage))
                return errorMessage;

            try
            {
                (bool created, _) = await quorumConfigurationService.UpsertAsync(
                    Guild.Id,
                    resolvedScope.ScopeType,
                    resolvedScope.Channel.Id,
                    roles.Select(role => role.Id).ToArray(),
                    proportion);

                string action = created ? "created" : "updated";
                string roleSummary = string.Join(", ", roles.Select(role => role.Mention));

                return resolvedScope.Channel switch
                {
                    SocketTextChannel textChannel =>
                        $"Quorum config {action} for channel {textChannel.Mention} with roles {roleSummary} and proportion {proportion}.",
                    SocketCategoryChannel categoryChannel =>
                        $"Quorum config {action} for category \"{categoryChannel.Name}\" with roles {roleSummary} and proportion {proportion}.",
                    _ => "Invalid channel type for quorum config."
                };
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return ex.ParamName switch
                {
                    "value" => "Invalid proportion provided. Please provide a value greater than 0 and at most 1.",
                    "roleIds" => "Invalid role IDs provided. Please provide a comma-separated list of valid role IDs.",
                    "scopeType" => "Invalid channel type for quorum config.",
                    "scopeId" => "Invalid scope ID provided.",
                    _ => "Invalid quorum configuration provided."
                };
            }
        }

        private async Task<string> UnsetResponseAsync(string scopeId)
        {
            if (!TryResolveQuorumScope(scopeId, out ResolvedQuorumScope resolvedScope, out string errorMessage))
                return errorMessage;

            bool deleted = await quorumConfigurationService.DeleteAsync(
                Guild.Id,
                resolvedScope.ScopeType,
                resolvedScope.Channel.Id);

            if (!deleted)
                return "No quorum config exists for that scope.";

            return resolvedScope.Channel switch
            {
                SocketTextChannel textChannel => $"Quorum config removed for channel {textChannel.Mention}.",
                SocketCategoryChannel categoryChannel => $"Quorum config removed for category \"{categoryChannel.Name}\".",
                _ => "Invalid channel type for quorum config."
            };
        }

        private bool TryResolveQuorumScope(
            string scopeId,
            out ResolvedQuorumScope resolvedScope,
            out string errorMessage)
        {
            resolvedScope = null!;

            if (!TryResolveGuildScope(scopeId, out SocketGuildChannel? scope, out errorMessage))
                return false;

            QuorumScopeType? scopeType = scope.ChannelType switch
            {
                ChannelType.Text => QuorumScopeType.Channel,
                ChannelType.Category => QuorumScopeType.Category,
                _ => null
            };

            if (scopeType is null)
            {
                errorMessage = "Invalid channel type for quorum config.";
                return false;
            }

            resolvedScope = new ResolvedQuorumScope(scope, scopeType.Value);
            errorMessage = string.Empty;
            return true;
        }

        private sealed record ResolvedQuorumScope(SocketGuildChannel Channel, QuorumScopeType ScopeType);
    }
}
