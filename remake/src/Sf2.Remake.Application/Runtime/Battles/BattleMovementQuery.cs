using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleMovementRange(ActorRef Actor, MapPosition Origin,
    IReadOnlyList<MapPosition> ReachableCells, IReadOnlyList<MapPosition> Destinations);

internal static class BattleMovementQuery
{
    internal static BattleMovementRange? Query(SessionSnapshot current)
    {
        if (current.Active is not ActiveBattle { Outcome: null, Scene: null, Retreat: null, Round: null,
                Give: null, Discard: null, Selection: { Stage: BattleSelectionStage.Movement } selection } active ||
            current.Story.Cursor is not null || current.Story.Wait is not null ||
            active.Movement is { } movement && (movement.Actor != selection.Actor ||
                movement.Purpose is not (BattleMovementPurpose.Player or BattleMovementPurpose.Return))) return null;
        var actor = active.Battle.GetActor(selection.Actor);
        if (actor.Hp == 0 || actor.Control != BattleControl.Player || actor.Position is null) return null;
        return ForActor(active.Battle, actor.Actor, actor.Position);
    }

    internal static BattleMovementRange ForActor(EngineBattleState battle, ActorRef actor, MapPosition origin)
    {
        var (reachableCells, destinations) = BattleMovement.Range(battle, actor);
        return new(actor, origin, reachableCells, destinations);
    }
}
