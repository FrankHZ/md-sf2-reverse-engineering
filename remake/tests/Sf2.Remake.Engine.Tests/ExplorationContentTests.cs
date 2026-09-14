using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ExplorationContentTests
{
    [Fact]
    public void UntakenBranchTargetsMustStillResolveAtContentAdmission()
    {
        var document = Document("harbor-arrival");
        document["world"]!["programs"]![0]!["instructions"]![3]!["target"]!["program"] = "missing";
        var failure = Assert.IsType<ScenarioReadRejected>(Reader(document).Read()).Failure;
        Assert.Equal("unresolved-program-target", failure.Code);
    }
    [Fact]
    public void DuplicateEntitiesAndMissingMapTargetsAreRejected()
    {
        var document = Document("harbor-arrival");
        var entities = document["world"]!["maps"]![0]!["entities"]!.AsArray();
        entities.Add(entities[0]!.DeepClone());
        Assert.Equal("duplicate-entity", Assert.IsType<ScenarioReadRejected>(Reader(document).Read()).Failure.Code);
        document = Document("harbor-arrival");
        document["world"]!["programs"]![1]!["instructions"]![4]!["map"] = "missing";
        Assert.Equal("missing-map", Assert.IsType<ScenarioReadRejected>(Reader(document).Read()).Failure.Code);
    }

    [Fact]
    public void LastMatchingSetupWinsAndUnsupportedPopulationCannotBePublishedAtStart()
    {
        var document = Document("harbor-arrival");
        document["world"]!["maps"]![0]!["setup"] = JsonNode.Parse("""
            {"default":"base","variants":[{"flag":7,"setup":"closed"},{"flag":8,"setup":"base"}]}
            """);
        document["start"]!["flags"] = JsonNode.Parse("[7,8]");
        Assert.IsType<SessionStarted>(GameSession.Start(Reader(document)));
        document["start"]!["flags"] = JsonNode.Parse("[7]");
        var rejected = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal(SessionFailureKind.UnsupportedCapability, rejected.Failure.Kind);
        Assert.Equal("map-setup", rejected.Failure.Code);
        Assert.Equal("closed", rejected.Failure.Field);
    }
}
