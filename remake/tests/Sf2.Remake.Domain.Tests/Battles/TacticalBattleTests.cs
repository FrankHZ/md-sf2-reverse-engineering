using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class TacticalBattleTests
{
    [Fact]
    public void PlayerCanMoveSelectTargetCancelAndCompleteDeterministically()
    {
        TacticalBattleState initial = Rules().CreateInitialState();

        TacticalCursorMoveResult moved = TacticalBattleReducer.MoveCursor(
            initial,
            TacticalDirection.East);
        TacticalSelectionResult moveConfirmed = TacticalBattleReducer.Confirm(moved.State);
        TacticalCursorMoveResult targeted = TacticalBattleReducer.MoveCursor(
            moveConfirmed.State,
            TacticalDirection.East);
        TacticalCancelResult cancelled = TacticalBattleReducer.Cancel(targeted.State);

        Assert.Equal(TacticalCursorMoveOutcome.Moved, moved.Outcome);
        Assert.Equal(TacticalSelectionOutcome.MoveConfirmed, moveConfirmed.Outcome);
        Assert.Equal(TacticalCancelOutcome.ReturnedToMoveSelection, cancelled.Outcome);
        Assert.Equal(initial.ActorPosition, cancelled.State.ActorPosition);
        Assert.Equal(initial.TurnOrigin, cancelled.State.CursorPosition);

        TacticalBattleState replayedTarget = TacticalBattleReducer.MoveCursor(
            TacticalBattleReducer.Confirm(
                TacticalBattleReducer.MoveCursor(
                    cancelled.State,
                    TacticalDirection.East).State).State,
            TacticalDirection.East).State;
        TacticalSelectionResult completed = TacticalBattleReducer.Confirm(replayedTarget);

        Assert.Equal(TacticalSelectionOutcome.BattleCompleted, completed.Outcome);
        Assert.Equal(TacticalBattlePhase.Completed, completed.State.Phase);
        Assert.Equal(0, completed.State.EnemyHitPoints);
        Assert.Equal(new TacticalPosition(1, 1), completed.State.ActorPosition);
        Assert.Equal(new TacticalPosition(2, 1), completed.State.EnemyPosition);
        Assert.Equal(initial.Rules.Battle, completed.State.Rules.Battle);
    }

    [Fact]
    public void IllegalCursorMovementAndSelectionAreReferenceStable()
    {
        TacticalBattleState initial = Rules().CreateInitialState();

        TacticalCursorMoveResult boundary = TacticalBattleReducer.MoveCursor(
            initial,
            TacticalDirection.West);
        TacticalSelectionResult moveConfirmed = TacticalBattleReducer.Confirm(initial);
        TacticalBattleState north = TacticalBattleReducer.MoveCursor(
            moveConfirmed.State,
            TacticalDirection.North).State;
        TacticalCursorMoveResult range = TacticalBattleReducer.MoveCursor(
            north,
            TacticalDirection.East);
        TacticalSelectionResult invalidTarget = TacticalBattleReducer.Confirm(
            moveConfirmed.State);

        Assert.Equal(TacticalCursorMoveOutcome.BlockedByBoundary, boundary.Outcome);
        Assert.Same(initial, boundary.State);
        Assert.Equal(TacticalCursorMoveOutcome.BlockedByRange, range.Outcome);
        Assert.Same(north, range.State);
        Assert.Equal(TacticalSelectionOutcome.InvalidSelection, invalidTarget.Outcome);
        Assert.Same(moveConfirmed.State, invalidTarget.State);
    }

    [Fact]
    public void EnemyOccupancyBlocksMoveSelection()
    {
        TacticalBattleRules rules = new(
            new TacticalBattleId("public-synthetic-test-battle"),
            new TacticalBattleGrid(2, 1, [true, true]),
            new TacticalCombatantId("public-synthetic-test-actor"),
            new TacticalPosition(0, 0),
            new TacticalCombatantId("public-synthetic-test-enemy"),
            new TacticalPosition(1, 0),
            actorMoveRange: 1,
            actorAttackRange: 1,
            enemyMaxHitPoints: 1,
            actorDamage: 1);

        TacticalBattleState state = rules.CreateInitialState();
        TacticalCursorMoveResult result = TacticalBattleReducer.MoveCursor(
            state,
            TacticalDirection.East);

        Assert.Equal(TacticalCursorMoveOutcome.BlockedByOccupant, result.Outcome);
        Assert.Same(state, result.State);
    }

    [Fact]
    public void CompletedStateRejectsFurtherMutationByReference()
    {
        TacticalBattleState state = Rules().CreateInitialState();
        state = TacticalBattleReducer.MoveCursor(state, TacticalDirection.East).State;
        state = TacticalBattleReducer.Confirm(state).State;
        state = TacticalBattleReducer.MoveCursor(state, TacticalDirection.East).State;
        state = TacticalBattleReducer.Confirm(state).State;

        TacticalCursorMoveResult moved = TacticalBattleReducer.MoveCursor(
            state,
            TacticalDirection.West);
        TacticalSelectionResult confirmed = TacticalBattleReducer.Confirm(state);
        TacticalCancelResult cancelled = TacticalBattleReducer.Cancel(state);

        Assert.Equal(TacticalCursorMoveOutcome.BattleCompleted, moved.Outcome);
        Assert.Equal(TacticalSelectionOutcome.BattleAlreadyCompleted, confirmed.Outcome);
        Assert.Equal(TacticalCancelOutcome.NotAvailable, cancelled.Outcome);
        Assert.Same(state, moved.State);
        Assert.Same(state, confirmed.State);
        Assert.Same(state, cancelled.State);
    }

    [Fact]
    public void GridAndRulesRejectMalformedOrAliasedInputs()
    {
        bool[] cells = [true, true, true, true, true, true];
        TacticalBattleGrid grid = new(3, 2, cells);
        cells[0] = false;

        Assert.True(grid.IsPassable(new TacticalPosition(0, 0)));
        Assert.Throws<ArgumentException>(() => new TacticalBattleGrid(3, 2, [true]));
        Assert.Throws<ArgumentException>(() => new TacticalBattleGrid(
            3,
            2,
            [true, true, true, true, true, true, true]));
        Assert.Throws<ArgumentException>(() => new TacticalBattleRules(
            new TacticalBattleId("public-synthetic-test-battle"),
            grid,
            new TacticalCombatantId("same"),
            new TacticalPosition(0, 0),
            new TacticalCombatantId("same"),
            new TacticalPosition(2, 1),
            1,
            1,
            1,
            1));
    }

    private static TacticalBattleRules Rules() =>
        new(
            new TacticalBattleId("public-synthetic-test-battle"),
            new TacticalBattleGrid(3, 2, [true, true, true, true, true, true]),
            new TacticalCombatantId("public-synthetic-test-actor"),
            new TacticalPosition(0, 1),
            new TacticalCombatantId("public-synthetic-test-enemy"),
            new TacticalPosition(2, 1),
            actorMoveRange: 1,
            actorAttackRange: 1,
            enemyMaxHitPoints: 1,
            actorDamage: 1);
}
