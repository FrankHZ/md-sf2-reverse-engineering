using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.Application.Runtime.Exploration;

// csc11 / YesNoPrompt in the normal, gold-absent field profile. Raw controller
// repeat ancestry is outside the admitted fresh conditioned semantic input stream.
internal static class ExplorationChoiceRunner
{
    internal static SessionSnapshot Held(SessionSnapshot current, bool held, List<SessionObservation> observations)
    {
        var wait = (ChoiceWait)current.Story.Wait!;
        var work = wait.Work!;
        if (work.Phase == ChoicePhase.Entry)
        {
            current = Save(current, work with { Phase = ChoicePhase.Opening, EntryHeld = held }, observations, "choice-opened");
            return MoveWindows(current, observations, "open");
        }
        return Save(current, work with { Phase = held ? ChoicePhase.Release : ChoicePhase.Input }, observations,
            held ? "choice-held" : "choice-released");
    }

    internal static SessionSnapshot Poll(ScenarioDefinition definition, SessionSnapshot current, ChoiceInput input,
        List<SessionObservation> observations)
    {
        var work = ((ChoiceWait)current.Story.Wait!).Work!;
        bool left = (input & ChoiceInput.Left) != 0, right = (input & ChoiceInput.Right) != 0;
        if (left || right)
        {
            work = work.Yes ? work with { YesAlternate = false } : work with { NoAlternate = false };
            work = work with { Yes = left, Counter = 19 };
            current = Save(current, work, observations, "choice-selected", left ? "yes" : "no");
            current = Sound(current, observations, 66);
            current = current.WithStory(RefreshDialogue(current.Story, 1, settledOnly: true));
            // SetWindowDestination(8080) refreshes the settled own window too.
            work = work with { OriginX = work.X, DestinationX = work.X, Length = 1, Movement = 0 };
        }
        else if ((input & (ChoiceInput.Cancel | ChoiceInput.ConfirmC | ChoiceInput.ConfirmA)) != 0)
        {
            bool yes = (input & ChoiceInput.Cancel) == 0 && work.Yes;
            current = Save(current, work with { Phase = ChoicePhase.Closing, Yes = yes,
                OriginX = work.X, DestinationX = -16, Length = 4, Movement = 0 }, observations,
                "choice-accepted", yes ? "yes" : "no");
            return MoveWindows(current, observations, "close");
        }
        bool alternate = work.Counter >= 10;
        work = work.Yes ? work with { YesAlternate = alternate } : work with { NoAlternate = alternate };
        work = work with { Counter = work.Counter == 1 ? 20 : work.Counter - 1 };
        current = Save(current, work, observations, "choice-poll", input.ToString());
        return ExplorationDispatcher.Service(definition, current, observations, "choice-poll-service");
    }

    // Called at the window stage, before the registered portrait service.
    internal static StoryState Windows(StoryState story)
    {
        if (story.Wait is not ChoiceWait { Work: { Visible: true } work } wait) return story;
        bool moving = work.Movement < work.Length;
        work = work with { Moving = moving, Movement = moving ? work.Movement + 1 : work.Movement };
        return story.Copy(story.Cursor, wait with { Work = work });
    }

    internal static StoryState FixPosition(StoryState story)
    {
        if (story.Wait is not ChoiceWait { Work: { Visible: true } work } wait || work.X != work.DestinationX) return story;
        return story.Copy(story.Cursor, wait with { Work = work with
        { OriginX = work.X, DestinationX = work.X, Length = 1, Movement = 0 } });
    }

    internal static SessionSnapshot Advance(SessionSnapshot current, List<SessionObservation> observations)
    {
        var wait = (ChoiceWait)current.Story.Wait!;
        var work = wait.Work!;
        if (work.Phase == ChoicePhase.ReturnDelay)
        {
            if (work.Remaining > 1)
                return Save(current, work with { Remaining = work.Remaining - 1 }, observations, "choice-return-delay");
            return ProgramRunner.Commit(current, current.Active, current.Story.Copy(ProgramRunner.Next(current.Story.Cursor!.Value)),
                observations, "choice-returned", work.Yes ? "yes" : "no");
        }
        if (ExplorationTextRunner.WindowsMoving(current.Story))
            return current;
        if (work.Phase == ChoicePhase.Opening)
            return Save(current, work with { Phase = work.EntryHeld ? ChoicePhase.Release : ChoicePhase.Input }, observations, "choice-open-completed");
        current = Save(current, work with { Phase = ChoicePhase.ReturnDelay, Remaining = 10 }, observations, "choice-deleted");
        return ProgramRunner.Commit(current, current.Active, current.Story.Copy(current.Story.Cursor, current.Story.Wait,
            flags: ProgramRunner.Flags(current.Story, wait.ResultFlag, work.Yes)), observations, "choice-result-flag", wait.ResultFlag.ToString());
    }

    private static SessionSnapshot MoveWindows(SessionSnapshot current, List<SessionObservation> observations, string direction)
    {
        current = Sound(current, observations, 65);
        if (current.Story.LogicalText is { Open: true })
        {
            current = current.WithStory(RefreshDialogue(current.Story, 4, settledOnly: false));
            current = Sound(current, observations, 65);
        }
        return ProgramRunner.Commit(current, current.Active, current.Story, observations, "choice-window-moving", direction);
    }

    private static StoryState RefreshDialogue(StoryState story, int length, bool settledOnly)
    {
        if (story.LogicalText is not { Open: true } window || settledOnly && window.WindowY != window.DestinationY) return story;
        return story.Copy(story.Cursor, story.Wait, logicalText: ExplorationTextRunner.Refresh(window, length));
    }

    private static SessionSnapshot Save(SessionSnapshot current, ChoiceWork work, List<SessionObservation> observations, string kind, string? detail = null) =>
        ProgramRunner.Commit(current, current.Active, current.Story.Copy(current.Story.Cursor,
            ((ChoiceWait)current.Story.Wait!) with { Work = work }), observations, kind, detail);

    private static SessionSnapshot Sound(SessionSnapshot current, List<SessionObservation> observations, int command) =>
        ProgramRunner.Commit(current, current.Active, current.Story, observations, "choice-sound", command.ToString());
}
