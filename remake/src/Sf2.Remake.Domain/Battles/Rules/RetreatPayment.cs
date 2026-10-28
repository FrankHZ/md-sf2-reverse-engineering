namespace Sf2.Remake.Domain.Battles;

// Only the two admitted retreat payments. Item payment names the selected packed slot,
// never a spell proxy or a search for the first matching item ID.
public abstract record RetreatPayment;
public sealed record SpellRetreatPayment(SpellRef Spell, byte MpCost) : RetreatPayment;
public sealed record ItemRetreatPayment(byte ItemId, string Name, int Slot, ushort Word) : RetreatPayment;
public sealed record RetreatItemDefinition(byte ItemId, string Name);
