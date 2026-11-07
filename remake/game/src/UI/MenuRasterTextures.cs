using Godot;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter.UI;

// Decode atomically; buttons retain sole texture ownership, including across temporary reparenting.
internal static class MenuRasterTextures
{
    internal static void Configure(IReadOnlyList<DiamondCommandButton> commands,
        IReadOnlyDictionary<string, ExplorationRaster>? rasters, string prefix, string owner)
    {
        bool mounted = rasters?.Keys.Any(key => key.StartsWith(prefix, StringComparison.Ordinal)) == true;
        var textures = new List<ImageTexture>();
        try
        {
            if (mounted)
                foreach (var command in commands)
                    foreach (string state in new[] { "normal", "selected" })
                    {
                        string key = prefix + command.Identity.ToLowerInvariant() + "." + state;
                        if (!rasters!.TryGetValue(key, out var raster) || raster.Width != 24 || raster.Height != 24)
                            throw new InvalidOperationException(owner + "-resource-required");
                        using var image = raster.Format == "rgba8"
                            ? Image.CreateFromData(24, 24, false, Image.Format.Rgba8, raster.CopyBytes()) : new Image();
                        if (raster.Format != "rgba8" && image.LoadPngFromBuffer(raster.CopyBytes()) != Godot.Error.Ok ||
                            image.GetWidth() != 24 || image.GetHeight() != 24)
                            throw new InvalidOperationException(owner + "-resource-invalid");
                        textures.Add(ImageTexture.CreateFromImage(image));
                    }
        }
        catch { foreach (var texture in textures) texture.Dispose(); throw; }
        for (int i = 0; i < commands.Count; i++)
            commands[i].SetArt(mounted ? textures[i * 2] : null, mounted ? textures[i * 2 + 1] : null);
    }
}
