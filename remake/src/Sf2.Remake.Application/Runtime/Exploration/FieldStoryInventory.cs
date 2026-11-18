using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

// Direct story services use the named combatant, without joined/deployed/HP or party-space gates.
internal static class FieldStoryInventory
{
    private static (BattleActorStartInput Actor, BattleSourceLoadout Loadout) Pack(
        ScenarioDefinition definition, SessionSnapshot current, ActorRef actor)
    {
        var carried = current.Exploration!.Party.Actors.FirstOrDefault(row => row.Actor == actor)
            ?? throw new BattleRuleException("missing-party-member", "program.actor", true);
        var loadout = carried.SourceLoadout ?? carried.Progress?.SourceLoadout ??
            definition.Encounters.Values.SelectMany(row => row.Deployments).FirstOrDefault(row => row.Actor == actor)?.Definition.SourceLoadout
            ?? throw new BattleRuleException("field-loadout-unavailable", "actor.items", true);
        if (loadout.Items.Count != 4) throw new BattleRuleException("field-item-slots", "actor.items");
        return (carried, loadout);
    }

    internal static bool HasSpace(ScenarioDefinition definition, SessionSnapshot current, ActorRef actor) =>
        Pack(definition, current, actor).Loadout.Items.Any(word => (word & 127) == 127);

    internal static ActiveSessionState Add(ScenarioDefinition definition, SessionSnapshot current, ActorRef actor, byte item)
    {
        var (carried, loadout) = Pack(definition, current, actor);
        // AddItem's failure is intentionally a no-op. The caller owns subsequent narration/flags.
        if (!loadout.Items.Any(word => (word & 127) == 127)) return current.Active;
        var changed = BattleInventory.Add(loadout, item);
        var updated = carried with { SourceLoadout = changed, Progress = carried.Progress is { } progress
            ? progress with { SourceLoadout = changed } : null };
        var world = current.Exploration!;
        return new ActiveExploration(world.WithParty(FieldInventory.Replace(world.Party, [updated])));
    }
}
