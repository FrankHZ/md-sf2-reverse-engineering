using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class FieldEgress
{
    internal static SessionResult Begin(ScenarioDefinition definition, SessionSnapshot current, ActorRef caster,
        BattleStartInput paid, SessionRules rules)
    {
        var world = current.Exploration!;
        var capability = definition.FieldPreparation!.Egress;
        if (capability is null || !capability.Maps.Contains(world.Map) || !current.Story.Flags.Contains(399) ||
            current.Story.Flags.Any(flag => flag is 64 or 640) || world.TravelMode != FieldTravelMode.Walking)
            throw new BattleRuleException("field-egress-context", "field.egress", true);
        var anchor = current.Story.EgressMap is { } live ? Sf2OutcomeReturnPolicy.FindEgress(definition, live) : null;
        if (anchor is null) throw new BattleRuleException("egress-map", "story.egressMap", true);
        var warp = new ExplorationEvent(ExplorationEventKind.Warp, null, null, null, null,
            anchor.Map, anchor.Position, anchor.Facing);
        // Compute entry recovery and validate the selected destination before publishing payment.
        // The program applies that recovery only after actual flash completion.
        var recovered = OutcomeReturnRules.Heal(rules.OutcomeReturn, definition, paid, all: false);
        MapTransfer.Validate(definition, current, world.WithParty(recovered), warp, rules);
        if (!definition.Exploration!.Programs.ContainsKey(capability.Program.Program))
            throw new BattleRuleException("field-egress-program", "field.egress", true);
        List<SessionObservation> observations = [];
        var story = current.Story.Copy(capability.Program, callers: [],
            scriptWarp: new(anchor.Map, anchor.Position, anchor.Facing, MapLoadMode.Rebuild),
            continuation: ProgramContinuation.FieldInput);
        current = ProgramRunner.Commit(current, new ActiveExploration(world.WithParty(paid)), story,
            observations, "field-egress-paid", caster.Value);
        return ProgramRunner.Run(definition, current, observations, rules);
    }
}
