using System.Collections.ObjectModel;

namespace Sf2.Remake.Application.Content;

public sealed record LocalPresentationAssetPackRequest
{
    public LocalPresentationAssetPackRequest(
        string packageId,
        ContentProfile profile,
        string expectedRepositoryId,
        string expectedAssetRepositoryCommit,
        string expectedManifestDigest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(expectedRepositoryId);
        ValidateCanonicalCommit(
            expectedAssetRepositoryCommit,
            nameof(expectedAssetRepositoryCommit));
        OriginalMapImportRequest.ValidateSha256(
            expectedManifestDigest,
            nameof(expectedManifestDigest));

        PackageId = packageId;
        Profile = profile;
        ExpectedRepositoryId = expectedRepositoryId;
        ExpectedAssetRepositoryCommit = expectedAssetRepositoryCommit;
        ExpectedManifestDigest = expectedManifestDigest.ToUpperInvariant();
    }

    public string PackageId { get; }

    public ContentProfile Profile { get; }

    public string ExpectedRepositoryId { get; }

    public string ExpectedAssetRepositoryCommit { get; }

    public string ExpectedManifestDigest { get; }

    internal static void ValidateCanonicalCommit(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (value.Length != 40 ||
            value.Any(character =>
                character is not (>= '0' and <= '9') and
                not (>= 'a' and <= 'f')))
        {
            throw new ArgumentException(
                "An asset repository commit must be exactly 40 lowercase hexadecimal characters.",
                parameterName);
        }
    }
}

public sealed record LocalPresentationLogicalSize
{
    public LocalPresentationLogicalSize(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }
}

public sealed record LocalPresentationRasterBucket
{
    public LocalPresentationRasterBucket(
        int scale,
        int width,
        int height,
        long byteLength,
        string sha256,
        string mediaType,
        string filter,
        bool mipmaps,
        bool repeat,
        string colorSpace,
        string alphaMode)
    {
        if (!LocalPresentationAssetPackAdmission.BucketScales.Contains(scale))
        {
            throw new ArgumentOutOfRangeException(nameof(scale));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(byteLength, 1);
        OriginalMapImportRequest.ValidateSha256(sha256, nameof(sha256));
        if (!string.Equals(
                mediaType,
                LocalPresentationAssetPackAdmission.RasterMediaType,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A v1 raster bucket must use image/png.",
                nameof(mediaType));
        }

        if (!LocalPresentationAssetPackAdmission.IsAcceptedRasterFilter(filter))
        {
            throw new ArgumentException(
                "A v1 raster bucket filter is unsupported.",
                nameof(filter));
        }

        if (!string.Equals(
                colorSpace,
                LocalPresentationAssetPackAdmission.ColorSpace,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A v1 raster bucket must use the srgb color space.",
                nameof(colorSpace));
        }

        if (!string.Equals(
                alphaMode,
                LocalPresentationAssetPackAdmission.AlphaMode,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A v1 raster bucket must use straight alpha.",
                nameof(alphaMode));
        }

        Scale = scale;
        Width = width;
        Height = height;
        ByteLength = byteLength;
        Sha256 = sha256.ToUpperInvariant();
        MediaType = mediaType;
        Filter = filter;
        Mipmaps = mipmaps;
        Repeat = repeat;
        ColorSpace = colorSpace;
        AlphaMode = alphaMode;
    }

    public int Scale { get; }

    public int Width { get; }

    public int Height { get; }

    public long ByteLength { get; }

    public string Sha256 { get; }

    public string MediaType { get; }

    public string Filter { get; }

    public bool Mipmaps { get; }

    public bool Repeat { get; }

    public string ColorSpace { get; }

    public string AlphaMode { get; }
}

public sealed class LocalPresentationRasterAssetDefinition
{
    private readonly ReadOnlyCollection<LocalPresentationRasterBucket> _buckets;

    public LocalPresentationRasterAssetDefinition(
        string assetId,
        LocalPresentationLogicalSize logicalSize,
        IEnumerable<LocalPresentationRasterBucket> buckets)
    {
        if (!LocalPresentationAssetPackAdmission.IsCanonicalSemanticId(assetId))
        {
            throw new ArgumentException(
                "A local presentation asset identity must use the canonical lowercase semantic-ID grammar.",
                nameof(assetId));
        }

        LogicalSize = logicalSize ?? throw new ArgumentNullException(nameof(logicalSize));
        ArgumentNullException.ThrowIfNull(buckets);
        LocalPresentationRasterBucket[] copied =
        [
            .. buckets.Take(LocalPresentationAssetPackAdmission.BucketScales.Count + 1),
        ];
        if (copied.Length != LocalPresentationAssetPackAdmission.BucketScales.Count ||
            copied.Any(bucket => bucket is null) ||
            !LocalPresentationAssetPackAdmission.HasExactBucketScales(
                copied.Select(bucket => bucket.Scale)))
        {
            throw new ArgumentException(
                "A local presentation raster asset requires exact ordered 2x and 4x semantic buckets.",
                nameof(buckets));
        }

        for (int index = 0; index < copied.Length; index++)
        {
            LocalPresentationRasterBucket bucket = copied[index];
            int expectedWidth;
            int expectedHeight;
            try
            {
                expectedWidth = checked(logicalSize.Width * bucket.Scale);
                expectedHeight = checked(logicalSize.Height * bucket.Scale);
            }
            catch (OverflowException error)
            {
                throw new ArgumentException(
                    "A local presentation raster bucket dimension overflowed.",
                    nameof(buckets),
                    error);
            }

            if (bucket.Width != expectedWidth || bucket.Height != expectedHeight)
            {
                throw new ArgumentException(
                    "A local presentation raster bucket does not match its logical size and scale.",
                    nameof(buckets));
            }
        }

        AssetId = assetId;
        _buckets = Array.AsReadOnly(copied);
    }

    public string AssetId { get; }

    public LocalPresentationLogicalSize LogicalSize { get; }

    public IReadOnlyList<LocalPresentationRasterBucket> Buckets => _buckets;
}

public sealed class LocalPresentationAssetPackDefinition
{
    private readonly ReadOnlyCollection<LocalPresentationRasterAssetDefinition> _assets;

    public LocalPresentationAssetPackDefinition(
        string repositoryId,
        LocalPresentationLogicalSize logicalPresentation,
        IEnumerable<LocalPresentationRasterAssetDefinition> assets)
    {
        if (!string.Equals(
                repositoryId,
                LocalPresentationAssetPackAdmission.RepositoryId,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A local presentation asset pack requires the exact v1 repository identity.",
                nameof(repositoryId));
        }

        LogicalPresentation = logicalPresentation ??
            throw new ArgumentNullException(nameof(logicalPresentation));
        ArgumentNullException.ThrowIfNull(assets);
        LocalPresentationRasterAssetDefinition[] copied = [.. assets];
        if (copied.Length == 0 || copied.Any(asset => asset is null))
        {
            throw new ArgumentException(
                "A local presentation asset pack requires at least one raster asset.",
                nameof(assets));
        }

        if (copied.Select(asset => asset.AssetId).Distinct(StringComparer.Ordinal).Count() !=
            copied.Length)
        {
            throw new ArgumentException(
                "A local presentation asset pack cannot contain duplicate asset identities.",
                nameof(assets));
        }

        if (logicalPresentation.Width != LocalPresentationAssetPackAdmission.LogicalWidth ||
            logicalPresentation.Height != LocalPresentationAssetPackAdmission.LogicalHeight)
        {
            throw new ArgumentException(
                "The local presentation asset pack must use the accepted 960-by-540 logical grid.",
                nameof(logicalPresentation));
        }

        RepositoryId = repositoryId;
        _assets = Array.AsReadOnly(copied);
    }

    public string RepositoryId { get; }

    public LocalPresentationLogicalSize LogicalPresentation { get; }

    public IReadOnlyList<LocalPresentationRasterAssetDefinition> Assets => _assets;
}

public sealed class LocalPresentationAssetPackReceipt
{
    private readonly ReadOnlyCollection<int> _bucketScales;

    public LocalPresentationAssetPackReceipt(
        string packageId,
        int schemaVersion,
        ContentProfile profile,
        string capability,
        string repositoryId,
        string mountedAssetRepositoryCommit,
        string manifestDigest,
        int assetCount,
        int bucketCount,
        IEnumerable<int> bucketScales)
    {
        if (!string.Equals(
                packageId,
                LocalPresentationAssetPackAdmission.PackageId,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A local presentation asset-pack receipt requires the exact v1 package identity.",
                nameof(packageId));
        }

        if (schemaVersion != LocalPresentationAssetPackAdmission.SchemaVersion)
        {
            throw new ArgumentOutOfRangeException(
                nameof(schemaVersion),
                "A local presentation asset-pack receipt requires the exact v1 schema version.");
        }

        if (profile != ContentProfile.PrivateLocal)
        {
            throw new ArgumentException(
                "A local presentation asset-pack receipt must remain PrivateLocal.",
                nameof(profile));
        }

        if (!string.Equals(
                capability,
                LocalPresentationAssetPackAdmission.Capability,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A local presentation asset-pack receipt requires the exact v1 capability.",
                nameof(capability));
        }

        if (!string.Equals(
                repositoryId,
                LocalPresentationAssetPackAdmission.RepositoryId,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A local presentation asset-pack receipt requires the exact v1 repository identity.",
                nameof(repositoryId));
        }

        LocalPresentationAssetPackRequest.ValidateCanonicalCommit(
            mountedAssetRepositoryCommit,
            nameof(mountedAssetRepositoryCommit));
        OriginalMapImportRequest.ValidateSha256(manifestDigest, nameof(manifestDigest));
        ArgumentOutOfRangeException.ThrowIfLessThan(assetCount, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(bucketCount, 2);
        ArgumentNullException.ThrowIfNull(bucketScales);
        int[] copiedScales = [.. bucketScales];
        if (!LocalPresentationAssetPackAdmission.HasExactBucketScales(copiedScales))
        {
            throw new ArgumentException(
                "A local presentation asset-pack receipt requires exact 2x and 4x scales.",
                nameof(bucketScales));
        }

        if (bucketCount != checked(assetCount * copiedScales.Length))
        {
            throw new ArgumentException(
                "A local presentation asset-pack receipt bucket count drifted.",
                nameof(bucketCount));
        }

        PackageId = packageId;
        SchemaVersion = schemaVersion;
        Profile = profile;
        Capability = capability;
        RepositoryId = repositoryId;
        MountedAssetRepositoryCommit = mountedAssetRepositoryCommit;
        ManifestDigest = manifestDigest.ToUpperInvariant();
        AssetCount = assetCount;
        BucketCount = bucketCount;
        _bucketScales = Array.AsReadOnly(copiedScales);
    }

    public string PackageId { get; }

    public int SchemaVersion { get; }

    public ContentProfile Profile { get; }

    public string Capability { get; }

    public string RepositoryId { get; }

    public string MountedAssetRepositoryCommit { get; }

    public string ManifestDigest { get; }

    public int AssetCount { get; }

    public int BucketCount { get; }

    public IReadOnlyList<int> BucketScales => _bucketScales;
}

public enum LocalPresentationAssetPackFailureCode
{
    PackageUnavailable,
    PackageIdentityMismatch,
    ProfileMismatch,
    RepositoryIdentityMismatch,
    ContentDigestMismatch,
    UnsupportedSchema,
    InvalidManifest,
    UnsupportedCapability,
    DuplicateIdentity,
    MissingBucket,
    InvalidBucket,
    AssetPathRejected,
    PayloadMismatch,
}

public sealed record LocalPresentationAssetPackDiagnostic
{
    public LocalPresentationAssetPackDiagnostic(
        LocalPresentationAssetPackFailureCode code,
        string field,
        string message)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Field = field;
        Message = message;
    }

    public LocalPresentationAssetPackFailureCode Code { get; }

    public string Field { get; }

    public string Message { get; }
}

public abstract record LocalPresentationAssetPackResult;

public sealed record LocalPresentationAssetPackAccepted(
    LocalPresentationAssetPackDefinition Definition,
    LocalPresentationAssetPackReceipt Receipt) : LocalPresentationAssetPackResult
{
    public LocalPresentationAssetPackDefinition Definition { get; } =
        Definition ?? throw new ArgumentNullException(nameof(Definition));

    public LocalPresentationAssetPackReceipt Receipt { get; } =
        Receipt ?? throw new ArgumentNullException(nameof(Receipt));
}

public sealed record LocalPresentationAssetPackRejected(
    LocalPresentationAssetPackDiagnostic Diagnostic) : LocalPresentationAssetPackResult
{
    public LocalPresentationAssetPackDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

public interface ILocalPresentationAssetPackSource
{
    LocalPresentationAssetPackResult Admit(LocalPresentationAssetPackRequest request);
}
