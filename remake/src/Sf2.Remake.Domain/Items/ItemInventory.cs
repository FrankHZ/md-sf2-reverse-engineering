using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Items;

public sealed record ItemId
{
    public ItemId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed class ItemInventoryState
{
    private readonly ReadOnlyCollection<ItemId> _items;
    private readonly HashSet<ItemId> _itemLookup;

    public ItemInventoryState(IEnumerable<ItemId> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        List<ItemId> copiedItems = [];
        _itemLookup = [];
        foreach (ItemId item in items)
        {
            ItemId admittedItem = item ?? throw new ArgumentException(
                "Inventory items cannot contain null values.",
                nameof(items));
            if (!_itemLookup.Add(admittedItem))
            {
                throw new ArgumentException(
                    $"Duplicate inventory item '{admittedItem}'.",
                    nameof(items));
            }

            copiedItems.Add(admittedItem);
        }

        _items = copiedItems.AsReadOnly();
    }

    public IReadOnlyList<ItemId> Items => _items;

    public bool Contains(ItemId item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _itemLookup.Contains(item);
    }
}

public enum ItemAcquisitionOutcome
{
    Acquired,
    AlreadyOwned,
}

public sealed record ItemAcquisitionResult
{
    public ItemAcquisitionResult(
        ItemInventoryState state,
        ItemId item,
        ItemAcquisitionOutcome outcome)
    {
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }

        State = state ?? throw new ArgumentNullException(nameof(state));
        Item = item ?? throw new ArgumentNullException(nameof(item));
        Outcome = outcome;
    }

    public ItemInventoryState State { get; }

    public ItemId Item { get; }

    public ItemAcquisitionOutcome Outcome { get; }
}

public static class ItemInventoryReducer
{
    public static ItemAcquisitionResult TryAcquireUnique(
        ItemInventoryState state,
        ItemId item)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(item);
        if (state.Contains(item))
        {
            return new ItemAcquisitionResult(
                state,
                item,
                ItemAcquisitionOutcome.AlreadyOwned);
        }

        return new ItemAcquisitionResult(
            new ItemInventoryState(state.Items.Append(item)),
            item,
            ItemAcquisitionOutcome.Acquired);
    }
}
