using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Battles;

// One immutable session value: the active battle owns it, then transfers it to the
// exploration party. Null at either boundary means unobserved, never an empty store.
public sealed class BattleItemCustody
{
    internal BattleItemCustody(IEnumerable<byte> onceFlags, IReadOnlyDictionary<byte, byte> deals)
    {
        OnceFlags = Array.AsReadOnly(onceFlags.Distinct().Order().ToArray());
        Deals = new ReadOnlyDictionary<byte, byte>(deals.ToDictionary(pair => pair.Key, pair => pair.Value));
        if (OnceFlags.Any(flag => flag > 29) || Deals.Any(pair => pair.Key >= 127 || pair.Value > 15))
            throw new ArgumentOutOfRangeException(nameof(deals));
    }
    public IReadOnlyList<byte> OnceFlags { get; }
    public IReadOnlyDictionary<byte, byte> Deals { get; }
    internal BattleItemCustody Mark(byte flag) => new(OnceFlags.Append(flag), Deals);
    internal BattleItemCustody AddDeal(byte item) => new(OnceFlags,
        Deals.Where(pair => pair.Key != item).Append(new KeyValuePair<byte, byte>(item,
            (byte)Math.Min(15, Deals.GetValueOrDefault(item) + 1))).ToDictionary());
}

public sealed record EnemyDropItem(byte ItemId, string Name, bool Rare);
public sealed record EnemyDropRow(byte Battle, byte EnemySlot, byte ItemId, byte OnceFlag);

public sealed class EnemyItemDropDefinition
{
    internal EnemyItemDropDefinition(byte battle, IReadOnlyDictionary<ActorRef, byte> enemySlots,
        IEnumerable<EnemyDropRow> rows, IEnumerable<EnemyDropItem> items)
    {
        Battle = battle;
        EnemySlots = new ReadOnlyDictionary<ActorRef, byte>(enemySlots.ToDictionary());
        Rows = Array.AsReadOnly(rows.ToArray());
        Items = new ReadOnlyDictionary<byte, EnemyDropItem>(items.ToDictionary(item => item.ItemId));
    }
    public byte Battle { get; }
    public IReadOnlyDictionary<ActorRef, byte> EnemySlots { get; }
    public IReadOnlyList<EnemyDropRow> Rows { get; }
    public IReadOnlyDictionary<byte, EnemyDropItem> Items { get; }
}

public sealed record BattleItemReceipt(ActorRef Recipient, ActorRef Enemy, EnemyDropItem Item);
