using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Maps;

public sealed class OriginalMapTraversalTests
{
    [Fact]
    public void CardinalMovementUsesActiveAreaAndCurrentCollisionWord()
    {
        ushort[] words = EmptyWords();
        words[Index(2, 1)] = OriginalMapTraversal.CollisionMask;
        OriginalMapTraversal traversal = Traversal(new OriginalMapTraversalArea(1, 1, 3, 3));
        WorkingMapLayout layout = new(words);

        OriginalMapTraversalResult blockedCollision = traversal.TryMove(
            layout,
            new MapPosition(1, 1),
            ExplorationDirection.East);
        OriginalMapTraversalResult blockedArea = traversal.TryMove(
            layout,
            new MapPosition(1, 1),
            ExplorationDirection.North);
        OriginalMapTraversalResult moved = traversal.TryMove(
            layout,
            new MapPosition(1, 1),
            ExplorationDirection.South);

        Assert.Equal(OriginalMapTraversalOutcome.BlockedByCollision, blockedCollision.Outcome);
        Assert.Equal(new MapPosition(1, 1), blockedCollision.Position);
        Assert.Equal(OriginalMapTraversal.CollisionMask, blockedCollision.DestinationWord);
        Assert.Equal(OriginalMapTraversalOutcome.BlockedOutsideActiveArea, blockedArea.Outcome);
        Assert.Equal((ushort)0, blockedArea.DestinationWord);
        Assert.Equal(OriginalMapTraversalOutcome.Moved, moved.Outcome);
        Assert.Equal(new MapPosition(1, 2), moved.Position);
    }

    [Fact]
    public void LayoutBoundaryIsDistinctFromActiveAreaBoundary()
    {
        OriginalMapTraversal traversal = Traversal(
            new OriginalMapTraversalArea(0, 0, 63, 63));

        OriginalMapTraversalResult result = traversal.TryMove(
            new WorkingMapLayout(EmptyWords()),
            new MapPosition(0, 0),
            ExplorationDirection.West);

        Assert.Equal(OriginalMapTraversalOutcome.BlockedByBoundary, result.Outcome);
        Assert.Null(result.DestinationWord);
        Assert.Equal(new MapPosition(0, 0), result.Position);
    }

    [Fact]
    public void ActiveAreaSelectionUsesFirstAdmittedRecordOrderOrReturnsNull()
    {
        OriginalMapTraversalArea first = new(0, 0, 10, 10);
        OriginalMapTraversalArea second = new(5, 5, 15, 15);
        OriginalMapTraversal traversal = Traversal(first, second);

        OriginalMapTraversalAreaSelection overlap =
            Assert.IsType<OriginalMapTraversalAreaSelection>(
                traversal.SelectActiveArea(new MapPosition(7, 7)));
        OriginalMapTraversalAreaSelection secondOnly =
            Assert.IsType<OriginalMapTraversalAreaSelection>(
                traversal.SelectActiveArea(new MapPosition(12, 12)));

        Assert.Equal(1, overlap.OneBasedRecordOrdinal);
        Assert.Same(first, overlap.Area);
        Assert.Equal(2, secondOnly.OneBasedRecordOrdinal);
        Assert.Same(second, secondOnly.Area);
        Assert.Null(traversal.SelectActiveArea(new MapPosition(20, 20)));
        Assert.False(traversal.IsWithinActiveArea(new MapPosition(20, 20)));
    }

    [Theory]
    [InlineData(0x8000, ExplorationDirection.East, 11, 9)]
    [InlineData(0x8000, ExplorationDirection.West, 9, 11)]
    [InlineData(0x4000, ExplorationDirection.East, 11, 11)]
    [InlineData(0x4000, ExplorationDirection.West, 9, 9)]
    public void HorizontalMovementUsesTheAcceptedDirectionalStairMapping(
        ushort stairClass,
        ExplorationDirection direction,
        int expectedX,
        int expectedY)
    {
        ushort[] words = EmptyWords();
        words[Index(10, 10)] = stairClass;
        words[Index(expectedX, expectedY)] = stairClass;
        words[Index(direction == ExplorationDirection.East ? 11 : 9, 10)] =
            OriginalMapTraversal.CollisionMask;
        OriginalMapTraversal traversal = Traversal(
            new OriginalMapTraversalArea(0, 0, 63, 63));

        OriginalMapTraversalResult result = traversal.TryMove(
            new WorkingMapLayout(words),
            new MapPosition(10, 10),
            direction);

        Assert.Equal(OriginalMapTraversalOutcome.Moved, result.Outcome);
        Assert.Equal(new MapPosition(expectedX, expectedY), result.Position);
        Assert.Equal(stairClass, result.SourceWord);
        Assert.Equal(stairClass, result.DestinationWord);
        Assert.Equal(new[] { -63, 63, 65, -65 }, OriginalMapTraversal.StairWordDeltas);
    }

    [Fact]
    public void StairProbeFallsBackToTheOrdinaryHorizontalCellWhenClassDoesNotMatch()
    {
        ushort[] words = EmptyWords();
        words[Index(10, 10)] = OriginalMapTraversal.RightStairMask;
        words[Index(11, 9)] = OriginalMapTraversal.LeftStairMask;
        OriginalMapTraversal traversal = Traversal(
            new OriginalMapTraversalArea(0, 0, 63, 63));

        OriginalMapTraversalResult result = traversal.TryMove(
            new WorkingMapLayout(words),
            new MapPosition(10, 10),
            ExplorationDirection.East);

        Assert.Equal(OriginalMapTraversalOutcome.Moved, result.Outcome);
        Assert.Equal(new MapPosition(11, 10), result.Position);
    }

    [Theory]
    [InlineData(ExplorationDirection.North, 10, 9)]
    [InlineData(ExplorationDirection.East, 11, 10)]
    [InlineData(ExplorationDirection.South, 10, 11)]
    [InlineData(ExplorationDirection.West, 9, 10)]
    public void CandidateTargetUsesTheSameCardinalPolicyAsTraversal(
        ExplorationDirection direction,
        int expectedX,
        int expectedY)
    {
        OriginalMapTraversal traversal = Traversal(
            new OriginalMapTraversalArea(0, 0, 63, 63));
        WorkingMapLayout layout = new(EmptyWords());

        MapPosition? target = traversal.ResolveCandidateTarget(
            layout,
            new MapPosition(10, 10),
            direction);

        Assert.Equal(new MapPosition(expectedX, expectedY), target);
    }

    [Fact]
    public void CandidateTargetPreservesStairAndBoundaryRulesWithoutApplyingCollision()
    {
        OriginalMapTraversal traversal = Traversal(
            new OriginalMapTraversalArea(0, 0, 63, 63));
        ushort[] stairWords = EmptyWords();
        stairWords[Index(10, 10)] = OriginalMapTraversal.RightStairMask;
        stairWords[Index(11, 9)] = OriginalMapTraversal.RightStairMask;
        WorkingMapLayout stairLayout = new(stairWords);

        Assert.Equal(
            new MapPosition(11, 9),
            traversal.ResolveCandidateTarget(
                stairLayout,
                new MapPosition(10, 10),
                ExplorationDirection.East));
        Assert.Null(traversal.ResolveCandidateTarget(
            stairLayout,
            new MapPosition(0, 0),
            ExplorationDirection.West));

        ushort[] blockedWords = EmptyWords();
        blockedWords[Index(11, 10)] = OriginalMapTraversal.CollisionMask;
        WorkingMapLayout blockedLayout = new(blockedWords);
        Assert.Equal(
            new MapPosition(11, 10),
            traversal.ResolveCandidateTarget(
                blockedLayout,
                new MapPosition(10, 10),
                ExplorationDirection.East));
        Assert.Equal(
            OriginalMapTraversalOutcome.BlockedByCollision,
            traversal.TryMove(
                blockedLayout,
                new MapPosition(10, 10),
                ExplorationDirection.East).Outcome);
    }

    [Fact]
    public void BlockCopyChangesLaterTraversalBecauseThePolicyRereadsTheCurrentLayout()
    {
        ushort[] words = EmptyWords();
        words[Index(2, 1)] = OriginalMapTraversal.CollisionMask;
        WorkingMapLayout initial = new(words);
        OriginalMapTraversal traversal = Traversal(
            new OriginalMapTraversalArea(0, 0, 63, 63));

        OriginalMapTraversalResult before = traversal.TryMove(
            initial,
            new MapPosition(1, 1),
            ExplorationDirection.East);
        WorkingMapLayout mutated = initial.ApplyBlockCopy(
            new WorkingMapBlockCopy(0, 0, 2, 1, 1, 1));
        OriginalMapTraversalResult after = traversal.TryMove(
            mutated,
            new MapPosition(1, 1),
            ExplorationDirection.East);

        Assert.Equal(OriginalMapTraversalOutcome.BlockedByCollision, before.Outcome);
        Assert.Equal(OriginalMapTraversalOutcome.Moved, after.Outcome);
        Assert.Equal(new MapPosition(2, 1), after.Position);
        Assert.Equal(OriginalMapTraversal.CollisionMask, initial[2, 1]);
        Assert.Equal((ushort)0, mutated[2, 1]);
    }

    [Fact]
    public void AreasAreDefensivelyCopiedAndInvalidSetsFailClosed()
    {
        List<OriginalMapTraversalArea> source =
            [new OriginalMapTraversalArea(1, 1, 2, 2)];
        OriginalMapTraversal traversal = new(source);
        source.Clear();

        Assert.Single(traversal.ActiveAreas);
        Assert.Throws<ArgumentException>(() => new OriginalMapTraversal([]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapTraversal(
                [
                    new OriginalMapTraversalArea(1, 1, 2, 2),
                    new OriginalMapTraversalArea(1, 1, 2, 2),
                ]));
        Assert.Throws<ArgumentException>(() => new OriginalMapTraversalArea(2, 1, 1, 2));
    }

    [Fact]
    public void TypedOccupiedEntityResultRetainsPositionWithoutChangingTraversalPolicy()
    {
        MapPosition source = new(42, 9);
        OriginalMapTraversalResult result = new(
            source,
            source,
            ExplorationDirection.North,
            OriginalMapTraversalOutcome.BlockedByOccupiedEntity,
            sourceWord: 1,
            destinationWord: 2);

        Assert.Equal(OriginalMapTraversalOutcome.BlockedByOccupiedEntity, result.Outcome);
        Assert.Same(source, result.Position);
        Assert.Equal((ushort)2, result.DestinationWord);
        Assert.Equal(
            OriginalMapTraversalOutcome.Moved,
            Traversal(new OriginalMapTraversalArea(0, 0, 63, 63))
                .TryMove(new WorkingMapLayout(EmptyWords()), source, ExplorationDirection.North)
                .Outcome);
    }

    private static OriginalMapTraversal Traversal(params OriginalMapTraversalArea[] areas) =>
        new(areas);

    private static ushort[] EmptyWords() => new ushort[WorkingMapLayout.WordCount];

    private static int Index(int x, int y) => (y * WorkingMapLayout.ColumnCount) + x;
}
