using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Battles;

public sealed record TacticalBattleId
{
    public TacticalBattleId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record TacticalCombatantId
{
    public TacticalCombatantId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record TacticalPosition
{
    public TacticalPosition(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }
}

public enum TacticalDirection
{
    North,
    East,
    South,
    West,
}

public sealed class TacticalBattleGrid
{
    public const int MaximumDimension = 16;

    private readonly bool[] _passableCells;
    private readonly ReadOnlyCollection<bool> _readOnlyPassableCells;

    public TacticalBattleGrid(int width, int height, IEnumerable<bool> passableCells)
    {
        if (width < 1 || width > MaximumDimension)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height < 1 || height > MaximumDimension)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        ArgumentNullException.ThrowIfNull(passableCells);
        int expectedCount = checked(width * height);
        bool[] copied = [.. passableCells.Take(expectedCount + 1)];
        if (copied.Length != expectedCount)
        {
            throw new ArgumentException(
                $"A tactical grid must contain exactly {expectedCount} cells.",
                nameof(passableCells));
        }

        Width = width;
        Height = height;
        _passableCells = copied;
        _readOnlyPassableCells = Array.AsReadOnly(_passableCells);
    }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<bool> PassableCells => _readOnlyPassableCells;

    public bool Contains(TacticalPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        return position.X < Width && position.Y < Height;
    }

    public bool IsPassable(TacticalPosition position)
    {
        ArgumentNullException.ThrowIfNull(position);
        return Contains(position) && _passableCells[(position.Y * Width) + position.X];
    }
}

public sealed record TacticalBattleRules
{
    public TacticalBattleRules(
        TacticalBattleId battle,
        TacticalBattleGrid grid,
        TacticalCombatantId actor,
        TacticalPosition actorStart,
        TacticalCombatantId enemy,
        TacticalPosition enemyStart,
        int actorMoveRange,
        int actorAttackRange,
        int enemyMaxHitPoints,
        int actorDamage)
    {
        Battle = battle ?? throw new ArgumentNullException(nameof(battle));
        Grid = grid ?? throw new ArgumentNullException(nameof(grid));
        Actor = actor ?? throw new ArgumentNullException(nameof(actor));
        ActorStart = actorStart ?? throw new ArgumentNullException(nameof(actorStart));
        Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
        EnemyStart = enemyStart ?? throw new ArgumentNullException(nameof(enemyStart));
        ArgumentOutOfRangeException.ThrowIfLessThan(actorMoveRange, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(actorAttackRange, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(enemyMaxHitPoints, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(actorDamage, 1);
        if (actor == enemy)
        {
            throw new ArgumentException(
                "Tactical actor and enemy identities must remain distinct.",
                nameof(enemy));
        }

        if (!grid.IsPassable(actorStart) ||
            !grid.IsPassable(enemyStart) ||
            actorStart == enemyStart)
        {
            throw new ArgumentException(
                "Tactical combatants require distinct passable start cells.",
                nameof(actorStart));
        }

        ActorMoveRange = actorMoveRange;
        ActorAttackRange = actorAttackRange;
        EnemyMaxHitPoints = enemyMaxHitPoints;
        ActorDamage = actorDamage;
    }

    public TacticalBattleId Battle { get; }

    public TacticalBattleGrid Grid { get; }

    public TacticalCombatantId Actor { get; }

    public TacticalPosition ActorStart { get; }

    public TacticalCombatantId Enemy { get; }

    public TacticalPosition EnemyStart { get; }

    public int ActorMoveRange { get; }

    public int ActorAttackRange { get; }

    public int EnemyMaxHitPoints { get; }

    public int ActorDamage { get; }

    public TacticalBattleState CreateInitialState() => new(
        this,
        TacticalBattlePhase.MoveSelection,
        ActorStart,
        EnemyStart,
        ActorStart,
        ActorStart,
        EnemyMaxHitPoints);
}

public enum TacticalBattlePhase
{
    MoveSelection,
    TargetSelection,
    Completed,
}

public sealed record TacticalBattleState
{
    internal TacticalBattleState(
        TacticalBattleRules rules,
        TacticalBattlePhase phase,
        TacticalPosition actorPosition,
        TacticalPosition enemyPosition,
        TacticalPosition cursorPosition,
        TacticalPosition turnOrigin,
        int enemyHitPoints)
    {
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        if (!Enum.IsDefined(phase))
        {
            throw new ArgumentOutOfRangeException(nameof(phase));
        }

        ActorPosition = actorPosition ?? throw new ArgumentNullException(nameof(actorPosition));
        EnemyPosition = enemyPosition ?? throw new ArgumentNullException(nameof(enemyPosition));
        CursorPosition = cursorPosition ?? throw new ArgumentNullException(nameof(cursorPosition));
        TurnOrigin = turnOrigin ?? throw new ArgumentNullException(nameof(turnOrigin));
        if (!rules.Grid.IsPassable(actorPosition) ||
            !rules.Grid.IsPassable(enemyPosition) ||
            !rules.Grid.IsPassable(cursorPosition) ||
            !rules.Grid.IsPassable(turnOrigin) ||
            actorPosition == enemyPosition ||
            enemyHitPoints < 0 ||
            enemyHitPoints > rules.EnemyMaxHitPoints ||
            (phase == TacticalBattlePhase.Completed) != (enemyHitPoints == 0))
        {
            throw new ArgumentException("Tactical battle state is inconsistent with its rules.");
        }

        Phase = phase;
        EnemyHitPoints = enemyHitPoints;
    }

    public TacticalBattleRules Rules { get; }

    public TacticalBattlePhase Phase { get; }

    public TacticalPosition ActorPosition { get; }

    public TacticalPosition EnemyPosition { get; }

    public TacticalPosition CursorPosition { get; }

    public TacticalPosition TurnOrigin { get; }

    public int EnemyHitPoints { get; }
}

public enum TacticalCursorMoveOutcome
{
    Moved,
    BlockedByBoundary,
    BlockedByRange,
    BlockedByOccupant,
    BattleCompleted,
}

public sealed record TacticalCursorMoveResult(
    TacticalBattleState State,
    TacticalCursorMoveOutcome Outcome)
{
    public TacticalBattleState State { get; } =
        State ?? throw new ArgumentNullException(nameof(State));
}

public enum TacticalSelectionOutcome
{
    MoveConfirmed,
    AttackConfirmed,
    BattleCompleted,
    InvalidSelection,
    BattleAlreadyCompleted,
}

public sealed record TacticalSelectionResult(
    TacticalBattleState State,
    TacticalSelectionOutcome Outcome)
{
    public TacticalBattleState State { get; } =
        State ?? throw new ArgumentNullException(nameof(State));
}

public enum TacticalCancelOutcome
{
    ReturnedToMoveSelection,
    NotAvailable,
}

public sealed record TacticalCancelResult(
    TacticalBattleState State,
    TacticalCancelOutcome Outcome)
{
    public TacticalBattleState State { get; } =
        State ?? throw new ArgumentNullException(nameof(State));
}

public static class TacticalBattleReducer
{
    public static TacticalCursorMoveResult MoveCursor(
        TacticalBattleState state,
        TacticalDirection direction)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        if (state.Phase == TacticalBattlePhase.Completed)
        {
            return new TacticalCursorMoveResult(
                state,
                TacticalCursorMoveOutcome.BattleCompleted);
        }

        (int deltaX, int deltaY) = direction switch
        {
            TacticalDirection.North => (0, -1),
            TacticalDirection.East => (1, 0),
            TacticalDirection.South => (0, 1),
            TacticalDirection.West => (-1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
        int x = state.CursorPosition.X + deltaX;
        int y = state.CursorPosition.Y + deltaY;
        if (x < 0 || y < 0)
        {
            return new TacticalCursorMoveResult(
                state,
                TacticalCursorMoveOutcome.BlockedByBoundary);
        }

        TacticalPosition destination = new(x, y);
        if (!state.Rules.Grid.IsPassable(destination))
        {
            return new TacticalCursorMoveResult(
                state,
                TacticalCursorMoveOutcome.BlockedByBoundary);
        }

        TacticalPosition origin = state.Phase == TacticalBattlePhase.MoveSelection
            ? state.TurnOrigin
            : state.ActorPosition;
        int range = state.Phase == TacticalBattlePhase.MoveSelection
            ? state.Rules.ActorMoveRange
            : state.Rules.ActorAttackRange;
        if (Manhattan(origin, destination) > range)
        {
            return new TacticalCursorMoveResult(
                state,
                TacticalCursorMoveOutcome.BlockedByRange);
        }

        if (state.Phase == TacticalBattlePhase.MoveSelection &&
            destination == state.EnemyPosition)
        {
            return new TacticalCursorMoveResult(
                state,
                TacticalCursorMoveOutcome.BlockedByOccupant);
        }

        return new TacticalCursorMoveResult(
            new TacticalBattleState(
                state.Rules,
                state.Phase,
                state.ActorPosition,
                state.EnemyPosition,
                destination,
                state.TurnOrigin,
                state.EnemyHitPoints),
            TacticalCursorMoveOutcome.Moved);
    }

    public static TacticalSelectionResult Confirm(TacticalBattleState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.Phase == TacticalBattlePhase.Completed)
        {
            return new TacticalSelectionResult(
                state,
                TacticalSelectionOutcome.BattleAlreadyCompleted);
        }

        if (state.Phase == TacticalBattlePhase.MoveSelection)
        {
            if (state.CursorPosition == state.EnemyPosition)
            {
                return new TacticalSelectionResult(
                    state,
                    TacticalSelectionOutcome.InvalidSelection);
            }

            return new TacticalSelectionResult(
                new TacticalBattleState(
                    state.Rules,
                    TacticalBattlePhase.TargetSelection,
                    state.CursorPosition,
                    state.EnemyPosition,
                    state.CursorPosition,
                    state.TurnOrigin,
                    state.EnemyHitPoints),
                TacticalSelectionOutcome.MoveConfirmed);
        }

        if (state.CursorPosition != state.EnemyPosition ||
            Manhattan(state.ActorPosition, state.EnemyPosition) > state.Rules.ActorAttackRange)
        {
            return new TacticalSelectionResult(
                state,
                TacticalSelectionOutcome.InvalidSelection);
        }

        int remainingHitPoints = Math.Max(
            state.EnemyHitPoints - state.Rules.ActorDamage,
            0);
        TacticalBattlePhase phase = remainingHitPoints == 0
            ? TacticalBattlePhase.Completed
            : TacticalBattlePhase.TargetSelection;
        return new TacticalSelectionResult(
            new TacticalBattleState(
                state.Rules,
                phase,
                state.ActorPosition,
                state.EnemyPosition,
                state.CursorPosition,
                state.TurnOrigin,
                remainingHitPoints),
            remainingHitPoints == 0
                ? TacticalSelectionOutcome.BattleCompleted
                : TacticalSelectionOutcome.AttackConfirmed);
    }

    public static TacticalCancelResult Cancel(TacticalBattleState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.Phase != TacticalBattlePhase.TargetSelection)
        {
            return new TacticalCancelResult(state, TacticalCancelOutcome.NotAvailable);
        }

        return new TacticalCancelResult(
            new TacticalBattleState(
                state.Rules,
                TacticalBattlePhase.MoveSelection,
                state.TurnOrigin,
                state.EnemyPosition,
                state.TurnOrigin,
                state.TurnOrigin,
                state.EnemyHitPoints),
            TacticalCancelOutcome.ReturnedToMoveSelection);
    }

    private static int Manhattan(TacticalPosition left, TacticalPosition right) =>
        Math.Abs(left.X - right.X) + Math.Abs(left.Y - right.Y);
}
