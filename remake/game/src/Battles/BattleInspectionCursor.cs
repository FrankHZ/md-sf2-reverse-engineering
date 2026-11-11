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
        DrawRect(new Rect2(4, 4, 88, 88), Colors.Black, false, 7);
        DrawRect(new Rect2(4, 4, 88, 88), Colors.Cyan, false, 3);
        DrawLine(new(40, 48), new(56, 48), Colors.White, 2);
        DrawLine(new(48, 40), new(48, 56), Colors.White, 2);
    }
    internal object Observe() => new { visible = IsVisibleInTree(), resource = Original ? Resource : null,
        mode = Original ? "original-single-cell" : "authored-fallback", sha256 = _sha256,
        width = _texture?.GetWidth(), height = _texture?.GetHeight(), filter = TextureFilter.ToString(),
        sourceScale = Original ? 4 : 0, draws = _draws, z = ZIndex,
        rect = Rectangle(GetGlobalRect()), drawn = Rectangle(GetGlobalTransformWithCanvas() * ArtRect) };
    private static object Rectangle(Rect2 rect) => new { x = rect.Position.X, y = rect.Position.Y,
        width = rect.Size.X, height = rect.Size.Y };
}
