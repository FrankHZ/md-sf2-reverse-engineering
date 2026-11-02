using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.Content.Scenarios;

// Private offline entry package, independent of gameplay/save definitions.
public sealed record EntryPresentation(IReadOnlyDictionary<string, byte[]> Images,
    IReadOnlyDictionary<string, string> Dialogue, IReadOnlyDictionary<string, ExplorationAudio> Audio);

public static class EntryPresentationReader
{
    public static EntryPresentation Read(string path)
    {
        try { return ReadCore(path); }
        catch (ScenarioJson.AdmissionIssue issue) { throw new ArgumentException(issue.Failure.Code); }
    }

    private static EntryPresentation ReadCore(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            throw new ArgumentException("entry-content-selection");
        if (new FileInfo(path).Length > 64 * 1024 * 1024) throw new ArgumentException("entry-content-size");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        ScenarioJson.Object(root, "entry", "formatVersion", "profile", "upstreamCommit", "romSha256", "resources", "dialogue", "boundary", "presentation");
        if (root.GetProperty("formatVersion").GetInt32() != 1 ||
            root.GetProperty("profile").GetString() != "private-local-entry" ||
            root.GetProperty("upstreamCommit").GetString() != "c834c652b6862bc5679fd7f69a38a7093206efc6" ||
            root.GetProperty("romSha256").GetString() != "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9")
            throw new ArgumentException("entry-content-identity");
        string[] names = ["title", "numeral", "witch-arrival", "witch", "witch-blink", "witch-mouth"];
        var images = new Dictionary<string, byte[]>();
        foreach (var resource in root.GetProperty("resources").EnumerateArray())
        {
            ScenarioJson.Object(resource, "entry-image", "name", "file", "width", "height", "usedTiles");
            string name = resource.GetProperty("name").GetString()!;
            if (!names.Contains(name) || resource.GetProperty("file").GetString() != name + ".png" ||
                resource.GetProperty("width").GetInt32() != 256 || resource.GetProperty("height").GetInt32() != 224 || images.ContainsKey(name))
                throw new ArgumentException("entry-image-binding");
            string image = Path.Combine(Path.GetDirectoryName(path)!, name + ".png");
            if (new FileInfo(image).Length > 256 * 1024) throw new ArgumentException("entry-image-size");
            images.Add(name, File.ReadAllBytes(image));
        }
        if (images.Count != names.Length) throw new ArgumentException("entry-image-required");
        var dialogue = new Dictionary<string, string>();
        ScenarioJson.Object(root.GetProperty("dialogue"), "entry-dialogue", "216", "217", "218", "219", "220", "221");
        for (int id = 216; id <= 221; id++)
        {
            string text = root.GetProperty("dialogue").GetProperty(id.ToString()).GetString()!;
            if (string.IsNullOrWhiteSpace(text) || text.Length > 300) throw new ArgumentException("entry-dialogue");
            dialogue.Add(id.ToString(), text);
        }
        var audio = ExplorationAssetReader.Read(root.GetProperty("presentation")).Audio;
        if (audio.Count != 2 || !audio.TryGetValue("MUSIC_TITLE", out var title) || title.Command != 28 ||
            !audio.TryGetValue("MUSIC_WITCH", out var witch) || witch.Command != 10 || witch.LoopBegin is null)
            throw new ArgumentException("entry-music-required");
        return new(images, dialogue, audio);
    }
}
