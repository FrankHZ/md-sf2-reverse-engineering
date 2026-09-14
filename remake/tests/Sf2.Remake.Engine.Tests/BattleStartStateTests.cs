using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleStartStateTests
{
    [Fact]
    public void SharedDefinitionSupportsIndependentStartsAndNaturalActionHistories()
    {
        var admitted = Admitted();
        var definition = admitted.Definition;
        var encounter = definition.Encounters[admitted.Start.Encounter];
        string frozenDeployments = JsonSerializer.Serialize(encounter.Deployments);
        var frozenTerrain = encounter.Terrain.ToArray();
        var frozenSpells = encounter.Spells.ToArray();
        var inputs = admitted.Start.Actors.Select(a => a.Actor == new ActorRef("medic-a")
            ? a with { Hp = 70, Mp = 7, Exp = 40, Kills = 12, Defeats = 3, PositionOverride = new MapPosition(4, 3) } : a).Reverse().ToArray();
        var otherStart = new BattleStartInput(admitted.Start.Encounter, inputs, admitted.Start.MainSeed, 0x12344321, 1234);
        inputs[2] = inputs[2] with { Hp = 1 }; // The real start input owns its external collection.
        var first = Assert.IsType<SessionStarted>(GameSession.Start(definition, admitted.Start)).Session;
        var second = Assert.IsType<SessionStarted>(GameSession.Start(definition, otherStart)).Session;
        Assert.Same(encounter, first.Current.Battle.Definition);
        Assert.Same(encounter, second.Current.Battle.Definition);
        Assert.Same(encounter.Deployments[0].Definition, second.Current.Battle.Actors[0].Definition);
        Assert.Equal(new[] { new ActorRef("medic-a"), new("guard-a"), new("dummy-a") }, second.Current.Battle.Actors.Select(a => a.Actor));
        Assert.NotSame(first.Current.Battle.Actors[0], second.Current.Battle.Actors[0]);
        Assert.Equal(new MapPosition(3, 3), first.Current.Battle.Actors[0].Position);
        Assert.Equal(new MapPosition(4, 3), second.Current.Battle.Actors[0].Position);
        Assert.Equal(new MapPosition(3, 3), encounter.Deployments[0].Position);
        Assert.Equal((70, 7, 40, 12, 3), ((int)second.Current.Battle.Actors[0].Hp, second.Current.Battle.Actors[0].Mp,
            second.Current.Battle.Actors[0].Exp!.Value, second.Current.Battle.Actors[0].Kills!.Value, second.Current.Battle.Actors[0].Defeats!.Value));
        Assert.Equal((uint)1234, second.Current.Battle.Gold);
        Assert.Equal(0x12344321u, second.Current.Battle.ThinkingSeed);
        foreach (var session in new[] { first, second })
        {
            for (int i = 0; session.Current.Battle.Round != 2 || session.Current.Selection?.Actor != new ActorRef("medic-a"); i++)
            { Assert.True(i < 8); Stay(session); }
            Assert.Equal(0x0A061234u, session.Current.Battle.MainSeed);
        }
        var secondBefore = second.Current;
        Accept(first, new Confirm());
        Accept(first, new SelectSpell(new("mend", 1)));
        Accept(first, new SelectTarget(new("medic-a")));
        var healed = Accept(first, new Confirm());
        Assert.Equal((100, 17, 10), ((int)healed.Snapshot.Battle.Actors[0].Hp, healed.Snapshot.Battle.Actors[0].Mp, healed.Snapshot.Battle.Actors[0].Exp!.Value));
        Assert.Equal(0x9E581234u, healed.Snapshot.Battle.MainSeed);
        Assert.Same(secondBefore, second.Current);
        var firstAfter = first.Current;
        Stay(second);
        Assert.Same(firstAfter, first.Current);
        Assert.Equal((70, 7, 40, 12, 3), ((int)second.Current.Battle.Actors[0].Hp, second.Current.Battle.Actors[0].Mp,
            second.Current.Battle.Actors[0].Exp!.Value, second.Current.Battle.Actors[0].Kills!.Value, second.Current.Battle.Actors[0].Defeats!.Value));
        Assert.Equal(frozenDeployments, JsonSerializer.Serialize(encounter.Deployments));
        Assert.Equal(frozenTerrain, encounter.Terrain);
        Assert.Equal(frozenSpells, encounter.Spells);
        Assert.Equal((ushort)95, admitted.Start.Actors[0].Hp);
    }

    [Fact]
    public void EncounterSelectionBindsAnotherDeploymentWithoutRebuildingDefinitions()
    {
        var document = Document();
        var alternate = document["encounters"]![0]!.DeepClone();
        alternate["id"] = "alternate-watch"; alternate["placements"]![0]!["x"] = 2;
        document["encounters"]!.AsArray().Add(alternate);
        var admitted = Assert.IsType<ScenarioReadAccepted>(Reader(document).Read());
        var alternateStart = new BattleStartInput("alternate-watch", admitted.Start.Actors, admitted.Start.MainSeed,
            admitted.Start.ThinkingSeed, admitted.Start.Gold);
        var first = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session;
        var second = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, alternateStart)).Session;
        Assert.Equal("yard-watch", first.Current.Battle.Definition.Encounter);
        Assert.Equal("alternate-watch", second.Current.Battle.Definition.Encounter);
        Assert.Equal(new MapPosition(2, 3), second.Current.Battle.Actors[0].Position);
        Assert.Same(admitted.Definition.Encounters["alternate-watch"], second.Current.Battle.Definition);
        Assert.Equal(first.Current.Battle.MainSeed, second.Current.Battle.MainSeed);
    }

    [Theory]
    [InlineData("encounter", "missing-encounter")]
    [InlineData("duplicate", "duplicate-start-actor")]
    [InlineData("resources", "numeric-range")]
    public void ReusableTypedStartStillValidatesBeforePublishingASecondSession(string change, string code)
    {
        var admitted = Admitted();
        var running = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session;
        var before = running.Current;
        var actors = admitted.Start.Actors.ToList();
        if (change == "duplicate") actors.Add(actors[0]);
        if (change == "resources") actors[0] = actors[0] with { Hp = 101 };
        var invalid = new BattleStartInput(change == "encounter" ? "absent" : admitted.Start.Encounter, actors,
            admitted.Start.MainSeed, admitted.Start.ThinkingSeed, admitted.Start.Gold);
        var failure = Assert.IsType<SessionStartFailed>(GameSession.Start(admitted.Definition, invalid));
        Assert.Equal((SessionFailureKind.ContentError, code), (failure.Failure.Kind, failure.Failure.Code));
        Assert.Same(before, running.Current);
    }

    [Theory]
    [InlineData("missing", "missing-start-actor")]
    [InlineData("duplicate", "duplicate-start-actor")]
    [InlineData("actor-ref", "missing-actor")]
    [InlineData("encounter-ref", "missing-encounter")]
    [InlineData("hp", "numeric-range")]
    [InlineData("mp", "numeric-range")]
    [InlineData("exp", "numeric-range")]
    [InlineData("kills", "numeric-range")]
    [InlineData("defeats", "numeric-range")]
    [InlineData("gold", "numeric-range")]
    [InlineData("missing-counter", "missing-field")]
    [InlineData("outside", "start-placement-bounds")]
    [InlineData("overlap", "occupied-placement")]
    [InlineData("old-definition-state", "unknown-or-duplicate-field")]
    public void RealContentBindingRejectsInvalidOrImplicitStartState(string change, string code)
    {
        var document = Document(); var actors = document["start"]!["actors"]!.AsArray();
        switch (change)
        {
            case "missing": actors.RemoveAt(0); break;
            case "duplicate": actors.Add(actors[0]!.DeepClone()); break;
            case "actor-ref": actors[0]!["actor"] = "absent"; break;
            case "encounter-ref": document["start"]!["encounter"] = "absent"; break;
            case "hp": actors[0]!["hp"] = 101; break;
            case "mp": actors[0]!["mp"] = 21; break;
            case "exp": actors[0]!["exp"] = 100; break;
            case "kills": actors[0]!["kills"] = 10000; break;
            case "defeats": actors[0]!["defeats"] = 10000; break;
            case "gold": document["start"]!["gold"] = 10000000; break;
            case "missing-counter": actors[0]!.AsObject().Remove("kills"); break;
            case "outside": actors[0]!["positionOverride"] = new JsonObject { ["x"] = 12, ["y"] = 3 }; break;
            case "overlap": actors[0]!["positionOverride"] = new JsonObject { ["x"] = 3, ["y"] = 5 }; break;
            case "old-definition-state": document["actors"]![0]!["hp"] = 95; break;
        }
        var rejected = Assert.IsType<ScenarioReadRejected>(Reader(document).Read());
        Assert.Equal((SessionFailureKind.ContentError, code), (rejected.Failure.Kind, rejected.Failure.Code));
    }
}
