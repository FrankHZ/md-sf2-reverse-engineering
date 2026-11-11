using System.Text.RegularExpressions;
using Sf2.Remake.Content.Localization;
using Godot;
using Sf2.Remake.GodotAdapter.Localization;
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
    internal StoryDialoguePanel Panel { get; } = new() { Name = "DialoguePanel", JourneyChoices = true };
    private ContextHint _context = new() { Name = "ContextHint" };
    internal Label ActiveHint => Panel.Visible ? Panel.Hint : _context.Text;
    private Label _dialogue => Panel.Body;
    private TextWindow? _textWindow;
    private bool _fieldTextProjection;
    private FieldTextWait? _displaySpan;
    private byte _portraitFlags;
    internal string EffectiveLocale { get; private set; } = "en";
    internal string? FallbackReason { get; private set; }
    internal double Revealed { get; set; }
    internal string ReturnNotice { get; set; } = "";
    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Ignore;
        AddChild(Panel); AddChild(_context);
        _dialogue.AutoTranslateMode = AutoTranslateModeEnum.Disabled;
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
        bool retainedQuestionWindow = current.Story.Wait is ChoiceWait &&
            (current.Story.TextWindow is not OpenTextWindow || ReferenceEquals(current.Story.TextWindow, _textWindow));
        if (!_fieldTextProjection && !retainedQuestionWindow)
        {
            EffectiveLocale = "en";
            FallbackReason = current.Story.TextWindow is OpenTextWindow || current.Story.Wait is DialogueWait or W1TextWait or FieldSearchWait
                ? "unmigrated-consumer" : null;
        }
        // A choice attached to this window must not reconstruct the question from raw source
        // and undo its locale/reflow. The existing choice controller still owns confirmation.
        if (retainedQuestionWindow) _dialogue.Text = previousText;
        else if (current.Story.Wait is FieldSearchWait search)
        {
            var display = GameText.Search(search.Result, session);
            _dialogue.Text = display.Text; EffectiveLocale = display.EffectiveLocale; FallbackReason = display.Reason;
        }
        else if (current.Story.Wait is FieldTextWait fieldSpan)
        {
            var display = GameText.Field(fieldSpan, session.Definition.Exploration);
            _dialogue.Text = display.Text; EffectiveLocale = display.EffectiveLocale; FallbackReason = display.Reason;
        }
        else if (_fieldTextProjection) _dialogue.Text = previousText;
        else if (current.Story.Wait is DialogueWait dialogue)
        {
            var display = GameText.Dialogue(dialogue, session);
            _dialogue.Text = display.Text; EffectiveLocale = display.EffectiveLocale; FallbackReason = display.Reason;
        }
        else if (current.Story.Wait is W1TextWait span)
            _dialogue.Text = string.Concat(session.Definition.Exploration.TextTokens[span.Text].Take(span.EndToken).Select((part, index) => part.Kind switch
            {
                ExplorationTextTokenKind.Literal => part.Value,
                ExplorationTextTokenKind.Newline => SourceTextLayout.Break(session.Definition.Exploration.TextTokens[span.Text], index, "en"),
                ExplorationTextTokenKind.MemberName => CharacterNames.Member(session.Definition, current.Story, part.Member!.Value),
                _ => "",
            }));
        else
        {
            // Compatibility display for consumers outside the admitted W1 contract.
            _dialogue.Text = Regex.Replace(_dialogue.Text.Replace("{N}", "\n").Replace("{CLEAR}", "\n"), @"\{W[0-9]+\}", "");
            _dialogue.Text = Regex.Replace(_dialogue.Text, @"\{(?:LEADER|NAME|NAME;([0-9]+))\}", match =>
            {
                int member = match.Groups[1].Success ? int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : 0;
                return member < names.Count ? CharacterNames.Member(session.Definition, current.Story, member) : match.Value;
            });
        }
        // A sound wait can become an input wait on the same already-visible text window.
        // Restart reveal only for changed text or a newly opened window, not that wait handoff.
        bool retainedQuestion = current.Story.Wait is ChoiceWait { Work: null } && _dialogue.Text == previousText;
        bool newFieldSpan = current.Story.Wait is FieldTextWait newSpan && (_displaySpan?.Token != newSpan.Token ||
            _displaySpan.DisplayStart != newSpan.DisplayStart);
        bool fieldContinuity = current.Story.Wait is FieldTextWait continuous && _displaySpan is { } prior &&
            ReferenceEquals(current.Story.TextWindow, _textWindow) && continuous.Text == prior.Text &&
            continuous.DisplayStart == prior.DisplayStart && continuous.DisplayEnd >= prior.DisplayEnd;
        if (!retainedQuestion && (newFieldSpan || _dialogue.Text != previousText || !ReferenceEquals(current.Story.TextWindow, _textWindow)))
        {
            bool continuingSpan = fieldContinuity || current.Story.Wait is W1TextWait && ReferenceEquals(current.Story.TextWindow, _textWindow) &&
                _dialogue.Text.StartsWith(previousText, StringComparison.Ordinal);
            Revealed = continuingSpan ? previouslyVisible : 0;
            _dialogue.VisibleCharacters = current.Story.EntrySkip is not null || input.Settings.TextMode == "instant" ? -1 : (int)Revealed;
        }
        bool continuingProjection = fieldContinuity || ReferenceEquals(current.Story.TextWindow, _textWindow) &&
            current.Story.Wait is W1TextWait && _dialogue.Text.StartsWith(previousText, StringComparison.Ordinal);
        if (!ReferenceEquals(current.Story.TextWindow, _textWindow) && current.Story.TextWindow is OpenTextWindow ||
            _dialogue.Text != previousText && !continuingProjection)
            Panel.ResetReading();
        if (current.Story.Wait is FieldTextWait displayed) _displaySpan = displayed;
        else if (!_fieldTextProjection) _displaySpan = null;
        bool portrait = session.Definition.Exploration?.Visuals is not null &&
            current.Story.PortraitWindow is OpenPortraitWindow or UnknownPortraitWindow { LegacySpeaker: not null };
        byte flags = current.Story.PortraitWindow switch
        { OpenPortraitWindow open => open.Flags, UnknownPortraitWindow unknown => unknown.LegacyFlags, _ => 0 };
        if (portrait) _portraitFlags = flags;
        // Source portrait and text close waits are independent. Keep the last mounted portrait
        // with this text window until the composed panel exits; a new window must not inherit it.
        else if (current.Story.TextWindow is OpenTextWindow && ReferenceEquals(current.Story.TextWindow, _textWindow) &&
            Panel.Portrait.Texture is not null)
        { portrait = true; flags = _portraitFlags; }
        else Panel.Portrait.Texture = null;
        _textWindow = current.Story.TextWindow;
        bool sourceChoice = current.Story.Wait is ChoiceWait { Work: not null };
        var choice = (current.Story.Wait as ChoiceWait)?.Work;
        Panel.Choices.Present(current.Story.Wait is ChoiceWait && (choice is null || choice.Visible) &&
            current.Story.LogicalView?.HideWindows != true, choice?.Yes ?? true,
            revealed && !Panel.MoreBelow && (choice is null || current.CanWaitForChoice), sourceChoice);
        Rect2 subject = default;
        var world = current.Exploration;
        var speaker = (current.Story.TextWindow as OpenTextWindow)?.Speaker;
        ExplorationEntity? actor = speaker is { } id && world?.TryResolveEntity(id, out var speaking) == true ? speaking :
            current.Story.CameraEntitySlot is { } slot ? world?.AllEntities.FirstOrDefault(entity => entity.Slot == slot) : world?.PlayerEntity;
        if (actor is not null)
            subject = new Rect2((new Vector2(actor.Motion.X / 16f, actor.Motion.Y / 16f) - viewport.Origin) * WorldViewportLayout.PixelScale,
                Vector2.One * WorldViewportLayout.CellSize);
        bool openWindow = current.Story.TextWindow is OpenTextWindow || current.Story.PortraitWindow is OpenPortraitWindow ||
            current.Story.Wait is DialogueWait or ChoiceWait or FieldSearchWait;
        Panel.Present(openWindow && current.Story.LogicalView?.HideWindows != true, portrait, (flags & 0x80) != 0, subject, "", openWindow);
        RefreshHint(session, input, battleMounted, revealed);
    }
    internal void RefreshHint(GameSession session, GameInput input, bool battleMounted, bool revealed)
    {
        // Present may have just installed a new span after its caller sampled the old reader.
        revealed = _dialogue.VisibleCharacters < 0 || _dialogue.VisibleCharacters >= _dialogue.GetTotalCharacterCount();
        var current = session.Current;
        bool atInput = current.Story.Wait is DialogueWait or FieldSearchWait or W1TextWait { AtInput: true, Revealed: true } || current.CanWaitForText;
        string hint = current.Story.Wait switch
        {
            FieldSearchWait when revealed => GameText.Get("search.continue", input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Cancel)),
            ChoiceWait when revealed && current.CanWaitForChoice => GameText.Get("story.choice-help", input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Cancel)),
            DialogueWait or FieldTextWait or W1TextWait when revealed && !Panel.MoreBelow => GameText.Get("story.continue", input.CompactHint(GameAction.Confirm)),
            _ when Panel.Visible && !revealed => GameText.Get(
                (current.Story.Wait is DialogueWait or FieldTextWait or W1TextWait) && !Panel.MoreBelow
                    ? "field.reveal-continue" : "field.reveal", input.CompactHint(GameAction.Confirm)),
            _ when Panel.Visible && atInput => GameText.Get("story.continue", input.CompactHint(GameAction.Confirm)),
            _ when current.CanWaitAtInput => session.QueryExplorationInteraction() is not null
                ? GameText.Get("field.talk", input.CompactHint(GameAction.Confirm))
                : session.CanSearch ? GameText.Get("field.search", input.CompactHint(GameAction.Confirm)) : "",
            _ => GameText.Get("story.in-progress"),
        };
        if (Panel.Visible && Panel.MoreBelow && revealed) hint = GameText.Get("field.more", input.CompactHint(GameAction.Confirm));
        if (current.Story.Wait is ChoiceWait { Work: null } && !revealed) hint = GameText.Get("field.reveal", input.CompactHint(GameAction.Confirm));
        if (BattleEntrySkip.Eligible(current)) hint += current.Story.EntrySkip is null
            ? "    " + GameText.Get("story.skip", input.CompactHint(GameAction.Cancel)) : "    " + GameText.Get("story.skipping");
        if (!Panel.Visible && ReturnNotice.Length > 0) hint = ReturnNotice + "\n" + hint;
        if (current.Story.Wait is FieldSearchWait && GameText.Language == "zh-CN")
        {
            hint += "\n" + GameText.Get("localization.names");
            if (FallbackReason is { } reason) hint += " " + GameText.Get("localization.fallback", reason);
        }
        else if (Panel.Visible && GameText.Language == "zh-CN" && FallbackReason is { } bodyReason)
            hint += "\n" + GameText.Get("localization.fallback", bodyReason);
        _context.Text.Text = hint;
        _context.Arrange(GetViewportRect().Size);
        _context.Visible = !Panel.Visible && (!battleMounted || BattleEntrySkip.Eligible(current));
        Panel.SetHint(hint);
    }
}
