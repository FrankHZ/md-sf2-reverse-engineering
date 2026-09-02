using Godot;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class Map3RootDisplayPolicyTests
{
    [Fact]
    public void RawProcessArgumentsRecognizeOnlyEngineDisplayRequestsBeforeUserArguments()
    {
        Assert.True(Map3Root.HasExplicitPhysicalStartupArgument(
            ["godot.exe", "--resolution", "960x540", "--", "--runtime-profile=private-local"]));
        Assert.True(Map3Root.HasExplicitPhysicalStartupArgument(
            ["godot.exe", "--resolution=1920x1080"]));
        Assert.True(Map3Root.HasExplicitPhysicalStartupArgument(
            ["godot.exe", "--fullscreen"]));
        Assert.True(Map3Root.HasExplicitPhysicalStartupArgument(
            ["godot.exe", "--maximized"]));
        Assert.False(Map3Root.HasExplicitPhysicalStartupArgument(
            ["godot.exe", "--", "--resolution=3840x2160"]));
        Assert.False(Map3Root.HasExplicitPhysicalStartupArgument(
            ["godot.exe", "--runtime-profile=private-local"]));
    }

    [Fact]
    public void ExplicitModeSizeOrRawResolutionPreservesThePhysicalTarget()
    {
        Assert.False(Map3Root.ShouldPreservePhysicalStartupTarget(
            Window.ModeEnum.Windowed,
            new Vector2I(960, 540),
            ["godot.exe"]));
        Assert.True(Map3Root.ShouldPreservePhysicalStartupTarget(
            Window.ModeEnum.Windowed,
            new Vector2I(960, 540),
            ["godot.exe", "--resolution=960x540"]));
        Assert.True(Map3Root.ShouldPreservePhysicalStartupTarget(
            Window.ModeEnum.Windowed,
            new Vector2I(1920, 1080),
            ["godot.exe"]));
        Assert.True(Map3Root.ShouldPreservePhysicalStartupTarget(
            Window.ModeEnum.Maximized,
            new Vector2I(960, 540),
            ["godot.exe"]));
        Assert.True(Map3Root.ShouldPreservePhysicalStartupTarget(
            Window.ModeEnum.Fullscreen,
            new Vector2I(960, 540),
            ["godot.exe"]));
    }

    [Fact]
    public void HeadlessUsesTheVirtualContentSizeInsteadOfTheAbsentDesktopClient()
    {
        Assert.Equal(
            new Vector2I(960, 540),
            Map3Root.SelectObservedClientSize(
                headless: true,
                virtualContentSize: new Vector2I(960, 540),
                displayServerSize: Vector2I.Zero));
        Assert.Equal(
            new Vector2I(1920, 1080),
            Map3Root.SelectObservedClientSize(
                headless: false,
                virtualContentSize: new Vector2I(960, 540),
                displayServerSize: new Vector2I(1920, 1080)));
    }

    [Theory]
    [InlineData(2560, 1400, 16, 39, 1920, 1080)]
    [InlineData(1936, 1119, 16, 39, 1920, 1080)]
    [InlineData(1935, 1119, 16, 39, 1600, 900)]
    [InlineData(1920, 1040, 16, 39, 1600, 900)]
    [InlineData(1366, 728, 16, 39, 960, 540)]
    public void AdaptiveLadderChoosesTheLargestDecoratedClientThatFits(
        int usableWidth,
        int usableHeight,
        int decorationWidth,
        int decorationHeight,
        int expectedWidth,
        int expectedHeight)
    {
        Vector2I? selected = Map3Root.SelectAdaptiveWindowedClientSize(
            new Vector2I(usableWidth, usableHeight),
            new Vector2I(decorationWidth, decorationHeight));

        Assert.Equal(new Vector2I(expectedWidth, expectedHeight), selected);
    }

    [Theory]
    [InlineData(975, 579, 16, 39)]
    [InlineData(976, 578, 16, 39)]
    [InlineData(0, 1080, 16, 39)]
    [InlineData(1920, 1080, -1, 39)]
    public void InvalidOrTooSmallUsableSurfaceHasNoAdaptiveTarget(
        int usableWidth,
        int usableHeight,
        int decorationWidth,
        int decorationHeight)
    {
        Assert.Null(Map3Root.SelectAdaptiveWindowedClientSize(
            new Vector2I(usableWidth, usableHeight),
            new Vector2I(decorationWidth, decorationHeight)));
    }

    [Fact]
    public void DecorationInsetsPreserveAsymmetricClientAndOuterGeometry()
    {
        (Vector2I clientInset, Vector2I oppositeInset) = Map3Root.DecorationInsets(
                new Vector2I(960, 540),
                new Vector2I(976, 580),
                new Vector2I(107, 131),
                new Vector2I(100, 100));

        Assert.Equal(new Vector2I(7, 31), clientInset);
        Assert.Equal(new Vector2I(9, 9), oppositeInset);
    }

    [Fact]
    public void InconsistentDecorationOriginsClampWithoutInventingNegativeInsets()
    {
        (Vector2I clientInset, Vector2I oppositeInset) = Map3Root.DecorationInsets(
            new Vector2I(960, 540),
            new Vector2I(900, 500),
            new Vector2I(90, 90),
            new Vector2I(100, 100));

        Assert.Equal(
            Vector2I.Zero,
            clientInset);
        Assert.Equal(Vector2I.Zero, oppositeInset);
    }

    [Fact]
    public void CenteringUsesTheWholeUsableRectIncludingNegativeDesktopOrigins()
    {
        Assert.Equal(
            new Vector2I(-1712, 161),
            Map3Root.CenterDecoratedWindow(
                new Rect2I(-1920, 0, 1920, 1040),
                new Vector2I(1504, 718)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Map3Root.CenterDecoratedWindow(
                new Rect2I(0, 0, 960, 540),
                new Vector2I(976, 579)));
    }

    [Fact]
    public void DecoratedFrameContainmentUsesVirtualDesktopCoordinates()
    {
        Rect2I usable = new(-1920, 0, 1920, 1040);

        Assert.True(Map3Root.IsDecoratedFrameInside(
            usable,
            new Vector2I(-1712, 161),
            new Vector2I(1504, 718)));
        Assert.False(Map3Root.IsDecoratedFrameInside(
            usable,
            new Vector2I(-1921, 161),
            new Vector2I(1504, 718)));
        Assert.False(Map3Root.IsDecoratedFrameInside(
            usable,
            new Vector2I(-1503, 161),
            new Vector2I(1504, 718)));
    }

    [Theory]
    [InlineData(960, 540, true)]
    [InlineData(1920, 1080, true)]
    [InlineData(3840, 2160, true)]
    [InlineData(959, 540, false)]
    [InlineData(960, 539, false)]
    public void ProductClientMinimumIsExact(
        int width,
        int height,
        bool expected)
    {
        Assert.Equal(
            expected,
            Map3Root.ValidatePrivateProductClientSize(
                new Vector2I(width, height),
                out string? diagnostic));
        Assert.Equal(expected, diagnostic is null);
    }

    [Fact]
    public void ProjectKeepsLogicalFallbackAndDeclaresHighDpiKeepAspectPolicy()
    {
        string project = File.ReadAllText(FindRepositoryPath("remake", "game", "project.godot"));

        Assert.Contains("window/size/viewport_width=960", project, StringComparison.Ordinal);
        Assert.Contains("window/size/viewport_height=540", project, StringComparison.Ordinal);
        Assert.Contains("window/size/window_width_override=960", project, StringComparison.Ordinal);
        Assert.Contains("window/size/window_height_override=540", project, StringComparison.Ordinal);
        Assert.Contains("window/size/resizable=true", project, StringComparison.Ordinal);
        Assert.Contains("window/dpi/allow_hidpi=true", project, StringComparison.Ordinal);
        Assert.Contains("window/stretch/mode=\"canvas_items\"", project, StringComparison.Ordinal);
        Assert.Contains("window/stretch/aspect=\"keep\"", project, StringComparison.Ordinal);
        Assert.DoesNotContain("min_width", project, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("min_height", project, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryPath(params string[] segments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("The repository root was not found.");
    }
}
