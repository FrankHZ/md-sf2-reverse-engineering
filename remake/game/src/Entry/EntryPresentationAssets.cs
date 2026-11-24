using Godot;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Content.Localization;

namespace Sf2.Remake.GodotAdapter.Entry;

internal sealed class EntryPresentationAssets : IDisposable
{
    internal readonly Dictionary<string, ImageTexture> Textures = [];
    internal readonly Dictionary<string, ImageTexture> HdTextures = [];
    internal IReadOnlyDictionary<string, ImageTexture> Select(string mode) =>
        mode == "hd" && HdTextures.Count == Textures.Count ? HdTextures : Textures;
    internal readonly EntryPresentation Content;
    internal SessionAudio? Audio { get; private set; }
    private readonly DisplayCatalog? _display;
    private readonly string? _displayError;
    internal DisplayResult Dialogue(int id)
    {
        var original = new DisplayResult(Content.Dialogue[id.ToString(System.Globalization.CultureInfo.InvariantCulture)], "en", null);
        var display = GameText.Language == "en" ? original :
            _display?.ResolveEntry(id, Content) ?? original with { Reason = _displayError };
        return display with { Text = SourceTextLayout.EntryLines(display.Text, display.EffectiveLocale) };
    }

    internal EntryPresentationAssets()
    {
        Content = EntryPresentationReader.Read(System.Environment.GetEnvironmentVariable("SF2_PRIVATE_ENTRY_CONTENT") ?? "");
        var display = GameText.EntryCatalog(Content, System.Environment.GetEnvironmentVariable("SF2_ENTRY_DISPLAY_CATALOG"));
        _display = display.Catalog; _displayError = display.Error;
        try
        {
            Load(Content.Images, Textures, 1);
            if (Content.HdImages is { } hd) Load(hd, HdTextures, 4);
        }
        catch { Dispose(); throw; }

        static void Load(IReadOnlyDictionary<string, byte[]> source, Dictionary<string, ImageTexture> target, int scale)
        {
            foreach (var (name, bytes) in source)
            {
                using var image = new Image();
                var result = image.LoadPngFromBuffer(bytes);
                if (result != Error.Ok)
                    throw new ArgumentException($"entry-image-decode ({name}): {result}");
                if (image.GetWidth() != 256 * scale || image.GetHeight() != 224 * scale)
                    throw new ArgumentException($"entry-image-dimensions ({name}): {image.GetWidth()}x{image.GetHeight()}, expected {256 * scale}x{224 * scale}");
                target.Add(name, ImageTexture.CreateFromImage(image));
            }
        }
    }
    internal void AttachAudio(Node owner) => Audio = new SessionAudio(owner, Content.Audio);
    public void Dispose()
    {
        Audio?.Dispose(); Audio = null;
        foreach (var texture in Textures.Values.Concat(HdTextures.Values)) texture.Dispose();
        Textures.Clear(); HdTextures.Clear();
    }
}
