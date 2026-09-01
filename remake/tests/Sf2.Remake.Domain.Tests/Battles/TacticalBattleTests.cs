using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class TacticalBattleTests
{
    [Fact]
    public void RangedRepositioningCanDefeatTheProjectAuthoredEnemy()
    {
        TacticalBattleState state = Rules().CreateInitialState();

        TacticalSelectionResult firstAttack = Attack(
            state,
            move: [],
            target: [TacticalDirection.East, TacticalDirection.East]);
        Assert.Equal(TacticalSelectionOutcome.AttackConfirmed, firstAttack.Outcome);
        Assert.Equal(TacticalEnemyResponseKind.Moved, firstAttack.EnemyResponse!.Kind);
        Assert.Equal(new TacticalPosition(1, 1), firstAttack.State.EnemyPosition);
        Assert.Equal(2, firstAttack.State.ActorHitPoints);
        Assert.Equal(2, firstAttack.State.EnemyHitPoints);

        TacticalSelectionResult secondAttack = Attack(
            firstAttack.State,
            move: [TacticalDirection.North],
            target: [TacticalDirection.East, TacticalDirection.South]);
        Assert.Equal(TacticalEnemyResponseKind.Moved, secondAttack.EnemyResponse!.Kind);
        Assert.Equal(
            new TacticalPosition(1, 0),
            secondAttack.State.EnemyPosition);
        Assert.Equal(2, secondAttack.State.ActorHitPoints);
        Assert.Equal(1, secondAttack.State.EnemyHitPoints);

        TacticalSelectionResult completed = Attack(
            secondAttack.State,
            move: [TacticalDirection.South],
            target: [TacticalDirection.East, TacticalDirection.North]);
        Assert.Equal(TacticalSelectionOutcome.BattleCompleted, completed.Outcome);
        Assert.Null(completed.EnemyResponse);
        Assert.Equal(TacticalBattlePhase.Completed, completed.State.Phase);
        Assert.Equal(TacticalBattleOutcome.Victory, completed.State.Outcome);
        Assert.Equal(0, completed.State.EnemyHitPoints);
        Assert.Equal(2, completed.State.ActorHitPoints);
    }

    [Fact]
    public void AdjacentExchangesReachTypedDefeatWithoutMovingTheEnemy()
    {
        TacticalBattleState state = Rules().CreateInitialState();

        TacticalSelectionResult firstAttack = Attack(
            state,
            move: [TacticalDirection.East],
            target: [TacticalDirection.East]);
        Assert.Equal(TacticalSelectionOutcome.AttackConfirmed, firstAttack.Outcome);
        Assert.Equal(TacticalEnemyResponseKind.Attacked, firstAttack.EnemyResponse!.Kind);
        Assert.Equal(firstAttack.EnemyResponse.EnemyPositionBefore,
            firstAttack.EnemyResponse.EnemyPositionAfter);
        Assert.Equal(1, firstAttack.State.ActorHitPoints);
        Assert.Equal(2, firstAttack.State.EnemyHitPoints);

        TacticalSelectionResult defeated = Attack(
            firstAttack.State,
            move: [],
            target: [TacticalDirection.East]);
        Assert.Equal(TacticalSelectionOutcome.BattleDefeated, defeated.Outcome);
        Assert.Equal(TacticalEnemyResponseKind.ActorDefeated, defeated.EnemyResponse!.Kind);
        Assert.Equal(TacticalBattlePhase.Completed, defeated.State.Phase);
        Assert.Equal(TacticalBattleOutcome.Defeat, defeated.State.Outcome);
        Assert.Equal(0, defeated.State.ActorHitPoints);
        Assert.Equal(1, defeated.State.EnemyHitPoints);
    }

    [Fact]
    public void NorthEastSouthWestTieOrderAndBlockedResponseAreDeterministic()
    {
        TacticalSelectionResult first = Attack(
            Rules().CreateInitialState(),
            move: [],
            target: [TacticalDirection.East, TacticalDirection.East]);
        TacticalSelectionResult tied = Attack(
            first.State,
            move: [TacticalDirection.North],
            target: [TacticalDirection.East, TacticalDirection.South]);

        Assert.Equal(new TacticalPosition(1, 0), tied.State.EnemyPosition);

        TacticalBattleRules blockedRules = new(
            new TacticalBattleId("project-authored-blocked-response"),
            new TacticalBattleGrid(3, 2, [true, true, true, true, false, true]),
            new TacticalCombatantId("project-authored-actor"),
            new TacticalPosition(0, 1),
            new TacticalCombatantId("project-authored-enemy"),
            new TacticalPosition(2, 1),
            actorMoveRange: 1,
            actorAttackRange: 4,
            actorMaxHitPoints: 2,
            actorDamage: 1,
            enemyMoveRange: 1,
            enemyAttackRange: 1,
            enemyMaxHitPoints: 2,
            enemyDamage: 1);
        TacticalSelectionResult blocked = Attack(
            blockedRules.CreateInitialState(),
            move: [],
            target:
            [
                TacticalDirection.North,
                TacticalDirection.East,
                TacticalDirection.East,
                TacticalDirection.South,
            ]);

        Assert.Equal(TacticalEnemyResponseKind.Blocked, blocked.EnemyResponse!.Kind);
        Assert.Equal(blocked.EnemyResponse.EnemyPositionBefore,
            blocked.EnemyResponse.EnemyPositionAfter);
    }

    [Fact]
    public void IllegalCursorMovementCancelAndCompletedMutationAreReferenceStable()
    {
        TacticalBattleState initial = Rules().CreateInitialState();
        TacticalCursorMoveResult boundary = TacticalBattleReducer.MoveCursor(
            initial,
            TacticalDirection.West);
        TacticalBattleState targetSelection = TacticalBattleReducer.Confirm(initial).State;
        TacticalCursorMoveResult range = TacticalBattleReducer.MoveCursor(
            TacticalBattleReducer.MoveCursor(
                TacticalBattleReducer.MoveCursor(
                    targetSelection,
                    TacticalDirection.East).State,
                TacticalDirection.East).State,
            TacticalDirection.North);
        TacticalCancelResult cancelled = TacticalBattleReducer.Cancel(targetSelection);
        TacticalSelectionResult defeated = Attack(
            Attack(
                initial,
                move: [TacticalDirection.East],
                target: [TacticalDirection.East]).State,
            move: [],
            target: [TacticalDirection.East]);

        Assert.Equal(TacticalCursorMoveOutcome.BlockedByBoundary, boundary.Outcome);
        Assert.Same(initial, boundary.State);
        Assert.Equal(TacticalCursorMoveOutcome.BlockedByRange, range.Outcome);
        Assert.Equal(TacticalCancelOutcome.ReturnedToMoveSelection, cancelled.Outcome);
        Assert.Equal(initial.ActorPosition, cancelled.State.ActorPosition);
        Assert.Equal(initial.TurnOrigin, cancelled.State.CursorPosition);

        TacticalCursorMoveResult moved = TacticalBattleReducer.MoveCursor(
            defeated.State,
            TacticalDirection.West);
        TacticalSelectionResult confirmed = TacticalBattleReducer.Confirm(defeated.State);
        TacticalCancelResult completedCancel = TacticalBattleReducer.Cancel(defeated.State);
        Assert.Equal(TacticalCursorMoveOutcome.BattleCompleted, moved.Outcome);
        Assert.Equal(TacticalSelectionOutcome.BattleAlreadyCompleted, confirmed.Outcome);
        Assert.Equal(TacticalCancelOutcome.NotAvailable, completedCancel.Outcome);
        Assert.Same(defeated.State, moved.State);
        Assert.Same(defeated.State, confirmed.State);
        Assert.Same(defeated.State, completedCancel.State);
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
            new TacticalBattleId("project-authored-test-battle"),
            grid,
            new TacticalCombatantId("same"),
            new TacticalPosition(0, 0),
            new TacticalCombatantId("same"),
            new TacticalPosition(2, 1),
            1,
            2,
            2,
            1,
            1,
            1,
            3,
            1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TacticalBattleRules(
            new TacticalBattleId("project-authored-test-battle"),
            grid,
            new TacticalCombatantId("actor"),
            new TacticalPosition(0, 0),
            new TacticalCombatantId("enemy"),
            new TacticalPosition(2, 1),
            1,
            2,
            2,
            1,
            enemyMoveRange: 2,
            1,
            3,
            1));
        Assert.Throws<ArgumentException>(() => new TacticalEnemyResponse(
            TacticalEnemyResponseKind.Moved,
            new TacticalPosition(2, 1),
            new TacticalPosition(2, 1),
            actorHitPointsBefore: 2,
            actorHitPointsAfter: 2));
    }

    private static TacticalSelectionResult Attack(
        TacticalBattleState state,
        IEnumerable<TacticalDirection> move,
        IEnumerable<TacticalDirection> target)
    {
        foreach (TacticalDirection direction in move)
        {
            state = AssertMoved(state, direction);
        }

        TacticalSelectionResult moveConfirmed = TacticalBattleReducer.Confirm(state);
        Assert.Equal(TacticalSelectionOutcome.MoveConfirmed, moveConfirmed.Outcome);
        state = moveConfirmed.State;
        foreach (TacticalDirection direction in target)
        {
            state = AssertMoved(state, direction);
        }

        return TacticalBattleReducer.Confirm(state);
    }

    private static TacticalBattleState AssertMoved(
        TacticalBattleState state,
        TacticalDirection direction)
    {
        TacticalCursorMoveResult moved = TacticalBattleReducer.MoveCursor(state, direction);
        Assert.Equal(TacticalCursorMoveOutcome.Moved, moved.Outcome);
        return moved.State;
    }

    private static TacticalBattleRules Rules() =>
        new(
            new TacticalBattleId("project-authored-test-battle"),
            new TacticalBattleGrid(3, 2, [true, true, true, true, true, true]),
            new TacticalCombatantId("project-authored-test-actor"),
            new TacticalPosition(0, 1),
            new TacticalCombatantId("project-authored-test-enemy"),
            new TacticalPosition(2, 1),
            actorMoveRange: 1,
            actorAttackRange: 2,
            actorMaxHitPoints: 2,
            actorDamage: 1,
            enemyMoveRange: 1,
            enemyAttackRange: 1,
            enemyMaxHitPoints: 3,
            enemyDamage: 1);
}
