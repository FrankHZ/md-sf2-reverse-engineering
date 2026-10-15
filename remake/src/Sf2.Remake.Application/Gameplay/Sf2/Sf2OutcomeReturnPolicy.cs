using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Gameplay.Sf2;

internal class Sf2OutcomeReturnPolicy : IOutcomeReturnPolicy
{
    public virtual string Identity => "sf2-outcome-return";
    public OutcomeStart Start(ScenarioDefinition definition, EngineBattleState battle, StoryState story,
        BattleOutcomeKind kind, BattleStartInput party)
    {
        var selected = story.EnteringBattle!;
        var route = selected.Outcome ?? throw new BattleRuleException("outcome-route", "battle.outcome", true);
        var leader = battle.GetActor(battle.Definition.Outcome!.Leader);
        var first = battle.Actors.Where(actor => actor.IsAlly && actor.Hp > 0 && actor.Position is not null)
            .OrderBy(actor => actor.ProcessingOrder).FirstOrDefault();
        if (kind == BattleOutcomeKind.Victory && route.BattleOrigin is not null && definition.Exploration!.PartyFlags is { } layout)
        {
            // UpdateForce returns the first active member by member index, not turn/deployment order.
            int member = MapPartyMembership.Rebuild(story.Flags, layout).Active.First();
            first = battle.Actors.Single(actor => actor.IsAlly && actor.Deployment.Initialization?.AllyPartyMember == member);
            if (first.Position is null) throw new BattleRuleException("outcome-first-member-position", "battle.outcome", true);
        }
        var origin = route.BattleOrigin ?? new MapPosition(0, 0);
        var position = first?.Position ?? leader.Deployment.Position;
        var projected = new MapPosition(position.X + origin.X, position.Y + origin.Y);
        if (kind == BattleOutcomeKind.Defeat && (!story.Flags.Contains(399) || story.Flags.Any(flag => flag is 64 or 640)))
            throw new BattleRuleException("egress-flags", "battle.outcome.egress", true);
        var destination = kind == BattleOutcomeKind.Victory
            ? new FieldReturnAnchor(route.BattleMap, projected, route.BattleOrigin is null ? route.VictoryFacing : first!.Facing)
            : new FieldReturnAnchor(route.EgressMap, route.EgressPosition, route.EgressFacing);
        bool skip = kind == BattleOutcomeKind.Victory && selected.CompletedFlag is { } flag && story.Flags.Contains(flag);
        return new(kind == BattleOutcomeKind.Victory ? skip ? null : route.AfterProgram : route.DefeatProgram,
            destination, projected,
            kind == BattleOutcomeKind.Victory ? Heal(definition, party, all: false) : party);
    }

    public OutcomeFinish Finish(ScenarioDefinition definition, StoryState story, BattleStartInput party)
    {
        var selected = story.EnteringBattle!;
        var route = selected.Outcome!;
        bool victory = story.Continuation == ProgramContinuation.VictoryProgramFinished;
        if (!victory)
        {
            var encounter = definition.Encounters[selected.Encounter];
            var leader = encounter.Outcome!.Leader;
            party = new(party.Encounter, party.Actors.Select(input => input.Actor == leader ? input with
                { Hp = input.Progress?.MaxHp ?? encounter.Deployments.Single(row => row.Actor == leader).Definition.MaxHp } : input),
                party.MainSeed, party.ThinkingSeed,
                DefeatGold(party.Gold ?? throw new BattleRuleException("unspecified-gold", "party.gold", true)),
                party.NewBattle, party.ActiveAllies);
        }
        return new(party, Heal(definition, party, all: false), victory ? route.JoinMember : null,
            victory || selected.CompletedFlag is { } completed && story.Flags.Contains(completed), victory, route.ReturnProgram);
    }
    protected virtual uint DefeatGold(uint current) => current / 2;

    public BattleStartInput Heal(ScenarioDefinition definition, BattleStartInput party, bool all)
    {
        var encounter = definition.Encounters[party.Encounter];
        var actors = party.Actors.Select(input =>
        {
            var deployment = encounter.Deployments.FirstOrDefault(row => row.Actor == input.Actor) ??
                definition.Encounters.Values.SelectMany(value => value.Deployments)
                    .First(row => row.Faction == BattleFaction.Ally && row.Actor == input.Actor);
            if (deployment.Faction != BattleFaction.Ally || (!all && input.Hp == 0 && deployment.Initialization?.AllyPartyMember is not (7 or 28))) return input;
            if (input.Status != 0) throw new BattleRuleException("return-status-refresh", "party.status", true);
            return input with { Hp = input.Progress?.MaxHp ?? deployment.Definition.MaxHp,
                Mp = input.Progress?.MaxMp ?? deployment.Definition.MaxMp };
        }).ToArray();
        return new(party.Encounter, actors, party.MainSeed, party.ThinkingSeed, party.Gold, party.NewBattle, party.ActiveAllies);
    }
}
