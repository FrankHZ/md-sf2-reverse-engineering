using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Exploration;
namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleInspectionCursor : Control
{
    internal const string Resource = "battle-cursor.single-cell";
    private (ImageTexture? Texture, string? Sha256) _original, _variant;
    private bool _hd;
    private int _draws;
    private bool UseHd => _hd && _variant.Texture is not null;
    private bool Mounted => _original.Texture is not null;
    private (ImageTexture? Texture, string? Sha256) Art => UseHd ? _variant : _original;
    private Rect2 ArtRect => Mounted ? new(-16, -16, 128, 128) : new(4, 4, 88, 88);
    internal void Configure(BattleSceneDefinition? scene)
    {
        if (scene is null || !scene.Rasters.TryGetValue(Resource, out var raster)) return;
        var original = Load(raster, 32);
        try
        {
            if (scene.HdRasters?.TryGetValue(Resource, out var variant) == true)
                _variant = Load(variant, 128);
        }
        catch
        {
            original.Item1.Dispose();
            throw;
        }
        _original = original;
    }

    private static (ImageTexture, string) Load(ExplorationRaster raster, int edge)
    {
        if (raster.Width != edge || raster.Height != edge)
            throw new InvalidOperationException("battle-cursor-resource-invalid");
        using var image = ExplorationRasterImages.Open(raster, "battle-cursor-resource-invalid");
        return (ImageTexture.CreateFromImage(image), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(image.GetData())));
    }

    internal void SelectGraphics(bool hd) { _hd = hd; QueueRedraw(); }
    public override void _Ready()
    { MouseFilter = MouseFilterEnum.Ignore; TextureFilter = TextureFilterEnum.Nearest; QueueRedraw(); }
    public override void _Notification(int what)
    { if (what == NotificationPredelete) { _original.Texture?.Dispose(); _variant.Texture?.Dispose(); } }
    public override void _Draw()
    {
        _draws++;
        if (Art.Texture is { } texture) { DrawTextureRect(texture, ArtRect, false); return; }
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
    internal object Observe() => new { visible = IsVisibleInTree(), resource = Mounted ? Resource : null,
        mode = UseHd ? "hd-single-cell" : Mounted ? "original-single-cell" : "authored-fallback", sha256 = Art.Sha256,
        width = Art.Texture?.GetWidth(), height = Art.Texture?.GetHeight(), texture = Art.Texture?.GetInstanceId(),
        filter = TextureFilter.ToString(), sourceScale = Mounted ? 4 : 0, draws = _draws, z = ZIndex,
        rect = Rectangle(GetGlobalRect()), drawn = Rectangle(GetGlobalTransformWithCanvas() * ArtRect) };
    private static object Rectangle(Rect2 rect) => new { x = rect.Position.X, y = rect.Position.Y,
        width = rect.Size.X, height = rect.Size.Y };
}
