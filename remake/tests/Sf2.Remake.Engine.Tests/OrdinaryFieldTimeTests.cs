using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class OrdinaryFieldTimeTests
{
    [Theory]
    [InlineData(ExplorationDirection.North, 0u)]
    [InlineData(ExplorationDirection.East, 0x1234abcdu)]
    [InlineData(ExplorationDirection.South, 7u)]
    [InlineData(ExplorationDirection.West, uint.MaxValue)]
    public void PlayerReadinessDoesNotFreezeFollowersOrRelaxSave(ExplorationDirection direction, uint seed)
    {
        var (definition, data) = OpeningRestoreTests.Fixture(joined: true);
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data with { MainSeed = seed }, RuleCompositions.Sf2())).Session;
        var world = session.Current.Exploration!;
        var follower = world.AllEntities.Single(e => e.Follower is not null);
        var player = world.PlayerEntity;
        byte facing = direction switch { ExplorationDirection.East => 0, ExplorationDirection.North => 1,
            ExplorationDirection.West => 2, _ => 3 };
        int spacing = 2 + (int)(seed % 3);
        var position = new MapPosition(player.Position.X - spacing, player.Position.Y + spacing);
        world = world.WithEntity(player with { Motion = player.Motion with { Facing = facing } })
            .WithEntity(follower with { Motion = EntityMotionState.At(position, facing, 32) });
        var current = new SessionSnapshot(session.Current.SessionId, 0, 0, new ActiveExploration(world), session.Current.Story, SessionStopReason.PlayerInput);
        void Step()
        {
            var result = ExplorationDispatcher.Submit(definition, current, new WaitAtInput(), session.Rules);
            Assert.Null(result.Failure); current = result.Snapshot;
        }
        Step();
        Assert.True(current.CanWaitAtInput);
        Assert.Contains(current.Exploration!.AllEntities, e => e.Follower is not null && e.Motion.IsMoving);
        Assert.Equal("save-entity-unsettled", OpeningSave.Query(definition, current, session.Rules).Error);
        for (int i = 0; !OpeningSave.Query(definition, current, session.Rules).Allowed && i < 120; i++) Step();
        Assert.True(OpeningSave.Query(definition, current, session.Rules).Allowed);
        Assert.All(current.Exploration!.AllEntities.Where(e => e.Follower is not null), e => Assert.False(e.Motion.IsMoving));
        Assert.Equal(data.Gold, current.Exploration.Party.Gold);
    }

    [Theory]
    [InlineData(14, 0, true, 15)]
    [InlineData(28, 16, true, 0)]
    [InlineData(14, 16, true, 17)]
    [InlineData(255, 16, true, 255)]
    [InlineData(14, 8, true, 14)]
    [InlineData(14, 0, false, 14)]
    public void OrdinarySpritePhaseIsIndependentOfDisabledEntities(int counter, int flags, bool visible, int expected)
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var before = session.Current; var world = before.Exploration!;
        world = world.WithEntity(world.PlayerEntity with { Visible = visible,
            Motion = world.PlayerEntity.Motion with { AnimationCounter = (byte)counter, FlagsB = (byte)flags } });
        before = new(before.SessionId, before.Revision, before.ObservationSequence, new ActiveExploration(world),
            before.Story.Copy(null, entityServices: false), before.StopReason);
        var after = ExplorationDispatcher.Submit(definition, before, new WaitAtInput(), session.Rules);
        Assert.Null(after.Failure);
        Assert.Equal((byte)expected, after.Snapshot.Exploration!.PlayerEntity.Motion.AnimationCounter);
        Assert.Equal(before.Exploration!.Party.MainSeed, after.Snapshot.Exploration.Party.MainSeed);
        Assert.True(after.Snapshot.Revision > before.Revision);
    }

    [Fact]
    public void MatchedActionsAndLogicalOpportunitiesKeepNpcCollisionAndSharedSeedAcrossPartitions()
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var world = session.Current.Exploration!;
        foreach (var npc in world.AllEntities.Where(e => e.Slot != world.PlayerEntity.Slot).ToArray())
            world = world.WithEntity(npc with { Actions = new([new RandomWalkEntity(npc.Position, 1),
                new WaitEntityTicks(3), new JumpEntityAction(0)]), ActionCursor = 0 });
        var initial = new SessionSnapshot(session.Current.SessionId, 0, 0, new ActiveExploration(world), session.Current.Story, SessionStopReason.PlayerInput);
        SessionSnapshot Run(int[] partitions)
        {
            var current = initial;
            void Send(SessionCommand command)
            {
                var result = ExplorationDispatcher.Submit(definition, current, command, session.Rules);
                Assert.Null(result.Failure); current = result.Snapshot;
            }
            void Time(int steps)
            {
                long target = current.Story.SimulationTick + steps;
                for (int i = 0; current.Story.SimulationTick < target; i++)
                    Send(new AdvanceSimulation(current.Story.Wait?.Token,
                        (int)Math.Min(partitions[i % partitions.Length], target - current.Story.SimulationTick)));
            }
            Send(new WaitAtInput()); Time(11); Send(new Move(ExplorationDirection.South)); Time(35);
            return current;
        }
        var single = Run([1]); var batched = Run([4, 2, 3]);
        Assert.Equal(single.Story.SimulationTick, batched.Story.SimulationTick);
        Assert.Equal(single.Exploration!.AllEntities, batched.Exploration!.AllEntities);
        Assert.Equal(single.Exploration.Party.MainSeed, batched.Exploration.Party.MainSeed);
        Assert.NotEqual(data.MainSeed, single.Exploration.Party.MainSeed);
        Assert.Equal(single.Exploration.Party.ThinkingSeed, batched.Exploration.Party.ThinkingSeed);
    }

    [Fact]
    public void AnimationOnlyOpportunityInvalidatesAnOfferWithoutChangingPersistentResources()
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
        var offer = session.QuerySave(); var before = session.CaptureSave(offer.SessionId, offer.Revision).Data!;
        EngineTestContent.Accept(session, new WaitAtInput());
        Assert.True(session.Current.Revision > offer.Revision);
        Assert.Equal("save-stale-revision", session.CaptureSave(offer.SessionId, offer.Revision).Error);
        var after = session.CaptureSave(session.Current.SessionId, session.Current.Revision).Data!;
        Assert.Equal(before.MainSeed, after.MainSeed); Assert.Equal(before.ThinkingSeed, after.ThinkingSeed);
        Assert.Equal(before.Gold, after.Gold); Assert.Equivalent(before.Allies, after.Allies);
        Assert.Equal(before.Flags, after.Flags);
    }
}
