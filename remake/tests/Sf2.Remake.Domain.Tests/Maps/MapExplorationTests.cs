using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Maps;

public sealed class MapExplorationTests
{
    [Fact]
    public void MoveReturnsNewStateAndLeavesInputUnchanged()
    {
        ExplorationMovementState input = State(
            width: 3,
            height: 2,
            passable: [true, true, true, true, true, true],
            x: 1,
            y: 0);

        ExplorationMovementResult result = ExplorationMovementReducer.TryMove(
            input,
            new ExplorationMovementCommand(ExplorationDirection.South));

        Assert.Equal(ExplorationMovementOutcome.Moved, result.Outcome);
        Assert.Equal(new MapPosition(1, 1), result.State.PlayerPosition);
        Assert.Equal(new MapPosition(1, 0), input.PlayerPosition);
        Assert.Same(input.Layout, result.State.Layout);
        Assert.Same(input.Walkability, result.State.Walkability);
    }

    [Fact]
    public void BoundaryBlocksWithoutChangingState()
    {
        ExplorationMovementState input = State(
            width: 2,
            height: 2,
            passable: [true, true, true, true],
            x: 0,
            y: 0);

        ExplorationMovementResult result = ExplorationMovementReducer.TryMove(
            input,
            new ExplorationMovementCommand(ExplorationDirection.West));

        Assert.Equal(ExplorationMovementOutcome.BlockedByBoundary, result.Outcome);
        Assert.Same(input, result.State);
    }

    [Fact]
    public void TerrainBlocksWithoutChangingState()
    {
        ExplorationMovementState input = State(
            width: 2,
            height: 2,
            passable: [true, false, true, true],
            x: 0,
            y: 0);

        ExplorationMovementResult result = ExplorationMovementReducer.TryMove(
            input,
            new ExplorationMovementCommand(ExplorationDirection.East));

        Assert.Equal(ExplorationMovementOutcome.BlockedByTerrain, result.Outcome);
        Assert.Same(input, result.State);
    }

    [Fact]
    public void StateRejectsStartOutsideTraversalOrOnBlockedCell()
    {
        SyntheticWalkabilityGrid walkability = new(2, 2, [true, false, true, true]);

        Assert.Throws<ArgumentException>(() => new ExplorationMovementState(
            new MapId("synthetic-map"),
            EmptyLayout(),
            walkability,
            new MapPosition(1, 0)));
        Assert.Throws<ArgumentException>(() => new ExplorationMovementState(
            new MapId("synthetic-map"),
            EmptyLayout(),
            walkability,
            new MapPosition(2, 0)));
    }

    [Fact]
    public void TraversalGridDefensivelyCopiesAndValidatesItsCellCount()
    {
        bool[] callerCells = [true, false, true, true];
        SyntheticWalkabilityGrid walkability = new(2, 2, callerCells);

        callerCells[0] = false;

        Assert.True(walkability.IsPassable(new MapPosition(0, 0)));
        Assert.Throws<ArgumentException>(() => new SyntheticWalkabilityGrid(2, 2, [true]));
        Assert.Throws<ArgumentException>(
            () => new SyntheticWalkabilityGrid(2, 2, [true, true, true, true, true]));
    }

    [Fact]
    public void IdenticalInputsProduceIdenticalResults()
    {
        ExplorationMovementState input = State(
            width: 2,
            height: 2,
            passable: [true, true, true, true],
            x: 0,
            y: 0);
        ExplorationMovementCommand command = new(ExplorationDirection.East);

        ExplorationMovementResult first = ExplorationMovementReducer.TryMove(input, command);
        ExplorationMovementResult second = ExplorationMovementReducer.TryMove(input, command);

        Assert.Equal(first, second);
    }

    private static ExplorationMovementState State(
        int width,
        int height,
        IEnumerable<bool> passable,
        int x,
        int y) =>
        new(
            new MapId("synthetic-map"),
            EmptyLayout(),
            new SyntheticWalkabilityGrid(width, height, passable),
            new MapPosition(x, y));

    private static WorkingMapLayout EmptyLayout() =>
        new(new ushort[WorkingMapLayout.WordCount]);
}
