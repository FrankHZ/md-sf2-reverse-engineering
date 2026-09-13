using System.Collections.ObjectModel;

namespace Sf2.Remake.Application.Content;

public static class LocalPresentationAssetPackAdmission
{
    public const string PackageId = "sf2-local-presentation-asset-pack-v1";
    public const int SchemaVersion = 1;
    public const string RepositoryId = "md-sf2-remake-assets";
    public const string Capability =
        "private-local-presentation-asset-pack-admission-v1";
    public const int LogicalWidth = 960;
    public const int LogicalHeight = 540;
    public const string RasterMediaType = "image/png";
    public const string ColorSpace = "srgb";
    public const string AlphaMode = "straight";
    // V1 manifests are metadata-only; 4 MiB closes allocation before JSON admission.
    public const int MaximumManifestBytes = 4 * 1024 * 1024;

    // A 256 MiB per-bucket ceiling safely covers a 4K RGBA surface without int-sized reads.
    public const long MaximumRasterPayloadBytes = 256L * 1024 * 1024;

    private static readonly ReadOnlyCollection<int> ReadOnlyBucketScales =
        Array.AsReadOnly(new[] { 2, 4 });
    private static readonly ReadOnlyCollection<string> ReadOnlyRasterFilters =
        Array.AsReadOnly(new[] { "nearest", "linear" });

    public static IReadOnlyList<int> BucketScales => ReadOnlyBucketScales;

    public static IReadOnlyList<string> RasterFilters => ReadOnlyRasterFilters;

    public static bool IsCanonicalSemanticId(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 160 ||
            !IsLowerAsciiLetterOrDigit(value[0]) ||
            !IsLowerAsciiLetterOrDigit(value[^1]))
        {
            return false;
        }

        bool previousWasSeparator = false;
        foreach (char character in value)
        {
            if (IsLowerAsciiLetterOrDigit(character))
            {
                previousWasSeparator = false;
                continue;
            }

            if (character is not ('.' or '-') || previousWasSeparator)
            {
                return false;
            }

            previousWasSeparator = true;
        }

        return true;
    }

    public static bool IsAcceptedRasterFilter(string? value) =>
        value is not null && ReadOnlyRasterFilters.Contains(value, StringComparer.Ordinal);

    internal static bool HasExactBucketScales(IEnumerable<int> scales)
    {
        ArgumentNullException.ThrowIfNull(scales);
        return scales.SequenceEqual(ReadOnlyBucketScales);
    }

    private static bool IsLowerAsciiLetterOrDigit(char character) =>
        character is (>= 'a' and <= 'z') or (>= '0' and <= '9');
}
