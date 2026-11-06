using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class FollowerTransferTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(3, false)]
    [InlineData(3, true)]
    public void PreservingArrivalSettlesTheLivePhysicalChainAcrossDifferentLandings(int count, bool reverse)
    {
        var (definition, initial) = Start(joined: count > 0);
        var world = initial.Exploration!;
        world = world.WithEntity(world.PlayerEntity with { Motion = Moving(world.PlayerEntity.Position),
            Actions = new([new StopEntityActions()]), WaitingForMotion = true });
        // Physical order deliberately differs from character identity and, optionally, chain order.
        int[] slots = reverse ? [7, 1, 5] : [1, 5, 7];
        int leader = world.PlayerEntity.Slot;
        foreach (int slot in slots.Take(count))
        {
            var reference = world.AllEntities.FirstOrDefault(e => e.Slot == slot)?.Entity ?? new("entity-" + (20 - slot));
            var entity = new ExplorationEntity(reference, Moving(new(30 + slot, 30)), true,
                Slot: slot, Follower: new(leader, -24, 0), WaitingForMotion: true);
            world = world.WithEntities(world.AllEntities.Where(e => e.Slot != slot).Append(entity)).WithFollower(entity);
            leader = slot;
        }
        var scripted = new ExplorationEntity(new("separated"), Moving(new(9, 9)), true,
            new([new WaitEntityTicks(20)]), Slot: 9, Follower: new(0, -24, 0));
        var npc = new ExplorationEntity(new("npc"), Moving(new(10, 10)), true, Slot: 10);
        var independent = new ExplorationEntity(new("npc-follower"), Moving(new(11, 10)), true,
            Slot: 11, Follower: new(10, -24, 0));
        var separatedTail = new ExplorationEntity(new("separated-tail"), Moving(new(12, 10)), true,
            Slot: 12, Follower: new(9, -24, 0));
        world = world.WithEntities(world.AllEntities.Append(scripted).Append(npc).Append(independent).Append(separatedTail));
        var current = Snapshot(initial, world);
        foreach (var landing in new[] { new MapPosition(39, 53), new(39, 39), new(25, 50), new(25, 35) })
        {
            var before = current.Exploration!;
            current = MapTransfer.Apply(definition, current, before.Map, landing, 2,
                MapLoadMode.Preserve, current.Story, [], RuleCompositions.Sf2());
            var after = current.Exploration!;
            AssertSettled(after.PlayerEntity, landing);
            Assert.False(after.PlayerEntity.WaitingForMotion);
            foreach (int slot in slots.Take(count))
            {
                var follower = after.AllEntities.Single(e => e.Slot == slot);
                AssertSettled(follower, landing);
                Assert.Equal(before.AllEntities.Single(e => e.Slot == slot).Follower, follower.Follower);
                Assert.False(follower.WaitingForMotion);
                Assert.Equal((ushort)32, follower.Motion.XSpeed);
            }
            foreach (var retained in new[] { scripted, npc, independent, separatedTail })
                Assert.Equal(retained, after.AllEntities.Single(e => e.Slot == retained.Slot));
            Assert.Same(before.Layout, after.Layout);
            Assert.Same(before.Party, after.Party);
            Assert.Equal(before.Aliases, after.Aliases);
            Assert.Equal(before.CurrentEntities, after.CurrentEntities);
        }
    }

    [Fact]
    public void RebuildUsesDestinationPopulationAndFlagsInsteadOfRetainingTheOldChain()
    {
        var (definition, initial) = Start();
        var world = initial.Exploration!;
        world = world.WithEntity(world.AllEntities.Single(e => e.Follower is not null) with { Motion = Moving(new(8, 8)) });
        var current = Snapshot(initial, world);
        var result = MapTransfer.Apply(definition, current, new("map-57"), new(4, 5), 1,
            MapLoadMode.Rebuild, current.Story, [], RuleCompositions.Sf2());
        Assert.Equal(new MapId("map-57"), result.Exploration!.Map);
        Assert.All(result.Exploration.AllEntities, e => AssertSettled(e, new(4, 5)));
        Assert.Equal(new EntityFollower(0, -24, 0), Assert.Single(result.Exploration.AllEntities, e => e.Follower is not null).Follower);
        Assert.DoesNotContain(result.Exploration.AllEntities, e => e.Entity == new EntityRef("entity-142"));
        Assert.Same(world.Party, result.Exploration.Party);
    }

    [Fact]
    public void SettledPreservingArrivalKeepsTheExistingSaveAndSubsequentMovementRules()
    {
        var (definition, initial) = Start();
        var world = initial.Exploration!;
        world = world.WithEntity(world.AllEntities.Single(e => e.Follower is not null) with { Motion = Moving(new(8, 8)) });
        var current = Snapshot(initial, world);
        Assert.Equal("save-entity-unsettled", OpeningSave.Query(definition, current, RuleCompositions.Sf2()).Error);
        current = MapTransfer.Apply(definition, current, world.Map, new(32, 13), 1,
            MapLoadMode.Preserve, current.Story, [], RuleCompositions.Sf2());
        // MapTransfer publishes MapLoaded; its on-load caller still owns control until it returns.
        Assert.False(OpeningSave.Query(definition, current, RuleCompositions.Sf2()).Allowed);
        current = ProgramRunner.Run(definition, current, [], RuleCompositions.Sf2()).Snapshot;
        Assert.True(OpeningSave.Query(definition, current, RuleCompositions.Sf2()).Allowed);
        var moved = ExplorationDispatcher.Submit(definition, current, new Move(ExplorationDirection.South), RuleCompositions.Sf2());
        Assert.Null(moved.Failure);
        Assert.False(OpeningSave.Query(definition, moved.Snapshot, RuleCompositions.Sf2()).Allowed);
    }

    private static (ScenarioDefinition Definition, SessionSnapshot Snapshot) Start(bool joined = true)
    {
        var (definition, data) = OpeningRestoreTests.Fixture(joined);
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        return (definition, session.Current);
    }

    private static SessionSnapshot Snapshot(SessionSnapshot initial, ExplorationState world) =>
        new(initial.SessionId, initial.Revision, initial.ObservationSequence, new ActiveExploration(world), initial.Story, initial.StopReason);

    private static EntityMotionState Moving(MapPosition position) => EntityMotionState.At(position, 3, 32) with
    {
        XDestination = (short)((position.X + 1) * 384), YDestination = (short)((position.Y + 1) * 384),
        XVelocity = 32, YVelocity = 32, XTravel = 384, YTravel = 384, WaitTimer = 9,
    };

    private static void AssertSettled(ExplorationEntity entity, MapPosition position)
    {
        Assert.Equal(position, entity.Position);
        Assert.False(entity.Motion.IsMoving);
        Assert.Equal((short)0, entity.Motion.XVelocity); Assert.Equal((short)0, entity.Motion.YVelocity);
        Assert.Equal((ushort)0, entity.Motion.XTravel); Assert.Equal((ushort)0, entity.Motion.YTravel);
        Assert.Equal((byte)0, entity.Motion.WaitTimer);
    }
}
