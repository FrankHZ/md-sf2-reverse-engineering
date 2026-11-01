using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldServiceTests
{
    private static readonly ActorRef First = FieldInventoryTests.Giver, Second = FieldInventoryTests.Recipient;
    private static readonly ChurchDefinition Terms = new(10, 200, 200, 10, 20);
    private static GameSession Session(uint? gold = 100, ushort hp = 0, ushort status = 0,
        ushort secondHp = 100, ushort secondStatus = 0, byte level = 2, bool promoted = false, ushort maxHp = 250,
        ChurchDefinition? terms = null, byte? currentLevel = null, bool failReturn = false)
    {
        var original = FieldInventoryTests.Session().Current.Exploration!.Party;
        var source = FieldInventoryTests.Session().Definition.Encounters.Values.Single();
        var deployments = source.Deployments.Select(row => row.Actor != First ? row : row with
        {
            Definition = new(row.Actor, row.Definition.ClassRule, level, maxHp, row.Definition.MaxMp,
                row.Definition.Attack, row.Definition.Defense, row.Definition.Agility, false, row.Definition.Move,
                row.Definition.Spells, row.Definition.Physical! with { Promoted = promoted }, sourceLoadout: row.Definition.SourceLoadout),
        });
        var battle = new BattleDefinition(source.Encounter, source.Map, source.Width, source.Height,
            source.Terrain, deployments, source.Spells.Values);
        EntityRef priest = new("priest");
        var map = new ExplorationMapDefinition(new("village"), new(new ushort[WorkingMapLayout.WordCount]),
            new([new(0, 0, 63, 63)]), [new(priest, new(2, 1), 3, 32)],
            [new(ExplorationEventKind.Interact, null, null, priest, new("caller", 0))]);
        var world = new ExplorationDefinition([map],
            [new("caller", [new CallProgram(new("service", 0)), new WriteFlag(999, true),
                failReturn ? new UnsupportedInstruction("unadmitted", "authored-caller") : new EndProgram()]),
             new("service", [new OpenChurch(terms ?? Terms), new ReturnProgram()])], []);
        var definition = new ScenarioDefinition("service-behavior", [battle], exploration: world);
        var party = new BattleStartInput(battle.Encounter, original.Actors.Select(actor => actor.Actor == First
            ? actor with { Hp = hp, Status = status, Progress = currentLevel is { } attained
                ? new(attained, 76, 10, 20, 4, 60, [], actor.SourceLoadout) : null }
            : actor.Actor == Second ? actor with { Hp = secondHp, Status = secondStatus } : actor),
            original.MainSeed, original.ThinkingSeed, gold, original.NewBattle, original.ActiveAllies, original.ItemCustody);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(map.Map, new("player"), new(2, 2), 1, 32, [], party))).Session;
        Accept(session, new Interact(priest));
        Assert.IsType<ChurchWait>(session.Current.Story.Wait);
        return session;
    }
    private static ChurchWait Wait(GameSession session) => Assert.IsType<ChurchWait>(session.Current.Story.Wait);
    private static SessionResult Send(GameSession session, SessionCommand command) =>
        session.Submit(new(session.Current.SessionId, session.Current.Revision, null, command));
    private static SessionResult Accept(GameSession session, SessionCommand command)
    { var result = Send(session, command); Assert.Null(result.Failure); return result; }
    private static void Choose(GameSession session, ChurchAction action) => Accept(session, new ChooseChurch(Wait(session).Token, action));
    private static SessionResult Answer(GameSession session, bool yes = true) => Accept(session, new AnswerChurch(Wait(session).Token, Wait(session).Offer!, yes));
    private static void Continue(GameSession session) => Accept(session, new ContinueChurch(Wait(session).Token));
    private static BattleActorStartInput Member(GameSession session, ActorRef actor) => session.Current.Exploration!.Party.Actors.Single(row => row.Actor == actor);

    [Theory]
    [InlineData(2, false, 250, 20, 200)]
    [InlineData(7, true, 90, 270, 90)]
    public void RaiseUsesCurrentLevelPromotionCapAndExactFundsWithoutResettingTheParty(byte level, bool promoted, ushort maxHp, uint cost, ushort raisedHp)
    {
        var session = Session(gold: cost, level: level, promoted: promoted, maxHp: maxHp);
        var before = session.Current.Exploration!.Party;
        Choose(session, ChurchAction.Raise);
        Assert.Equal(cost, Wait(session).Offer!.Cost); Assert.Same(before, session.Current.Exploration!.Party);
        var result = Answer(session);
        Assert.Contains(result.Observations, row => row.Kind == "church-paid");
        Assert.Equal(raisedHp, Member(session, First).Hp); Assert.Equal(0u, session.Current.Exploration!.Party.Gold);
        Assert.Equal(before.Actors.Single(row => row.Actor == First) with { Hp = raisedHp }, Member(session, First));
        Assert.Same(before.Actors.Single(row => row.Actor == Second), Member(session, Second));
        Assert.Equal(before.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(before.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        Assert.Same(before.ItemCustody, session.Current.Exploration.Party.ItemCustody);
        Continue(session); Accept(session, new LeaveChurch(Wait(session).Token));
        Assert.True(session.Current.CanWaitAtInput); Assert.Contains(999, session.Current.Story.Flags);
        Assert.Empty(session.Current.Story.Callers);
    }

    [Fact]
    public void CureScansPoisonThenStunAndRechecksTheChangedStatusForLaterOffers()
    {
        var session = Session(gold: 60, hp: 0, status: 3, secondStatus: 3);
        var before = session.Current.Exploration!.Party;
        Choose(session, ChurchAction.Cure);
        foreach (var (actor, effect, gold) in new[] { (First, ChurchEffect.Poison, 50u), (Second, ChurchEffect.Poison, 40u),
            (First, ChurchEffect.Stun, 20u), (Second, ChurchEffect.Stun, 0u) })
        {
            Assert.Equal(actor, Wait(session).Offer!.Member); Assert.Equal(effect, Wait(session).Offer!.Effect);
            Answer(session); Assert.Equal(gold, session.Current.Exploration!.Party.Gold); Continue(session);
        }
        Assert.Null(Wait(session).Action); Assert.Equal(0, Member(session, First).Status);
        Assert.Equal(0, Member(session, First).Hp); Assert.Equal(0, Member(session, Second).Status);
        Assert.Equal(before.Actors.Select(row => row.Exp), session.Current.Exploration!.Party.Actors.Select(row => row.Exp));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void DeclineAndInsufficientRaisePreserveEveryResourceAndContinueToTheNextDeadMember(bool insufficient)
    {
        var session = Session(gold: 19, secondHp: 0); var before = session.Current.Exploration!.Party;
        Choose(session, ChurchAction.Raise); var result = Answer(session, insufficient);
        Assert.Contains(result.Observations, row => row.Kind == (insufficient ? "church-insufficient-gold" : "church-declined"));
        Assert.Same(before, session.Current.Exploration!.Party); Continue(session);
        Assert.Equal(Second, Wait(session).Offer!.Member);
    }

    [Fact]
    public void InsufficientCureSkipsRemainingPoisonMembersThenStillOffersStun()
    {
        var session = Session(gold: 9, hp: 100, status: 3, secondStatus: 2);
        Choose(session, ChurchAction.Cure); Answer(session); Continue(session);
        Assert.Equal(ChurchEffect.Stun, Wait(session).Offer!.Effect);
        Assert.Equal(3, Member(session, First).Status); Assert.Equal(2, Member(session, Second).Status);
    }

    [Theory]
    [InlineData(ChurchAction.Raise)] [InlineData(ChurchAction.Cure)]
    public void NoApplicableTargetAndRetryKeepControlsAndParty(ChurchAction action)
    {
        var session = Session(hp: 100); var before = session.Current.Exploration!.Party;
        Choose(session, action); Assert.Null(Wait(session).Offer); Assert.Contains("No applicable", Wait(session).Message);
        Continue(session); Choose(session, action); Continue(session);
        Accept(session, new LeaveChurch(Wait(session).Token, true));
        Assert.Same(before, session.Current.Exploration!.Party); Assert.True(session.Current.CanWaitAtInput);
    }

    [Fact]
    public void InvalidStaleAndDuplicateConfirmationCannotPayTwice()
    {
        var session = Session(); Choose(session, ChurchAction.Raise); var before = session.Current; var wait = Wait(session);
        foreach (var command in new ChurchCommand[] { new AnswerChurch(wait.Token, wait.Offer! with { Cost = 0 }, true),
            new AnswerChurch(new(wait.Token.Value - 1), wait.Offer!, true), new ChooseChurch(wait.Token, ChurchAction.Cure),
            new LeaveChurch(wait.Token) })
        { Assert.NotNull(Send(session, command).Failure); Assert.Same(before, session.Current); }
        var envelope = new CommandEnvelope(before.SessionId, before.Revision, null, new AnswerChurch(wait.Token, wait.Offer!, true));
        Assert.Null(session.Submit(envelope).Failure); var paid = session.Current;
        Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
        Assert.NotNull(Send(session, envelope.Command).Failure); Assert.Same(paid, session.Current);
    }

    [Fact]
    public void MissingGoldRejectsAtomicallyAndAllowsDeclineAndReturn()
    {
        var session = Session(gold: null); Choose(session, ChurchAction.Raise); var before = session.Current; var wait = Wait(session);
        Assert.Equal("service-gold-unavailable", Send(session, new AnswerChurch(wait.Token, wait.Offer!, true)).Failure!.Code);
        Assert.Same(before, session.Current); Answer(session, false); Continue(session);
        Accept(session, new LeaveChurch(Wait(session).Token)); Assert.True(session.Current.CanWaitAtInput);
    }

    [Fact]
    public void ContentOperandsAreConsumedAndUnsupportedCurseIsExplicit()
    {
        var session = Session(gold: 7, hp: 100, status: 6, terms: new(4, 50, 80, 7, 11));
        Choose(session, ChurchAction.Cure); Assert.Equal(7u, Wait(session).Offer!.Cost); Answer(session);
        Assert.Equal(4, Member(session, First).Status); Assert.Contains("Curse removal is unavailable", Wait(session).Message);
        Continue(session); Choose(session, ChurchAction.Cure); Assert.Null(Wait(session).Offer);
    }

    [Fact]
    public void CurrentProgressSuppliesRaiseCostAndMaximumInsteadOfTheDeploymentBaseline()
    {
        var session = Session(gold: 40, level: 1, maxHp: 250, currentLevel: 4);
        Choose(session, ChurchAction.Raise); Assert.Equal(40u, Wait(session).Offer!.Cost);
        Answer(session); Assert.Equal(76, Member(session, First).Hp); Assert.Equal(4, Member(session, First).Progress!.Level);
    }

    [Fact]
    public void OrdinaryCommandsCannotAdvanceAServiceOrConsumeItsOffer()
    {
        var session = Session(); Choose(session, ChurchAction.Raise); var before = session.Current;
        foreach (var command in new SessionCommand[] { new Move(ExplorationDirection.North), new AdvanceSimulation(),
            new Confirm(), new Acknowledge(Wait(session).Token), new Search() })
        { Assert.NotNull(Send(session, command).Failure); Assert.Same(before, session.Current); }
        Answer(session); Assert.Equal(80u, session.Current.Exploration!.Party.Gold);
    }

    [Fact]
    public void ARealCallerFailureRetainsItsCommittedProgressAndAttributedError()
    {
        var session = Session(failReturn: true); var before = session.Current;
        var result = Send(session, new LeaveChurch(Wait(session).Token));
        Assert.NotNull(result.Failure); Assert.Equal("program-opcode", result.Failure!.Code);
        Assert.NotSame(before, session.Current); Assert.Contains(999, session.Current.Story.Flags);
        Assert.Null(session.Current.Story.Wait); Assert.Equal(SessionStopReason.Unsupported, session.Current.StopReason);
    }
}
