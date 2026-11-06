using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// Coordinates are battle-local; the receipt is the same durable story flag used by field chests.
public sealed record BattleChest(MapPosition Position, int Flag, byte? ItemId, string? ItemName, uint? Gold);
public sealed class BattleSearchDefinition
{
    internal BattleSearchDefinition(IEnumerable<BattleChest> chests) => Chests = Array.AsReadOnly(chests.ToArray());
    public IReadOnlyList<BattleChest> Chests { get; }
}
public enum BattleSearchKind { Item, Gold, Empty, Full, Occupied }
internal sealed record BattleSearchResult(EngineBattleState Battle, BattleSearchKind Kind, ActorRef? Occupant = null);

internal static class BattleSearch
{
    internal static BattleChest? Faced(EngineBattleState battle, ActorRef actor, MapPosition origin)
    {
        var offset = battle.GetActor(actor).Facing switch { 0 => (1, 0), 1 => (0, -1), 2 => (-1, 0), _ => (0, 1) };
        return battle.Definition.Search?.Chests.SingleOrDefault(chest =>
            chest.Position.X == origin.X + offset.Item1 && chest.Position.Y == origin.Y + offset.Item2);
    }

    internal static BattleSearchResult Prepare(EngineBattleState battle, ActorRef actorRef, BattleChest chest, bool opened)
    {
        var actor = battle.GetActor(actorRef);
        if (!actor.IsAlly || actor.Control != BattleControl.Player || actor.Hp == 0 || actor.Position is null || !actor.Initialized)
            throw new BattleRuleException("search-actor", "actor");
        var occupant = battle.Actors.FirstOrDefault(a => a.Actor != actorRef && a.Position == chest.Position);
        if (occupant is not null) return new(battle, BattleSearchKind.Occupied, occupant.Actor);
        if (opened || chest.ItemId is null && chest.Gold is null) return new(battle, BattleSearchKind.Empty);
        if (chest.Gold is { } gold)
        {
            if (battle.Gold is null) throw new BattleRuleException("search-gold-unknown", "battle.gold", true);
            return new(battle.With(gold: (uint)Math.Min(999999UL, (ulong)battle.Gold.Value + gold)), BattleSearchKind.Gold);
        }
        if (actor.SourceLoadout is not { Items.Count: 4 } loadout)
            throw new BattleRuleException("search-loadout", "actor.sourceLoadout", true);
        if (loadout.Items.All(word => (word & 127) != 127)) return new(battle, BattleSearchKind.Full);
        var recipient = actor.With(sourceLoadout: BattleInventory.Add(loadout, chest.ItemId!.Value));
        return new(battle.With(actors: battle.Actors.Select(a => a.Actor == actorRef ? recipient : a)), BattleSearchKind.Item);
    }
}
