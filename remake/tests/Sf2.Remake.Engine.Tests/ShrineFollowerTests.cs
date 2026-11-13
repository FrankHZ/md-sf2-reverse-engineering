using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ShrineFollowerTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void FollowerGuardUsesTheSpecifiedPartyMemberBeforeResolvingEitherEntity(ushort hp, bool follows)
    {
        var session = Start("harbor-arrival");
        var field = session.Current.Exploration!;
        ActorRef guard = new("medic-a");
        var party = field.Party.WithResources(field.Party.Actors.Select(actor =>
            actor.Actor == guard ? actor with { Hp = hp } : actor), field.Party.Gold);
        var follower = follows ? new EntityRef("ferryman") : new EntityRef("removed-follower");
        var leader = follows ? field.Player : new EntityRef("removed-leader");
        var scene = new StoryProgram("shrine", [
            new FollowEntity(follower, leader, -24, 0, guard),
            new WriteFlag(730, true), new WaitProgramTicks(1), new EndProgram()]);
        var definition = new ScenarioDefinition("shrine", session.Definition.Encounters.Values,
            exploration: new([field.Definition], [scene]));
        var snapshot = new SessionSnapshot(session.Current.SessionId, 1, 1, new ActiveExploration(field.WithParty(party)),
            new StoryState(cursor: new(scene.Id, 0)), SessionStopReason.SimulationWait);
        var result = ProgramRunner.Run(definition, snapshot, []);
        Assert.Null(result.Failure);
        Assert.Contains(730, result.Snapshot.Story.Flags);
        var actor = result.Snapshot.Exploration!.Entities[new("ferryman")];
        Assert.Equal(follows ? new EntityFollower(field.PlayerEntity.Slot, -24, 0) : null, actor.Follower);
    }
}
