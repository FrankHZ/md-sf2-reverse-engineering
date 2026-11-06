using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed record BattleActorMarker(ActorRef Actor, MapPosition Position, bool Ally, bool Selected, string Text, int? Facing = null, bool DeathEffect = false, bool Targeted = false);
internal sealed record BattlePresentation(string Title, string Status, string Roster,
    IReadOnlyList<BattleActorMarker> Markers, IReadOnlyList<MapPosition> Preview, IReadOnlyList<MapPosition> Focus,
    BattleMovementState? Movement, BattleMovementRange? MovementRange);

internal static class BattleSnapshotProjection
{
    internal static BattlePresentation Project(SessionResult result, ActorRef? candidate, string origin, BattleMovementRange? movementRange,
        BattleSceneDefinition? names = null)
    {
        var snapshot = result.Snapshot;
        var battle = snapshot.Battle;
        var selection = snapshot.Selection;
        string status = result.Failure is { } failure
            ? GameText.BattleFailure(failure, snapshot.StopReason is SessionStopReason.Unsupported or SessionStopReason.Faulted)
            : snapshot.BattleResult is { } outcome ? $"{outcome}."
            : selection is null ? GameText.Get("battle.in-progress")
            : selection.Stage switch
            {
                BattleSelectionStage.Movement => GameText.Get("battle.status.position"),
                BattleSelectionStage.ActionChoice => GameText.Get("battle.status.action"),
                BattleSelectionStage.TargetChoice => GameText.Get("battle.status.target"),
                BattleSelectionStage.EquipmentChoice => GameText.Get("battle.status.equipment"),
                _ => GameText.Get("battle.status.confirm"),
            };
        var death = snapshot.BattleScene is { IsFieldDeath: true } field ? field : null;
        var staged = snapshot.BattleFieldScene;
        var mapOrigin = snapshot.Story.EnteringBattle?.Outcome?.BattleOrigin ?? new MapPosition(0, 0);
        ExplorationEntity? Entity(BattleActorState actor)
        {
            int slot = actor.IsAlly ? actor.Deployment.Initialization?.AllyPartyMember ?? actor.ProcessingOrder : actor.ProcessingOrder;
            var reference = new EntityRef("entity-" + slot.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return staged?.Entities.TryGetValue(reference, out var entity) == true && entity.Entity == reference ? entity : null;
        }
        MapPosition Position(BattleActorState actor) => Entity(actor) is { } entity
            ? new(entity.Position.X - mapOrigin.X, entity.Position.Y - mapOrigin.Y)
            : snapshot.BattleMovement?.Actor == actor.Actor ? snapshot.BattleMovement.From
            : selection?.Actor == actor.Actor ? selection.Preview.Destination : actor.Position!;
        var markers = battle.Actors.Where(a => a.Position is not null && (staged is null || Entity(a) is { Visible: true })).Select(a => new BattleActorMarker(
            a.Actor, Position(a), a.IsAlly, a.Actor == selection?.Actor,
            $"{BattleCharacterNames.Resolve(a, names, snapshot.Story)}\n{a.Hp}/{a.MaxHp}",
            death?.DeadActors.Contains(a.Actor) == true ? death.FieldFacing : Entity(a)?.Motion.Facing ?? a.Facing,
            death?.DeadActors.Contains(a.Actor) == true && death.Phase == Sf2.Remake.Application.Runtime.Battles.BattleScenePhase.FieldExit,
            selection?.Target == a.Actor && selection.Stage is BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady)).ToArray();
        string Status(BattleActorState a) => (a.Status & 0x300) == 0 ? "" : " · " + GameText.Get("battle.status.silence", (a.Status & 0x300) >> 8);
        string roster = string.Join("\n", battle.Actors.Select(a =>
            $"{BattleCharacterNames.Resolve(a, names, snapshot.Story)}: HP {a.Hp}/{a.MaxHp}  MP {a.Mp}/{a.MaxMp}  EXP {a.Exp?.ToString() ?? "Unknown"}  KILLS {a.Kills?.ToString() ?? "Unknown"}  DEFEATS {a.Defeats?.ToString() ?? "Unknown"}{Status(a)}"));
        var focus = selection?.Preview.Path.ToList() ?? [];
        foreach (var actor in battle.Actors.Where(a => a.Hp > 0 && (a.Actor == selection?.Actor ||
            a.Actor == selection?.Target || a.Actor == candidate))) focus.Add(actor.Position!);
        if (death is not null)
            foreach (var id in death.DeadActors)
                if (battle.GetActor(id).Position is not null) focus.Add(Position(battle.GetActor(id)));
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
