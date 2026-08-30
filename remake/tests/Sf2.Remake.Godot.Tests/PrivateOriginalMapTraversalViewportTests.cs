using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class PrivateOriginalMapTraversalViewportTests
{
    [Fact]
    public void ProjectionUsesTheExactTwelveBySevenPlayerCenteredCrop()
    {
        PrivateOriginalMapTraversalViewProjection projection =
            PrivateOriginalMapTraversalViewProjection.Create(
                Snapshot(
                    new ushort[WorkingMapLayout.WordCount],
                    new MapPosition(30, 30),
                    new OriginalMapTraversalArea(0, 0, 63, 63)));

        Assert.Equal(24, projection.OriginX);
        Assert.Equal(27, projection.OriginY);
        Assert.Equal(6, projection.PlayerColumn);
        Assert.Equal(3, projection.PlayerRow);
        Assert.Equal(
            PrivateOriginalMapTraversalViewProjection.ColumnCount *
                PrivateOriginalMapTraversalViewProjection.RowCount,
            projection.Cells.Count);
        PrivateOriginalMapTraversalViewCell player = Assert.Single(
            projection.Cells,
            cell => cell.IsPlayer);
        Assert.Equal((30, 30, 6, 3),
            (player.MapX, player.MapY, player.Column, player.Row));
    }

    [Fact]
    public void ProjectionClampsAtBothWorkingLayoutEdges()
    {
        OriginalMapTraversalArea fullMap = new(0, 0, 63, 63);
        PrivateOriginalMapTraversalViewProjection upperLeft =
            PrivateOriginalMapTraversalViewProjection.Create(
                Snapshot(
                    new ushort[WorkingMapLayout.WordCount],
                    new MapPosition(0, 0),
                    fullMap));
        PrivateOriginalMapTraversalViewProjection lowerRight =
            PrivateOriginalMapTraversalViewProjection.Create(
                Snapshot(
                    new ushort[WorkingMapLayout.WordCount],
                    new MapPosition(63, 63),
                    fullMap));

        Assert.Equal((0, 0, 0, 0),
            (upperLeft.OriginX, upperLeft.OriginY,
                upperLeft.PlayerColumn, upperLeft.PlayerRow));
        Assert.Equal((52, 57, 11, 6),
            (lowerRight.OriginX, lowerRight.OriginY,
                lowerRight.PlayerColumn, lowerRight.PlayerRow));
    }

    [Fact]
    public void ProjectionUsesOnlyAcceptedDomainAreaAndCollisionPolicy()
    {
        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        words[Index(55, 3)] = OriginalMapTraversal.CollisionMask;
        words[Index(57, 3)] = OriginalMapTraversal.RightStairMask;
        PrivateOriginalMapTraversalViewProjection projection =
            PrivateOriginalMapTraversalViewProjection.Create(
                Snapshot(
                    words,
                    new MapPosition(56, 3),
                    new OriginalMapTraversalArea(54, 1, 58, 5)));

        Assert.Equal(59, Count(
            projection,
            PrivateOriginalMapTraversalCellCategory.OutsideAcceptedActiveArea));
        Assert.Equal(24, Count(
            projection,
            PrivateOriginalMapTraversalCellCategory.ActiveNonBlocked));
        Assert.Equal(1, Count(
            projection,
            PrivateOriginalMapTraversalCellCategory.BlockedByAcceptedCollisionClass));
        Assert.Equal(
            PrivateOriginalMapTraversalCellCategory.ActiveNonBlocked,
            Assert.Single(
                projection.Cells,
                cell => cell.MapX == 57 && cell.MapY == 3).Category);
    }

    [Fact]
    public void ProjectionOwnsAnImmutableCellCopyWithoutRawWordSurface()
    {
        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        PrivateOriginalMapTraversalViewProjection projection =
            PrivateOriginalMapTraversalViewProjection.Create(
                Snapshot(
                    words,
                    new MapPosition(56, 3),
                    new OriginalMapTraversalArea(0, 0, 63, 63)));
        words[Index(56, 3)] = OriginalMapTraversal.CollisionMask;

        IList<PrivateOriginalMapTraversalViewCell> cells =
            Assert.IsAssignableFrom<IList<PrivateOriginalMapTraversalViewCell>>(
                projection.Cells);
        Assert.True(cells.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => cells.Add(cells[0]));
        Assert.DoesNotContain(
            typeof(PrivateOriginalMapTraversalViewCell).GetProperties(),
            property => property.Name.Contains("Word", StringComparison.Ordinal));
        Assert.Equal(
            PrivateOriginalMapTraversalCellCategory.ActiveNonBlocked,
            Assert.Single(projection.Cells, cell => cell.IsPlayer).Category);
    }

    private static int Count(
        PrivateOriginalMapTraversalViewProjection projection,
        PrivateOriginalMapTraversalCellCategory category) =>
        projection.Cells.Count(cell => cell.Category == category);

    private static PrivateOriginalMapSessionSnapshot Snapshot(
        ushort[] words,
        MapPosition playerPosition,
        params OriginalMapTraversalArea[] activeAreas)
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        OriginalMapImportDefinition definition = new(
            map,
            new WorkingMapLayout(words),
            new OriginalMapTraversal(activeAreas),
            new OriginalMapControlledAdmission(
                map,
                playerPosition,
                OriginalMapRuntimeAdmission.OpaqueStartFacing,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SelectedInitIdentity,
                noProgramRequest: true),
            ["natural-route-and-presentation-unknown"]);
        OriginalMapImportReceipt receipt = new(
            OriginalMapRuntimeAdmission.PackageId,
            OriginalMapRuntimeAdmission.SchemaVersion,
            OriginalMapRuntimeAdmission.AcceptedContentDigest,
            OriginalMapRuntimeAdmission.AcceptedDecodedLayoutDigest,
            OriginalMapRuntimeAdmission.AcceptedCollisionProjectionDigest,
            ContentProfile.PrivateLocal,
            new OriginalMapImportProvenance(
                OriginalMapRuntimeAdmission.PackageId,
                OriginalMapRuntimeAdmission.AcceptedRomSha256,
                OriginalMapRuntimeAdmission.AcceptedUpstreamRepository,
                OriginalMapRuntimeAdmission.AcceptedUpstreamCommit),
            OriginalMapRuntimeAdmission.RequiredEvidenceOwners,
            OriginalMapRuntimeAdmission.RequiredCapabilities);
        return new PrivateOriginalMapSessionSnapshot(
            definition,
            receipt,
            simulationStep: 0,
            playerPosition,
            lastTraversal: null);
    }

    private static int Index(int x, int y) =>
        (y * WorkingMapLayout.ColumnCount) + x;
}
