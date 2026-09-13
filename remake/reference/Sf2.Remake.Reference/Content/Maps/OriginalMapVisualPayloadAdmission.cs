using System.Collections.ObjectModel;

namespace Sf2.Remake.Application.Content;

public static class OriginalMapVisualPayloadAdmission
{
    public const string PackageId = "sf2-private-map3-base-visual-payload-v1";
    public const int SchemaVersion = 1;
    public const string Capability =
        "private-local-map3-base-visual-payload-admission-v1";

    public const string AcceptedRomSha256 =
        OriginalMapRuntimeAdmission.AcceptedRomSha256;
    public const string AcceptedUpstreamRepository =
        OriginalMapRuntimeAdmission.AcceptedUpstreamRepository;
    public const string AcceptedUpstreamCommit =
        OriginalMapRuntimeAdmission.AcceptedUpstreamCommit;

    public const string TilesetMetadataId = "sf2-map-tileset-decode-v1";
    public const string AcceptedTilesetMetadataDigest =
        "2EA6AB3485CAE4F92F31647C05233F0E1C07E81CCB02806706A51F9F0C1E087F";
    public const string PaletteMetadataId = "sf2-map-palette-static-v1";
    public const string AcceptedPaletteMetadataDigest =
        "4F977B4B3EB8E731D2ABB6664F36030487DC186D267E66E9C2DAF3CB211007AB";

    public const int SelectedPaletteCount = 1;
    public const int SelectedTilesetCount = 5;
    public const int PaletteWordCount = 16;
    public const int PaletteByteCount = PaletteWordCount * sizeof(ushort);
    public const ushort PaletteWordMask = 0x0EEE;
    public const int DecodedBytesPerTileset = 4096;
    public const int TotalDecodedTilesetBytes =
        SelectedTilesetCount * DecodedBytesPerTileset;

    private static readonly ReadOnlyCollection<string> ReadOnlyEvidenceOwners =
        Array.AsReadOnly(
            new[]
            {
                TilesetMetadataId,
                PaletteMetadataId,
            });

    private static readonly ReadOnlyCollection<string> ReadOnlyUnsupportedCapabilities =
        Array.AsReadOnly(
            new[]
            {
                "map3-animation-tileset-74-and-replacement-lifecycle",
                "tileset-load-vram-cache-and-reload-semantics",
                "block-tile-layer-camera-and-parallax-composition",
                "palette-rgb-color-space-cram-vint-dma-and-fades",
                "rendered-pixels-original-presentation-and-fidelity",
                "private-payload-publication-pck-and-export",
            });

    public static IReadOnlyList<string> RequiredEvidenceOwners => ReadOnlyEvidenceOwners;

    public static IReadOnlyList<string> UnsupportedCapabilities =>
        ReadOnlyUnsupportedCapabilities;

    public static bool HasExactAcceptedSelection(
        OriginalMapVisualResourceSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return OriginalMapRuntimeAdmission.HasExactAcceptedVisualResourceSelection(selection);
    }

    internal static bool HasExactEvidenceOwners(IEnumerable<string> evidenceOwners)
    {
        ArgumentNullException.ThrowIfNull(evidenceOwners);
        HashSet<string> actual = new(evidenceOwners, StringComparer.Ordinal);
        return actual.Count == ReadOnlyEvidenceOwners.Count &&
            actual.SetEquals(ReadOnlyEvidenceOwners);
    }
}
