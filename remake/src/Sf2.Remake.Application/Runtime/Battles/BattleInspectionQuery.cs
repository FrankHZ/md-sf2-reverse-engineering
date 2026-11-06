using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleInspectionUnit(ActorRef Actor, MapPosition? Position, bool Ally, string Name,
    int? NameIndex, string? CustomName, byte? ClassId, string? ClassName,
    int? Hp, int? MaxHp, int? Mp, int? MaxMp, int? Level, int? Exp,
    int? Attack, int? Defense, int? Agility, int? Move,
    IReadOnlyList<string> Status, IReadOnlyList<PartySpell> Spells, IReadOnlyList<PartyInventorySlot> Inventory);
public sealed record BattleInspectionMarker(ActorRef Actor, MapPosition Position, bool Ally, string Name,
    int? NameIndex, string? CustomName);
public sealed record BattleInspectionSnapshot(Guid SessionId, long Revision, bool Available, string? Reason,
    ActorRef? ActiveActor, int Width, int Height, BattleMapRegion? Region,
    IReadOnlyList<TerrainSurface> Terrain, IReadOnlyList<BattleInspectionMarker> Units, BattleInspectionUnit? Unit, BattleLandEffect? Land);

internal static class BattleInspectionQuery
{
    internal static BattleInspectionSnapshot Query(ScenarioDefinition definition, SessionSnapshot current,
        Guid? session = null, long? revision = null, ActorRef? requested = null)
    {
        BattleInspectionSnapshot Reject(string reason) => new(current.SessionId, current.Revision, false, reason,
            null, 0, 0, null, Array.AsReadOnly(Array.Empty<TerrainSurface>()), Array.AsReadOnly(Array.Empty<BattleInspectionMarker>()), null, null);
        if (Unavailable(current, session, revision) is { } reason) return Reject(reason);
        var battle = current.Battle;
        var active = current.Selection!.Actor;
        var scenes = definition.ScenesFor(battle.Definition.Encounter);
        var actors = battle.Actors.Where(a => a.Hp > 0 && a.Initialized && BattleVisibility.OnBoard(current, a)).ToArray();
        var markers = actors.Select(a => new BattleInspectionMarker(a.Actor, a.Position!, a.IsAlly,
            BattleUnitProjection.Name(definition, current, a), BattleUnitProjection.NameIndex(a, scenes), current.Story.Names.GetValueOrDefault(a.Actor.Value))).ToArray();
        var actor = requested is null ? null : actors.FirstOrDefault(a => a.Actor == requested);
        if (requested is not null && actor is null) return Reject("unit-not-disclosed");
        BattleInspectionUnit? unit = null;
        BattleLandEffect? land = null;
        if (actor is not null)
        {
            unit = BattleUnitProjection.Details(definition, current, actor);
            land = BattleUnitProjection.Land(current, actor, actor.Position!);
        }
        return new(current.SessionId, current.Revision, true, null, active, battle.Definition.Width,
            battle.Definition.Height, battle.Definition.SourceMapRegion,
            Array.AsReadOnly(Enumerable.Range(0, battle.Definition.Height).SelectMany(y =>
                Enumerable.Range(0, battle.Definition.Width).Select(x => battle.Definition.Terrain[y * 48 + x].Surface)).ToArray()),
            Array.AsReadOnly(markers), unit, land);
    }

    // Shared settled-origin eligibility; callers do not need Map/marker materialization.
    internal static string? Unavailable(SessionSnapshot current, Guid? session, long? revision)
    {
        if (session is { } id && id != current.SessionId || revision is { } rev && rev != current.Revision)
            return "stale-inspection";
        if (!current.HasBattleControl || current.StopReason != SessionStopReason.PlayerInput ||
            current.Selection is not { Stage: BattleSelectionStage.Movement } selection)
            return "inspection-unavailable";
        var active = current.Battle.GetActor(selection.Actor);
        if (!active.Initialized || active.Hp == 0 || active.Control != BattleControl.Player || !BattleVisibility.OnBoard(current, active) ||
            selection.Preview.Destination != active.Position || selection.Preview.Path.Count != 1)
            return "inspection-unsettled";
        return null;
    }
}
