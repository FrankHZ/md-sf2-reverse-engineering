using System.Runtime.CompilerServices;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;

[assembly: InternalsVisibleTo("Sf2.Remake.Content")]
[assembly: InternalsVisibleTo("Sf2.Remake.Engine.Tests")]

namespace Sf2.Remake.Application.Content.Scenarios;

public sealed class ScenarioDefinition
{
    internal ScenarioDefinition(string package, BattleDefinition battle, uint mainSeed, uint thinkingSeed)
    { Package = package; Battle = battle; MainSeed = mainSeed; ThinkingSeed = thinkingSeed; }
    public string Package { get; }
    public BattleDefinition Battle { get; }
    public uint MainSeed { get; }
    public uint ThinkingSeed { get; }
    public string Origin => "public-authored-controlled-start";
}

public interface IScenarioSource { ScenarioReadResult Read(); }
public abstract record ScenarioReadResult;
public sealed record ScenarioReadAccepted(ScenarioDefinition Definition) : ScenarioReadResult;
public sealed record ScenarioReadRejected(SessionFailure Failure) : ScenarioReadResult;
