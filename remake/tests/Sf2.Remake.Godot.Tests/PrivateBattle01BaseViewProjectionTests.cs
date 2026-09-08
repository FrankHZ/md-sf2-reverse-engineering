using System.Buffers.Binary;
using System.IO.Compression;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Sf2.Remake.TestSupport;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class PrivateBattle01BaseViewProjectionTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void SharedBlockSamplerPreservesPhysicalTexelsFlipsTransparencyAndTwentyFourPixelSpacing(int scale)
    {
        var block = new OriginalMapBlockDefinition(new("authored", 0),
            new ushort[] { 0x100, 0x900, 0x1100, 0x1900, 0x101, 0x100, 0x100, 0x100, 0x100 });
        var blocks = new OriginalMapBlockCatalog([block]);
        var layout = new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]);
        byte[] atlas = new byte[128 * 320 * scale * scale * 4];
        for (int y = 0; y < 8 * scale; y++)
        for (int x = 0; x < 8 * scale; x++)
        {
            int offset = (y * 128 * scale + x) * 4;
            atlas[offset] = (byte)(x * 7); atlas[offset + 1] = (byte)(y * 7);
            atlas[offset + 2] = 73; atlas[offset + 3] = 255;
        }
        byte[] pixels = PrivateOriginalMapBaseViewProjection.RenderAtlasRegion(layout, blocks,
            atlas, scale, 0, 0, 2, 1);
        Assert.Equal(48 * 24 * scale * scale * 4, pixels.Length);
        for (int y = 0; y < 8 * scale; y++)
        for (int x = 0; x < 8 * scale; x++)
        {
            Assert.Equal(new byte[] { (byte)(x * 7), (byte)(y * 7), 73, 255 }, Pixel(pixels, 48 * scale, x, y));
            Assert.Equal(new byte[] { (byte)((8 * scale - 1 - x) * 7), (byte)(y * 7), 73, 255 },
                Pixel(pixels, 48 * scale, x + 8 * scale, y));
            Assert.Equal(new byte[] { (byte)(x * 7), (byte)((8 * scale - 1 - y) * 7), 73, 255 },
                Pixel(pixels, 48 * scale, x + 16 * scale, y));
            Assert.Equal(new byte[] { (byte)((8 * scale - 1 - x) * 7), (byte)((8 * scale - 1 - y) * 7), 73, 255 },
                Pixel(pixels, 48 * scale, x, y + 8 * scale));
            Assert.Equal(new byte[] { 0x12, 0x18, 0x20, 255 },
                Pixel(pixels, 48 * scale, x + 8 * scale, y + 8 * scale));
            Assert.Equal(Pixel(pixels, 48 * scale, x, y), Pixel(pixels, 48 * scale, x + 24 * scale, y));
        }
    }

    [Fact]
    public void UnusedCatalogSlotsAreAllowedButAnyReferencedUnloadedSlotRejects()
    {
        var blocks = new OriginalMapBlockCatalog(new ushort[] { 0x27F, 0x280, 0x300 }.Select((word, index) =>
            new OriginalMapBlockDefinition(new("authored", index), Enumerable.Repeat(word, 9))));
        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        PrivateBattle01BaseViewProjection.ValidateReferencedSlots(new(words), blocks);
        foreach (ushort index in new ushort[] { 1, 2 })
        {
            words[63 * 64 + 63] = index; // Also reject outside the visible battle area.
            Assert.Throws<ArgumentException>(() => PrivateBattle01BaseViewProjection.ValidateReferencedSlots(new(words), blocks));
        }
    }

    [PrivateInputFact("SF2_PRIVATE_CANONICAL_MAP_IMPORT", "SF2_PRIVATE_PRESENTATION_ASSET_ROOT")]
    public void AcceptedMap57DefinitionAndRealBucketsComposeTheFixedAreaAndRejectLayoutDrift()
    {
        string canonical = PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_CANONICAL_MAP_IMPORT");
        string assets = PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_PRESENTATION_ASSET_ROOT");
        var import = Assert.IsType<OriginalMapImportAccepted>(new PrivateCanonicalMap3ImportReader(canonical).Admit(
            new(OriginalMapRuntimeAdmission.PackageId, ContentProfile.PrivateLocal, OriginalMapRuntimeAdmission.AcceptedContentDigest)));
        var definition = import.Definition.Battle01Admission!;
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedBattle01Admission(definition));
        var reader = new LocalPresentationAssetPackReader(assets, PrivateLocalPresentationAssetCatalog.Map3AssetRepositoryCommit);
        var request = new LocalPresentationAssetPackRequest(LocalPresentationAssetPackAdmission.PackageId,
            ContentProfile.PrivateLocal, LocalPresentationAssetPackAdmission.RepositoryId,
            PrivateLocalPresentationAssetCatalog.Map3AssetRepositoryCommit, PrivateLocalPresentationAssetCatalog.Map3AssetManifestDigest);
        var accepted = Assert.IsType<LocalPresentationAssetPackAccepted>(reader.Admit(request));
        var catalog = new PrivateLocalPresentationAssetCatalog(reader);
        PrivateBattle01BaseViewProjection? smaller = null;
        foreach (int scale in new[] { 2, 4 })
        {
            var mount = Assert.IsType<PrivateLocalPresentationAssetMounted>(catalog.MountMap57BaseAtlas(request, accepted, scale)).Asset;
            byte[] rgba = ReadRgba(mount.CopyPngBytes(), 128 * scale, 320 * scale);
            var view = PrivateBattle01BaseViewProjection.Create(definition, rgba, scale);
            Assert.Same(definition, view.Definition);
            Assert.Equal((384, 480, scale), (view.PixelWidth, view.PixelHeight, view.RasterScale));
            Assert.Equal(384 * 480 * scale * scale * 4, view.RgbaBytes.Count);
            Assert.Contains(view.RgbaBytes, value => value != 0x12 && value != 0x18 && value != 0x20 && value != 255);
            if (smaller is null) smaller = view;
            else
            {
                byte[] large = view.RgbaBytes.ToArray(), small = smaller.RgbaBytes.ToArray();
                for (int y = 0; y < 960; y++)
                {
                    byte[] expected = new byte[1536 * 4];
                    for (int x = 0; x < 768; x++)
                    {
                        var color = Pixel(small, 768, x, y);
                        color.CopyTo(expected, x * 8); color.CopyTo(expected, x * 8 + 4);
                    }
                    Assert.Equal(expected, large.AsSpan(y * 2 * 1536 * 4, 1536 * 4).ToArray());
                    Assert.Equal(expected, large.AsSpan((y * 2 + 1) * 1536 * 4, 1536 * 4).ToArray());
                }
            }
            ushort[] changed = definition.WorkingLayout.Words.ToArray();
            changed[0] = changed[0] == 0 ? (ushort)3 : (ushort)0;
            var drift = new OriginalBattle01AdmissionDefinition(new(changed), definition.BlockCatalog,
                definition.AreaCatalog, definition.VisualResourceSelection, definition.WarpIdentity,
                definition.TriggerX, definition.TriggerY, definition.Destination, definition.DestinationOpaqueFacing, definition.Preset);
            Assert.Throws<ArgumentException>(() => PrivateBattle01BaseViewProjection.Create(drift, rgba, scale));
        }
    }

    private static byte[] Pixel(byte[] pixels, int width, int x, int y) =>
        pixels.AsSpan((y * width + x) * 4, 4).ToArray();

    // The maintained atlas writer emits RGBA8 filter-zero rows. Decode with the
    // standard PNG/zlib boundary so this required test runs without a Godot host.
    private static byte[] ReadRgba(byte[] png, int width, int height)
    {
        using MemoryStream compressed = new();
        for (int offset = 8; offset < png.Length;)
        {
            int length = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(offset, 4));
            string kind = System.Text.Encoding.ASCII.GetString(png, offset + 4, 4);
            if (kind == "IHDR")
            {
                Assert.Equal(width, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(offset + 8, 4)));
                Assert.Equal(height, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(offset + 12, 4)));
                Assert.Equal(8, png[offset + 16]); Assert.Equal(6, png[offset + 17]);
            }
            if (kind == "IDAT") compressed.Write(png, offset + 8, length);
            offset += length + 12;
        }
        compressed.Position = 0;
        using ZLibStream decoder = new(compressed, CompressionMode.Decompress);
        byte[] rgba = new byte[width * height * 4];
        for (int y = 0; y < height; y++)
        {
            Assert.Equal(0, decoder.ReadByte());
            decoder.ReadExactly(rgba.AsSpan(y * width * 4, width * 4));
        }
        Assert.Equal(-1, decoder.ReadByte());
        return rgba;
    }
}
