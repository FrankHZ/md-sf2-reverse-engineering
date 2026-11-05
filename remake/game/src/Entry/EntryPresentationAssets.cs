using Godot;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Content.Localization;

namespace Sf2.Remake.GodotAdapter.Entry;

internal sealed class EntryPresentationAssets : IDisposable
{
    internal readonly Dictionary<string, ImageTexture> Textures = [];
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
            foreach (var (name, bytes) in Content.Images)
            {
                using var image = new Image();
                if (image.LoadPngFromBuffer(bytes) != Error.Ok || image.GetWidth() != 256 || image.GetHeight() != 224)
                    throw new ArgumentException("entry-image-decode");
                Textures.Add(name, ImageTexture.CreateFromImage(image));
            }
        }
        catch { Dispose(); throw; }
    }
    internal void AttachAudio(Node owner) => Audio = new SessionAudio(owner, Content.Audio);
    public void Dispose()
    {
        Audio?.Dispose(); Audio = null;
        foreach (var texture in Textures.Values) texture.Dispose();
        Textures.Clear();
    }
}
