using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class BattleOutcome
{
    internal static SessionResult Begin(ScenarioDefinition definition, SessionResult result) =>
        Begin(definition, result, Gameplay.RuleCompositions.Sf2());

    internal static SessionResult Begin(ScenarioDefinition definition, SessionResult result, SessionRules rules)
    {
        var current = result.Snapshot;
        if (current.Mode != SessionMode.Battle || current.BattleScene is not null || current.BattleMovement is not null) return result;
        var observations = result.Observations.ToList();
        try
        {
            if (BattleOutcomeSelection.Check(rules.Outcome, current.Battle) is not { } kind) return result;
            var route = current.Story.EnteringBattle?.Outcome ?? throw new BattleRuleException("outcome-route", "battle.outcome", true);
            var battle = current.Battle;
            var party = new BattleStartInput(battle.Definition.Encounter,
                battle.Actors.Select(actor => new BattleActorStartInput(actor.Actor, actor.Hp, actor.Mp, actor.Exp,
                    actor.Kills, actor.Defeats, actor.Status, null, actor.Progress, actor.SourceLoadout)),
                battle.MainSeed, battle.ThinkingSeed, battle.Gold, battle.StartPolicy);
            var selected = OutcomeReturnRules.Start(rules.OutcomeReturn, definition, battle, current.Story, kind, party);
            var map = definition.Exploration!.Maps[route.BattleMap];
            var world = MapTransfer.Build(map, new("entity-0"), selected.ProjectionPosition, route.VictoryFacing, 32,
                selected.Party, current.Story.Flags);
            var story = current.Story.Copy(selected.Program, callers: [],
                continuation: kind == BattleOutcomeKind.Victory ? ProgramContinuation.VictoryProgramFinished : ProgramContinuation.DefeatProgramFinished,
                outcomeReturn: selected.Return, clearCameraEntity: true, clearCameraTarget: true, textWindow: new ClosedTextWindow());
            story = ExplorationTextRunner.Initialize(world, story);
            current = ProgramRunner.Commit(current, new ActiveExploration(world), story, observations,
                "outcome-program-started", kind.ToString());
            return ProgramRunner.Run(definition, current, observations, rules);
        }
        catch (BattlePolicyFault error) { return ProgramRunner.Failure(current, observations, error); }
        catch (BattleRuleException error) { return ProgramRunner.Failure(current, observations, error); }
    }

    internal static SessionSnapshot Continue(ScenarioDefinition definition, SessionSnapshot current,
        List<SessionObservation> observations, SessionRules rules)
    {
        var route = current.Story.EnteringBattle!;
        var story = current.Story;
        var field = current.Exploration!;
        // One finite decision and its validation precede this stage's publications. No selected
        // call follows the join/flag commits, so a policy failure cannot lose or repeat their tail.
        var selected = OutcomeReturnRules.Finish(rules.OutcomeReturn, definition, story, field.Party);
        MapPartyJoinResult? joined = null;
        if (selected.JoinMember is { } member)
        {
            var layout = definition.Exploration!.PartyFlags ?? throw new BattleRuleException("party-flag-layout", "world.partyFlags", true);
            joined = MapPartyMembership.Join(story.Flags, layout, member);
        }
        if (joined is not null)
        {
            story = story.Copy(null, flags: joined.Flags, partyLists: joined.Lists);
            current = ProgramRunner.Commit(current, current.Active, story, observations, "after-battle-join", selected.JoinMember!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        if (selected.ClearUnlocked && route.UnlockedFlag is { } unlocked)
        {
            story = story.Copy(null, flags: ProgramRunner.Flags(story, unlocked, false));
            current = ProgramRunner.Commit(current, current.Active, story, observations, "battle-unlock-cleared", unlocked.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        if (selected.SetCompleted && route.CompletedFlag is { } completed)
        {
            story = story.Copy(null, flags: ProgramRunner.Flags(story, completed, true));
            current = ProgramRunner.Commit(current, current.Active, story, observations, "battle-completed-set", completed.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        if (story.Continuation == ProgramContinuation.DefeatProgramFinished)
            current = ProgramRunner.Commit(current, new ActiveExploration(field.WithParty(selected.Recovered)), story,
                observations, "defeat-recovered", "selected leader recovery and gold; egress selected");
        return ProgramRunner.Commit(current, new ActiveExploration(field.WithParty(selected.Returned)),
            current.Story.Copy(selected.Program, continuation: ProgramContinuation.OutcomeMapLoaded), observations,
            "exploration-return-started");
    }

    // Explicit source-default scalar comparison entry. Runtime reset instructions pass SessionRules.
    internal static BattleStartInput Heal(ScenarioDefinition definition, BattleStartInput party, bool all) =>
        OutcomeReturnRules.Heal(Gameplay.RuleCompositions.SourceOutcomeReturn(), definition, party, all);
}
