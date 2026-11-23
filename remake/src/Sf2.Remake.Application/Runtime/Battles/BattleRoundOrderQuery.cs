using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleRoundActor(ActorRef Actor, bool Ally, MapPosition Position,
    string Name, int? NameIndex, string? CustomName);

// Null upcoming identities occupy their original slots without disclosing an actor.
public sealed record BattleRoundOrder(BattleRoundActor Current, IReadOnlyList<BattleRoundActor?> Next, int Additional);

internal static class BattleRoundOrderQuery
{
    internal static BattleRoundOrder? Query(ScenarioDefinition definition, SessionSnapshot current)
    {
        if (current.Selection?.Stage is not (BattleSelectionStage.Movement or BattleSelectionStage.ActionChoice or
            BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady) ||
            BattleActingStatusQuery.ActingActor(current) is not { } acting) return null;
        var battle = current.Battle;
        if (BattleTurnFlow.AtRoundEnd(battle) || battle.Queue[battle.Cursor].Actor != acting.Actor) return null;

        var scenes = definition.ScenesFor(battle.Definition);
        BattleRoundActor Identity(BattleActorState actor) => new(actor.Actor, actor.IsAlly, actor.Position!,
            BattleUnitProjection.Name(definition, current, actor), BattleUnitProjection.NameIndex(actor, scenes),
            current.Story.Names.GetValueOrDefault(actor.Actor.Value));

        List<BattleRoundActor?> next = new(3);
        int additional = 0;
        for (int slot = battle.Cursor + 1; !BattleTurnFlow.AtRoundEnd(battle, slot); slot++)
        {
            var actor = battle.GetActor(battle.Queue[slot].Actor!.Value);
            if (BattleAdvancer.Classify(actor) != BattleAdvancer.Dispatch.Act) continue;
            if (next.Count < 3)
                next.Add(BattleVisibility.IsDisclosed(current, actor) ? Identity(actor) : null);
            else additional++;
        }
        return new(Identity(acting), next.AsReadOnly(), additional);
    }
}
