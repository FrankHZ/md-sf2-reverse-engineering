using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

[assembly: InternalsVisibleTo("Sf2.Remake.Content")]
[assembly: InternalsVisibleTo("Sf2.Remake.Engine.Tests")]

namespace Sf2.Remake.Application.Content.Scenarios;

public sealed class ScenarioDefinition
{
    internal ScenarioDefinition(string package, IEnumerable<BattleDefinition> encounters, PrivateBattleDefinitions? privateDefinitions = null, ExplorationDefinition? exploration = null,
        BattleSceneDefinition? battleScenes = null, IEnumerable<BattleStartInput>? encounterStarts = null,
        IEnumerable<BattleSceneDefinition>? encounterScenes = null, BattleStoryDefinition? battleStory = null,
        BattleStartOrigin startOrigin = BattleStartOrigin.Controlled)
    {
        Package = package; PrivateDefinitions = privateDefinitions; Exploration = exploration; BattleScenes = battleScenes; BattleStory = battleStory;
        StartOrigin = startOrigin;
        Encounters = new ReadOnlyDictionary<string, BattleDefinition>(
            encounters.ToDictionary(encounter => encounter.Encounter, StringComparer.Ordinal));
        EncounterStarts = new ReadOnlyDictionary<string, BattleStartInput>((encounterStarts ?? [])
            .ToDictionary(start => start.Encounter, StringComparer.Ordinal));
        EncounterScenes = new ReadOnlyDictionary<string, BattleSceneDefinition>((encounterScenes ?? (battleScenes is null ? [] : [battleScenes]))
            .ToDictionary(scene => scene.Encounter, StringComparer.Ordinal));
    }
    internal (MapId Register, MapId Destination)? DefeatFallback { get; init; }
    internal Runtime.Persistence.IOpeningRestorePolicy? Persistence { get; init; }
    internal Runtime.Exploration.FieldPreparationDefinition? FieldPreparation { get; init; }
    public string Package { get; }
    public BattleStoryDefinition? BattleStory { get; }
    internal IReadOnlyDictionary<string, StoryProgram> Programs => Exploration?.Programs ?? BattleStory!.Programs;
    public IReadOnlyDictionary<int, string> Texts => Exploration?.Texts ?? BattleStory!.Texts;
    public ExplorationDefinition? Exploration { get; }
    public BattleSceneDefinition? BattleScenes { get; }
    public IReadOnlyDictionary<string, BattleStartInput> EncounterStarts { get; }
    public IReadOnlyDictionary<string, BattleSceneDefinition> EncounterScenes { get; }
    public BattleSceneDefinition? ScenesFor(string encounter) => EncounterScenes.GetValueOrDefault(encounter);
    // The Godot viewport retains resources by scene reference across frames. Bind a
    // fresh roster once and release it when the live battle definition goes away.
    private readonly ConditionalWeakTable<BattleDefinition, BattleSceneDefinition> _battleScenes = new();
    public BattleSceneDefinition? ScenesFor(BattleDefinition battle)
    {
        var scene = ScenesFor(battle.Encounter);
        if (scene?.Species is not { Count: > 0 } species || ReferenceEquals(battle, Encounters[battle.Encounter])) return scene;
        return _battleScenes.GetValue(battle, value =>
        {
            var ids = value.Deployments.Where(row => row.SourceEnemy.HasValue)
                .ToDictionary(row => row.Actor, row => row.SourceEnemy!.Value);
            if (ids.All(row => scene.EnemyNameIds![row.Key] == row.Value)) return scene;
            return scene with
            {
                EnemyNameIds = ids.AsReadOnly(),
                EnemyNames = ids.ToDictionary(row => row.Key, row => species[row.Value].Name).AsReadOnly(),
                Enemies = ids.ToDictionary(row => row.Key, row => species[row.Value].Body).AsReadOnly(),
                FieldDeath = scene.FieldDeath! with { EnemySprites = ids.ToDictionary(row => row.Key, row => species[row.Value].MapSprite).AsReadOnly() },
            };
        });
    }
    public PrivateBattleDefinitions? PrivateDefinitions { get; }
    public BattleStartOrigin StartOrigin { get; }
    public string Origin => PrivateDefinitions is null ? "public-authored-controlled-start" :
        StartOrigin == BattleStartOrigin.NewGame ? "private-local-new-game" : "private-local-controlled-start";
    public IReadOnlyDictionary<string, BattleDefinition> Encounters { get; }
}

public interface IScenarioSource { ScenarioReadResult Read(); }
public abstract record ScenarioReadResult;
public sealed record ScenarioReadAccepted(ScenarioDefinition Definition, BattleStartInput Start) : ScenarioReadResult
{
    public string Origin => Definition.Origin;
}
public sealed record ScenarioReadRejected(SessionFailure Failure) : ScenarioReadResult;
