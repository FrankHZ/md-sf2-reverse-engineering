using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.Content.Scenarios;

// Private offline entry package, independent of gameplay/save definitions.
public sealed record EntryPresentation(IReadOnlyDictionary<string, byte[]> Images,
    IReadOnlyDictionary<string, string> Dialogue, IReadOnlyDictionary<string, ExplorationAudio> Audio,
    string Profile, string UpstreamCommit, string RomSha256,
    IReadOnlyDictionary<string, byte[]>? HdImages = null);

public static class EntryPresentationReader
{
    public static EntryPresentation Read(string path)
    {
        try { return ReadCore(path); }
        catch (ScenarioJson.AdmissionIssue issue)
        { throw new ArgumentException($"{issue.Failure.Code} ({issue.Failure.Field})", issue); }
    }

    private static EntryPresentation ReadCore(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            throw new ArgumentException("entry-content-selection");
        if (new FileInfo(path).Length > 64 * 1024 * 1024) throw new ArgumentException("entry-content-size");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        ScenarioJson.ObjectOptional(root, "entry", "hdResources", "formatVersion", "profile", "upstreamCommit", "romSha256", "resources", "dialogue", "boundary", "presentation");
        if (root.GetProperty("formatVersion").GetInt32() != 1 ||
            root.GetProperty("profile").GetString() != "private-local-entry" ||
            root.GetProperty("upstreamCommit").GetString() != "c834c652b6862bc5679fd7f69a38a7093206efc6" ||
            root.GetProperty("romSha256").GetString() != "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9")
            throw new ArgumentException("entry-content-identity");
        string[] names = ["title", "numeral", "witch-arrival", "witch", "witch-blink", "witch-mouth"];
        var images = ReadImages(root.GetProperty("resources"), false);
        if (images.Count != names.Length) throw new ArgumentException("entry-image-required");
        var hdImages = root.TryGetProperty("hdResources", out var hd)
            ? ReadImages(hd, true) : new Dictionary<string, byte[]>();

        Dictionary<string, byte[]> ReadImages(JsonElement resources, bool alternate)
        {
            var result = new Dictionary<string, byte[]>();
            int scale = alternate ? 4 : 1;
            foreach (var resource in resources.EnumerateArray())
            {
                if (alternate)
                    ScenarioJson.Object(resource, "entry-image", "name", "file", "width", "height");
                else
                    ScenarioJson.Object(resource, "entry-image", "name", "file", "width", "height", "usedTiles");
                string name = resource.GetProperty("name").GetString()!;
                string file = name + (alternate ? ".hd.png" : ".png");
                if (!names.Contains(name) || resource.GetProperty("file").GetString() != file ||
                    resource.GetProperty("width").GetInt32() != 256 * scale ||
                    resource.GetProperty("height").GetInt32() != 224 * scale || result.ContainsKey(name))
                    throw new ArgumentException("entry-image-binding");
                string image = Path.Combine(Path.GetDirectoryName(path)!, file);
                if (new FileInfo(image).Length > 256 * 1024 * scale * scale)
                    throw new ArgumentException("entry-image-size");
                result.Add(name, File.ReadAllBytes(image));
            }
            return result;
        }
        var dialogue = new Dictionary<string, string>();
        var textRows = root.GetProperty("dialogue");
        string[] ids = ["216", "217", "218", "219", "220", "221"];
        if (textRows.TryGetProperty("225", out _)) ids = [.. ids, "224", "225", "226"];
        ScenarioJson.Object(textRows, "entry-dialogue", ids);
        foreach (string id in ids)
        {
            string text = textRows.GetProperty(id).GetString()!;
            if (string.IsNullOrWhiteSpace(text) || text.Length > 300) throw new ArgumentException("entry-dialogue");
            if (id is "224" or "225" or "226")
            {
                var parameters = ExplorationTextTokens.Read(text)
                    .Where(token => token.Kind != ExplorationTextTokenKind.Literal)
                    .Select(token => token.Value);
                string[] expected = id == "226" ? ["{NAME;0}"] : [];
                if (!parameters.SequenceEqual(expected)) throw new ArgumentException("entry-dialogue-parameters");
            }
            dialogue.Add(id, text);
        }
        var audio = ExplorationAssetReader.Read(root.GetProperty("presentation")).Audio;
        if (audio.Count != 2 || !audio.TryGetValue("MUSIC_TITLE", out var title) || title.Command != 28 ||
            !audio.TryGetValue("MUSIC_WITCH", out var witch) || witch.Command != 10 || witch.LoopBegin is null)
            throw new ArgumentException("entry-music-required");
        return new(images, dialogue, audio, root.GetProperty("profile").GetString()!,
            root.GetProperty("upstreamCommit").GetString()!, root.GetProperty("romSha256").GetString()!, hdImages);
    }
}
