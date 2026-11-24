using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Exploration;

namespace Sf2.Remake.GodotAdapter.UI;

// Decode atomically; buttons retain sole texture ownership, including across temporary reparenting.
internal static class MenuRasterTextures
{
    internal static void Configure(IReadOnlyList<DiamondCommandButton> commands,
        IReadOnlyDictionary<string, ExplorationRaster>? rasters, string prefix, string owner,
        IReadOnlyDictionary<string, ExplorationRaster>? hd = null)
    {
        bool mounted = rasters?.Keys.Any(key => key.StartsWith(prefix, StringComparison.Ordinal)) == true;
        string[] keys = commands.SelectMany(command => Keys(command.Identity, prefix)).ToArray();
        // A composite menu may show only part of a source family.
        bool complete = mounted && hd is not null &&
            rasters!.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)).All(hd.ContainsKey);
        var originals = new ImageTexture?[keys.Length];
        var variants = new ImageTexture?[keys.Length];
        try
        {
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                if (mounted)
                {
                    if (!rasters!.TryGetValue(key, out var raster) || raster.Width != 24 || raster.Height != 24)
                        throw new InvalidOperationException(owner + "-resource-required");
                    using var image = ExplorationRasterImages.Open(raster, owner + "-resource-invalid");
                    originals[i] = ImageTexture.CreateFromImage(image);
                }
                if (hd?.TryGetValue(key, out var variant) == true)
                {
                    if (!mounted || variant.Width != 96 || variant.Height != 96)
                        throw new InvalidOperationException(owner + "-hd-resource-invalid");
                    using var image = ExplorationRasterImages.Open(variant, owner + "-hd-resource-invalid");
                    if (complete) variants[i] = ImageTexture.CreateFromImage(image);
                }
            }
        }
        catch
        {
            foreach (var texture in originals.Concat(variants)) texture?.Dispose();
            throw;
        }
        for (int i = 0; i < commands.Count; i++)
            commands[i].SetArt(originals[i * 2], originals[i * 2 + 1], variants[i * 2], variants[i * 2 + 1]);
    }

    private static IEnumerable<string> Keys(string identity, string prefix)
    {
        string key = prefix + identity.ToLowerInvariant();
        return [key + ".normal", key + ".selected"];
    }
}
