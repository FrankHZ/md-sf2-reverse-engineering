using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleMemberQueryTests
{
    [Fact]
    public void AuthoredRosterReadsTheSameLiveScalarsAsInspectionWithoutMutatingTheSession()
    {
        var session = Start("battle-equipment"); var before = session.Current;
        var roster = session.QueryBattleMembers();
        Assert.True(roster.Available); Assert.NotEmpty(roster.Members);
        Assert.All(roster.Members, member =>
        {
            var detail = session.QueryBattleMembers(roster.SessionId, roster.Revision, member.Actor).Unit!;
            var inspection = session.QueryBattleInspection(unit: member.Actor).Unit!;
            Assert.Equal(inspection.Actor, detail.Actor); Assert.Equal(inspection.Hp, detail.Hp);
            Assert.Equal(inspection.Attack, detail.Attack); Assert.Equal(inspection.Inventory, detail.Inventory);
            Assert.Equal(inspection.Spells, detail.Spells); Assert.Equal(inspection.Status, detail.Status);
        });
        Assert.Same(before, session.Current); // Includes both seeds, queue, resources, story and selection.
        Assert.Throws<NotSupportedException>(() => ((IList<BattleMemberEntry>)roster.Members).Clear());
        Assert.Equal("member-not-disclosed", session.QueryBattleMembers(member: new("dummy-a")).Reason);
        Assert.Equal("member-not-disclosed", session.QueryBattleMembers(member: new("missing")).Reason);
        Assert.Same(before, session.Current);
    }

    [Theory]
    [InlineData(true, true, 0, false, true)] // Defeated active member, with retained live data.
    [InlineData(true, true, 9, false, true)] // Active, alive but unplaced.
    [InlineData(true, true, 9, false, false)] // Active identity, unavailable live state.
    [InlineData(true, false, 9, true, true)] // Joined reserve is not the battle Member list.
    [InlineData(false, true, 9, true, true)] // An active bit without joining grants nothing.
    public void ActiveForceMembershipIsIndependentOfLivingBoardDisclosure(bool joined, bool active,
        ushort hp, bool placed, bool initialized)
    {
        var session = Start(); var current = CurrentWithMembers(session, joined, active, hp, placed, initialized);
        var target = new ActorRef("guard-a");
        var query = BattleMemberQuery.Query(session.Definition, current, requested: target);
        if (!joined || !active)
        {
            Assert.False(query.Available); Assert.Empty(query.Members); Assert.Null(query.Unit);
            return;
        }
        Assert.True(query.Available); Assert.Contains(query.Members, m => m.Actor == target);
        if (!initialized)
        {
            Assert.Null(query.Unit); Assert.Equal("member-unavailable", query.Reason);
            Assert.False(query.Members.Single(m => m.Actor == target).Available);
        }
        else
        {
            Assert.Equal(hp, query.Unit!.Hp); Assert.Null(query.Unit.Position);
            Assert.Equal(hp == 0, query.Members.Single(m => m.Actor == target).Defeated);
            Assert.False(query.Members.Single(m => m.Actor == target).Deployed);
            Assert.Equal(0, query.Unit.Mp); Assert.NotEmpty(query.Unit.Inventory);
        }
        Assert.DoesNotContain(BattleInspectionQuery.Query(session.Definition, current).Units, m => m.Actor == target);
    }

    [Fact]
    public void SourceRosterRebuildsLiveFlagsRatherThanTrustingACountedListFromBeforeTheActiveWrite()
    {
        var session = Start();
        var current = CurrentWithMembers(session, true, true, 9, true, true);
        var definition = new ScenarioDefinition("source-membership", session.Definition.Encounters.Values,
            battleStory: new BattleStoryDefinition([], new Dictionary<int, string>()));
        // A prior counted list can be stale after source JoinForce's conditional active-bit write.
        current = current.WithStory(new StoryState([0, 1, 32, 33], partyLists: new([0, 1], [0], [1])));
        Assert.Equal(new[] { "medic-a", "guard-a" }, BattleMemberQuery.Query(definition, current).Members.Select(m => m.Actor.Value));
        current = current.WithStory(new StoryState([0, 1, 32], partyLists: new([0, 1], [0, 1], [])));
        Assert.Equal("medic-a", Assert.Single(BattleMemberQuery.Query(definition, current).Members).Actor.Value);
    }

    [Fact]
    public void EmptyMembershipAndStaleBusyAndReplacementContextsDiscloseNoMembers()
    {
        var session = Start(); var current = CurrentWithMembers(session, false, false, 9, true, true);
        var empty = current.WithStory(new StoryState(partyLists: new([], [], [])));
        var query = BattleMemberQuery.Query(session.Definition, empty);
        Assert.True(query.Available); Assert.Empty(query.Members);
        var first = session.QueryBattleMembers();
        Assert.Equal("stale-inspection", session.QueryBattleMembers(Guid.NewGuid(), first.Revision).Reason);
        Accept(session, new Confirm());
        Assert.False(session.QueryBattleMembers().Available); Assert.Empty(session.QueryBattleMembers().Members);
        Assert.Equal("stale-inspection", session.QueryBattleMembers(first.SessionId, first.Revision).Reason);
        Assert.False(Start("harbor-arrival").QueryBattleMembers().Available);
        var story = empty.WithStory(new StoryState(cursor: new("control", 0)));
        Assert.Empty(BattleMemberQuery.Query(session.Definition, story).Members);
    }

    private static SessionSnapshot CurrentWithMembers(GameSession session, bool joined, bool active,
        ushort hp, bool placed, bool initialized)
    {
        var current = session.Current;
        var actors = current.Battle.Actors.Select(a =>
        {
            if (!a.IsAlly) return a;
            int member = a.Actor.Value == "medic-a" ? 0 : 1;
            var deployment = a.Deployment with { Initialization = new(null, 0, 0, 0, 0, AllyPartyMember: member) };
            return new BattleActorState(deployment, member == 0 ? a.Hp : hp, a.Mp, a.Exp,
                member == 0 || placed ? a.Position : null, a.Kills, a.Defeats, status: a.Status,
                initialized: member == 0 || initialized);
        });
        var lists = MapPartyMembership.Rebuild((joined ? new[] { 0, 1, 32 } : new[] { 0, 32 }).Concat(active ? [33] : []).ToArray(),
            new(30, 0, 32, 12));
        return new(current.SessionId, current.Revision, current.ObservationSequence,
            ((ActiveBattle)current.Active) with { Battle = current.Battle.With(actors: actors) },
            new StoryState(partyLists: lists), current.StopReason);
    }
}
