using System.Text;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class AuthoredScenarioTests
{
    [Fact]
    public void DifferentRealPackagesResolveTheirOwnMapRosterSpellAndPlacement()
    {
        var first = Admitted();
        var second = Admitted("garden-watch");
        var firstBattle = first.Definition.Encounters[first.Start.Encounter];
        var secondBattle = second.Definition.Encounters[second.Start.Encounter];
        Assert.Equal("public-authored-controlled-start", first.Origin);
        Assert.NotEqual(firstBattle.Map, secondBattle.Map);
        Assert.NotEqual(firstBattle.Encounter, secondBattle.Encounter);
        Assert.Equal((12, 9), (firstBattle.Width, firstBattle.Height));
        Assert.Equal((10, 7), (secondBattle.Width, secondBattle.Height));
        Assert.Equal((ushort)95, first.Start.Actors[0].Hp);
        Assert.Equal((ushort)8, second.Start.Actors[0].Hp);
        Assert.NotEqual(firstBattle.Deployments[0].Actor, secondBattle.Deployments[0].Actor);
        Assert.NotEqual(firstBattle.Deployments[0].Position, secondBattle.Deployments[0].Position);
        Assert.NotEqual(firstBattle.Spells.Keys.Single(), secondBattle.Spells.Keys.Single());
    }

    [Theory]
    [InlineData("terrain", "missing-terrain", SessionFailureKind.ContentError)]
    [InlineData("actor", "duplicate-actor", SessionFailureKind.ContentError)]
    [InlineData("map", "missing-map", SessionFailureKind.ContentError)]
    [InlineData("placement", "missing-actor", SessionFailureKind.ContentError)]
    [InlineData("hp", "numeric-range", SessionFailureKind.ContentError)]
    [InlineData("profile", "profile", SessionFailureKind.ContentError)]
    [InlineData("effect", "spell-effect", SessionFailureKind.UnsupportedCapability)]
    [InlineData("ai", "ai-commandset", SessionFailureKind.UnsupportedCapability)]
    [InlineData("status", "actor-status", SessionFailureKind.UnsupportedCapability)]
    public void AdmissionAttributesContentAndCapabilityFailures(string variant, string code, SessionFailureKind kind)
    {
        var document = Document();
        switch (variant)
        {
            case "terrain": document["terrains"]!.AsArray().Clear(); break;
            case "actor": document["actors"]!.AsArray().Add(document["actors"]![0]!.DeepClone()); break;
            case "map": document["encounters"]![0]!["map"] = "missing"; break;
            case "placement": document["encounters"]![0]!["placements"]![0]!["actor"] = "missing"; break;
            case "hp": document["start"]!["actors"]![0]!["hp"] = 101; break;
            case "profile": document["profile"] = "private-original"; break;
            case "effect": document["spells"]![0]!["effect"]!["kind"] = "resurrect"; break;
            case "ai": document["actors"]![2]!["controller"] = "pursuit"; break;
            case "status": document["start"]!["actors"]![0]!["status"] = "poison"; break;
        }
        var rejected = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal((kind, code), (rejected.Failure.Kind, rejected.Failure.Code));
    }

    [Theory]
    [InlineData("{", "json-syntax")]
    [InlineData("{\"formatVersion\":1,\"formatVersion\":1}", "unknown-or-duplicate-field")]
    public void MalformedDocumentsRemainAttributedContentFailures(string json, string code)
    {
        var rejected = Assert.IsType<ScenarioReadRejected>(
            AuthoredScenarioPackageReader.FromDocumentBytes(Encoding.UTF8.GetBytes(json)).Read());
        Assert.Equal(SessionFailureKind.ContentError, rejected.Failure.Kind);
        Assert.Equal(code, rejected.Failure.Code);
    }
}
