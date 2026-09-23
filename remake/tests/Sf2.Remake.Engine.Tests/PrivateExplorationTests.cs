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
    internal static SessionResult RunUntilStop(GameSession session, int limit = 3000,
        List<SessionObservation>? observations = null, List<(int Id, int? Speaker)>? texts = null, bool yes = true)
    {
        SessionResult? result = null;
        for (int index = 0; index < limit && session.Current.StopReason is SessionStopReason.SimulationWait or SessionStopReason.PresentationWait; index++)
        {
            foreach (var entity in session.Current.Exploration?.AllEntities.Where(entity => entity.WaitingForSprite && entity.SpriteReady != entity.SpriteRequest).ToArray() ?? [])
            {
                result = Accept(session, new EntitySpriteReady(entity.Slot, entity.SpriteRequest));
                observations?.AddRange(result.Observations);
            }
            if (session.Current.StopReason == SessionStopReason.PlayerInput) break;
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

    [PrivateInputFact(World)]
    public void SelectedR1WalkingPhasesResumeBeforeTheFirstAutonomousDraw()
    {
        var session = StartSource("map3-opening-start", opening: true);
        var world = session.Current.Exploration!;
        var allocation = world.AllEntities.Select(entity => (entity.Slot, entity.Entity)).ToArray();
        var aliases = world.Aliases.ToDictionary(pair => pair.Key, pair => pair.Value);
        Assert.Equal(20, allocation.Length);
        Assert.Equal(30, Entity(session, 130).Motion.WaitTimer);
        Assert.Equal(0, Entity(session, 130).ActionCursor);
        Assert.Equal(9, Entity(session, 131).ActionCursor);
        Assert.True(Entity(session, 131).WaitingForMotion);
        Assert.Equal((short)3453, Entity(session, 133).Motion.Y);
        Assert.Equal((short)3072, Entity(session, 133).Motion.YDestination);
        Assert.Equal((short)-2, Entity(session, 133).Motion.YVelocity);
        Assert.Equal((ushort)384, Entity(session, 133).Motion.YTravel);
        Assert.True(Entity(session, 133).WaitingForMotion);

        Accept(session, new AdvanceSimulation());
        Assert.Equal(0xC6320000u, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal(allocation, session.Current.Exploration.AllEntities.Select(entity => (entity.Slot, entity.Entity)));
        Assert.Equal(aliases, session.Current.Exploration.Aliases);
        Assert.Equal(9, Entity(session, 130).ActionCursor);
        Assert.True(Entity(session, 130).WaitingForMotion);
        Assert.Equal((short)5376, Entity(session, 130).Motion.YDestination);
        Assert.Equal((byte)1, Entity(session, 131).Motion.WaitTimer);
        Assert.False(Entity(session, 131).WaitingForMotion);
        Assert.Equal((short)3450, Entity(session, 133).Motion.Y);
        Assert.Equal((byte)0, Entity(session, 133).Motion.WaitTimer);
        Assert.True(Entity(session, 133).WaitingForMotion);
    }

    internal static GameSession RunOpening(bool yes, List<SessionObservation> observations, List<(int Id, int? Speaker)> texts)
    {
        var session = StartSource("map3-opening-start", opening: true);
        using var r1 = Fixture("r1");
        var state = Record(r1).GetProperty("scenarioState");
        var player = state.GetProperty("playerEntity");
        Assert.Equal(player.GetProperty("x").GetInt16(), Entity(session, 0).Motion.X);
        Assert.Equal(player.GetProperty("y").GetInt16(), Entity(session, 0).Motion.Y);
        Assert.Equal(player.GetProperty("facing").GetByte(), Entity(session, 0).Motion.Facing);
        Assert.Equal(60u, session.Current.Exploration!.Party.Gold);
        Assert.Equal(2568421376u, session.Current.Exploration.Party.MainSeed);
        foreach (string name in new[] { "joinedFlags", "activeFlags" })
            Assert.Equal(state.GetProperty(name).EnumerateArray().Select(flag => flag.GetBoolean()),
                Enumerable.Range(name == "joinedFlags" ? 0 : 32, 30).Select(session.Current.Story.Flags.Contains));
        Assert.Equal(new[] { 0, 2, 5 }, Enumerable.Range(0, 3).Select(id => Entity(session, id).Sprite!.Value));
        var identity = session.Current.SessionId;
        using var r2 = Fixture("r2");
        var inputTrace = Record(r2).GetProperty("logicalInputTrace").EnumerateArray().ToArray();
        string previousInputOutcome = "start";
        for (int index = 0; index < inputTrace.Length; index++)
        {
            var edge = inputTrace[index];
            Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
            var expectedPosition = new MapPosition(edge.GetProperty("x").GetInt32(), edge.GetProperty("y").GetInt32());
            var actualPosition = Entity(session, 0).Position;
            if (expectedPosition != actualPosition)
            {
                var walker = Entity(session, 130);
                var world = session.Current.Exploration!;
                bool occupied = EntityMotion.FieldObstructed(expectedPosition.X * 384, expectedPosition.Y * 384,
                    world.AllEntities.Where(entity => entity.Slot != 0 && entity.Visible).Select(entity => entity.Motion));
                Assert.True(false, $"input {index}: expected {expectedPosition}, actual {actualPosition}; previous {previousInputOutcome}; " +
                    $"walker5=({walker.Motion.X},{walker.Motion.Y})->({walker.Motion.XDestination},{walker.Motion.YDestination}) " +
                    $"flagsA={walker.Motion.FlagsA}; expected tile occupied={occupied}");
            }
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
            var commandResult = Accept(session, command);
            observations.AddRange(commandResult.Observations);
            previousInputOutcome = string.Join(",", commandResult.Observations.Select(observation => observation.Kind));
            if (session.Current.StopReason != SessionStopReason.PlayerInput)
                Assert.Null(RunUntilStop(session, observations: observations, texts: texts, yes: yes).Failure);
            Assert.Equal(identity, session.Current.SessionId);
        }
        var expectedPrograms = Record(r2).GetProperty("scriptTrace").EnumerateArray().Select(value => value.GetString()!.ToLowerInvariant().Replace('_', '-')).ToArray();
        Assert.Equal(expectedPrograms, observations.Where(row => row.Program is { Instruction: 0 } location && expectedPrograms.Contains(location.Program))
            .Select(row => row.Program!.Value.Program).ToArray());
        Assert.Contains(observations, row => row.Kind == "door-opened");
        Assert.Contains(603, session.Current.Story.Flags);
        Assert.False(session.Current.Exploration!.TryResolveEntity(new("entity-142"), out _));
        Assert.False(session.Current.Exploration.TryResolveEntity(new("entity-46"), out _));
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
    public void CastleMapInitSelectsAstralVisibilityAndAllocatesLiveFollowerFlagsWithoutChangingMembership()
    {
        var source = new PrivateExplorationReader(PrivateInputFactAttribute.RequireInput(World),
            Path.Combine(AppContext.BaseDirectory, "controlled", "map3-opening-start.json"),
            PrivateBattleScenarioTests.Selected(Path.Combine(AppContext.BaseDirectory, "controlled", "map3-opening-party.json")));
        var admitted = Assert.IsType<ExplorationReadAccepted>(source.Read());
        foreach (int phase in new[] { 0, 605, 608 })
        foreach (int count in new[] { 0, 2, 3 })
        {
            int[] flags = [0, 32, .. phase == 0 ? Array.Empty<int>() : phase == 605 ? new[] { 605 } : new[] { 605, 608 },
                .. count == 0 ? Array.Empty<int>() : count == 2 ? new[] { 66 } : new[] { 66, 67 }];
            var input = new ExplorationStartInput(new("map-19"), admitted.Start.Player, new(26, 30), 1, 32, flags, admitted.Start.Party);
            var session = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, input)).Session;
            if (session.Current.StopReason != SessionStopReason.PlayerInput) Assert.Null(RunUntilStop(session).Failure);
            Assert.Equal(phase == 605 ? new MapPosition(16, 5) : new MapPosition(63, 63), Entity(session, 140).Position);
            Assert.Equal(count + 13, Entity(session, 140).Slot);
            Assert.Equal(count, session.Current.Exploration!.AllEntities.Count(entity => entity.Follower is not null));
            Assert.Equal(new[] { 0 }, session.Current.Story.PartyLists!.Joined);
            Assert.Equal(new[] { 0 }, session.Current.Story.PartyLists.Active);
            Assert.DoesNotContain(401, session.Current.Story.Flags);
            Assert.DoesNotContain(607, session.Current.Story.Flags);
        }
    }

    [PrivateInputFact(World)]
    public void CastlePalaceAndAstralRunFromTheLiveOpeningWithFirstRepeatAndChoiceVariants()
    {
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "castle-tower.json")));
        foreach (bool followers in new[] { true, false })
        {
            List<SessionObservation> observations = [];
            var session = RunOpening(followers, observations, []);
            if (!followers)
            {
                Accept(session, new Move(ExplorationDirection.West));
                Assert.Null(RunUntilStop(session).Failure);
                Accept(session, new Move(ExplorationDirection.West));
                Accept(session, new Interact(new("entity-1")));
                Assert.Null(RunUntilStop(session).Failure);
                Accept(session, new Move(ExplorationDirection.East));
                Assert.Null(RunUntilStop(session).Failure);
            }
            var identity = session.Current.SessionId;
            var expectedParty = session.Current.Story.PartyLists;
            var guard138 = Entity(session, 138).Position;
            var guard139 = Entity(session, 139).Position;
            void Input(ExplorationDirection direction)
            {
                observations.AddRange(Accept(session, new Move(direction)).Observations);
                if (session.Current.StopReason != SessionStopReason.PlayerInput)
                    Assert.Null(RunUntilStop(session, observations: observations).Failure);
                Assert.Equal(identity, session.Current.SessionId);
            }
            foreach (var segment in fixture.RootElement.GetProperty("static").GetProperty("routeGraph").GetProperty("segments").EnumerateArray())
            {
                var kind = segment.GetProperty("kind").GetString();
                string id = segment.GetProperty("id").GetString()!;
                if (kind == "navigation")
                {
                    int step = 0;
                    foreach (var input in segment.GetProperty("inputs").EnumerateArray())
                    {
                        var point = segment.GetProperty("points")[step++];
                        Assert.Equal(new MapPosition(point[0].GetInt32(), point[1].GetInt32()), Entity(session, 0).Position);
                        Assert.Equal("map-" + segment.GetProperty("map").GetInt32(), session.Current.Exploration!.Map.Value);
                        Input(input.GetString() switch
                        {
                            "Left" => ExplorationDirection.West, "Right" => ExplorationDirection.East,
                            "Up" => ExplorationDirection.North, _ => ExplorationDirection.South,
                        });
                    }
                }
                if (id == "map3-castle-gate-zone")
                {
                    Assert.Contains(604, session.Current.Story.Flags);
                    Assert.Equal(guard138, Entity(session, 138).Position);
                    Assert.Equal(guard139, Entity(session, 139).Position);
                }
                if (id == "map3-to-map19-north-warp")
                {
                    Assert.Equal(new MapPosition(63, 63), Entity(session, 140).Position);
                    Assert.DoesNotContain(session.Current.Story.Flags, flag => flag is >= 256 and <= 383);
                    Assert.Contains(80, session.Current.Story.Flags);
                    Assert.Equal(2, session.Current.Exploration!.AllEntities.Count(entity => entity.Follower is not null));
                }
                if (id == "map20-palace-init-and-return")
                {
                    Assert.Contains(605, session.Current.Story.Flags);
                    Assert.Equal(new MapPosition(23, 39), Entity(session, 0).Position);
                    Assert.Equal(new MapPosition(20, 39), Entity(session, 131).Position);
                    Assert.False(session.Current.Exploration!.Aliases.ContainsKey(new("entity-130")));
                    Assert.False(Entity(session, 0).Priority);
                    Assert.True(Entity(session, 131).Priority);
                }
                if (id == "map20-to-map19-royal-return")
                {
                    Assert.Equal(new MapPosition(16, 5), Entity(session, 140).Position);
                    Input(ExplorationDirection.West); Input(ExplorationDirection.East);
                    Assert.Equal(new MapPosition(23, 37), Entity(session, 0).Position);
                    Assert.False(session.Current.Exploration!.Aliases.ContainsKey(new("entity-130")));
                    Assert.False(Entity(session, 0).Priority);
                    Input(ExplorationDirection.South); Input(ExplorationDirection.North);
                    Assert.Equal("map-19", session.Current.Exploration.Map.Value);
                }
                if (kind == "entity-interaction")
                {
                    Input(ExplorationDirection.North);
                    Accept(session, new Interact(new("entity-140")));
                    Assert.Null(RunUntilStop(session, observations: observations, yes: false).Failure);
                    Assert.Contains(607, session.Current.Story.Flags);
                    Assert.DoesNotContain(608, session.Current.Story.Flags);
                    Assert.Equal(new MapPosition(16, 5), Entity(session, 140).Position);
                    Accept(session, new Interact(new("entity-140")));
                    Assert.Null(RunUntilStop(session, observations: observations).Failure);
                    Assert.Contains(608, session.Current.Story.Flags);
                    Assert.Equal(new MapPosition(63, 63), Entity(session, 140).Position);
                }
                if (kind == "entity-terminal")
                {
                    Input(ExplorationDirection.East); Input(ExplorationDirection.East);
                    Accept(session, new Interact(new("entity-128")));
                    var result = RunUntilStop(session, observations: observations);
                    Assert.Null(result.Failure);
                    Assert.Null(session.Current.Story.Cursor);
                    Assert.Equal(new MapPosition(6, 16), Entity(session, 128).Position);
                    Assert.Equal(3, Entity(session, 0).Motion.Facing);
                    Assert.Same(Entity(session, 0), Entity(session, 135));
                    Assert.Contains(401, session.Current.Story.Flags);
                    Assert.Contains(256, session.Current.Story.Flags);
                    Input(ExplorationDirection.East); Input(ExplorationDirection.East);
                    List<(int Id, int? Speaker)> repeated = [];
                    Accept(session, new Interact(new("entity-128")));
                    Assert.Null(RunUntilStop(session, texts: repeated).Failure);
                    Assert.Equal(new[] { 579 }, repeated.Select(text => text.Id));
                    Assert.Equal(new MapPosition(6, 16), Entity(session, 128).Position);
                    Input(ExplorationDirection.North);
                    Assert.Equal(new MapPosition(5, 15), Entity(session, 0).Position);
                    Input(ExplorationDirection.South); Input(ExplorationDirection.North);
                    Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
                }
            }
            foreach (string program in fixture.RootElement.GetProperty("static").GetProperty("programs").EnumerateObject()
                .Select(property => property.Name.ToLowerInvariant().Replace('_', '-')))
                Assert.Contains(observations, row => row.Program is { Instruction: 0 } location && location.Program == program);
            Assert.Equal(expectedParty!.Joined, session.Current.Story.PartyLists!.Joined);
            Assert.Equal(expectedParty.Active, session.Current.Story.PartyLists.Active);
        }
    }

    [PrivateInputFact(World)]
    public void SourceGuardEventBranchesUseLiveFlagsAndFollowerSlotsWithoutInventingMembership()
    {
        var source = new PrivateExplorationReader(PrivateInputFactAttribute.RequireInput(World),
            Path.Combine(AppContext.BaseDirectory, "controlled", "map3-opening-start.json"),
            PrivateBattleScenarioTests.Selected(Path.Combine(AppContext.BaseDirectory, "controlled", "map3-opening-party.json")));
        var admitted = Assert.IsType<ExplorationReadAccepted>(source.Read());
        foreach (bool accepted in new[] { false, true })
        foreach (bool talked in new[] { false, true })
        foreach (int followers in new[] { 0, 2, 3 })
        {
            int[] flags = [0, 32, 601, 602, 605, .. accepted ? new[] { 608 } : Array.Empty<int>(),
                .. talked ? new[] { 256 } : Array.Empty<int>(),
                .. followers == 0 ? Array.Empty<int>() : followers == 2 ? new[] { 66 } : new[] { 66, 67 }];
            var input = new ExplorationStartInput(new("map-21"), admitted.Start.Player, new(4, 16), 0, 32, flags, admitted.Start.Party);
            var session = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, input)).Session;
            if (session.Current.StopReason != SessionStopReason.PlayerInput) Assert.Null(RunUntilStop(session).Failure);
            Assert.Equal(followers + 1, Entity(session, 128).Slot);
            Assert.Same(Entity(session, 0), Entity(session, 135));
            List<(int Id, int? Speaker)> texts = [];
            List<SessionObservation> observations = [];
            Accept(session, new Interact(new("entity-128")));
            Assert.Null(RunUntilStop(session, observations: observations, texts: texts).Failure);
            bool released = accepted && !talked;
            Assert.Equal(accepted ? new[] { 579 } : talked ? new[] { 569 } : new[] { 568, 569 }, texts.Select(text => text.Id));
            Assert.Equal(released, session.Current.Story.Flags.Contains(401));
            Assert.Contains(256, session.Current.Story.Flags);
            Assert.Equal(new MapPosition(released ? 6 : 5, 16), Entity(session, 128).Position);
            Assert.Equal(released ? 3 : 0, Entity(session, 0).Motion.Facing);
            Assert.Equal(new[] { 0 }, session.Current.Story.PartyLists!.Joined);
            Assert.Equal(new[] { 0 }, session.Current.Story.PartyLists.Active);
            Assert.DoesNotContain(session.Current.Exploration!.AllEntities, row => row.Entity.Value == "entity-135");
            if (released)
            {
                int sprite = observations.FindIndex(row => row.Kind == "entity-sprite-ready" && row.Detail == "0");
                int unlock = observations.FindIndex(row => row.Detail == "WriteFlag" && row.Program?.Program == "cs-53ef4");
                int caller = observations.FindLastIndex(row => row.Detail == "WriteFlag" && row.Program?.Program != "cs-53ef4");
                Assert.True(sprite >= 0 && unlock > sprite && caller > unlock);
            }
        }
    }

    [PrivateInputFact(World)]
    public void SourceGuardMovesThenFacesThePlayerThroughZeroIdentityBeforePublishingItsUnlock()
    {
        var session = StartSource("map21-guard-start");
        for (int tick = 0; tick < 100 && session.Current.Story.Wait is not EntitySpriteWait; tick++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var wait = Assert.IsType<EntitySpriteWait>(session.Current.Story.Wait);
        Assert.Equal(new ProgramLocation("cs-53ef4", 1), session.Current.Story.Cursor);
        Assert.Equal(new MapPosition(6, 16), Entity(session, 128).Position);
        Assert.Same(Entity(session, 0), Entity(session, 135));
        Assert.Equal(3, Entity(session, 0).Motion.Facing);
        Assert.Equal(0, wait.Slot);
        Assert.DoesNotContain(401, session.Current.Story.Flags);
        Assert.DoesNotContain(256, session.Current.Story.Flags);
        Accept(session, new EntitySpriteReady(wait.Slot, wait.Request));
        Assert.Contains(401, session.Current.Story.Flags);
        Assert.DoesNotContain(256, session.Current.Story.Flags); // This explicit program start has no event caller.
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
    }

    [PrivateInputFact(World)]
    public void ExplicitMap57ExplorationStartRetainsItsControlledLayoutEventBoundary()
    {
        var source = new PrivateExplorationReader(PrivateInputFactAttribute.RequireInput(World),
            Path.Combine(AppContext.BaseDirectory, "controlled", "map40-seen-start.json"), PrivateBattleScenarioTests.Selected());
        var admitted = Assert.IsType<ExplorationReadAccepted>(source.Read());
        foreach (bool enabled in new[] { false, true })
        {
            var input = new ExplorationStartInput(new("map-57"), admitted.Start.Player, new(8, 18), 0, 32,
                enabled ? [0, 1, 2, 32, 33, 34, 401, 451, 506] : [0, 1, 2, 32, 33, 34, 401, 451], admitted.Start.Party);
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
            Assert.Equal(new[] { 0, 1, 2, 32, 33, 34, 401, 451, 506 }, started.Session.Current.Story.Flags);
        }
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
