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
        BattleSceneDefinition? battleScenes = null)
    {
        Package = package; PrivateDefinitions = privateDefinitions; Exploration = exploration; BattleScenes = battleScenes;
        Encounters = new ReadOnlyDictionary<string, BattleDefinition>(
            encounters.ToDictionary(encounter => encounter.Encounter, StringComparer.Ordinal));
    }
    public string Package { get; }
    public ExplorationDefinition? Exploration { get; }
    public BattleSceneDefinition? BattleScenes { get; }
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
