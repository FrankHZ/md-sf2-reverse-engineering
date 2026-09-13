using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalMapCastleGateGuardMove
{
    public OriginalMapCastleGateGuardMove(
        int logicalActorId,
        OriginalMapEntityRecordIdentity sourceRecord,
        MapPosition source,
        ExplorationDirection direction,
        MapPosition destination)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(logicalActorId);
        SourceRecord = sourceRecord ?? throw new ArgumentNullException(nameof(sourceRecord));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        MapPosition expected = direction switch
        {
            ExplorationDirection.North => new(source.X, source.Y - 1),
            ExplorationDirection.East => new(source.X + 1, source.Y),
            ExplorationDirection.South => new(source.X, source.Y + 1),
            ExplorationDirection.West => new(source.X - 1, source.Y),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
        if (destination != expected)
        {
            throw new ArgumentException(
                "A castle-gate guard move must retain one exact cardinal source step.",
                nameof(destination));
        }

        LogicalActorId = logicalActorId;
        Direction = direction;
    }

    public int LogicalActorId { get; }

    public OriginalMapEntityRecordIdentity SourceRecord { get; }

    public MapPosition Source { get; }

    public ExplorationDirection Direction { get; }

    public MapPosition Destination { get; }
}

public enum OriginalMapCastleGateStage
{
    SetTextCursor537,
    BeginGuard138Actions,
    MoveGuard138RightOne,
    EndGuard138Actions,
    BeginGuard139ActionsAndWait,
    MoveGuard139LeftOne,
    EndCastleGateProgram,
}

public sealed record OriginalMapCastleGateDefinition
{
    private readonly ReadOnlyCollection<OriginalMapCastleGateGuardMove> _guardMoves;
    private readonly ReadOnlyCollection<OriginalMapCastleGateStage> _stages;
    private readonly ReadOnlyCollection<int> _projectionSourceOperationIndices;

    public OriginalMapCastleGateDefinition(
        OriginalMapZoneEventIdentity identity,
        MapPosition approach,
        ExplorationDirection entryDirection,
        MapPosition trigger,
        string programIdentity,
        string controlShapeSha256,
        int textCursorId,
        int completionFlag,
        int sourceOperationCount,
        IEnumerable<int> projectionSourceOperationIndices,
        IEnumerable<OriginalMapCastleGateGuardMove> guardMoves,
        IEnumerable<OriginalMapCastleGateStage> stages)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Approach = approach ?? throw new ArgumentNullException(nameof(approach));
        if (!Enum.IsDefined(entryDirection))
        {
            throw new ArgumentOutOfRangeException(nameof(entryDirection));
        }

        Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        ArgumentException.ThrowIfNullOrWhiteSpace(programIdentity);
        OriginalMapImportRequest.ValidateSha256(controlShapeSha256, nameof(controlShapeSha256));
        ArgumentOutOfRangeException.ThrowIfNegative(textCursorId);
        ArgumentOutOfRangeException.ThrowIfNegative(completionFlag);
        ArgumentOutOfRangeException.ThrowIfLessThan(sourceOperationCount, 1);

        ArgumentNullException.ThrowIfNull(guardMoves);
        OriginalMapCastleGateGuardMove[] copiedMoves = [.. guardMoves];
        if (copiedMoves.Length == 0 ||
            copiedMoves.Select(move => move.LogicalActorId).Distinct().Count() !=
                copiedMoves.Length ||
            copiedMoves.Select(move => move.SourceRecord).Distinct().Count() !=
                copiedMoves.Length)
        {
            throw new ArgumentException(
                "Castle-gate guard moves must be non-empty and distinctly bound.",
                nameof(guardMoves));
        }

        ArgumentNullException.ThrowIfNull(stages);
        OriginalMapCastleGateStage[] copiedStages = [.. stages];
        if (copiedStages.Length == 0 ||
            copiedStages.Any(stage => !Enum.IsDefined(stage)) ||
            copiedStages.Distinct().Count() != copiedStages.Length)
        {
            throw new ArgumentException(
                "Castle-gate stages must be a non-empty distinct typed sequence.",
                nameof(stages));
        }

        ArgumentNullException.ThrowIfNull(projectionSourceOperationIndices);
        int[] copiedIndices = [.. projectionSourceOperationIndices];
        if (copiedIndices.Length != copiedStages.Length ||
            copiedIndices.Any(index => index < 0 || index >= sourceOperationCount) ||
            copiedIndices.Distinct().Count() != copiedIndices.Length ||
            !copiedIndices.SequenceEqual(copiedIndices.Order()))
        {
            throw new ArgumentException(
                "The bounded castle-gate projection must map every stage to one ordered source operation.",
                nameof(projectionSourceOperationIndices));
        }

        EntryDirection = entryDirection;
        ProgramIdentity = programIdentity;
        ControlShapeSha256 = controlShapeSha256.ToUpperInvariant();
        TextCursorId = textCursorId;
        CompletionFlag = completionFlag;
        SourceOperationCount = sourceOperationCount;
        _projectionSourceOperationIndices = Array.AsReadOnly(copiedIndices);
        _guardMoves = Array.AsReadOnly(copiedMoves);
        _stages = Array.AsReadOnly(copiedStages);
    }

    public OriginalMapZoneEventIdentity Identity { get; }

    public MapPosition Approach { get; }

    public ExplorationDirection EntryDirection { get; }

    public MapPosition Trigger { get; }

    public string ProgramIdentity { get; }

    public string ControlShapeSha256 { get; }

    public int TextCursorId { get; }

    public int CompletionFlag { get; }

    public int SourceOperationCount { get; }

    public IReadOnlyList<int> ProjectionSourceOperationIndices =>
        _projectionSourceOperationIndices;

    public IReadOnlyList<OriginalMapCastleGateGuardMove> GuardMoves => _guardMoves;

    public IReadOnlyList<OriginalMapCastleGateStage> Stages => _stages;
}
