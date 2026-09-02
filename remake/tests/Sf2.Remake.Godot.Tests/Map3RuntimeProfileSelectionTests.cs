using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class Map3RuntimeProfileSelectionTests
{
    private const string AssetCommit = "d221cf3e89e90e647467415fc58edc38faad04f8";
    private const string ManifestDigest =
        "308C9E2617D0591E6B535F8BB6A9C6EC959A6B873F2495FB8B36CE7C99336897";

    [Fact]
    public void EmptyArgumentsKeepThePublicSyntheticDefault()
    {
        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse([]);

        Assert.True(selection.IsAvailable);
        Assert.Equal(Map3RuntimeProfile.PublicSynthetic, selection.RequestedProfile);
        Assert.Null(selection.CanonicalImportPath);
        Assert.False(selection.PrivateSmokeRequested);
        Assert.False(selection.PrivateBaseViewRequested);
        Assert.Null(selection.OriginalRomPath);
        Assert.Null(selection.TilesetMetadataPath);
        Assert.Null(selection.PaletteMetadataPath);
        Assert.False(selection.PrivateHudPreviewRequested);
        Assert.Null(selection.PresentationAssetRoot);
        Assert.Null(selection.PresentationAssetCommit);
        Assert.Null(selection.PresentationManifestDigest);
    }

    [Fact]
    public void ExplicitPublicProfileRemainsAvailableWithoutPrivateOptions()
    {
        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse(
            ["--runtime-profile=public-synthetic", "--map3-smoke"]);

        Assert.True(selection.IsAvailable);
        Assert.Equal(Map3RuntimeProfile.PublicSynthetic, selection.RequestedProfile);
    }

    [Fact]
    public void ExplicitPrivateProfileRequiresAndNormalizesOneAbsolutePath()
    {
        string path = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "sf2-private-map3",
            "canonical-map-import.json"));
        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse(
            [
                "--runtime-profile=private-local",
                $"--canonical-map-import={path}",
                Map3RuntimeProfileSelection.PrivateSmokeOption,
            ]);

        Assert.True(selection.IsAvailable);
        Assert.Equal(Map3RuntimeProfile.PrivateLocal, selection.RequestedProfile);
        Assert.Equal(path, selection.CanonicalImportPath);
        Assert.True(selection.PrivateSmokeRequested);
        Assert.False(selection.PrivateBaseViewRequested);
        Assert.Null(selection.Diagnostic);
    }

    [Fact]
    public void ExplicitPrivateHudPreviewRequiresAndNormalizesTheExactValueShape()
    {
        string canonical = Absolute("canonical-map-import.json");
        string assetRoot = Absolute("presentation-assets");
        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse(
            [
                "--runtime-profile=private-local",
                $"--canonical-map-import={canonical}",
                Map3RuntimeProfileSelection.PrivateHudPreviewOption,
                $"--presentation-asset-root={assetRoot}",
                $"--presentation-asset-commit={AssetCommit}",
                $"--presentation-manifest-sha256={ManifestDigest}",
            ]);

        Assert.True(selection.IsAvailable);
        Assert.Equal(Map3RuntimeProfile.PrivateLocal, selection.RequestedProfile);
        Assert.True(selection.PrivateHudPreviewRequested);
        Assert.Equal(assetRoot, selection.PresentationAssetRoot);
        Assert.Equal(AssetCommit, selection.PresentationAssetCommit);
        Assert.Equal(ManifestDigest, selection.PresentationManifestDigest);
        Assert.Null(selection.Diagnostic);
    }

    [Theory]
    [InlineData("flag-only")]
    [InlineData("values-without-flag")]
    [InlineData("relative-root")]
    [InlineData("uppercase-commit")]
    [InlineData("lowercase-digest")]
    [InlineData("duplicate-flag")]
    public void PartialAmbiguousOrNonCanonicalHudPreviewSelectionIsUnavailable(string mutation)
    {
        string canonical = Absolute("canonical-map-import.json");
        string assetRoot = Absolute("presentation-assets");
        List<string> arguments =
        [
            "--runtime-profile=private-local",
            $"--canonical-map-import={canonical}",
        ];
        if (mutation != "values-without-flag")
        {
            arguments.Add(Map3RuntimeProfileSelection.PrivateHudPreviewOption);
        }

        if (mutation != "flag-only")
        {
            arguments.Add($"--presentation-asset-root={(mutation == "relative-root" ? "relative-assets" : assetRoot)}");
            arguments.Add($"--presentation-asset-commit={(mutation == "uppercase-commit" ? AssetCommit.ToUpperInvariant() : AssetCommit)}");
            arguments.Add($"--presentation-manifest-sha256={(mutation == "lowercase-digest" ? ManifestDigest.ToLowerInvariant() : ManifestDigest)}");
        }

        if (mutation == "duplicate-flag")
        {
            arguments.Add(Map3RuntimeProfileSelection.PrivateHudPreviewOption);
        }

        Map3RuntimeProfileSelection selection =
            Map3RuntimeProfileSelection.Parse(arguments);

        Assert.False(selection.IsAvailable);
        Assert.Equal(Map3RuntimeProfile.PrivateLocal, selection.RequestedProfile);
        Assert.Null(selection.PresentationAssetRoot);
        Assert.Null(selection.PresentationAssetCommit);
        Assert.Null(selection.PresentationManifestDigest);
        Assert.DoesNotContain(assetRoot, selection.Diagnostic, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicProfileCannotConsumeACompletePrivateHudPreviewRequest()
    {
        string assetRoot = Absolute("presentation-assets");
        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse(
            [
                "--runtime-profile=public-synthetic",
                Map3RuntimeProfileSelection.PrivateHudPreviewOption,
                $"--presentation-asset-root={assetRoot}",
                $"--presentation-asset-commit={AssetCommit}",
                $"--presentation-manifest-sha256={ManifestDigest}",
            ]);

        Assert.False(selection.IsAvailable);
        Assert.Equal(Map3RuntimeProfile.PublicSynthetic, selection.RequestedProfile);
        Assert.Null(selection.PresentationAssetRoot);
        Assert.DoesNotContain(assetRoot, selection.Diagnostic, StringComparison.Ordinal);
    }

    [Fact]
    public void ExplicitPrivateBaseViewRequiresAndNormalizesAllIgnoredInputs()
    {
        string canonical = Absolute("canonical-map-import.json");
        string rom = Absolute("sf2.bin");
        string tilesets = Absolute("map-tilesets.json");
        string palettes = Absolute("map-palettes.json");

        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse(
            [
                "--runtime-profile=private-local",
                $"--canonical-map-import={canonical}",
                Map3RuntimeProfileSelection.PrivateBaseViewOption,
                $"--original-rom={rom}",
                $"--map-tileset-metadata={tilesets}",
                $"--map-palette-metadata={palettes}",
                Map3RuntimeProfileSelection.PrivateSmokeOption,
            ]);

        Assert.True(selection.IsAvailable);
        Assert.Equal(Map3RuntimeProfile.PrivateLocal, selection.RequestedProfile);
        Assert.Equal(canonical, selection.CanonicalImportPath);
        Assert.True(selection.PrivateBaseViewRequested);
        Assert.Equal(rom, selection.OriginalRomPath);
        Assert.Equal(tilesets, selection.TilesetMetadataPath);
        Assert.Equal(palettes, selection.PaletteMetadataPath);
        Assert.True(selection.PrivateSmokeRequested);
        Assert.Null(selection.Diagnostic);
    }

    [Theory]
    [InlineData("--runtime-profile=private-local")]
    [InlineData("--runtime-profile=private-local", "--canonical-map-import=")]
    [InlineData("--runtime-profile=private-local", "--canonical-map-import=relative.json")]
    public void MissingOrRelativePrivateInputIsUnavailable(params string[] arguments)
    {
        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse(arguments);

        Assert.False(selection.IsAvailable);
        Assert.Equal(Map3RuntimeProfile.PrivateLocal, selection.RequestedProfile);
        Assert.Null(selection.CanonicalImportPath);
    }

    [Fact]
    public void PrivateInputNeverInfersPrivateProfileFromPresence()
    {
        string path = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "canonical.json"));
        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse(
            [$"--canonical-map-import={path}"]);

        Assert.False(selection.IsAvailable);
        Assert.Null(selection.RequestedProfile);
        Assert.Null(selection.CanonicalImportPath);
        Assert.DoesNotContain(path, selection.Diagnostic, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("--private-map3-base-view")]
    [InlineData("--private-map3-base-view", "--original-rom=relative.bin")]
    [InlineData("--private-map3-base-view", "--original-rom=")]
    public void IncompleteOrRelativePrivateBaseViewIsUnavailable(
        params string[] visualArguments)
    {
        string canonical = Absolute("canonical-map-import.json");
        string[] arguments =
        [
            "--runtime-profile=private-local",
            $"--canonical-map-import={canonical}",
            .. visualArguments,
        ];

        Map3RuntimeProfileSelection selection =
            Map3RuntimeProfileSelection.Parse(arguments);

        Assert.False(selection.IsAvailable);
        Assert.Equal(Map3RuntimeProfile.PrivateLocal, selection.RequestedProfile);
        Assert.False(selection.PrivateBaseViewRequested);
        Assert.Null(selection.OriginalRomPath);
        Assert.DoesNotContain(canonical, selection.Diagnostic, StringComparison.Ordinal);
    }

    [Fact]
    public void VisualPathsWithoutExplicitBaseViewNeverEnableThePrivateRenderer()
    {
        string canonical = Absolute("canonical-map-import.json");
        string rom = Absolute("sf2.bin");
        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse(
            [
                "--runtime-profile=private-local",
                $"--canonical-map-import={canonical}",
                $"--original-rom={rom}",
            ]);

        Assert.False(selection.IsAvailable);
        Assert.False(selection.PrivateBaseViewRequested);
        Assert.Null(selection.OriginalRomPath);
        Assert.DoesNotContain(rom, selection.Diagnostic, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicProfileCannotSilentlyConsumePrivateOptions()
    {
        string path = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "canonical.json"));
        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse(
            [
                "--runtime-profile=public-synthetic",
                $"--canonical-map-import={path}",
            ]);

        Assert.False(selection.IsAvailable);
        Assert.Equal(Map3RuntimeProfile.PublicSynthetic, selection.RequestedProfile);
        Assert.Null(selection.CanonicalImportPath);
    }

    [Fact]
    public void UnknownOrDuplicateProfileOptionsAreUnavailable()
    {
        Map3RuntimeProfileSelection unknown = Map3RuntimeProfileSelection.Parse(
            ["--runtime-profile=unknown"]);
        Map3RuntimeProfileSelection duplicate = Map3RuntimeProfileSelection.Parse(
            [
                "--runtime-profile=private-local",
                "--runtime-profile=private-local",
            ]);

        Assert.False(unknown.IsAvailable);
        Assert.Null(unknown.RequestedProfile);
        Assert.False(duplicate.IsAvailable);
    }

    [Fact]
    public void DuplicateVisualOptionsAreUnavailableAndPathFree()
    {
        string canonical = Absolute("canonical-map-import.json");
        string rom = Absolute("sf2.bin");
        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse(
            [
                "--runtime-profile=private-local",
                $"--canonical-map-import={canonical}",
                Map3RuntimeProfileSelection.PrivateBaseViewOption,
                $"--original-rom={rom}",
                $"--original-rom={rom}",
            ]);

        Assert.False(selection.IsAvailable);
        Assert.Null(selection.OriginalRomPath);
        Assert.DoesNotContain(rom, selection.Diagnostic, StringComparison.Ordinal);
    }

    private static string Absolute(string fileName) =>
        Path.GetFullPath(Path.Combine(Path.GetTempPath(), "sf2-private-map3", fileName));
}
