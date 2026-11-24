using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Maps;

internal static class HdTerrainObservation
{
    // The ordinary packager supplies a relocated worktree-local copy. Only the two freshly
    // created control files are rewritten; all its admitted package members stay read-only.
    internal static long Run(string package)
    {
        const string map = "map-49", file = "authored-hd-control.json";
        string worldPath = Path.Combine(package, "authored-world-control.json");
        string bankPath = Path.Combine(package, file);
        using (File.Open(worldPath, FileMode.CreateNew)) { }
        using (File.Open(bankPath, FileMode.CreateNew)) { }
        var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        using var source = File.OpenRead(Path.Combine(package, "world.json"));
        var world = JsonNode.Parse(source)!;
        var visual = world["world"]!["presentation"]!["maps"]!.AsArray().Single(v => (string?)v!["map"] == map)!.AsObject();
        int count = visual["blocks"]!.AsArray().Count;
        var frames = new JsonArray(Enumerable.Range(0, count).Select(i => Raster(i)).ToArray());
        var context = new JsonArray(new JsonObject {
            ["neighbors"] = JsonNode.Parse("[[-1,-1,-1,-1,0,-1,-1,-1,-1]]"), ["raster"] = Raster(count)
        });
        var bank = new JsonObject { ["map"] = map, ["hdBlocks"] = frames, ["hdBlockContexts"] = context };
        var neighborhood = new BlockNeighborhood(-1, -1, -1, -1, 0, -1, -1, -1, -1);
        string Input(string key) => Path.Combine(package, "opening-inputs", $"SF2_PRIVATE_{key}.json");
        var reader = new OpeningRestoreReader(worldPath, Input("BATTLE01_DATA"), Input("BATTLE01_SCENE"),
            Input("BATTLE01_TERRAIN"), Input("STATIC_DATA"), Input("ENEMY_DATA"), Input("ENEMY_GOLD"),
            Path.Combine(package, "main-scenes.json"));

        Publish(worldPath, world);
        var original = Read();
        if (original is not { HdBlocks: null, HdBlockContexts: null })
            throw new InvalidOperationException("Expected a map without an HD bank");
        visual["hdBlocks"] = frames.DeepClone();
        visual["hdBlockContexts"] = context.DeepClone();
        Publish(worldPath, world);
        Match(Read()!);
        visual.Remove("hdBlocks");
        visual.Remove("hdBlockContexts");
        visual["hdTerrainFile"] = file;
        Publish(worldPath, world);
        Publish(bankPath, bank);
        Match(Read()!);

        bank.Remove("hdBlockContexts");
        Publish(bankPath, bank);
        if (Read() is not { HdBlockContexts: null, HdBlocks.Count: > 0 })
            throw new InvalidOperationException("Center-only HD bank did not mount");
        bank["hdBlockContexts"] = context;
        bank["map"] = "map-3";
        Publish(bankPath, bank);
        Read("hd-terrain-map");
        bank["map"] = map;
        context[0]!["neighbors"]!.AsArray().Add(context[0]!["neighbors"]![0]!.DeepClone());
        Publish(bankPath, bank);
        Read("duplicate-hd-context");
        context[0]!["neighbors"]!.AsArray().RemoveAt(1);
        frames[0]!["sha256"] = new string('0', 64);
        Publish(bankPath, bank);
        Read("raster-identity");
        File.WriteAllText(bankPath, """{"map":"map-49","map":"map-49","hdBlocks":[]}""");
        Read("unknown-or-duplicate-field");
        File.WriteAllText(bankPath, "{");
        Read("json-syntax");
        using (var oversized = File.OpenWrite(bankPath)) oversized.SetLength(8 * 1024 * 1024 + 1);
        Read("document-size");

        visual["hdTerrainFile"] = "absent-control.json";
        Publish(worldPath, world);
        Read("hd-terrain-read");
        visual["hdTerrainFile"] = "../outside.json";
        Publish(worldPath, world);
        Read("hd-terrain-path");
        visual["hdBlockContexts"] = context.DeepClone();
        Publish(worldPath, world);
        Read("mixed-hd-terrain");
        return new FileInfo(bankPath).Length + new FileInfo(worldPath).Length;

        ExplorationMapVisual? Read(string expected = "accepted")
        {
            var result = reader.Read();
            string actual = result switch
            {
                ExplorationReadAccepted => "accepted",
                ScenarioReadRejected rejection => rejection.Failure.Code,
                _ => "unexpected"
            };
            Console.WriteLine($"map={map} expected={expected} actual={actual}");
            if (actual != expected || result is ScenarioReadRejected { Failure.Kind: not SessionFailureKind.ContentError })
                throw new InvalidOperationException("HD admission control failed");
            return (result as ExplorationReadAccepted)?.Definition.Exploration!.Visuals!.Maps[new(map)];
        }

        void Match(ExplorationMapVisual selected)
        {
            if (selected.HdBlocks?.Count != count || selected.HdBlockContexts?.Count != 1 ||
                !selected.HdBlockContexts[neighborhood].CopyBytes().SequenceEqual(Pixels(count)) ||
                selected.HdBlocks.Where((r, i) => r.Width != 96 || r.Height != 96 || r.Format != "rgba8" ||
                    !r.CopyBytes().SequenceEqual(Pixels(i))).Any())
                throw new InvalidOperationException("Authored center/context raster mismatch");
        }

        void Publish(string path, JsonNode node)
        {
            using var output = File.Create(path);
            JsonSerializer.Serialize(output, node, options);
        }
    }

    private static byte[] Pixels(int index) => Enumerable.Repeat((byte)index, 96 * 96 * 4).ToArray();
    private static JsonNode Raster(int index)
    {
        byte[] data = Pixels(index);
        return new JsonObject { ["width"] = 96, ["height"] = 96, ["format"] = "rgba8",
            ["sha256"] = Convert.ToHexString(SHA256.HashData(data)), ["data"] = Convert.ToBase64String(data) };
    }
}
