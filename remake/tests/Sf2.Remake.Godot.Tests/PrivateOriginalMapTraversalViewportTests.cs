using System.Security.Cryptography;
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
    public void ProjectionUsesTheCatalogOwnedCurrentMapRuntimeAfterCrossMapAdmission()
    {
        PrivateOriginalMapSessionSnapshot snapshot = CrossMapSnapshot();

        PrivateOriginalMapTraversalViewProjection projection =
            PrivateOriginalMapTraversalViewProjection.Create(snapshot);

        Assert.Equal(new MapId(OriginalMapRuntimeAdmission.Map19Id), projection.Map);
        Assert.True(Count(
            projection,
            PrivateOriginalMapTraversalCellCategory.OutsideAcceptedActiveArea) > 0);
        Assert.Equal(
            PrivateOriginalMapTraversalCellCategory.ActiveNonBlocked,
            Assert.Single(projection.Cells, cell => cell.IsPlayer).Category);
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

    [Fact]
    public void AstralMarkerFollowsApplicationOccupancyAndRetiresAfterAcceptance()
    {
        var ready = AstralSnapshot(completed: false);
        var visible = PrivateOriginalMapTraversalViewProjection.Create(ready);
        var actor = Assert.Single(visible.Cells, cell => cell.IsAstral);
        Assert.Equal((16, 5), (actor.MapX, actor.MapY));
        Assert.False(actor.IsPlayer);
        var completed = PrivateOriginalMapTraversalViewProjection.Create(AstralSnapshot(completed: true));
        Assert.DoesNotContain(completed.Cells, cell => cell.IsAstral);
        Assert.Single(completed.Cells, cell => cell.IsPlayer);
    }

    internal static PrivateOriginalMapSessionSnapshot AstralSnapshot(bool completed)
    {
        var basis = Snapshot(new ushort[WorkingMapLayout.WordCount], new MapPosition(16, 6), new OriginalMapTraversalArea(0, 0, 63, 63));
        MapId map = new("map19");
        var actors = Enumerable.Range(1, 13).Select(ordinal => new OriginalMapEntityDefinition(
            new("ms_map19_Entities", ordinal), ordinal == 13 ? (byte)16 : (byte)1,
            ordinal == 13 ? (byte)5 : (byte)1, 3, 209, [0, 4, 0x60, 0xCE])).ToArray();
        var population = new OriginalMapEntityPopulation(map, new("ms_map19"), actors);
        var runtime = new OriginalMapExplorationRuntimeDefinition(map, basis.WorkingLayout, basis.Definition.BlockCatalog,
            basis.Definition.AreaCatalog, population, new("ms_map19"), "ms_map19_InitFunction",
            basis.Definition.InitialRuntime.DecodedLayoutDigest, basis.Definition.InitialRuntime.CollisionProjectionDigest);
        var palace = new OriginalMapPalaceFirstVisitDefinition(OriginalMapRuntimeAdmission.PalaceInitBodySha256,
            OriginalMapRuntimeAdmission.PalaceScriptProjectionSha256);
        var astral = new OriginalMapAstralAcceptanceDefinition(population.Records[12]);
        var definition = new OriginalMapImportDefinition(basis.Definition.Map, basis.WorkingLayout,
            basis.Definition.BlockCatalog, basis.Definition.AreaCatalog, basis.EntityPopulation,
            basis.Definition.VisualResourceSelection, basis.Definition.ControlledAdmission,
            null, null, basis.Definition.UnsupportedCapabilities, runtimeCatalog: new([basis.Definition.InitialRuntime, runtime]),
            palaceFirstVisit: palace, astralAcceptance: astral);
        T Construct<T>(params object[] args) => (T)Activator.CreateInstance(typeof(T),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null, args: args, culture: null)!;
        var visit = Construct<PrivateOriginalMapPalaceFirstVisitReceipt>(palace, 1L);
        var state = completed ? Construct<PrivateOriginalMapAstralAcceptanceState>(astral, 2L) : null;
        return new(definition, basis.Receipt, runtime.WorkingLayout, 3, new(16, 6),
            runtime.Traversal.TryMove(runtime.WorkingLayout, new(16, 7), ExplorationDirection.North), false, null,
            currentRuntime: runtime, palaceFirstVisit: visit, astralAcceptance: state);
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

    private static PrivateOriginalMapSessionSnapshot CrossMapSnapshot()
    {
        MapId map3 = new(OriginalMapRuntimeAdmission.MapId);
        MapId map19 = new(OriginalMapRuntimeAdmission.Map19Id);
        OriginalMapExplorationRuntimeDefinition initial = Runtime(
            map3,
            new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
            OriginalMapRuntimeAdmission.SelectedInitIdentity,
            "project-authored-map3-blocks",
            new OriginalMapTraversalArea(0, 0, 63, 63),
            new MapPosition(56, 3));
        OriginalMapExplorationRuntimeDefinition destination = Runtime(
            map19,
            new MapSetupId(OriginalMapRuntimeAdmission.Map19SelectedSetupId),
            OriginalMapRuntimeAdmission.Map19SelectedInitIdentity,
            "project-authored-map19-blocks",
            new OriginalMapTraversalArea(24, 28, 28, 31),
            new MapPosition(26, 30));
        OriginalMapExplorationRuntimeCatalog catalog = new([initial, destination]);
        OriginalMapCrossMapTransitionDefinition transition = new(
            new OriginalMapCrossMapTransitionIdentity(
                ContentProfile.PrivateLocal,
                map3,
                OriginalMapRuntimeAdmission.SameMapWarpResourceId,
                OriginalMapRuntimeAdmission.NorthMap19WarpRecordOrdinal),
            OriginalMapRuntimeAdmission.NorthMap19WarpSourceTriggerX,
            OriginalMapRuntimeAdmission.NorthMap19WarpSourceTriggerY,
            new MapPosition(28, 2),
            ExplorationDirection.North,
            new MapPosition(28, 1),
            map19,
            new MapPosition(26, 30),
            OriginalMapRuntimeAdmission.NorthMap19WarpDestinationOpaqueFacing);
        OriginalMapImportDefinition definition = new(
            map3,
            initial.WorkingLayout,
            initial.BlockCatalog,
            initial.AreaCatalog,
            initial.EntityPopulation,
            new OriginalMapVisualResourceSelection(map3, 0, [0, 37, 43, 53, 66]),
            new OriginalMapControlledAdmission(
                map3,
                new MapPosition(56, 3),
                OriginalMapRuntimeAdmission.OpaqueStartFacing,
                initial.SelectedSetup,
                initial.SelectedInitIdentity,
                noProgramRequest: true),
            controlledStepCopy: null,
            sameMapWarps: null,
            unsupportedCapabilities: ["project-authored-unknown"],
            runtimeCatalog: catalog,
            northMap19Transition: transition);
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
            destination.WorkingLayout,
            simulationStep: 1,
            new MapPosition(26, 30),
            lastTraversal: new OriginalMapTraversalResult(
                new MapPosition(26, 30),
                new MapPosition(26, 30),
                ExplorationDirection.North,
                OriginalMapTraversalOutcome.BlockedByBoundary,
                sourceWord: 0,
                destinationWord: null),
            controlledStepCopyApplied: false,
            lastLayoutMutation: null,
            currentRuntime: destination);
    }

    private static OriginalMapExplorationRuntimeDefinition Runtime(
        MapId map,
        MapSetupId setup,
        string initIdentity,
        string blockResourceId,
        OriginalMapTraversalArea area,
        MapPosition entityPosition)
    {
        WorkingMapLayout layout = new(new ushort[WorkingMapLayout.WordCount]);
        OriginalMapBlockCatalog blocks = new(
        [
            new OriginalMapBlockDefinition(
                new OriginalMapBlockRecordIdentity(blockResourceId, 0),
                new ushort[OriginalMapBlockDefinition.OpaqueWordCount]),
        ]);
        OriginalMapAreaCatalog areas = new(
        [
            new OriginalMapAreaDefinition(
                new OriginalMapAreaRecordIdentity(blockResourceId + "-areas", 1),
                area,
                new OriginalMapAreaWordPair(0, 0),
                new OriginalMapAreaWordPair(0, 0),
                new OriginalMapAreaWordPair(256, 256),
                new OriginalMapAreaWordPair(256, 256),
                new OriginalMapAreaBytePair(0, 0),
                new OriginalMapAreaBytePair(0, 0),
                mainLayerType: 0,
                defaultMusic: 0),
        ]);
        OriginalMapEntityPopulation population = new(
            map,
            setup,
        [
            new OriginalMapEntityDefinition(
                new OriginalMapEntityRecordIdentity(blockResourceId + "-entities", 1),
                checked((byte)entityPosition.X),
                checked((byte)entityPosition.Y),
                opaqueFacing: 0,
                mapSprite: 0,
                [0, 0, 0, 0]),
        ]);
        return new OriginalMapExplorationRuntimeDefinition(
            map,
            layout,
            blocks,
            areas,
            population,
            setup,
            initIdentity,
            Convert.ToHexString(SHA256.HashData(new byte[WorkingMapLayout.WordCount * 2])),
            Convert.ToHexString(SHA256.HashData(new byte[WorkingMapLayout.WordCount])));
    }

    private static int Index(int x, int y) =>
        (y * WorkingMapLayout.ColumnCount) + x;
}
