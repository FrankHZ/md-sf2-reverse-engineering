using System.Security.Cryptography;
using System.Text.Json;
using Sf2.Remake.Application.Content;

namespace Sf2.Remake.Content;

public sealed class LocalPresentationAssetPackReader : ILocalPresentationAssetPackSource
{
    private const string ManifestRelativePath = "manifests/presentation-assets-v1.json";
    private const int MaximumAssetCount = 4096;

    private readonly string? _assetRoot;
    private readonly string? _mountedAssetRepositoryCommit;

    public LocalPresentationAssetPackReader(
        string assetRoot,
        string mountedAssetRepositoryCommit)
    {
        _assetRoot = ResolveFullyQualifiedPath(assetRoot);
        _mountedAssetRepositoryCommit = IsCanonicalCommit(mountedAssetRepositoryCommit)
            ? mountedAssetRepositoryCommit
            : null;
    }

    public LocalPresentationAssetPackResult Admit(LocalPresentationAssetPackRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!string.Equals(
                request.PackageId,
                LocalPresentationAssetPackAdmission.PackageId,
                StringComparison.Ordinal))
        {
            return Reject(
                LocalPresentationAssetPackFailureCode.PackageIdentityMismatch,
                "packageId",
                "The requested local presentation package identity is unsupported.");
        }

        if (request.Profile != ContentProfile.PrivateLocal)
        {
            return Reject(
                LocalPresentationAssetPackFailureCode.ProfileMismatch,
                "profile",
                "The local presentation asset pack is available only to the PrivateLocal profile.");
        }

        if (!string.Equals(
                request.ExpectedRepositoryId,
                LocalPresentationAssetPackAdmission.RepositoryId,
                StringComparison.Ordinal))
        {
            return Reject(
                LocalPresentationAssetPackFailureCode.RepositoryIdentityMismatch,
                "repositoryId",
                "The requested asset repository identity is unsupported.");
        }

        if (_mountedAssetRepositoryCommit is null ||
            !string.Equals(
                request.ExpectedAssetRepositoryCommit,
                _mountedAssetRepositoryCommit,
                StringComparison.Ordinal))
        {
            return Reject(
                LocalPresentationAssetPackFailureCode.RepositoryIdentityMismatch,
                "assetRepositoryCommit",
                "The mounted asset repository commit does not match the requested canonical identity.");
        }

        if (_assetRoot is null || !Directory.Exists(_assetRoot))
        {
            return Reject(
                LocalPresentationAssetPackFailureCode.PackageUnavailable,
                "assetRoot",
                "The local presentation asset-pack root is unavailable.");
        }

        if (!TryResolveContainedPath(
                _assetRoot,
                ManifestRelativePath,
                out string? manifestPath,
                out LocalPresentationAssetPackResult? pathFailure))
        {
            return pathFailure!;
        }

        if (!TryReadManifestBytes(
                manifestPath!,
                out byte[]? manifestBytes,
                out var readFailure))
        {
            return readFailure!;
        }

        string manifestDigest = Digest(manifestBytes!);
        if (!string.Equals(
                manifestDigest,
                request.ExpectedManifestDigest,
                StringComparison.Ordinal))
        {
            return Reject(
                LocalPresentationAssetPackFailureCode.ContentDigestMismatch,
                "manifestDigest",
                "The local presentation manifest bytes do not match the requested digest.");
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(
                manifestBytes!,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 32,
                });
            ParsedManifest parsed = ParseManifest(document.RootElement);
            List<LocalPresentationRasterAssetDefinition> definitions = [];
            HashSet<string> assetIds = new(StringComparer.Ordinal);
            HashSet<string> payloadPaths = new(
                OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal);

            for (int assetIndex = 0; assetIndex < parsed.Assets.Count; assetIndex++)
            {
                ParsedAsset asset = parsed.Assets[assetIndex];
                string assetField = $"assets[{assetIndex}]";
                if (!assetIds.Add(asset.AssetId))
                {
                    throw Invalid(
                        LocalPresentationAssetPackFailureCode.DuplicateIdentity,
                        assetField + ".assetId",
                        "The local presentation manifest contains a duplicate asset identity.");
                }

                List<LocalPresentationRasterBucket> buckets = [];
                for (int bucketIndex = 0; bucketIndex < asset.Buckets.Count; bucketIndex++)
                {
                    ParsedBucket bucket = asset.Buckets[bucketIndex];
                    string bucketField = $"{assetField}.buckets[{bucketIndex}]";
                    if (!TryResolveContainedPath(
                            _assetRoot,
                            bucket.RuntimePath,
                            out string? payloadPath,
                            out pathFailure))
                    {
                        return pathFailure!;
                    }

                    if (!payloadPaths.Add(payloadPath!))
                    {
                        throw Invalid(
                            LocalPresentationAssetPackFailureCode.DuplicateIdentity,
                            bucketField + ".runtimePath",
                            "The local presentation manifest reuses a runtime payload path.");
                    }

                    if (!TryVerifyPayload(
                            payloadPath!,
                            bucket.ByteLength,
                            bucket.Sha256,
                            bucketField + ".runtimePath",
                            out readFailure))
                    {
                        return readFailure!;
                    }

                    buckets.Add(new LocalPresentationRasterBucket(
                        bucket.Scale,
                        bucket.Width,
                        bucket.Height,
                        bucket.MediaType,
                        bucket.Filter,
                        bucket.Mipmaps,
                        bucket.Repeat,
                        bucket.ColorSpace,
                        bucket.AlphaMode));
                }

                definitions.Add(new LocalPresentationRasterAssetDefinition(
                    asset.AssetId,
                    new LocalPresentationLogicalSize(
                        asset.LogicalWidth,
                        asset.LogicalHeight),
                    buckets));
            }

            LocalPresentationAssetPackDefinition definition = new(
                parsed.RepositoryId,
                new LocalPresentationLogicalSize(parsed.LogicalWidth, parsed.LogicalHeight),
                definitions);
            LocalPresentationAssetPackReceipt receipt = new(
                parsed.PackageId,
                parsed.SchemaVersion,
                ContentProfile.PrivateLocal,
                LocalPresentationAssetPackAdmission.Capability,
                parsed.RepositoryId,
                _mountedAssetRepositoryCommit,
                manifestDigest,
                definitions.Count,
                definitions.Count * LocalPresentationAssetPackAdmission.BucketScales.Count,
                LocalPresentationAssetPackAdmission.BucketScales);
            return new LocalPresentationAssetPackAccepted(definition, receipt);
        }
        catch (JsonException)
        {
            return Reject(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                "manifest",
                "The local presentation manifest is not valid closed JSON.");
        }
        catch (ManifestException error)
        {
            return Reject(error.Code, error.Field, error.Message);
        }
        catch (ArgumentException)
        {
            return Reject(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                "manifest",
                "The local presentation manifest violates the admitted typed shape.");
        }
        catch (OverflowException)
        {
            return Reject(
                LocalPresentationAssetPackFailureCode.InvalidBucket,
                "manifest",
                "A local presentation raster dimension overflowed the admitted range.");
        }
    }

    private static ParsedManifest ParseManifest(JsonElement root)
    {
        RequireExactProperties(
            root,
            "manifest",
            "schemaVersion",
            "packageId",
            "repositoryId",
            "profile",
            "capabilities",
            "logicalPresentation",
            "assets");
        int schemaVersion = RequiredInt(root, "schemaVersion", "schemaVersion");
        if (schemaVersion != LocalPresentationAssetPackAdmission.SchemaVersion)
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.UnsupportedSchema,
                "schemaVersion",
                "The local presentation manifest schema version is unsupported.");
        }

        string packageId = RequiredString(root, "packageId", "packageId");
        if (!string.Equals(
                packageId,
                LocalPresentationAssetPackAdmission.PackageId,
                StringComparison.Ordinal))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.PackageIdentityMismatch,
                "packageId",
                "The local presentation manifest package identity drifted.");
        }

        string repositoryId = RequiredString(root, "repositoryId", "repositoryId");
        if (!string.Equals(
                repositoryId,
                LocalPresentationAssetPackAdmission.RepositoryId,
                StringComparison.Ordinal))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.RepositoryIdentityMismatch,
                "repositoryId",
                "The local presentation manifest repository identity drifted.");
        }

        string profile = RequiredString(root, "profile", "profile");
        if (!string.Equals(profile, "private-local", StringComparison.Ordinal))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.ProfileMismatch,
                "profile",
                "The local presentation manifest profile drifted.");
        }

        JsonElement capabilities = RequiredArray(root, "capabilities", "capabilities");
        if (capabilities.GetArrayLength() != 1 ||
            capabilities[0].ValueKind != JsonValueKind.String ||
            !string.Equals(
                capabilities[0].GetString(),
                LocalPresentationAssetPackAdmission.Capability,
                StringComparison.Ordinal))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.UnsupportedCapability,
                "capabilities",
                "The local presentation manifest capability set drifted.");
        }

        JsonElement logical = RequiredObject(
            root,
            "logicalPresentation",
            "logicalPresentation");
        RequireExactProperties(logical, "logicalPresentation", "width", "height");
        int logicalWidth = RequiredPositiveInt(logical, "width", "logicalPresentation.width");
        int logicalHeight = RequiredPositiveInt(logical, "height", "logicalPresentation.height");
        if (logicalWidth != LocalPresentationAssetPackAdmission.LogicalWidth ||
            logicalHeight != LocalPresentationAssetPackAdmission.LogicalHeight)
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                "logicalPresentation",
                "The local presentation manifest logical grid drifted.");
        }

        JsonElement assetsElement = RequiredArray(root, "assets", "assets");
        if (assetsElement.GetArrayLength() is < 1 or > MaximumAssetCount)
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                "assets",
                "The local presentation manifest asset count is outside the admitted range.");
        }

        List<ParsedAsset> assets = [];
        int index = 0;
        foreach (JsonElement asset in assetsElement.EnumerateArray())
        {
            assets.Add(ParseAsset(asset, index));
            index++;
        }

        return new ParsedManifest(
            schemaVersion,
            packageId,
            repositoryId,
            logicalWidth,
            logicalHeight,
            assets);
    }

    private static ParsedAsset ParseAsset(JsonElement asset, int assetIndex)
    {
        string field = $"assets[{assetIndex}]";
        RequireExactProperties(
            asset,
            field,
            "assetId",
            "kind",
            "logicalSize",
            "source",
            "derivation",
            "buckets");
        string assetId = RequiredString(asset, "assetId", field + ".assetId");
        if (!LocalPresentationAssetPackAdmission.IsCanonicalSemanticId(assetId))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                field + ".assetId",
                "A local presentation asset identity violates the canonical semantic-ID grammar.");
        }

        if (!string.Equals(
                RequiredString(asset, "kind", field + ".kind"),
                "raster-image",
                StringComparison.Ordinal))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                field + ".kind",
                "A v1 local presentation asset must be a raster-image.");
        }

        JsonElement logical = RequiredObject(asset, "logicalSize", field + ".logicalSize");
        RequireExactProperties(logical, field + ".logicalSize", "width", "height");
        int logicalWidth = RequiredPositiveInt(logical, "width", field + ".logicalSize.width");
        int logicalHeight = RequiredPositiveInt(logical, "height", field + ".logicalSize.height");

        JsonElement source = RequiredObject(asset, "source", field + ".source");
        RequireExactProperties(source, field + ".source", "assetId", "sha256");
        string sourceAssetId = RequiredString(source, "assetId", field + ".source.assetId");
        if (!LocalPresentationAssetPackAdmission.IsCanonicalSemanticId(sourceAssetId))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                field + ".source.assetId",
                "A local presentation source identity violates the canonical semantic-ID grammar.");
        }

        RequireSha256(source, "sha256", field + ".source.sha256");

        JsonElement derivation = RequiredObject(asset, "derivation", field + ".derivation");
        RequireExactProperties(
            derivation,
            field + ".derivation",
            "policyId",
            "generatorId",
            "generatorVersion",
            "generatorArtifactSha256");
        _ = RequiredString(derivation, "policyId", field + ".derivation.policyId");
        _ = RequiredString(derivation, "generatorId", field + ".derivation.generatorId");
        _ = RequiredString(derivation, "generatorVersion", field + ".derivation.generatorVersion");
        RequireSha256(
            derivation,
            "generatorArtifactSha256",
            field + ".derivation.generatorArtifactSha256");

        JsonElement bucketElements = RequiredArray(asset, "buckets", field + ".buckets");
        if (bucketElements.GetArrayLength() !=
            LocalPresentationAssetPackAdmission.BucketScales.Count)
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.MissingBucket,
                field + ".buckets",
                "A v1 local presentation raster requires exactly two ordered buckets.");
        }

        List<ParsedBucket> buckets = [];
        int bucketIndex = 0;
        foreach (JsonElement bucket in bucketElements.EnumerateArray())
        {
            buckets.Add(ParseBucket(
                bucket,
                field,
                bucketIndex,
                logicalWidth,
                logicalHeight));
            bucketIndex++;
        }

        return new ParsedAsset(assetId, logicalWidth, logicalHeight, buckets);
    }

    private static ParsedBucket ParseBucket(
        JsonElement bucket,
        string assetField,
        int bucketIndex,
        int logicalWidth,
        int logicalHeight)
    {
        string field = $"{assetField}.buckets[{bucketIndex}]";
        RequireExactProperties(
            bucket,
            field,
            "scale",
            "runtimePath",
            "width",
            "height",
            "byteLength",
            "sha256",
            "mediaType",
            "filter",
            "mipmaps",
            "repeat",
            "colorSpace",
            "alphaMode");
        int scale = RequiredPositiveInt(bucket, "scale", field + ".scale");
        if (scale != LocalPresentationAssetPackAdmission.BucketScales[bucketIndex])
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidBucket,
                field + ".scale",
                "The v1 local presentation bucket order or scale drifted.");
        }

        int width = RequiredPositiveInt(bucket, "width", field + ".width");
        int height = RequiredPositiveInt(bucket, "height", field + ".height");
        int expectedWidth = checked(logicalWidth * scale);
        int expectedHeight = checked(logicalHeight * scale);
        if (width != expectedWidth || height != expectedHeight)
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidBucket,
                field,
                "A local presentation bucket dimension does not match its logical size and scale.");
        }

        long byteLength = RequiredPositiveLong(bucket, "byteLength", field + ".byteLength");
        if (byteLength > LocalPresentationAssetPackAdmission.MaximumRasterPayloadBytes)
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidBucket,
                field + ".byteLength",
                "A local presentation payload length exceeds the admitted range.");
        }

        string sha256 = RequireSha256(bucket, "sha256", field + ".sha256");
        string mediaType = RequiredString(bucket, "mediaType", field + ".mediaType");
        if (!string.Equals(
                mediaType,
                LocalPresentationAssetPackAdmission.RasterMediaType,
                StringComparison.Ordinal))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidBucket,
                field + ".mediaType",
                "A v1 local presentation bucket must use image/png.");
        }

        string filter = RequiredString(bucket, "filter", field + ".filter");
        if (!LocalPresentationAssetPackAdmission.IsAcceptedRasterFilter(filter))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidBucket,
                field + ".filter",
                "A v1 local presentation bucket filter is unsupported.");
        }

        string colorSpace = RequiredString(bucket, "colorSpace", field + ".colorSpace");
        if (!string.Equals(
                colorSpace,
                LocalPresentationAssetPackAdmission.ColorSpace,
                StringComparison.Ordinal))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidBucket,
                field + ".colorSpace",
                "A v1 local presentation bucket must use srgb.");
        }

        string alphaMode = RequiredString(bucket, "alphaMode", field + ".alphaMode");
        if (!string.Equals(
                alphaMode,
                LocalPresentationAssetPackAdmission.AlphaMode,
                StringComparison.Ordinal))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidBucket,
                field + ".alphaMode",
                "A v1 local presentation bucket must use straight alpha.");
        }

        return new ParsedBucket(
            scale,
            RequiredString(bucket, "runtimePath", field + ".runtimePath"),
            width,
            height,
            byteLength,
            sha256,
            mediaType,
            filter,
            RequiredBoolean(bucket, "mipmaps", field + ".mipmaps"),
            RequiredBoolean(bucket, "repeat", field + ".repeat"),
            colorSpace,
            alphaMode);
    }

    private static bool TryResolveContainedPath(
        string root,
        string relativePath,
        out string? path,
        out LocalPresentationAssetPackResult? failure)
    {
        path = null;
        failure = null;
        if (string.IsNullOrWhiteSpace(relativePath) ||
            Path.IsPathFullyQualified(relativePath) ||
            relativePath.Contains('\\', StringComparison.Ordinal) ||
            !relativePath.StartsWith("runtime/", StringComparison.Ordinal) &&
                !string.Equals(relativePath, ManifestRelativePath, StringComparison.Ordinal))
        {
            failure = Reject(
                LocalPresentationAssetPackFailureCode.AssetPathRejected,
                "runtimePath",
                "A local presentation asset path is not an admitted relative path.");
            return false;
        }

        string[] parts = relativePath.Split('/');
        if (parts.Any(part => !IsCanonicalPathComponent(part)))
        {
            failure = Reject(
                LocalPresentationAssetPackFailureCode.AssetPathRejected,
                "runtimePath",
                "A local presentation asset path contains an invalid component.");
            return false;
        }

        string candidate;
        try
        {
            candidate = Path.GetFullPath(Path.Combine(root, Path.Combine(parts)));
            string relative = Path.GetRelativePath(root, candidate);
            if (Path.IsPathFullyQualified(relative) ||
                relative.Equals("..", StringComparison.Ordinal) ||
                relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                throw new ArgumentException("The resolved path escaped the asset root.");
            }
        }
        catch (Exception error) when (
            error is ArgumentException or NotSupportedException or PathTooLongException)
        {
            failure = Reject(
                LocalPresentationAssetPackFailureCode.AssetPathRejected,
                "runtimePath",
                "A local presentation asset path could not be resolved safely.");
            return false;
        }

        string current = root;
        if (!TryGetPathStatus(
                current,
                out bool currentExists,
                out bool isReparsePoint))
        {
            failure = Reject(
                LocalPresentationAssetPackFailureCode.PackageUnavailable,
                "runtimePath",
                "A local presentation path component could not be inspected.");
            return false;
        }

        if (isReparsePoint)
        {
            failure = ReparseFailure();
            return false;
        }

        if (!currentExists)
        {
            failure = Reject(
                LocalPresentationAssetPackFailureCode.PackageUnavailable,
                "runtimePath",
                "The local presentation asset-pack root is unavailable.");
            return false;
        }

        foreach (string part in parts)
        {
            current = Path.Combine(current, part);
            if (!TryGetPathStatus(current, out currentExists, out isReparsePoint))
            {
                failure = Reject(
                    LocalPresentationAssetPackFailureCode.PackageUnavailable,
                    "runtimePath",
                    "A local presentation path component could not be inspected.");
                return false;
            }

            if (isReparsePoint)
            {
                failure = ReparseFailure();
                return false;
            }

            if (!currentExists)
            {
                break;
            }
        }

        path = candidate;
        return true;
    }

    private static bool IsCanonicalPathComponent(string component) =>
        !string.IsNullOrEmpty(component) &&
        IsAsciiLetterOrDigit(component[0]) &&
        component.All(character =>
            IsAsciiLetterOrDigit(character) || character is '.' or '_' or '-' or '@');

    private static bool IsAsciiLetterOrDigit(char character) =>
        character is (>= 'a' and <= 'z') or
            (>= 'A' and <= 'Z') or
            (>= '0' and <= '9');

    private static LocalPresentationAssetPackResult ReparseFailure() =>
        Reject(
            LocalPresentationAssetPackFailureCode.AssetPathRejected,
            "runtimePath",
            "A local presentation asset path crosses a reparse or symbolic-link boundary.");

    private static bool TryGetPathStatus(
        string path,
        out bool exists,
        out bool isReparsePoint)
    {
        exists = false;
        isReparsePoint = false;
        try
        {
            exists = true;
            isReparsePoint =
                (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
            return true;
        }
        catch (Exception error) when (
            error is FileNotFoundException or DirectoryNotFoundException)
        {
            if (!TryGetLinkTargetStatus(path, out isReparsePoint))
            {
                return false;
            }

            exists = isReparsePoint;
            return true;
        }
        catch (Exception error) when (
            error is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return false;
        }
    }

    private static bool TryGetLinkTargetStatus(string path, out bool hasLinkTarget)
    {
        hasLinkTarget = false;
        try
        {
            hasLinkTarget = new FileInfo(path).LinkTarget is not null ||
                new DirectoryInfo(path).LinkTarget is not null;
            return true;
        }
        catch (Exception error) when (
            error is FileNotFoundException or DirectoryNotFoundException)
        {
            return true;
        }
        catch (Exception error) when (
            error is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return false;
        }
    }

    private static bool TryReadManifestBytes(
        string path,
        out byte[]? bytes,
        out LocalPresentationAssetPackResult? failure)
    {
        bytes = null;
        failure = null;
        try
        {
            using FileStream stream = new(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.SequentialScan);
            if (stream.Length > LocalPresentationAssetPackAdmission.MaximumManifestBytes)
            {
                failure = Reject(
                    LocalPresentationAssetPackFailureCode.InvalidManifest,
                    "manifest",
                    "The local presentation manifest exceeds the conservative v1 byte bound.");
                return false;
            }

            bytes = new byte[checked((int)stream.Length)];
            stream.ReadExactly(bytes);
            return true;
        }
        catch (Exception error) when (
            error is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            failure = Reject(
                LocalPresentationAssetPackFailureCode.PackageUnavailable,
                "manifest",
                $"A required local presentation input is unavailable: {error.GetType().Name}.");
            return false;
        }
    }

    private static bool TryVerifyPayload(
        string path,
        long expectedLength,
        string expectedDigest,
        string field,
        out LocalPresentationAssetPackResult? failure)
    {
        failure = null;
        try
        {
            using FileStream stream = new(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                FileOptions.SequentialScan);
            if (stream.Length > LocalPresentationAssetPackAdmission.MaximumRasterPayloadBytes ||
                stream.Length != expectedLength)
            {
                failure = Reject(
                    LocalPresentationAssetPackFailureCode.PayloadMismatch,
                    field,
                    "A local presentation runtime payload length drifted.");
                return false;
            }

            string actualDigest = Convert.ToHexString(SHA256.HashData(stream));
            if (!string.Equals(actualDigest, expectedDigest, StringComparison.Ordinal))
            {
                failure = Reject(
                    LocalPresentationAssetPackFailureCode.PayloadMismatch,
                    field,
                    "A local presentation runtime payload digest drifted.");
                return false;
            }

            return true;
        }
        catch (Exception error) when (
            error is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            failure = Reject(
                LocalPresentationAssetPackFailureCode.PackageUnavailable,
                field,
                $"A required local presentation input is unavailable: {error.GetType().Name}.");
            return false;
        }
    }

    private static string? ResolveFullyQualifiedPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
        {
            return null;
        }

        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception error) when (
            error is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }

    private static bool IsCanonicalCommit(string? value) =>
        value is not null &&
        value.Length == 40 &&
        value.All(character =>
            character is (>= '0' and <= '9') or (>= 'a' and <= 'f'));

    private static string Digest(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes));

    private static LocalPresentationAssetPackRejected Reject(
        LocalPresentationAssetPackFailureCode code,
        string field,
        string message) =>
        new(new LocalPresentationAssetPackDiagnostic(code, field, message));

    private static ManifestException Invalid(
        LocalPresentationAssetPackFailureCode code,
        string field,
        string message) =>
        new(code, field, message);

    private static void RequireExactProperties(
        JsonElement value,
        string field,
        params string[] expectedProperties)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                field,
                "A local presentation manifest object has the wrong JSON kind.");
        }

        HashSet<string> expected = new(expectedProperties, StringComparer.Ordinal);
        HashSet<string> actual = new(StringComparer.Ordinal);
        foreach (JsonProperty property in value.EnumerateObject())
        {
            if (!actual.Add(property.Name) || !expected.Contains(property.Name))
            {
                throw Invalid(
                    LocalPresentationAssetPackFailureCode.InvalidManifest,
                    field,
                    "A local presentation manifest object has duplicate or unknown fields.");
            }
        }

        if (!actual.SetEquals(expected))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                field,
                "A local presentation manifest object is missing required fields.");
        }
    }

    private static JsonElement RequiredObject(JsonElement owner, string property, string field)
    {
        JsonElement value = owner.GetProperty(property);
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                field,
                "A required local presentation manifest object is invalid.");
        }

        return value;
    }

    private static JsonElement RequiredArray(JsonElement owner, string property, string field)
    {
        JsonElement value = owner.GetProperty(property);
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                field,
                "A required local presentation manifest array is invalid.");
        }

        return value;
    }

    private static string RequiredString(JsonElement owner, string property, string field)
    {
        JsonElement value = owner.GetProperty(property);
        if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                field,
                "A required local presentation manifest string is invalid.");
        }

        return value.GetString()!;
    }

    private static int RequiredInt(JsonElement owner, string property, string field)
    {
        JsonElement value = owner.GetProperty(property);
        if (!value.TryGetInt32(out int result))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                field,
                "A required local presentation manifest integer is invalid.");
        }

        return result;
    }

    private static int RequiredPositiveInt(JsonElement owner, string property, string field)
    {
        int result = RequiredInt(owner, property, field);
        if (result < 1)
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                field,
                "A required local presentation manifest integer must be positive.");
        }

        return result;
    }

    private static long RequiredPositiveLong(JsonElement owner, string property, string field)
    {
        JsonElement value = owner.GetProperty(property);
        if (!value.TryGetInt64(out long result) || result < 1)
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidBucket,
                field,
                "A local presentation payload length must be a positive integer.");
        }

        return result;
    }

    private static bool RequiredBoolean(JsonElement owner, string property, string field)
    {
        JsonElement value = owner.GetProperty(property);
        if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidBucket,
                field,
                "A local presentation import-policy boolean is invalid.");
        }

        return value.GetBoolean();
    }

    private static string RequireSha256(JsonElement owner, string property, string field)
    {
        string value = RequiredString(owner, property, field);
        if (value.Length != 64 || value.Any(character => !Uri.IsHexDigit(character)))
        {
            throw Invalid(
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                field,
                "A local presentation manifest SHA-256 identity is invalid.");
        }

        return value.ToUpperInvariant();
    }

    private sealed record ParsedManifest(
        int SchemaVersion,
        string PackageId,
        string RepositoryId,
        int LogicalWidth,
        int LogicalHeight,
        IReadOnlyList<ParsedAsset> Assets);

    private sealed record ParsedAsset(
        string AssetId,
        int LogicalWidth,
        int LogicalHeight,
        IReadOnlyList<ParsedBucket> Buckets);

    private sealed record ParsedBucket(
        int Scale,
        string RuntimePath,
        int Width,
        int Height,
        long ByteLength,
        string Sha256,
        string MediaType,
        string Filter,
        bool Mipmaps,
        bool Repeat,
        string ColorSpace,
        string AlphaMode);

    private sealed class ManifestException(
        LocalPresentationAssetPackFailureCode code,
        string field,
        string message) : Exception(message)
    {
        public LocalPresentationAssetPackFailureCode Code { get; } = code;

        public string Field { get; } = field;
    }
}
