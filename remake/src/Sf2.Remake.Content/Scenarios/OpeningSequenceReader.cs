using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;

namespace Sf2.Remake.Content.Scenarios;

// A bounded, closed selection. Campaign worlds and the live journey are never consulted.
public static class OpeningSequenceReader
{
    public sealed record Prepared(SessionStartOutcome Outcome, string Catalog);

    public static Prepared Read(string path, int? act = null)
    {
        try { return ReadCore(path, act); }
        catch (ScenarioJson.AdmissionIssue error) { throw new ArgumentException(error.Failure.Code, error); }
    }

    private static Prepared ReadCore(string path, int? act)
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
        IScenarioSource source = new PrivateExplorationReader(world, Field("start"), party);
        if (act is { } selectedAct) source = new SingleActSource(source, selectedAct);
        return new(GameSession.Start(source, Sf2.Remake.Application.Gameplay.RuleCompositions.ForGame()), Field("catalog"));
    }

    private sealed class SingleActSource(IScenarioSource source, int act) : IScenarioSource
    {
        public ScenarioReadResult Read()
        {
            var read = source.Read();
            if (read is not ExplorationReadAccepted { Definition: var definition, Start: var start } accepted)
                return read;
            var field = definition.Exploration!;
            var entry = field.Maps[start.Map].OnLoad;
            if (act is < 1 or > 4 || entry is not { Instruction: 0, Program: var programId } ||
                !field.Programs.TryGetValue(programId, out var dispatch) ||
                dispatch.Instructions is not [PresentCue { Kind: PresentationCueKind.Sound } music,
                    CallProgram, CallProgram, CallProgram, CallProgram, CloseText close, EndScene end])
                throw new ArgumentException("opening-act-dispatch");

            // Keep the published act bodies and their targets. Select just one call in the
            // launch-local dispatcher, between the ordinary music cue and scene end.
            var selected = new StoryProgram(dispatch.Id, [music, dispatch.Instructions[act], close, end],
                dispatch.Source, dispatch.EntitiesRunning);
            var scopedField = new ExplorationDefinition(field.Maps.Values,
                field.Programs.Values.Select(program => program == dispatch ? selected : program),
                field.Texts, field.Provenance, field.PartyFlags, field.Visuals, field.MemberNames,
                field.TextTokens, field.TextFont) { GameplayRevision = field.GameplayRevision, Guidance = field.Guidance };
            return accepted with
            {
                Definition = new(definition.Package, definition.Encounters.Values, definition.PrivateDefinitions,
                    scopedField, definition.BattleScenes, definition.EncounterStarts.Values,
                    definition.EncounterScenes.Values, definition.BattleStory, definition.StartOrigin),
            };
        }
    }
}
