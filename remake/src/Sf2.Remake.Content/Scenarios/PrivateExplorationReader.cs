using System.Text.Json;
using System.Text.RegularExpressions;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

// A prepared private world and separately declared party are admitted once. Runtime never rereads them.
public sealed class PrivateExplorationReader : IScenarioSource
{
    private readonly string path;
    private readonly string? startPath;
    private readonly IScenarioSource? partySource;
    private readonly PrivateBattleContentReader? newGame;
    public PrivateExplorationReader(string path, string startPath, IScenarioSource partySource)
    { this.path = path; this.startPath = startPath; this.partySource = partySource; }
    internal PrivateExplorationReader(string path, PrivateBattleContentReader newGame)
    { this.path = path; this.newGame = newGame; }

    public ScenarioReadResult Read()
    {
        try
        {
            var party = partySource?.Read();
            if (party is ScenarioReadRejected rejected) return rejected;
            // The measured seven-map/expanded-audio input is about 127.44 million bytes.
            // Reject oversized files before allocating their full contents, and recheck after read.
            Require(new FileInfo(path).Length <= 128 * 1024 * 1024, "document-size", "world");
            byte[] bytes = File.ReadAllBytes(path);
            Require(bytes.Length <= 128 * 1024 * 1024, "document-size", "world");
            using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 32 });
            var root = document.RootElement;
            ObjectOptional(root, "private-world", "encounters", ["formatVersion", "profile", "package", "provenance", "world"]);
            if (newGame is not null)
            {
                Require(root.GetProperty("world").TryGetProperty("growth", out var growth), "initial-growth-required", "world.growth");
                newGame.SelectOpening(growth);
                party = newGame.Read();
            }
            Require(party is ScenarioReadAccepted { Definition.PrivateDefinitions: not null }, "private-party-required", "party");
            var admitted = (ScenarioReadAccepted)party!;
            if (root.TryGetProperty("encounters", out var encounters))
                admitted = PrivateWorldEncounterReader.Read(encounters, path, admitted, newGame);
            _ = Number(root, "formatVersion", 1, 1);
            Require(Text(root, "profile") == "private-local-controlled-start", "profile", "profile");
            var provenance = root.GetProperty("provenance");
            Object(provenance, "provenance", "repository", "commit", "romSha256", "sources", "controlledBoundary");
            var expected = admitted.Definition.PrivateDefinitions!.Encounter.PlacementSource;
            Require(Text(provenance, "repository") == expected.Repository && Text(provenance, "commit") == expected.Commit,
                "world-source-identity", "provenance");
            Require(Regex.IsMatch(Text(provenance, "romSha256"), "^[A-Fa-f0-9]{64}$", RegexOptions.CultureInvariant),
                "source-digest", "provenance.romSha256");
            Require(admitted.Definition.PrivateDefinitions!.EnemyGold.Values.All(gold =>
                string.Equals(gold.RomSha256, Text(provenance, "romSha256"), StringComparison.OrdinalIgnoreCase)),
                "world-rom-identity", "provenance.romSha256");
            Require(!string.IsNullOrWhiteSpace(Text(provenance, "controlledBoundary")), "controlled-provenance", "provenance");
            var paths = new HashSet<string>(StringComparer.Ordinal);
            var sources = new List<EncounterSource>();
            foreach (var source in Array(provenance, "sources"))
            {
                Object(source, "source", "path", "sha256");
                string relative = Text(source, "path");
                Require(!string.IsNullOrWhiteSpace(relative) && !Path.IsPathRooted(relative) && !relative.Split('/', '\\').Contains("..") && paths.Add(relative),
                    "source-path", "provenance.sources");
                Require(Regex.IsMatch(Text(source, "sha256"), "^[A-Fa-f0-9]{64}$", RegexOptions.CultureInvariant),
                    "source-digest", "provenance.sources");
                sources.Add(new(expected.Repository, expected.Commit, relative, Text(source, "sha256")));
            }
            Require(paths.Count > 0, "source-required", "provenance.sources");
            using var startDocument = newGame is null
                ? JsonDocument.Parse(File.ReadAllBytes(startPath!), new JsonDocumentOptions { MaxDepth = 16 }) : null;
            JsonElement start;
            string startBoundary = "Product New Game; no controlled checkpoint supplied.";
            if (startDocument is not null)
            {
                var controlled = startDocument.RootElement;
                Object(controlled, "controlled-exploration-start", "formatVersion", "controlledBoundary", "start");
                _ = Number(controlled, "formatVersion", 1, 1);
                Require(!string.IsNullOrWhiteSpace(Text(controlled, "controlledBoundary")), "controlled-provenance", "start");
                start = controlled.GetProperty("start");
                startBoundary = Text(controlled, "controlledBoundary");
            }
            else start = PrivateBattleContentReader.Start();
            var result = ExplorationContentReader.Read(Id(root, "package"), root.GetProperty("world"), start, admitted,
                new(expected.Repository, expected.Commit, Text(provenance, "romSha256"), sources, Text(provenance, "controlledBoundary") + " Start: " + startBoundary));
            if (result.Definition.Exploration!.Visuals is { } visuals)
                foreach (var scene in result.Definition.EncounterScenes.Values)
                    if (scene.FieldDeath is { } field)
                        Require(field.AllySprites.Values.Concat(field.EnemySprites.Values).All(visuals.Sprites.ContainsKey),
                            "field-actor-resource-unavailable", "world.presentation.sprites");
            return newGame?.Initialize(result) ?? result;
        }
        catch (AdmissionIssue error) { return new ScenarioReadRejected(error.Failure); }
        catch (BattleRuleException error)
        { return new ScenarioReadRejected(new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.ContentError,
            error.Code, error.Field, error.Code.Replace('-', ' '))); }
        catch (IOException) { return Rejected("content-read", "world"); }
        catch (UnauthorizedAccessException) { return Rejected("content-read", "world"); }
        catch (JsonException) { return Rejected("json-syntax", "world"); }
        catch (PrivateBattleEncounterReader.EncounterReadException error)
        { return Rejected("private-definition-input", error.Diagnostic.Field); }
        catch (InvalidOperationException) { return Rejected("invalid-definition", "world"); }
        catch (KeyNotFoundException) { return Rejected("missing-definition-field", "world"); }
        catch (FormatException) { return Rejected("invalid-definition-field", "world"); }
        catch (ArgumentException) { return Rejected("invalid-input-selection", "world"); }
    }
}
