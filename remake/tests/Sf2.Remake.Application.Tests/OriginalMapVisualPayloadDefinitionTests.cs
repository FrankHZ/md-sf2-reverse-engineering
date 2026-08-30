using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class OriginalMapVisualPayloadDefinitionTests
{
    [Fact]
    public void ExactDefinitionDefensivelyOwnsPaletteAndFiveOrderedDecodedTilesets()
    {
        OriginalMapVisualResourceSelection selection = Selection();
        ushort[] sourceWords = SourceWords();
        byte[][] decoded = Enumerable.Range(0, 5)
            .Select(index => Enumerable.Repeat(
                    checked((byte)(index + 1)),
                    OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset)
                .ToArray())
            .ToArray();
        OriginalMapPalettePayload palette = new(selection.PaletteIndex, sourceWords);
        OriginalMapTilesetPayload[] tilesets = Enumerable.Range(0, 5)
            .Select(index => new OriginalMapTilesetPayload(
                index + 1,
                selection.TilesetSlots[index],
                decoded[index]))
            .ToArray();

        OriginalMapVisualPayloadDefinition definition = new(selection, palette, tilesets);
        sourceWords[0] = 0;
        decoded[0][0] = 99;
        tilesets[0] = tilesets[1];

        Assert.Equal((ushort)0x0EEE, definition.Palette.SourceWords[0]);
        Assert.Equal((ushort)0, definition.Palette.EffectiveWords[0]);
        Assert.Equal((ushort)0x0222, definition.Palette.EffectiveWords[1]);
        Assert.Equal(5, definition.Tilesets.Count);
        Assert.Equal(Enumerable.Range(1, 5), definition.Tilesets.Select(item => item.SlotOrdinal));
        Assert.Equal(selection.TilesetSlots, definition.Tilesets.Select(item => item.ResourceIndex));
        Assert.Equal((byte)1, definition.Tilesets[0].DecodedBytes[0]);
        Assert.All(
            definition.Tilesets,
            item => Assert.Equal(
                OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset,
                item.DecodedBytes.Count));
        Assert.Contains(
            "map3-animation-tileset-74-and-replacement-lifecycle",
            definition.UnsupportedCapabilities);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<byte>)definition.Tilesets[0].DecodedBytes).Add(1));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<ushort>)definition.Palette.SourceWords).Add(0));
    }

    [Fact]
    public void DefinitionRejectsSelectionPaletteSlotAndDecodedShapeDrift()
    {
        OriginalMapVisualResourceSelection selection = Selection();
        OriginalMapPalettePayload palette = new(selection.PaletteIndex, SourceWords());
        OriginalMapTilesetPayload[] exact = Tilesets(selection);

        Assert.Throws<ArgumentException>(() => new OriginalMapVisualPayloadDefinition(
            new OriginalMapVisualResourceSelection(
                new MapId("map3"),
                0,
                new byte[] { 0, 37, 43, 53, 65 }),
            palette,
            exact));
        Assert.Throws<ArgumentException>(() => new OriginalMapVisualPayloadDefinition(
            selection,
            new OriginalMapPalettePayload(1, SourceWords()),
            exact));
        Assert.Throws<ArgumentException>(() => new OriginalMapVisualPayloadDefinition(
            selection,
            palette,
            exact.Reverse()));
        Assert.Throws<ArgumentException>(() => new OriginalMapVisualPayloadDefinition(
            selection,
            palette,
            exact.Take(4)));
        Assert.Throws<ArgumentException>(() => new OriginalMapTilesetPayload(
            1,
            0,
            new byte[OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset - 1]));
        Assert.Throws<ArgumentException>(() => new OriginalMapPalettePayload(
            0,
            Enumerable.Repeat((ushort)0, 15)));
        ushort[] invalidMask = SourceWords();
        invalidMask[4] = 0x0001;
        Assert.Throws<ArgumentException>(() => new OriginalMapPalettePayload(0, invalidMask));
    }

    [Fact]
    public void RequestAndReceiptRetainOnlyFixedWholeContractIdentityAndShape()
    {
        OriginalMapVisualPayloadRequest request = Request();
        OriginalMapVisualPayloadReceipt receipt = new(
            OriginalMapVisualPayloadAdmission.PackageId,
            OriginalMapVisualPayloadAdmission.SchemaVersion,
            ContentProfile.PrivateLocal,
            OriginalMapVisualPayloadAdmission.Capability,
            new OriginalMapVisualPayloadProvenance(
                OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
                OriginalMapVisualPayloadAdmission.AcceptedUpstreamRepository,
                OriginalMapVisualPayloadAdmission.AcceptedUpstreamCommit,
                OriginalMapVisualPayloadAdmission.TilesetMetadataId,
                OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest,
                OriginalMapVisualPayloadAdmission.PaletteMetadataId,
                OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest),
            OriginalMapVisualPayloadAdmission.RequiredEvidenceOwners,
            1,
            16,
            5,
            4096);

        Assert.Equal(ContentProfile.PrivateLocal, request.Profile);
        Assert.Equal(OriginalMapVisualPayloadAdmission.Capability, receipt.Capability);
        Assert.Equal(
            new[]
            {
                OriginalMapVisualPayloadAdmission.TilesetMetadataId,
                OriginalMapVisualPayloadAdmission.PaletteMetadataId,
            },
            receipt.EvidenceOwnerIds);
        Assert.Equal(5, receipt.TilesetCount);
        Assert.Equal(4096, receipt.DecodedBytesPerTileset);
        Assert.DoesNotContain(
            receipt.GetType().GetProperties(),
            property => property.Name.Contains("Path", StringComparison.Ordinal) ||
                property.Name.Contains("Address", StringComparison.Ordinal) ||
                property.Name.Contains("Payload", StringComparison.Ordinal));
    }

    private static OriginalMapVisualPayloadRequest Request() =>
        new(
            OriginalMapVisualPayloadAdmission.PackageId,
            ContentProfile.PrivateLocal,
            Selection(),
            OriginalMapVisualPayloadAdmission.AcceptedRomSha256,
            OriginalMapVisualPayloadAdmission.AcceptedTilesetMetadataDigest,
            OriginalMapVisualPayloadAdmission.AcceptedPaletteMetadataDigest);

    private static OriginalMapVisualResourceSelection Selection() =>
        new(
            new MapId("map3"),
            0,
            new byte[] { 0, 37, 43, 53, 66 });

    private static ushort[] SourceWords() =>
    [
        0x0EEE,
        0x0222,
        0x0444,
        0x0666,
        0x0888,
        0x0AAA,
        0x0CCC,
        0x0000,
        0x0002,
        0x0020,
        0x0200,
        0x000E,
        0x00E0,
        0x0E00,
        0x0246,
        0x068A,
    ];

    private static OriginalMapTilesetPayload[] Tilesets(
        OriginalMapVisualResourceSelection selection) =>
        Enumerable.Range(0, 5)
            .Select(index => new OriginalMapTilesetPayload(
                index + 1,
                selection.TilesetSlots[index],
                new byte[OriginalMapVisualPayloadAdmission.DecodedBytesPerTileset]))
            .ToArray();
}
