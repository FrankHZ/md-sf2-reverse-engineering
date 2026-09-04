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

    [Fact]
    public void ProjectAuthoredNearDoorSnapshotReprojectsTheAuthoritativeMutatedLayout()
    {
        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        words[Index(41, 13)] = OriginalMapTraversal.CollisionMask;
        PrivateOriginalMapSessionSnapshot before = Snapshot(
            words,
            new MapPosition(41, 12),
            new OriginalMapTraversalArea(0, 0, 63, 63));
        OriginalMapStepCopyDefinition stepCopy = before.Definition.ControlledStepCopy!;
        WorkingMapLayout mutated = before.WorkingLayout.ApplyBlockCopy(stepCopy.Copy);
        PrivateOriginalMapLayoutMutationReceipt receipt = new(
            stepCopy.Identity,
            stepCopy.Trigger,
            stepCopy.Copy,
            PrivateOriginalMapCollisionCategory.BlockedByAcceptedCollisionClass,
            PrivateOriginalMapCollisionCategory.ActiveNonBlocked,
            simulationStep: 1);
        PrivateOriginalMapSessionSnapshot after = new(
            before.Definition,
            before.Receipt,
            mutated,
            simulationStep: 1,
            before.PlayerPosition,
            lastTraversal: null,
            controlledStepCopyApplied: true,
            receipt);

        PrivateOriginalMapTraversalViewProjection beforeProjection =
            PrivateOriginalMapTraversalViewProjection.Create(before);
        PrivateOriginalMapTraversalViewProjection afterProjection =
            PrivateOriginalMapTraversalViewProjection.Create(after);
        Assert.Equal(
            PrivateOriginalMapTraversalCellCategory.BlockedByAcceptedCollisionClass,
            Assert.Single(
                beforeProjection.Cells,
                cell => cell.MapX == 41 && cell.MapY == 13).Category);
        Assert.Equal(
            PrivateOriginalMapTraversalCellCategory.ActiveNonBlocked,
            Assert.Single(
                afterProjection.Cells,
                cell => cell.MapX == 41 && cell.MapY == 13).Category);
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
        WorkingMapLayout layout = new(words);
        OriginalMapAreaCatalog areaCatalog = new(activeAreas.Select(
            (area, index) => new OriginalMapAreaDefinition(
                new OriginalMapAreaRecordIdentity(
                    "project-authored-viewport-area-table",
                    index + 1),
                area,
                new OriginalMapAreaWordPair(0, 0),
                new OriginalMapAreaWordPair(0, 0),
                new OriginalMapAreaWordPair(256, 256),
                new OriginalMapAreaWordPair(256, 256),
                new OriginalMapAreaBytePair(0, 0),
                new OriginalMapAreaBytePair(0, 0),
                mainLayerType: 0,
                defaultMusic: 0)));
        OriginalMapImportDefinition definition = new(
            map,
            layout,
            new OriginalMapBlockCatalog(
            [
                new OriginalMapBlockDefinition(
                    new OriginalMapBlockRecordIdentity(
                        "project-authored-viewport-blocks",
                        zeroBasedBlockIndex: 0),
                    new ushort[OriginalMapBlockDefinition.OpaqueWordCount]),
            ]),
            areaCatalog,
            ProjectAuthoredEntityPopulation(map),
            new OriginalMapVisualResourceSelection(
                map,
                paletteIndex: 0,
                [0, 37, 43, 53, 66]),
            new OriginalMapControlledAdmission(
                map,
                playerPosition,
                OriginalMapRuntimeAdmission.OpaqueStartFacing,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SelectedInitIdentity,
                noProgramRequest: true),
            new OriginalMapStepCopyDefinition(
                new OriginalMapStepCopyIdentity(
                    ContentProfile.PrivateLocal,
                    map,
                    OriginalMapRuntimeAdmission.ControlledStepCopyResourceId,
                    OriginalMapRuntimeAdmission.ControlledStepCopyRecordOrdinal),
                new MapPosition(
                    OriginalMapRuntimeAdmission.ControlledStepCopyTriggerX,
                    OriginalMapRuntimeAdmission.ControlledStepCopyTriggerY),
                new WorkingMapBlockCopy(
                    OriginalMapRuntimeAdmission.ControlledStepCopySourceX,
                    OriginalMapRuntimeAdmission.ControlledStepCopySourceY,
                    OriginalMapRuntimeAdmission.ControlledStepCopyDestinationX,
                    OriginalMapRuntimeAdmission.ControlledStepCopyDestinationY,
                    OriginalMapRuntimeAdmission.ControlledStepCopyWidth,
                    OriginalMapRuntimeAdmission.ControlledStepCopyHeight)),
            ["natural-route-and-presentation-unknown"]);
        Assert.Same(areaCatalog, definition.AreaCatalog);
        Assert.Same(areaCatalog.Traversal, definition.Traversal);
        Assert.Equal(activeAreas.Length, definition.AreaCatalog.Records.Count);
        Assert.Equal(
            "project-authored-viewport-blocks",
            definition.BlockCatalog.ResourceId);
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
            layout,
            simulationStep: 0,
            playerPosition,
            lastTraversal: null,
            controlledStepCopyApplied: false,
            lastLayoutMutation: null);
    }

    private static OriginalMapEntityPopulation ProjectAuthoredEntityPopulation(MapId map) =>
        new(
            map,
            new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
            [
                new OriginalMapEntityDefinition(
                    new OriginalMapEntityRecordIdentity(
                        "project-authored-viewport-entities",
                        1),
                    rawX: 1,
                    rawY: 1,
                    opaqueFacing: 0,
                    mapSprite: 0,
                    [0, 0, 0, 0]),
            ]);

    private static int Index(int x, int y) =>
        (y * WorkingMapLayout.ColumnCount) + x;
}
