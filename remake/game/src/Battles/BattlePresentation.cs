using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed record BattleActorMarker(ActorRef Actor, MapPosition Position, bool Ally, bool Selected, string Text, int? Facing = null, bool DeathEffect = false, bool Targeted = false);
internal sealed record BattlePresentation(string Title, string Status, string Roster,
    IReadOnlyList<BattleActorMarker> Markers, IReadOnlyList<MapPosition> Preview, IReadOnlyList<MapPosition> Focus,
    BattleMovementState? Movement, BattleMovementRange? MovementRange);

internal static class BattleSnapshotProjection
{
    internal static BattlePresentation Project(SessionResult result, ActorRef? candidate, string origin, BattleMovementRange? movementRange)
    {
        var snapshot = result.Snapshot;
        var battle = snapshot.Battle;
        var selection = snapshot.Selection;
        string status = result.Failure is { } failure
            ? failure.Kind == SessionFailureKind.IllegalCommand ? $"Unavailable: {failure.Message}"
                : $"The battle has stopped.\n{failure.Message} ({failure.Code})"
            : snapshot.BattleResult is { } outcome ? $"{outcome}."
            : selection is null ? "Battle in progress…"
            : selection.Stage switch
            {
                BattleSelectionStage.Movement => "Choose a position.",
                BattleSelectionStage.ActionChoice => "Choose an action.",
                BattleSelectionStage.TargetChoice => "Choose a target.",
                BattleSelectionStage.EquipmentChoice => "Choose equipment.",
                _ => "Choose or confirm your selection.",
            };
        var death = snapshot.BattleScene is { IsFieldDeath: true } field ? field : null;
        var markers = battle.Actors.Where(a => a.Position is not null).Select(a => new BattleActorMarker(
            a.Actor, snapshot.BattleMovement?.Actor == a.Actor ? snapshot.BattleMovement.From :
                selection?.Actor == a.Actor ? selection.Preview.Destination : a.Position!, a.IsAlly, a.Actor == selection?.Actor,
            $"{a.Actor.Value}\n{a.Hp}/{a.MaxHp}",
            death?.DeadActors.Contains(a.Actor) == true ? death.FieldFacing : a.Facing,
            death?.DeadActors.Contains(a.Actor) == true && death.Phase == Sf2.Remake.Application.Runtime.Battles.BattleScenePhase.FieldExit,
            selection?.Target == a.Actor && selection.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady)).ToArray();
        string roster = string.Join("\n", battle.Actors.Select(a =>
            $"{a.Actor.Value}: HP {a.Hp}/{a.MaxHp}  MP {a.Mp}/{a.MaxMp}  EXP {a.Exp?.ToString() ?? "Unknown"}  KILLS {a.Kills?.ToString() ?? "Unknown"}  DEFEATS {a.Defeats?.ToString() ?? "Unknown"}"));
        var focus = selection?.Preview.Path.ToList() ?? [];
        foreach (var actor in battle.Actors.Where(a => a.Hp > 0 && (a.Actor == selection?.Actor ||
            a.Actor == selection?.Target || a.Actor == candidate))) focus.Add(actor.Position!);
        if (death is not null)
            foreach (var id in death.DeadActors)
                if (battle.GetActor(id).Position is { } position) focus.Add(position);
        if (snapshot.BattleMovement is { } movement) { focus.Add(movement.From); focus.Add(movement.To); }
        string title = origin switch
        {
            "private-local-controlled-start" => "PRIVATE CONTROLLED BATTLE",
            "private-local-new-game" => "BATTLE",
            _ => "AUTHORED BATTLE",
        };
        return new($"{title} · {battle.Definition.Map.Value} · round {battle.Round} · gold {battle.Gold?.ToString() ?? "Unknown"}", status,
            roster, Array.AsReadOnly(markers), selection?.Preview.Path ?? [], focus.AsReadOnly(), snapshot.BattleMovement, movementRange);
    }
}
