using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.GodotAdapter.UI;

// Sole owner of decoded identity textures. Rows borrow textures and never dispose them.
internal sealed class IdentityIcons : IDisposable
{
    private readonly Dictionary<string, ImageTexture> _textures = new(StringComparer.Ordinal);
    private bool _required;
    internal bool Mounted => _required;
    internal string? Error(string? identity) => identity is not null && _required &&
        (!_textures.ContainsKey(identity + ".normal") || !_textures.ContainsKey(identity + ".selected")) ? "identity-icon-binding-required" : null;
    internal static string Item(int word) => $"identity-icon.item.{word & 127}";
    internal static string Unarmed => "identity-icon.other.128";
    internal static string? Spell(ScenarioDefinition definition, SpellRef spell) =>
        definition.PrivateDefinitions?.Spells.TryGetValue(spell, out var source) == true
            ? $"identity-icon.spell.{source.BaseId}" : null;
    internal void Configure(IReadOnlyDictionary<string, ExplorationRaster>? rasters)
    {
        Dispose(); _required = rasters?.Keys.Any(key => key.StartsWith("identity-icon.", StringComparison.Ordinal)) == true;
        try
        {
            foreach (var (key, raster) in rasters ?? new Dictionary<string, ExplorationRaster>())
            {
                if (!key.StartsWith("identity-icon.", StringComparison.Ordinal)) continue;
                using var image = raster.Format == "rgba8"
                    ? Image.CreateFromData(raster.Width, raster.Height, false, Image.Format.Rgba8, raster.CopyBytes()) : new Image();
                if (raster.Format != "rgba8" && image.LoadPngFromBuffer(raster.CopyBytes()) != Godot.Error.Ok ||
                    image.GetWidth() != 16 || image.GetHeight() != 24)
                    throw new InvalidOperationException("identity-icon-invalid");
                _textures.Add(key, ImageTexture.CreateFromImage(image));
            }
        }
        catch { Dispose(); throw; }
    }
    internal ImageTexture? Get(string? identity, bool selected)
    {
        if (identity is null) return null;
        if (_textures.TryGetValue(identity + (selected ? ".selected" : ".normal"), out var texture)) return texture;
        return null;
    }
    public void Dispose() { foreach (var texture in _textures.Values) texture.Dispose(); _textures.Clear(); }
}

internal sealed partial class IdentityIcon : Control
{
    private ImageTexture? _texture;
    private string? _identity, _resource, _drawn, _error;
    private readonly Label _missing = new() { Text = "!", MouseFilter = MouseFilterEnum.Ignore };
    public override void _Ready() { AddChild(_missing); _missing.AddThemeColorOverride("font_color", Colors.Red); _missing.AddThemeFontSizeOverride("font_size", 40); }
    internal IdentityIcon()
    { MouseFilter = MouseFilterEnum.Ignore; TextureFilter = TextureFilterEnum.Nearest; CustomMinimumSize = new(64, 96); }
    internal void Present(IdentityIcons owner, string? identity, bool selected)
    {
        _identity = identity; _texture = owner.Get(identity, selected); _error = owner.Error(identity);
        _missing.Visible = _error is not null; _missing.TooltipText = _error is null ? "" : $"{_error}: {identity}";
        _resource = _texture is null ? null : identity + (selected ? ".selected" : ".normal");
        Visible = identity is not null && (_texture is not null || _error is not null); QueueRedraw();
    }
    public override void _Draw()
    {
        _drawn = null;
        if (_texture is null) return;
        DrawTextureRect(_texture, new Rect2(Vector2.Zero, new(64, 96)), false); _drawn = _resource;
    }
    internal string? PixelDigest()
    {
        if (_texture is null) return null;
        using var image = _texture.GetImage();
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(image.GetData()));
    }
    internal object Observe() => new { identity = _identity, resource = _resource, drawn = _drawn, visible = IsVisibleInTree(),
        error = _error, texture = _texture?.GetInstanceId(), width = _texture?.GetWidth(), height = _texture?.GetHeight(),
        scale = 4, filter = TextureFilter.ToString(), rect = Battles.BattleMapViewport.Rectangle(GetGlobalRect()) };
}
