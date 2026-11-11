using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.GodotAdapter.UI;

// Sole owner of decoded identity textures. Rows borrow textures and never dispose them.
internal sealed class IdentityIcons : IDisposable
{
    private readonly Dictionary<string, ImageTexture> _textures = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ImageTexture> _hd = new(StringComparer.Ordinal);
    private bool _required;
    internal string Mode { get; set; } = "original";
    internal bool Mounted => _required;
    internal string? Error(string? identity) => identity is not null && _required &&
        (!_textures.ContainsKey(identity + ".normal") || !_textures.ContainsKey(identity + ".selected")) ? "identity-icon-binding-required" : null;
    internal static string Item(int word) => $"identity-icon.item.{word & 127}";
    internal static string Unarmed => "identity-icon.other.128";
    internal static string? Spell(ScenarioDefinition definition, SpellRef spell) =>
        definition.PrivateDefinitions?.Spells.TryGetValue(spell, out var source) == true
            ? $"identity-icon.spell.{source.BaseId}" : null;
    internal void Configure(IReadOnlyDictionary<string, ExplorationRaster>? rasters,
        IReadOnlyDictionary<string, ExplorationRaster>? hd = null)
    {
        var prepared = new Dictionary<string, ImageTexture>(StringComparer.Ordinal);
        var preparedHd = new Dictionary<string, ImageTexture>(StringComparer.Ordinal);
        try
        {
            Decode(rasters, prepared, 16, 24); Decode(hd, preparedHd, 64, 96);
        }
        catch
        {
            foreach (var texture in prepared.Values.Concat(preparedHd.Values)) texture.Dispose();
            throw;
        }
        Dispose();
        foreach (var row in prepared) _textures.Add(row.Key, row.Value);
        foreach (var row in preparedHd) _hd.Add(row.Key, row.Value);
        _required = _textures.Count > 0;
    }
    private static void Decode(IReadOnlyDictionary<string, ExplorationRaster>? rasters,
        Dictionary<string, ImageTexture> output, int width, int height)
    {
        foreach (var (key, raster) in rasters ?? new Dictionary<string, ExplorationRaster>())
        {
            if (!key.StartsWith("identity-icon.", StringComparison.Ordinal)) continue;
            using var image = raster.Format == "rgba8"
                ? Image.CreateFromData(raster.Width, raster.Height, false, Image.Format.Rgba8, raster.CopyBytes()) : new Image();
            if (raster.Format != "rgba8" && image.LoadPngFromBuffer(raster.CopyBytes()) != Godot.Error.Ok ||
                image.GetWidth() != width || image.GetHeight() != height)
                throw new InvalidOperationException("identity-icon-invalid");
            output.Add(key, ImageTexture.CreateFromImage(image));
        }
    }
    internal string Variant(string? identity) => identity is null ? "text" :
        Mode == "hd" && _hd.ContainsKey(identity + ".normal") ? "hd" :
        _textures.ContainsKey(identity + ".normal") ? "original" : "text";
    internal ImageTexture? Get(string? identity, bool selected)
    {
        if (identity is null) return null;
        string key = identity + (selected ? ".selected" : ".normal");
        if (Mode == "hd" && _hd.TryGetValue(key, out var hd)) return hd;
        return _textures.GetValueOrDefault(key);
    }
    public void Dispose()
    {
        foreach (var texture in _textures.Values.Concat(_hd.Values)) texture.Dispose();
        _textures.Clear(); _hd.Clear(); _required = false;
    }
}

internal sealed partial class IdentityIcon : Control
{
    private readonly int _pixelScale;
    private ImageTexture? _texture;
    private string? _identity, _resource, _drawn, _error;
    private string _mode = "original", _variant = "text";
    private readonly Label _missing = new() { Text = "!", MouseFilter = MouseFilterEnum.Ignore };
    public override void _Ready() { AddChild(_missing); _missing.AddThemeColorOverride("font_color", Colors.Red); _missing.AddThemeFontSizeOverride("font_size", 40); }
    internal IdentityIcon(int pixelScale = 4)
    { _pixelScale = pixelScale; MouseFilter = MouseFilterEnum.Ignore; TextureFilter = TextureFilterEnum.Nearest; CustomMinimumSize = new(16 * pixelScale, 24 * pixelScale); }
    internal void Present(IdentityIcons owner, string? identity, bool selected)
    {
        _identity = identity; _texture = owner.Get(identity, selected); _error = owner.Error(identity);
        _mode = owner.Mode; _variant = owner.Variant(identity);
        _missing.Visible = _error is not null; _missing.TooltipText = _error is null ? "" : $"{_error}: {identity}";
        _resource = _texture is null ? null : identity + (selected ? ".selected" : ".normal");
        Visible = identity is not null && (_texture is not null || _error is not null); QueueRedraw();
    }
    public override void _Draw()
    {
        _drawn = null;
        if (_texture is null) return;
        DrawTextureRect(_texture, new Rect2(Vector2.Zero, new(16 * _pixelScale, 24 * _pixelScale)), false); _drawn = _resource;
    }
    internal string? PixelDigest()
    {
        if (_texture is null) return null;
        using var image = _texture.GetImage();
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(image.GetData()));
    }
    internal object Observe() => new { identity = _identity, resource = _resource, drawn = _drawn, visible = IsVisibleInTree(),
        error = _error, mode = _mode, variant = _variant, texture = _texture?.GetInstanceId(), width = _texture?.GetWidth(), height = _texture?.GetHeight(),
        scale = _pixelScale, filter = TextureFilter.ToString(), rect = Battles.BattleMapViewport.Rectangle(GetGlobalRect()) };
}
