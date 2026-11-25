using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed partial class FieldServiceTests
{
    [Theory]
    [InlineData(false, 24)]
    [InlineData(false, 23)]
    [InlineData(true, 24)]
    [InlineData(true, 23)]
    public void SorcererRequiresFinalConsentAndReplacesOnlyTheBook(bool selfHolder, int firstLearn)
    {
        var session = Session(hp: 27, currentLevel: 29, promotion: true, armed: true, power: true,
            sourceRule: BattleClassRule.UnpromotedMage, bindSpecial: true, selfHolder: selfHolder, sorcererFirstLearn: firstLearn);
        var before = session.Current.Exploration!.Party;
        Choose(session, ChurchAction.Promote);
        var offer = Wait(session).Candidates!.Single(c => c.Member == First);
        Assert.Equal((byte)18, offer.SpecialDestination);
        void Select()
        {
            Accept(session, new SelectPromotion(Wait(session).Token, offer));
            Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
            Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
            Assert.Equal(ChurchPhase.ReplaceBook, Wait(session).Phase);
            Assert.Same(before, session.Current.Exploration!.Party);
        }
        Select();
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, false));
        Assert.Equal(ChurchPhase.Members, Wait(session).Phase);
        Assert.Same(before, session.Current.Exploration.Party);
        Select();
        var final = new AnswerPromotion(Wait(session).Token, offer, true);
        Accept(session, final);
        var after = session.Current;
        Assert.NotNull(Send(session, final).Failure);
        Assert.Same(after, session.Current);
        var member = Member(session, First);
        Assert.Equal((byte)18, member.Progress!.ClassId);
        Assert.Equal((byte)1, member.Progress.Level);
        Assert.Equal((byte)0, member.Exp);
        Assert.Equal(new byte[] { 29, 63, 63, 63 }, member.Progress.SourceLoadout!.Spells);
        Assert.Equal(new[] { new SpellRef("dao", 1) }, member.Progress.Spells);
        var original = before.Actors.Single(a => a.Actor == First);
        Assert.Equal((original.Hp, original.Mp, original.Status), (member.Hp, member.Mp, member.Status));
        Assert.Equal(selfHolder ? new ushort[] { 121, 147, 213, 127 } : [213, 147, 127, 127], member.Progress.SourceLoadout.Items);
        if (!selfHolder)
        {
            var holder = Member(session, Second);
            Assert.Equal(new ushort[] { 121, 0, 189, 127 }, holder.Progress!.SourceLoadout!.Items);
            Assert.Equal((byte)6, holder.Progress.BaseMove);
            Assert.NotEqual(before.Actors.Single(a => a.Actor == Second).Progress!.CurrentStats, holder.Progress.CurrentStats);
        }
        Assert.Equal(before.MainSeed, after.Exploration!.Party.MainSeed);
        Assert.Equal(before.ThinkingSeed, after.Exploration.Party.ThinkingSeed);
        Assert.Equal(before.Gold, after.Exploration.Party.Gold);
        var details = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First);
        var growth = details.Deployment!.Definition.Growth!;
        Assert.True(growth.CanKnow(29, 1));
        Assert.False(growth.CanKnow(11, 99));
        Assert.False(growth.CanKnow(30, (byte)(firstLearn - 21)));
        Assert.True(growth.CanKnow(30, (byte)(firstLearn - 20)));
        uint seed = before.MainSeed;
        var actor = new BattleActorState(details.Deployment, member.Hp, member.Mp, 99, null, 0, 0,
            progress: member.Progress with { Level = (byte)(firstLearn - 21) });
        var grown = BattleGrowthRules.Award(actor, 1, ref seed, []);
        Assert.Equal(new byte[] { 29, 30, 63, 63 }, grown.SourceLoadout!.Spells);
        Assert.Equal(new[] { new SpellRef("dao", 1), new SpellRef("apollo", 1) }, grown.Spells);
    }
}
