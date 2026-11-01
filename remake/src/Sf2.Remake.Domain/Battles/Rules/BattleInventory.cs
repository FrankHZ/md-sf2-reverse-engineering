namespace Sf2.Remake.Domain.Battles;

// The four source words are shared by field and battle; neither caller needs the other's state.
internal static class BattleInventory
{
    internal static BattleSourceLoadout Remove(BattleSourceLoadout loadout, int slot)
    {
        var words = loadout.Items.ToList();
        if (slot < 0 || slot >= words.Count || (words[slot] & 127) == 127)
            throw new BattleRuleException("item-slot", "items.slot");
        words.RemoveAt(slot); words.Add(127);
        return new(words, loadout.Spells);
    }

    internal static BattleSourceLoadout Add(BattleSourceLoadout loadout, ushort word)
    {
        var words = loadout.Items.ToArray();
        int slot = Array.FindIndex(words, value => (value & 127) == 127);
        if (slot < 0) throw new BattleRuleException("give-full", "target.items");
        // Source AddItem keeps identity/broken, and clears equipped on the moved word.
        words[slot] = (ushort)(word & 0x17F);
        return new(words, loadout.Spells);
    }
}
