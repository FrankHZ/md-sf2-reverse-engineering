using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class Map3RuntimeProfileSelectionTests
{
    [Fact]
    public void EmptyArgumentsKeepThePublicSyntheticDefault()
    {
        Map3RuntimeProfileSelection selection = Map3RuntimeProfileSelection.Parse([]);

        Assert.True(selection.IsAvailable);
        Assert.Equal(Map3RuntimeProfile.PublicSynthetic, selection.RequestedProfile);
        Assert.Null(selection.CanonicalImportPath);
        Assert.False(selection.PrivateSmokeRequested);
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
}
