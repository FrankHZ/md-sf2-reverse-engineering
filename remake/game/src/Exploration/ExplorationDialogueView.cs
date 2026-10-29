using System.Text.RegularExpressions;
using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Maps;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Field text projection only; the controller retains reveal delivery, input and completion tokens.
internal sealed partial class ExplorationDialogueView : Control
{
    internal StoryDialoguePanel Panel { get; } = new() { Name = "DialoguePanel" };
    private ContextHint _context = new() { Name = "ContextHint" };
    internal Label ActiveHint => Panel.Visible ? Panel.Hint : _context.Text;
    private Label _dialogue => Panel.Body;
    private TextWindow? _textWindow;
    private bool _fieldTextProjection;
    internal double Revealed { get; set; }
    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Ignore;
        AddChild(Panel); AddChild(_context);
    }
    internal void Present(GameSession session, GameInput input, WorldViewportLayout viewport, bool battleMounted, bool revealed)
    {
        var current = session.Current;
        string previousText = _dialogue.Text;
        int previouslyVisible = _dialogue.VisibleCharacters < 0 ? _dialogue.GetTotalCharacterCount() : _dialogue.VisibleCharacters;
        _dialogue.Text = current.Story.Wait switch
        {
            DialogueWait text => session.Definition.Exploration!.Texts.GetValueOrDefault(text.Text) ?? "",
            ChoiceWait => current.Story.TextWindow is OpenTextWindow question ? session.Definition.Exploration!.Texts.GetValueOrDefault(question.Text) ?? "" : previousText,
            PresentationWait => current.Story.TextWindow is OpenTextWindow text ? session.Definition.Exploration!.Texts.GetValueOrDefault(text.Text) ?? "" : "",
            _ => current.Story.TextWindow is OpenTextWindow open ? session.Definition.Exploration!.Texts.GetValueOrDefault(open.Text) ?? "" : "",
        };
        var names = session.Definition.Exploration!.MemberNames;
        // The resolved field projection belongs to this open window through program waits and close animation.
        _fieldTextProjection = current.Story.Wait is FieldTextWait ||
            (_fieldTextProjection && current.Story.TextWindow is OpenTextWindow && ReferenceEquals(current.Story.TextWindow, _textWindow));
        if (current.Story.Wait is FieldTextWait fieldSpan) _dialogue.Text = fieldSpan.Projection;
        else if (_fieldTextProjection) _dialogue.Text = previousText;
        else if (current.Story.Wait is W1TextWait span)
            _dialogue.Text = string.Concat(session.Definition.Exploration.TextTokens[span.Text].Take(span.EndToken).Select(part => part.Kind switch
            {
                ExplorationTextTokenKind.Literal => part.Value,
                ExplorationTextTokenKind.Newline => "\n",
                ExplorationTextTokenKind.MemberName => names[part.Member!.Value],
                _ => "",
            }));
        else
        {
            // Compatibility display for consumers outside the admitted W1 contract.
            _dialogue.Text = Regex.Replace(_dialogue.Text.Replace("{N}", "\n"), @"\{W[0-9]+\}", "");
            _dialogue.Text = Regex.Replace(_dialogue.Text, @"\{(?:LEADER|NAME;([0-9]+))\}", match =>
            {
                int member = match.Groups[1].Success ? int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : 0;
                return member < names.Count ? names[member] : match.Value;
            });
        }
        // A sound wait can become an input wait on the same already-visible text window.
        // Restart reveal only for changed text or a newly opened window, not that wait handoff.
        bool retainedQuestion = current.Story.Wait is ChoiceWait { Work: null } && _dialogue.Text == previousText;
        if (!retainedQuestion && (_dialogue.Text != previousText || !ReferenceEquals(current.Story.TextWindow, _textWindow)))
        {
            bool continuingSpan = current.Story.Wait is (W1TextWait or FieldTextWait) && ReferenceEquals(current.Story.TextWindow, _textWindow) &&
                _dialogue.Text.StartsWith(previousText, StringComparison.Ordinal);
            Revealed = continuingSpan ? previouslyVisible : 0;
            _dialogue.VisibleCharacters = current.Story.EntrySkip is not null || input.Settings.TextMode == "instant" ? -1 : (int)Revealed;
        }
        bool continuingProjection = ReferenceEquals(current.Story.TextWindow, _textWindow) &&
            current.Story.Wait is W1TextWait or FieldTextWait && _dialogue.Text.StartsWith(previousText, StringComparison.Ordinal);
        if (!ReferenceEquals(current.Story.TextWindow, _textWindow) && current.Story.TextWindow is OpenTextWindow ||
            _dialogue.Text != previousText && !continuingProjection)
            Panel.ResetReading();
        _textWindow = current.Story.TextWindow;
        bool portrait = session.Definition.Exploration?.Visuals is not null &&
            current.Story.PortraitWindow is OpenPortraitWindow or UnknownPortraitWindow { LegacySpeaker: not null };
        byte flags = current.Story.PortraitWindow switch
        { OpenPortraitWindow open => open.Flags, UnknownPortraitWindow unknown => unknown.LegacyFlags, _ => 0 };
        bool sourceChoice = current.Story.Wait is ChoiceWait { Work: not null };
        var choice = (current.Story.Wait as ChoiceWait)?.Work;
        Panel.Choices.Present(current.Story.Wait is ChoiceWait && (choice is null || choice.Visible) &&
            current.Story.LogicalView?.HideWindows != true, choice?.Yes ?? true, sourceChoice);
        Rect2 subject = default;
        var world = current.Exploration;
        var speaker = (current.Story.TextWindow as OpenTextWindow)?.Speaker;
        ExplorationEntity? actor = speaker is { } id && world?.TryResolveEntity(id, out var speaking) == true ? speaking :
            current.Story.CameraEntitySlot is { } slot ? world?.AllEntities.FirstOrDefault(entity => entity.Slot == slot) : world?.PlayerEntity;
        if (actor is not null)
            subject = new Rect2((new Vector2(actor.Motion.X / 16f, actor.Motion.Y / 16f) - viewport.Origin) * WorldViewportLayout.PixelScale,
                Vector2.One * WorldViewportLayout.CellSize);
        bool openWindow = current.Story.TextWindow is OpenTextWindow || current.Story.PortraitWindow is OpenPortraitWindow ||
            current.Story.Wait is DialogueWait or ChoiceWait;
        Panel.Present(openWindow && current.Story.LogicalView?.HideWindows != true, portrait, (flags & 0x80) != 0, subject, "", openWindow);
        RefreshHint(session, input, battleMounted, revealed);
    }
    internal void RefreshHint(GameSession session, GameInput input, bool battleMounted, bool revealed)
    {
        var current = session.Current;
        bool atInput = current.Story.Wait is DialogueWait or W1TextWait { AtInput: true, Revealed: true } || current.CanWaitForText;
        string hint = current.Story.Wait switch
        {
            ChoiceWait { Work: not null } => $"{input.Hint(GameAction.Left)} / {input.Hint(GameAction.Right)}: Select    {input.Hint(GameAction.Confirm)}: Confirm    {input.Hint(GameAction.Cancel)}: No",
            ChoiceWait => $"{input.Hint(GameAction.Confirm)}: Yes    {input.Hint(GameAction.Cancel)}: No",
            _ when Panel.Visible && !revealed => $"{input.Hint(GameAction.Confirm)}: Reveal",
            _ when Panel.Visible && atInput => $"{input.Hint(GameAction.Confirm)}: Continue ▾",
            _ when current.CanWaitAtInput => (session.QueryExplorationInteraction() is not null ? $"{input.Hint(GameAction.Confirm)}: Talk    " : "") + $"Hold {input.Hint(GameAction.Wait)}: Wait",
            _ => "Scene in progress…",
        };
        if (Panel.Visible && Panel.MoreBelow && revealed) hint = $"{input.Hint(GameAction.Confirm)}: Read more";
        if (current.Story.Wait is ChoiceWait { Work: null } && !revealed) hint = $"{input.Hint(GameAction.Confirm)}: Reveal";
        if (BattleEntrySkip.Eligible(current)) hint += current.Story.EntrySkip is null
            ? $"    {input.Hint(GameAction.Cancel)}: Skip battle intro" : "    Skipping battle intro…";
        _context.Text.Text = hint;
        _context.Arrange(GetViewportRect().Size);
        _context.Visible = !Panel.Visible && (!battleMounted || BattleEntrySkip.Eligible(current));
        Panel.SetHint(hint);
    }
}
