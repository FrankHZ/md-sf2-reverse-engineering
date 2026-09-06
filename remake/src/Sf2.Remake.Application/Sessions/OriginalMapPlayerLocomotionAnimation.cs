using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum PrivateOriginalMapPlayerLocomotionPhase
{
    Admission,
    Blocked,
    Moving,
    Settled,
    Relocated,
    ScriptedEndpoint,
}

public enum PrivateOriginalMapPlayerLocomotionSheet
{
    Up,
    Horizontal,
    Down,
}

public sealed record PrivateOriginalMapPlayerLocomotionSnapshot
{
    public const int SourceUnitsPerMapTile = 384;
    public const int SourceUnitsPerMovementTick = 32;
    public const int SuccessfulMovementTickCount = 13;

    private const int HalfThreshold = 15;
    private const int CounterResetAbove = 30;

    private PrivateOriginalMapPlayerLocomotionSnapshot(
        PrivateOriginalMapPlayerLocomotionPhase phase,
        ExplorationDirection direction,
        byte opaqueFacing,
        PrivateOriginalMapPlayerLocomotionSheet sheet,
        int sourceSlot,
        bool horizontalMirror,
        int tick,
        int counterAtSelection,
        int storedCounter,
        int selectedHalf,
        MapPosition sourcePosition,
        MapPosition destinationPosition,
        int offsetXUnits,
        int offsetYUnits)
    {
        if (!Enum.IsDefined(phase))
        {
            throw new ArgumentOutOfRangeException(nameof(phase));
        }

        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        if (!Enum.IsDefined(sheet))
        {
            throw new ArgumentOutOfRangeException(nameof(sheet));
        }

        if (sourceSlot is < 0 or > 2)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceSlot));
        }

        if (tick is < 0 or > SuccessfulMovementTickCount)
        {
            throw new ArgumentOutOfRangeException(nameof(tick));
        }

        if (counterAtSelection is < 0 or > CounterResetAbove + 1 ||
            storedCounter is < 0 or > CounterResetAbove)
        {
            throw new ArgumentOutOfRangeException(nameof(counterAtSelection));
        }

        if (selectedHalf is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(selectedHalf));
        }

        SourcePosition = sourcePosition ?? throw new ArgumentNullException(nameof(sourcePosition));
        DestinationPosition = destinationPosition ??
            throw new ArgumentNullException(nameof(destinationPosition));
        if (Math.Abs(offsetXUnits) > SourceUnitsPerMapTile ||
            Math.Abs(offsetYUnits) > SourceUnitsPerMapTile ||
            (offsetXUnits != 0 && offsetYUnits != 0))
        {
            throw new ArgumentOutOfRangeException(nameof(offsetXUnits));
        }

        Phase = phase;
        Direction = direction;
        OpaqueFacing = opaqueFacing;
        Sheet = sheet;
        SourceSlot = sourceSlot;
        HorizontalMirror = horizontalMirror;
        Tick = tick;
        CounterAtSelection = counterAtSelection;
        StoredCounter = storedCounter;
        SelectedHalf = selectedHalf;
        OffsetXUnits = offsetXUnits;
        OffsetYUnits = offsetYUnits;
    }

    public PrivateOriginalMapPlayerLocomotionPhase Phase { get; }

    public ExplorationDirection Direction { get; }

    public byte OpaqueFacing { get; }

    public PrivateOriginalMapPlayerLocomotionSheet Sheet { get; }

    public int SourceSlot { get; }

    public bool HorizontalMirror { get; }

    public int Tick { get; }

    public int CounterAtSelection { get; }

    public int StoredCounter { get; }

    public int SelectedHalf { get; }

    public MapPosition SourcePosition { get; }

    public MapPosition DestinationPosition { get; }

    public int OffsetXUnits { get; }

    public int OffsetYUnits { get; }

    public bool IsMoving => Phase == PrivateOriginalMapPlayerLocomotionPhase.Moving;

    internal static PrivateOriginalMapPlayerLocomotionSnapshot ControlledAdmission(
        MapPosition position) =>
        new(
            PrivateOriginalMapPlayerLocomotionPhase.Admission,
            ExplorationDirection.South,
            opaqueFacing: 3,
            PrivateOriginalMapPlayerLocomotionSheet.Down,
            sourceSlot: 2,
            horizontalMirror: false,
            tick: 0,
            counterAtSelection: 25,
            storedCounter: 26,
            selectedHalf: 1,
            position,
            position,
            offsetXUnits: 0,
            offsetYUnits: 0);

    internal static PrivateOriginalMapPlayerLocomotionSnapshot Begin(
        PrivateOriginalMapPlayerLocomotionSnapshot current,
        ExplorationDirection direction,
        MapPosition sourcePosition,
        OriginalMapTraversalResult traversal)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(sourcePosition);
        ArgumentNullException.ThrowIfNull(traversal);
        if (current.IsMoving)
        {
            throw new InvalidOperationException(
                "A private original-map player locomotion cycle is already active.");
        }

        if (traversal.Source != sourcePosition)
        {
            throw new ArgumentException(
                "The traversal result must begin at the authoritative source position.",
                nameof(traversal));
        }

        (byte facing, PrivateOriginalMapPlayerLocomotionSheet sheet, int slot, bool mirror) =
            Selection(direction);
        int counterAtSelection = current.StoredCounter;
        int storedCounter = IncrementSpriteCounter(counterAtSelection);
        bool moved = traversal.Outcome == OriginalMapTraversalOutcome.Moved;
        return new PrivateOriginalMapPlayerLocomotionSnapshot(
            moved
                ? PrivateOriginalMapPlayerLocomotionPhase.Moving
                : PrivateOriginalMapPlayerLocomotionPhase.Blocked,
            direction,
            facing,
            sheet,
            slot,
            mirror,
            tick: 1,
            counterAtSelection,
            storedCounter,
            SelectHalf(counterAtSelection),
            sourcePosition,
            traversal.Position,
            offsetXUnits: 0,
            offsetYUnits: 0);
    }

    internal static PrivateOriginalMapPlayerLocomotionSnapshot Relocate(
        PrivateOriginalMapPlayerLocomotionSnapshot current,
        PrivateOriginalMapSameMapWarpReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(receipt);
        ExplorationDirection direction = receipt.OpaqueFacing switch
        {
            0 => ExplorationDirection.East,
            1 => ExplorationDirection.North,
            2 => ExplorationDirection.West,
            3 => ExplorationDirection.South,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt)),
        };
        (byte facing, PrivateOriginalMapPlayerLocomotionSheet sheet, int slot, bool mirror) =
            Selection(direction);
        return new PrivateOriginalMapPlayerLocomotionSnapshot(
            PrivateOriginalMapPlayerLocomotionPhase.Relocated,
            direction,
            facing,
            sheet,
            slot,
            mirror,
            tick: 0,
            counterAtSelection: current.StoredCounter,
            storedCounter: current.StoredCounter,
            selectedHalf: current.SelectedHalf,
            receipt.Source,
            receipt.Destination,
            offsetXUnits: 0,
            offsetYUnits: 0);
    }

    internal static PrivateOriginalMapPlayerLocomotionSnapshot Relocate(
        PrivateOriginalMapPlayerLocomotionSnapshot current,
        PrivateOriginalMapCrossMapTransitionReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(receipt);
        ExplorationDirection direction = DirectionFromOpaqueFacing(
            receipt.DestinationOpaqueFacing,
            nameof(receipt));
        (byte facing, PrivateOriginalMapPlayerLocomotionSheet sheet, int slot, bool mirror) =
            Selection(direction);
        return new PrivateOriginalMapPlayerLocomotionSnapshot(
            PrivateOriginalMapPlayerLocomotionPhase.Relocated,
            direction,
            facing,
            sheet,
            slot,
            mirror,
            tick: 0,
            counterAtSelection: current.StoredCounter,
            storedCounter: current.StoredCounter,
            selectedHalf: current.SelectedHalf,
            receipt.Source,
            receipt.Destination,
            offsetXUnits: 0,
            offsetYUnits: 0);
    }

    internal static PrivateOriginalMapPlayerLocomotionSnapshot CompleteMessengerAcceptance(
        PrivateOriginalMapPlayerLocomotionSnapshot current,
        PrivateOriginalMapMessengerAcceptanceReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(receipt);
        ExplorationDirection direction = receipt.EndpointOpaqueFacing switch
        {
            0 => ExplorationDirection.East,
            1 => ExplorationDirection.North,
            2 => ExplorationDirection.West,
            3 => ExplorationDirection.South,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt)),
        };
        (byte facing, PrivateOriginalMapPlayerLocomotionSheet sheet, int slot, bool mirror) =
            Selection(direction);
        return new PrivateOriginalMapPlayerLocomotionSnapshot(
            PrivateOriginalMapPlayerLocomotionPhase.ScriptedEndpoint,
            direction,
            facing,
            sheet,
            slot,
            mirror,
            tick: 0,
            counterAtSelection: current.StoredCounter,
            storedCounter: current.StoredCounter,
            selectedHalf: current.SelectedHalf,
            receipt.PlayerSource,
            receipt.Endpoint,
            offsetXUnits: 0,
            offsetYUnits: 0);
    }

    internal static PrivateOriginalMapPlayerLocomotionSnapshot CompletePalaceFirstVisit(
        PrivateOriginalMapPlayerLocomotionSnapshot current,
        PrivateOriginalMapPalaceFirstVisitReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(receipt);
        ExplorationDirection direction = DirectionFromOpaqueFacing(receipt.PlayerOpaqueFacing, nameof(receipt));
        (byte facing, PrivateOriginalMapPlayerLocomotionSheet sheet, int slot, bool mirror) = Selection(direction);
        return new PrivateOriginalMapPlayerLocomotionSnapshot(
            PrivateOriginalMapPlayerLocomotionPhase.ScriptedEndpoint, direction, facing, sheet, slot, mirror,
            tick: 0, counterAtSelection: current.StoredCounter, storedCounter: current.StoredCounter,
            selectedHalf: current.SelectedHalf, receipt.PlayerSource, receipt.PlayerEndpoint,
            offsetXUnits: 0, offsetYUnits: 0);
    }

    internal PrivateOriginalMapPlayerLocomotionSnapshot Advance()
    {
        if (!IsMoving)
        {
            throw new InvalidOperationException(
                "Only an active private original-map locomotion cycle can advance.");
        }

        int nextTick = checked(Tick + 1);
        int counterAtSelection = IncrementMovementCounter(StoredCounter);
        int storedCounter = IncrementSpriteCounter(counterAtSelection);
        int distance = checked((nextTick - 1) * SourceUnitsPerMovementTick);
        (int deltaX, int deltaY) = DirectionDelta(Direction);
        bool settled = nextTick == SuccessfulMovementTickCount;
        return new PrivateOriginalMapPlayerLocomotionSnapshot(
            settled
                ? PrivateOriginalMapPlayerLocomotionPhase.Settled
                : PrivateOriginalMapPlayerLocomotionPhase.Moving,
            Direction,
            OpaqueFacing,
            Sheet,
            SourceSlot,
            HorizontalMirror,
            nextTick,
            counterAtSelection,
            storedCounter,
            SelectHalf(counterAtSelection),
            SourcePosition,
            DestinationPosition,
            checked(deltaX * distance),
            checked(deltaY * distance));
    }

    private static (
        byte Facing,
        PrivateOriginalMapPlayerLocomotionSheet Sheet,
        int SourceSlot,
        bool HorizontalMirror) Selection(ExplorationDirection direction) => direction switch
        {
            ExplorationDirection.North =>
                (1, PrivateOriginalMapPlayerLocomotionSheet.Up, 0, false),
            ExplorationDirection.West =>
                (2, PrivateOriginalMapPlayerLocomotionSheet.Horizontal, 1, false),
            ExplorationDirection.East =>
                (0, PrivateOriginalMapPlayerLocomotionSheet.Horizontal, 1, true),
            ExplorationDirection.South =>
                (3, PrivateOriginalMapPlayerLocomotionSheet.Down, 2, false),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

    private static ExplorationDirection DirectionFromOpaqueFacing(
        byte opaqueFacing,
        string parameterName) => opaqueFacing switch
        {
            0 => ExplorationDirection.East,
            1 => ExplorationDirection.North,
            2 => ExplorationDirection.West,
            3 => ExplorationDirection.South,
            _ => throw new ArgumentOutOfRangeException(parameterName),
        };

    private static (int DeltaX, int DeltaY) DirectionDelta(
        ExplorationDirection direction) => direction switch
        {
            ExplorationDirection.North => (0, -1),
            ExplorationDirection.East => (1, 0),
            ExplorationDirection.South => (0, 1),
            ExplorationDirection.West => (-1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

    private static int SelectHalf(int counterAtSelection) =>
        counterAtSelection < HalfThreshold ? 0 : 1;

    private static int IncrementMovementCounter(int storedCounter) =>
        checked(storedCounter + 1);

    private static int IncrementSpriteCounter(int counterAtSelection) =>
        counterAtSelection + 1 > CounterResetAbove ? 0 : counterAtSelection + 1;
}

public sealed record PrivateOriginalMapPlayerLocomotionStarted(
    PrivateOriginalMapMoveApplied Move,
    PrivateOriginalMapPlayerLocomotionSnapshot Animation)
{
    public PrivateOriginalMapMoveApplied Move { get; } =
        Move ?? throw new ArgumentNullException(nameof(Move));

    public PrivateOriginalMapPlayerLocomotionSnapshot Animation { get; } =
        Animation ?? throw new ArgumentNullException(nameof(Animation));
}

public sealed partial class GameSession
{
    private PrivateOriginalMapPlayerLocomotionSnapshot? _privateOriginalMapPlayerLocomotion;

    public PrivateOriginalMapPlayerLocomotionSnapshot PrivateOriginalMapPlayerLocomotion =>
        _privateOriginalMapPlayerLocomotion ?? throw new InvalidOperationException(
            "This GameSession does not own private original-map player locomotion state.");

    public PrivateOriginalMapPlayerLocomotionStarted BeginPrivateOriginalMapPlayerLocomotion(
        MoveExplorationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        PrivateOriginalMapPlayerLocomotionSnapshot current =
            PrivateOriginalMapPlayerLocomotion;
        if (current.IsMoving)
        {
            throw new InvalidOperationException(
                "A private original-map player locomotion cycle is already active.");
        }

        MapPosition sourcePosition = PrivateOriginalMapSnapshot.PlayerPosition;
        PrivateOriginalMapMoveApplied move = ApplyPrivateOriginalMap(command);
        PrivateOriginalMapPlayerLocomotionSnapshot next = move.CrossMapTransition is not null
            ? PrivateOriginalMapPlayerLocomotionSnapshot.Relocate(
                current,
                move.CrossMapTransition)
            : move.SameMapWarp is not null
            ? PrivateOriginalMapPlayerLocomotionSnapshot.Relocate(
                current,
                move.SameMapWarp)
            : move.MessengerAcceptance is not null
                ? PrivateOriginalMapPlayerLocomotionSnapshot.CompleteMessengerAcceptance(
                    current,
                    move.MessengerAcceptance)
            : PrivateOriginalMapPlayerLocomotionSnapshot.Begin(
                current,
                command.Direction,
                sourcePosition,
                move.Traversal);
        _privateOriginalMapPlayerLocomotion = next;
        return new PrivateOriginalMapPlayerLocomotionStarted(move, next);
    }

    public PrivateOriginalMapPlayerLocomotionSnapshot AdvancePrivateOriginalMapPlayerLocomotion()
    {
        PrivateOriginalMapPlayerLocomotionSnapshot next =
            PrivateOriginalMapPlayerLocomotion.Advance();
        _privateOriginalMapPlayerLocomotion = next;
        return next;
    }

    private void InitializePrivateOriginalMapPlayerLocomotion()
    {
        _privateOriginalMapPlayerLocomotion =
            PrivateOriginalMapPlayerLocomotionSnapshot.ControlledAdmission(
                PrivateOriginalMapSnapshot.PlayerPosition);
    }
}
