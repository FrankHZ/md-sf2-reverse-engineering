using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PoisonSessionTests
{
    private static readonly ActorRef Hero = new("champion"), Friend = new("receiver"), Enemy = new("reserve");
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(200)]
    public void StayPublishesFirstThenTypedNoticeAppliesOnce(ushort hp)
    {
        var session = Start("poison-turn", d => d["start"]!["actors"]![0]!["hp"] = hp);
        var before = session.Current.Battle;
        var begun = Stay(session); Assert.Single(begun.Observations, e => e.Kind == "action-committed");
        Assert.Equal(hp, session.Current.Battle.GetActor(Hero).Hp);
        Assert.Equal(before.Cursor, session.Current.Battle.Cursor);
        var held = session.Current; var token = held.BattleScene!.Token;
        foreach (var bad in new SessionCommand[] { new Acknowledge(token), new Confirm(),
            new CompletePresentation(new(token.Value + 1), held.BattleScene.CompletionKind),
            new CompletePresentation(token, PresentationCueKind.FadeIn) })
        { Assert.NotNull(Send(session, bad).Failure); Assert.Same(held, session.Current); }
        var finished = FinishBattleScenes(session, Accept(session, PoisonTurnTests.Command(held)));
        Assert.Equal((ushort)Math.Max(0, hp - 2), session.Current.Battle.GetActor(Hero).Hp);
        Assert.Single(finished.Observations, e => e.Kind == "poison-hp");
        Assert.Single(finished.Observations, e => e.Kind == "after-turn");
        Assert.Equal(before.MainSeed, session.Current.Battle.MainSeed);
        var final = session.Current;
        Assert.NotNull(Send(session, new CompletePresentation(token, held.BattleScene.CompletionKind)).Failure);
        Assert.Same(final, session.Current);
        if (hp <= 2) { Assert.Equal(BattleOutcomeKind.Defeat, final.BattleResult); Assert.Equal(SessionStopReason.BattleEnded, final.StopReason); }
        else { Assert.Equal(Friend, final.Selection!.Actor); Assert.Equal((ushort)2, final.Battle.GetActor(Hero).Status); }
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void AutomaticQueueOnlyStandbyTicksAndCanEndBattle(ushort hp)
    {
        var session = Start("poison-turn", d =>
        {
            d["actors"]![2]!["agility"] = 100;
            d["start"]!["actors"]![2]!["hp"] = hp;
            d["start"]!["actors"]![2]!["status"] = "poison";
            d["start"]!["actors"]![0]!["status"] = "none";
            d["start"]!["actors"]![1]!["status"] = "none";
        });
        Assert.Equal(Enemy, session.Current.BattleScene!.Actor);
        Assert.True(session.Current.BattleScene.IsPoisonTurn);
        var finished = FinishBattleScenes(session, Accept(session, PoisonTurnTests.Command(session.Current)));
        Assert.Single(finished.Observations, e => e.Kind == "poison-hp" && e.Actor == Enemy);
        Assert.Equal((ushort)Math.Max(0, hp - 2), session.Current.Battle.GetActor(Enemy).Hp);
        if (hp <= 2)
        {
            Assert.Equal(BattleOutcomeKind.Victory, session.Current.BattleResult);
            Assert.Equal((ushort)1, session.Current.Battle.GetActor(Friend).Kills);
        }
        else Assert.Equal(Hero, session.Current.Selection!.Actor);
    }

    [Fact]
    public void MenuCancelEquipAndDropAreFreeUntilStay()
    {
        var session = Start("poison-turn"); var before = session.Current.Battle;
        Accept(session, new Confirm());
        Accept(session, new BeginEquipment()); Accept(session, new SelectEquipment(4)); Accept(session, new Confirm());
        Assert.Equal((ushort)3, session.Current.Battle.GetActor(Hero).Hp);
        Accept(session, new BeginDiscard()); Accept(session, new Cancel());
        Accept(session, new BeginDiscard()); Accept(session, new SelectDiscardItem(0));
        Accept(session, new Confirm()); var result = Accept(session, new Confirm());
        while (session.Current.BattleDiscard is { } pending)
            result = Accept(session, pending.Revealed ? new Acknowledge(pending.Token) : new CompleteTextReveal(pending.Token));
        Assert.Equal(before.Cursor, session.Current.Battle.Cursor);
        Assert.Equal((ushort)3, session.Current.Battle.GetActor(Hero).Hp);
        Assert.DoesNotContain(result.Observations, e => e.Kind is "after-turn" or "poison-hp");
        Accept(session, new Cancel());
        var finished = FinishBattleScenes(session, Stay(session));
        Assert.Single(finished.Observations, e => e.Kind == "poison-hp");
    }

    [Fact]
    public void PhysicalActionEndingOutcomeSkipsPoison()
    {
        var session = Start("poison-turn", d =>
        {
            d["start"]!["actors"]![2]!["hp"] = 1;
            d["encounters"]![0]!["placements"]![2]!["x"] = 1;
            d["encounters"]![0]!["placements"]![2]!["y"] = 3;
        });
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(Enemy));
        var result = FinishBattleScenes(session, Accept(session, new Confirm()));
        Assert.Equal(BattleOutcomeKind.Victory, session.Current.BattleResult);
        Assert.Equal((ushort)3, session.Current.Battle.GetActor(Hero).Hp);
        Assert.DoesNotContain(result.Observations, e => e.Kind is "poison-hp" or "poison-message");
        Assert.Single(result.Observations, e => e.Kind == "action-committed");
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ItemAndPhysicalGrowthFinishBeforePoisonWithoutGrowthHealing(bool physical)
    {
        var session = Start("poison-turn", d =>
        {
            d["growth"] = Document("battle-equipment")["growth"]!.DeepClone();
            d["start"]!["actors"]![0]!["exp"] = 99;
            d["start"]!["actors"]![0]!["hp"] = 100;
            d["actors"]![2]!["attack"] = 0;
            d["encounters"]![0]!["placements"]![2]!["x"] = 1;
            d["encounters"]![0]!["placements"]![2]!["y"] = 3;
        });
        Accept(session, new Confirm());
        if (physical) Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        else Accept(session, new SelectItem(0));
        Accept(session, new SelectTarget(physical ? Enemy : Hero));
        var result = Accept(session, new Confirm()); var facts = result.Observations.ToList();
        ushort hpAtGrowth = 0;
        while (session.Current.BattleScene is { IsPoisonTurn: false } scene)
        {
            ushort before = session.Current.Battle.GetActor(Hero).Hp;
            result = Accept(session, scene.Healing is { LogicalComplete: false } healing && !healing.AtTimedInput
                ? new AdvanceSimulation(scene.Token) : scene.RequiresAcknowledgement ? new Acknowledge(scene.Token)
                : new CompletePresentation(scene.Token, scene.CompletionKind));
            facts.AddRange(result.Observations);
            if (result.Observations.Any(e => e.Kind == "level-max-hp"))
            { hpAtGrowth = before; Assert.Equal(before, session.Current.Battle.GetActor(Hero).Hp); }
        }
        Assert.True(hpAtGrowth > 0); Assert.Equal(2, session.Current.Battle.GetActor(Hero).Level);
        ushort hpBeforePoison = session.Current.Battle.GetActor(Hero).Hp;
        Assert.Equal((ushort)2, session.Current.Battle.GetActor(Hero).Status);
        result = Accept(session, PoisonTurnTests.Command(session.Current)); facts.AddRange(result.Observations);
        Assert.Equal(hpBeforePoison - 2, session.Current.Battle.GetActor(Hero).Hp);
        Assert.Single(facts, e => e.Kind == "action-committed"); Assert.Single(facts, e => e.Kind == "poison-hp");
        Assert.True(facts.FindIndex(e => e.Kind == "exp-threshold") < facts.FindIndex(e => e.Kind == "poison-hp"));
    }

    [Fact]
    public void HealingCostRecoveryAndRewardPrecedeExactlyOnePoisonTick()
    {
        var session = Start("practice-yard", d =>
        {
            d["battlePoison"] = "fixed-two-after-turn"; d["start"]!["firstAlly"] = "guard-a";
            d["start"]!["actors"]![0]!["status"] = "poison";
            d["encounters"]![0]!["outcome"] = JsonNode.Parse("""{"leader":"medic-a","defeatedGateEnemy":"dummy-a"}""");
        });
        var actor = session.Current.Selection!.Actor; var before = session.Current.Battle.GetActor(actor);
        Accept(session, new Confirm()); Accept(session, new SelectSpell(before.Spells[0]));
        Accept(session, new SelectTarget(actor));
        var result = FinishBattleScenes(session, Accept(session, new Confirm()));
        Assert.Equal(before.MaxHp - 2, session.Current.Battle.GetActor(actor).Hp);
        Assert.True(session.Current.Battle.GetActor(actor).Mp < before.Mp);
        var facts = result.Observations.ToList();
        Assert.Single(facts, e => e.Kind == "poison-hp"); Assert.Single(facts, e => e.Kind == "action-committed");
        Assert.True(facts.FindIndex(e => e.Kind == "exp") < facts.FindIndex(e => e.Kind == "poison-hp"));
    }

    [Fact]
    public void AlreadyDeadLeaderAtControlledStartProducesTerminalDefeatWithoutPoison()
    {
        var session = Start("poison-turn", d => d["start"]!["actors"]![0]!["hp"] = 0);
        Assert.Equal(BattleOutcomeKind.Defeat, session.Current.BattleResult);
        Assert.False(session.Current.HasBattleControl);
        Assert.Null(session.Current.BattleScene);
        var terminal = session.Current;
        Assert.Equal("battle-ended", Send(session, new AdvanceSimulation()).Failure!.Code);
        Assert.Same(terminal, session.Current);
    }

    [Fact]
    public void UnpoisonedActorDoesNotTickAndMixedPhysicalStatusesRemainUnsupported()
    {
        var session = Start("poison-turn", d => d["start"]!["actors"]![0]!["status"] = "none");
        var result = Stay(session);
        Assert.Equal((ushort)3, session.Current.Battle.GetActor(Hero).Hp);
        Assert.DoesNotContain(result.Observations, e => e.Kind is "poison-message" or "poison-hp");
        Assert.False(Sf2.Remake.Domain.Gameplay.Sf2.PoisonTurnRules.SupportedPhysicalStatus(0xC2));
    }

}
