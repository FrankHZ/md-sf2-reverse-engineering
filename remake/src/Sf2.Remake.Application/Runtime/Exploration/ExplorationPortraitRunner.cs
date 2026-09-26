using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class ExplorationPortraitRunner
{
    internal static bool Admitted(StoryState story) => story.PortraitWindow is ClosedPortraitWindow ||
        story.EventCaller is not null && story.PortraitWindow is OpenPortraitWindow { Work: { Moving: false, Registered: true } };

    internal static SessionSnapshot Open(ScenarioDefinition definition, SessionSnapshot current, EntityRef actor, byte flags,
        List<SessionObservation> observations, bool entry)
    {
        if (current.Story.EventCaller is null || current.Story.PortraitWindow is UnknownPortraitWindow)
            throw new BattleRuleException("field-portrait-context", "story.portrait", true);
        if (current.Story.PortraitWindow is OpenPortraitWindow existing)
        {
            if (existing.Work is not { Moving: false, Registered: true })
                throw new BattleRuleException("field-portrait-state", "story.portrait", true);
            return current; // OpenPortraitWindow does not replace an existing window or counters.
        }
        var entity = ProgramRunner.Entity(current, actor);
        if (entity.Sprite is not { } sprite || definition.Exploration!.Visuals is not { } visuals ||
            !visuals.Sprites.TryGetValue(sprite, out var visual))
            throw new BattleRuleException("field-portrait-speaker", "program.portrait", true);
        if (visual.Portrait is not { } portrait) return current;
        if (!visuals.Portraits.TryGetValue(portrait, out var asset) || asset.Eyes is null || asset.Mouth is null)
            throw new BattleRuleException("field-portrait-mapping", "world.portrait", true);
        var story = current.Story.Copy(current.Story.Cursor,
            new PortraitMovementWait(new(current.ObservationSequence + 1), false, entry),
            portraitWindow: new OpenPortraitWindow(portrait, flags, new()));
        return ProgramRunner.Commit(current, current.Active, story, observations, "portrait-window-moving", "open");
    }

    internal static SessionSnapshot Close(SessionSnapshot current, List<SessionObservation> observations, bool returning)
    {
        if (current.Story.PortraitWindow is ClosedPortraitWindow) return current;
        if (current.Story.EventCaller is null || current.Story.PortraitWindow is not OpenPortraitWindow { Work: { } work } portrait)
            throw new BattleRuleException("field-portrait-context", "story.portrait", true);
        // Source removes the service before starting the close movement.
        return ProgramRunner.Commit(current, current.Active, current.Story.Copy(current.Story.Cursor,
            new PortraitMovementWait(new(current.ObservationSequence + 1), true, returning),
            portraitWindow: portrait with { Work = work with { Registered = false, Movement = 0, Moving = true, Closing = true } }),
            observations, "portrait-window-moving", "close");
    }

    // Runs after the admitted entity/view/scroll/window services, in the source's added slot.
    internal static SessionSnapshot Service(SessionSnapshot current, List<SessionObservation> observations)
    {
        if (current.Story.PortraitWindow is not OpenPortraitWindow { Work: { } work } portrait) return current;
        if (work.Moving)
            work = work with { Moving = work.Movement < 4, Movement = Math.Min(4, work.Movement + 1) };
        if (work.Registered)
        {
            work = work with { Blink = unchecked((short)(work.Blink - 1)) };
            if (work.Blink == 3) work = work with { EyesClosed = true };
            if (work.Blink == 0) work = work with { EyesClosed = false, Blink = (short)(Draw(120, "blink") + 30) };
            if (current.Story.Typewriting)
            {
                work = work with { Mouth = unchecked((short)(work.Mouth - 1)) };
                if (work.Mouth == 5) work = work with { MouthOpen = true };
            }
            if (current.Story.Typewriting ? work.Mouth == 0 : work.Mouth <= 5)
                work = work with { MouthOpen = false, Mouth = (short)(Draw(5, "mouth") + 10) };
        }
        return current.WithStory(current.Story.Copy(current.Story.Cursor, current.Story.Wait,
            portraitWindow: portrait with { Work = work }));

        ushort Draw(ushort range, string kind)
        {
            var world = current.Exploration!;
            var party = world.Party;
            var draw = BattleRandom.NextMain(party.MainSeed, range);
            current = ProgramRunner.Commit(current, new ActiveExploration(world.WithParty(
                new(party.Encounter, party.Actors, draw.After, party.ThinkingSeed, party.Gold, party.NewBattle))),
                current.Story, observations, "rng-portrait-" + kind);
            observations[^1] = observations[^1] with { Before = draw.Before, After = draw.After, RandomRange = range, RandomValue = draw.Value };
            return draw.Value;
        }
    }

    internal static SessionSnapshot Advance(SessionSnapshot current, List<SessionObservation> observations)
    {
        var wait = (PortraitMovementWait)current.Story.Wait!;
        var portrait = (OpenPortraitWindow)current.Story.PortraitWindow;
        if (portrait.Work!.Moving) return current;
        var story = current.Story.Copy(current.Story.Cursor, portraitWindow: wait.Closing
            ? new ClosedPortraitWindow() : portrait with { Work = portrait.Work with { Registered = true } });
        current = ProgramRunner.Commit(current, current.Active, story, observations,
            wait.Closing ? "portrait-closed" : "portrait-service-registered");
        if (wait.CallerBoundary)
            return wait.Closing ? MapEventDispatcher.Finish(current, observations) : MapEventDispatcher.EnterBody(current, observations);
        return current.WithStory(current.Story.Copy(ProgramRunner.Next(current.Story.Cursor!.Value)));
    }
}
