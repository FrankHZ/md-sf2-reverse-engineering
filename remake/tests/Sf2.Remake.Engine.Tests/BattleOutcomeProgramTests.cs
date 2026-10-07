using Sf2.Remake.Domain.Gameplay.Sf2;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleOutcomeProgramTests
{
    [Fact]
    public void AiSeedSurvivesLastActionAndOutcomeWithoutRestoringHistoricalTextByte()
    {
        var decision = SourceEnemyAi.Resolve(SourceEnemyAiTests.State(thinking: 0x80ABCDEF), new("varied-source-enemy"));
        Assert.Equal(0x39ABCDEFu, decision.Battle.ThinkingSeed);
        // Feed the real AI result to the existing legal final-action/outcome fixture.
        var (_, completed, outcome) = WinningOutcome(3, false, decision.Battle.ThinkingSeed);
        Assert.Null(outcome.Failure);
        Assert.Equal(decision.Battle.ThinkingSeed, completed.Battle.ThinkingSeed);
        Assert.Equal(decision.Battle.ThinkingSeed, outcome.Snapshot.Exploration!.Party.ThinkingSeed);
        Assert.Equal((byte)0x39, outcome.Snapshot.CurrentRandomSeedCopy);
        Assert.Equal((byte)0xA9, outcome.Snapshot.Story.RandomSeedCopy);
    }

    [Theory]
    [InlineData(3, false)]
    [InlineData(5, true)]
    public void BoundLastActionInitializesOutcomeWindowsAndReturnsTheCarriedParty(int x, bool deadAlly)
    {
        var (definition, completed, outcome) = WinningOutcome(x, deadAlly);
        Assert.Null(outcome.Failure);
        var current = outcome.Snapshot;
        var world = current.Exploration!;
        Assert.Equal(new MapPosition(x, 3), world.PlayerEntity.Position);
        Assert.Equal(ExplorationViewRunner.Initialize(world), current.Story.LogicalView);
        Assert.IsType<ClosedPortraitWindow>(current.Story.PortraitWindow);
        Assert.False(current.Story.LogicalText!.Open);
        Assert.Null(current.Story.EntityServices);
        Assert.Equal(completed.Story.Display, current.Story.Display);
        Assert.Equal(completed.Battle.MainSeed, world.Party.MainSeed);
        Assert.Equal(completed.Battle.ThinkingSeed, world.Party.ThinkingSeed);
        Assert.Equal(completed.CurrentRandomSeedCopy, current.CurrentRandomSeedCopy);
        Assert.Equal((byte)0xA9, current.Story.RandomSeedCopy); // An older text write is not the carried live byte.
        Assert.NotEqual(current.Story.RandomSeedCopy, current.CurrentRandomSeedCopy);
        Assert.Equal(completed.Battle.Gold, world.Party.Gold);
        Assert.Equal(completed.Battle.GetActor(new("medic-a")).Exp, world.Party.Actors.Single(a => a.Actor.Value == "medic-a").Exp);
        foreach (var actor in completed.Battle.Actors)
        {
            var carried = world.Party.Actors.Single(a => a.Actor == actor.Actor);
            Assert.Equal(actor.Exp, carried.Exp);
            Assert.Equal(actor.Kills, carried.Kills);
            Assert.Equal(actor.Defeats, carried.Defeats);
            Assert.Equal(actor.Progress, carried.Progress);
            Assert.Equal(actor.SourceLoadout, carried.SourceLoadout);
        }
        Assert.Equal(deadAlly ? 0 : 12, world.Party.Actors.Single(a => a.Actor.Value == "guard-a").Hp);
        Assert.Equal(ProgramContinuation.VictoryProgramFinished, current.Story.Continuation);
        Assert.Equal(new FieldReturnAnchor(world.Map, new(x, 3), 3), current.Story.OutcomeReturn);
        var events = outcome.Observations.ToList();
        var afterText = false; var onLoadText = false; var registered = false;
        for (int guard = 0; current.StopReason != SessionStopReason.PlayerInput && guard < 2000; guard++)
        {
            Assert.NotNull(current.Story.EnteringBattle);
            Assert.False(current.CanWaitAtInput);
            registered |= current.Story.PortraitWindow is OpenPortraitWindow { Work.Registered: true };
            if (current.CanWaitForText)
            {
                if (current.Story.Continuation == ProgramContinuation.VictoryProgramFinished) afterText = true;
                if (current.Story.Continuation == ProgramContinuation.OutcomeMapLoaded)
                {
                    onLoadText = true;
                    Assert.Contains(new ProgramLocation("return-field", 2), current.Story.Callers);
                    Assert.Contains(501, current.Story.Flags);
                    Assert.DoesNotContain(401, current.Story.Flags);
                }
            }
            var wait = current.Story.Wait!;
            SessionCommand command = wait switch
            {
                W1TextWait { Revealed: false } or FieldTextWait { Revealed: false } => new CompleteTextReveal(wait.Token),
                W1TextWait or FieldTextWait when current.CanWaitForText => new Acknowledge(wait.Token),
                FullFadeWait { LogicalDone: true } fade => new CompletePresentation(fade.Token, fade.Kind),
                _ => new AdvanceSimulation(wait.Token),
            };
            if (command is CompletePresentation receipt)
            {
                var rejected = ExplorationDispatcher.Submit(definition, current, receipt with { Wait = new(receipt.Wait.Value + 1) });
                Assert.NotNull(rejected.Failure); Assert.Same(current.Active, rejected.Snapshot.Active);
                Assert.Equal(current.Story, rejected.Snapshot.Story);
            }
            var result = ExplorationDispatcher.Submit(definition, current, command);
            Assert.Null(result.Failure);
            events.AddRange(result.Observations); current = result.Snapshot;
        }
        Assert.True(afterText && onLoadText && registered);
        Assert.True(current.CanWaitAtInput);
        Assert.Null(current.Story.EnteringBattle);
        Assert.Empty(current.Story.Callers);
        Assert.Equal(FullFadeVisibility.BaseRestored, current.Story.Display!.Visibility);
        Assert.Equal(current.Story.Display.Base, current.Story.Display.Current);
        var fieldDraws = events.Where(row => row.Kind.StartsWith("rng-", StringComparison.Ordinal)).ToArray();
        Assert.Contains(fieldDraws, row => row.Kind.StartsWith("rng-portrait-", StringComparison.Ordinal));
        Assert.Contains(fieldDraws, row => row.Kind.StartsWith("rng-text-", StringComparison.Ordinal));
        Assert.Equal(world.Party.MainSeed, fieldDraws[0].Before);
        for (int index = 1; index < fieldDraws.Length; index++)
            Assert.Equal(fieldDraws[index - 1].After, fieldDraws[index].Before);
        Assert.Equal(fieldDraws[^1].After, current.Exploration!.Party.MainSeed);
        var lastTextDraw = fieldDraws.Last(row => row.Kind.StartsWith("rng-text-", StringComparison.Ordinal));
        Assert.Equal((world.Party.ThinkingSeed & 0x00FFFFFFu) | ((uint)lastTextDraw.RandomValue!.Value << 24),
            current.Exploration.Party.ThinkingSeed);
        Assert.Equal((byte)lastTextDraw.RandomValue.Value, current.CurrentRandomSeedCopy);
        Assert.Equal(current.Story.RandomSeedCopy, current.CurrentRandomSeedCopy);
        Assert.Equal(world.Party.Gold, current.Exploration.Party.Gold);
        foreach (var actor in world.Party.Actors)
        {
            var returned = current.Exploration.Party.Actors.Single(a => a.Actor == actor.Actor);
            Assert.Equal(actor.Exp, returned.Exp);
            Assert.Equal(actor.Kills, returned.Kills);
            Assert.Equal(actor.Defeats, returned.Defeats);
            Assert.Equal(actor.Progress, returned.Progress);
            Assert.Equal(actor.SourceLoadout, returned.SourceLoadout);
        }
        Assert.Equal(12, current.Exploration!.Party.Actors.Single(a => a.Actor.Value == "guard-a").Hp); // Source reset-all revived it.
        var ordered = events.Where(row => row.Kind is "after-battle-join" or "battle-unlock-cleared" or "battle-completed-set" or "battle-returned").Select(row => row.Kind);
        Assert.Equal(new[] { "after-battle-join", "battle-unlock-cleared", "battle-completed-set", "battle-returned" }, ordered);
        var party = current.Exploration.Party;
        var moved = ExplorationDispatcher.Submit(definition, current, new Move(ExplorationDirection.West));
        Assert.Null(moved.Failure); current = moved.Snapshot;
        while (current.Story.Wait is EntityWait)
        {
            moved = ExplorationDispatcher.Submit(definition, current, new AdvanceSimulation(current.Story.Wait.Token, 60));
            Assert.Null(moved.Failure); current = moved.Snapshot;
        }
        Assert.Equal(new MapPosition(x - 1, 3), current.Exploration!.PlayerEntity.Position);
        Assert.Equal(party.Actors, current.Exploration.Party.Actors);
        Assert.Equal(party.Gold, current.Exploration.Party.Gold);
    }

    [Theory]
    [InlineData("missing-route")]
    [InlineData("missing-outcome")]
    [InlineData("missing-anchor")]
    [InlineData("battle-load")]
    [InlineData("active-battle")]
    public void BoundOutcomeContextRejectsIncompleteOrWrongOwnershipBeforeWindowPublication(string shape)
    {
        var (definition, completed, result) = WinningOutcome(3, false);
        var current = result.Snapshot;
        var story = current.Story.Copy(new("after", 0),
            continuation: shape == "battle-load" ? ProgramContinuation.BattleLoadFinished : ProgramContinuation.VictoryProgramFinished,
            clearEnteringBattle: shape == "missing-route",
            enteringBattle: shape == "missing-outcome" ? current.Story.EnteringBattle! with { Outcome = null } : null);
        if (shape == "missing-anchor") story = new(story.Flags, story.Cursor, continuation: story.Continuation,
            enteringBattle: story.EnteringBattle, textSettings: story.TextSettings, logicalText: story.LogicalText,
            logicalView: story.LogicalView, portraitWindow: story.PortraitWindow, display: story.Display);
        var input = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            shape == "active-battle" ? completed.Active : current.Active, story, current.StopReason);
        Assert.Equal("field-text-context", Assert.Throws<BattleRuleException>(() => ExplorationTextRunner.ValidateContext(input)).Code);
        var observations = new List<SessionObservation>();
        Assert.Equal("field-portrait-context", Assert.Throws<BattleRuleException>(() =>
            ExplorationPortraitRunner.Open(definition, input, new("ferryman"), 0, observations, false)).Code);
        Assert.Empty(observations);
        Assert.Same(story, input.Story);
    }

    private static (ScenarioDefinition Definition, SessionSnapshot Completed, SessionResult Outcome) WinningOutcome(int x, bool deadAlly, uint? thinkingSeed = null)
    {
        var field = ExplorationTextWaitTests.StartFieldText("A{W1}", configure: document =>
        {
            document["world"]!["presentation"]!["sprites"]![0]!["portrait"] = 7;
            document["world"]!["maps"]![0]!["basePalette"] = JsonNode.Parse("""{"color2":546,"color3":1092}""");
        });
        var source = Start("harbor-arrival", document =>
        {
            document["start"]!["map"] = "yard-map";
            document["start"]!["flags"] = new JsonArray(13);
            var physical = Document("stone-court");
            for (int index = 0; index < 3; index++)
                document["battle"]!["actors"]![index]!["physical"] = physical["actors"]![index]!["physical"]!.DeepClone();
            document["battle"]!["encounters"]![0]!["rewards"] = physical["encounters"]![0]!["rewards"]!.DeepClone();
        });
        Accept(source, new Acknowledge(source.Current.Story.Wait!.Token));
        var basis = source.Current.Battle;
        var d = basis.Definition;
        var battleDefinition = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, d.Deployments,
            d.Spells.Values, d.Rewards, d.Initialization, new(new("medic-a"), new("dummy-a")), d.HealingItems.Values);
        var actors = basis.Actors.Select(actor => actor.Actor.Value switch
        {
            "medic-a" => actor.With(position: new(x, 3), hp: 4, mp: 2, exp: 9),
            "guard-a" => actor.With(hp: deadAlly ? (ushort)0 : (ushort)9),
            _ => actor.With(position: new(x + 1, 3), hp: 1),
        });
        var battle = new EngineBattleState(battleDefinition, actors, 0x12341234, thinkingSeed ?? basis.ThinkingSeed, basis.Round, basis.Queue, basis.Cursor, 73);
        var world = field.Definition.Exploration!;
        var map = world.Maps[new("quay")];
        var returnMap = new ExplorationMapDefinition(map.Map, map.Layout, map.Traversal, map.Entities, [],
            onLoad: new("return-onload", 0), basePalette: map.BasePalette, viewAreas: map.ViewAreas);
        StoryProgram[] programs = [
            new("after", [new WaitProgramTicks(1), new ResetPartyBattleStats(), new PresentCue(PresentationCueKind.FadeOut, "black"),
                new LoadSceneMap(map.Map, new(0, 0)), new PresentCue(PresentationCueKind.FadeIn, "black"),
                new OpenPortrait(new("ferryman")), new SetTextCursor(100), new ShowText(TextDisplayMode.Single, new("ferryman"), ExplicitWindows: true),
                new ClosePortrait(), new CloseText(), new EndProgram(true)]),
            new("return-field", [new PresentCue(PresentationCueKind.FadeOut, "black"), new ReturnBattleMap(),
                new PresentCue(PresentationCueKind.FadeIn, "black"), new EndProgram()]),
            new("return-onload", [new SetTextCursor(101), new ShowText(TextDisplayMode.Single, null, 255, ExplicitWindows: true),
                new CloseText(), new EndProgram(true)]), new("empty", [new EndProgram()])];
        var exploration = new ExplorationDefinition([returnMap], programs, world.Texts, partyFlags: new(2, 0, 32, 2),
            visuals: world.Visuals, memberNames: world.MemberNames, textTokens: world.TextTokens, textFont: world.TextFont);
        var definition = new ScenarioDefinition("bound-outcome", [battleDefinition], exploration: exploration);
        var route = new ExplorationBattleRoute(d.Encounter, 401, 501, 451, null, null, Outcome:
            new(new("after", 0), 0, new("empty", 0), new("empty", 0), new("return-field", 0), map.Map, 3, map.Map, new(1, 1), 1));
        var story = field.Current.Story.Copy(null, flags: [0, 32, 399, 401, 451], enteringBattle: route,
            entityServices: false, display: new(3, map.BasePalette!, map.BasePalette!, FullFadeVisibility.BaseRestored),
            logicalView: field.Current.Story.LogicalView! with { TargetSlot = 9, FollowCounter = 11 },
            logicalText: field.Current.Story.LogicalText! with { Open = true }, randomSeedCopy: 0xA9);
        var input = new SessionSnapshot(field.Current.SessionId, field.Current.Revision, field.Current.ObservationSequence,
            new ActiveBattle(battle, null), story, SessionStopReason.SimulationWait);
        var prepared = PhysicalBattleAction.PrepareSourceDefault(battle, new("medic-a"), new(x, 3), new("dummy-a"));
        var result = BattleSceneContinuation.Begin(input, prepared, []);
        while (result.Snapshot.BattleScene is { } scene)
        {
            Assert.Equal(SessionMode.Battle, result.Snapshot.Mode);
            result = BattleSceneContinuation.Submit(result.Snapshot, scene.RequiresAcknowledgement
                ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(result.Failure);
        }
        // Match GameSession: battle dispatch preserves the carried field story.
        result = result with { Snapshot = result.Snapshot.WithStory(story) };
        Assert.Equal(BattleOutcomeKind.Victory, BattleOutcomeRules.Check(result.Snapshot.Battle));
        return (definition, result.Snapshot, BattleOutcome.Begin(definition, result));
    }

    [Theory]
    [InlineData("normal", true)]
    [InlineData("live-alias", true)]
    [InlineData("wrong-map", false)]
    [InlineData("wrong-cursor", false)]
    [InlineData("active-window", false)]
    [InlineData("field-input", false)]
    [InlineData("missing-flag", false)]
    [InlineData("missing-record", false)]
    [InlineData("visible-record", false)]
    public void PostMessengerScratchRequiresTheExactNormalReloadAndRealEntityRetirement(string shape, bool accepted)
    {
        var source = Start("harbor-arrival");
        var original = source.Current.Exploration!;
        var map = new ExplorationMapDefinition(new(shape == "wrong-map" ? "elsewhere" : "map-3"),
            original.Layout, original.Definition.Traversal, [], []);
        var actor = new ExplorationEntity(new("entity-142"), EntityMotionState.At(new(2, 2), 3, 32), true);
        var world = new ExplorationState(map, original.Layout, original.Player, [original.PlayerEntity, actor], original.Party);
        if (shape is not ("live-alias" or "visible-record")) world = world.Hide(world.Entities[actor.Entity], removeAliases: true);
        if (shape == "visible-record")
            world = new(map, original.Layout, original.Player, world.AllEntities, original.Party,
                aliases: new Dictionary<EntityRef, int> { [original.Player] = original.PlayerEntity.Slot });
        if (shape == "missing-record") world = new(map, original.Layout, original.Player, [original.PlayerEntity], original.Party);
        string id = shape == "wrong-cursor" ? "another-call" : "byte-513a8";
        var program = new StoryProgram(id, [new EndProgram(), new RetiredMap3EntityScratch(), new EndProgram()]);
        var definition = new ScenarioDefinition("scratch-context", source.Definition.Encounters.Values,
            exploration: new([map], [program]));
        var flags = shape == "missing-flag" ? new[] { 1 } : shape == "live-alias" ? new[] { 603 } : new[] { 1, 603 };
        var story = new StoryState(flags, new(id, 1), continuation: shape == "field-input" ? ProgramContinuation.FieldInput : ProgramContinuation.MapLoaded,
            textWindow: shape == "active-window" ? new OpenTextWindow(1, TextDisplayMode.Single, null) : new ClosedTextWindow());
        var before = new SessionSnapshot(Guid.NewGuid(), 1, 1, new ActiveExploration(world), story, SessionStopReason.SimulationWait);
        var result = ProgramRunner.Run(definition, before, []);
        if (!accepted)
        {
            Assert.Equal("inactive-window-scratch-context", result.Failure!.Code);
            Assert.Same(world, result.Snapshot.Exploration);
            Assert.Equal(story.Flags, result.Snapshot.Story.Flags);
            return;
        }
        Assert.Null(result.Failure);
        var after = result.Snapshot.Exploration!;
        Assert.Same(world.Layout, after.Layout);
        Assert.Equal(original.PlayerEntity.Position, after.PlayerEntity.Position);
        Assert.Equal(flags, result.Snapshot.Story.Flags);
        if (shape == "normal")
        {
            Assert.Same(world, after);
            Assert.False(after.TryResolveEntity(new("entity-142"), out _));
        }
        else Assert.True(after.TryResolveEntity(new("entity-142"), out _));
        var hidden = after.AllEntities.Single(row => row.Entity.Value == "entity-142");
        Assert.False(hidden.Visible); Assert.Equal(0x7000, hidden.Motion.X); Assert.Equal(0x7000, hidden.Motion.Y);
    }

    [Fact]
    public void SpriteChangeRetainsTheEntityIdentityAndWaitsForItsExactRendererRequest()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"sprite","entity":"ferryman","sprite":60},{"op":"end"}]
                """);
            document["start"]!["program"] = JsonNode.Parse("""{"program":"invitation","instruction":0}""");
        });
        var before = session.Current;
        var wait = Assert.IsType<EntitySpriteWait>(before.Story.Wait);
        var entity = before.Exploration!.Entities[new("ferryman")];
        Assert.Equal(60, entity.Sprite);
        Assert.True(entity.WaitingForSprite);
        Assert.NotNull(Send(session, new Acknowledge(wait.Token)).Failure);
        Assert.NotNull(Send(session, new EntitySpriteReady(wait.Slot, wait.Request + 1)).Failure);
        Accept(session, new EntitySpriteReady(wait.Slot, wait.Request));
        var after = session.Current.Exploration!.Entities[new("ferryman")];
        Assert.Equal(entity.Entity, after.Entity); Assert.Equal(entity.Position, after.Position);
        Assert.False(after.WaitingForSprite); Assert.Null(session.Current.Story.Wait);
    }
}
