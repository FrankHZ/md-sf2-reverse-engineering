using Godot;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private bool _scaleReview, _titleLargeText;

    private void StartScaleReview(string[] args)
    {
        _scaleReview = args.Contains("--scale-review");
        if (!_scaleReview) return;
        _settings.LargeText = args.Contains("--larger");
        GetNode<Control>("PreviewToolbar").Hide();
        GetWindow().SizeChanged += FitReviewPicture;
        FitReviewPicture();
        ShowReviewTitle();
    }

    private void FitReviewPicture()
    {
        var window = GetWindow();
        var mode = window.Size.X < 1920 || window.Size.Y < 1080
            ? Window.ContentScaleModeEnum.CanvasItems : Window.ContentScaleModeEnum.Disabled;
        if (window.ContentScaleMode != mode) window.ContentScaleMode = mode;
        _game.Size = (Vector2I)GetViewportRect().Size;
        var picture = GetNode<TextureRect>("ProductionPicture");
        picture.Position = Vector2.Zero;
        picture.Size = _game.Size;
    }

    private void ShowReviewTitle()
    {
        _titleLargeText = _settings.LargeText;
        GetWindow().Title =
            $"SF2 · {(_titleLargeText ? "大字 / Larger" : "标准 / Standard")} · U 对比 / Compare · F2 Dialogue · F4 Battle · F5 Members · F10 System · L 中文/English";
    }

    public override void _ExitTree()
    {
        if (_scaleReview) GetWindow().SizeChanged -= FitReviewPicture;
    }
}
