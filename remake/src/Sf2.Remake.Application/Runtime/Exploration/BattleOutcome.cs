using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class BattleOutcome
{
    internal static SessionResult Begin(ScenarioDefinition definition, SessionResult result)
    {
        var current = result.Snapshot;
        if (current.Mode != SessionMode.Battle || current.BattleScene is not null || current.BattleMovement is not null ||
            BattleOutcomeRules.Check(current.Battle) is not { } kind) return result;
        var observations = result.Observations.ToList();
        try
        {
            var route = current.Story.EnteringBattle?.Outcome ?? throw new BattleRuleException("outcome-route", "battle.outcome", true);
            var battle = current.Battle;
            var leader = battle.GetActor(battle.Definition.Outcome!.Leader);
            var first = battle.Actors.Where(actor => actor.IsAlly && actor.Hp > 0 && actor.Position is not null)
                .OrderBy(actor => actor.ProcessingOrder).FirstOrDefault();
            if (kind == BattleOutcomeKind.Defeat && (!current.Story.Flags.Contains(399) || current.Story.Flags.Any(flag => flag is 64 or 640)))
                throw new BattleRuleException("egress-flags", "battle.outcome.egress", true);
            var destination = kind == BattleOutcomeKind.Victory
                ? new FieldReturnAnchor(route.BattleMap, first!.Position!, route.VictoryFacing)
                : new FieldReturnAnchor(route.EgressMap, route.EgressPosition, route.EgressFacing);
            var party = new BattleStartInput(battle.Definition.Encounter,
                battle.Actors.Select(actor => new BattleActorStartInput(actor.Actor, actor.Hp, actor.Mp, actor.Exp,
                    actor.Kills, actor.Defeats, actor.Status, null, actor.Progress, actor.SourceLoadout)),
                battle.MainSeed, battle.ThinkingSeed, battle.Gold, battle.StartPolicy);
            if (kind == BattleOutcomeKind.Victory) party = Heal(definition, party, all: false);
            var map = definition.Exploration!.Maps[route.BattleMap];
            // This temporary field projection carries the same party until the after-program's
            // actual scene/entity loads. Return coordinates remain the controller's earlier values.
            var position = first?.Position ?? battle.Definition.Deployments.Single(row => row.Actor == leader.Actor).Position;
            var world = MapTransfer.Build(map, new("entity-0"), position, route.VictoryFacing, 32, party, current.Story.Flags);
            bool skip = kind == BattleOutcomeKind.Victory && current.Story.EnteringBattle!.CompletedFlag is { } flag && current.Story.Flags.Contains(flag);
            var story = current.Story.Copy(kind == BattleOutcomeKind.Victory ? skip ? null : route.AfterProgram : route.DefeatProgram,
                callers: [], continuation: kind == BattleOutcomeKind.Victory ? ProgramContinuation.VictoryProgramFinished : ProgramContinuation.DefeatProgramFinished,
                outcomeReturn: destination, clearCameraEntity: true, clearCameraTarget: true, textWindow: new ClosedTextWindow());
            current = ProgramRunner.Commit(current, new ActiveExploration(world), story, observations,
                "outcome-program-started", kind.ToString());
            return ProgramRunner.Run(definition, current, observations);
        }
        catch (BattleRuleException error) { return ProgramRunner.Failure(current, observations, error); }
    }

    internal static SessionSnapshot Continue(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations)
    {
        var selected = current.Story.EnteringBattle!;
        var route = selected.Outcome!;
        var story = current.Story;
        if (story.Continuation == ProgramContinuation.VictoryProgramFinished)
        {
            var layout = definition.Exploration!.PartyFlags ?? throw new BattleRuleException("party-flag-layout", "world.partyFlags", true);
            var joined = MapPartyMembership.Join(story.Flags, layout, route.JoinMember);
            story = story.Copy(null, flags: joined.Flags, partyLists: joined.Lists);
            current = ProgramRunner.Commit(current, current.Active, story, observations, "after-battle-join", route.JoinMember.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (selected.UnlockedFlag is { } unlocked)
            {
                story = story.Copy(null, flags: ProgramRunner.Flags(story, unlocked, false));
                current = ProgramRunner.Commit(current, current.Active, story, observations, "battle-unlock-cleared", unlocked.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            if (selected.CompletedFlag is { } complete)
            {
                story = story.Copy(null, flags: ProgramRunner.Flags(story, complete, true));
                current = ProgramRunner.Commit(current, current.Active, story, observations, "battle-completed-set", complete.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        else
        {
            var world = current.Exploration!;
            var encounter = definition.Encounters[selected.Encounter];
            var leader = encounter.Outcome!.Leader;
            var actors = world.Party.Actors.Select(input => input.Actor == leader ? input with
            { Hp = input.Progress?.MaxHp ?? encounter.Deployments.Single(row => row.Actor == leader).Definition.MaxHp } : input);
            var party = new BattleStartInput(world.Party.Encounter, actors, world.Party.MainSeed, world.Party.ThinkingSeed,
                (world.Party.Gold ?? throw new BattleRuleException("unspecified-gold", "party.gold", true)) / 2,
                world.Party.NewBattle, world.Party.ActiveAllies);
            current = ProgramRunner.Commit(current, new ActiveExploration(world.WithParty(party)), story,
                observations, "defeat-recovered", "leader HP; unsigned gold/2; egress selected");
        }
        // ExplorationLoop heals living/immortal allies on either return path, after the outcome's
        // own recovery/program. Keep dead ordinary allies distinct from the csc55 all-party reset.
        var field = current.Exploration!;
        return ProgramRunner.Commit(current, new ActiveExploration(field.WithParty(Heal(definition, field.Party, all: false))),
            current.Story.Copy(route.ReturnProgram, continuation: ProgramContinuation.OutcomeMapLoaded), observations,
            "exploration-return-started");
    }

    internal static BattleStartInput Heal(ScenarioDefinition definition, BattleStartInput party, bool all)
    {
        var encounter = definition.Encounters[party.Encounter];
        var actors = party.Actors.Select(input =>
        {
            var deployment = encounter.Deployments.Single(row => row.Actor == input.Actor);
            if (deployment.Faction != BattleFaction.Ally || (!all && input.Hp == 0 && deployment.Initialization?.AllyPartyMember is not (7 or 28))) return input;
            if (input.Status != 0) throw new BattleRuleException("return-status-refresh", "party.status", true);
            return input with { Hp = input.Progress?.MaxHp ?? deployment.Definition.MaxHp,
                Mp = input.Progress?.MaxMp ?? deployment.Definition.MaxMp };
        }).ToArray();
        return new(party.Encounter, actors, party.MainSeed, party.ThinkingSeed, party.Gold, party.NewBattle, party.ActiveAllies);
    }
}
