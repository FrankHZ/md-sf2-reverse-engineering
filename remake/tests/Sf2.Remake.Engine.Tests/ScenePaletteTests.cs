using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ScenePaletteTests
{
    private static GameSession StartPalette(string operations, bool black = false, string terminal = "end-scene", bool beforeBattle = false) =>
        ExplorationTextWaitTests.StartFieldText("A", configure: document =>
        {
            var colors = Enumerable.Repeat(0x444, 16).ToArray();
            byte[] indices = new byte[128 * 320];
            indices[0] = 2; indices[1] = 3;
            int map = beforeBattle ? 1 : 0;
            document["world"]!["presentation"]!["maps"]![map]!["scenePalette"] =
                JsonSerializer.SerializeToNode(new { indices = Convert.ToBase64String(indices), colors });
            foreach (var row in document["world"]!["maps"]!.AsArray())
                row!["basePalette"] = JsonNode.Parse("""{"color2":1092,"color3":1092}""");
            document["start"]!["display"] = JsonNode.Parse($$"""
                {"period":6,"base":{"color2":1092,"color3":1092},
                "current":{"color2":{{(black ? 0 : 1092)}},"color3":{{(black ? 0 : 1092)}}},
                "visibility":"{{(black ? "black" : "base-restored")}}"}
                """);
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse($"[{operations},{{\"op\":\"{terminal}\"}}]");
            if (beforeBattle)
            {
                document["start"]!["map"] = "yard-map";
                document["start"]!["flags"] = new JsonArray(13, 90, 91);
                document["start"]!.AsObject().Remove("program");
                document["world"]!["programs"]!.AsArray().Single(row => row!["id"]!.GetValue<string>() == "before-battle")!["instructions"] =
                    document["world"]!["programs"]![0]!["instructions"]!.DeepClone();
            }
        });

    [Fact]
    public void PulseChangesDistinctSlotsAndRestoresExactPriorBankAfterTenServices()
    {
        var colors = Enumerable.Repeat(0xEEE, 16).ToArray();
        colors[2] = 0x222; colors[3] = 0x888;
        var session = StartPalette(JsonSerializer.Serialize(new { op = "scene-palette-pulse", bank = 0, colors, ticks = 10 }));
        var before = session.Current;
        var wait = Assert.IsType<ScenePaletteWait>(before.Story.Wait);
        var scene = before.Story.Display!.Scene!;
        Assert.Equal(scene.Resource.Base[2], scene.Resource.Base[3]);
        Assert.NotEqual(scene.Terrain[scene.Resource.Slot(0, 0)], scene.Terrain[scene.Resource.Slot(1, 0)]);
        Assert.Equal(0, scene.ActorOffset);
        Assert.False(session.Current.CanWaitAtInput);
        Assert.False(session.QuerySave().Allowed);
        Assert.NotNull(Send(session, new CompletePresentation(wait.Token, PresentationCueKind.ScenePalette)).Failure);
        Assert.Same(before, session.Current);
        Accept(session, new AdvanceSimulation(wait.Token, 9));
        Assert.Same(scene, session.Current.Story.Display!.Scene);
        Accept(session, new AdvanceSimulation(wait.Token));
        Assert.Same(wait.Previous, session.Current.Story.Display!.Scene);
        Accept(session, new AdvanceSimulation(wait.Token, 10));
        Assert.True(Assert.IsType<ScenePaletteWait>(session.Current.Story.Wait).LogicalDone);
        Assert.NotNull(Send(session, new CompletePresentation(new(wait.Token.Value + 1), PresentationCueKind.ScenePalette)).Failure);
        Accept(session, new CompletePresentation(wait.Token, PresentationCueKind.ScenePalette));
        Assert.IsType<SceneEndWait>(session.Current.Story.Wait);
        Assert.Null(session.Current.Story.Display!.Scene);
        Assert.False(session.Current.CanWaitAtInput);
        Assert.False(session.QuerySave().Allowed);
        Assert.Same(before.Exploration!.Party, session.Current.Exploration!.Party);
        Assert.Equal(before.Story.SimulationTick + 20, session.Current.Story.SimulationTick);
    }

    [Theory]
    [InlineData(15, -2)]
    [InlineData(1, -7)]
    public void FadeUsesBaseBankAndSourcePeriodWhileOtherBanksFollowMask(int banks, int actorOffset)
    {
        var session = StartPalette($$"""{"op":"scene-palette-fade","banks":{{banks}},"offsets":[-7,-6,-5,-4,-3,-2],"period":6}""", black: true);
        var wait = Assert.IsType<ScenePaletteWait>(session.Current.Story.Wait);
        Assert.Equal(43, wait.Duration);
        Accept(session, new AdvanceSimulation(wait.Token, 35));
        Assert.Equal((ushort)0, session.Current.Story.Display!.Scene!.Terrain[2]);
        Accept(session, new AdvanceSimulation(wait.Token));
        Assert.Equal(ScenePaletteState.Offset(0x444, -2), session.Current.Story.Display!.Scene!.Terrain[2]);
        Assert.Equal(actorOffset, session.Current.Story.Display.Scene.ActorOffset);
        Accept(session, new AdvanceSimulation(wait.Token, 7));
        Assert.True(Assert.IsType<ScenePaletteWait>(session.Current.Story.Wait).LogicalDone);
        Assert.NotNull(Send(session, new AdvanceSimulation(wait.Token)).Failure);
        Accept(session, new CompletePresentation(wait.Token, PresentationCueKind.ScenePalette));
        Assert.IsType<SceneEndWait>(session.Current.Story.Wait);
    }

    [Fact]
    public void TransientPaletteCannotEscapeToOrdinaryFieldControl()
    {
        var session = StartPalette("""{"op":"scene-palette-fade","banks":1,"offsets":[-1,-2],"period":6}""", terminal: "end");
        var wait = Assert.IsType<ScenePaletteWait>(session.Current.Story.Wait);
        Accept(session, new AdvanceSimulation(wait.Token, wait.Duration));
        var result = Send(session, new CompletePresentation(wait.Token, PresentationCueKind.ScenePalette));
        Assert.Equal("field-display-not-visible", result.Failure!.Code);
        Assert.False(session.Current.CanWaitAtInput);
        Assert.False(session.QuerySave().Allowed);
    }

    [Fact]
    public void BattleEntrySkipRetainsPulseRestorationAndConsequentialFlag()
    {
        var colors = Enumerable.Repeat(0xEEE, 16).ToArray();
        var pulse = JsonSerializer.Serialize(new { op = "scene-palette-pulse", bank = 0, colors, ticks = 10 });
        var session = StartPalette(pulse + "," + """{"op":"set-flag","flag":777,"value":true}""", beforeBattle: true);
        var wait = Assert.IsType<ScenePaletteWait>(session.Current.Story.Wait);
        long tick = session.Current.Story.SimulationTick;
        Accept(session, new SkipBattleEntry());
        Accept(session, new AdvanceBattleEntrySkip());
        Assert.True(Assert.IsType<ScenePaletteWait>(session.Current.Story.Wait).LogicalDone);
        Assert.Equal(tick + 20, session.Current.Story.SimulationTick);
        Assert.Same(wait.Previous, session.Current.Story.Display!.Scene);
        Assert.DoesNotContain(777, session.Current.Story.Flags);
        Accept(session, new CompleteSkippedEntryPresentation(wait.Token, PresentationCueKind.ScenePalette));
        Assert.Contains(777, session.Current.Story.Flags);
        Assert.IsType<SceneEndWait>(session.Current.Story.Wait);
    }

    [Fact]
    public void BlackSceneBankCanBindNextSceneAfterStartingFromVisibleBase()
    {
        var session = StartPalette("""
            {"op":"scene-palette-fade","banks":15,"offsets":[-2,-3,-4,-5,-6,-7],"period":6},
            {"op":"scene-map","map":"yard-map","camera":{"x":0,"y":0}}
            """);
        var party = session.Current.Exploration!.Party;
        var wait = Assert.IsType<ScenePaletteWait>(session.Current.Story.Wait);
        Accept(session, new AdvanceSimulation(wait.Token, wait.Duration));
        Assert.Equal(FullFadeVisibility.BaseRestored, session.Current.Story.Display!.Visibility);
        Accept(session, new CompletePresentation(wait.Token, PresentationCueKind.ScenePalette));
        Assert.Equal("yard-map", session.Current.Exploration!.Map.Value);
        Assert.Same(party, session.Current.Exploration.Party);
        Assert.Equal(FullFadeVisibility.Black, session.Current.Story.Display!.Visibility);
        Assert.True(session.Current.Story.Display.Current.Black);
        Assert.IsType<SceneEndWait>(session.Current.Story.Wait);
    }
}
