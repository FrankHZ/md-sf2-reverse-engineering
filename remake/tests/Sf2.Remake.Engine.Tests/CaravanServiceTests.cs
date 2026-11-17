using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class CaravanServiceTests
{
    private static readonly MapPartyFlagLayout Layout = new(20, 0, 32, 12);
    internal static GameSession Session(int active = 12, int count = 14, int? dead = null, bool failReturn = false,
        BattleItemCustody? custody = null, Func<int, BattleSourceLoadout>? pack = null,
        FieldPreparationDefinition? preparation = null, PrivateBattleDefinitions? privateDefinitions = null,
        bool equipment = false, Func<int, byte>? status = null)
    {
        var basis = FieldInventoryTests.Session();
        var source = basis.Definition.Encounters.Values.Single();
        var prototype = source.Deployments[0].Definition;
        var deployments = Enumerable.Range(0, count).Select(i => new BattleDeploymentDefinition(
            new(new("member-" + i), prototype.ClassRule, 2, 30, 8, 20, 12, 10, false, 5, [],
                prototype.Physical, sourceLoadout: pack?.Invoke(i) ?? new([127, 127, 127, 127], []),
                equipment: equipment ? prototype.Equipment : null), BattleFaction.Ally, i,
            BattleControl.Player, null, new(i % 6, i / 6), new(null, 0, 0, 0, 0, AllyPartyMember: i))).ToArray();
        var battle = new BattleDefinition(source.Encounter, source.Map, source.Width, source.Height, source.Terrain, deployments, []);
        EntityRef npc = new("keeper");
        var map = new ExplorationMapDefinition(new("village"), new(new ushort[WorkingMapLayout.WordCount]),
            new([new(0, 0, 63, 63)]), [new(npc, new(2, 1), 3, 32)],
            [new(ExplorationEventKind.Interact, null, null, npc, new("caller", 0))]);
        var world = new ExplorationDefinition([map],
            [new("caller", [new CallProgram(new("service", 0)), new WriteFlag(999, true),
                failReturn ? new UnsupportedInstruction("unavailable", "caller") : new EndProgram()]),
             new("service", [new OpenCaravan(), new ReturnProgram()])], [], partyFlags: Layout);
        var party = new BattleStartInput(battle.Encounter, deployments.Select((row, i) =>
            new BattleActorStartInput(row.Actor, (ushort)(i == dead ? 0 : 25), 6, (byte)i, 2, 3, status?.Invoke(i) ?? 0, null,
                SourceLoadout: row.Definition.SourceLoadout)), 123, 456, 789,
            activeAllies: deployments.Take(active).Select(row => row.Actor), itemCustody: custody ?? new([], new Dictionary<byte, byte>()));
        var flags = Enumerable.Range(0, count).Concat(Enumerable.Range(32, active)).Append(70).Append(787);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(new("formation-test", [battle], privateDefinitions, exploration: world) { FieldPreparation = preparation ?? basis.Definition.FieldPreparation },
            new ExplorationStartInput(map.Map, new("player"), new(2, 2), 1, 32, flags, party))).Session;
        Accept(session, new Interact(npc));
        return session;
    }
    private static CaravanWait Wait(GameSession session) => Assert.IsType<CaravanWait>(session.Current.Story.Wait);
    private static SessionResult Send(GameSession session, SessionCommand command) => session.Submit(new(session.Current.SessionId, session.Current.Revision, null, command));
    private static SessionResult Accept(GameSession session, SessionCommand command) { var result = Send(session, command); Assert.Null(result.Failure); return result; }
    private static void Choose(GameSession s, CaravanAction a) => Accept(s, new ChooseCaravan(Wait(s).Token, a));
    private static void Select(GameSession s, int i) => Accept(s, new SelectCaravanMember(Wait(s).Token, i));
    private static void Continue(GameSession s) => Accept(s, new ContinueCaravan(Wait(s).Token));
    private static void Back(GameSession s) => Accept(s, new BackCaravan(Wait(s).Token));
    private static void Preserved(BattleStartInput before, BattleStartInput after)
    {
        Assert.Equal(before.Actors, after.Actors); Assert.Equal(before.Gold, after.Gold);
        Assert.Equal(before.MainSeed, after.MainSeed); Assert.Equal(before.ThinkingSeed, after.ThinkingSeed);
        Assert.Same(before.ItemCustody, after.ItemCustody); Assert.Equal(before.StoryFlags, after.StoryFlags);
    }

    [Theory]
    [InlineData(12, 1)] [InlineData(13, 7)]
    public void FullFormationExchangesSelectedReserveAndRetainsEveryResource(int recruit, int relief)
    {
        var s = Session(); var before = s.Current.Exploration!.Party;
        Choose(s, CaravanAction.Join); Select(s, recruit);
        Assert.Equal(CaravanPhase.Relief, Wait(s).Phase); Assert.Same(before, s.Current.Exploration!.Party);
        Select(s, relief);
        Assert.Equal(CaravanResult.Joined, Wait(s).Result);
        Assert.DoesNotContain(32 + relief, s.Current.Story.Flags); Assert.Contains(32 + recruit, s.Current.Story.Flags);
        Assert.Contains(relief, s.Current.Story.PartyLists!.Joined); Assert.Contains(relief, s.Current.Story.PartyLists.Reserve);
        Assert.Equal(12, s.Current.Story.PartyLists.Active.Count);
        Assert.Equal(s.Current.Story.PartyLists.Active.Select(i => new ActorRef("member-" + i)), s.Current.Exploration!.Party.ActiveAllies);
        Preserved(before, s.Current.Exploration.Party);
        Continue(s); Back(s); Back(s);
        Assert.True(s.Current.CanWaitAtInput); Assert.Contains(999, s.Current.Story.Flags);
        Assert.Contains(70, s.Current.Story.Flags); Assert.Contains(787, s.Current.Story.Flags); Assert.Empty(s.Current.Story.Callers);
        Assert.Equal(new MapPosition(2, 2), s.Current.Exploration!.PlayerEntity.Position);
        var exited = s.Current; Assert.NotNull(Send(s, new BackCaravan(new(1))).Failure); Assert.Same(exited, s.Current);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void DeadCandidateRequiresExplicitAnswerAndDeclineDoesNotChangeParty(bool accept)
    {
        var s = Session(active: 11, dead: 13); var before = s.Current.Exploration!.Party;
        Choose(s, CaravanAction.Join); Select(s, 13); Assert.Equal(CaravanPhase.ConfirmDead, Wait(s).Phase);
        Accept(s, new AnswerCaravan(Wait(s).Token, accept));
        Assert.Equal(accept, s.Current.Story.Flags.Contains(45)); Preserved(before, s.Current.Exploration!.Party);
        Assert.Equal(0, s.Current.Exploration.Party.Actors.Single(a => a.Actor.Value == "member-13").Hp);
        Assert.Equal(accept ? CaravanPhase.Result : CaravanPhase.Member, Wait(s).Phase);
    }

    [Fact]
    public void ReliefCancelAndLeaderRefusalReturnToReserveSelectionWithoutMutation()
    {
        var s = Session(); var before = s.Current.Exploration!.Party; var flags = s.Current.Story.Flags;
        Choose(s, CaravanAction.Join); Select(s, 13); Back(s);
        Assert.Equal(CaravanPhase.Member, Wait(s).Phase); Assert.Null(Wait(s).Member);
        Select(s, 12); Select(s, 0); Assert.Equal(CaravanResult.LeaderRequired, Wait(s).Result); Continue(s);
        Assert.Equal(CaravanPhase.Member, Wait(s).Phase); Assert.Same(before, s.Current.Exploration!.Party); Assert.Equal(flags, s.Current.Story.Flags);
    }

    [Fact]
    public void PurgeAndRejoinPreserveJoinedStateAndLeaderCannotBePurged()
    {
        var s = Session(); var before = s.Current.Exploration!.Party;
        Choose(s, CaravanAction.Purge); Select(s, 0); Assert.Equal(CaravanResult.LeaderRequired, Wait(s).Result); Continue(s);
        Select(s, 8); Assert.Contains(8, s.Current.Story.Flags); Assert.DoesNotContain(40, s.Current.Story.Flags);
        Continue(s); Back(s); Choose(s, CaravanAction.Join); Select(s, 8);
        Assert.Equal(CaravanResult.Joined, Wait(s).Result); Assert.Contains(40, s.Current.Story.Flags); Preserved(before, s.Current.Exploration!.Party);
    }

    [Fact]
    public void NoReserveAndItemSubmenuReturnToRootAndKeepControl()
    {
        var s = Session(count: 12); var before = s.Current.Exploration!.Party;
        Choose(s, CaravanAction.Join); Assert.Equal(CaravanResult.NoReserve, Wait(s).Result); Continue(s);
        Choose(s, CaravanAction.Item);
        Assert.Equal(CaravanPhase.ItemMenu, Wait(s).Phase);
        Back(s);
        Assert.Equal(CaravanPhase.Menu, Wait(s).Phase);
        Assert.Same(before, s.Current.Exploration!.Party);
    }

    [Fact]
    public void StaleAndRepeatedCommandsAndOtherInputCannotBypassTheService()
    {
        var s = Session(); var stale = new ChooseCaravan(Wait(s).Token, CaravanAction.Join); Accept(s, stale);
        foreach (var c in new SessionCommand[] { stale, new SelectCaravanMember(Wait(s).Token, 999),
            new AdvanceSimulation(Wait(s).Token), new Move(ExplorationDirection.South), new Interact(new("keeper")) })
        { var before = s.Current; Assert.NotNull(Send(s, c).Failure); Assert.Same(before, s.Current); }
        var old = s.Current; Select(s, 12);
        var rejected = s.Submit(new(old.SessionId, old.Revision, null, new SelectCaravanMember(Wait(s).Token, 1)));
        Assert.Equal("stale-input", rejected.Failure!.Code); Assert.Equal(CaravanPhase.Relief, Wait(s).Phase);
    }

    [Fact]
    public void FailureAfterServiceReturnRetainsTheCommittedFormation()
    {
        var s = Session(active: 11, failReturn: true); Choose(s, CaravanAction.Join); Select(s, 13); Continue(s); Back(s);
        var result = Send(s, new BackCaravan(Wait(s).Token)); Assert.NotNull(result.Failure);
        Assert.Contains(45, s.Current.Story.Flags); Assert.Contains(new ActorRef("member-13"), s.Current.Exploration!.Party.ActiveAllies!);
        Assert.Contains(999, s.Current.Story.Flags);
    }
}
