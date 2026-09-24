using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ExplorationSessionTests
{
    [Theory]
    [InlineData("clear", 2, 2, false)]
    [InlineData("current", 2, 2, true)]
    [InlineData("reserved", 4, 3, true)]
    [InlineData("near-current", 4, 3, true)]
    [InlineData("axis-boundary", 2, 2, false)]
    [InlineData("mover-ignores", 4, 3, false)]
    [InlineData("nonblocking", 2, 2, false)]
    [InlineData("hidden", 4, 3, false)]
    [InlineData("retired", 2, 2, false)]
    [InlineData("non-door", 4, 3, false)]
    [InlineData("non-door-current", 2, 2, true)]
    public void SourceDoorChecksEntityObstructionBeforeCopyAndTraversal(string shape, int x, int y, bool blocked)
    {
        var source = Start("harbor-arrival");
        var original = source.Current.Exploration!;
        var words = new ushort[WorkingMapLayout.WordCount];
        bool door = !shape.StartsWith("non-door", StringComparison.Ordinal);
        words[y * 64 + x] = door ? (ushort)0xC400 : (ushort)0; // Closed traversal would return the origin.
        var layout = new WorkingMapLayout(words);
        var afterStep = new ProgramLocation("after-step", 0);
        var map = new ExplorationMapDefinition(new("door-yard"), layout,
            new OriginalMapTraversal([new(0, 0, 7, 7)]), [],
            [new(ExplorationEventKind.Step, x, y, null, afterStep, RequiredMarker: 0)],
            population: new(30, 128, 0, []),
            layoutEvents: new([new(new(x, y), new(0, 0, x, y, 1, 1))], [], new([])));
        var player = original.PlayerEntity with
        {
            Motion = EntityMotionState.At(new(x - 1, y), 3, 32) with
                { FlagsA = shape == "mover-ignores" ? (byte)0x80 : (byte)0xA0 },
        };
        var motion = EntityMotionState.At(new(x, y), 0, 32) with { FlagsA = 0x80 };
        motion = shape switch
        {
            "clear" or "retired" or "non-door" => motion with { X = 0x7000, Y = 0x7000, XDestination = 0x7000, YDestination = 0x7000 },
            "current" => motion with { XDestination = 0, YDestination = 0 },
            "reserved" => motion with { X = 0, Y = 0 },
            "near-current" => motion with { X = (short)(x * 384 + 255), Y = (short)(y * 384 - 255), XDestination = 0, YDestination = 0 },
            "axis-boundary" => motion with { X = (short)(x * 384 + 256), XDestination = 0, YDestination = 0 },
            "nonblocking" => motion with { FlagsA = 0x20 },
            _ => motion,
        };
        var other = new ExplorationEntity(new("blocker"), motion, shape != "hidden", Slot: player.Slot + 1);
        var world = new ExplorationState(map, layout, player.Entity, shape == "clear" ? [player] : [player, other], original.Party);
        var definition = new ScenarioDefinition("door-yard", source.Definition.Encounters.Values,
            exploration: new([map], [new StoryProgram("after-step", [new EndProgram()])]));
        var before = new SessionSnapshot(Guid.NewGuid(), 1, 1, new ActiveExploration(world), new StoryState([], null), SessionStopReason.PlayerInput);
        var result = ExplorationDispatcher.Submit(definition, before, new Move(ExplorationDirection.East));
        Assert.Null(result.Failure);
        Assert.Equal((byte)0, result.Snapshot.Exploration!.PlayerEntity.Motion.Facing);
        Assert.Equal(original.Party.MainSeed, result.Snapshot.Exploration.Party.MainSeed);
        Assert.Equal(before.Story.SimulationTick, result.Snapshot.Story.SimulationTick);
        if (blocked)
        {
            Assert.Equal("movement-blocked", Assert.Single(result.Observations).Kind);
            Assert.Same(layout, result.Snapshot.Exploration.Layout);
            Assert.Equal(player.Position, result.Snapshot.Exploration.PlayerEntity.Position);
            Assert.False(result.Snapshot.Exploration.PlayerEntity.Busy);
            Assert.Null(result.Snapshot.Story.Wait);
        }
        else
        {
            Assert.Equal(door ? new[] { "door-opened", "movement-started" } : new[] { "movement-started" }, result.Observations.Select(row => row.Kind));
            Assert.Equal(0, result.Snapshot.Exploration.Layout[x, y]);
            Assert.Equal(afterStep, Assert.IsType<EntityWait>(result.Snapshot.Story.Wait).AfterMotion);
            Assert.Equal(new(x, y), Assert.IsType<MoveEntityAbsolute>(result.Snapshot.Exploration.PlayerEntity.Actions!.Actions[0]).Position);
            // Return to the same approach in the now-open layout: no second door event.
            var repeat = new SessionSnapshot(before.SessionId, before.Revision, before.ObservationSequence,
                new ActiveExploration(result.Snapshot.Exploration.WithEntity(player)), before.Story, before.StopReason);
            var again = ExplorationDispatcher.Submit(definition, repeat, new Move(ExplorationDirection.East));
            Assert.Null(again.Failure);
            Assert.Equal("movement-started", Assert.Single(again.Observations).Kind);
        }
    }

    [Fact]
    public void ImmediatePlainAcknowledgementPreservesNpcPhaseAndOpenTextUntilExplicitClose()
    {
        var session = StartProgram("""
            [{"op":"close-portrait"},
             {"op":"motion","entity":"ferryman","wait":false,"actions":[{"op":"random-walk","x":2,"y":1,"radius":1}]},
             {"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"wait-text-input"},{"op":"end"}]
            """);
        var before = session.Current;
        Assert.True(before.CanWaitForText);
        var result = Accept(session, new Acknowledge(before.Story.Wait!.Token));
        Assert.Same(before.Exploration, session.Current.Exploration);
        Assert.Same(before.Story.TextWindow, session.Current.Story.TextWindow);
        Assert.Equal(0, session.Current.Story.SimulationTick);
        Assert.DoesNotContain(result.Observations, row => row.Kind is "simulation-tick" or "gameplay-wait");
    }

    [Theory]
    [InlineData(true, 0x12341234u, 0, 0xECAB1234u, 2, 2)]
    [InlineData(true, 0xC632A55Au, 2, 0x1091A55Au, 3, 1)]
    [InlineData(false, 0xC632A55Au, 2, 0xC632A55Au, 2, 1)]
    public void PlainInputWaitRunsOnlyEnabledEntityServiceAndAcceptingInputAddsNoPoll(
        bool enabled, uint seed, int delay, uint expectedSeed, int targetX, int targetY)
    {
        var session = StartProgram("""
            [{"op":"close-portrait"},{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"wait-text-input"},{"op":"close-text"},{"op":"wait-ticks","ticks":10},{"op":"end"}]
            """, document =>
        {
            document["world"]!["programs"]![0]!["entitiesRunning"] = enabled;
            document["battle"]!["start"]!["mainSeed"] = seed;
            document["world"]!["maps"]![0]!["entities"]![0]!["actions"] = System.Text.Json.Nodes.JsonNode.Parse($$"""
                [{"op":"wait","ticks":{{delay}}},{"op":"random-walk","x":2,"y":1,"radius":1}]
                """);
        });
        var initial = session.Current;
        var token = initial.Story.Wait!.Token;
        Assert.True(initial.CanWaitForText);
        Assert.Equal("explicit-text-wait-required", Send(session, new AdvanceSimulation(token, 60)).Failure!.Code);
        Assert.Equal("stale-or-wrong-wait", Send(session, new WaitForText(new(token.Value + 1))).Failure!.Code);
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(initial, session.Current);
        for (int tick = 0; tick <= delay; tick++)
        {
            var waited = Accept(session, new WaitForText(token));
            Assert.Equal("gameplay-wait", Assert.Single(waited.Observations).Kind);
            Assert.Equal(initial.Revision + tick + 1, session.Current.Revision);
            Assert.Equal(token, session.Current.Story.Wait!.Token);
        }
        var beforeAck = session.Current;
        Assert.Equal(expectedSeed, beforeAck.Exploration!.Party.MainSeed);
        var npc = beforeAck.Exploration.Entities[new("ferryman")];
        Assert.Equal((targetX * 384, targetY * 384), ((int)npc.Motion.XDestination, (int)npc.Motion.YDestination));
        var ack = Accept(session, new Acknowledge(token));
        Assert.Equal(beforeAck.Story.SimulationTick, session.Current.Story.SimulationTick);
        Assert.Equal(beforeAck.Exploration, session.Current.Exploration);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.Equal(10, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        Assert.DoesNotContain(ack.Observations, row => row.Kind is "simulation-tick" or "gameplay-wait");
        Assert.False(session.Current.CanWaitForText);
        // Subsequent sleep remains mandatory and separate from the input-first accepting poll.
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 10));
        Assert.Equal(beforeAck.Story.SimulationTick + 10, session.Current.Story.SimulationTick);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BranchAndCallCarryRealPortraitCloseRatherThanInferringItFromText(bool executeClose)
    {
        var session = StartProgram("""
            [{"op":"call","target":{"program":"portrait-tail","instruction":0}},
             {"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"close-text"},{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"wait-text-input"},{"op":"end"}]
            """, document =>
        {
            if (executeClose) document["start"]!["flags"]!.AsArray().Add(9);
            document["world"]!["programs"]!.AsArray().Add(System.Text.Json.Nodes.JsonNode.Parse("""
                {"id":"portrait-tail","instructions":[
                 {"op":"branch-flag","flag":9,"whenSet":false,"target":{"program":"portrait-tail","instruction":2}},
                 {"op":"close-portrait"},{"op":"end"}]}
                """));
        });
        Assert.Equal(executeClose, session.Current.CanWaitForText);
        Assert.Empty(session.Current.Story.Callers);
        if (executeClose) Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
        else
        {
            Assert.IsType<UnknownPortraitWindow>(session.Current.Story.PortraitWindow);
            var before = session.Current;
            Assert.Equal("text-input-unavailable", Send(session, new WaitForText(before.Story.Wait!.Token)).Failure!.Code);
            Assert.Same(before, session.Current);
        }
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("skip")]
    [InlineData("missing")]
    [InlineData("absent")]
    [InlineData("open")]
    public void PortraitLookupSkipAndTextOnlyClosePreserveTheirActualGate(string mode)
    {
        string prefix = mode == "unknown" ? "" : """{"op":"close-portrait"},""";
        string entity = mode == "skip" ? "null" : "\"ferryman\"";
        string secondEntity = mode == "open" ? "\"ferryman\"" : "null";
        var session = StartProgram("[" + prefix + $$"""
             {"op":"open-portrait","entity":{{entity}},"flags":192},
             {"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"close-text"},
             {"op":"open-portrait","entity":{{secondEntity}},"flags":0},
             {"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"wait-text-input"},{"op":"end"}]
            """, document => { if (mode != "missing") AddPortraitVisuals(document, mode == "absent" ? null : 7); });
        Assert.Equal(mode is "skip" or "absent", session.Current.CanWaitForText);
        if (mode == "open") Assert.Equal(new OpenPortraitWindow(7, 192), session.Current.Story.PortraitWindow);
        if (mode is "unknown" or "missing") Assert.IsType<UnknownPortraitWindow>(session.Current.Story.PortraitWindow);
    }

    [Fact]
    public void SingleTextAcknowledgementRunsItsExplicitCloseTailBeforeMandatorySleep()
    {
        var session = StartProgram("""
            [{"op":"close-portrait"},{"op":"open-portrait","entity":"ferryman","flags":128},
             {"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},
             {"op":"close-portrait"},{"op":"close-text"},{"op":"wait-ticks","ticks":10},{"op":"end"}]
            """, document => AddPortraitVisuals(document, 7));
        Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow);
        Assert.False(session.Current.CanWaitForText);
        var result = Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Assert.Equal(new[] { "ClosePortrait", "CloseText", "WaitProgramTicks" },
            result.Observations.Where(row => row.Kind == "program-instruction").Select(row => row.Detail));
        Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.Equal(0, session.Current.Story.SimulationTick);
        Assert.Equal(10, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
    }

    [Fact]
    public void PlainWaitKeepsEntityFailureAndRejectsStaleEnvelopes()
    {
        var session = StartProgram("""
            [{"op":"close-portrait"},
             {"op":"motion","entity":"ferryman","wait":false,"actions":[{"op":"native-call","symbol":"missing","source":"authored"}]},
             {"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"explicitWindows":true,"waitForAcknowledgement":false},
             {"op":"wait-text-input"},{"op":"end"}]
            """);
        var before = session.Current;
        var envelope = new CommandEnvelope(before.SessionId, before.Revision, null, new WaitForText(before.Story.Wait!.Token));
        Assert.Equal("stale-input", session.Submit(envelope with { ExpectedRevision = before.Revision - 1 }).Failure!.Code);
        Assert.Equal("wrong-actor", session.Submit(envelope with { Actor = new("outsider") }).Failure!.Code);
        Assert.Same(before, session.Current);
        var failed = session.Submit(envelope);
        Assert.Equal(SessionStopReason.Unsupported, failed.StopReason);
        Assert.Equal("entity-action-stopped", Assert.Single(failed.Observations).Kind);
        Assert.Equal(before.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.Equal(before.Exploration!.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal("session-stopped", Send(session, envelope.Command).Failure!.Code);
    }

    private static void AddPortraitVisuals(System.Text.Json.Nodes.JsonNode document, int? portrait)
    {
        object Raster(int width, int height)
        {
            byte[] data = new byte[width * height * 4];
            return new { width, height, format = "rgba8", data = Convert.ToBase64String(data),
                sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)) };
        }
        document["world"]!["maps"]![0]!["entities"]![0]!["sprite"] = 30;
        document["world"]!["presentation"] = System.Text.Json.JsonSerializer.SerializeToNode(new
        {
            maps = document["world"]!["maps"]!.AsArray().Select(map => new
            { map = map!["id"]!.GetValue<string>(), atlas = Raster(128, 320), scale = 1,
                blocks = Enumerable.Range(0, 1024).Select(_ => new int[9]).ToArray() }).ToArray(),
            sprites = new[] { new { sprite = 30, directions = Enumerable.Range(0, 3).Select(_ => Raster(48, 24)).ToArray(), portrait, speech = 0 } },
            portraits = new[] { new { portrait = 7, raster = Raster(64, 64) } },
        });
    }

    [Theory]
    [InlineData(0x12341234u, 0, 0xECAB1234u, 2, 2)]
    [InlineData(0xC632A55Au, 2, 0x1091A55Au, 3, 1)]
    public void PlayerWaitAdvancesOneEligibleOpportunityWithSourceWaitAndRandomWalkRules(
        uint seed, int waitTicks, uint afterSeed, int targetX, int targetY)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["battle"]!["start"]!["mainSeed"] = seed;
            document["world"]!["maps"]![0]!["entities"]![0]!["actions"] = System.Text.Json.Nodes.JsonNode.Parse($$"""
                [{"op":"wait","ticks":{{waitTicks}}},{"op":"random-walk","x":2,"y":1,"radius":1}]
                """);
        });
        var initial = session.Current;
        for (int tick = 1; tick <= waitTicks; tick++)
        {
            Accept(session, new WaitAtInput());
            Assert.Equal(tick, session.Current.Exploration!.Entities[new("ferryman")].Motion.WaitTimer);
            Assert.Equal(seed, session.Current.Exploration.Party.MainSeed);
        }
        var before = session.Current;
        var result = Accept(session, new WaitAtInput());
        var npc = session.Current.Exploration!.Entities[new("ferryman")];
        // Source LCG advances the high word; scaled range 4 selects South or East above.
        Assert.Equal(afterSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal((targetX * 384, targetY * 384), ((int)npc.Motion.XDestination, (int)npc.Motion.YDestination));
        Assert.Equal((768, 384), ((int)npc.Motion.X, (int)npc.Motion.Y));
        Assert.Equal(0, npc.Motion.WaitTimer);
        Assert.True(npc.Busy);
        Assert.Equal(SessionStopReason.PlayerInput, result.StopReason);
        Assert.Equal(before.Revision + 1, session.Current.Revision);
        Assert.Equal(before.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.Equal("gameplay-wait", Assert.Single(result.Observations).Kind);
        Assert.Equal(initial.SessionId, session.Current.SessionId);
        Assert.Equal(initial.Exploration!.PlayerEntity, session.Current.Exploration.PlayerEntity);
        Assert.Equal(initial.Exploration.Party.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        Accept(session, new WaitAtInput());
        npc = session.Current.Exploration.Entities[new("ferryman")];
        Assert.Equal((768 + (targetX - 2) * 96, 384 + (targetY - 1) * 96), ((int)npc.Motion.X, (int)npc.Motion.Y));
        Assert.Equal(afterSeed, session.Current.Exploration.Party.MainSeed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlayerWaitResolvesCompetingDestinationsInPhysicalSlotOrder(bool reverse)
    {
        var session = Start("harbor-arrival", document =>
        {
            var entities = document["world"]!["maps"]![0]!["entities"]!.AsArray();
            entities[0]!["actions"] = System.Text.Json.Nodes.JsonNode.Parse("""[{"op":"move","x":1,"y":0}]""");
            entities.Add(System.Text.Json.Nodes.JsonNode.Parse("""
                {"id":"other","position":{"x":4,"y":1},"facing":2,"speed":96,"visible":true,
                 "obstruction":true,"actions":[{"op":"move","x":-1,"y":0}]}
                """));
            if (reverse)
            {
                var first = entities[0]; entities.RemoveAt(0); entities.Add(first);
            }
        });
        var seed = session.Current.Exploration!.Party.MainSeed;
        Accept(session, new WaitAtInput());
        var firstNpc = session.Current.Exploration!.AllEntities[1];
        var secondNpc = session.Current.Exploration.AllEntities[2];
        Assert.Equal(reverse ? "other" : "ferryman", firstNpc.Entity.Value);
        Assert.Equal((1, 2), (firstNpc.Slot, secondNpc.Slot));
        Assert.Equal(1152, firstNpc.Motion.XDestination);
        Assert.Equal(1, firstNpc.ActionCursor);
        Assert.Equal(secondNpc.Motion.X, secondNpc.Motion.XDestination);
        Assert.Equal(0, secondNpc.ActionCursor);
        Accept(session, new WaitAtInput());
        var moved = session.Current.Exploration.AllEntities[1];
        Assert.Equal(firstNpc.Motion.X + (reverse ? -96 : 96), moved.Motion.X);
        Assert.Equal(0, session.Current.Exploration.AllEntities[2].ActionCursor);
        Assert.Equal(seed, session.Current.Exploration.Party.MainSeed);
    }

    [Fact]
    public void PlayerWaitThenMoveUsesChangedOccupancyAndRejectsAnotherWaitDuringPlayerMotion()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[{"op":"move","x":0,"y":-1}]},{"op":"end"}]
            """);
        var blocked = Accept(session, new Move(ExplorationDirection.East));
        Assert.Contains(blocked.Observations, row => row.Kind == "movement-blocked");
        Assert.Equal(new MapPosition(1, 1), session.Current.Exploration!.PlayerEntity.Position);
        var before = session.Current;
        foreach (SessionCommand wrong in new SessionCommand[]
        {
            new Acknowledge(new(1)), new CompletePresentation(new(1), PresentationCueKind.SoundWait),
        })
        {
            Assert.NotNull(Send(session, wrong).Failure);
            Assert.Same(before, session.Current);
        }
        Accept(session, new WaitAtInput());
        var moving = Accept(session, new Move(ExplorationDirection.East));
        Assert.Contains(moving.Observations, row => row.Kind == "movement-started");
        var pending = session.Current;
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(pending, session.Current);
        // Mandatory motion still requires the existing token-bound simulation command.
        Accept(session, new AdvanceSimulation(pending.Story.Wait!.Token, 600));
        Assert.Equal(new MapPosition(2, 1), session.Current.Exploration!.PlayerEntity.Position);
        Accept(session, new WaitAtInput());
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Theory]
    [InlineData("dialogue")]
    [InlineData("choice")]
    [InlineData("ticks")]
    [InlineData("audio")]
    [InlineData("open-text")]
    [InlineData("player-actions")]
    public void PlayerWaitRejectsOtherExplorationConsumersWithoutAdvancing(string consumer)
    {
        string instructions = consumer switch
        {
            "dialogue" => """{"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":null},""",
            "choice" => """{"op":"yes-no","flag":10},""",
            "ticks" => """{"op":"wait-ticks","ticks":2},""",
            "audio" => """{"op":"present","kind":"SoundWait","resource":null,"entity":null,"position":null},""",
            "open-text" => """{"op":"text-cursor","text":100},{"op":"show-text","mode":"continued","speaker":null,"waitForAcknowledgement":false},""",
            _ => """{"op":"motion","entity":"traveler","wait":false,"actions":[{"op":"move","x":0,"y":1}]},""",
        };
        var session = StartProgram("[" + instructions + """{"op":"end"}]""");
        var before = session.Current;
        var result = Send(session, new WaitAtInput());
        Assert.Equal("field-input-unavailable", result.Failure!.Code);
        Assert.Equal("text-input-unavailable", Send(session, new WaitForText(before.Story.Wait?.Token ?? new(1))).Failure!.Code);
        Assert.Empty(result.Observations);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void PlayerWaitRejectsBattleControlAndSceneWithoutGrantingBattleTicks()
    {
        var session = Start("stone-court");
        var battle = session.Current;
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(battle, session.Current);
        Assert.Equal("text-input-unavailable", Send(session, new WaitForText(new(1))).Failure!.Code);
        Accept(session, new Confirm());
        Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(new("raider")));
        Accept(session, new Confirm());
        var scene = session.Current;
        Assert.NotNull(scene.BattleScene);
        Assert.Equal("text-input-unavailable", Send(session, new WaitForText(scene.BattleScene!.Token)).Failure!.Code);
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Equal("invalid-battle-tick", Send(session, new AdvanceSimulation(scene.BattleScene!.Token)).Failure!.Code);
        Assert.Same(scene, session.Current);
    }

    [Fact]
    public void PlayerWaitUsesSessionRevisionAndDoesNotAdvanceForStaleOrWrongActorInput()
    {
        var session = Start("harbor-arrival");
        var before = session.Current;
        var envelope = new CommandEnvelope(before.SessionId, before.Revision, null, new WaitAtInput());
        Assert.Equal("stale-input", session.Submit(envelope with { SessionId = Guid.NewGuid() }).Failure!.Code);
        Assert.Equal("wrong-actor", session.Submit(envelope with { Actor = new("outsider") }).Failure!.Code);
        Assert.Same(before, session.Current);
        Assert.Null(session.Submit(envelope).Failure);
        var after = session.Current;
        Assert.Equal(before.Story.SimulationTick + 1, after.Story.SimulationTick);
        Assert.Equal(before.Exploration!.Party.MainSeed, after.Exploration!.Party.MainSeed);
        Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
        Assert.Same(after, session.Current);
    }

    [Fact]
    public void PlayerWaitPreservesEntityFailureAndCannotContinueTheStoppedConsumer()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[
              {"op":"native-call","symbol":"unimplemented-service","source":"authored"}]},{"op":"end"}]
            """);
        var before = session.Current;
        var result = Send(session, new WaitAtInput());
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure!.Kind);
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Equal(before.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.Equal(before.Exploration!.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal("entity-action-stopped", Assert.Single(result.Observations).Kind);
        var stopped = session.Current;
        Assert.Equal("session-stopped", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(stopped, session.Current);
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(63, false)]
    public void OrdinaryWarpPublishesOriginOnlyAfterItsDestinationIsAdmitted(int destinationX, bool accepted)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["maps"]![0]!["events"]!.AsArray().Add(System.Text.Json.Nodes.JsonNode.Parse($$"""
                {"kind":"warp","x":2,"y":1,"map":"quay","position":{"x":{{destinationX}},"y":2},
                 "facing":3,"marker":null,"requiredFlag":null,"requiredValue":true}
                """));
        });
        var before = session.Current;
        var result = Send(session, new Move(ExplorationDirection.East));
        if (accepted)
        {
            Assert.Null(result.Failure);
            Assert.Equal(new MapPosition(destinationX, 2), session.Current.Exploration!.PlayerEntity.Position);
            Assert.Equal(new[] { "warp-started", "map-transferred" }, result.Observations.Take(2).Select(row => row.Kind));
        }
        else
        {
            Assert.NotNull(result.Failure);
            Assert.Equal(before.Exploration!.PlayerEntity.Position, session.Current.Exploration!.PlayerEntity.Position);
            Assert.DoesNotContain(result.Observations, row => row.Kind == "warp-started");
        }
    }

    [Fact]
    public void OpenTextCanWaitForFiniteSoundAndPreviousMusicBeforeAcceptingInput()
    {
        var session = StartProgram("""
            [{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null,"waitForAcknowledgement":false},
             {"op":"present","kind":"SoundWait","resource":null,"entity":null,"position":null},
             {"op":"present","kind":"PreviousMusic","resource":null,"entity":null,"position":null},
             {"op":"wait-text-input"},{"op":"close-text"},
             {"op":"set-flag","flag":7,"value":true},{"op":"end"}]
            """);
        var sound = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.Equal(PresentationCueKind.SoundWait, sound.Cue.Kind);
        Assert.Equal(100, Assert.IsType<OpenTextWindow>(session.Current.Story.TextWindow).Text);
        var waiting = session.Current;
        Assert.NotNull(Send(session, new Acknowledge(sound.Token)).Failure);
        Assert.Same(waiting, session.Current);
        Accept(session, new CompletePresentation(sound.Token, PresentationCueKind.SoundWait));
        var previous = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.Equal(PresentationCueKind.PreviousMusic, previous.Cue.Kind);
        Assert.NotNull(Send(session, new Acknowledge(previous.Token)).Failure);
        Accept(session, new CompletePresentation(previous.Token, PresentationCueKind.PreviousMusic));
        var input = Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        Assert.Equal(100, input.Text);
        Assert.Equal(101, session.Current.Story.TextCursor);
        Assert.DoesNotContain(7, session.Current.Story.Flags);
        Accept(session, new Acknowledge(input.Token));
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.Contains(7, session.Current.Story.Flags);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Fact]
    public void WaitingForTextInputWithoutAnOpenWindowFailsAtThatInstruction()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = System.Text.Json.Nodes.JsonNode.Parse("""
                [{"op":"wait-text-input"},{"op":"end"}]
                """);
        });
        var result = Send(session, new Interact(new("ferryman")));
        Assert.Equal("text-input-without-window", result.Failure!.Code);
        Assert.Equal(new ProgramLocation("invitation", 0), session.Current.Story.Cursor);
    }

    [Theory]
    [InlineData("harbor-arrival", "ferryman", 2, 1, 3, 1)]
    [InlineData("hill-passage", "watcher", 3, 2, 3, 1)]
    public void DialogueMotionCallTransferAndBothIntroHooksReachExistingBattle(string package, string npc,
        int fromX, int fromY, int toX, int toY)
    {
        var session = Start(package);
        var identity = session.Current.SessionId;
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        var initialSeed = session.Current.Exploration!.Party.MainSeed;
        var dialogue = Accept(session, new Interact(new(npc)));
        var wait = Assert.IsType<DialogueWait>(dialogue.Snapshot.Story.Wait);
        Assert.Equal(1, session.Current.Story.Cursor!.Value.Instruction);
        Assert.DoesNotContain(10, session.Current.Story.Flags);
        Assert.Same(session.Current, Send(session, new Acknowledge(new(wait.Token.Value + 1))).Snapshot);
        Accept(session, new Acknowledge(wait.Token));
        var choice = Assert.IsType<ChoiceWait>(session.Current.Story.Wait);
        Accept(session, new ChooseDialogue(choice.Token, true));
        var motionWait = Assert.IsType<EntityWait>(session.Current.Story.Wait);
        Assert.Equal(new MapPosition(fromX, fromY), session.Current.Exploration!.Entities[new(npc)].Position);
        Accept(session, new AdvanceSimulation(motionWait.Token));
        var first = session.Current.Exploration.Entities[new(npc)].Motion;
        Assert.Equal((fromX * 384, fromY * 384), ((int)first.X, (int)first.Y));
        Assert.True(first.IsMoving);
        Assert.Equal(initialSeed, session.Current.Exploration.Party.MainSeed);
        for (int i = 0; session.Current.Story.Wait is EntityWait && i < 10; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait.Token));
        Assert.Equal(new MapPosition(toX, toY), session.Current.Exploration.Entities[new(npc)].Position);
        Assert.Contains(12, session.Current.Story.Flags);
        var timer = Assert.IsType<TickWait>(session.Current.Story.Wait);
        Assert.Equal(2, timer.Remaining);
        Accept(session, new AdvanceSimulation(timer.Token));
        Assert.Equal(1, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        var transferred = Accept(session, new AdvanceSimulation(timer.Token));
        Assert.Contains(transferred.Observations, observation => observation.Kind == "map-transferred");
        Assert.DoesNotContain(transferred.Observations, observation => observation.Kind == "warp-started");
        Assert.Contains(14, session.Current.Story.Flags);
        Assert.Contains(15, session.Current.Story.Flags);
        Assert.DoesNotContain(20, session.Current.Story.Flags);
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        var entered = Accept(session, new Acknowledge(session.Current.Story.Wait.Token));
        Assert.Equal(identity, session.Current.SessionId);
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Null(session.Current.Exploration);
        Assert.NotNull(session.Current.Selection);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.Equal(new[] { 10, 12, 13, 14, 15, 16, 20 }, session.Current.Story.Flags);
        string[] kinds = entered.Observations.Select(observation => observation.Kind).ToArray();
        Assert.True(Array.IndexOf(kinds, "battle-initialized") < Array.IndexOf(kinds, "battle-loaded"));
        Assert.True(Array.IndexOf(kinds, "battle-loaded") < Array.IndexOf(kinds, "round-started"));
        var flags = session.Current.Story.Flags;
        Stay(session);
        Assert.Equal(flags, session.Current.Story.Flags);
    }

    [Theory]
    [InlineData("harbor-arrival", "ferryman")]
    [InlineData("hill-passage", "watcher")]
    public void DecliningReturnsControlAndAReplayedChoiceCannotMutateFlags(string package, string npc)
    {
        var session = Start(package);
        var position = session.Current.Exploration!.PlayerEntity.Position;
        Accept(session, new Interact(new(npc)));
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        var token = session.Current.Story.Wait!.Token;
        Accept(session, new ChooseDialogue(token, false));
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal(position, session.Current.Exploration!.PlayerEntity.Position);
        Assert.Empty(session.Current.Story.Flags);
        var before = session.Current;
        Assert.NotNull(Send(session, new ChooseDialogue(token, true)).Failure);
        Assert.Same(before, session.Current);
    }

    [Fact]
    public void UnsupportedInstructionKeepsItsCursorAndEarlierCommittedFlag()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = System.Text.Json.Nodes.JsonNode.Parse("""
                [{"op":"set-flag","flag":7,"value":true},
                 {"op":"native-call","symbol":"unimplemented-service","source":"authored"},
                 {"op":"set-flag","flag":8,"value":true},{"op":"end"}]
                """);
        });
        var result = Send(session, new Interact(new("ferryman")));
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure!.Kind);
        Assert.Equal(new ProgramLocation("invitation", 1), session.Current.Story.Cursor);
        Assert.Equal(new[] { 7 }, session.Current.Story.Flags);
        Assert.Equal(SessionStopReason.Unsupported, session.Current.StopReason);
    }

    [Fact]
    public void BlockedMovementStillFacesTheObstacleWithoutRelocatingOrAdvancingRng()
    {
        var session = Start("harbor-arrival");
        var before = session.Current;
        var result = Accept(session, new Move(ExplorationDirection.East));
        Assert.Equal(before.Exploration!.PlayerEntity.Position, session.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal(before.Exploration.Party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Contains(result.Observations, observation => observation.Kind == "movement-blocked");
        Accept(session, new Interact(new("ferryman")));
    }

    [Fact]
    public void ContinuedTextSurvivesItsAcknowledgementAndTickWaitUntilExplicitClose()
    {
        var session = StartProgram("""
            [{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"continued","speaker":"ferryman","speakerFlags":192},
             {"op":"wait-ticks","ticks":2},{"op":"close-text"},
             {"op":"wait-ticks","ticks":1},{"op":"end"}]
            """);
        Assert.Equal(0, session.Current.Story.SimulationTick);
        var text = Assert.IsType<OpenTextWindow>(session.Current.Story.TextWindow);
        Assert.Equal(100, text.Text);
        Assert.Equal(192, text.SpeakerFlags);
        Assert.Equal(192, Assert.IsType<DialogueWait>(session.Current.Story.Wait).SpeakerFlags);
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Assert.Equal(text, session.Current.Story.TextWindow);
        Assert.Equal(0, session.Current.Story.SimulationTick);
        var wait = session.Current.Story.Wait!.Token;
        Accept(session, new AdvanceSimulation(wait, 600));
        Assert.Equal(2, session.Current.Story.SimulationTick);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.Equal(1, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        var stopped = session.Current;
        Assert.NotNull(Send(session, new AdvanceSimulation(wait)).Failure);
        Assert.Same(stopped, session.Current);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(3, session.Current.Story.SimulationTick);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Fact]
    public void SpriteRefreshRetainsItsCursorUntilTheMatchingPresentationCompletes()
    {
        var session = StartProgram("""
            [{"op":"set-flag","flag":7,"value":true},
             {"op":"motion","entity":"ferryman","wait":true,"actions":[
               {"op":"speed","x":32,"y":48},
               {"op":"acceleration","x":2,"y":3},
               {"op":"flags","field":"a","mask":128,"value":128},
               {"op":"sprite-size","size":24},
               {"op":"refresh-sprite","source":"authored/native-refresh"},
               {"op":"face","facing":2}]},
             {"op":"set-flag","flag":8,"value":true},{"op":"end"}]
            """);
        var result = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Null(result.Failure);
        Assert.Equal(new ProgramLocation("invitation", 1), session.Current.Story.Cursor);
        Assert.Equal(new[] { 7 }, session.Current.Story.Flags);
        Assert.Equal(1, session.Current.Story.SimulationTick);
        Assert.Equal(24, session.Current.Exploration!.SpriteSize);
        var entity = session.Current.Exploration.Entities[new("ferryman")];
        Assert.Equal(4, entity.ActionCursor);
        Assert.Equal((32, 48, 2, 3, 160), ((int)entity.Motion.XSpeed, (int)entity.Motion.YSpeed,
            (int)entity.Motion.XAcceleration, (int)entity.Motion.YAcceleration, (int)entity.Motion.FlagsA));
        Assert.True(entity.WaitingForSprite);
        var pending = session.Current;
        Assert.NotNull(Send(session, new EntitySpriteReady(entity.Slot, entity.SpriteRequest + 1)).Failure);
        Assert.Same(pending, session.Current);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 3));
        Assert.Equal(4, session.Current.Exploration!.Entities[new("ferryman")].ActionCursor);
        Assert.DoesNotContain(8, session.Current.Story.Flags);
        Accept(session, new EntitySpriteReady(entity.Slot, entity.SpriteRequest));
        Assert.NotNull(Send(session, new EntitySpriteReady(entity.Slot, entity.SpriteRequest)).Failure);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Contains(8, session.Current.Story.Flags);
        Assert.Equal(2, session.Current.Exploration.Entities[new("ferryman")].Motion.Facing);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Fact]
    public void NonblockingActionOperationsRunInOrderBeforeTheFirstMovementTick()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":true,"actions":[
               {"op":"speed","x":192,"y":192},
               {"op":"face","facing":3},{"op":"move","x":1,"y":0}]},
             {"op":"set-flag","flag":11,"value":true},{"op":"end"}]
            """);
        var initial = session.Current.Exploration!.Entities[new("ferryman")].Motion;
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var scheduled = session.Current.Exploration.Entities[new("ferryman")];
        Assert.Equal(initial.X, scheduled.Motion.X);
        Assert.Equal(initial.X + 384, scheduled.Motion.XDestination);
        Assert.Equal(3, scheduled.Motion.Facing);
        Assert.Equal(3, scheduled.ActionCursor);
        Assert.DoesNotContain(11, session.Current.Story.Flags);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(initial.X + 192, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(initial.X + 384, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Contains(11, session.Current.Story.Flags);
        Assert.Equal(3, session.Current.Story.SimulationTick);
    }

    [Fact]
    public void ReplacingAnActionStreamChangesTheRemainingPathWithoutSnappingCurrentMotion()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[
               {"op":"move","x":1,"y":0},{"op":"move","x":1,"y":0}]},
             {"op":"wait-ticks","ticks":1},
             {"op":"motion","entity":"ferryman","wait":true,"actions":[
               {"op":"face","facing":3}]},{"op":"end"}]
            """);
        var initial = session.Current.Exploration!.Entities[new("ferryman")].Motion;
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(initial.X, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 600));
        Assert.Equal(initial.X + 384, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Fact]
    public void SeenIntroFlagSkipsBothHooksWhileStillInitializingAndLoadingTheEncounter()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["start"]!["flags"] = System.Text.Json.Nodes.JsonNode.Parse("[20]");
        });
        Accept(session, new Interact(new("ferryman")));
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Accept(session, new ChooseDialogue(session.Current.Story.Wait!.Token, true));
        for (int ticks = 0; session.Current.Mode == SessionMode.Exploration && ticks < 30; ticks++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.DoesNotContain(15, session.Current.Story.Flags);
        Assert.DoesNotContain(16, session.Current.Story.Flags);
        Assert.Contains(20, session.Current.Story.Flags);
    }

    [Theory]
    [InlineData("dialogue")]
    [InlineData("choice")]
    [InlineData("ticks")]
    public void FreshBattleStartRetainsProgramControlUntilItsWaitCompletes(string waiting)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["start"]!["map"] = "yard-map";
            document["start"]!["flags"] = System.Text.Json.Nodes.JsonNode.Parse("[13]");
            document["world"]!["maps"]![1]!["battle"]!["before"] = null;
            string instructions = waiting switch
            {
                "dialogue" => """{"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":null}""",
                "choice" => """{"op":"yes-no","flag":10}""",
                _ => """{"op":"wait-ticks","ticks":2}""",
            };
            document["world"]!["programs"]!.AsArray().Single(program => program!["id"]!.GetValue<string>() == "battle-start")!["instructions"] =
                System.Text.Json.Nodes.JsonNode.Parse("[" + instructions + """,{"op":"set-flag","flag":16,"value":true},{"op":"end"}]""");
        });
        var pending = session.Current;
        Assert.Equal(SessionMode.Battle, pending.Mode);
        Assert.False(pending.HasBattleControl);
        Assert.Equal(0, pending.Battle.Round);
        Assert.Null(pending.Selection);
        Assert.DoesNotContain(16, pending.Story.Flags);
        Assert.Equal("program-owns-control", Send(session, new Confirm()).Failure!.Code);
        Assert.Same(pending, session.Current);
        Assert.Equal("field-input-unavailable", Send(session, new WaitAtInput()).Failure!.Code);
        Assert.Same(pending, session.Current);
        var token = pending.Story.Wait!.Token;
        SessionCommand command = waiting switch
        {
            "dialogue" => new Acknowledge(token),
            "choice" => new ChooseDialogue(token, true),
            _ => new AdvanceSimulation(token, 2),
        };
        Accept(session, command);
        Assert.Equal(pending.SessionId, session.Current.SessionId);
        Assert.True(session.Current.HasBattleControl);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.NotNull(session.Current.Selection);
        Assert.Contains(16, session.Current.Story.Flags);
        if (waiting == "choice") Assert.Contains(10, session.Current.Story.Flags);
        Accept(session, new Confirm());
        Assert.Equal(BattleSelectionStage.ActionChoice, session.Current.Selection!.Stage);
    }

    [Fact]
    public void SimulationBatchesConsumeElapsedTicksAndStopBeforeAcknowledgingTheNextDialogue()
    {
        var session = StartProgram("""
            [{"op":"wait-ticks","ticks":120},{"op":"text-cursor","text":100},
             {"op":"show-text","mode":"single","speaker":null},
             {"op":"set-flag","flag":7,"value":true},{"op":"end"}]
            """);
        var timer = session.Current.Story.Wait!.Token;
        Accept(session, new AdvanceSimulation(timer, 30));
        Assert.Equal(30, session.Current.Story.SimulationTick);
        Assert.Equal(90, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        Accept(session, new AdvanceSimulation(timer, 60));
        Assert.Equal(90, session.Current.Story.SimulationTick);
        Assert.Equal(30, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        Accept(session, new AdvanceSimulation(timer, 60));
        Assert.Equal(120, session.Current.Story.SimulationTick);
        var dialogue = Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        Assert.DoesNotContain(7, session.Current.Story.Flags);
        var pending = session.Current;
        Assert.Equal("stale-or-wrong-wait", Send(session, new AdvanceSimulation(timer, 30)).Failure!.Code);
        Assert.Same(pending, session.Current);
        Accept(session, new Acknowledge(dialogue.Token));
        Assert.Contains(7, session.Current.Story.Flags);
        Assert.Equal(120, session.Current.Story.SimulationTick);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BackgroundMotionConsumesTheBatchWithoutReleasingExistingInputOrDialogue(bool dialogue)
    {
        var session = StartProgram("""
            [{"op":"set-flag","flag":7,"value":true},
             {"op":"motion","entity":"ferryman","wait":false,"actions":[
               {"op":"speed","x":1,"y":1},{"op":"move","x":1,"y":0}]},
            """ + (dialogue ? """
             {"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":null},
            """ : "") + """{"op":"end"}]""");
        var wait = session.Current.Story.Wait;
        var expectedStop = dialogue ? SessionStopReason.PresentationWait : SessionStopReason.PlayerInput;
        Assert.Equal(expectedStop, session.Current.StopReason);
        Accept(session, new AdvanceSimulation(wait?.Token));
        var before = session.Current;
        var npc = before.Exploration!.Entities[new("ferryman")];
        var result = Accept(session, new AdvanceSimulation(wait?.Token, 60));
        Assert.Equal(before.Story.SimulationTick + 60, session.Current.Story.SimulationTick);
        Assert.Equal(npc.Motion.X + 60, session.Current.Exploration!.Entities[new("ferryman")].Motion.X);
        Assert.Equal(expectedStop, result.StopReason);
        Assert.Same(wait, session.Current.Story.Wait);
        Assert.Equal(before.SessionId, session.Current.SessionId);
        Assert.Equal(before.Story.Flags, session.Current.Story.Flags);
        Assert.Equal(before.Exploration.Party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(60, result.Observations.Count(observation => observation.Kind == "simulation-tick"));
        Accept(session, new AdvanceSimulation(wait?.Token, 30));
        Assert.Equal(before.Story.SimulationTick + 90, session.Current.Story.SimulationTick);
        Assert.Equal(npc.Motion.X + 90, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Equal(expectedStop, session.Current.StopReason);
    }

    [Fact]
    public void CompletingForegroundMotionYieldsAtNewFieldInputBeforeContinuingBackgroundTicks()
    {
        var session = StartProgram("""
            [{"op":"motion","entity":"ferryman","wait":false,"actions":[
               {"op":"speed","x":1,"y":1},{"op":"move","x":1,"y":0}]},
             {"op":"motion","entity":"traveler","wait":true,"actions":[
               {"op":"move","x":1,"y":0}]},{"op":"end"}]
            """);
        var startX = session.Current.Exploration!.Entities[new("ferryman")].Motion.X;
        var wait = Assert.IsType<EntityWait>(session.Current.Story.Wait).Token;
        Accept(session, new AdvanceSimulation(wait, 60));
        Assert.Equal(5, session.Current.Story.SimulationTick);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Null(session.Current.Story.Wait);
        Assert.Equal(startX + 4, session.Current.Exploration!.Entities[new("ferryman")].Motion.X);
        var boundary = session.Current;
        Assert.Equal("stale-or-wrong-wait", Send(session, new AdvanceSimulation(wait, 55)).Failure!.Code);
        Assert.Same(boundary, session.Current);
        Accept(session, new AdvanceSimulation(null, 2));
        Assert.Equal(7, session.Current.Story.SimulationTick);
        Assert.Equal(startX + 6, session.Current.Exploration.Entities[new("ferryman")].Motion.X);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    private static GameSession StartProgram(string instructions, Action<System.Text.Json.Nodes.JsonNode>? change = null) => Start("harbor-arrival", document =>
    {
        document["world"]!["programs"]![0]!["instructions"] = System.Text.Json.Nodes.JsonNode.Parse(instructions);
        document["start"]!["program"] = System.Text.Json.Nodes.JsonNode.Parse("""
            {"program":"invitation","instruction":0}
            """);
        change?.Invoke(document);
    });

    [Fact]
    public void UnsupportedTargetSetupKeepsTheSourceMapAndTheCompletedProgramPrefix()
    {
        var session = Start("harbor-arrival", document =>
        {
            document["world"]!["maps"]![1]!["setup"] = System.Text.Json.Nodes.JsonNode.Parse("""
                {"default":"base","variants":[{"flag":10,"setup":"closed"}]}
                """);
        });
        var originalMap = session.Current.Exploration!.Map;
        Accept(session, new Interact(new("ferryman")));
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Accept(session, new ChooseDialogue(session.Current.Story.Wait!.Token, true));
        SessionResult? result = null;
        for (int tick = 0; session.Current.StopReason == SessionStopReason.SimulationWait && tick < 100; tick++)
            result = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal("map-setup", result!.Failure!.Code);
        Assert.Equal(originalMap, session.Current.Exploration!.Map);
        Assert.Contains(13, session.Current.Story.Flags);
        Assert.DoesNotContain(14, session.Current.Story.Flags);
    }
}
