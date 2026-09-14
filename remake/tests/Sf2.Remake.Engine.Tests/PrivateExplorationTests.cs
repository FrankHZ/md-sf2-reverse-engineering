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
    private static GameSession StartSource(string start)
    {
        var source = new PrivateExplorationReader(PrivateInputFactAttribute.RequireInput(World),
            Path.Combine(AppContext.BaseDirectory, "controlled", start + ".json"), PrivateBattleScenarioTests.Selected());
        var read = source.Read();
        Assert.True(read is ExplorationReadAccepted, read is ScenarioReadRejected rejected ? rejected.Failure.ToString() : "world admission");
        var admitted = (ExplorationReadAccepted)read;
        var outcome = GameSession.Start(admitted.Definition, admitted.Start);
        Assert.True(outcome is SessionStarted, outcome is SessionStartFailed failed ? failed.Failure.ToString() : "session start");
        return ((SessionStarted)outcome).Session;
    }
    private static ExplorationEntity Entity(GameSession session, int id) => session.Current.Exploration!.Entities[new("entity-" + id)];
    private static SessionResult RunUntilStop(GameSession session, int limit = 3000)
    {
        SessionResult? result = null;
        for (int index = 0; index < limit && session.Current.StopReason is SessionStopReason.SimulationWait or SessionStopReason.PresentationWait; index++)
        {
            SessionCommand command = session.Current.Story.Wait is DialogueWait dialogue ? new Acknowledge(dialogue.Token) :
                new AdvanceSimulation(session.Current.Story.Wait?.Token);
            result = Send(session, command);
            if (result.Failure is not null) return result;
        }
        Assert.NotNull(result);
        Assert.NotEqual(SessionStopReason.SimulationWait, session.Current.StopReason);
        return result;
    }

    [PrivateInputFact(World)]
    public void SourceSarahProgramTraversesBothSegmentsAndRetainsIndependentPartyAndFlags()
    {
        var session = StartSource("map3-sarah-start");
        var identity = session.Current.SessionId;
        var seed = session.Current.Exploration!.Party.MainSeed;
        Assert.Equal(new MapPosition(42, 8), Entity(session, 1).Position);
        Assert.Equal(new ProgramLocation("cs-513d6", 0), session.Current.Story.Cursor);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(42 * 384, Entity(session, 1).Motion.X);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(42 * 384 - 32, Entity(session, 1).Motion.X);
        Assert.Null(RunUntilStop(session).Failure);
        Assert.Equal(new MapPosition(41, 7), Entity(session, 1).Position);
        Assert.Equal(25, session.Current.Story.SimulationTick);
        Assert.Equal(new[] { 256 }, session.Current.Story.Flags);
        Assert.Equal(seed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(identity, session.Current.SessionId);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        var stopped = Send(session, new Move(ExplorationDirection.South));
        Assert.Equal(SessionFailureKind.UnsupportedCapability, stopped.Failure!.Kind);
        Assert.Equal(new MapPosition(41, 7), Entity(session, 1).Position);
    }

    [PrivateInputFact(World)]
    public void SourceGateProgramMovesBothGuardsThroughDialogueAndBack()
    {
        var session = StartSource("map3-gate-start");
        var left = Entity(session, 138).Position;
        var right = Entity(session, 139).Position;
        int dialogues = 0;
        for (int step = 0; step < 300 && session.Current.StopReason != SessionStopReason.PlayerInput; step++)
        {
            if (session.Current.Story.Wait is DialogueWait text)
            {
                Assert.Equal(new MapPosition(left.X + 1, left.Y), Entity(session, 138).Position);
                Assert.Equal(new MapPosition(right.X - 1, right.Y), Entity(session, 139).Position);
                byte[] sourceSpeakerFlags = [0, 192, 0, 192, 0, 0];
                Assert.Equal(sourceSpeakerFlags[dialogues++], text.SpeakerFlags);
                Accept(session, new Acknowledge(text.Token));
            }
            else Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        }
        Assert.Equal(6, dialogues);
        Assert.Equal(left, Entity(session, 138).Position);
        Assert.Equal(right, Entity(session, 139).Position);
        Assert.Equal(new[] { 66, 600 }, session.Current.Story.Flags);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [PrivateInputFact(World)]
    public void SourceMessengerRunsItsWaitMotionDialoguePrefixAndStopsAtUnimplementedGesture()
    {
        var session = StartSource("map3-messenger-start");
        Assert.Equal(20, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        var result = RunUntilStop(session);
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure!.Kind);
        Assert.EndsWith("cs_5149A[39]:nod", result.Failure.Field);
        Assert.Equal(new ProgramLocation("cs-5149a", 24), session.Current.Story.Cursor);
        Assert.Equal(new MapPosition(42, 7), Entity(session, 142).Position);
        Assert.Equal(new MapPosition(42, 8), Entity(session, 143).Position);
        Assert.Equal(48, Entity(session, 143).Motion.XSpeed);
        Assert.Equal(new[] { 256, 260, 601, 602 }, session.Current.Story.Flags);
        Assert.DoesNotContain(600, session.Current.Story.Flags);
        Assert.Equal(521, session.Current.Story.TextCursor);
    }

    [PrivateInputFact(World)]
    public void SourceInitializationStopsAtSpriteRefreshAfterItsGlobalSizeAndFlagWrites()
    {
        var session = StartSource("map3-sprite-init-start");
        var result = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal("entity-sprite-refresh", result.Failure!.Code);
        Assert.Equal(new ProgramLocation("cs-5145c", 0), session.Current.Story.Cursor);
        Assert.Equal(24, session.Current.Exploration!.SpriteSize);
        Assert.Equal(128, Entity(session, 128).Motion.FlagsA);
        Assert.Equal(64, Entity(session, 128).Motion.FlagsB);
        Assert.Equal(new MapPosition(5, 6), Entity(session, 128).Position);
        Assert.Equal(1, session.Current.Story.SimulationTick);
        Assert.Empty(session.Current.Story.Flags);
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
    public void SourcePositionProgramsApplyTheirDeclaredAssignmentsWithoutRunningAnEarlierRoute()
    {
        var astral = StartSource("map3-position-start");
        Assert.Equal(new MapPosition(41, 10), Entity(astral, 1).Position);
        Assert.Equal(new MapPosition(6, 4), Entity(astral, 128).Position);
        Assert.Equal(0, astral.Current.Story.SimulationTick);
        Assert.Equal(new[] { 256 }, astral.Current.Story.Flags);
        var zone = StartSource("map3-zone-position-start");
        Assert.Equal(new MapPosition(41, 10), Entity(zone, 1).Position);
        Assert.Equal(0, zone.Current.Story.SimulationTick);
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
}
