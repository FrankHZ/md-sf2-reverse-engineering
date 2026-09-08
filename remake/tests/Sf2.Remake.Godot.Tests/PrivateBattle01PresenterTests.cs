using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class PrivateBattle01PresenterTests
{
    [Theory]
    [InlineData(GameFlowStage.Exploration, false, false, false)]
    [InlineData(GameFlowStage.Exploration, false, true, false)]
    [InlineData(GameFlowStage.Exploration, true, false, false)]
    [InlineData(GameFlowStage.Exploration, true, true, true)]
    [InlineData(GameFlowStage.Battle, false, false, true)]
    [InlineData(GameFlowStage.Battle, false, true, true)]
    public void BattleRouteClosesOldInputEvenAfterPendingIsConsumed(GameFlowStage stage,
        bool pending, bool selected, bool expected) =>
        Assert.Equal(expected, PrivateBattle01Ui.OwnsInput(stage, pending, selected));

    [Fact]
    public void ProjectionTracksLiveRelocationCancelAndActualCandidateRatherThanDeploymentOrActorOne()
    {
        var initial = AuthoredBattle();
        var round = Battle01FirstRound.Enter(initial);
        Assert.Equal(2, round.FirstRound!.FirstCandidate!.Value.CombatantIndex);
        var ready = Battle01FirstControl.Enter(round, 2).State!;
        var before = PrivateBattle01Presenter.BuildProjection(ready, "ready");
        Assert.Equal(2, before.ActorIndex);
        Assert.Equal(9, before.Units.Count);
        Assert.Equal(320, before.Tiles.Count);
        Assert.Equal(new MapPosition(2, 1), before.Cursor);
        Assert.True(before.Tiles.Single(tile => tile.Position == new MapPosition(2, 2)).CanStop);
        Assert.False(before.Tiles.Single(tile => tile.Position == new MapPosition(15, 19)).Reachable);
        var selected = Battle01PlayerMovement.SelectDestination(ready, 2, new(2, 2));
        var preview = PrivateBattle01Presenter.BuildProjection(selected, "selected");
        Assert.Equal(new MapPosition(2, 1), preview.Units.Single(unit => unit.Index == 2).Position);
        Assert.Equal(new MapPosition(2, 2), preview.Cursor);
        Assert.Equal(new MapPosition[] { new(2, 1), new(2, 2) }, preview.Path);
        Assert.Equal(2, preview.PathCost);
        Assert.Equal(2, preview.GridCost);
        Assert.Equal(10, preview.Budget);
        Assert.True(preview.CanConfirm);
        var confirmed = Battle01PlayerMovement.Confirm(selected, 2);
        var action = PrivateBattle01Presenter.BuildProjection(confirmed, "confirmed");
        Assert.Equal(Battle01Phase.PlayerActionChoice, action.Phase);
        Assert.Equal(new MapPosition(2, 2), action.Units.Single(unit => unit.Index == 2).Position);
        Assert.Equal(new MapPosition(2, 1), confirmed.Roster[2].Deployment.Position);
        Assert.False(action.CanConfirm);
        Assert.DoesNotContain("Space", action.Controls);
        Assert.Contains("Backspace", action.Controls);
        var cancelled = Battle01PlayerMovement.Cancel(confirmed, 2);
        var restored = PrivateBattle01Presenter.BuildProjection(cancelled, "cancelled");
        Assert.Equal(before.Units, restored.Units);
        Assert.Equal(before.Cursor, restored.Cursor);
        Assert.Equal(0, restored.PathCost);
        Assert.Equal(ready.Occupancy, cancelled.Occupancy);
        Assert.Same(round.FirstRound, cancelled.FirstRound);
        Assert.Equal(round.RandomSeedImage, cancelled.RandomSeedImage);
    }

    [Fact]
    public void EarlierFailurePhasesRemainBattleProjectionsWithoutMovementControls()
    {
        var initial = AuthoredBattle();
        foreach (var battle in new[] { initial, Battle01FirstRound.Enter(initial) })
        {
            var projection = PrivateBattle01Presenter.BuildProjection(battle, "First control unavailable");
            Assert.Equal(battle.Phase, projection.Phase);
            Assert.Null(projection.Cursor);
            Assert.Empty(projection.Path);
            Assert.False(projection.CanConfirm);
            Assert.DoesNotContain("Space", projection.Controls);
            Assert.Equal(9, projection.Units.Count);
            Assert.Contains("Current battle retained", projection.Controls);
        }
        Assert.Contains("DIAGNOSTIC", PrivateBattle01Presenter.Heading);
        Assert.Contains("Original Map 57 graphics unavailable", PrivateBattle01Presenter.Boundary);
    }

    // Public authored geometry and party, deliberately with actor 2 as the first candidate.
    private static Battle01InitializedState AuthoredBattle()
    {
        var rows = Enumerable.Range(0, 9).Select(index => new Battle01Deployment((byte)index,
            index < 3 ? index : 128 + index - 3, (byte)(index < 3 ? index : 39), new(index, 1),
            0, 127, 255, 0, 255, 15, 0, 0));
        var regions = Enumerable.Range(0, 3).Select(index => new Battle01Region((byte)index, 0,
            [new(0, 0), new(1, 0), new(1, 1), new(0, 1)], 0, 0));
        var allies = Enumerable.Range(0, 3).Select(index => new Battle01AllyInput((byte)index, 4,
            new(1, 12, 12, 8, 8, 9, 4, (byte)(index == 2 ? 50 : 4), 5, 0,
                [127, 127, 127, 127], [63, 63, 63, 63])));
        var enemy = new Battle01EnemyInput(39, 39, 0,
            new(0, 5, 5, 0, 0, 7, 5, 5, 5, 0, [127, 127, 127, 127], [63, 63, 63, 63]),
            0x40E3, 0, 6, 0x2000);
        return Battle01Initialization.Initialize(rows, regions, Enumerable.Repeat((byte)1, 2304),
            allies, enemy, 0x1234, 0);
    }
}
