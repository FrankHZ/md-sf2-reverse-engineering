using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class ExplorationTextRunner
{
    internal static StoryState Initialize(ExplorationState world, StoryState story)
    {
        if (story.TextSettings is not { } settings) return story;
        if (settings.MessageSpeed > 3 || settings.ViewSpeed != 0)
            throw new BattleRuleException("field-text-settings", "start.textSettings", true);
        return story.Copy(story.Cursor, story.Wait, portraitWindow: new ClosedPortraitWindow(),
            logicalText: new(), logicalView: ExplorationViewRunner.Initialize(world));
    }

    internal static StoryState AfterEntities(ExplorationState world, StoryState story)
    {
        if (story.TextSettings is null) return story;
        var view = story.LogicalView is { } currentView ? ExplorationViewRunner.Tick(world, currentView, story.TextSettings) : null;
        var window = story.LogicalText;
        if (window is { Open: true })
        {
            bool moving = window.AnimationCounter < window.AnimationLength;
            window = window with { Moving = moving, AnimationCounter = moving ? window.AnimationCounter + 1 : window.AnimationCounter };
        }
        return story.Copy(story.Cursor, story.Wait, logicalText: window, logicalView: view);
    }

    internal static int TypewriteDelay(ExplorationTextSettings settings, byte logicalInput) =>
        settings.MouthControl == 0 && logicalInput != 0 ? 0 : settings.MessageSpeed switch
        { 0 => 4, 1 => 2, 2 => 1, 3 => 0, _ => throw new BattleRuleException("field-text-speed", "story.textSettings", true) };

    internal static void ValidateContext(SessionSnapshot current)
    {
        if (current.Story is not { TextSettings: not null, LogicalText: not null, LogicalView: not null,
                PortraitWindow: ClosedPortraitWindow, EnteringBattle: null, Continuation: ProgramContinuation.FieldInput } || current.Exploration is null)
            throw new BattleRuleException("field-text-context", "story.text", true);
    }

    internal static StoryState Begin(ExplorationDefinition definition, SessionSnapshot current, ShowText text, WaitToken token)
    {
        ValidateContext(current);
        var speaker = text.UseEventSpeaker ? current.Story.EntityEvent?.Entity : text.Speaker;
        if (speaker is not { } actor || !current.Exploration!.TryResolveEntity(actor, out var entity) || entity.Sprite is not { } sprite ||
            definition.Visuals is not { } visuals || !visuals.Sprites.TryGetValue(sprite, out var visual) || visual.Portrait is not null)
            throw new BattleRuleException("field-text-speaker", "program.text", true);
        if (definition.TextFont is not { } font || !definition.TextTokens.TryGetValue(current.Story.TextCursor, out var tokens))
            throw new BattleRuleException("field-text-font", "world.textFont", true);
        var units = new List<FieldTextUnit>();
        foreach (var part in tokens)
        {
            if (part.Kind is ExplorationTextTokenKind.Wait1 or ExplorationTextTokenKind.Wait2 or ExplorationTextTokenKind.Newline)
            { units.Add(new(part.Kind, part.Kind == ExplorationTextTokenKind.Newline ? "\n" : "")); continue; }
            string value = part.Kind switch
            {
                ExplorationTextTokenKind.Literal => part.Value,
                ExplorationTextTokenKind.MemberName when part.Member is >= 0 && part.Member < definition.MemberNames.Count => definition.MemberNames[part.Member.Value],
                _ => throw new BattleRuleException("field-text-token", "program.text", true),
            };
            foreach (char character in value)
            {
                if (character > 255 || font.AsciiToSymbol[character] is not (> 0 and <= 80))
                    throw new BattleRuleException("field-text-symbol", "program.text", true);
                byte symbol = font.AsciiToSymbol[character];
                units.Add(new(ExplorationTextTokenKind.Literal, character.ToString(), symbol, font.Advances[symbol - 1]));
            }
        }
        var wait = Span(token, current.Story.TextCursor, units.AsReadOnly(), 0,
            current.Story.LogicalText!.Open ? FieldTextPhase.Tokens : FieldTextPhase.ClearFirst);
        var story = current.Story.Copy(current.Story.Cursor, wait, textCursor: checked(current.Story.TextCursor + 1),
            textWindow: new OpenTextWindow(current.Story.TextCursor, text.Mode, speaker, text.SpeakerFlags));
        return Normalize(story);
    }

    private static FieldTextWait Span(WaitToken token, int text, IReadOnlyList<FieldTextUnit> units, int start,
        FieldTextPhase phase, bool firstGlyph = true)
    {
        int end = start;
        while (end < units.Count && units[end].Kind is not (ExplorationTextTokenKind.Wait1 or ExplorationTextTokenKind.Wait2)) end++;
        return new(token, text, units, start, end, string.Concat(units.Take(end).Select(unit => unit.Text)), phase, FirstGlyph: firstGlyph);
    }

    // Consume only zero-opportunity source operations until the next actual wait/input boundary.
    internal static StoryState Normalize(StoryState story)
    {
        var wait = (FieldTextWait)story.Wait!;
        var window = story.LogicalText!;
        while (wait.Phase == FieldTextPhase.Tokens)
        {
            if (wait.Index == wait.End)
            {
                wait = wait with { Phase = wait.End == wait.Units.Count ? FieldTextPhase.End : FieldTextPhase.Input };
                if (wait.Wait2) window = window with { Indicator = 20, IndicatorVisible = false };
                break;
            }
            var unit = wait.Units[wait.Index];
            bool newline = unit.Kind == ExplorationTextTokenKind.Newline;
            bool wrap = newline || window.X > 204 || wait.FirstGlyph && window.X != 2;
            if (!newline) wait = wait with { FirstGlyph = false };
            if (wrap)
            {
                window = window with { X = 2, Y = unchecked((byte)(window.Y + 16)) };
                if (newline) wait = wait with { Index = wait.Index + 1 };
                if (window.Y >= 48)
                { wait = wait with { Phase = FieldTextPhase.Scroll, Remaining = 3 }; break; }
                if (newline) continue;
            }
            window = window with { X = unchecked((byte)(window.X + unit.Advance)) };
            wait = wait with { Index = wait.Index + 1, Phase = FieldTextPhase.GlyphCursor };
        }
        story = story.Copy(story.Cursor, wait, logicalText: window);
        return FinishDelivery(story);
    }

    internal static StoryState FinishDelivery(StoryState story) => story.Wait is FieldTextWait { Phase: FieldTextPhase.End, Revealed: true }
        ? story.Copy(story.Cursor is { } cursor ? ProgramRunner.Next(cursor) : null) : story;

    internal static StoryState BeforeService(StoryState story)
    {
        if (story.Wait is FieldTextWait { Phase: FieldTextPhase.Scroll, Remaining: > 1 })
            return story.Copy(story.Cursor, story.Wait, logicalText: story.LogicalText! with { Row = (story.LogicalText.Row + 1) % 6 });
        return story;
    }

    internal static StoryState AfterService(StoryState story)
    {
        if (story.Wait is ViewWait view)
        {
            if (view.FinalService) return story.Copy(ProgramRunner.Next(story.Cursor!.Value));
            return story.Copy(story.Cursor, view.Recheck
                ? view with { FinalService = !story.LogicalView!.Scrolling, Recheck = false }
                : view with { Recheck = !story.LogicalView!.Scrolling });
        }
        if (story.Wait is TextCloseWait close)
            return story.LogicalText!.Moving ? story : story.Copy(story.Cursor is { } closeCursor ? ProgramRunner.Next(closeCursor) : null,
                clearEntityEvent: close.EntityEventReturn,
                textWindow: new ClosedTextWindow(), logicalText: story.LogicalText with { Open = false, Indicator = 0, IndicatorVisible = false });
        var wait = (FieldTextWait)story.Wait!;
        var window = story.LogicalText!;
        switch (wait.Phase)
        {
            case FieldTextPhase.ClearFirst: wait = wait with { Phase = FieldTextPhase.ClearSecond }; break;
            case FieldTextPhase.ClearSecond:
                window = new(Open: true, AnimationLength: 8);
                wait = wait with { Phase = FieldTextPhase.Opening, Remaining = 8 }; break;
            case FieldTextPhase.Opening:
                wait = wait with { Remaining = wait.Remaining - 1, Phase = wait.Remaining == 1 ? FieldTextPhase.Tokens : wait.Phase }; break;
            case FieldTextPhase.GlyphCursor:
                int delay = TypewriteDelay(story.TextSettings!, 0);
                wait = wait with { Phase = delay == 0 ? FieldTextPhase.Tokens : FieldTextPhase.GlyphDelay, Remaining = delay }; break;
            case FieldTextPhase.GlyphDelay:
                wait = wait with { Remaining = wait.Remaining - 1, Phase = wait.Remaining == 1 ? FieldTextPhase.Tokens : wait.Phase }; break;
            case FieldTextPhase.Scroll:
                if (wait.Remaining == 1) window = window with { Y = unchecked((byte)(window.Y - 16)) };
                wait = wait with { Remaining = wait.Remaining - 1, Phase = wait.Remaining == 1 ? FieldTextPhase.Tokens : wait.Phase }; break;
            default: throw new BattleRuleException("field-text-not-mandatory", "story.text");
        }
        return Normalize(story.Copy(story.Cursor, wait, logicalText: window));
    }

    internal static StoryState Accepted(StoryState story, WaitToken nextToken)
    {
        var wait = (FieldTextWait)story.Wait!;
        int next = wait.End + 1;
        story = story.Copy(story.Cursor, logicalText: story.LogicalText! with { Indicator = 0, IndicatorVisible = false });
        if (next == wait.Units.Count) return story.Copy(ProgramRunner.Next(story.Cursor!.Value));
        return Normalize(story.Copy(story.Cursor, Span(nextToken, wait.Text, wait.Units, next, FieldTextPhase.Tokens, wait.FirstGlyph)));
    }
}
