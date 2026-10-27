using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using System.Text.Json.Nodes;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class CaravanTests
{
    [Theory]
    [InlineData(2, 2, 0, 0)]
    [InlineData(4, 4, 1, 1)]
    [InlineData(6, 5, 2, 3)]
    [InlineData(3, 6, 3, 2)]
    public void BoardingPreservesIdentityResourcesAndCommitsOnlyAfterActualWaits(int x, int y, byte facing, int followers)
    {
        var session = Pilot(x, y, facing, followers);
        var before = session.Current.Exploration!;
        Accept(session, new Move(Direction(facing)));
        Assert.Equal(before.PlayerEntity.Motion, session.Current.Exploration!.PlayerEntity.Motion);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait?.Token));
        Assert.Equal(FieldTravelMode.Walking, session.Current.Exploration!.TravelMode);
        Assert.Equal(before.PlayerEntity.Position, session.Current.Exploration.PlayerEntity.Position);
        Assert.Equal(before.Party.Actors, session.Current.Exploration.Party.Actors);
        Assert.IsType<EntityWait>(session.Current.Story.Wait);
        Assert.NotNull(Send(session, new Move(Direction(facing))).Failure);
        var seenSizes = new HashSet<ushort>();
        for (int i = 0; !session.Current.CanWaitAtInput && i < 500; i++)
        {
            seenSizes.Add(session.Current.Exploration!.SpriteSize);
            Deliver(session);
        }
        Assert.True(session.Current.CanWaitAtInput);
        var after = session.Current.Exploration!;
        Assert.Equal(FieldTravelMode.Caravan, after.TravelMode);
        Assert.Equal(62, after.PlayerEntity.Sprite);
        Assert.Equal(0x6000, after.Entities[new("wagon")].Motion.X);
        Assert.Equal(0x6000, after.Entities[new("wagon")].Motion.Y);
        Assert.Equal(before.CurrentEntities, after.CurrentEntities);
        Assert.Equal(before.Aliases, after.Aliases);
        Assert.Equal(before.Party.Actors, after.Party.Actors);
        Assert.Equal(before.Party.MainSeed, after.Party.MainSeed);
        Assert.Equal(before.Party.ThinkingSeed, after.Party.ThinkingSeed);
        Assert.Equal(before.Party.Gold, after.Party.Gold);
        Assert.Subset(seenSizes, new HashSet<ushort> { 12, 14, 16, 18, 20, 22 });
    }

    [Fact]
    public void MissingLogicalMappingRetainsEarlierIdleWriteAndCannotCommitMode()
    {
        var session = Pilot(missing: true);
        Accept(session, new Move(ExplorationDirection.East));
        var result = Send(session, new AdvanceSimulation(session.Current.Story.Wait?.Token));
        Assert.NotNull(result.Failure);
        Assert.True(session.Current.Exploration!.PlayerEntity.IsScriptIdle);
        Assert.Equal(FieldTravelMode.Walking, session.Current.Exploration.TravelMode);
        Assert.Equal(new MapPosition(2, 2), session.Current.Exploration.PlayerEntity.Position);
    }

    [Fact]
    public void ZeroSpeedAndUndeliveredSpritesRemainPending()
    {
        var stalled = Pilot(stall: true);
        Accept(stalled, new Move(ExplorationDirection.East));
        Accept(stalled, new AdvanceSimulation(stalled.Current.Story.Wait?.Token));
        var cursor = stalled.Current.Story.Cursor;
        for (int i = 0; i < 40; i++) Accept(stalled, new AdvanceSimulation(stalled.Current.Story.Wait!.Token));
        Assert.Equal(cursor, stalled.Current.Story.Cursor);
        Assert.Equal(FieldTravelMode.Walking, stalled.Current.Exploration!.TravelMode);
        Assert.Equal(new MapPosition(1, 1), stalled.Current.Exploration.Entities[new("wagon")].Position);

        var pending = Pilot();
        Accept(pending, new Move(ExplorationDirection.East));
        for (int i = 0; i < 80; i++) Accept(pending, new AdvanceSimulation(pending.Current.Story.Wait?.Token));
        var player = pending.Current.Exploration!.PlayerEntity;
        Assert.True(player.WaitingForSprite);
        Assert.Equal(FieldTravelMode.Walking, pending.Current.Exploration.TravelMode);
        Assert.Equal(22, pending.Current.Exploration.SpriteSize);
        Assert.NotNull(Send(pending, new EntitySpriteReady(player.Slot, player.SpriteRequest + 1)).Failure);
        Accept(pending, new EntitySpriteReady(player.Slot, player.SpriteRequest));
        Assert.NotNull(Send(pending, new EntitySpriteReady(player.Slot, player.SpriteRequest)).Failure);
    }

    [Fact]
    public void FinalListWaitObservesOnlyLastActorAndLogicalAliasDoesNotReplaceRawOne()
    {
        var suffix = Pilot(followers: 2, firstFollowerStalls: true);
        Accept(suffix, new Move(ExplorationDirection.East));
        for (int i = 0; !suffix.Current.CanWaitAtInput && i < 600; i++) Deliver(suffix);
        Assert.Equal(FieldTravelMode.Caravan, suffix.Current.Exploration!.TravelMode);
        Assert.True(suffix.Current.Exploration.Entities[new("friend-0")].Busy);
        Assert.True(suffix.Current.Exploration.Entities[new("friend-1")].IsScriptIdle);

        var alias = Pilot(followers: 1, alternate: true);
        Accept(alias, new Move(ExplorationDirection.East));
        for (int i = 0; !alias.Current.CanWaitAtInput && i < 1200; i++) Deliver(alias);
        Assert.Equal(FieldTravelMode.Caravan, alias.Current.Exploration!.TravelMode);
        Assert.Equal(new MapPosition(1, 1), alias.Current.Exploration.PlayerEntity.Position);
        Assert.Equal(new MapPosition(1, 1), alias.Current.Exploration.Entities[new("wagon")].Position);
        Assert.Equal(2, alias.Current.Exploration.Aliases[new("friend-0")]);
    }

    [Theory]
    [InlineData("hero", "wagon", 95)]
    [InlineData("walker-b", "cart-b", 0)]
    public void AuthoredSessionTravelsLandsAndThenRunsWalkingZone(string playerId, string wagonId, int hp)
    {
        var session = Start("caravan-travel", document =>
        {
            document["start"]!["player"] = playerId;
            document["world"]!["maps"]![0]!["entities"]![0]!["id"] = wagonId;
            document["world"]!["maps"]![0]!["caravan"]!["followerA"] = wagonId;
            document["battle"]!["start"]!["actors"]![0]!["hp"] = hp;
        });
        Guid identity = session.Current.SessionId;
        var initial = session.Current.Exploration!;
        var observations = new List<SessionObservation>();
        void MoveAndSettle(ExplorationDirection direction)
        {
            observations.AddRange(Accept(session, new Move(direction)).Observations);
            for (int i = 0; !session.Current.CanWaitAtInput && i < 1500; i++) Deliver(session, observations);
            Assert.True(session.Current.CanWaitAtInput);
        }
        MoveAndSettle(ExplorationDirection.East);
        Assert.Equal(FieldTravelMode.Caravan, session.Current.Exploration!.TravelMode);
        for (int x = 4; x <= 8; x++)
        {
            MoveAndSettle(ExplorationDirection.East);
            Assert.Equal(new MapPosition(x, 3), session.Current.Exploration!.PlayerEntity.Position);
            Assert.Equal((short)9, session.Current.Story.StepCounter);
            Assert.DoesNotContain(90, session.Current.Story.Flags);
        }
        MoveAndSettle(ExplorationDirection.East);
        var landed = session.Current.Exploration!;
        Assert.Equal(FieldTravelMode.Walking, landed.TravelMode);
        Assert.Equal(new MapPosition(9, 3), landed.PlayerEntity.Position);
        Assert.Equal(hp == 0 ? 143 : 7, landed.PlayerEntity.Sprite);
        Assert.Equal(initial.CurrentEntities, landed.CurrentEntities);
        Assert.Equal(initial.Aliases, landed.Aliases);
        Assert.Equal(initial.Party.Actors, landed.Party.Actors);
        Assert.Equal((short)9, session.Current.Story.StepCounter);
        Assert.DoesNotContain(90, session.Current.Story.Flags);
        Assert.Single(observations, row => row.Kind == "caravan-boarding-requested");
        Assert.Single(observations, row => row.Kind == "caravan-landing-requested");
        Assert.Equal(2, observations.Count(row => row.Detail == nameof(CommitCaravanMode)));
        MoveAndSettle(ExplorationDirection.South);
        MoveAndSettle(ExplorationDirection.North);
        Assert.Contains(90, session.Current.Story.Flags);
        Assert.Equal((short)7, session.Current.Story.StepCounter);
        Assert.Equal(identity, session.Current.SessionId);
        Assert.Equal(initial.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
    }

    [Fact]
    public void ControllerUsesCurrentObstructionAndUnshrunkBoundsWithoutWalkingZoneOrSteps()
    {
        var session = Start("caravan-travel");
        var world = session.Current.Exploration!;
        var motion = EntityActionRunner.ControlledMotion(world.PlayerEntity.Motion);
        world = world.WithEntity(world.PlayerEntity with { Motion = motion });
        var other = world.AllEntities[2];
        // Reserved target obstructs walking, but opcode7 examines current coordinates only.
        other = other with { Motion = other.Motion with { X = 5 * 384, Y = 5 * 384,
            XDestination = 3 * 384, YDestination = 3 * 384, FlagsA = 0x80 } };
        world = world.WithEntity(other);
        Assert.Null(MapEventDispatcher.Move(world, ExplorationDirection.East, [65]).Outcome.VehicleEvent);
        var vehicle = world.WithTravelMode(FieldTravelMode.Caravan);
        Assert.True(MapEventDispatcher.Move(vehicle, ExplorationDirection.East, [65]).Outcome.Moved);
        vehicle = vehicle.WithEntity(other with { Motion = other.Motion with { X = 3 * 384, Y = 3 * 384 } });
        Assert.False(MapEventDispatcher.Move(vehicle, ExplorationDirection.East, [65]).Outcome.Moved);
        var words = new ushort[4096]; words[3 * 64] = 0xF800;
        world = world.WithLayout(new(words)).WithEntity(world.PlayerEntity with { Motion = motion with
            { X = 384, XDestination = 384 } });
        Assert.False(MapEventDispatcher.Move(world, ExplorationDirection.West, [65]).Outcome.Moved);
        Assert.True(MapEventDispatcher.Move(world.WithTravelMode(FieldTravelMode.Caravan), ExplorationDirection.West, [65]).Outcome.Moved);
        // No F65 marker request when acquisition is clear; the same raw blocked word remains blocked.
        Assert.False(MapEventDispatcher.Move(session.Current.Exploration!, ExplorationDirection.East, []).Outcome.Moved);
        Assert.Null(MapEventDispatcher.Move(session.Current.Exploration!, ExplorationDirection.East, []).Outcome.VehicleEvent);
    }

    [Fact]
    public void ChangedInitialPositionAndLogicalBindingKeepLoadedAreaAfterRawOneClone()
    {
        var session = Start("caravan-travel", document =>
        {
            document["start"]!["position"]!["y"] = 5;
            var map = document["world"]!["maps"]![0]!;
            foreach (var row in map["layout"]!.AsArray()) { row!.AsArray().Add(0); row.AsArray().Add(0); }
            map["entities"]![0]!["position"] = JsonNode.Parse("{\"x\":12,\"y\":5}");
            map["caravan"]!["followerA"] = "scout";
            map["caravan"]!["currentEntities"] = JsonNode.Parse("[2,3]");
        });
        void MoveAndSettle(ExplorationDirection direction)
        {
            Accept(session, new Move(direction));
            for (int i = 0; !session.Current.CanWaitAtInput && i < 1500; i++) Deliver(session);
            Assert.True(session.Current.CanWaitAtInput);
        }
        MoveAndSettle(ExplorationDirection.East);
        Assert.Equal(FieldTravelMode.Caravan, session.Current.Exploration!.TravelMode);
        Assert.Equal(new MapPosition(12, 5), session.Current.Exploration.PlayerEntity.Position);
        Assert.Equal(0x6000, session.Current.Exploration.Entities[new("scout")].Motion.X);
        // The loaded area is still 0..10 / 0..8, even though raw physical1 was outside it.
        MoveAndSettle(ExplorationDirection.West);
        Assert.Equal(FieldTravelMode.Walking, session.Current.Exploration!.TravelMode);
        Assert.Equal(new MapPosition(11, 5), session.Current.Exploration.PlayerEntity.Position);
        MoveAndSettle(ExplorationDirection.North);
        Assert.Equal(new MapPosition(11, 4), session.Current.Exploration!.PlayerEntity.Position);
        MoveAndSettle(ExplorationDirection.East);
        Assert.Equal(new MapPosition(11, 4), session.Current.Exploration!.PlayerEntity.Position);
        MoveAndSettle(ExplorationDirection.West);
        Assert.Equal(new MapPosition(10, 4), session.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal((short)7, session.Current.Story.StepCounter);
    }

    [Fact]
    public void ChangedCurrentListSkipsItsLiteralFirstSlot()
    {
        var session = Start("caravan-travel", document =>
            document["world"]!["maps"]![0]!["caravan"]!["currentEntities"] = JsonNode.Parse("[3,2]"));
        var guest = session.Current.Exploration!.Entities[new("guest")].Motion;
        Accept(session, new Move(ExplorationDirection.East));
        for (int i = 0; !session.Current.CanWaitAtInput && i < 1000; i++) Deliver(session);
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Equal(FieldTravelMode.Caravan, session.Current.Exploration!.TravelMode);
        Assert.Equal(new[] { 3, 2 }, session.Current.Exploration.CurrentEntities);
        Assert.Equal(guest, session.Current.Exploration.Entities[new("guest")].Motion);
        Assert.Equal(0x6000, session.Current.Exploration.Entities[new("scout")].Motion.X);
    }

    [Fact]
    public void FollowerReinstallUsesActiveRowOrdinalAndPreservesPhysicalStateUntilService()
    {
        var session = Start("caravan-travel");
        var world = session.Current.Exploration!;
        var installed = CaravanRuntime.InstallFollowers(world, [65, 67, 68]);
        Assert.Equal(world.CurrentEntities, installed.CurrentEntities);
        Assert.Equal(world.Aliases, installed.Aliases);
        Assert.Equal(world.AllEntities.Select(row => row.Motion), installed.AllEntities.Select(row => row.Motion));
        Assert.Equal(new FollowPhysical(0, -24, -24), installed.AllEntities[2].Actions!.Actions[3]);
        var changed = CaravanRuntime.InstallFollowers(world, [65, 68]);
        Assert.Equal(new FollowPhysical(0, -24, -24), changed.AllEntities[2].Actions!.Actions[3]);
        Assert.Null(changed.AllEntities[3].Actions);
        var ordinary = CaravanRuntime.InstallFollowers(world, [67, 68]);
        Assert.Equal(new FollowPhysical(1, -24, 0), ordinary.AllEntities[2].Actions!.Actions[3]);
        // Overworld second follower retargets during motion; ordinary second waits for arrival.
        var leader = world.PlayerEntity with { Motion = world.PlayerEntity.Motion with { X = 7 * 384,
            Y = 6 * 384, XDestination = 8 * 384, YDestination = 6 * 384, Facing = 0 } };
        var moving = installed.AllEntities[2] with { ActionCursor = 3, Motion = installed.AllEntities[2].Motion with
            { XDestination = 2 * 384, YDestination = 2 * 384, XTravel = 384, YTravel = 384, XVelocity = 8, YVelocity = 8 } };
        var retarget = EntityActionRunner.Tick(installed.WithEntity(leader).WithEntity(moving)).World.AllEntities[2];
        Assert.NotEqual(moving.Motion.XDestination, retarget.Motion.XDestination);
    }

    [Theory]
    [InlineData("currentEntities", "[1,9]")]
    [InlineData("currentEntities", "[1,1]")]
    [InlineData("followerA", "\"absent\"")]
    public void UnsupportedBindingsFailAdmission(string key, string json)
    {
        var document = Document("caravan-travel");
        document["world"]!["maps"]![0]!["caravan"]![key] = JsonNode.Parse(json);
        Assert.IsType<ScenarioReadRejected>(Reader(document).Read());
    }

    [Fact]
    public void LandingRetainsModeAndPrefixAcrossUndeliveredSpriteAndLaterUnsupportedFollowerSelection()
    {
        var session = Start("caravan-travel", document => document["start"]!["flags"] = JsonNode.Parse("[65,67,68,72]"));
        for (int step = 0; step < 6; step++)
        {
            Accept(session, new Move(ExplorationDirection.East));
            for (int i = 0; !session.Current.CanWaitAtInput && i < 1200; i++) Deliver(session);
            Assert.True(session.Current.CanWaitAtInput);
        }
        Accept(session, new Move(ExplorationDirection.East));
        for (int i = 0; session.Current.Story.Wait is not EntitySpriteWait && i < 50; i++) Deliver(session);
        var wait = Assert.IsType<EntitySpriteWait>(session.Current.Story.Wait);
        Assert.Equal(FieldTravelMode.Caravan, session.Current.Exploration!.TravelMode);
        Assert.Equal(7, session.Current.Exploration.PlayerEntity.Sprite);
        Assert.Equal(new MapPosition(8, 3), session.Current.Exploration.Entities[new("wagon")].Position);
        Assert.NotNull(Send(session, new CompletePresentation(wait.Token, PresentationCueKind.EntityEffect)).Failure);
        for (int i = 0; i < 10; i++) Accept(session, new AdvanceSimulation(wait.Token));
        Assert.Equal(wait, session.Current.Story.Wait);
        // Valid growth prefix completes; changed active rows require a fourth physical slot,
        // which this admitted controlled population does not contain. No normalization occurs.
        SessionResult? failure = null;
        for (int i = 0; i < 1000; i++)
        {
            var pending = session.Current.Exploration.AllEntities.FirstOrDefault(actor => actor.WaitingForSprite && actor.SpriteReady != actor.SpriteRequest);
            SessionCommand command = pending is not null ? new EntitySpriteReady(pending.Slot, pending.SpriteRequest) :
                session.Current.Story.Wait is PresentationWait presentation ? new CompletePresentation(presentation.Token, presentation.Cue.Kind) :
                new AdvanceSimulation(session.Current.Story.Wait?.Token);
            var result = Send(session, command);
            if (result.Failure is not null) { failure = result; break; }
        }
        Assert.NotNull(failure);
        Assert.Equal("caravan-physical-entity", failure.Failure!.Code);
        Assert.Equal(FieldTravelMode.Caravan, session.Current.Exploration.TravelMode);
        Assert.Equal(new MapPosition(9, 3), session.Current.Exploration.PlayerEntity.Position);
        Assert.Equal(new[] { 1, 2, 3 }, session.Current.Exploration.CurrentEntities);
        Assert.Equal((short)9, session.Current.Story.StepCounter);
        Assert.NotNull(Send(session, new Move(ExplorationDirection.South)).Failure);
    }

    [Fact]
    public void UnsupportedMarkersAndTravellingMapReplacementFailExplicitly()
    {
        var session = Start("caravan-travel");
        var world = session.Current.Exploration!.WithTravelMode(FieldTravelMode.Caravan);
        var words = world.Layout.Words.ToArray(); words[3 * 64 + 3] = 0x1000;
        var exception = Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() =>
            MapEventDispatcher.Move(world.WithLayout(new(words)), ExplorationDirection.East, [65]));
        Assert.Equal("caravan-unsupported-marker", exception.Code);
        exception = Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() => CaravanRuntime.RequireOrdinaryMapOperation(world));
        Assert.Equal("caravan-map-replacement", exception.Code);
        var document = Document("caravan-travel");
        document["world"]!["maps"]![0]!["layout"]![3]![3] = 0x1000;
        Assert.Equal("caravan-unsupported-marker", Assert.IsType<ScenarioReadRejected>(Reader(document).Read()).Failure.Code);
    }

    private static ExplorationDirection Direction(byte facing) => facing switch
    { 0 => ExplorationDirection.East, 1 => ExplorationDirection.North, 2 => ExplorationDirection.West, _ => ExplorationDirection.South };

    private static void Deliver(GameSession session, List<SessionObservation>? observations = null)
    {
        var pending = session.Current.Exploration!.AllEntities.FirstOrDefault(actor => actor.WaitingForSprite && actor.SpriteReady != actor.SpriteRequest);
        var result = pending is not null ? Accept(session, new EntitySpriteReady(pending.Slot, pending.SpriteRequest)) :
            session.Current.Story.Wait is PresentationWait present ? Accept(session, new CompletePresentation(present.Token, present.Cue.Kind)) :
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait?.Token));
        observations?.AddRange(result.Observations);
    }

    private static GameSession Pilot(int x = 2, int y = 2, byte facing = 0, int followers = 0, bool missing = false, bool stall = false, bool firstFollowerStalls = false, bool alternate = false)
    {
        var battle = Admitted();
        var player = new EntityRef("hero");
        var words = new ushort[4096];
        int dx = facing == 0 ? 1 : facing == 2 ? -1 : 0;
        int dy = facing == 3 ? 1 : facing == 1 ? -1 : 0;
        words[(y + dy) * 64 + x + dx] = 0xF800;
        var caravan = new CaravanDefinition(new(missing ? "absent" : alternate ? "friend-0" : "wagon"),
            Enumerable.Range(1, followers + 1).ToArray(), true, new(new("medic-a"), Sf2.Remake.Domain.Battles.BattleClassRule.UnpromotedPriest, 7, 143));
        var actors = new List<ExplorationEntityDefinition> { new(new("wagon"), new(1, 1), 3,
            stall ? (ushort)0 : (ushort)32, Sprite: 62) };
        actors.AddRange(Enumerable.Range(0, followers).Select(i => new ExplorationEntityDefinition(new("friend-" + i), new(2, 2), 0, firstFollowerStalls && i == 0 ? (ushort)0 : (ushort)32, Sprite: 10 + i)));
        var map = new ExplorationMapDefinition(new("field"), new(words), new([new(0, 0, 10, 10)]), actors, [], caravan: caravan);
        var exploration = new ExplorationDefinition([map], CaravanPrograms.Create(caravan, player));
        var definition = new ScenarioDefinition("caravan-pilot", battle.Definition.Encounters.Values, exploration: exploration);
        return Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(map.Map, player, new(x, y), facing, 32, [65], battle.Start))).Session;
    }
}
