using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PrivateExplorationTests
{
    private const string World = "SF2_PRIVATE_EXPLORATION_CONTENT";
    private static GameSession StartSource(string start, bool opening = false)
    {
        var source = new PrivateExplorationReader(PrivateInputFactAttribute.RequireInput(World),
            Path.Combine(AppContext.BaseDirectory, "controlled", start + ".json"), PrivateBattleScenarioTests.Selected(opening ? Path.Combine(AppContext.BaseDirectory, "controlled", "map3-opening-party.json") : null));
        var read = source.Read();
        Assert.True(read is ExplorationReadAccepted, read is ScenarioReadRejected rejected ? rejected.Failure.ToString() : "world admission");
        var admitted = (ExplorationReadAccepted)read;
        var outcome = GameSession.Start(admitted.Definition, admitted.Start);
        Assert.True(outcome is SessionStarted, outcome is SessionStartFailed failed ? failed.Failure.ToString() : "session start");
        return ((SessionStarted)outcome).Session;
    }
    private static ExplorationEntity Entity(GameSession session, int id) => session.Current.Exploration!.Entities[new("entity-" + id)];
    private static SessionResult RunUntilStop(GameSession session, int limit = 3000,
        List<SessionObservation>? observations = null, List<(int Id, int? Speaker)>? texts = null, bool yes = true)
    {
        SessionResult? result = null;
        for (int index = 0; index < limit && session.Current.StopReason is SessionStopReason.SimulationWait or SessionStopReason.PresentationWait; index++)
        {
            foreach (var entity in session.Current.Exploration?.AllEntities.Where(entity => entity.WaitingForSprite && entity.SpriteReady != entity.SpriteRequest).ToArray() ?? [])
            {
                var mounted = Accept(session, new EntitySpriteReady(entity.Slot, entity.SpriteRequest));
                observations?.AddRange(mounted.Observations);
            }
            var waiting = session.Current.Story.Wait;
            if (waiting is DialogueWait text)
                texts?.Add((text.Text, text.Speaker is { } speaker ? int.Parse(speaker.Value[7..], System.Globalization.CultureInfo.InvariantCulture) | text.SpeakerFlags << 8 : null));
            if (waiting is PresentationWait { Cue.Kind: PresentationCueKind.Gesture } gesture)
            {
                Assert.Equal(255, session.Current.Exploration!.Entities[gesture.Cue.Entity!.Value].Motion.AnimationCounter);
                Assert.NotNull(Send(session, new Acknowledge(gesture.Token)).Failure);
            }
            // A typed test presentation port; native Godot observations separately prove drawing/playback.
            SessionCommand command = waiting switch
            {
                DialogueWait dialogue => new Acknowledge(dialogue.Token),
                ChoiceWait choice => new ChooseDialogue(choice.Token, yes),
                PresentationWait cue => new CompletePresentation(cue.Token, cue.Cue.Kind),
                _ => new AdvanceSimulation(waiting?.Token),
            };
            result = Send(session, command);
            observations?.AddRange(result.Observations);
            if (result.Failure is not null) return result;
        }
        Assert.NotNull(result);
        Assert.NotEqual(SessionStopReason.SimulationWait, session.Current.StopReason);
        return result;
    }

    private static JsonDocument Fixture(string name) => JsonDocument.Parse(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "fixtures", "opening-" + name + ".json")));
    private static JsonElement Record(JsonDocument fixture) => fixture.RootElement.GetProperty("expectedObservation").GetProperty("records")[0];

    private static GameSession RunOpening(bool yes, List<SessionObservation> observations, List<(int Id, int? Speaker)> texts)
    {
        var session = StartSource("map3-opening-start", opening: true);
        using var r1 = Fixture("r1");
        var state = Record(r1).GetProperty("scenarioState");
        var player = state.GetProperty("playerEntity");
        Assert.Equal(player.GetProperty("x").GetInt16(), Entity(session, 0).Motion.X);
        Assert.Equal(player.GetProperty("y").GetInt16(), Entity(session, 0).Motion.Y);
        Assert.Equal(player.GetProperty("facing").GetByte(), Entity(session, 0).Motion.Facing);
        Assert.Equal(0u, session.Current.Exploration!.Party.Gold);
        Assert.Equal(2568421376u, session.Current.Exploration.Party.MainSeed);
        foreach (string name in new[] { "joinedFlags", "activeFlags" })
            Assert.Equal(state.GetProperty(name).EnumerateArray().Select(flag => flag.GetBoolean()),
                Enumerable.Range(name == "joinedFlags" ? 0 : 32, 30).Select(session.Current.Story.Flags.Contains));
        Assert.Equal(new[] { 0, 2, 5 }, Enumerable.Range(0, 3).Select(id => Entity(session, id).Sprite!.Value));
        var identity = session.Current.SessionId;
        using var r2 = Fixture("r2");
        foreach (var edge in Record(r2).GetProperty("logicalInputTrace").EnumerateArray())
        {
            Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
            Assert.Equal(new MapPosition(edge.GetProperty("x").GetInt32(), edge.GetProperty("y").GetInt32()), Entity(session, 0).Position);
            var input = edge.GetProperty("input").GetString();
            SessionCommand command;
            if (input == "C")
            {
                var leader = Entity(session, 0);
                (int dx, int dy) = leader.Motion.Facing switch { 0 => (1, 0), 1 => (0, -1), 2 => (-1, 0), _ => (0, 1) };
                var target = session.Current.Exploration!.Entities.Values.Single(entity => entity.Visible && entity.Entity != leader.Entity &&
                    entity.Position.X == leader.Position.X + dx && entity.Position.Y == leader.Position.Y + dy);
                command = new Interact(target.Entity);
            }
            else command = new Move(input switch { "Left" => ExplorationDirection.West, "Right" => ExplorationDirection.East,
                "Up" => ExplorationDirection.North, "Down" => ExplorationDirection.South, _ => throw new InvalidOperationException() });
            observations.AddRange(Accept(session, command).Observations);
            if (session.Current.StopReason != SessionStopReason.PlayerInput)
                Assert.Null(RunUntilStop(session, observations: observations, texts: texts, yes: yes).Failure);
            Assert.Equal(identity, session.Current.SessionId);
        }
        var expectedPrograms = Record(r2).GetProperty("scriptTrace").EnumerateArray().Select(value => value.GetString()!.ToLowerInvariant().Replace('_', '-')).ToArray();
        Assert.Equal(expectedPrograms, observations.Where(row => row.Program is { Instruction: 0 } location && expectedPrograms.Contains(location.Program))
            .Select(row => row.Program!.Value.Program).ToArray());
        Assert.Contains(observations, row => row.Kind == "door-opened");
        Assert.Contains(603, session.Current.Story.Flags);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        return session;
    }

    [PrivateInputFact(World)]
    public void FullOriginalOpeningConsumesR1InputsAndMatchesR2AndR2aThroughStableFieldControl()
    {
        List<SessionObservation> observations = []; List<(int Id, int? Speaker)> texts = [];
        var session = RunOpening(true, observations, texts);
        using var fixture = Fixture("r2a");
        var expected = Record(fixture);
        var messenger = texts.SkipWhile(text => text.Id != 517).ToArray();
        Assert.Equal(expected.GetProperty("textIds").EnumerateArray().Select(value => value.GetInt32()), messenger.Select(text => text.Id));
        Assert.Equal(expected.GetProperty("speakerOperands").EnumerateArray().Select(value => value.ValueKind == JsonValueKind.Null ? (int?)null : value.GetInt32()), messenger.Select(text => text.Speaker));
        var endpoint = expected.GetProperty("endpoint");
        Assert.Equal(new MapPosition(endpoint.GetProperty("x").GetInt32(), endpoint.GetProperty("y").GetInt32()), Entity(session, 0).Position);
        Assert.Equal(endpoint.GetProperty("facing").GetByte(), Entity(session, 0).Motion.Facing);
        foreach (var guard in expected.GetProperty("guards").EnumerateArray())
        {
            var entity = Entity(session, guard.GetProperty("id").GetInt32());
            Assert.Equal(new MapPosition(guard.GetProperty("x").GetInt32(), guard.GetProperty("y").GetInt32()), entity.Position);
            Assert.Equal(guard.GetProperty("facing").GetByte(), entity.Motion.Facing);
        }
        foreach (int flag in new[] { 0, 1, 2, 32, 33, 34, 66, 89, 256, 260, 600, 601, 602, 603 }) Assert.Contains(flag, session.Current.Story.Flags);
        Assert.Equal(new EntityFollower(0, -24, 0), Entity(session, 1).Follower);
        Assert.Equal(new EntityFollower(1, -24, 0), Entity(session, 2).Follower);
        Assert.Equal(new[] { 0, 1, 2 }, session.Current.Story.PartyLists!.Joined);
        Assert.Equal(new[] { 0, 1 }, session.Current.Story.PartyLists.Active);
        Assert.False(session.Current.Exploration!.Aliases.ContainsKey(new("entity-142")));
        Assert.False(session.Current.Exploration.Aliases.ContainsKey(new("entity-143")));
        foreach (var direction in new[] { ExplorationDirection.South, ExplorationDirection.North })
        {
            Accept(session, new Move(direction));
            Assert.Null(RunUntilStop(session).Failure);
        }
        Assert.Equal(new MapPosition(43, 10), Entity(session, 0).Position);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [PrivateInputFact(World)]
    public void RefusalPreservesUnjoinedStateAndARealSarahInteractionCanLaterJoinTheParty()
    {
        var session = RunOpening(false, [], []);
        foreach (int flag in new[] { 1, 2, 33, 34, 66, 89, 600 }) Assert.DoesNotContain(flag, session.Current.Story.Flags);
        Assert.Null(Entity(session, 1).Follower); Assert.Null(Entity(session, 2).Follower);
        Assert.Equal(new MapPosition(41, 10), Entity(session, 1).Position);
        Accept(session, new Move(ExplorationDirection.West));
        Assert.Null(RunUntilStop(session).Failure);
        Accept(session, new Move(ExplorationDirection.West)); // Blocked by Sarah, with the required facing.
        Accept(session, new Interact(new("entity-1")));
        List<(int Id, int? Speaker)> texts = [];
        Assert.Null(RunUntilStop(session, texts: texts).Failure);
        Assert.Equal(new[] { 534, 535, 536, 447 }, texts.Select(text => text.Id));
        Assert.Contains(600, session.Current.Story.Flags); Assert.Contains(66, session.Current.Story.Flags);
        Assert.Equal(new EntityFollower(0, -24, 0), Entity(session, 1).Follower);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [PrivateInputFact(World)]
    public void SourceFollowerFlagsShiftPhysicalSlotsAndKeepLogicalEntity142Interactable()
    {
        var session = StartSource("map3-followers-start", opening: true);
        Assert.Equal(18, Entity(session, 142).Slot);
        Assert.Equal(209, Entity(session, 142).Sprite);
        Assert.Equal(4, session.Current.Exploration!.AllEntities.Count(entity => entity.Slot <= 3));
        Assert.Equal(new EntityFollower(0, -24, 0), Entity(session, 1).Follower);
    }

    [PrivateInputFact(World)]
    public void SourceGuardDoesNotInventUnknownEntity135OrPublishTheLaterUnlockFlag()
    {
        var session = StartSource("map21-guard-start");
        var result = RunUntilStop(session);
        Assert.Equal("program-entity", result.Failure!.Code);
        Assert.Equal(new ProgramLocation("cs-53ef4", 1), session.Current.Story.Cursor);
        Assert.Equal(new MapPosition(6, 16), Entity(session, 128).Position);
        Assert.DoesNotContain(401, session.Current.Story.Flags);
    }

    [PrivateInputFact(World)]
    public void SourceMap40InputsTransferAtTheMarkerAndStopInsideTheActualBeforeProgram()
    {
        var session = TraverseMap40("map40-intro-start");
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        Assert.Equal(new MapId("map-57"), session.Current.Exploration!.Map);
        Assert.Equal(new ProgramLocation("bbcs-01", 1), session.Current.Story.Cursor);
        Assert.Equal(2292, session.Current.Story.TextCursor);
        Assert.Equal(SessionStopReason.Unsupported, session.Current.StopReason);
        Assert.DoesNotContain(451, session.Current.Story.Flags);
    }

    [PrivateInputFact(World)]
    public void SourceMap40WithExplicitSeenIntroInitializesTheExistingBattleInTheSameSession()
    {
        var session = TraverseMap40("map40-seen-start");
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Equal(1, session.Current.Battle.Round);
        Assert.Equal(new[] { 401, 451 }, session.Current.Story.Flags);
        Assert.Equal("ally-1", session.Current.Selection!.Actor.Value);
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "player-ready.json")));
        var ready = fixture.RootElement.GetProperty("expectedObservation").GetProperty("records")[0]
            .GetProperty("deterministicState").GetProperty("ready");
        Assert.Equal(ready.GetProperty("randomSeed").GetUInt32(), session.Current.Battle.MainSeed);
    }

    [PrivateInputFact(World)]
    public void SourceMap57StopsAtAnEnabledLayoutEventBeforeSelectingBattle()
    {
        var source = new PrivateExplorationReader(PrivateInputFactAttribute.RequireInput(World),
            Path.Combine(AppContext.BaseDirectory, "controlled", "map40-seen-start.json"), PrivateBattleScenarioTests.Selected());
        var admitted = Assert.IsType<ExplorationReadAccepted>(source.Read());
        foreach (bool enabled in new[] { false, true })
        {
            var input = new ExplorationStartInput(new("map-57"), admitted.Start.Player, new(8, 18), 0, 32,
                enabled ? [401, 451, 506] : [401, 451], admitted.Start.Party);
            var started = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, input));
            if (!enabled)
            {
                Assert.Null(started.Result.Failure);
                Assert.Equal(SessionMode.Battle, started.Session.Current.Mode);
                continue;
            }
            Assert.Equal(SessionFailureKind.UnsupportedCapability, started.Result.Failure!.Kind);
            Assert.Equal("Map57s3_FlagEvents[0]:flag-layout-copy", started.Result.Failure.Field);
            Assert.Equal(SessionMode.Exploration, started.Session.Current.Mode);
            Assert.Equal(SessionStopReason.Unsupported, started.Session.Current.StopReason);
            Assert.Equal(new ProgramLocation("map-57-flag-layout", 1), started.Session.Current.Story.Cursor);
            Assert.Equal(new[] { 401, 451, 506 }, started.Session.Current.Story.Flags);
        }
    }

    private static GameSession TraverseMap40(string start)
    {
        var session = StartSource(start);
        var identity = session.Current.SessionId;
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "player-ready.json")));
        var plan = fixture.RootElement.GetProperty("static").GetProperty("inputPlan").EnumerateArray()
            .Where(row => row.GetProperty("from").GetProperty("map").GetInt32() == 40).ToArray();
        Assert.NotEmpty(plan);
        foreach (var row in plan)
        {
            var direction = row.GetProperty("input").GetString() switch
            { "Up" => ExplorationDirection.North, "Down" => ExplorationDirection.South,
                "Left" => ExplorationDirection.West, "Right" => ExplorationDirection.East, _ => throw new InvalidOperationException() };
            var destination = row.GetProperty("to");
            bool reachesWarp = (session.Current.Exploration!.Layout[destination.GetProperty("x").GetInt32(),
                destination.GetProperty("y").GetInt32()] & 0x3C00) == 0x1000;
            var result = Send(session, new Move(direction));
            if (session.Current.Story.Wait is EntityWait) result = RunUntilStop(session);
            var expected = row.GetProperty("to");
            Assert.Equal(identity, session.Current.SessionId);
            // The fixture's inputPlan is a static geometry plan. Its last target is the
            // warp marker; original event dispatch intercepts that input before relocation.
            if (!reachesWarp && expected.GetProperty("map").GetInt32() == 40)
            {
                Assert.Null(result.Failure);
                Assert.Equal(new MapPosition(expected.GetProperty("x").GetInt32(), expected.GetProperty("y").GetInt32()), session.Current.Exploration!.PlayerEntity.Position);
            }
        }
        return session;
    }

    [PrivateInputFact(World)]
    public void APrivateWorldWithDifferentRomProvenanceCannotStartTheSession()
    {
        var path = Path.Combine(Path.GetTempPath(), "private-exploration-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            var content = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(PrivateInputFactAttribute.RequireInput(World)))!;
            content["provenance"]!["romSha256"] = new string('0', 64);
            File.WriteAllText(path, content.ToJsonString());
            var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(new PrivateExplorationReader(path,
                Path.Combine(AppContext.BaseDirectory, "controlled", "map3-sarah-start.json"), PrivateBattleScenarioTests.Selected())));
            Assert.Equal(SessionFailureKind.ContentError, failed.Failure.Kind);
            Assert.Equal("world-rom-identity", failed.Failure.Code);
        }
        finally { File.Delete(path); }
    }

    [PrivateInputFact(World)]
    public void PrivatePresentationBytesAndRequiredSpriteLinksAreAdmittedBeforeStartup()
    {
        var path = Path.Combine(Path.GetTempPath(), "private-exploration-visual-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            foreach (bool corruptBytes in new[] { true, false })
            {
                var content = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(PrivateInputFactAttribute.RequireInput(World)))!;
                var visuals = content["world"]!["presentation"]!;
                if (corruptBytes) visuals["maps"]![0]!["atlas"]!["data"] = "AAAA";
                else
                {
                    var sprites = visuals["sprites"]!.AsArray();
                    sprites.Remove(sprites.Single(sprite => sprite!["sprite"]!.GetValue<int>() == 209));
                }
                File.WriteAllText(path, content.ToJsonString());
                var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(new PrivateExplorationReader(path,
                    Path.Combine(AppContext.BaseDirectory, "controlled", "map3-opening-start.json"), PrivateBattleScenarioTests.Selected())));
                Assert.Equal(corruptBytes ? "raster-identity" : "missing-sprite-visual", failed.Failure.Code);
            }
        }
        finally { File.Delete(path); }
    }
}
