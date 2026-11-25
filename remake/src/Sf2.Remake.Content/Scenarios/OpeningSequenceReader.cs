using System.Text.Json;
using Sf2.Remake.Application.Runtime;

namespace Sf2.Remake.Content.Scenarios;

// A bounded, closed selection. Campaign worlds and the live journey are never consulted.
public static class OpeningSequenceReader
{
    public sealed record Prepared(SessionStartOutcome Outcome, string Catalog);

    public static Prepared Read(string path)
    {
        try { return ReadCore(path); }
        catch (ScenarioJson.AdmissionIssue error) { throw new ArgumentException(error.Failure.Code, error); }
    }

    private static Prepared ReadCore(string path)
    {
        if (!Path.IsPathFullyQualified(path) || new FileInfo(path).Length > 4096)
            throw new ArgumentException("opening-selection");
        using var json = JsonDocument.Parse(File.ReadAllBytes(path));
        var root = json.RootElement;
        ScenarioJson.Object(root, "opening", "formatVersion", "world", "start", "party", "catalog");
        _ = ScenarioJson.Number(root, "formatVersion", 1, 1);
        string directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        string FilePath(string relative)
        {
            if (Path.IsPathRooted(relative) || relative.Split('/', '\\').Contains(".."))
                throw new ArgumentException("opening-relative-path");
            return Path.Combine(directory, relative);
        }
        string Field(string name) => FilePath(ScenarioJson.Text(root, name));
        string world = Field("world");
        if (new FileInfo(world).Length > 16 * 1024 * 1024)
            throw new ArgumentException("opening-world-size");
        string Input(string name) => FilePath("opening-inputs/" + name + ".json");
        var party = new PrivateBattleScenarioReader(Input("SF2_PRIVATE_BATTLE01_DATA"),
            Input("SF2_PRIVATE_BATTLE01_SCENE"), Input("SF2_PRIVATE_BATTLE01_TERRAIN"),
            Input("SF2_PRIVATE_STATIC_DATA"), Input("SF2_PRIVATE_ENEMY_DATA"), Input("SF2_PRIVATE_ENEMY_GOLD"), Field("party"));
        var source = new PrivateExplorationReader(world, Field("start"), party);
        return new(GameSession.Start(source, Sf2.Remake.Application.Gameplay.RuleCompositions.ForGame()), Field("catalog"));
    }
}
