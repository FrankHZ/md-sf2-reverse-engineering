using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
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

    private static ExplorationDirection Direction(byte facing) => facing switch
    { 0 => ExplorationDirection.East, 1 => ExplorationDirection.North, 2 => ExplorationDirection.West, _ => ExplorationDirection.South };

    private static void Deliver(GameSession session)
    {
        var pending = session.Current.Exploration!.AllEntities.FirstOrDefault(actor => actor.WaitingForSprite && actor.SpriteReady != actor.SpriteRequest);
        if (pending is not null) Accept(session, new EntitySpriteReady(pending.Slot, pending.SpriteRequest));
        else if (session.Current.Story.Wait is PresentationWait present)
            Accept(session, new CompletePresentation(present.Token, present.Cue.Kind));
        else Accept(session, new AdvanceSimulation(session.Current.Story.Wait?.Token));
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
            Enumerable.Range(1, followers + 1).ToArray(), [], 7);
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
