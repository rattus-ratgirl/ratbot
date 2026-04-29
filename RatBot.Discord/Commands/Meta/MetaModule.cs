using RatBot.Application.Meta;

namespace RatBot.Discord.Commands.Meta;

[Group("meta", "Meta suggestion commands.")]
public sealed class MetaModule(MetaSuggestionService metaSuggestionService) : SlashCommandBase
{
    private const string ModalCustomIdPrefix = "meta-suggest";
    private const string Public = "Public";
    private const string Anonymous = "Anonymous";

    [SlashCommand("suggest", "Submit a meta suggestion.")]
    public async Task SuggestAsync(
        [Summary("attribution", "How your identity should be treated if this suggestion is published publicly.")]
        [Choice(Public, Public)]
        [Choice(Anonymous, Anonymous)]
        string attribution)
    {
        if (Context.Guild is null)
        {
            await RespondAsync("This command can only be used in a guild.", ephemeral: true);
            return;
        }

        string customId = $"{ModalCustomIdPrefix}:{Context.User.Id}:{attribution}";
        await Context.Interaction.RespondWithModalAsync<MetaSuggestModal>(customId);
    }

    [ModalInteraction($"{ModalCustomIdPrefix}:*:*", true)]
    public async Task SuggestModalAsync(ulong invokerUserId, string attributionValue, MetaSuggestModal modal)
    {
        if (Context.User.Id != invokerUserId)
        {
            await RespondAsync("Only the user who opened this modal can submit it.", ephemeral: true);
            return;
        }

        if (Context.Guild is null)
        {
            await RespondAsync("This command can only be used in a guild.", ephemeral: true);
            return;
        }

        bool isPublic = string.Equals(attributionValue, Public, StringComparison.Ordinal);

        await DeferAsync(true);

        ErrorOr<MetaSuggestion> suggestionResult = MetaSuggestion.Create(
            Context.Guild.Id,
            invokerUserId,
            modal.SuggestionTitle,
            modal.Summary,
            modal.Motivation,
            modal.Specification,
            isAnonymous: !isPublic,
            DateTimeOffset.UtcNow
        );

        if (suggestionResult.IsError)
        {
            await FollowupAsync(suggestionResult.FirstError.Description, ephemeral: true);
            return;
        }

        ErrorOr<Success> submitResult = await metaSuggestionService.SubmitAsync(
            suggestionResult.Value
        );

        await submitResult.SwitchFirstAsync(
            async _ =>
                await FollowupAsync(
                    "Your suggestion has been noted and will now be reviewed by the committee <a:wrattendown:1494139087614120076>",
                    ephemeral: true
                ),
            async error => await FollowupAsync(error.Description, ephemeral: true)
        );
    }
}