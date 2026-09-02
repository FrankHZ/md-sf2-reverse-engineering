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
        Assert.Equal(
            new global::Godot.Rect2(
                global::Godot.Vector2.Zero,
                new global::Godot.Vector2(
                    PrivateOriginalMapBaseViewProjection.PixelWidth,
                    PrivateOriginalMapBaseViewProjection.PixelHeight)),
            PrivateOriginalMapBaseViewport.LogicalTextureRect);
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
        Assert.Equal(1, projection.RasterScale);
        Assert.Equal(288, projection.RasterPixelWidth);
        Assert.Equal(168, projection.RasterPixelHeight);
        Assert.Equal(
            projection.RasterPixelWidth * projection.RasterPixelHeight * 4,
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

        PrivateOriginalMapBaseViewProjection edgeBefore =
            PrivateOriginalMapBaseViewProjection.CreateEdgeScale2x(
                beforeProjection,
                outputScale: 2);
        PrivateOriginalMapBaseViewProjection edgeAfter =
            PrivateOriginalMapBaseViewProjection.CreateEdgeScale2x(
                afterProjection,
                outputScale: 2);
        Assert.Equal(
            new byte[] { 255, 0, 0, 255 },
            Pixel(edgeBefore, (playerPixelX * 2) + 1, (playerPixelY * 2) + 1));
        Assert.Equal(
            new byte[] { 0, 255, 0, 255 },
            Pixel(edgeAfter, (playerPixelX * 2) + 1, (playerPixelY * 2) + 1));
        Assert.NotEqual(edgeBefore.RgbaBytes, edgeAfter.RgbaBytes);
    }

    [Fact]
    public void StaticOverlayDiagnosticComposesTheUniqueAreaAndRetainsTransparentHoles()
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        ushort[] overlayBlock = new ushort[9];
        overlayBlock[0] = 0x0101;
        overlayBlock[1] = 0x0103;
        ushort[][] blocks =
        [
            Enumerable.Repeat((ushort)0x0100, 9).ToArray(),
            overlayBlock,
            new ushort[9],
        ];
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(
            blocks,
            StaticOverlayLayout(firstOverlayBlock: 1),
            areaDefinitions: StaticOverlayAreas());

        PrivateOriginalMapBaseViewProjection projection =
            PrivateOriginalMapBaseViewProjection.Create(
                snapshot,
                visual,
                staticOverlayDiagnostic: true);

        Assert.True(projection.StaticOverlayDiagnostic);
        Assert.False(projection.ShowsPlayerMarker);
        Assert.Equal(1, projection.OverlayAreaRecordOrdinal);
        Assert.Equal(0, projection.OverlayDeltaX);
        Assert.Equal(32, projection.OverlayDeltaY);
        Assert.Equal(0, projection.OriginX);
        Assert.Equal(0, projection.OriginY);
        Assert.Equal(new byte[] { 0, 255, 0, 255 }, Pixel(projection, 0, 0));
        Assert.Equal(new byte[] { 0, 0, 0, 255 }, Pixel(projection, 8, 0));
        Assert.Equal(new byte[] { 255, 0, 0, 255 }, Pixel(projection, 16, 0));
    }

    [Fact]
    public void StaticOverlayDiagnosticIgnoresTilePriorityForPixelsAndReadsLatestLayout()
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        ushort[] withoutPriority = new ushort[9];
        withoutPriority[0] = 0x0101;
        ushort[] withPriority = [.. withoutPriority];
        withPriority[0] = 0x8101;
        ushort[][] blocks =
        [
            Enumerable.Repeat((ushort)0x0100, 9).ToArray(),
            withoutPriority,
            withPriority,
            new ushort[9],
        ];
        PrivateOriginalMapBaseViewProjection plain =
            PrivateOriginalMapBaseViewProjection.Create(
                Snapshot(
                    blocks,
                    StaticOverlayLayout(firstOverlayBlock: 1, transparentOverlayBlock: 3),
                    areaDefinitions: StaticOverlayAreas()),
                visual,
                staticOverlayDiagnostic: true);
        PrivateOriginalMapBaseViewProjection priority =
            PrivateOriginalMapBaseViewProjection.Create(
                Snapshot(
                    blocks,
                    StaticOverlayLayout(firstOverlayBlock: 2, transparentOverlayBlock: 3),
                    areaDefinitions: StaticOverlayAreas()),
                visual,
                staticOverlayDiagnostic: true);
        PrivateOriginalMapBaseViewProjection cleared =
            PrivateOriginalMapBaseViewProjection.Create(
                Snapshot(
                    blocks,
                    StaticOverlayLayout(firstOverlayBlock: 3, transparentOverlayBlock: 3),
                    areaDefinitions: StaticOverlayAreas()),
                visual,
                staticOverlayDiagnostic: true);

        Assert.Equal(plain.RgbaBytes, priority.RgbaBytes);
        Assert.Equal(new byte[] { 0, 255, 0, 255 }, Pixel(priority, 0, 0));
        Assert.Equal(new byte[] { 255, 0, 0, 255 }, Pixel(cleared, 0, 0));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void StaticOverlayDiagnosticRetainsAtlasPixelsAndTreatsTheComposedCrop(int scale)
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        ushort[] overlayBlock = new ushort[9];
        overlayBlock[0] = 0x0101;
        ushort[][] blocks =
        [
            Enumerable.Repeat((ushort)0x0100, 9).ToArray(),
            overlayBlock,
            new ushort[9],
        ];
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(
            blocks,
            StaticOverlayLayout(firstOverlayBlock: 1),
            areaDefinitions: StaticOverlayAreas());
        PrivateOriginalMapBaseViewProjection logical =
            PrivateOriginalMapBaseViewProjection.Create(
                snapshot,
                visual,
                staticOverlayDiagnostic: true);
        PrivateOriginalMapBaseViewProjection physical =
            PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                snapshot,
                visual.Selection,
                BuildNearestAtlas(visual, scale),
                scale,
                staticOverlayDiagnostic: true);
        PrivateOriginalMapBaseViewProjection treated =
            PrivateOriginalMapBaseViewProjection.CreateEdgeScale2x(logical, scale);
        PrivateOriginalMapBaseViewProjection mainOnly =
            PrivateOriginalMapBaseViewProjection.Create(snapshot, visual);

        Assert.True(PrivateOriginalMapBaseViewProjection.IsExactNearestReplication(
            logical,
            physical));
        Assert.True(treated.StaticOverlayDiagnostic);
        Assert.Equal(logical.OverlayAreaRecordOrdinal, treated.OverlayAreaRecordOrdinal);
        Assert.NotEqual(mainOnly.RgbaBytes, logical.RgbaBytes);
        Assert.NotEqual(
            PrivateOriginalMapBaseViewProjection.CreateEdgeScale2x(mainOnly, scale).RgbaBytes,
            treated.RgbaBytes);
    }

    [Fact]
    public void StaticOverlayDiagnosticFailsClosedOnAmbiguousOrOutOfBoundsAreas()
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        ushort[][] blocks =
        [
            Enumerable.Repeat((ushort)0x0100, 9).ToArray(),
        ];
        OriginalMapAreaDefinition[] ambiguous =
        [
            .. StaticOverlayAreas(),
            Area(
                ordinal: 3,
                bounds: new OriginalMapTraversalArea(51, 10, 61, 19),
                foreground: new OriginalMapAreaWordPair(1, 0)),
        ];
        OriginalMapAreaDefinition[] outOfBounds =
        [
            Area(
                ordinal: 1,
                bounds: new OriginalMapTraversalArea(0, 57, 11, 63),
                foreground: new OriginalMapAreaWordPair(0, 1)),
            Area(
                ordinal: 2,
                bounds: new OriginalMapTraversalArea(51, 0, 61, 9),
                foreground: new OriginalMapAreaWordPair(0, 0)),
        ];

        Assert.Equal(
            "snapshot",
            Assert.Throws<ArgumentException>(() =>
                PrivateOriginalMapBaseViewProjection.Create(
                    Snapshot(blocks, new ushort[WorkingMapLayout.WordCount],
                        areaDefinitions: ambiguous),
                    visual,
                    staticOverlayDiagnostic: true)).ParamName);
        Assert.Equal(
            "snapshot",
            Assert.Throws<ArgumentException>(() =>
                PrivateOriginalMapBaseViewProjection.Create(
                    Snapshot(blocks, new ushort[WorkingMapLayout.WordCount],
                        areaDefinitions: outOfBounds),
                    visual,
                    staticOverlayDiagnostic: true)).ParamName);
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

        Assert.Equal(scale, atlasBefore.RasterScale);
        Assert.Equal(PrivateOriginalMapBaseViewProjection.PixelWidth * scale,
            atlasBefore.RasterPixelWidth);
        Assert.Equal(PrivateOriginalMapBaseViewProjection.PixelHeight * scale,
            atlasBefore.RasterPixelHeight);
        Assert.Equal(
            atlasBefore.RasterPixelWidth * atlasBefore.RasterPixelHeight * 4,
            atlasBefore.RgbaBytes.Count);
        Assert.True(PrivateOriginalMapBaseViewProjection.IsExactNearestReplication(
            payloadBefore,
            atlasBefore));
        Assert.True(PrivateOriginalMapBaseViewProjection.IsExactNearestReplication(
            payloadAfter,
            atlasAfter));
        Assert.NotEqual(atlasBefore.RgbaBytes, atlasAfter.RgbaBytes);
        atlas[0] ^= 0xFF;
        Assert.True(PrivateOriginalMapBaseViewProjection.IsExactNearestReplication(
            payloadBefore,
            atlasBefore));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void PhysicalAtlasProjectionPreservesSubpixelsAndFlipsTheCompleteTileAxis(int scale)
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        ushort[] block = new ushort[9];
        block[0] = 0x0100;
        block[1] = 0x1900;
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(
            [block],
            new ushort[WorkingMapLayout.WordCount]);
        byte[] atlas = BuildDetailedAtlas(scale);

        PrivateOriginalMapBaseViewProjection projection =
            PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                snapshot,
                visual.Selection,
                atlas,
                scale);

        Assert.Equal(new byte[] { 0, 0, 0, 255 }, Pixel(projection, 0, 0));
        Assert.Equal(
            new byte[] { (byte)((8 * scale) - 1), (byte)((8 * scale) - 1), 0, 255 },
            Pixel(projection, 8 * scale, 0));
        Assert.Equal(
            new byte[] { 0, 1, 0, 255 },
            Pixel(projection, 0, 1));
        Assert.Equal(
            new byte[] { (byte)((8 * scale) - 1), (byte)((8 * scale) - 2), 0, 255 },
            Pixel(projection, 8 * scale, 1));
        atlas[0] = 0xFF;
        Assert.Equal(new byte[] { 0, 0, 0, 255 }, Pixel(projection, 0, 0));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void ExactNearestBindingRejectsOnePhysicalSubpixelDrift(int scale)
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(
            [Enumerable.Repeat((ushort)0x0100, 9).ToArray()],
            new ushort[WorkingMapLayout.WordCount]);
        byte[] atlas = BuildNearestAtlas(visual, scale);
        atlas[0] ^= 0x01;

        PrivateOriginalMapBaseViewProjection logical =
            PrivateOriginalMapBaseViewProjection.Create(snapshot, visual);
        PrivateOriginalMapBaseViewProjection physical =
            PrivateOriginalMapBaseViewProjection.CreateFromAtlas(
                snapshot,
                visual.Selection,
                atlas,
                scale);

        Assert.False(PrivateOriginalMapBaseViewProjection.IsExactNearestReplication(
            logical,
            physical));
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

    [Fact]
    public void EdgeScale2xUsesExactRgbaNeighborsClampsEdgesAndAddsNoColors()
    {
        byte[] transparent = [0, 0, 0, 0];
        byte[] red = [240, 16, 16, 128];
        byte[] blue = [16, 16, 240, 255];
        byte[] green = [16, 240, 16, 64];
        byte[] source =
        [
            .. transparent, .. red, .. transparent,
            .. red, .. blue, .. green,
            .. transparent, .. green, .. transparent,
        ];

        byte[] output = PrivateOriginalMapBaseViewProjection.ApplyEdgeScale2x(
            source,
            width: 3,
            height: 3);

        Assert.Equal(6 * 6 * 4, output.Length);
        Assert.Equal(transparent, Pixel(output, width: 6, x: 0, y: 0));
        Assert.Equal(red, Pixel(output, width: 6, x: 2, y: 2));
        Assert.Equal(blue, Pixel(output, width: 6, x: 3, y: 2));
        Assert.Equal(blue, Pixel(output, width: 6, x: 2, y: 3));
        Assert.Equal(green, Pixel(output, width: 6, x: 3, y: 3));

        HashSet<string> sourceColors = PixelKeys(source);
        Assert.All(PixelKeys(output), color => Assert.Contains(color, sourceColors));
        byte firstOutput = output[0];
        source[0] = 99;
        Assert.Equal(firstOutput, output[0]);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void EdgeTreatmentIsDeterministicAcrossBucketsAndPreservesProjectionIdentity(
        int outputScale)
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        ushort[] block =
        [
            0x0100, 0x0101, 0x1902,
            0x0100, 0x0101, 0x1902,
            0x0100, 0x0101, 0x1902,
        ];
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot(
            [block],
            new ushort[WorkingMapLayout.WordCount]);
        PrivateOriginalMapBaseViewProjection logical =
            PrivateOriginalMapBaseViewProjection.Create(snapshot, visual);

        PrivateOriginalMapBaseViewProjection treated =
            PrivateOriginalMapBaseViewProjection.CreateEdgeScale2x(
                logical,
                outputScale);
        PrivateOriginalMapBaseViewProjection repeated =
            PrivateOriginalMapBaseViewProjection.CreateEdgeScale2x(
                logical,
                outputScale);

        Assert.Equal(logical.Map, treated.Map);
        Assert.Equal(logical.OriginX, treated.OriginX);
        Assert.Equal(logical.OriginY, treated.OriginY);
        Assert.Equal(logical.PlayerColumn, treated.PlayerColumn);
        Assert.Equal(logical.PlayerRow, treated.PlayerRow);
        Assert.Equal(outputScale, treated.RasterScale);
        Assert.Equal(288 * outputScale, treated.RasterPixelWidth);
        Assert.Equal(168 * outputScale, treated.RasterPixelHeight);
        Assert.Equal(treated.RgbaBytes, repeated.RgbaBytes);
        HashSet<string> logicalColors = PixelKeys(logical.RgbaBytes);
        Assert.All(
            PixelKeys(treated.RgbaBytes),
            color => Assert.Contains(color, logicalColors));

        if (outputScale == 4)
        {
            PrivateOriginalMapBaseViewProjection edge2x =
                PrivateOriginalMapBaseViewProjection.CreateEdgeScale2x(
                    logical,
                    outputScale: 2);
            AssertNearestReplication(edge2x, treated, factor: 2);
        }
    }

    [Fact]
    public void EdgeTreatmentCrossesComposedTileAndBlockBoundariesWithoutAtlasSeams()
    {
        OriginalMapVisualPayloadDefinition visual = VisualDefinition();
        ushort[] firstBlock = Enumerable.Repeat((ushort)0x0100, 9).ToArray();
        firstBlock[1] = 0x0101;
        firstBlock[2] = 0x1902;
        ushort[] secondBlock = Enumerable.Repeat((ushort)0x0101, 9).ToArray();
        ushort[] layout = new ushort[WorkingMapLayout.WordCount];
        layout[51] = 1;
        PrivateOriginalMapBaseViewProjection logical =
            PrivateOriginalMapBaseViewProjection.Create(
                Snapshot([firstBlock, secondBlock], layout),
                visual);
        PrivateOriginalMapBaseViewProjection edge =
            PrivateOriginalMapBaseViewProjection.CreateEdgeScale2x(
                logical,
                outputScale: 2);

        Assert.Equal(new byte[] { 255, 0, 0, 255 }, Pixel(edge, 14, 4));
        Assert.Equal(new byte[] { 255, 0, 0, 255 }, Pixel(edge, 15, 4));
        Assert.Equal(new byte[] { 0, 255, 0, 255 }, Pixel(edge, 16, 4));
        Assert.Equal(new byte[] { 0, 255, 0, 255 }, Pixel(edge, 17, 4));
        Assert.Equal(new byte[] { 255, 0, 0, 255 }, Pixel(edge, 46, 16));
        Assert.Equal(new byte[] { 255, 0, 0, 255 }, Pixel(edge, 47, 16));
        Assert.Equal(new byte[] { 0, 255, 0, 255 }, Pixel(edge, 48, 16));
        Assert.Equal(new byte[] { 0, 255, 0, 255 }, Pixel(edge, 49, 16));
        Assert.Contains(
            Enumerable.Range(0, edge.RgbaBytes.Count / 4)
                .Select(index => Pixel(
                    edge.RgbaBytes,
                    edge.RasterPixelWidth,
                    index % edge.RasterPixelWidth,
                    index / edge.RasterPixelWidth)),
            pixel => pixel.SequenceEqual(new byte[] { 0, 0, 255, 255 }));
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
        FillTile(decoded[0], localTile: 3, packedPixels: 0x44);
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

    private static byte[] BuildDetailedAtlas(int scale)
    {
        int logicalWidth = PrivateLocalPresentationAssetCatalog.Map3BaseAtlasLogicalWidth;
        int logicalHeight = PrivateLocalPresentationAssetCatalog.Map3BaseAtlasLogicalHeight;
        int width = logicalWidth * scale;
        byte[] rgba = new byte[checked(width * logicalHeight * scale * 4)];
        for (int y = 0; y < 8 * scale; y++)
        {
            for (int x = 0; x < 8 * scale; x++)
            {
                int offset = ((y * width) + x) * 4;
                rgba[offset] = (byte)x;
                rgba[offset + 1] = (byte)y;
                rgba[offset + 3] = 255;
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
        byte paletteIndex = 0,
        IEnumerable<OriginalMapAreaDefinition>? areaDefinitions = null)
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
        OriginalMapAreaCatalog areaCatalog = new(areaDefinitions ??
        [
            Area(
                ordinal: 1,
                bounds: new OriginalMapTraversalArea(0, 0, 63, 63),
                foreground: new OriginalMapAreaWordPair(0, 0)),
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

    private static OriginalMapAreaDefinition[] StaticOverlayAreas() =>
    [
        Area(
            ordinal: 1,
            bounds: new OriginalMapTraversalArea(0, 0, 50, 31),
            foreground: new OriginalMapAreaWordPair(0, 32)),
        Area(
            ordinal: 2,
            bounds: new OriginalMapTraversalArea(51, 0, 61, 9),
            foreground: new OriginalMapAreaWordPair(0, 0)),
    ];

    private static OriginalMapAreaDefinition Area(
        int ordinal,
        OriginalMapTraversalArea bounds,
        OriginalMapAreaWordPair foreground) =>
        new(
            new OriginalMapAreaRecordIdentity(
                "project-authored-base-view-areas",
                ordinal),
            bounds,
            foreground,
            new OriginalMapAreaWordPair(0, 0),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaBytePair(0, 0),
            new OriginalMapAreaBytePair(0, 0),
            mainLayerType: 0,
            defaultMusic: 0);

    private static ushort[] StaticOverlayLayout(
        int firstOverlayBlock,
        int transparentOverlayBlock = 2)
    {
        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        for (int y = 32; y < 32 + PrivateOriginalMapBaseViewProjection.RowCount; y++)
        {
            for (int x = 0; x < PrivateOriginalMapBaseViewProjection.ColumnCount; x++)
            {
                words[(y * WorkingMapLayout.ColumnCount) + x] =
                    checked((ushort)transparentOverlayBlock);
            }
        }

        words[32 * WorkingMapLayout.ColumnCount] = checked((ushort)firstOverlayBlock);
        return words;
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
        int offset = ((y * projection.RasterPixelWidth) + x) * 4;
        return projection.RgbaBytes.Skip(offset).Take(4).ToArray();
    }

    private static byte[] Pixel(
        IReadOnlyList<byte> rgbaBytes,
        int width,
        int x,
        int y)
    {
        int offset = ((y * width) + x) * 4;
        return rgbaBytes.Skip(offset).Take(4).ToArray();
    }

    private static HashSet<string> PixelKeys(IReadOnlyList<byte> rgbaBytes) =>
        Enumerable.Range(0, rgbaBytes.Count / 4)
            .Select(index => Convert.ToHexString(
                rgbaBytes.Skip(index * 4).Take(4).ToArray()))
            .ToHashSet(StringComparer.Ordinal);

    private static void AssertNearestReplication(
        PrivateOriginalMapBaseViewProjection source,
        PrivateOriginalMapBaseViewProjection target,
        int factor)
    {
        for (int y = 0; y < source.RasterPixelHeight; y++)
        {
            for (int x = 0; x < source.RasterPixelWidth; x++)
            {
                byte[] expected = Pixel(source, x, y);
                for (int subY = 0; subY < factor; subY++)
                {
                    for (int subX = 0; subX < factor; subX++)
                    {
                        Assert.Equal(
                            expected,
                            Pixel(
                                target,
                                (x * factor) + subX,
                                (y * factor) + subY));
                    }
                }
            }
        }
    }
}
