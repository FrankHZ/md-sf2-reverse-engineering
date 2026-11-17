namespace Sf2.Remake.Domain.Battles;

// Prepare both immutable stores before the caller publishes either one.
internal static class CaravanInventory
{
    internal static (BattleSourceLoadout Pack, BattleItemCustody Custody) Deposit(
        BattleSourceLoadout pack, BattleItemCustody custody, int slot, ushort expectedWord, bool cursed)
    {
        if (custody.Depot.Count == 64)
            throw new BattleRuleException("depot-full", "depot");
        ushort word = Outgoing(pack, slot, expectedWord, cursed);
        var stored = custody.WithDepot(custody.Depot.Append((byte)(word & 127)));
        return (BattleInventory.Remove(pack, slot), stored);
    }

    internal static (BattleSourceLoadout Pack, BattleItemCustody Custody) Derive(
        BattleSourceLoadout pack, BattleItemCustody custody, int index, byte expectedItem,
        int? returnSlot = null, ushort? expectedReturn = null, bool cursed = false)
    {
        byte item = Selected(custody, index, expectedItem);
        bool full = pack.Items.All(word => (word & 127) != 127);
        if (full != returnSlot.HasValue || returnSlot.HasValue != expectedReturn.HasValue)
            throw new BattleRuleException("depot-exchange-required", "pack");
        ushort? returned = null;
        if (returnSlot is { } slot)
        {
            returned = Outgoing(pack, slot, expectedReturn!.Value, cursed);
            pack = BattleInventory.Remove(pack, slot);
        }
        var stored = custody.Depot.ToList();
        stored.RemoveAt(index);
        pack = BattleInventory.Add(pack, item);
        if (returned is { } word)
            stored.Add((byte)(word & 127));
        return (pack, custody.WithDepot(stored));
    }

    internal static BattleItemCustody Drop(BattleItemCustody custody, int index, byte expectedItem,
        BattleDiscardItem terms)
    {
        byte item = Selected(custody, index, expectedItem);
        if (terms.ItemId != item)
            throw new BattleRuleException("depot-item", "item");
        if (terms.Unsellable)
            throw new BattleRuleException("depot-unsellable", "item");
        var stored = custody.Depot.ToList();
        stored.RemoveAt(index);
        var changed = custody.WithDepot(stored);
        return terms.Rare ? changed.AddDeal(item) : changed;
    }

    internal static byte Selected(BattleItemCustody custody, int index, byte expectedItem)
    {
        if (index < 0 || index >= custody.Depot.Count || custody.Depot[index] != expectedItem)
            throw new BattleRuleException("depot-stale-item", "depot.index");
        return expectedItem;
    }

    private static ushort Outgoing(BattleSourceLoadout pack, int slot, ushort expectedWord, bool cursed)
    {
        if (pack.Items.Count != 4 || pack.Items.Any(word => word > 511) || slot is < 0 or >= 4 ||
            pack.Items[slot] != expectedWord || (expectedWord & 127) == 127)
            throw new BattleRuleException("depot-stale-slot", "pack.slot");
        if (cursed && (expectedWord & 128) != 0)
            throw new BattleRuleException("depot-equipped-curse", "pack.slot");
        return expectedWord;
    }
}
