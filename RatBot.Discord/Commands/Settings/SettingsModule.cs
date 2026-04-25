using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using RatBot.Application.Common.Extensions;
using RatBot.Application.Meta;
using RatBot.Application.RoleColours;
using RatBot.Infrastructure.Data;
using RatBot.Discord.Handlers;
using RatBot.Infrastructure.RoleColours;
using Microsoft.EntityFrameworkCore;

namespace RatBot.Discord.Commands.Settings;

[Group("config", "Configuration commands.")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public sealed class SettingsModule : SlashCommandBase
{
    // ReSharper disable once InconsistentNaming
    private const string ONLY_IN_GUILD = "This command can only be used in a guild.";

    [Group("colour", "Role colour configuration.")]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public sealed class ColourSettingsModule(BotDbContext db, IRoleColourReconciler reconciler)
        : SlashCommandBase
    {
        [SlashCommand("add", "Register a source/display role colour mapping.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddAsync(
            [Summary("name", "Name used to identify this colour.")]
            string name,
            [Summary("source", "Source colour role users select.")]
            IRole source,
            [Summary("display", "Display colour role RatBot manages.")]
            IRole display)
        {
            if (Context.Guild is null)
            {
                await RespondAsync(ONLY_IN_GUILD, ephemeral: true);
                return;
            }

            await DeferAsync(true);

            RoleColourRegistrationContext registrationContext = BuildRegistrationContext(source.Id, display.Id);

            ErrorOr<RoleColourOption> result = await RegisterRoleColourOption.ExecuteAsync(
                db,
                new RegisterRoleColourOption.Command(
                    Key: name,
                    Label: name,
                    SourceRoleId: source.Id,
                    DisplayRoleId: display.Id,
                    RegistrationContext: registrationContext
                ),
                CancellationToken.None);

            await result.SwitchFirstAsync(
                async option =>
                {
                    int reconciled = await ReconcileMembersWithSourceRoleAsync(Context.Guild, option.SourceRoleId);
                    string message =
                        $"Registered colour option `{option.Key}` (‘{option.Label}’): {source.Mention} -> {display.Mention}.\n" +
                        $"Triggered colour sync for {reconciled} member(s).";

                    await FollowupAsync(message, ephemeral: true);
                },
                async error => await FollowupAsync(error.Description, ephemeral: true)
            );
        }

        [SlashCommand("disable", "Disable a configured role colour option.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DisableAsync(
            [Summary("name", "Name of the colour option to disable.")]
            string name)
        {
            if (Context.Guild is null)
            {
                await RespondAsync(ONLY_IN_GUILD, ephemeral: true);
                return;
            }

            ErrorOr<RoleColourOption> result = await DisableRoleColourOption.ExecuteAsync(
                db,
                new DisableRoleColourOption.Command(name),
                CancellationToken.None);

            await result.SwitchFirstAsync(
                async option => await RespondAsync($"Disabled colour option `{option.Key}`.", ephemeral: true),
                async error => await RespondAsync(error.Description, ephemeral: true)
            );
        }

        [SlashCommand("list", "List configured role colour options.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListAsync(
            [Summary("include-disabled", "Include disabled colour options.")]
            bool includeDisabled = true)
        {
            if (Context.Guild is null)
            {
                await RespondAsync(ONLY_IN_GUILD, ephemeral: true);
                return;
            }

            IReadOnlyList<RoleColourOption> options = await ListRoleColourOptions.ExecuteAsync(
                db,
                new ListRoleColourOptions.Query(includeDisabled),
                CancellationToken.None);

            if (options.Count == 0)
            {
                await RespondAsync("No colour options are configured.", ephemeral: true);
                return;
            }

            await RespondAsync(BuildListResponse(options), ephemeral: true);
        }

        [SlashCommand("sync", "Reconcile display colour roles for all members who currently have a source colour role.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SyncAsync()
        {
            if (Context.Guild is null)
            {
                await RespondAsync(ONLY_IN_GUILD, ephemeral: true);
                return;
            }

            await DeferAsync(true);

            // Load enabled options and gather SCR set
            List<RoleColourOption> enabled = await db.RoleColourOptions
                .AsNoTracking()
                .Where(o => o.IsEnabled)
                .ToListAsync(CancellationToken.None);

            if (enabled.Count == 0)
            {
                await FollowupAsync("No enabled colour options are configured.", ephemeral: true);
                return;
            }

            HashSet<ulong> scrSet = enabled
                .Select(o => o.SourceRoleId)
                .ToHashSet();

            int reconciled = await ReconcileMembersWithAnySourceRoleAsync(Context.Guild, scrSet);

            await FollowupAsync($"Triggered colour sync for {reconciled} member(s).", ephemeral: true);
        }

        private async Task<int> ReconcileMembersWithSourceRoleAsync(IGuild guild, ulong sourceRoleId)
        {
            IReadOnlyCollection<IGuildUser> users = await guild.GetUsersAsync();
            List<IGuildUser> targetUsers = users
                .Where(u => u.RoleIds.Contains(sourceRoleId))
                .ToList();

            foreach (IGuildUser user in targetUsers)
                await reconciler.ReconcileMemberAsync(guild, user.Id, CancellationToken.None);

            return targetUsers.Count;
        }

        private async Task<int> ReconcileMembersWithAnySourceRoleAsync(IGuild guild, HashSet<ulong> sourceRoleIds)
        {
            IReadOnlyCollection<IGuildUser> users = await guild.GetUsersAsync();
            List<IGuildUser> targetUsers = users
                .Where(u => u.RoleIds.Any(sourceRoleIds.Contains))
                .ToList();

            foreach (IGuildUser user in targetUsers)
                await reconciler.ReconcileMemberAsync(guild, user.Id, CancellationToken.None);

            return targetUsers.Count;
        }

        private RoleColourRegistrationContext BuildRegistrationContext(ulong sourceRoleId, ulong displayRoleId)
        {
            SocketRole? sourceRole = Context.Guild.GetRole(sourceRoleId);
            SocketRole? displayRole = Context.Guild.GetRole(displayRoleId);

            return new RoleColourRegistrationContext(
                SourceRoleExists: sourceRole is not null,
                DisplayRoleExists: displayRole is not null,
                SourceRoleHasColour: sourceRole is not null
                                     && sourceRole.Colors.PrimaryColor != Color.Default);
        }

        private static string BuildListResponse(IReadOnlyList<RoleColourOption> options)
        {
            StringBuilder builder = new StringBuilder("Configured colour options:");

            foreach (RoleColourOption option in options)
            {
                string state = option.IsEnabled
                    ? "enabled"
                    : "disabled";

                builder.AppendLine()
                    .Append($"`{option.Key}`: <@&{option.SourceRoleId}> -> <@&{option.DisplayRoleId}> ({state})");
            }

            return builder.ToString();
        }
    }

    [Group("meta", "Meta configuration.")]
    public sealed class MetaSettingsModule(MetaSuggestionSettingsService metaSuggestionSettingsService)
        : SlashCommandBase
    {
        [SlashCommand("suggest", "Set the forum channel used for `/meta suggest` submissions.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetSuggestForumChannelAsync(IForumChannel channel)
        {
            ErrorOr<Success> result =
                await metaSuggestionSettingsService.UpsertSuggestForumChannelAsync(Context.Guild.Id, channel.Id);

            await result.SwitchFirstAsync(
                async _ => await RespondAsync($"Meta suggestion forum set to {channel.Mention}.", ephemeral: true),
                async error => await RespondAsync(error.Description, ephemeral: true)
            );
        }
    }

    [Group("quorum", "Quorum configuration.")]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public sealed class QuorumSettingsModule(
        IQuorumSettingsWriter quorumSettingsWriter,
        IQuorumSettingsReader quorumSettingsReader,
        IQuorumCommandInputResolver inputResolver)
        : SlashCommandBase
    {
        [SlashCommand("set", "Create or update quorum settings for a channel or category.")]
        public async Task SetAsync(
            [Summary("target", "The target channel or category.")]
            [ChannelTypes(ChannelType.Text, ChannelType.Category)]
            IChannel target,
            [Summary("roles", "Comma-separated role mentions or role IDs.")]
            string roles,
            [Summary("proportion", "The quorum proportion, from 0 to 1.")]
            double proportion
        )
        {
            ErrorOr<QuorumSettingsUpsertResult> upsertResult = await (
                from resolvedTarget in inputResolver.ResolveTarget(Context.Guild, target)
                from parsedRoles in inputResolver.ResolveRoles(Context.Guild, roles)
                let parsedRoleIds = parsedRoles.Select(role => role.Id)
                from upsert in quorumSettingsWriter.UpsertAsync(
                    resolvedTarget,
                    parsedRoleIds,
                    proportion)
                select upsert
            );

            await upsertResult.SwitchFirstAsync(
                async result =>
                {
                    ImmutableArray<SocketRole> savedRoles = result.Config
                        .Roles.Select(role => Context.Guild.GetRole(role.Id))
                        .Where(role => role is not null)
                        .ToImmutableArray();

                    await RespondAsync(
                        BuildSetResponse(target, savedRoles, result.Created, proportion),
                        ephemeral: true);
                },
                async error => await RespondAsync(DescribeError(error), ephemeral: true)
            );
        }

        [SlashCommand(
            "unset",
            "Remove a quorum settings for a channel or category. The target ID must be a channel or category ID.")]
        public async Task UnsetAsync(
            [Summary("target", "The target channel or category.")]
            [ChannelTypes(ChannelType.Text, ChannelType.Category)]
            IChannel target
        )
        {
            if (Context.Guild is null)
            {
                await RespondAsync(ONLY_IN_GUILD, ephemeral: true);
                return;
            }

            ErrorOr<QuorumTarget> resolvedTargetResult = inputResolver.ResolveTarget(Context.Guild, target);

            if (resolvedTargetResult.IsError)
            {
                await RespondAsync(resolvedTargetResult.FirstError.Description, ephemeral: true);
                return;
            }

            QuorumTarget resolvedTarget = resolvedTargetResult.Value;
            SocketGuildChannel targetChannel = (SocketGuildChannel)target;

            ErrorOr<Deleted> deleteResult = await quorumSettingsWriter.DeleteAsync(resolvedTarget);

            await deleteResult.SwitchFirstAsync(
                async _ => await RespondAsync(BuildUnsetResponse(targetChannel), ephemeral: true),
                async error => await RespondAsync(DescribeError(error), ephemeral: true)
            );
        }

        [SlashCommand("view", "View the quorum settings for a channel or category.")]
        public async Task ViewAsync(
            [Summary("target", "The target channel or category.")]
            [ChannelTypes(ChannelType.Text, ChannelType.Category)]
            IChannel target
        )
        {
            ErrorOr<QuorumSettings> getResult =
                await (
                    from resolvedTarget in Task.FromResult(inputResolver.ResolveTarget(Context.Guild, target))
                    from settings in quorumSettingsReader.GetAsync(resolvedTarget)
                    select settings
                );

            await getResult.SwitchFirstAsync(
                async settings =>
                {
                    ImmutableArray<SocketRole> resolvedRoles = settings
                        .Roles.Select(role => Context.Guild.GetRole(role.Id))
                        .Where(role => role is not null)
                        .ToImmutableArray();

                    await RespondAsync(
                        BuildViewResponse(target, resolvedRoles, settings.Proportion),
                        ephemeral: true);
                },
                async error => await RespondAsync(DescribeError(error), ephemeral: true)
            );
        }

        private static string BuildSetResponse(
            IChannel channel,
            ImmutableArray<SocketRole> roles,
            bool created,
            double prop)
        {
            string action = created
                ? "created"
                : "updated";

            string renderedRoles = string.Join(", ", roles.Select(role => role.Mention));
            string renderedProportion = FormatProportion(prop);

            return channel switch
            {
                SocketTextChannel textChannel =>
                    $"Quorum settings {action} for channel {textChannel.Mention} with roles {renderedRoles} and proportion {renderedProportion}.",
                SocketCategoryChannel categoryChannel =>
                    $"Quorum settings {action} for category \"{categoryChannel.Name}\" with roles {renderedRoles} and proportion {renderedProportion}.",
                _ => "Invalid channel type for quorum settings."
            };
        }

        private static string BuildViewResponse(IChannel channel, ImmutableArray<SocketRole> roles, double prop)
        {
            string renderedRoles = roles.Length == 0
                ? "none"
                : string.Join(", ", roles.Select(role => role.Mention));

            string renderedProportion = FormatProportion(prop);

            return channel switch
            {
                SocketTextChannel textChannel =>
                    $"Quorum settings for channel {textChannel.Mention}: roles {renderedRoles}; proportion {renderedProportion}.",
                SocketCategoryChannel categoryChannel =>
                    $"Quorum settings for category \"{categoryChannel.Name}\": roles {renderedRoles}; proportion {renderedProportion}.",
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

        private static string FormatProportion(double proportion) =>
            $"{(proportion * 100).ToString("0.0", CultureInfo.InvariantCulture)}%";
    }
}