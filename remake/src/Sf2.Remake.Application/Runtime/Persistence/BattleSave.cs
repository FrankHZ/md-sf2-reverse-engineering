using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Persistence;

internal static class BattleSave
{
    internal static BattleSaveAvailability Query(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules,
        ObserveBattleMusic? observedMusic = null)
    {
        // Menu eligibility can validate the live adapter observation without publishing it.
        // Actual capture still requires a committed observation on the authoritative session.
        if (observedMusic is not null)
        {
            var observed = BattleMusicObservation.Submit(definition, current, rules, observedMusic);
            if (observed.Failure is { } failure) return new(current.SessionId, current.Revision, null, failure.Code);
            var offer = Query(definition, observed.Snapshot, rules);
            return offer with { Revision = current.Revision };
        }
        try
        {
            var data = CaptureValues(definition, current, rules);
            _ = BattleRestore.Build(definition, data, rules);
            return new(current.SessionId, current.Revision,
                new(data.Battle.Encounter, data.Battle.Round, current.Selection!.Actor.Value), null);
        }
        catch (BattleRuleException error) { return new(current.SessionId, current.Revision, null, error.Code); }
    }

    internal static BattleSaveCapture Capture(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules, Guid session, long revision)
    {
        if (current.SessionId != session || current.Revision != revision) return new(null, "save-stale-revision");
        try
        {
            var data = CaptureValues(definition, current, rules);
            _ = BattleRestore.Build(definition, data, rules);
            return new(data, null);
        }
        catch (BattleRuleException error) { return new(null, error.Code); }
    }

    private static BattleSaveData CaptureValues(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules)
    {
        var compatibility = OpeningSave.FieldCompatibility(definition, rules);
        RequireStable(current, rules);
        if (current.Battle.StartPolicy is not { Origin: BattleStartOrigin.NewGame, BeforeBattleRouted: true,
            AlreadyRefreshedAllies: true, RosterOnly: true, SkipIntro: false }) Fail("profile-unavailable");
        var route = BattleSaveStory.Locate(definition, current.Story.EnteringBattle!);
        BattleSaveStory.RequireMembership(definition, current.Story, current.Battle);
        return new(compatibility, 1, route, BattleContinuationValues.Capture(current.Battle), BattleSaveStory.Capture(current.Story));
    }

    internal static void RequireStable(SessionSnapshot current, SessionRules rules)
    {
        if (current.BattleMovement is not null) Fail("movement-busy");
        if (current.Active is not ActiveBattle active || current.StopReason != SessionStopReason.PlayerInput ||
            !current.HasBattleControl || active.Scene is not null || active.Movement is not null || active.Retreat is not null ||
            active.Round is not null || active.Give is not null || active.Discard is not null || active.Outcome is not null)
            Fail("busy");
        var battle = current.Battle;
        if (BattleTurnFlow.AtRoundEnd(battle) || current.Selection is null) Fail("actor-unavailable");
        var actor = BattleTurnFlow.QueuedActor(battle);
        if (actor.Hp == 0 || actor.Position is null || actor.Control != BattleControl.Player || (actor.Status & 0xC1) != 0 ||
            current.Selection!.Actor != actor.Actor || BattleOutcomeSelection.Check(rules.Outcome, battle) is not null)
            Fail("actor-unavailable");
        var selection = current.Selection!;
        if (selection.Stage != BattleSelectionStage.Movement || selection.Action is not null || selection.Spell is not null ||
            selection.Target is not null || selection.ItemSlot is not null || selection.ItemWord is not null ||
            selection.EquipmentSlot is not null || selection.ReturnSlot is not null || selection.ReturnWord is not null)
            Fail("selection-busy");
        if (selection.Preview.Cost != 0 || selection.Preview.Path.Count != 1 || selection.Preview.Path[0] != actor.Position)
            Fail("cancel-movement");
        var story = current.Story;
        if (story.Cursor is not null || story.Callers.Count != 0 || story.Wait is not null || story.EventCaller is not null ||
            story.DefeatedScene is not null || story.Warp is not null || story.ScriptWarp is not null || story.EntrySkip is not null ||
            story.BattleRequest || story.Continuation != ProgramContinuation.FieldInput ||
            story.TextWindow is not ClosedTextWindow || story.PortraitWindow is not ClosedPortraitWindow || story.Typewriting ||
            story.LogicalText is not { Open: false, Moving: false } || story.LogicalView is not { Scrolling: false } ||
            story.Display is not { Visibility: FullFadeVisibility.BaseRestored } display || display.Base != display.Current)
            Fail("story-busy");
        if (story.EnteringBattle?.Outcome is null || story.FieldPlayer is null) Fail("route-unavailable");
    }

    private static void Fail(string code) => throw new BattleRuleException("save-battle-" + code, "save.boundary");
}
