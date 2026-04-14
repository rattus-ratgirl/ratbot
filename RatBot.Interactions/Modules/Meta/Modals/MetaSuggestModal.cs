namespace RatBot.Interactions.Modules.Meta.Modals;

[UsedImplicitly]
public record MetaSuggestModal : IModal
{
    [InputLabel("Title")]
    [ModalTextInput("title", maxLength: 75)]
    public required string SuggestionTitle { get; [UsedImplicitly] init; }

    [InputLabel("Summary (1000)")]
    [ModalTextInput("summary", TextInputStyle.Paragraph, maxLength: 1500)]
    public required string Summary { get; [UsedImplicitly] init; }

    [InputLabel("Motivation")]
    [ModalTextInput("motivation", TextInputStyle.Paragraph, maxLength: 1950)]
    public required string Motivation { get; [UsedImplicitly] init; }

    [InputLabel("Specification")]
    [ModalTextInput("specification", TextInputStyle.Paragraph, maxLength: 1950)]
    public required string Specification { get; [UsedImplicitly] init; }

    string IModal.Title => "Make a suggestion";
}