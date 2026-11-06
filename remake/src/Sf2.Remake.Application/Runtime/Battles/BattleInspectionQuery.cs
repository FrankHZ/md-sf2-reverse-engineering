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
public sealed record BattleInspectionLand(ActorRef Actor, MapPosition Position, BattleMover Mover, int Percent);
public sealed record BattleInspectionSnapshot(Guid SessionId, long Revision, bool Available, string? Reason,
    ActorRef? ActiveActor, int Width, int Height, BattleMapRegion? Region,
    IReadOnlyList<TerrainSurface> Terrain, IReadOnlyList<BattleInspectionMarker> Units, BattleInspectionUnit? Unit, BattleInspectionLand? Land);

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
        int? NameIndex(BattleActorState actor) => actor.IsAlly ? actor.Deployment.Initialization?.AllyPartyMember :
            scenes?.EnemyNameIds?.TryGetValue(actor.Actor, out byte enemy) == true ? enemy : null;
        string Name(BattleActorState actor)
        {
            int member = actor.Deployment.Initialization?.AllyPartyMember ?? actor.ProcessingOrder;
            bool admitted = actor.IsAlly
                ? scenes is not null &&
                    member >= 0 && member < scenes.MemberNames.Count && !string.IsNullOrWhiteSpace(scenes.MemberNames[member])
                : scenes?.EnemyNames?.ContainsKey(actor.Actor) == true;
            return definition.PrivateDefinitions is null || admitted || current.Story.Names.ContainsKey(actor.Actor.Value)
                ? CharacterNames.Battle(current.Story, actor, scenes) : "Name unavailable";
        }
        var actors = battle.Actors.Where(a => a.Hp > 0 && a.Initialized && BattleVisibility.OnBoard(current, a)).ToArray();
        var markers = actors.Select(a => new BattleInspectionMarker(a.Actor, a.Position!, a.IsAlly,
            Name(a), NameIndex(a), current.Story.Names.GetValueOrDefault(a.Actor.Value))).ToArray();
        var actor = requested is null ? null : actors.FirstOrDefault(a => a.Actor == requested);
        if (requested is not null && actor is null) return Reject("unit-not-disclosed");
        BattleInspectionUnit? unit = null;
        BattleInspectionLand? land = null;
        if (actor is not null)
        {
            byte? classId = actor.IsAlly ? actor.Definition.Growth?.ClassId : null;
            string? expression = classId is { } key ? definition.PrivateDefinitions?.Classes.GetValueOrDefault(key)?.NameExpression : null;
            string? className = expression is { Length: >= 2 } && expression[0] == '"' && expression[^1] == '"' ? expression[1..^1] : null;
            // Source-like unknown values are unavailable, never diagnostic numbers.
            int? Stat(int value) => definition.PrivateDefinitions is not null && value >= 100 ? null : value;
            unit = new(actor.Actor, actor.Position!, actor.IsAlly, Name(actor),
                NameIndex(actor), current.Story.Names.GetValueOrDefault(actor.Actor.Value), classId, className,
                Stat(actor.Hp), Stat(actor.MaxHp), Stat(actor.Mp), Stat(actor.MaxMp), actor.IsAlly ? actor.Level : null,
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
            land = new(actor.Actor, actor.Position!, actor.Definition.Mover,
                BattleTerrainRules.LandEffect(battle.Definition.Terrain[actor.Position!.Y * 48 + actor.Position.X], actor.Definition.Mover));
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
