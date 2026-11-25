using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed partial class FieldServiceTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void MasterMonkCompactsTheActualHolderThenUnwearsOnlyTheWeapon(bool selfHolder, bool worn)
    {
        var session = Session(hp: 27, currentLevel: 28, promotion: true, armed: worn, power: true,
            sourceRule: BattleClassRule.UnpromotedPriest, bindSpecial: true, selfHolder: selfHolder);
        var before = session.Current.Exploration!.Party;
        var original = Member(session, First);
        Choose(session, ChurchAction.Promote);
        var offer = Wait(session).Candidates!.Single(c => c.Member == First);
        Assert.Equal((byte)20, offer.SpecialDestination);
        // Both copies remain until the final Yes; the reverse-slot search selects the marked copy.
        Assert.Equal(new HeldPromotionItem(selfHolder ? First : Second, 2, 378), offer.SpecialItem);
        Accept(session, new CancelPromotion(Wait(session).Token));
        Assert.Same(before, session.Current.Exploration.Party);
        Accept(session, new ContinueChurch(Wait(session).Token));
        Choose(session, ChurchAction.Promote);
        offer = Wait(session).Candidates!.Single(c => c.Member == First);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        var waiting = session.Current;
        var holder = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == offer.SpecialItem!.Holder);
        var changed = FieldInventory.WithLoadout(holder, BattleInventory.SetWord(FieldInventory.Loadout(holder), 2, 122));
        var stale = new SessionSnapshot(waiting.SessionId, waiting.Revision, waiting.ObservationSequence,
            new ActiveExploration(waiting.Exploration!.WithParty(FieldInventory.Replace(before, [changed]))),
            waiting.Story, waiting.StopReason);
        var failed = FieldService.Submit(session.Definition, stale, new AnswerPromotion(Wait(session).Token, offer, true), session.Rules);
        Assert.Equal("stale-promotion", failed.Failure!.Code);
        Assert.Same(stale, failed.Snapshot);
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        var member = Member(session, First);
        var details = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First);
        Assert.Equal((byte)20, member.Progress!.ClassId);
        Assert.Equal((byte)1, member.Progress.Level);
        Assert.Equal((byte)0, member.Exp);
        Assert.Equal(original.Hp, member.Hp);
        Assert.Equal(original.Mp, member.Mp);
        Assert.Equal(original.Progress!.SourceLoadout!.Spells, member.Progress.SourceLoadout!.Spells);
        Assert.Equal(selfHolder ? new ushort[] { 122, 147, 89, 127 } : [89, 147, 127, 127], member.Progress.SourceLoadout.Items);
        Assert.Equal((byte)25, details.Attack);
        Assert.Equal((BattleClassRule.MasterMonk, BattleMover.Regular, (byte)5),
            (details.Deployment!.Definition.ClassRule, details.Deployment.Definition.Mover, details.Deployment.Definition.Move));
        Assert.False(BattleEquipment.IsEligible(details.Deployment.Definition, 89));
        Assert.Equal(PhysicalCriticalRule.WolfBaron, details.Deployment.Definition.Physical!.Critical);
        Assert.Equal(new[] { 7, 7, 10, 25 }, Enum.GetValues<PhysicalPriorityTable>().Select(t => PhysicalTargetRules.ClassRank(20, t)));
        if (!selfHolder) Assert.Equal(new ushort[] { 122, 0, 189, 127 }, Member(session, Second).Progress!.SourceLoadout!.Items);
        Assert.Equal(before.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(before.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        Assert.Equal(before.Gold, session.Current.Exploration.Party.Gold);
        uint seed = before.MainSeed;
        var actor = new BattleActorState(details.Deployment, member.Hp, member.Mp, 99, null, 0, 0, progress: member.Progress);
        var grown = BattleGrowthRules.Award(actor, 1, ref seed, []);
        Assert.Equal((byte)20, grown.Progress!.ClassId);
        Assert.Equal(2, grown.Level);
        Assert.Equal(member.Progress.SourceLoadout.Spells, grown.SourceLoadout!.Spells);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DecliningTheBallRequiresSeparateVicarConfirmation(bool regularYes)
    {
        var session = Session(hp: 27, currentLevel: 28, promotion: true, armed: true, power: true,
            sourceRule: BattleClassRule.UnpromotedPriest, bindSpecial: true);
        var before = session.Current.Exploration!.Party;
        Choose(session, ChurchAction.Promote);
        var offer = Wait(session).Candidates!.Single(c => c.Member == First);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, false));
        Assert.Equal(ChurchPhase.Regular, Wait(session).Phase);
        Assert.Same(before, session.Current.Exploration.Party);
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, regularYes));
        Assert.Equal(regularYes ? (byte)19 : (byte)4, Member(session, First).Progress!.ClassId);
        Assert.Same(before.Actors.Single(m => m.Actor == Second), Member(session, Second));
        Assert.Equal(new ushort[] { 217, 147, 127, 127 }, Member(session, First).Progress!.SourceLoadout!.Items);
    }
}
