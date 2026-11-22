using Godot;
using Sf2.Remake.Application.Content.Scenarios;
namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleInspectionCursor : Control
{
    internal const string Resource = "battle-cursor.single-cell";
    private ImageTexture? _texture;
    private string? _sha256;
    private int _draws;
    internal bool Original => _texture is not null;
    private Rect2 ArtRect => Original ? new(-16, -16, 128, 128) : new(4, 4, 88, 88);
    internal void Configure(IReadOnlyDictionary<string, ExplorationRaster>? rasters)
    {
        if (rasters is null || !rasters.TryGetValue(Resource, out var raster)) return;
        using var image = raster.Format == "rgba8"
            ? Image.CreateFromData(raster.Width, raster.Height, false, Image.Format.Rgba8, raster.CopyBytes()) : new Image();
        if (raster.Format != "rgba8" && image.LoadPngFromBuffer(raster.CopyBytes()) != Error.Ok ||
            image.GetWidth() != 32 || image.GetHeight() != 32)
            throw new InvalidOperationException("battle-cursor-resource-invalid");
        _texture = ImageTexture.CreateFromImage(image);
        _sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(image.GetData()));
    }
    public override void _Ready()
    { MouseFilter = MouseFilterEnum.Ignore; TextureFilter = TextureFilterEnum.Nearest; QueueRedraw(); }
    public override void _Notification(int what)
    { if (what == NotificationPredelete) { _texture?.Dispose(); _texture = null; } }
    public override void _Draw()
    {
        _draws++;
        if (_texture is not null) { DrawTextureRect(_texture, ArtRect, false); return; }
        // Authored 4px steps match the board's native pixel grid.
        foreach (var corner in new[] { new Vector2(4, 4), new(92, 4), new(4, 92), new(92, 92) })
        {
            var sign = (new Vector2(48, 48) - corner).Sign();
            var bar = new Vector2(sign.X * 24, sign.Y * 8);
            var leg = new Vector2(sign.X * 8, sign.Y * 24);
            DrawRect(new Rect2(corner, bar).Abs(), new Color("18202b"));
            DrawRect(new Rect2(corner, leg).Abs(), new Color("18202b"));
            var inset = corner + sign * 4;
            DrawRect(new Rect2(inset, new Vector2(sign.X * 16, sign.Y * 4)).Abs(), new Color("e9f9fb"));
            DrawRect(new Rect2(inset, new Vector2(sign.X * 4, sign.Y * 16)).Abs(), new Color("e9f9fb"));
        }
    }
    internal object Observe() => new { visible = IsVisibleInTree(), resource = Original ? Resource : null,
        mode = Original ? "original-single-cell" : "authored-fallback", sha256 = _sha256,
        width = _texture?.GetWidth(), height = _texture?.GetHeight(), filter = TextureFilter.ToString(),
        sourceScale = Original ? 4 : 0, draws = _draws, z = ZIndex,
        rect = Rectangle(GetGlobalRect()), drawn = Rectangle(GetGlobalTransformWithCanvas() * ArtRect) };
    private static object Rectangle(Rect2 rect) => new { x = rect.Position.X, y = rect.Position.Y,
        width = rect.Size.X, height = rect.Size.Y };
}
