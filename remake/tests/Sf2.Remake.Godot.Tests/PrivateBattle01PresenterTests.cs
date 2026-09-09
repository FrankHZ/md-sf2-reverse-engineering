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
        Assert.Contains("Space: STAY", action.Controls);
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

    [Fact]
    public void BaseArtAndLiveOverlaysShareTheUnscaledTwentyFourPixelGrid()
    {
        Assert.Equal(24, PrivateBattle01Presenter.TileSize);
        Assert.Equal(PrivateOriginalMapBaseViewProjection.BlockPixelSize, PrivateBattle01Presenter.TileSize);
        var origin = PrivateBattle01Presenter.Cell(new(0, 0));
        Assert.Equal(384, PrivateBattle01Presenter.Cell(new(16, 0)).X - origin.X);
        Assert.Equal(480, PrivateBattle01Presenter.Cell(new(0, 20)).Y - origin.Y);
        Assert.True(origin.Y + 480 <= 540);
        Assert.Contains("DIAGNOSTIC UNITS", PrivateBattle01Presenter.BaseArtHeading);
        var ready = Battle01FirstControl.Enter(Battle01FirstRound.Enter(AuthoredBattle()), 2).State!;
        var selected = PrivateBattle01Presenter.BuildProjection(Battle01PlayerMovement.SelectDestination(ready, 2, new(2, 2)), "");
        Assert.Equal(24, PrivateBattle01Presenter.Cell(selected.Cursor!).Y -
            PrivateBattle01Presenter.Cell(selected.Units.Single(unit => unit.Index == 2).Position).Y);
        Assert.Equal(PrivateBattle01Presenter.Cell(selected.Cursor!), PrivateBattle01Presenter.Cell(selected.Path[^1]));
    }

    [Fact]
    public void CompletedStayShowsHistoricalActorAndUndispatchedCandidateWithoutOldInteraction()
    {
        var ready = Battle01FirstControl.Enter(Battle01FirstRound.Enter(AuthoredBattle()), 2).State!;
        var action = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 2, new(2, 2)), 2);
        var completed = Battle01TurnCompletion.CommitStay(action, 2, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        var view = PrivateBattle01Presenter.BuildProjection(completed, "STAY complete");
        Assert.Null(view.ActorIndex); Assert.Equal(2, view.CompletedActorIndex);
        Assert.Equal((int?)action.FirstRound!.Slots[1].CombatantIndex, view.NextCandidateIndex);
        Assert.Equal(new MapPosition(2, 2), view.Units.Single(unit => unit.Index == 2).Position);
        Assert.Null(view.Cursor); Assert.Empty(view.Path); Assert.Null(view.GridCost); Assert.Null(view.PathCost); Assert.Null(view.Budget);
        Assert.All(view.Tiles, tile => { Assert.False(tile.Reachable); Assert.False(tile.CanStop); });
        Assert.False(view.CanConfirm); Assert.DoesNotContain("Space", view.Controls); Assert.Contains("Candidate not started", view.Controls);
    }

    [Fact]
    public void PreviousCompletionDoesNotHideTheNextPlayersIndependentMovementProjection()
    {
        var ready = Battle01FirstControl.Enter(Battle01FirstRound.Enter(AuthoredBattle(nextPlayer: true)), 2).State!;
        var action = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 2, new(2, 2)), 2);
        var completed = Battle01TurnCompletion.CommitStay(action, 2, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        Assert.Equal((byte)1, completed.FirstRound!.CurrentCandidate!.Value.CombatantIndex);
        var next = Battle01NextPlayerControl.Enter(completed, 1).State!;
        var view = PrivateBattle01Presenter.BuildProjection(next, "next player ready");
        Assert.Equal(1, view.ActorIndex); Assert.Null(view.CompletedActorIndex); Assert.Null(view.NextCandidateIndex);
        Assert.Equal(Battle01Phase.PlayerMovementSelection, view.Phase);
        Assert.Equal(new MapPosition(1, 1), view.Cursor); Assert.Equal(14, view.Budget);
        Assert.Equal(new MapPosition(2, 2), view.Units.Single(unit => unit.Index == 2).Position);
        Assert.Same(completed.TurnCompletion, next.TurnCompletion);
        Assert.Contains("Space", view.Controls); Assert.Contains(view.Tiles, tile => tile.CanStop);
    }

    [Fact]
    public void EnemyStandbyProjectionShowsItsLiveMoveAndStopsBeforeTheNextEnemy()
    {
        MapPosition[] positions = [new(8, 18), new(9, 18), new(7, 18), new(7, 3), new(9, 4), new(6, 4), new(8, 3), new(9, 5), new(6, 5)];
        var rows = Enumerable.Range(0, 9).Select(i => new Battle01Deployment((byte)i, i < 3 ? i : 125 + i,
            (byte)(i < 3 ? i : 39), positions[i], (byte)(i >= 7 ? 7 : i >= 3 ? 6 : 0), 127, 255,
            (byte)(i < 6 ? 2 : i < 8 ? 1 : 0), 255, 15, (byte)(i >= 7 ? 112 : i >= 3 ? 96 : 0), 0));
        var regions = Enumerable.Range(0, 3).Select(i => new Battle01Region((byte)i, 0, [new(0, 0), new(1, 0), new(1, 1), new(0, 1)], 0, 0));
        byte[] agility = [4, 5, 7];
        var party = Enumerable.Range(0, 3).Select(i => new Battle01AllyInput((byte)i, (byte)(i == 1 ? 4 : 1),
            new(1, 12, 12, 8, 8, 9, 4, agility[i], (byte)(i == 2 ? 7 : 5), 0, [127, 127, 127, 127], [63, 63, 63, 63])));
        var enemy = new Battle01EnemyInput(39, 39, 0, new(0, 5, 5, 0, 0, 7, 5, 5, 5, 0,
            [127, 127, 127, 127], [63, 63, 63, 63]), 0x40E3, 0, 6, 0x2000);
        var terrain = Enumerable.Repeat((byte)1, 2304).ToArray(); terrain[4 * 48 + 7] = 255;
        var initial = Battle01Initialization.Initialize(rows, regions, terrain, party, enemy, 0x1234, 0, 0x1234);
        var ready = Battle01FirstControl.Enter(Battle01FirstRound.Enter(initial), 1).State!;
        var first = Stay(ready, 1, new(9, 17)); var next = Battle01NextPlayerControl.Enter(first, 2).State!;
        var second = Stay(next, 2, new(7, 17));
        var completed = Battle01EnemyStandby.CompleteFirst(second, 128, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        var view = PrivateBattle01Presenter.BuildProjection(completed, "Enemy standby: E0 moved; STAY complete.");
        Assert.Equal(Battle01Phase.EnemyTurnCompleted, view.Phase); Assert.Null(view.ActorIndex);
        Assert.Equal(128, view.CompletedActorIndex); Assert.Equal(131, view.NextCandidateIndex);
        Assert.Equal(new MapPosition(6, 3), view.Units.Single(unit => unit.Index == 128).Position);
        Assert.Equal(new MapPosition(8, 3), view.Units.Single(unit => unit.Index == 131).Position);
        Assert.Null(view.Cursor); Assert.Empty(view.Path); Assert.False(view.CanConfirm);
        Assert.All(view.Tiles, tile => { Assert.False(tile.Reachable); Assert.False(tile.CanStop); });
        Assert.Contains("Input closed", view.Controls); Assert.Contains("Candidate not started", view.Controls); Assert.Contains("Enemy standby", view.Status);
        static Battle01InitializedState Stay(Battle01InitializedState state, int actor, MapPosition target) =>
            Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(state, actor, target), actor),
                actor, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
    }

    // Public authored geometry and party, deliberately with actor 2 as the first candidate.
    private static Battle01InitializedState AuthoredBattle(bool nextPlayer = false)
    {
        var rows = Enumerable.Range(0, 9).Select(index => new Battle01Deployment((byte)index,
            index < 3 ? index : 128 + index - 3, (byte)(index < 3 ? index : 39), new(index, 1),
            0, 127, 255, 0, 255, 15, 0, 0));
        var regions = Enumerable.Range(0, 3).Select(index => new Battle01Region((byte)index, 0,
            [new(0, 0), new(1, 0), new(1, 1), new(0, 1)], 0, 0));
        var allies = Enumerable.Range(0, 3).Select(index => new Battle01AllyInput((byte)index, (byte)(nextPlayer && index == 1 ? 1 : 4),
            new(1, 12, 12, 8, 8, 9, 4, (byte)(index == 2 ? 50 : nextPlayer && index == 1 ? 30 : 4),
                (byte)(nextPlayer && index == 1 ? 7 : 5), 0,
                [127, 127, 127, 127], [63, 63, 63, 63])));
        var enemy = new Battle01EnemyInput(39, 39, 0,
            new(0, 5, 5, 0, 0, 7, 5, 5, 5, 0, [127, 127, 127, 127], [63, 63, 63, 63]),
            0x40E3, 0, 6, 0x2000);
        return Battle01Initialization.Initialize(rows, regions, Enumerable.Repeat((byte)1, 2304),
            allies, enemy, 0x1234, 0);
    }
}
