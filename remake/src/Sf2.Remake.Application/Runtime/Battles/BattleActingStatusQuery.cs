using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleActingStatus(BattleUnitSummary Unit, BattleLandEffect Land);

internal static class BattleActingStatusQuery
{
    internal static BattleActingStatus? Query(ScenarioDefinition definition, SessionSnapshot current)
    {
        if (current.Selection is not { } selection ||
            current.StopReason is not (SessionStopReason.PlayerInput or SessionStopReason.PresentationWait)) return null;
        // Remove only the owned player/undo delivery when checking ordinary control.
        // Other scene/program/outcome/automatic boundaries retain their normal exclusion.
        var movement = current.BattleMovement;
        if (movement is not null && (movement.Actor != selection.Actor ||
            movement.Purpose is not (BattleMovementPurpose.Player or BattleMovementPurpose.Return))) return null;
        var idle = movement is null ? current : new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            ((ActiveBattle)current.Active) with { Movement = null }, current.Story, SessionStopReason.PlayerInput);
        if (!idle.HasBattleControl) return null;
        var actor = current.Battle.GetActor(selection.Actor);
        if (!actor.Initialized || actor.Hp == 0 || actor.Control != BattleControl.Player || !BattleVisibility.OnBoard(current, actor)) return null;
        // Preview.Destination is requested before delivery; actor.Position stays at the turn origin.
        // From is the last fully delivered tile, including each segment of an undo route.
        var shown = movement?.From ?? selection.Preview.Destination;
        return new(BattleUnitProjection.Summary(definition, current, actor), BattleUnitProjection.Land(current, actor, shown));
    }
}
