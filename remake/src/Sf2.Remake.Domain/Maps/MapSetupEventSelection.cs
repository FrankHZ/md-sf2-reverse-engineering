using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Maps;

public sealed record EventTargetId
{
    public EventTargetId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public readonly record struct EventFieldMatch(byte? ExactValue)
{
    public static EventFieldMatch Any { get; } = new(null);

    public static EventFieldMatch Exact(byte value) => new(value);

    public bool Matches(byte value) => ExactValue is null || ExactValue == value;
}

public interface IMapSetupEventRecord
{
    bool IsDefault { get; }
}

public sealed record EntityEventRecord : IMapSetupEventRecord
{
    private EntityEventRecord(byte? entity, byte eventFlags, EventTargetId target)
    {
        Entity = entity;
        EventFlags = eventFlags;
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public byte? Entity { get; }

    public byte EventFlags { get; }

    public EventTargetId Target { get; }

    public bool IsDefault => Entity is null;

    public static EntityEventRecord Specific(byte entity, byte eventFlags, EventTargetId target) =>
        new(entity, eventFlags, target);

    public static EntityEventRecord Default(byte eventFlags, EventTargetId target) =>
        new(null, eventFlags, target);
}

public sealed record ZoneEventRecord : IMapSetupEventRecord
{
    private ZoneEventRecord(
        bool isDefault,
        EventFieldMatch x,
        EventFieldMatch y,
        EventTargetId target)
    {
        IsDefault = isDefault;
        X = x;
        Y = y;
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public bool IsDefault { get; }

    public EventFieldMatch X { get; }

    public EventFieldMatch Y { get; }

    public EventTargetId Target { get; }

    public static ZoneEventRecord Specific(
        EventFieldMatch x,
        EventFieldMatch y,
        EventTargetId target) => new(false, x, y, target);

    public static ZoneEventRecord Default(EventTargetId target) =>
        new(true, default, default, target);
}

public sealed record ItemEventRecord : IMapSetupEventRecord
{
    private ItemEventRecord(
        bool isDefault,
        EventFieldMatch x,
        EventFieldMatch y,
        EventFieldMatch facing,
        byte itemIndex,
        EventTargetId target)
    {
        if (!isDefault && itemIndex > MapSetupEventSelector.ItemIndexMask)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemIndex),
                itemIndex,
                "Item event indexes must already be normalized.");
        }

        IsDefault = isDefault;
        X = x;
        Y = y;
        Facing = facing;
        ItemIndex = itemIndex;
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public bool IsDefault { get; }

    public EventFieldMatch X { get; }

    public EventFieldMatch Y { get; }

    public EventFieldMatch Facing { get; }

    public byte ItemIndex { get; }

    public EventTargetId Target { get; }

    public static ItemEventRecord Specific(
        EventFieldMatch x,
        EventFieldMatch y,
        EventFieldMatch facing,
        byte itemIndex,
        EventTargetId target) => new(false, x, y, facing, itemIndex, target);

    public static ItemEventRecord Default(EventTargetId target) =>
        new(true, default, default, default, default, target);
}

public sealed class MapSetupEventTable<TRecord>
    where TRecord : IMapSetupEventRecord
{
    private readonly ReadOnlyCollection<TRecord> _records;

    public MapSetupEventTable(IEnumerable<TRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        List<TRecord> copiedRecords = [];
        int defaultCount = 0;
        foreach (TRecord record in records)
        {
            if (record is null)
            {
                throw new ArgumentException(
                    "Event tables cannot contain null records.",
                    nameof(records));
            }

            copiedRecords.Add(record);
            if (record.IsDefault)
            {
                defaultCount++;
            }
        }

        if (defaultCount != 1)
        {
            throw new ArgumentException(
                "Event tables must contain exactly one default record.",
                nameof(records));
        }

        _records = copiedRecords.AsReadOnly();
    }

    public IReadOnlyList<TRecord> Records => _records;
}

public readonly record struct EntityEventQuery(byte Entity);

public readonly record struct ZoneEventQuery(byte X, byte Y);

public readonly record struct ItemEventQuery(byte X, byte Y, byte Facing, byte Item);

public sealed record EntityEventSelection(EventTargetId Target, byte EventFlags);

public sealed record ZoneEventSelection(EventTargetId Target);

public sealed record ItemEventSelection(EventTargetId Target, byte ItemIndex);

public static class MapSetupEventSelector
{
    public const byte ItemIndexMask = 0x7F;

    public static EntityEventSelection Select(
        MapSetupEventTable<EntityEventRecord> table,
        EntityEventQuery query)
    {
        ArgumentNullException.ThrowIfNull(table);

        EntityEventRecord defaultRecord = table.Records.Single(record => record.IsDefault);
        EntityEventRecord selected = table.Records.FirstOrDefault(
            record => !record.IsDefault && record.Entity == query.Entity) ?? defaultRecord;
        return new EntityEventSelection(selected.Target, selected.EventFlags);
    }

    public static ZoneEventSelection Select(
        MapSetupEventTable<ZoneEventRecord> table,
        ZoneEventQuery query)
    {
        ArgumentNullException.ThrowIfNull(table);

        ZoneEventRecord defaultRecord = table.Records.Single(record => record.IsDefault);
        ZoneEventRecord selected = table.Records.FirstOrDefault(
            record =>
                !record.IsDefault &&
                record.X.Matches(query.X) &&
                record.Y.Matches(query.Y)) ?? defaultRecord;
        return new ZoneEventSelection(selected.Target);
    }

    public static ItemEventSelection Select(
        MapSetupEventTable<ItemEventRecord> table,
        ItemEventQuery query)
    {
        ArgumentNullException.ThrowIfNull(table);

        byte normalizedItem = (byte)(query.Item & ItemIndexMask);
        ItemEventRecord defaultRecord = table.Records.Single(record => record.IsDefault);
        ItemEventRecord selected = table.Records.FirstOrDefault(
            record =>
                !record.IsDefault &&
                record.X.Matches(query.X) &&
                record.Y.Matches(query.Y) &&
                record.Facing.Matches(query.Facing) &&
                record.ItemIndex == normalizedItem) ?? defaultRecord;
        return new ItemEventSelection(selected.Target, normalizedItem);
    }
}
