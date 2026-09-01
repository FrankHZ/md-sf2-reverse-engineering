using System.Security.Cryptography;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Content;
using Xunit;

namespace Sf2.Remake.Content.Tests;

public sealed class LocalPresentationAssetPackReaderTests
{
    private const string Commit = "0123456789abcdef0123456789abcdef01234567";

    [Fact]
    public void ExactProjectAuthoredPackageAdmitsImmutableSafeMetadataAndReceipt()
    {
        using TemporaryAssetPack package = new();

        LocalPresentationAssetPackAccepted accepted = Assert.IsType<LocalPresentationAssetPackAccepted>(
            package.Reader.Admit(package.Request()));

        Assert.Equal("ui.action-confirm", accepted.Definition.Assets[0].AssetId);
        Assert.Equal(960, accepted.Definition.LogicalPresentation.Width);
        Assert.Equal(new[] { 2, 4 }, accepted.Definition.Assets[0].Buckets.Select(item => item.Scale));
        Assert.Equal(new[] { 16, 32 }, accepted.Definition.Assets[0].Buckets.Select(item => item.Width));
        Assert.Equal("image/png", accepted.Definition.Assets[0].Buckets[0].MediaType);
        Assert.Equal(Commit, accepted.Receipt.MountedAssetRepositoryCommit);
        Assert.Equal(package.ManifestDigest, accepted.Receipt.ManifestDigest);
        Assert.Equal(1, accepted.Receipt.AssetCount);
        Assert.Equal(2, accepted.Receipt.BucketCount);
        Assert.DoesNotContain(
            accepted.Receipt.GetType().GetProperties(),
            property => property.Name.Contains("Path", StringComparison.Ordinal) ||
                property.Name.Contains("Payload", StringComparison.Ordinal));
    }

    [Fact]
    public void RequestIdentityAndMountedCommitRejectBeforeFilesystemRead()
    {
        LocalPresentationAssetPackReader missing = new(
            Path.Combine(Path.GetTempPath(), "sf2-assets-missing-" + Guid.NewGuid().ToString("N")),
            Commit);
        AssertCode(
            missing.Admit(Request("other-package", ContentProfile.PrivateLocal, Commit)),
            LocalPresentationAssetPackFailureCode.PackageIdentityMismatch);
        AssertCode(
            missing.Admit(Request(
                LocalPresentationAssetPackAdmission.PackageId,
                ContentProfile.PublicSynthetic,
                Commit)),
            LocalPresentationAssetPackFailureCode.ProfileMismatch);
        AssertCode(
            missing.Admit(new LocalPresentationAssetPackRequest(
                LocalPresentationAssetPackAdmission.PackageId,
                ContentProfile.PrivateLocal,
                "other-repository",
                Commit,
                Digest("manifest"u8.ToArray()))),
            LocalPresentationAssetPackFailureCode.RepositoryIdentityMismatch);
        AssertCode(
            new LocalPresentationAssetPackReader("relative/assets", Commit).Admit(
                Request(
                    LocalPresentationAssetPackAdmission.PackageId,
                    ContentProfile.PrivateLocal,
                    Commit)),
            LocalPresentationAssetPackFailureCode.PackageUnavailable);
        AssertCode(
            new LocalPresentationAssetPackReader("relative/assets", new string('a', 40)).Admit(
                Request(
                    LocalPresentationAssetPackAdmission.PackageId,
                    ContentProfile.PrivateLocal,
                    Commit)),
            LocalPresentationAssetPackFailureCode.RepositoryIdentityMismatch);
        AssertCode(
            new LocalPresentationAssetPackReader("relative/assets", Commit.ToUpperInvariant()).Admit(
                Request(
                    LocalPresentationAssetPackAdmission.PackageId,
                    ContentProfile.PrivateLocal,
                    Commit)),
            LocalPresentationAssetPackFailureCode.RepositoryIdentityMismatch);
    }

    [Fact]
    public void ManifestDigestRejectsBeforeMalformedJsonParsing()
    {
        using TemporaryAssetPack package = new();
        package.WriteManifestBytes("not-json"u8.ToArray());

        AssertCode(
            package.Reader.Admit(package.Request(Digest("different"u8.ToArray()))),
            LocalPresentationAssetPackFailureCode.ContentDigestMismatch);
        AssertCode(
            package.Reader.Admit(package.Request()),
            LocalPresentationAssetPackFailureCode.InvalidManifest);
    }

    [Theory]
    [InlineData("unknown-root")]
    [InlineData("missing-root")]
    [InlineData("wrong-capability")]
    [InlineData("wrong-repository")]
    [InlineData("wrong-profile")]
    public void ClosedManifestIdentityAndCapabilityDriftFailClosed(string mutation)
    {
        using TemporaryAssetPack package = new();
        JsonObject root = package.Manifest;
        switch (mutation)
        {
            case "unknown-root":
                root["unknown"] = true;
                break;
            case "missing-root":
                root.Remove("logicalPresentation");
                break;
            case "wrong-capability":
                root["capabilities"] = new JsonArray("other-capability");
                break;
            case "wrong-repository":
                root["repositoryId"] = "other-repository";
                break;
            case "wrong-profile":
                root["profile"] = "public-synthetic";
                break;
        }

        package.WriteManifest();
        LocalPresentationAssetPackRejected rejected = Assert.IsType<LocalPresentationAssetPackRejected>(
            package.Reader.Admit(package.Request()));

        Assert.Contains(
            rejected.Diagnostic.Code,
            new[]
            {
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                LocalPresentationAssetPackFailureCode.UnsupportedCapability,
                LocalPresentationAssetPackFailureCode.RepositoryIdentityMismatch,
                LocalPresentationAssetPackFailureCode.ProfileMismatch,
            });
    }

    [Theory]
    [InlineData("asset", "../ui.action-confirm")]
    [InlineData("asset", "ui/action-confirm")]
    [InlineData("asset", "UI.action-confirm")]
    [InlineData("source", "source\\ui.action-confirm")]
    [InlineData("source", "C:source.ui.action-confirm")]
    [InlineData("source", "source.ui action-confirm")]
    public void ManifestSemanticIdsUseTheApplicationCanonicalGrammar(
        string target,
        string identity)
    {
        using TemporaryAssetPack package = new();
        if (target == "asset")
        {
            package.Asset["assetId"] = identity;
        }
        else
        {
            package.Asset["source"]!["assetId"] = identity;
        }

        package.WriteManifest();
        AssertCode(
            package.Reader.Admit(package.Request()),
            LocalPresentationAssetPackFailureCode.InvalidManifest);
    }

    [Fact]
    public void DuplicateAssetAndRuntimePathIdentitiesReject()
    {
        using TemporaryAssetPack package = new();
        JsonObject duplicateAsset = (JsonObject)package.Asset.DeepClone();
        duplicateAsset["buckets"]![0]!["runtimePath"] = "runtime/ui/action-confirm-copy@2x.png";
        duplicateAsset["buckets"]![1]!["runtimePath"] = "runtime/ui/action-confirm-copy@4x.png";
        package.WritePayload("runtime/ui/action-confirm-copy@2x.png", "copy-2x"u8.ToArray());
        package.WritePayload("runtime/ui/action-confirm-copy@4x.png", "copy-4x"u8.ToArray());
        ReplaceBucketIdentity(duplicateAsset, 0, "copy-2x"u8.ToArray());
        ReplaceBucketIdentity(duplicateAsset, 1, "copy-4x"u8.ToArray());
        package.Assets.Add(duplicateAsset);
        package.WriteManifest();

        AssertCode(
            package.Reader.Admit(package.Request()),
            LocalPresentationAssetPackFailureCode.DuplicateIdentity);

        duplicateAsset["assetId"] = "ui.action-cancel";
        duplicateAsset["buckets"]![0]!["runtimePath"] =
            package.Asset["buckets"]![0]!["runtimePath"]!.GetValue<string>();
        package.WriteManifest();
        AssertCode(
            package.Reader.Admit(package.Request()),
            LocalPresentationAssetPackFailureCode.DuplicateIdentity);
    }

    [Theory]
    [InlineData("reorder")]
    [InlineData("dimension")]
    [InlineData("media")]
    [InlineData("missing")]
    [InlineData("unknown")]
    [InlineData("filter")]
    [InlineData("color")]
    [InlineData("alpha")]
    [InlineData("oversized-payload")]
    public void BucketOrderShapeDimensionsAndMediaTypeReject(string mutation)
    {
        using TemporaryAssetPack package = new();
        JsonArray buckets = package.Buckets;
        switch (mutation)
        {
            case "reorder":
                JsonNode first = buckets[0]!.DeepClone();
                buckets[0] = buckets[1]!.DeepClone();
                buckets[1] = first;
                break;
            case "dimension":
                buckets[0]!["width"] = 15;
                break;
            case "media":
                buckets[0]!["mediaType"] = "image/jpeg";
                break;
            case "missing":
                buckets.RemoveAt(1);
                break;
            case "unknown":
                buckets[0]!["unknown"] = 1;
                break;
            case "filter":
                buckets[0]!["filter"] = "bilinear";
                break;
            case "color":
                buckets[0]!["colorSpace"] = "linear";
                break;
            case "alpha":
                buckets[0]!["alphaMode"] = "premultiplied";
                break;
            case "oversized-payload":
                buckets[0]!["byteLength"] =
                    LocalPresentationAssetPackAdmission.MaximumRasterPayloadBytes + 1;
                break;
        }

        package.WriteManifest();
        LocalPresentationAssetPackRejected rejected = Assert.IsType<LocalPresentationAssetPackRejected>(
            package.Reader.Admit(package.Request()));
        Assert.Contains(
            rejected.Diagnostic.Code,
            new[]
            {
                LocalPresentationAssetPackFailureCode.InvalidManifest,
                LocalPresentationAssetPackFailureCode.InvalidBucket,
                LocalPresentationAssetPackFailureCode.MissingBucket,
            });
    }

    [Theory]
    [InlineData("../outside.png")]
    [InlineData("runtime/../outside.png")]
    [InlineData("runtime\\outside.png")]
    [InlineData("C:/outside.png")]
    public void RootedTraversalAndBackslashPathsRejectWithoutLeakingPath(string runtimePath)
    {
        using TemporaryAssetPack package = new();
        package.Buckets[0]!["runtimePath"] = runtimePath;
        package.WriteManifest();

        LocalPresentationAssetPackRejected rejected = Assert.IsType<LocalPresentationAssetPackRejected>(
            package.Reader.Admit(package.Request()));

        Assert.Equal(LocalPresentationAssetPackFailureCode.AssetPathRejected, rejected.Diagnostic.Code);
        Assert.DoesNotContain(package.Root, rejected.Diagnostic.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(runtimePath, rejected.Diagnostic.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PayloadLengthAndDigestDriftReject()
    {
        using TemporaryAssetPack package = new();
        package.WritePayload("runtime/ui/action-confirm@2x.png", "mutated"u8.ToArray());

        AssertCode(
            package.Reader.Admit(package.Request()),
            LocalPresentationAssetPackFailureCode.PayloadMismatch);

        byte[] original = TemporaryAssetPack.TwoXBytes;
        package.WritePayload("runtime/ui/action-confirm@2x.png", original);
        package.Buckets[0]!["sha256"] = new string('A', 64);
        package.WriteManifest();
        AssertCode(
            package.Reader.Admit(package.Request()),
            LocalPresentationAssetPackFailureCode.PayloadMismatch);
    }

    [Fact]
    public void ManifestByteBoundRejectsBeforeAllocationOrDigestAdmission()
    {
        using TemporaryAssetPack package = new();
        package.SetManifestLength(
            LocalPresentationAssetPackAdmission.MaximumManifestBytes + 1L);

        AssertCode(
            package.Reader.Admit(package.Request(new string('A', 64))),
            LocalPresentationAssetPackFailureCode.InvalidManifest);
    }

    [Fact]
    public void ReparsePointAtAnExistingComponentIsRejected()
    {
        using TemporaryAssetPack package = new(createRuntimePayloads: false);
        string outside = Path.Combine(Path.GetTempPath(), "sf2-assets-outside-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(outside, "ui"));
        string link = Path.Combine(package.Root, "runtime");
        try
        {
            File.WriteAllBytes(Path.Combine(outside, "ui", "action-confirm@2x.png"), TemporaryAssetPack.TwoXBytes);
            File.WriteAllBytes(Path.Combine(outside, "ui", "action-confirm@4x.png"), TemporaryAssetPack.FourXBytes);
            CreateDirectoryLink(link, outside);

            LocalPresentationAssetPackRejected rejected =
                Assert.IsType<LocalPresentationAssetPackRejected>(package.Reader.Admit(package.Request()));

            Assert.Equal(LocalPresentationAssetPackFailureCode.AssetPathRejected, rejected.Diagnostic.Code);
            Assert.DoesNotContain(outside, rejected.Diagnostic.Message, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(package.Root, rejected.Diagnostic.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(link))
            {
                Directory.Delete(link);
            }

            Directory.Delete(outside, recursive: true);
        }
    }

    [Fact]
    public void DanglingReparsePointIsRejectedRatherThanTreatedAsAMissingPackage()
    {
        using TemporaryAssetPack package = new(createRuntimePayloads: false);
        string missingTarget = Path.Combine(
            Path.GetTempPath(),
            "sf2-assets-missing-target-" + Guid.NewGuid().ToString("N"));
        string link = Path.Combine(package.Root, "runtime");
        try
        {
            CreateDirectoryLink(link, missingTarget);

            AssertCode(
                package.Reader.Admit(package.Request()),
                LocalPresentationAssetPackFailureCode.AssetPathRejected);
        }
        finally
        {
            try
            {
                Directory.Delete(link);
            }
            catch (DirectoryNotFoundException)
            {
            }
        }
    }

    [Fact]
    public void PublicSurfaceHasOnlyRootAndMountedCommitConstructorAndNoByteFactory()
    {
        Type readerType = typeof(LocalPresentationAssetPackReader);
        System.Reflection.ConstructorInfo constructor = Assert.Single(readerType.GetConstructors());

        Assert.Equal(
            new[] { typeof(string), typeof(string) },
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.DoesNotContain(
            readerType.GetMethods(),
            method => method.DeclaringType == readerType && method.IsStatic && method.IsPublic);
    }

    private static LocalPresentationAssetPackRequest Request(
        string packageId,
        ContentProfile profile,
        string commit) =>
        new(
            packageId,
            profile,
            LocalPresentationAssetPackAdmission.RepositoryId,
            commit,
            Digest("manifest"u8.ToArray()));

    private static void AssertCode(
        LocalPresentationAssetPackResult result,
        LocalPresentationAssetPackFailureCode code) =>
        Assert.Equal(code, Assert.IsType<LocalPresentationAssetPackRejected>(result).Diagnostic.Code);

    private static void ReplaceBucketIdentity(JsonObject asset, int index, byte[] bytes)
    {
        asset["buckets"]![index]!["byteLength"] = bytes.Length;
        asset["buckets"]![index]!["sha256"] = Digest(bytes);
    }

    private static string Digest(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes));

    private static void CreateDirectoryLink(string link, string target)
    {
        if (!OperatingSystem.IsWindows())
        {
            Directory.CreateSymbolicLink(link, target);
            return;
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = Environment.GetEnvironmentVariable("COMSPEC") ?? "cmd.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.ArgumentList.Add("/d");
        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add("mklink");
        startInfo.ArgumentList.Add("/J");
        startInfo.ArgumentList.Add(link);
        startInfo.ArgumentList.Add(target);
        using Process process = Process.Start(startInfo) ??
            throw new InvalidOperationException("The test junction helper did not start.");
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                "The test junction helper failed: " + process.StandardError.ReadToEnd());
        }
    }

    private sealed class TemporaryAssetPack : IDisposable
    {
        public static readonly byte[] TwoXBytes = "project-authored-png-2x"u8.ToArray();
        public static readonly byte[] FourXBytes = "project-authored-png-4x"u8.ToArray();

        public TemporaryAssetPack(bool createRuntimePayloads = true)
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                "sf2-local-presentation-pack-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(Root, "manifests"));
            if (createRuntimePayloads)
            {
                WritePayload("runtime/ui/action-confirm@2x.png", TwoXBytes);
                WritePayload("runtime/ui/action-confirm@4x.png", FourXBytes);
            }

            Manifest = CreateManifest();
            WriteManifest();
            Reader = new LocalPresentationAssetPackReader(Root, Commit);
        }

        public string Root { get; }

        public JsonObject Manifest { get; }

        public JsonArray Assets => Manifest["assets"]!.AsArray();

        public JsonObject Asset => Assets[0]!.AsObject();

        public JsonArray Buckets => Asset["buckets"]!.AsArray();

        public LocalPresentationAssetPackReader Reader { get; }

        public string ManifestDigest => Digest(File.ReadAllBytes(ManifestPath));

        private string ManifestPath => Path.Combine(
            Root,
            "manifests",
            "presentation-assets-v1.json");

        public LocalPresentationAssetPackRequest Request(string? manifestDigest = null) =>
            new(
                LocalPresentationAssetPackAdmission.PackageId,
                ContentProfile.PrivateLocal,
                LocalPresentationAssetPackAdmission.RepositoryId,
                Commit,
                manifestDigest ?? ManifestDigest);

        public void WriteManifest()
        {
            WriteManifestBytes(Encoding.UTF8.GetBytes(Manifest.ToJsonString(
                new JsonSerializerOptions { WriteIndented = true })));
        }

        public void WriteManifestBytes(byte[] bytes) =>
            File.WriteAllBytes(ManifestPath, bytes);

        public void SetManifestLength(long length)
        {
            using FileStream stream = new(ManifestPath, FileMode.Open, FileAccess.Write, FileShare.None);
            stream.SetLength(length);
        }

        public void WritePayload(string relativePath, byte[] bytes)
        {
            string path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);

        private static JsonObject CreateManifest() =>
            new()
            {
                ["schemaVersion"] = 1,
                ["packageId"] = LocalPresentationAssetPackAdmission.PackageId,
                ["repositoryId"] = LocalPresentationAssetPackAdmission.RepositoryId,
                ["profile"] = "private-local",
                ["capabilities"] = new JsonArray(LocalPresentationAssetPackAdmission.Capability),
                ["logicalPresentation"] = new JsonObject
                {
                    ["width"] = 960,
                    ["height"] = 540,
                },
                ["assets"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["assetId"] = "ui.action-confirm",
                        ["kind"] = "raster-image",
                        ["logicalSize"] = new JsonObject
                        {
                            ["width"] = 8,
                            ["height"] = 6,
                        },
                        ["source"] = new JsonObject
                        {
                            ["assetId"] = "source.ui.action-confirm",
                            ["sha256"] = Digest("project-authored-source"u8.ToArray()),
                        },
                        ["derivation"] = new JsonObject
                        {
                            ["policyId"] = "project-authored-nearest-v1",
                            ["generatorId"] = "project-authored-test-generator",
                            ["generatorVersion"] = "1",
                            ["generatorArtifactSha256"] = Digest("generator"u8.ToArray()),
                        },
                        ["buckets"] = new JsonArray
                        {
                            Bucket(
                                2,
                                "runtime/ui/action-confirm@2x.png",
                                16,
                                12,
                                TwoXBytes),
                            Bucket(
                                4,
                                "runtime/ui/action-confirm@4x.png",
                                32,
                                24,
                                FourXBytes),
                        },
                    },
                },
            };

        private static JsonObject Bucket(
            int scale,
            string runtimePath,
            int width,
            int height,
            byte[] bytes) =>
            new()
            {
                ["scale"] = scale,
                ["runtimePath"] = runtimePath,
                ["width"] = width,
                ["height"] = height,
                ["byteLength"] = bytes.Length,
                ["sha256"] = Digest(bytes),
                ["mediaType"] = "image/png",
                ["filter"] = "nearest",
                ["mipmaps"] = false,
                ["repeat"] = false,
                ["colorSpace"] = "srgb",
                ["alphaMode"] = "straight",
            };
    }
}
