using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content.Scenarios;

// Source item flags are field receipts. They are ordinary durable story flags,
// never BattleItemCustody's drop flags. Layout is reconstructed from these flags.
public sealed record FieldItemPlacement(MapPosition Position, int Flag, ushort? ItemWord, uint? Gold);
public sealed record FieldAreaDescription(MapPosition Position, bool MenuOnly, IReadOnlyList<int> Texts);
public sealed class FieldSearchDefinition
{
    internal FieldSearchDefinition(IEnumerable<FieldItemPlacement> chests, IEnumerable<FieldItemPlacement> otherItems,
        IEnumerable<FieldAreaDescription> descriptions, ushort closedChest, ushort openChest)
    {
        Chests = Array.AsReadOnly(chests.ToArray()); OtherItems = Array.AsReadOnly(otherItems.ToArray());
        Descriptions = Array.AsReadOnly(descriptions.ToArray()); ClosedChest = closedChest; OpenChest = openChest;
    }
    public IReadOnlyList<FieldItemPlacement> Chests { get; }
    public IReadOnlyList<FieldItemPlacement> OtherItems { get; }
    public IReadOnlyList<FieldAreaDescription> Descriptions { get; }
    public ushort ClosedChest { get; }
    public ushort OpenChest { get; }
}
