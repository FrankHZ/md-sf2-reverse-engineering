using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record PrivateOriginalMapCastleGateState
{
    private PrivateOriginalMapCastleGateState(
        OriginalMapZoneEventIdentity eventIdentity,
        bool opened,
        bool flag604Set)
    {
        EventIdentity = eventIdentity ?? throw new ArgumentNullException(nameof(eventIdentity));
        if (opened != flag604Set)
        {
            throw new ArgumentException(
                "Castle-gate state must retain the exact ready or opened shape.");
        }

        Opened = opened;
        Flag604Set = flag604Set;
    }

    public OriginalMapZoneEventIdentity EventIdentity { get; }

    public bool Opened { get; }

    public bool Flag604Set { get; }

    internal static PrivateOriginalMapCastleGateState Ready(
        OriginalMapCastleGateDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new(definition.Identity, opened: false, flag604Set: false);
    }

    internal static PrivateOriginalMapCastleGateState Completed(
        OriginalMapCastleGateDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new(definition.Identity, opened: true, flag604Set: true);
    }

    internal bool Matches(OriginalMapCastleGateDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        PrivateOriginalMapCastleGateState expected = Opened
            ? Completed(definition)
            : Ready(definition);
        return EventIdentity == expected.EventIdentity &&
            Opened == expected.Opened &&
            Flag604Set == expected.Flag604Set;
    }
}

public sealed record PrivateOriginalMapCastleGateReceipt
{
    private readonly ReadOnlyCollection<OriginalMapCastleGateGuardMove> _guardMoves;
    private readonly ReadOnlyCollection<OriginalMapCastleGateStage> _stages;
    private readonly ReadOnlyCollection<int> _projectionSourceOperationIndices;

    internal PrivateOriginalMapCastleGateReceipt(
        OriginalMapCastleGateDefinition definition,
        OriginalMapTraversalResult traversal,
        PrivateOriginalMapCastleGateState before,
        PrivateOriginalMapCastleGateState after,
        long simulationStep)
    {
        ArgumentNullException.ThrowIfNull(definition);
        Traversal = traversal ?? throw new ArgumentNullException(nameof(traversal));
        Before = before ?? throw new ArgumentNullException(nameof(before));
        After = after ?? throw new ArgumentNullException(nameof(after));
        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        EventIdentity = definition.Identity;
        ProgramIdentity = definition.ProgramIdentity;
        ControlShapeSha256 = definition.ControlShapeSha256;
        TextCursorId = definition.TextCursorId;
        CompletionFlag = definition.CompletionFlag;
        SourceOperationCount = definition.SourceOperationCount;
        _projectionSourceOperationIndices = Array.AsReadOnly(
            definition.ProjectionSourceOperationIndices.ToArray());
        _guardMoves = Array.AsReadOnly(definition.GuardMoves.ToArray());
        _stages = Array.AsReadOnly(definition.Stages.ToArray());
        SimulationStep = simulationStep;
    }

    public OriginalMapZoneEventIdentity EventIdentity { get; }

    public OriginalMapTraversalResult Traversal { get; }

    public string ProgramIdentity { get; }

    public string ControlShapeSha256 { get; }

    public int TextCursorId { get; }

    public int CompletionFlag { get; }

    public int SourceOperationCount { get; }

    public IReadOnlyList<int> ProjectionSourceOperationIndices =>
        _projectionSourceOperationIndices;

    public IReadOnlyList<OriginalMapCastleGateGuardMove> GuardMoves => _guardMoves;

    public IReadOnlyList<OriginalMapCastleGateStage> Stages => _stages;

    public PrivateOriginalMapCastleGateState Before { get; }

    public PrivateOriginalMapCastleGateState After { get; }

    public long SimulationStep { get; }

    internal bool Matches(OriginalMapCastleGateDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return EventIdentity == definition.Identity &&
            Traversal.Outcome == OriginalMapTraversalOutcome.Moved &&
            Traversal.Source == definition.Approach &&
            Traversal.Direction == definition.EntryDirection &&
            Traversal.Position == definition.Trigger &&
            string.Equals(ProgramIdentity, definition.ProgramIdentity, StringComparison.Ordinal) &&
            string.Equals(
                ControlShapeSha256,
                definition.ControlShapeSha256,
                StringComparison.OrdinalIgnoreCase) &&
            TextCursorId == definition.TextCursorId &&
            CompletionFlag == definition.CompletionFlag &&
            SourceOperationCount == definition.SourceOperationCount &&
            ProjectionSourceOperationIndices.SequenceEqual(
                definition.ProjectionSourceOperationIndices) &&
            GuardMoves.SequenceEqual(definition.GuardMoves) &&
            Stages.SequenceEqual(definition.Stages) &&
            Before.Matches(definition) && !Before.Opened &&
            After.Matches(definition) && After.Opened;
    }
}
