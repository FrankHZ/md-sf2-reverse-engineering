using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class CaravanDepotServiceTests
{
    [Fact]
    public void SourceTermsDriveCurseGuardPriceAndUnknownAppraisalWithoutInventingRingEligibility()
    {
        var s = CaravanServiceTests.Session(pack: _ => new([178, 127, 127, 127], []));
        var source = new PrivateBattleDefinitions(null!, [], [],
            [new(50, "TEST_CURSE", "Cursed ring", "", "NONE", [], 0, 0, 101,
                "CURSED|RARE", "NOTHING", "NOTHING", 0, [new("NONE", 0)])], [], [], []);
        var definition = new ScenarioDefinition(s.Definition.Package, s.Definition.Encounters.Values,
            source, s.Definition.Exploration);
        Accept(s, new ChooseCaravan(Wait(s).Token, CaravanAction.Depot));
        Action(s, DepotAction.Deposit);
        Accept(s, new SelectCaravanMember(Wait(s).Token, 13));
        var before = s.Current;
        var result = CaravanService.Submit(definition, before, new SelectDepotSlot(Wait(s).Token, 0, 178), s.Rules);
        Assert.Equal("depot-equipped-curse", result.Failure!.Code);
        Assert.Same(before, result.Snapshot);
        var appraisal = CaravanDepotQuery.Look(definition, before, 50);
        Assert.Equal(75u, appraisal.SaleValue);
        Assert.Null(appraisal.Eligible);
        Assert.True(appraisal.WiderAppraisalUnavailable);
    }

    [Fact]
    public void UnknownCustodyRefusesOperationsButAllowsLeavingDepot()
    {
        var s = Start();
        var field = s.Current.Exploration!;
        var p = field.Party;
        var unknown = new BattleStartInput(p.Encounter, p.Actors, p.MainSeed, p.ThinkingSeed,
            p.Gold, p.NewBattle, p.ActiveAllies, null, p.StoryFlags, p.Visuals, p.RetainedSourceSlots, p.DeathCredit);
        var current = new SessionSnapshot(s.Current.SessionId, s.Current.Revision, s.Current.ObservationSequence,
            new ActiveExploration(field.WithParty(unknown)), s.Current.Story, s.Current.StopReason);
        var rejected = CaravanService.Submit(s.Definition, current,
            new ChooseDepot(Wait(s).Token, DepotAction.Deposit), s.Rules);
        Assert.Equal("depot-unavailable", rejected.Failure!.Code);
        Assert.Same(current, rejected.Snapshot);
        var returned = CaravanService.Submit(s.Definition, current, new BackCaravan(Wait(s).Token), s.Rules);
        Assert.Null(returned.Failure);
        Assert.Equal(CaravanPhase.Menu, Assert.IsType<CaravanWait>(returned.Snapshot.Story.Wait).Phase);
        Assert.Null(returned.Snapshot.Exploration!.Party.ItemCustody);
    }

    private static CaravanWait Wait(GameSession s) => Assert.IsType<CaravanWait>(s.Current.Story.Wait);
    private static SessionResult Send(GameSession s, SessionCommand c) => s.Submit(new(s.Current.SessionId, s.Current.Revision, null, c));
    private static void Accept(GameSession s, SessionCommand c) => Assert.Null(Send(s, c).Failure);
    private static void Action(GameSession s, DepotAction a) => Accept(s, new ChooseDepot(Wait(s).Token, a));
    private static void Back(GameSession s) => Accept(s, new BackCaravan(Wait(s).Token));
    private static void Continue(GameSession s) => Accept(s, new ContinueCaravan(Wait(s).Token));
    private static GameSession Start(bool fail = false)
    {
        var s = CaravanServiceTests.Session(failReturn: fail, pack: i => i == 13 ? new([0, 0, 61, 127], []) : new([50, 0, 61, 1], []));
        Accept(s, new ChooseCaravan(Wait(s).Token, CaravanAction.Depot));
        return s;
    }

    [Fact]
    public void ReserveDepositDuplicateRetrieveExchangeAndDropPreserveUnrelatedState()
    {
        var s = Start();
        var before = s.Current;
        Action(s, DepotAction.Deposit);
        Accept(s, new SelectCaravanMember(Wait(s).Token, 13));
        Accept(s, new SelectDepotSlot(Wait(s).Token, 1, 0));
        Assert.Equal(new byte[] { 0 }, s.Current.Exploration!.Party.ItemCustody!.Depot);
        Assert.Equal(new ushort[] { 0, 61, 127, 127 }, s.Current.Exploration.Party.Actors[13].SourceLoadout!.Items);
        Continue(s);
        Back(s);
        Action(s, DepotAction.Derive);
        Accept(s, new SelectDepotItem(Wait(s).Token, 0, 0));
        Accept(s, new SelectCaravanMember(Wait(s).Token, 2));
        Assert.Equal(CaravanPhase.DepotExchange, Wait(s).Phase);
        var unchanged = s.Current.Exploration.Party;
        Back(s);
        Assert.Same(unchanged, s.Current.Exploration.Party);
        Accept(s, new SelectCaravanMember(Wait(s).Token, 2));
        Accept(s, new SelectDepotSlot(Wait(s).Token, 2, 61));
        Assert.Equal(new byte[] { 61 }, s.Current.Exploration.Party.ItemCustody!.Depot);
        Assert.Equal(new ushort[] { 50, 0, 1, 0 }, s.Current.Exploration.Party.Actors[2].SourceLoadout!.Items);
        Continue(s);
        Back(s);
        Action(s, DepotAction.Drop);
        Accept(s, new SelectDepotItem(Wait(s).Token, 0, 61));
        var unconfirmed = s.Current.Exploration.Party;
        Accept(s, new AnswerCaravan(Wait(s).Token, false));
        Assert.Same(unconfirmed, s.Current.Exploration.Party);
        Accept(s, new SelectDepotItem(Wait(s).Token, 0, 61));
        Accept(s, new AnswerCaravan(Wait(s).Token, true));
        Assert.Empty(s.Current.Exploration.Party.ItemCustody!.Depot);
        Assert.Equal(1, s.Current.Exploration.Party.ItemCustody.Deals[61]);
        Assert.Equal(before.Story.Flags, s.Current.Story.Flags);
        Assert.Equal(before.Exploration!.Party.MainSeed, s.Current.Exploration.Party.MainSeed);
        Assert.Equal(before.Exploration.Party.ThinkingSeed, s.Current.Exploration.Party.ThinkingSeed);
        Assert.Equal(before.Exploration.Party.Gold, s.Current.Exploration.Party.Gold);
        Assert.Equal(before.Exploration.Party.ActiveAllies, s.Current.Exploration.Party.ActiveAllies);
        Assert.Equal(before.Story.Raft, s.Current.Story.Raft);
    }

    [Fact]
    public void StaleCommandsFailAndLaterCallbackFailureDoesNotRollbackDeposit()
    {
        var s = Start(true);
        Action(s, DepotAction.Deposit);
        Accept(s, new SelectCaravanMember(Wait(s).Token, 13));
        var stale = new SelectDepotSlot(Wait(s).Token, 0, 0);
        Accept(s, stale);
        var accepted = s.Current;
        Assert.NotNull(Send(s, stale).Failure);
        Assert.Same(accepted, s.Current);
        Continue(s);
        Back(s);
        Back(s);
        Assert.NotNull(Send(s, new BackCaravan(Wait(s).Token)).Failure);
        Assert.Equal(new byte[] { 0 }, s.Current.Exploration!.Party.ItemCustody!.Depot);
    }

    [Fact]
    public void EmptyFullUnsellableAndLookReturnWithoutInventoryMutation()
    {
        var s = Start();
        var before = s.Current.Exploration!.Party;
        Action(s, DepotAction.Look);
        Assert.Equal("empty", Wait(s).DepotResult);
        Continue(s);
        Assert.Same(before, s.Current.Exploration.Party);
        Action(s, DepotAction.Deposit);
        Accept(s, new SelectCaravanMember(Wait(s).Token, 0));
        Accept(s, new SelectDepotSlot(Wait(s).Token, 0, 50));
        Continue(s);
        Back(s);
        var stored = s.Current.Exploration.Party;
        Action(s, DepotAction.Drop);
        Accept(s, new SelectDepotItem(Wait(s).Token, 0, 50));
        Assert.Equal("unsellable", Wait(s).DepotResult);
        Assert.Same(stored, s.Current.Exploration.Party);
        Continue(s);
        Back(s);
        Action(s, DepotAction.Look);
        Accept(s, new SelectDepotItem(Wait(s).Token, 0, 50));
        var appraisal = CaravanDepotQuery.Look(s.Definition, s.Current, 50);
        Assert.Null(appraisal.SaleValue);
        Assert.True(appraisal.WiderAppraisalUnavailable);
        Assert.Same(stored, s.Current.Exploration.Party);
        var full = CaravanServiceTests.Session(custody: new([], new Dictionary<byte, byte>(), Enumerable.Repeat((byte)0, 64)));
        Accept(full, new ChooseCaravan(Wait(full).Token, CaravanAction.Depot));
        var fullParty = full.Current.Exploration!.Party;
        Action(full, DepotAction.Deposit);
        Assert.Equal("full", Wait(full).DepotResult);
        Assert.Same(fullParty, full.Current.Exploration.Party);
    }
}
