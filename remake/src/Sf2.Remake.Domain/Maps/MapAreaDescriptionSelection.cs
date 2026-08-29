using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Maps;

public sealed record AreaDescriptionFunctionId
{
    public AreaDescriptionFunctionId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public enum AreaDescriptionCondition
{
    Always,
    RequiresConditionedAdmission,
}

public enum AreaDescriptionAdmission
{
    Ordinary,
    AllowConditioned,
}

public enum AreaDescriptionPayloadKind
{
    Text,
    Function,
}

public sealed record AreaDescriptionPayload
{
    private AreaDescriptionPayload(
        AreaDescriptionPayloadKind kind,
        int investigationOffset,
        int descriptionOffset,
        AreaDescriptionFunctionId? function)
    {
        Kind = kind;
        InvestigationOffset = investigationOffset;
        DescriptionOffset = descriptionOffset;
        Function = function;
    }

    public AreaDescriptionPayloadKind Kind { get; }

    public int InvestigationOffset { get; }

    public int DescriptionOffset { get; }

    public AreaDescriptionFunctionId? Function { get; }

    public static AreaDescriptionPayload Text(int investigationOffset, int descriptionOffset)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(investigationOffset);
        ArgumentOutOfRangeException.ThrowIfNegative(descriptionOffset);
        return new(
            AreaDescriptionPayloadKind.Text,
            investigationOffset,
            descriptionOffset,
            function: null);
    }

    public static AreaDescriptionPayload FunctionTarget(AreaDescriptionFunctionId function) =>
        new(
            AreaDescriptionPayloadKind.Function,
            investigationOffset: 0,
            descriptionOffset: 0,
            function ?? throw new ArgumentNullException(nameof(function)));
}

public sealed record MapAreaDescriptionEntry
{
    public MapAreaDescriptionEntry(
        byte x,
        byte y,
        AreaDescriptionCondition condition,
        AreaDescriptionPayload payload)
    {
        if (!Enum.IsDefined(condition))
        {
            throw new ArgumentOutOfRangeException(nameof(condition));
        }

        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        if (condition == AreaDescriptionCondition.RequiresConditionedAdmission &&
            payload.Kind != AreaDescriptionPayloadKind.Function)
        {
            throw new ArgumentException(
                "Conditioned area-description entries must use a function target.",
                nameof(payload));
        }

        X = x;
        Y = y;
        Condition = condition;
    }

    public byte X { get; }

    public byte Y { get; }

    public AreaDescriptionCondition Condition { get; }

    public AreaDescriptionPayload Payload { get; }
}

public sealed class MapAreaDescriptionSource
{
    private readonly ReadOnlyCollection<MapAreaDescriptionEntry> _entries;

    private MapAreaDescriptionSource(
        bool isDirectReturn,
        int descriptionTextBase,
        IEnumerable<MapAreaDescriptionEntry> entries)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(descriptionTextBase);
        ArgumentNullException.ThrowIfNull(entries);

        List<MapAreaDescriptionEntry> copiedEntries = [];
        foreach (MapAreaDescriptionEntry entry in entries)
        {
            copiedEntries.Add(
                entry ?? throw new ArgumentException(
                    "Area-description tables cannot contain null entries.",
                    nameof(entries)));
        }

        IsDirectReturn = isDirectReturn;
        DescriptionTextBase = descriptionTextBase;
        _entries = copiedEntries.AsReadOnly();
    }

    public bool IsDirectReturn { get; }

    public int DescriptionTextBase { get; }

    public IReadOnlyList<MapAreaDescriptionEntry> Entries => _entries;

    public static MapAreaDescriptionSource DirectReturn() =>
        new(isDirectReturn: true, descriptionTextBase: 0, entries: []);

    public static MapAreaDescriptionSource Table(
        int descriptionTextBase,
        IEnumerable<MapAreaDescriptionEntry> entries) =>
        new(isDirectReturn: false, descriptionTextBase, entries);
}

public readonly record struct MapAreaDescriptionQuery(
    byte X,
    byte Y,
    AreaDescriptionAdmission Admission);

public enum AreaDescriptionSelectionKind
{
    NoMatch,
    Text,
    Function,
}

public sealed record AreaDescriptionSelection
{
    private AreaDescriptionSelection(
        AreaDescriptionSelectionKind kind,
        int? investigationTextIndex,
        int? descriptionTextIndex,
        AreaDescriptionFunctionId? function)
    {
        Kind = kind;
        InvestigationTextIndex = investigationTextIndex;
        DescriptionTextIndex = descriptionTextIndex;
        Function = function;
    }

    public AreaDescriptionSelectionKind Kind { get; }

    public int? InvestigationTextIndex { get; }

    public int? DescriptionTextIndex { get; }

    public AreaDescriptionFunctionId? Function { get; }

    public static AreaDescriptionSelection NoMatch { get; } =
        new(AreaDescriptionSelectionKind.NoMatch, null, null, null);

    internal static AreaDescriptionSelection Text(
        int investigationTextIndex,
        int descriptionTextIndex) =>
        new(
            AreaDescriptionSelectionKind.Text,
            investigationTextIndex,
            descriptionTextIndex,
            function: null);

    internal static AreaDescriptionSelection FunctionTarget(AreaDescriptionFunctionId function) =>
        new(AreaDescriptionSelectionKind.Function, null, null, function);
}

public static class MapAreaDescriptionSelector
{
    public const int InvestigationTextIndexBase = 423;

    public static AreaDescriptionSelection Select(
        MapAreaDescriptionSource source,
        MapAreaDescriptionQuery query)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!Enum.IsDefined(query.Admission))
        {
            throw new ArgumentOutOfRangeException(nameof(query), "Unknown condition admission.");
        }

        if (source.IsDirectReturn)
        {
            return AreaDescriptionSelection.NoMatch;
        }

        MapAreaDescriptionEntry? selected = source.Entries.FirstOrDefault(
            entry =>
                entry.X == query.X &&
                entry.Y == query.Y &&
                IsAdmitted(entry.Condition, query.Admission));
        if (selected is null)
        {
            return AreaDescriptionSelection.NoMatch;
        }

        return selected.Payload.Kind switch
        {
            AreaDescriptionPayloadKind.Text => AreaDescriptionSelection.Text(
                checked(InvestigationTextIndexBase + selected.Payload.InvestigationOffset),
                checked(source.DescriptionTextBase + selected.Payload.DescriptionOffset)),
            AreaDescriptionPayloadKind.Function => AreaDescriptionSelection.FunctionTarget(
                selected.Payload.Function ?? throw new InvalidOperationException(
                    "Function payload is missing its target.")),
            _ => throw new InvalidOperationException("Unknown area-description payload kind."),
        };
    }

    private static bool IsAdmitted(
        AreaDescriptionCondition condition,
        AreaDescriptionAdmission admission) =>
        condition == AreaDescriptionCondition.Always ||
        admission == AreaDescriptionAdmission.AllowConditioned;
}
