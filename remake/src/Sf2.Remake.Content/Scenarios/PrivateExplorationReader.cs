using System.Text.Json;
using System.Text.RegularExpressions;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

// A prepared private world and separately declared party are admitted once. Runtime never rereads them.
public sealed class PrivateExplorationReader(string path, string startPath, IScenarioSource partySource) : IScenarioSource
{
    public ScenarioReadResult Read()
    {
        try
        {
            var party = partySource.Read();
            if (party is ScenarioReadRejected rejected) return rejected;
            Require(party is ScenarioReadAccepted { Definition.PrivateDefinitions: not null }, "private-party-required", "party");
            var admitted = (ScenarioReadAccepted)party;
            byte[] bytes = File.ReadAllBytes(path);
            // The reached six music / 25 SFX variants occupy 104,374,046 bytes in the private world.
            // Keep the private resident document bounded; other package limits are unchanged.
            Require(bytes.Length <= 100 * 1024 * 1024, "document-size", "world");
            using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 32 });
            var root = document.RootElement;
            Object(root, "private-world", "formatVersion", "profile", "package", "provenance", "world");
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
            using var startDocument = JsonDocument.Parse(File.ReadAllBytes(startPath), new JsonDocumentOptions { MaxDepth = 16 });
            var start = startDocument.RootElement;
            Object(start, "controlled-exploration-start", "formatVersion", "controlledBoundary", "start");
            _ = Number(start, "formatVersion", 1, 1);
            Require(!string.IsNullOrWhiteSpace(Text(start, "controlledBoundary")), "controlled-provenance", "start");
            return ExplorationContentReader.Read(Id(root, "package"), root.GetProperty("world"), start.GetProperty("start"), admitted,
                new(expected.Repository, expected.Commit, Text(provenance, "romSha256"), sources, Text(provenance, "controlledBoundary") + " Start: " + Text(start, "controlledBoundary")));
        }
        catch (AdmissionIssue error) { return new ScenarioReadRejected(error.Failure); }
        catch (BattleRuleException error)
        { return new ScenarioReadRejected(new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.ContentError,
            error.Code, error.Field, error.Code.Replace('-', ' '))); }
        catch (IOException) { return Rejected("content-read", "world"); }
        catch (UnauthorizedAccessException) { return Rejected("content-read", "world"); }
        catch (JsonException) { return Rejected("json-syntax", "world"); }
    }
}
