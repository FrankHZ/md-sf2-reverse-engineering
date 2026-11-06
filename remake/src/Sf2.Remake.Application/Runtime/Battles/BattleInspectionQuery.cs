using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleInspectionUnit(ActorRef Actor, MapPosition Position, bool Ally, string Name,
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
        if (session is { } id && id != current.SessionId || revision is { } rev && rev != current.Revision)
            return Reject("stale-inspection");
        if (!current.HasBattleControl || current.StopReason != SessionStopReason.PlayerInput ||
            current.Selection is not { Stage: BattleSelectionStage.Movement } selection)
            return Reject("inspection-unavailable");
        var battle = current.Battle;
        var active = battle.GetActor(selection.Actor);
        if (!active.Initialized || active.Hp == 0 || active.Control != BattleControl.Player || !BattleVisibility.OnBoard(current, active) ||
            selection.Preview.Destination != active.Position || selection.Preview.Path.Count != 1)
            return Reject("inspection-unsettled");
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
            var summary = BattleUnitProjection.Summary(definition, current, actor);
            int? Stat(int value) => BattleUnitProjection.Stat(definition, value);
            unit = new(actor.Actor, actor.Position!, actor.IsAlly, summary.Name,
                summary.NameIndex, summary.CustomName, summary.ClassId, summary.ClassName,
                summary.Hp, summary.MaxHp, summary.Mp, summary.MaxMp, summary.Level,
                actor.IsAlly ? actor.Exp : null, Stat(actor.Attack), Stat(actor.Defense), Stat(actor.Agility), Stat(actor.Definition.Move),
                Status(actor.Status), Array.AsReadOnly(actor.Spells.Select(spell => new PartySpell(spell,
                    PartyDetailsQuery.SpellName(definition, spell))).ToArray()),
                Array.AsReadOnly(Enumerable.Range(0, 4).Select(slot =>
                {
                    var items = actor.SourceLoadout?.Items;
                    if (items is null || slot >= items.Count) return new PartyInventorySlot(slot, null, null, false, false);
                    ushort word = items[slot]; byte item = (byte)(word & 127);
                    return new PartyInventorySlot(slot, item == 127 ? null : PartyDetailsQuery.ItemName(definition, battle.Definition, actor.Definition, item),
                        item == 127, item != 127 && (word & 128) != 0, item != 127 && (word & 256) != 0, item == 127 ? null : item);
                }).ToArray()));
            land = BattleUnitProjection.Land(current, actor, actor.Position!);
        }
        return new(current.SessionId, current.Revision, true, null, active.Actor, battle.Definition.Width,
            battle.Definition.Height, battle.Definition.SourceMapRegion,
            Array.AsReadOnly(Enumerable.Range(0, battle.Definition.Height).SelectMany(y =>
                Enumerable.Range(0, battle.Definition.Width).Select(x => battle.Definition.Terrain[y * 48 + x].Surface)).ToArray()),
            Array.AsReadOnly(markers), unit, land);
    }

    private static IReadOnlyList<string> Status(ushort word)
    {
        var result = new List<string>();
        if ((word & 4) != 0) result.Add("curse");
        if ((word & 2) != 0) result.Add("poison");
        if ((word & 1) != 0) result.Add("stun");
        if ((word & 0xC0) != 0) result.Add("sleep");
        if ((word & 0x300) != 0) result.Add("silence");
        if ((word & ~0x3C7) != 0) result.Add("Other status effect (details unavailable)");
        return result.AsReadOnly();
    }
}
