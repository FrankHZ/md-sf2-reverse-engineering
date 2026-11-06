using Godot;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.UI;

internal static class DiamondMenuGeometry
{
    internal const float Cell = 96, Step = 64;
    internal static readonly Vector2 Size = new(Cell * 3, Cell + Step * 2);
    internal static void Arrange(Control diamond, IReadOnlyList<DiamondCommandButton> commands)
    {
        diamond.Size = Size;
        Vector2[] locations = [new(Cell, 0), new(0, Step), new(Cell * 2, Step), new(Cell, Step * 2)];
        for (int i = 0; i < commands.Count; i++)
        {
            commands[i].Position = locations[i]; commands[i].Size = new(Cell, Cell);
            commands[i].Arrange();
        }
    }
}

// Source pixels are supplied through an admitted raster collection.
// An unmounted public scene shows text-only squares; it never substitutes authored glyphs.
internal sealed partial class DiamondCommandButton : Control
{
    internal string Identity { get; }
    private readonly string _captionKey, _resourcePrefix;
    internal string? CaptionOverride { get; set; }
    internal string Caption => GameText.Get(CaptionOverride ?? _captionKey);
    internal void RefreshCaption() { if (_label.Text != Caption) _label.Text = Caption; _label.Visible = _normal is null || CaptionOverride is not null; QueueRedraw(); }
    internal bool Selected { get; private set; }
    internal bool Available { get; set; }
    private readonly Label _label;
    private ImageTexture? _normal, _selected;
    private int _pixelScale = 1;
    private string? _drawnResource;
    private bool _drawnExtraFrame;
    private double _focusRemaining;
    private int _drawnOffsetY;
    private int FocusOffsetY => _focusRemaining > 0.12 ? -4 : _focusRemaining > 0.06 ? -2 : _focusRemaining > 0 ? -1 : 0;

    internal void SetSelection(bool selected, bool reducedFlash)
    {
        // Only a new focus starts the short modern emphasis; refresh/idle never restarts it.
        if (selected != Selected) _focusRemaining = selected && !reducedFlash ? 0.18 : 0;
        if (reducedFlash) _focusRemaining = 0;
        Selected = selected;
        QueueRedraw();
    }

    internal void AdvanceFocus(double delta)
    {
        if (_focusRemaining <= 0) return;
        _focusRemaining = Math.Max(0, _focusRemaining - delta);
        QueueRedraw();
    }

    internal void StopFocusEmphasis() { _focusRemaining = 0; QueueRedraw(); }
    internal DiamondCommandButton(string identity, string captionKey, string resourcePrefix)
    {
        Identity = identity; Name = identity; _captionKey = captionKey; _resourcePrefix = resourcePrefix;
        MouseFilter = MouseFilterEnum.Ignore;
        TextureFilter = TextureFilterEnum.Nearest;
        _label = new Label { HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        _label.AddThemeFontSizeOverride("font_size", 28);
    }
    public override void _Ready() => AddChild(_label);
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _normal?.Dispose(); _selected?.Dispose(); }
        base.Dispose(disposing);
    }
    internal void SetArt(ImageTexture? normal, ImageTexture? selected)
    {
        _normal?.Dispose(); _selected?.Dispose();
        _normal = normal; _selected = selected; _drawnResource = null;
        _label.Visible = normal is null || CaptionOverride is not null; QueueRedraw();
    }
    internal void Arrange()
    {
        _pixelScale = Math.Max(1, (int)Mathf.Floor(Mathf.Min(Size.X, Size.Y) / 24));
        _label.Position = Vector2.Zero; _label.Size = Size; QueueRedraw();
    }
    internal object ObserveArt() => new { shape = "square", mounted = _normal is not null,
        resource = _drawnResource,
        width = _normal?.GetWidth(), height = _normal?.GetHeight(), scale = _pixelScale,
        captionRect = new { x = _label.GetGlobalRect().Position.X, y = _label.GetGlobalRect().Position.Y,
            width = _label.GetGlobalRect().Size.X, height = _label.GetGlobalRect().Size.Y },
        captionMinimumWidth = _label.GetMinimumSize().X, captionMinimumHeight = _label.GetMinimumSize().Y,
        filter = TextureFilter.ToString(), extraFrame = _drawnExtraFrame, labelVisible = _label.Visible,
        focusRemainingMs = _focusRemaining * 1000, drawnOffsetY = _drawnOffsetY };
    public override void _Draw()
    {
        _drawnOffsetY = FocusOffsetY;
        var offset = new Vector2(0, _drawnOffsetY);
        _label.Position = offset;
        var texture = CaptionOverride is not null ? null : Selected ? _selected : _normal;
        _drawnExtraFrame = texture is null;
        if (texture is not null)
        {
            float side = 24 * _pixelScale;
            DrawTextureRect(texture, new Rect2(((Size - new Vector2(side, side)) / 2).Round() + offset, new(side, side)), false);
            _drawnResource = $"{_resourcePrefix}.{Identity.ToLowerInvariant()}.{(Selected ? "selected" : "normal")}";
        }
        else
        {
            var rect = new Rect2(Vector2.One + offset, Size - Vector2.One * 2);
            DrawRect(rect, new Color("002492"));
            DrawRect(rect, Selected ? Colors.White : new Color("ffdb49"), false, 2);
        }
        _label.Modulate = Selected ? Colors.White : Available ? new Color("ffdb49") : new Color("a0a0a0");
    }
}
