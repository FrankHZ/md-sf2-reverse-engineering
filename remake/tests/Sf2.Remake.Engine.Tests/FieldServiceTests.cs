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
        ChurchDefinition? terms = null, byte? currentLevel = null, bool failReturn = false, bool promotion = false, bool armed = false, bool power = false,
        bool knight = false)
    {
        var original = FieldInventoryTests.Session().Current.Exploration!.Party;
        var source = FieldInventoryTests.Session().Definition.Encounters.Values.Single();
        var deployments = source.Deployments.Select(row => row.Actor != First ? row : row with
        {
            Definition = new(row.Actor, knight ? BattleClassRule.UnpromotedKnight : row.Definition.ClassRule, level, maxHp, row.Definition.MaxMp,
                row.Definition.Attack, row.Definition.Defense, row.Definition.Agility, false, row.Definition.Move,
                row.Definition.Spells, row.Definition.Physical! with { Promoted = promoted }, sourceLoadout: row.Definition.SourceLoadout),
        });
        byte[] book = knight ? [63, 63, 63, 63] : currentLevel >= 22 ? [10, 13, 63, 63] : [10, 63, 63, 63];
        byte baseClass = knight ? (byte)1 : (byte)0;
        byte destination = knight ? (byte)13 : (byte)12;
        var spells = book.Where(spell => spell != 63).Select(spell => new SpellRef(spell == 10 ? "egress" : "bolt", 1)).ToArray();
        var loadout = new BattleSourceLoadout([armed ? (ushort)178 : (ushort)50, power ? (ushort)147 : (ushort)127, 127, 127], book);
        if (promotion)
        {
            var growth = new BattleGrowthDefinition(baseClass, 0, Enumerable.Repeat(new StatGrowth(20, 20, []), 5).ToArray(),
                knight ? [] : [new(1, 10), new(22, 13)], new Dictionary<byte, SpellRef> { [10] = new("egress", 1), [13] = new("bolt", 1) });
            var equipment = new BattleEquipmentDefinition(20, [new(50, "Weapon", (1u << baseClass) | (1u << destination), 5, 1, 1)],
                rings: [new(19, "Power Ring", uint.MaxValue, RingStat.Attack, 5)]);
            deployments = deployments.Select(d => d.Actor != First ? d : d with {
                Definition = d.Definition.WithGrowth(growth).WithEquipment(equipment),
                RegularPromotion = new(d.Actor, knight ? BattleClassRule.Paladin : BattleClassRule.Hero,
                    1, 42, 14, 28, 25, 24, false, knight ? (byte)7 : (byte)6, [],
                    d.Definition.Physical! with { Promoted = true, Critical = PhysicalCriticalRule.Hero },
                    sourceLoadout: loadout, growth: growth with { ClassId = destination, RetainedSpells = growth.Spells }, equipment: equipment) });
        }
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
        if (promotion)
            party = new(party.Encounter, party.Actors.Select(a => a.Actor == First ? a with { SourceLoadout = loadout,
                Progress = a.Progress! with { ClassId = baseClass, SourceLoadout = loadout, Spells = spells, BaseMove = 8 } }
                : knight && a.Actor == Second ? a with { SourceLoadout = new([118, 127, 127, 127], [63, 63, 63, 63]) } : a),
                party.MainSeed, party.ThinkingSeed, party.Gold, party.NewBattle, [Second], party.ItemCustody);
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
    [InlineData(19, false, 5, false)]
    [InlineData(20, true, 0, false)]
    [InlineData(31, false, 3, false)]
    [InlineData(20, true, 7, true)]
    public void OrdinaryPromotionCarriesEarnedStateThroughConsentAndTheNextLevel(byte level, bool armed, ushort hp, bool power)
    {
        var session = Session(hp: hp, currentLevel: level, promotion: true, armed: armed, power: power);
        var before = session.Current.Exploration!.Party;
        Choose(session, ChurchAction.Promote);
        if (level < 20)
        {
            Assert.Equal(ChurchResultKind.NoApplicableMember, Wait(session).Result!.Kind);
            Assert.Same(before, session.Current.Exploration.Party);
            return;
        }
        var row = Wait(session).Candidates!.Single(c => c.Member == First);
        Accept(session, new SelectPromotion(Wait(session).Token, row));
        Accept(session, new AnswerPromotion(Wait(session).Token, row, false));
        Assert.Same(before, session.Current.Exploration.Party);
        Accept(session, new SelectPromotion(Wait(session).Token, row));
        Accept(session, new AnswerPromotion(Wait(session).Token, row, true));
        var yes = new AnswerPromotion(Wait(session).Token, row, true);
        Accept(session, yes);
        var after = session.Current.Exploration.Party;
        var member = Member(session, First);
        var prior = before.Actors.Single(a => a.Actor == First);
        Assert.Equal(prior with { Exp = 0, Progress = prior.Progress! with { ClassId = 12, Level = 1, BaseMove = null } }, member);
        Assert.Equal(before.MainSeed, after.MainSeed); Assert.Equal(before.ThinkingSeed, after.ThinkingSeed);
        Assert.Equal(before.Gold, after.Gold); Assert.Same(before.ItemCustody, after.ItemCustody);
        Assert.DoesNotContain(First, after.ActiveAllies!);
        Assert.NotNull(Send(session, yes).Failure); Assert.Same(after, session.Current.Exploration.Party);
        var projected = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First);
        Assert.Equal((armed ? 25 : 20) + (power ? 5 : 0), projected.Attack!.Value);
        Assert.Equal(BattleClassRule.Hero, projected.Deployment!.Definition.ClassRule);
        Assert.Equal(6, projected.Move!.Value);
        var actor = new BattleActorState(projected.Deployment, hp, member.Mp, 99, null, 0, 0, progress: member.Progress);
        uint seed = before.MainSeed;
        var grown = Sf2.Remake.Domain.Gameplay.Sf2.BattleGrowthRules.Award(actor, 1, ref seed, []);
        Assert.Equal((byte)12, grown.Progress!.ClassId); Assert.Equal(2, grown.Level);
        Assert.Equal(hp, grown.Hp); Assert.Equal(member.Mp, grown.Mp);
        if (power)
        {
            Assert.Equal((ushort)147, grown.SourceLoadout!.Items[1]);
            Assert.Equal(grown.BaseAttack + 10, grown.Attack);
        }
        Assert.Single(grown.Spells, spell => spell.Value == "bolt");
        Assert.Equal(new byte[] { 10, 13, 63, 63 }, grown.SourceLoadout!.Spells);
        Continue(session);
        Accept(session, new SelectPromotion(Wait(session).Token, Wait(session).Candidates!.Single(c => c.Member == First)));
        Assert.Equal(ChurchResultKind.NoPromotion, Wait(session).Result!.Kind);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public void HeldSpecialItemRequiresItsOwnAnswerBeforeOrdinaryPromotion(bool acceptSpecial, bool acceptRegular)
    {
        var session = Session(hp: 0, currentLevel: 20, promotion: true, armed: true, power: true, knight: true);
        var before = session.Current.Exploration!.Party;
        var beforeFlags = session.Current.Story.Flags.ToArray();

        Choose(session, ChurchAction.Promote);
        var candidate = Wait(session).Candidates!.Single(offer => offer.Member == First);
        Assert.Equal(new HeldPromotionItem(Second, 0, 118), candidate.SpecialItem);
        Accept(session, new SelectPromotion(Wait(session).Token, candidate));
        Accept(session, new AnswerPromotion(Wait(session).Token, candidate, true));
        Assert.Equal(ChurchPhase.Special, Wait(session).Phase);

        var special = new AnswerPromotion(Wait(session).Token, candidate, acceptSpecial);
        Accept(session, special);
        Assert.Same(before, session.Current.Exploration.Party);
        Assert.NotNull(Send(session, special).Failure);
        Assert.Same(before, session.Current.Exploration.Party);

        if (acceptSpecial)
        {
            Assert.Equal(ChurchResultKind.SpecialUnavailable, Wait(session).Result!.Kind);
            return;
        }

        Assert.Equal(ChurchPhase.Regular, Wait(session).Phase);
        Accept(session, new AnswerPromotion(Wait(session).Token, candidate, acceptRegular));
        if (!acceptRegular)
        {
            Assert.Equal(ChurchPhase.Members, Wait(session).Phase);
            Assert.Same(before, session.Current.Exploration.Party);
            return;
        }

        Assert.Equal(ChurchResultKind.Promoted, Wait(session).Result!.Kind);
        var after = session.Current.Exploration.Party;
        var prior = before.Actors.Single(actor => actor.Actor == First);
        Assert.Equal(prior with { Exp = 0, Progress = prior.Progress! with { ClassId = 13, Level = 1, BaseMove = null } },
            Member(session, First));
        Assert.Same(before.Actors.Single(actor => actor.Actor == Second), Member(session, Second));
        Assert.Equal(beforeFlags, session.Current.Story.Flags.ToArray());
        Assert.Equal(before.Gold, after.Gold);
        Assert.Equal(before.MainSeed, after.MainSeed);
        Assert.Equal(before.ThinkingSeed, after.ThinkingSeed);
        Assert.DoesNotContain(First, after.ActiveAllies!);
        var projected = session.QueryPartyDetails()!.Members.Single(member => member.Carried.Actor == First);
        Assert.Equal(BattleClassRule.Paladin, projected.Deployment!.Definition.ClassRule);
        Assert.Equal(7, projected.Move!.Value);
        Assert.Equal(30, projected.Attack!.Value);
    }

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
        Choose(session, action); Assert.Null(Wait(session).Offer); Assert.Equal(ChurchResultKind.NoApplicableMember, Wait(session).Result!.Kind);
        Assert.Equal(ChurchPhase.Result, Wait(session).Phase);
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
    public void ResultPhaseRejectsFreshTokenAnswersUntilContinueWithoutAnotherPayment()
    {
        var session = Session(status: 7); Choose(session, ChurchAction.Cure);
        var offered = Wait(session).Offer!;
        Assert.Equal(ChurchPhase.Offer, Wait(session).Phase);
        Assert.NotNull(Send(session, new ContinueChurch(Wait(session).Token)).Failure);
        Answer(session); var paid = session.Current; var result = Wait(session);
        Assert.Equal(ChurchPhase.Result, result.Phase);
        Assert.Equal(new ChurchResult(ChurchResultKind.Paid, offered), result.Result);
        Assert.False(result.CurseUnavailable); // Stun is still queued; curse notice is independent at completion.
        Assert.NotNull(Send(session, new AnswerChurch(result.Token, result.Offer!, true)).Failure);
        Assert.Same(paid, session.Current);
        Continue(session); Assert.Equal(ChurchPhase.Offer, Wait(session).Phase);
        Assert.Equal(ChurchEffect.Stun, Wait(session).Offer!.Effect);
        Answer(session, false);
        Assert.Equal(ChurchResultKind.Declined, Wait(session).Result!.Kind);
        Assert.True(Wait(session).CurseUnavailable);
        Continue(session); Assert.Equal(ChurchPhase.Menu, Wait(session).Phase);
        Assert.Null(Wait(session).Result); Assert.False(Wait(session).CurseUnavailable);
        Assert.Equal(90u, session.Current.Exploration!.Party.Gold);
        Assert.Equal(5, Member(session, First).Status);
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
        Assert.Equal(4, Member(session, First).Status); Assert.True(Wait(session).CurseUnavailable);
        Assert.Equal(new ChurchResult(ChurchResultKind.Paid, new(First, ChurchEffect.Poison, 7, 100, 6)), Wait(session).Result);
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
