using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class PrivateOriginalMapBaseViewportTests
{
    [Fact]
    public void ViewportEnforcesNearestSamplingWithoutTextureRepeat()
    {
        Assert.Equal(
            global::Godot.CanvasItem.TextureFilterEnum.Nearest,
            PrivateOriginalMapBaseViewport.RequiredTextureFilter);
        Assert.Equal(
            global::Godot.CanvasItem.TextureRepeatEnum.Disabled,
            PrivateOriginalMapBaseViewport.RequiredTextureRepeat);
        Assert.True(PrivateOriginalMapBaseViewport.IsRequiredTextureSampling(
            global::Godot.CanvasItem.TextureFilterEnum.Nearest,
            global::Godot.CanvasItem.TextureRepeatEnum.Disabled));
        Assert.False(PrivateOriginalMapBaseViewport.IsRequiredTextureSampling(
            global::Godot.CanvasItem.TextureFilterEnum.Linear,
            global::Godot.CanvasItem.TextureRepeatEnum.Disabled));
        Assert.False(PrivateOriginalMapBaseViewport.IsRequiredTextureSampling(
            global::Godot.CanvasItem.TextureFilterEnum.Nearest,
            global::Godot.CanvasItem.TextureRepeatEnum.Enabled));
    }

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

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void NearestAtlasProjectionIsPixelEquivalentAcrossSlotsFlipsAndMutation(int scale)
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        ushort[] firstBlock =
        [
            0x0100,
            0x0980,
            0x1200,
            0x0280,
            0x0300,
            0x0101,
            0x0180,
            0x0200,
            0x0280,
        ];
        ushort[] secondBlock = Enumerable.Repeat((ushort)0x0300, 9).ToArray();
        ushort[][] blocks = [firstBlock, secondBlock];
        ushort[] beforeWords = new ushort[WorkingMapLayout.WordCount];
        ushort[] afterWords = [.. beforeWords];
        afterWords[(3 * WorkingMapLayout.ColumnCount) + 56] = 1;
        PrivateOriginalMapSessionSnapshot before = Snapshot(blocks, beforeWords);
        PrivateOriginalMapSessionSnapshot after = Snapshot(blocks, afterWords);
        byte[] atlas = BuildNearestAtlas(visual, scale);

        PrivateOriginalMapBaseViewProjection payloadBefore =
            PrivateOriginalMapBaseViewProjection.Create(before, visual);
        PrivateOriginalMapBaseViewProjection atlasBefore =
            PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                before,
                visual.Selection,
                atlas,
                scale);
        PrivateOriginalMapBaseViewProjection payloadAfter =
            PrivateOriginalMapBaseViewProjection.Create(after, visual);
        PrivateOriginalMapBaseViewProjection atlasAfter =
            PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                after,
                visual.Selection,
                atlas,
                scale);

        Assert.Equal(payloadBefore.RgbaBytes, atlasBefore.RgbaBytes);
        Assert.Equal(payloadAfter.RgbaBytes, atlasAfter.RgbaBytes);
        Assert.NotEqual(payloadBefore.RgbaBytes, payloadAfter.RgbaBytes);
        atlas[0] ^= 0xFF;
        Assert.Equal(payloadBefore.RgbaBytes, atlasBefore.RgbaBytes);
    }

    [Fact]
    public void AtlasProjectionRejectsUnsupportedShapeScaleOrSelection()
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(
            [Enumerable.Repeat((ushort)0x0100, 9).ToArray()],
            new ushort[WorkingMapLayout.WordCount]);
        byte[] exact = BuildNearestAtlas(visual, scale: 2);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                snapshot,
                visual.Selection,
                exact,
                scale: 1));
        Assert.Throws<ArgumentException>(() =>
            PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                snapshot,
                visual.Selection,
                exact[..^1],
                scale: 2));
        Assert.Throws<ArgumentException>(() =>
            PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                snapshot,
                Selection(paletteIndex: 1),
                exact,
                scale: 2));
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
        FillTile(decoded[1], localTile: 0, packedPixels: 0x23);
        FillTile(decoded[2], localTile: 0, packedPixels: 0x31);
        FillTile(decoded[3], localTile: 0, packedPixels: 0x12);
        FillTile(decoded[4], localTile: 0, packedPixels: 0x33);

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

    private static byte[] BuildNearestAtlas(
        OriginalMapVisualPayloadDefinition visual,
        int scale)
    {
        int logicalWidth = PrivateLocalPresentationAssetCatalog.Map3BaseAtlasLogicalWidth;
        int logicalHeight = PrivateLocalPresentationAssetCatalog.Map3BaseAtlasLogicalHeight;
        int width = logicalWidth * scale;
        byte[] rgba = new byte[checked(width * logicalHeight * scale * 4)];
        for (int slot = 0; slot < visual.Tilesets.Count; slot++)
        {
            IReadOnlyList<byte> decoded = visual.Tilesets[slot].DecodedBytes;
            for (int localTile = 0; localTile < 128; localTile++)
            {
                for (int row = 0; row < 8; row++)
                {
                    for (int column = 0; column < 8; column++)
                    {
                        byte packed = decoded[(localTile * 32) + (row * 4) + (column / 2)];
                        int paletteIndex = column % 2 == 0
                            ? (packed >> 4) & 0x0F
                            : packed & 0x0F;
                        byte[] pixel = PalettePixel(visual, paletteIndex);
                        int logicalX = ((localTile % 16) * 8) + column;
                        int logicalY = (slot * 64) + ((localTile / 16) * 8) + row;
                        for (int y = 0; y < scale; y++)
                        {
                            for (int x = 0; x < scale; x++)
                            {
                                int offset = ((((logicalY * scale) + y) * width) +
                                    (logicalX * scale) + x) * 4;
                                pixel.CopyTo(rgba, offset);
                            }
                        }
                    }
                }
            }
        }

        return rgba;
    }

    private static byte[] PalettePixel(
        OriginalMapVisualPayloadDefinition visual,
        int paletteIndex)
    {
        if (paletteIndex == 0)
        {
            return [0, 0, 0, 0];
        }

        ushort word = visual.Palette.EffectiveWords[paletteIndex];
        return
        [
            ExpandChannel((word & 0x000E) >> 1),
            ExpandChannel((word & 0x00E0) >> 5),
            ExpandChannel((word & 0x0E00) >> 9),
            255,
        ];
    }

    private static byte ExpandChannel(int value) =>
        checked((byte)((value << 5) | (value << 2) | (value >> 1)));

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
