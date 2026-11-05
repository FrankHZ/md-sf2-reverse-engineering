using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class PreVoyageSaveTests
{
    private static (ScenarioDefinition Definition, OpeningSaveData Data) Fixture()
    {
        var (original, data) = OpeningRestoreTests.Fixture(true, true);
        var old = original.Exploration!;
        var basis = old.Maps[new("map-57")];
        var map = new ExplorationMapDefinition(new("map-58"), basis.Layout, basis.Traversal, [], [],
            onLoad: new("dangerous-onload", 0),
            population: new(30, 128, 0, [new(74, 31, 31), new(74, 30, 30), new(74, 159, 59)]),
            basePalette: basis.BasePalette, viewAreas: basis.ViewAreas);
        var world = new ExplorationDefinition(old.Maps.Values.Append(map), old.Programs.Values,
            provenance: old.Provenance, partyFlags: old.PartyFlags, visuals: old.Visuals);
        var definition = new ScenarioDefinition(original.Package, original.Encounters.Values, exploration: world)
            { Persistence = new Sf2OpeningRestore() };
        return (definition, data with { Recipe = "battle07-return", Map = "map-58", X = 13, Y = 35, Facing = 3,
            Flags = data.Flags.Except([256, 401, 66]).Concat([74, 385, 506, 507, 640]).Order().ToArray(),
            EgressMap = "map-16", Names = new Dictionary<string, string> { ["ally-1"] = "Shelly" },
            Allies = data.Allies.Select(ally => ally.Actor == "ally-1" ? ally with { Hp = 0, Status = 3 } : ally).ToArray() });
    }

    [Fact]
    public void TowerAftermathCapturesAndRestoresProgressWithoutHealingOrReplayingTheScene()
    {
        var (definition, data) = Fixture();
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        Assert.True(session.Current.CanWaitAtInput);
        Assert.DoesNotContain(999, session.Current.Story.Flags);
        Assert.Equal(4, session.Current.Exploration!.CurrentEntities.Count);
        Assert.Equal(2, session.Current.Exploration.Party.ActiveAllies!.Count);
        Assert.Equal(0, session.Current.Exploration.Party.Actors.Single(a => a.Actor.Value == "ally-1").Hp);
        var captured = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(captured.Error);
        var bytes = OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UnixEpoch, captured.Data!));
        var decoded = OpeningSaveJson.Read(bytes, 1).Data;
        var again = Assert.IsType<SessionStarted>(GameSession.Restore(definition, decoded, session.Rules)).Session;
        var recaptured = again.CaptureSave(again.Current.SessionId, again.Current.Revision);
        Assert.Null(recaptured.Error);
        Assert.Equal(bytes, OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UnixEpoch, recaptured.Data!)));
        Assert.Equal(data.MainSeed, again.Current.Exploration!.Party.MainSeed);
        Assert.Equal(data.ThinkingSeed, again.Current.Exploration.Party.ThinkingSeed);
        Assert.Equal(data.Gold, again.Current.Exploration.Party.Gold);
        Assert.Equal("Shelly", again.Current.Story.Names["ally-1"]);
    }

    [Theory]
    [InlineData(74)] [InlineData(507)] [InlineData(640)]
    public void MissingAftermathPredicatesCannotSelectTheNewRecipe(int flag)
    {
        var (definition, data) = Fixture();
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Restore(definition,
            data with { Flags = data.Flags.Where(value => value != flag).ToArray() }, RuleCompositions.Sf2()));
        Assert.Equal("save-location-unavailable", failed.Failure.Code);
    }

    [Theory]
    [InlineData(407)] [InlineData(999)]
    public void ArmedBattleOrSubstitutedMapCannotRestoreTheTowerReturn(int flag)
    {
        var (definition, data) = Fixture();
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Restore(definition,
            data with { Flags = data.Flags.Append(flag).ToArray() }, RuleCompositions.Sf2()));
        Assert.Equal("save-location-unavailable", failed.Failure.Code);
    }
}
