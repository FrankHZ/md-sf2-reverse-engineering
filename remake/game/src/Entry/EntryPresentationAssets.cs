using Godot;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Audio;

namespace Sf2.Remake.GodotAdapter.Entry;

internal sealed class EntryPresentationAssets : IDisposable
{
    internal readonly Dictionary<string, ImageTexture> Textures = [];
    internal readonly EntryPresentation Content;
    internal SessionAudio? Audio { get; private set; }

    internal EntryPresentationAssets()
    {
        Content = EntryPresentationReader.Read(System.Environment.GetEnvironmentVariable("SF2_PRIVATE_ENTRY_CONTENT") ?? "");
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
