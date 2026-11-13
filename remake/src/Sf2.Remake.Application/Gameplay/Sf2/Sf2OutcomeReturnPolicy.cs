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
        if (kind == BattleOutcomeKind.Retreat)
        {
            if (!story.Flags.Contains(399) || story.Flags.Any(flag => flag is 64 or 640))
                throw new BattleRuleException("egress-flags", "battle.outcome.egress", true);
            var caster = battle.Actors.First(actor => actor.IsAlly && actor.Hp > 0 && actor.Position is not null);
            var retreatOrigin = route.BattleOrigin ?? new MapPosition(0, 0);
            return new(route.ReturnProgram, EgressDestination(definition, story, route, kind),
                new(caster.Position!.X + retreatOrigin.X, caster.Position.Y + retreatOrigin.Y), party);
        }
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
            : EgressDestination(definition, story, route, kind);
        if (kind == BattleOutcomeKind.Defeat && route.CompletedDefeatMap is { } handoff)
            destination = destination with { Map = handoff };
        bool skip = kind == BattleOutcomeKind.Victory && selected.CompletedFlag is { } flag && story.Flags.Contains(flag);
        return new(kind == BattleOutcomeKind.Victory ? skip ? null : route.AfterProgram : route.DefeatProgram,
            destination, projected,
            kind == BattleOutcomeKind.Victory ? Heal(definition, party, all: false) : party);
    }

    private static FieldReturnAnchor EgressDestination(ScenarioDefinition definition, StoryState story,
        ExplorationOutcomeRoute route, BattleOutcomeKind kind)
    {
        var live = route.FixedEgress ? route.EgressMap : story.EgressMap ?? route.EgressMap;
        var destination = FindEgress(definition, live, route);
        if (destination is null && kind == BattleOutcomeKind.Defeat &&
            definition.DefeatFallback is { } fallback && live == fallback.Register)
            destination = FindEgress(definition, fallback.Destination, route);
        if (destination is null) throw new BattleRuleException("egress-map", "story.egressMap", true);
        return destination;
    }

    internal static FieldReturnAnchor? FindEgress(ScenarioDefinition definition, MapId live, ExplorationOutcomeRoute? preferred = null)
    {
        var destination = preferred?.EgressMap == live ? preferred : definition.Exploration!.Maps.Values
            .SelectMany(value => value.Battles).Select(value => value.Outcome)
            .FirstOrDefault(value => value?.EgressMap == live);
        return destination is null ? null : new(destination.EgressMap, destination.EgressPosition, destination.EgressFacing);
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
            party = party.WithResources(party.Actors.Select(input => input.Actor == leader ? input with
                { Hp = input.Progress?.MaxHp ?? encounter.Deployments.Single(row => row.Actor == leader).Definition.MaxHp } : input),
                DefeatGold(party.Gold ?? throw new BattleRuleException("unspecified-gold", "party.gold", true)));
        }
        return new(party, Heal(definition, party, all: false), victory ? route.JoinMember : null,
            victory || route.CompletedDefeatMap is not null || selected.CompletedFlag is { } completed && story.Flags.Contains(completed),
            victory || route.CompletedDefeatMap is not null, route.ReturnProgram);
    }
    protected virtual uint DefeatGold(uint current) => current / 2;

    public virtual bool CanRecover(BattleDeploymentDefinition deployment, BattleActorStartInput actor, bool all) =>
        deployment.Faction == BattleFaction.Ally &&
        (all || actor.Hp > 0 || deployment.Initialization?.AllyPartyMember is 7 or 28);

    public BattleStartInput Heal(ScenarioDefinition definition, BattleStartInput party, bool all)
    {
        var encounter = definition.Encounters[party.Encounter];
        var actors = party.Actors.Select(input =>
        {
            var deployment = encounter.Deployments.FirstOrDefault(row => row.Actor == input.Actor) ??
                definition.Encounters.Values.SelectMany(value => value.Deployments)
                    .First(row => row.Faction == BattleFaction.Ally && row.Actor == input.Actor);
            if (!CanRecover(deployment, input, all)) return input;
            if ((input.Status & ~(0xC0 | 0x0300 | 2)) != 0) throw new BattleRuleException("return-status-refresh", "party.status", true);
            // HealLivingAndImmortalAllies and ResetAlliesStats preserve lasting poison.
            return input with { Status = (ushort)(input.Status & 2), Hp = input.Progress?.MaxHp ?? deployment.Definition.MaxHp,
                Mp = input.Progress?.MaxMp ?? deployment.Definition.MaxMp };
        }).ToArray();
        return party.WithResources(actors, party.Gold);
    }
}
