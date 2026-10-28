using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;

[assembly: InternalsVisibleTo("Sf2.Remake.Content")]
[assembly: InternalsVisibleTo("Sf2.Remake.Engine.Tests")]

namespace Sf2.Remake.Application.Content.Scenarios;

public sealed class ScenarioDefinition
{
    internal ScenarioDefinition(string package, IEnumerable<BattleDefinition> encounters, PrivateBattleDefinitions? privateDefinitions = null, ExplorationDefinition? exploration = null,
        BattleSceneDefinition? battleScenes = null, IEnumerable<BattleStartInput>? encounterStarts = null,
        IEnumerable<BattleSceneDefinition>? encounterScenes = null, BattleStoryDefinition? battleStory = null)
    {
        Package = package; PrivateDefinitions = privateDefinitions; Exploration = exploration; BattleScenes = battleScenes; BattleStory = battleStory;
        Encounters = new ReadOnlyDictionary<string, BattleDefinition>(
            encounters.ToDictionary(encounter => encounter.Encounter, StringComparer.Ordinal));
        EncounterStarts = new ReadOnlyDictionary<string, BattleStartInput>((encounterStarts ?? [])
            .ToDictionary(start => start.Encounter, StringComparer.Ordinal));
        EncounterScenes = new ReadOnlyDictionary<string, BattleSceneDefinition>((encounterScenes ?? (battleScenes is null ? [] : [battleScenes]))
            .ToDictionary(scene => scene.Encounter, StringComparer.Ordinal));
    }
    public string Package { get; }
    public BattleStoryDefinition? BattleStory { get; }
    internal IReadOnlyDictionary<string, StoryProgram> Programs => Exploration?.Programs ?? BattleStory!.Programs;
    public IReadOnlyDictionary<int, string> Texts => Exploration?.Texts ?? BattleStory!.Texts;
    public ExplorationDefinition? Exploration { get; }
    public BattleSceneDefinition? BattleScenes { get; }
    public IReadOnlyDictionary<string, BattleStartInput> EncounterStarts { get; }
    public IReadOnlyDictionary<string, BattleSceneDefinition> EncounterScenes { get; }
    public BattleSceneDefinition? ScenesFor(string encounter) => EncounterScenes.GetValueOrDefault(encounter);
    public PrivateBattleDefinitions? PrivateDefinitions { get; }
    public string Origin => PrivateDefinitions is null ? "public-authored-controlled-start" : "private-local-controlled-start";
    public IReadOnlyDictionary<string, BattleDefinition> Encounters { get; }
}

public interface IScenarioSource { ScenarioReadResult Read(); }
public abstract record ScenarioReadResult;
public sealed record ScenarioReadAccepted(ScenarioDefinition Definition, BattleStartInput Start) : ScenarioReadResult
{
    public string Origin => Definition.Origin;
}
public sealed record ScenarioReadRejected(SessionFailure Failure) : ScenarioReadResult;
