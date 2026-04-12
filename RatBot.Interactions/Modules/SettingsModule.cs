namespace RatBot.Interactions.Modules;

[Group("config", "Configuration commands.")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public sealed class SettingsModule : SlashCommandBase
{
    [Group("quorum", "Quorum configuration.")]
    public sealed class QuorumSettingsModule(QuorumSettingsService quorumSettingsService) : SlashCommandBase
    {
        [SlashCommand("set", "Create or update a quorum settings for a channel or category ID.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetAsync(string targetId, string roleIds, double proportion)
        {
            if (Context.Guild is null)
            {
                await RespondAsync("This command can only be used in a guild.", ephemeral: true);
                return;
            }

            ErrorOr<ResolvedQuorumTarget> resolvedTargetResult = ResolveQuorumTarget(targetId);

            if (resolvedTargetResult.IsError)
            {
                await RespondAsync(resolvedTargetResult.FirstError.Description, ephemeral: true);
                return;
            }

            ErrorOr<SocketRole[]> rolesResult = ResolveGuildRoles(
                roleIds,
                "Invalid role IDs provided. Please provide a comma-separated list of valid role IDs.",
                "One or more role IDs are invalid for this guild.");

            if (rolesResult.IsError)
            {
                await RespondAsync(rolesResult.FirstError.Description, ephemeral: true);
                return;
            }

            ResolvedQuorumTarget resolvedTarget = resolvedTargetResult.Value;
            SocketRole[] roles = rolesResult.Value;

            ErrorOr<QuorumSettingsUpsertResult> upsertResult = await quorumSettingsService.UpsertAsync(
                Context.Guild.Id,
                resolvedTarget.TargetType,
                resolvedTarget.Channel.Id,
                roles.Select(role => role.Id).ToArray(),
                proportion);

            await upsertResult.SwitchFirstAsync(
                async result => await RespondAsync(
                    BuildSetResponse(resolvedTarget.Channel, roles, result.Created, proportion),
                    ephemeral: true),
                async error => await RespondAsync(DescribeError(error), ephemeral: true));
        }

        [SlashCommand(
            "unset",
            "Remove a quorum settings for a channel or category. The target ID must be a channel or category ID.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task UnsetAsync(string targetId)
        {
            if (Context.Guild is null)
            {
                await RespondAsync("This command can only be used in a guild.", ephemeral: true);
                return;
            }

            ErrorOr<ResolvedQuorumTarget> resolvedTargetResult = ResolveQuorumTarget(targetId);

            if (resolvedTargetResult.IsError)
            {
                await RespondAsync(resolvedTargetResult.FirstError.Description, ephemeral: true);
                return;
            }

            ResolvedQuorumTarget resolvedTarget = resolvedTargetResult.Value;

            ErrorOr<Deleted> deleteResult = await quorumSettingsService.DeleteAsync(
                Context.Guild.Id,
                resolvedTarget.TargetType,
                resolvedTarget.Channel.Id);

            await deleteResult.SwitchFirstAsync(
                async _ => await RespondAsync(BuildUnsetResponse(resolvedTarget.Channel), ephemeral: true),
                async error => await RespondAsync(DescribeError(error), ephemeral: true));
        }

        private static string BuildSetResponse(
            SocketGuildChannel channel,
            IReadOnlyCollection<SocketRole> roles,
            bool created,
            double proportion)
        {
            string action = created
                ? "created"
                : "updated";

            string roleSummary = string.Join(", ", roles.Select(role => role.Mention));

            return channel switch
            {
                SocketTextChannel textChannel =>
                    $"Quorum settings {action} for channel {textChannel.Mention} with roles {roleSummary} and proportion {proportion}.",
                SocketCategoryChannel categoryChannel =>
                    $"Quorum settings {action} for category \"{categoryChannel.Name}\" with roles {roleSummary} and proportion {proportion}.",
                _ => "Invalid channel type for quorum settings."
            };
        }

        private static string BuildUnsetResponse(SocketGuildChannel channel) =>
            channel switch
            {
                SocketTextChannel textChannel => $"Quorum settings removed for channel {textChannel.Mention}.",
                SocketCategoryChannel categoryChannel =>
                    $"Quorum settings removed for category \"{categoryChannel.Name}\".",
                _ => "Invalid channel type for quorum settings removal."
            };

        private static string DescribeError(Error error) =>
            error.Type switch
            {
                ErrorType.NotFound => "No quorum settings exist for that target.",
                _ => error.Description
            };

        private ErrorOr<ResolvedQuorumTarget> ResolveQuorumTarget(string targetId)
        {
            ErrorOr<SocketGuildChannel> scopeResult = ResolveGuildChannel(
                targetId,
                "Invalid scope ID provided.",
                "Invalid scope ID provided.");

            if (scopeResult.IsError)
                return scopeResult.Errors;

            SocketGuildChannel scope = scopeResult.Value;

            QuorumSettingsType? targetType = scope.ChannelType switch
            {
                ChannelType.Text => QuorumSettingsType.Channel,
                ChannelType.Category => QuorumSettingsType.Category,
                _ => null
            };

            if (targetType is null)
                return Error.Validation(description: "Invalid channel type for quorum settings.");

            return new ResolvedQuorumTarget(scope, targetType.Value);
        }

        private sealed record ResolvedQuorumTarget(SocketGuildChannel Channel, QuorumSettingsType TargetType);
    }
}