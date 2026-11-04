using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PoisonCureTests
{
    private static readonly ActorRef Medic = new("medic-a"), Friend = new("guard-a");
    private static readonly SpellRef Detox = new("detox", 1);

    [Fact]
    public void PurchasedSuppliesAndDeathCreditSurviveFieldTicksTextAndConnectedBattleEntry()
    {
        var session=Assert.IsType<SessionStarted>(GameSession.Start(Reader(Document("shop-supplies")))).Session;
        Accept(session,new Interact(new("merchant")));
        ShopServiceTests.Buy(session,1,Medic);ShopServiceTests.Pay(session);ShopServiceTests.Continue(session);
        ShopServiceTests.Buy(session,3,Friend);ShopServiceTests.Pay(session);ShopServiceTests.Continue(session);
        Accept(session,new BackShop(ShopServiceTests.Wait(session).Token));
        var context=session.Current.Exploration!.Party;
        foreach(var direction in new[]{ExplorationDirection.South,ExplorationDirection.East,ExplorationDirection.East,ExplorationDirection.East,ExplorationDirection.North})
        {
            Accept(session,new Move(direction));
            for(int tick=0;tick<400 && session.Current.Mode==SessionMode.Exploration && !session.Current.CanWaitAtInput;tick++)
                Accept(session,session.Current.Story.Wait is DialogueWait dialogue ? new Acknowledge(dialogue.Token) :
                    new AdvanceSimulation(session.Current.Story.Wait?.Token));
        }
        Assert.Equal(SessionMode.Battle,session.Current.Mode);
        Assert.Equal(context.DeathCredit,session.Current.Battle.DeathCredit);
        Assert.Equal(context.Gold,session.Current.Battle.Gold);
        Assert.Equal((ushort)1,session.Current.Battle.GetActor(Medic).SourceLoadout!.Items[0]);
        Assert.Equal((ushort)3,session.Current.Battle.GetActor(Friend).SourceLoadout!.Items[0]);
        var finished=FinishBattleScenes(session,Accept(session,new Confirm()));
        Accept(session,new SelectItem(0));Accept(session,new SelectTarget(Medic));
        finished=FinishBattleScenes(session,Accept(session,new Confirm()));
        Assert.Equal(35,session.Current.Battle.GetActor(Medic).Hp);
        Assert.Equal(Friend,session.Current.Selection!.Actor);
        Accept(session,new Confirm());Accept(session,new SelectItem(0));Accept(session,new SelectTarget(Friend));
        FinishBattleScenes(session,Accept(session,new Confirm()));
        Assert.Equal(0,session.Current.Battle.GetActor(Friend).Status);
        Assert.Equal((ushort)127,session.Current.Battle.GetActor(Friend).SourceLoadout!.Items[0]);
    }
    private static GameSession Battle(bool cureItem = false, bool clean = false)
    {
        var document = Document("shop-supplies")["battle"]!.DeepClone();
        document["actors"]![0]!["items"] = new JsonArray(3, 1, 0, 127);
        document["actors"]![1]!["items"] = new JsonArray(3, 127, 127, 127);
        if (clean) document["start"]!["actors"]![1]!["status"] = "none";
        var read = Reader(document).Read();
        Assert.False(read is ScenarioReadRejected, read.ToString());
        var started = GameSession.Start(Reader(document));
        Assert.False(started is SessionStartFailed, started.ToString());
        return Assert.IsType<SessionStarted>(started).Session;
    }

    [Theory]
    [InlineData(false, false)] [InlineData(true, false)] [InlineData(false, true)] [InlineData(true, true)]
    public void SpellAndItemUseTheSameFiniteCureAndOnlyPublishStatusAtReaction(bool item, bool clean)
    {
        var session = Battle(clean: clean); var before = session.Current.Battle;
        var action = item ? new BattleActionRef(BattleActionKind.Item, ItemSlot: 0) : new BattleActionRef(BattleActionKind.Healing, Detox);
        var rule = item ? (IBattleActionRule)new Sf2ItemAction() : session.Rules.Action(action);
        var resolution = BattleActionRules.Prepare(rule, before, Medic, before.GetActor(Medic).Position!, action, Friend, session.Rules.Progression);
        Assert.Equal(before.GetActor(Friend).Status, resolution.Prepared.GetActor(Friend).Status);
        Assert.Equal(before.GetActor(Medic).Mp, resolution.Prepared.GetActor(Medic).Mp);
        Assert.Equal(item ? (ushort)1 : (ushort)3, resolution.Prepared.GetActor(Medic).SourceLoadout!.Items[0]);
        var paid = item ? resolution.Prepared : resolution.ApplySpellCost(resolution.Prepared);
        Assert.Equal(item ? 20 : 17, paid.GetActor(Medic).Mp);
        var reaction = Assert.Single(resolution.Reactions);
        Assert.Equal(clean ? BattleReactionKind.Ineffective : BattleReactionKind.Status, reaction.Kind);
        Assert.Equal(0, reaction.Amount); Assert.Equal(before.GetActor(Friend).Hp, reaction.HpAfter);
        Assert.Equal(0, resolution.ApplyReaction(paid, reaction).GetActor(Friend).Status);
        Assert.Equal(clean ? 1 : 0, resolution.ConstructionEffects.Count(row => row.Kind == "rng-cure-ineffective"));
        Assert.InRange(resolution.Reward!.Amount, 1, clean ? 2 : 6);
        Assert.Equal(before.ThinkingSeed, resolution.Prepared.ThinkingSeed);
    }

    [Theory]
    [InlineData(2)] [InlineData(3)] [InlineData(0x302)] [InlineData(0x3c2)] [InlineData(0)] [InlineData(1)] [InlineData(0x300)]
    public void CureClearsOnlyPoisonAndPreservesOtherPackedFields(ushort status)
    {
        var target = Battle().Current.Battle.GetActor(Friend).With(status: status);
        var reaction = PoisonCureRules.Reaction(Medic, target, "heal");
        Assert.Equal((status & 2) != 0 ? BattleReactionKind.Status : BattleReactionKind.Ineffective, reaction.Kind);
        Assert.Equal((status & 2) != 0 ? (ushort?)(status & ~2) : null, reaction.StatusWrite);
        Assert.Equal(target.Hp, reaction.HpAfter); Assert.Equal(target.Mp, reaction.MpAfter);
    }

    [Fact]
    public void SeedUsesTwentyHpAndConsumesSelectedSlotWithoutMpCost()
    {
        var session = Battle(); var battle = session.Current.Battle; var actor = battle.GetActor(Medic);
        var action = new BattleActionRef(BattleActionKind.Item, ItemSlot: 1);
        var resolution = BattleActionRules.Prepare(new Sf2ItemAction(), battle, Medic, actor.Position!, action, Medic, session.Rules.Progression);
        var reaction = Assert.Single(resolution.Reactions); Assert.Equal(20, reaction.Amount);
        Assert.Equal(actor.Hp, resolution.Prepared.GetActor(Medic).Hp);
        var applied = resolution.ApplyReaction(resolution.Prepared, reaction).GetActor(Medic);
        Assert.Equal(actor.Hp + 20, applied.Hp); Assert.Equal(actor.Mp, applied.Mp);
        Assert.Equal(new ushort[] {3,0,127,127}, applied.SourceLoadout!.Items);
    }

    [Fact]
    public void CancellationInvalidTargetAndInsufficientMpCommitNoMovementPaymentOrRng()
    {
        var session = Battle(); var before = session.Current.Battle;
        Accept(session, new Confirm());
        Accept(session, new SelectSpell(Detox)); Accept(session, new Cancel()); Accept(session, new Cancel());
        Assert.Same(before, session.Current.Battle);
        var action = new BattleActionRef(BattleActionKind.Healing, Detox);
        Assert.Throws<BattleRuleException>(() => BattleActionRules.Prepare(session.Rules.Action(action), before, Medic,
            before.GetActor(Medic).Position!, action, new("dummy-a"), session.Rules.Progression));
        var poor = before.With(actors: before.Actors.Select(a => a.Actor == Medic ? a.With(mp: 2) : a));
        Assert.Throws<BattleRuleException>(() => BattleActionRules.Prepare(session.Rules.Action(action), poor, Medic,
            poor.GetActor(Medic).Position!, action, Friend, session.Rules.Progression));
        Assert.Same(before, session.Current.Battle);
    }

    [Fact]
    public void SceneCureHasNoHealingFairyAndPaysBeforeTheResultThenReturnsControl()
    {
        var session = Battle(); var mp = session.Current.Battle.GetActor(Medic).Mp;
        Accept(session,new Confirm()); Accept(session,new SelectSpell(Detox));
        Accept(session,new SelectTarget(Friend)); var result = Accept(session,new Confirm());
        Assert.NotNull(session.Current.BattleScene); Assert.Null(session.Current.BattleScene!.Healing);
        var finished=FinishBattleScenes(session,result);
        Assert.Equal(mp-3,session.Current.Battle.GetActor(Medic).Mp); Assert.Equal(0,session.Current.Battle.GetActor(Friend).Status);
        Assert.Single(finished.Observations,row=>row.Kind=="status" && row.Actor==Friend);
        Assert.True(session.Current.HasBattleControl);
    }

    [Fact]
    public void ImmutableBattleCopiesKeepBothSupplyEffects()
    {
        var before=Battle().Current.Battle.Definition;
        var copy=before.WithDeployments(before.Deployments);
        Assert.True(copy.Spells[Detox].CuresPoison); Assert.True(copy.HealingItems[3].CuresPoison);
        Assert.Equal(20,copy.HealingItems[1].Power); Assert.False(copy.HealingItems[0].CuresPoison);
    }

    [Fact]
    public void AuthoredConnectedShopPackageAdmitsItsActualStockAndBattleConsumers()
    {
        var read=Reader(Document("shop-supplies")).Read(); Assert.False(read is ScenarioReadRejected,read.ToString());
        var admitted=Assert.IsType<ExplorationReadAccepted>(read);
        Assert.True(admitted.Definition.Encounters.Values.Single().Spells[Detox].CuresPoison);
        Assert.True(admitted.Definition.Encounters.Values.Single().Poison);
        Assert.Contains(admitted.Definition.Exploration!.Programs["shop-caller"].Instructions,
            op=>op is OpenShop {Definition.Stock.Count:3});
    }
}
