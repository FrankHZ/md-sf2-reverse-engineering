using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class PrivateOriginalMapBaseViewportTests
{
    [Fact]
    public void ProjectAuthoredBaseCompositionRendersTheAcceptedCropAndOwnsItsPixels()
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(
            blockWords:
            [
                Enumerable.Repeat((ushort)0x0100, 9).ToArray(),
            ],
            layoutWords: new ushort[WorkingMapLayout.WordCount]);

        PrivateOriginalMapBaseViewProjection projection =
            PrivateOriginalMapBaseViewProjection.Create(snapshot, visual);

        Assert.Equal(new MapId(OriginalMapRuntimeAdmission.MapId), projection.Map);
        Assert.Equal(50, projection.OriginX);
        Assert.Equal(0, projection.OriginY);
        Assert.Equal(6, projection.PlayerColumn);
        Assert.Equal(3, projection.PlayerRow);
        Assert.Equal(
            PrivateOriginalMapBaseViewProjection.PixelWidth *
                PrivateOriginalMapBaseViewProjection.PixelHeight * 4,
            projection.RgbaBytes.Count);
        Assert.Equal(new byte[] { 255, 0, 0, 255 }, Pixel(projection, 0, 0));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<byte>)projection.RgbaBytes)[0] = 0);
    }

    [Fact]
    public void TileMirrorAndFlipAreAppliedInsideTheProjectAuthoredRecipe()
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        ushort[] block = new ushort[9];
        block[0] = 0x1902;
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(
            [block],
            new ushort[WorkingMapLayout.WordCount]);

        PrivateOriginalMapBaseViewProjection projection =
            PrivateOriginalMapBaseViewProjection.Create(snapshot, visual);

        Assert.Equal(new byte[] { 0x12, 0x18, 0x20, 255 }, Pixel(projection, 0, 0));
        Assert.Equal(new byte[] { 0, 0, 255, 255 }, Pixel(projection, 7, 7));
    }

    [Fact]
    public void CurrentWorkingLayoutSelectsTheReprojectedBlockAfterMutation()
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        ushort[][] blocks =
        [
            Enumerable.Repeat((ushort)0x0100, 9).ToArray(),
            Enumerable.Repeat((ushort)0x0101, 9).ToArray(),
        ];
        ushort[] beforeWords = new ushort[WorkingMapLayout.WordCount];
        ushort[] afterWords = [.. beforeWords];
        afterWords[(3 * WorkingMapLayout.ColumnCount) + 56] = 1;
        PrivateOriginalMapSessionSnapshot before = Snapshot(blocks, beforeWords);
        PrivateOriginalMapSessionSnapshot after = Snapshot(blocks, afterWords);

        PrivateOriginalMapBaseViewProjection beforeProjection =
            PrivateOriginalMapBaseViewProjection.Create(before, visual);
        PrivateOriginalMapBaseViewProjection afterProjection =
            PrivateOriginalMapBaseViewProjection.Create(after, visual);
        int playerPixelX = beforeProjection.PlayerColumn *
            PrivateOriginalMapBaseViewProjection.BlockPixelSize;
        int playerPixelY = beforeProjection.PlayerRow *
            PrivateOriginalMapBaseViewProjection.BlockPixelSize;

        Assert.Equal(
            new byte[] { 255, 0, 0, 255 },
            Pixel(beforeProjection, playerPixelX, playerPixelY));
        Assert.Equal(
            new byte[] { 0, 255, 0, 255 },
            Pixel(afterProjection, playerPixelX, playerPixelY));
    }

    [Fact]
    public void SnapshotAndPayloadSelectionMismatchFailsBeforeRendering()
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(
            blockWords:
            [
                Enumerable.Repeat((ushort)0x0100, 9).ToArray(),
            ],
            layoutWords: new ushort[WorkingMapLayout.WordCount],
            paletteIndex: 1);

        ArgumentException error = Assert.Throws<ArgumentException>(() =>
            PrivateOriginalMapBaseViewProjection.Create(snapshot, visual));

        Assert.Equal("visualDefinition", error.ParamName);
    }

    private static OriginalMapVisualPayloadDefinition VisualDefinition()
    {
        OriginalMapVisualResourceSelection selection = Selection(paletteIndex: 0);
        ushort[] palette = new ushort[OriginalMapVisualPayloadAdmission.PaletteWordCount];
        palette[1] = 0x000E;
        palette[2] = 0x00E0;
        palette[3] = 0x0E00;
        byte[][] decoded = Enumerable.Range(0, 5)
            .Select(_ => new byte[OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset])
            .ToArray();
        FillTile(decoded[0], localTile: 0, packedPixels: 0x11);
        FillTile(decoded[0], localTile: 1, packedPixels: 0x22);
        decoded[0][2 * 32] = 0x30;

        return new OriginalMapVisualPayloadDefinition(
            selection,
            new OriginalMapPalettePayload(resourceIndex: 0, palette),
            selection.TilesetSlots.Select((resourceIndex, index) =>
                new OriginalMapTilesetPayload(
                    index + 1,
                    resourceIndex,
                    decoded[index])));
    }

    private static void FillTile(byte[] decoded, int localTile, byte packedPixels)
    {
        Array.Fill(decoded, packedPixels, localTile * 32, 32);
    }

    private static PrivateOriginalMapSessionSnapshot Snapshot(
        IEnumerable<ushort[]> blockWords,
        ushort[] layoutWords,
        byte paletteIndex = 0)
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        WorkingMapLayout definitionLayout = new(new ushort[WorkingMapLayout.WordCount]);
        OriginalMapBlockCatalog blockCatalog = new(
            blockWords.Select((words, index) =>
                new OriginalMapBlockDefinition(
                    new OriginalMapBlockRecordIdentity(
                        "project-authored-base-view-blocks",
                        index),
                    words)));
        OriginalMapAreaCatalog areaCatalog = new(
        [
            new OriginalMapAreaDefinition(
                new OriginalMapAreaRecordIdentity(
                    "project-authored-base-view-areas",
                    oneBasedRecordOrdinal: 1),
                new OriginalMapTraversalArea(0, 0, 63, 63),
                new OriginalMapAreaWordPair(0, 0),
                new OriginalMapAreaWordPair(0, 0),
                new OriginalMapAreaWordPair(256, 256),
                new OriginalMapAreaWordPair(256, 256),
                new OriginalMapAreaBytePair(0, 0),
                new OriginalMapAreaBytePair(0, 0),
                mainLayerType: 0,
                defaultMusic: 0),
        ]);
        OriginalMapImportDefinition definition = new(
            map,
            definitionLayout,
            blockCatalog,
            areaCatalog,
            Selection(paletteIndex),
            new OriginalMapControlledAdmission(
                map,
                new MapPosition(56, 3),
                OriginalMapRuntimeAdmission.OpaqueStartFacing,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SelectedInitIdentity,
                noProgramRequest: true),
            controlledStepCopy: null,
            unsupportedCapabilities: ["project-authored-original-presentation-unknown"]);
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
            new WorkingMapLayout(layoutWords),
            simulationStep: 0,
            new MapPosition(56, 3),
            lastTraversal: null,
            controlledStepCopyApplied: false,
            lastLayoutMutation: null);
    }

    private static OriginalMapVisualResourceSelection Selection(byte paletteIndex) =>
        new(
            new MapId(OriginalMapRuntimeAdmission.MapId),
            paletteIndex,
            [0, 37, 43, 53, 66]);

    private static byte[] Pixel(
        PrivateOriginalMapBaseViewProjection projection,
        int x,
        int y)
    {
        int offset = ((y * PrivateOriginalMapBaseViewProjection.PixelWidth) + x) * 4;
        return projection.RgbaBytes.Skip(offset).Take(4).ToArray();
    }
}
