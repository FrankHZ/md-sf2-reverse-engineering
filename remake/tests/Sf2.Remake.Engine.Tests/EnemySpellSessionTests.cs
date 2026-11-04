using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.EnemySpellDecisionTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class EnemySpellSessionTests
{
    [Fact]
    public void OrdinaryAutomaticBlazeAndHealPayOnceAtSceneBoundariesThenReturnUsablePlayerControl()
    {
        var session = Start("enemy-spells"); var original = session.Current;
        Assert.Equal(Scout, session.Current.Selection!.Actor);
        var result = Stay(session); var events = result.Observations.ToList();
        Assert.Equal(5, session.Current.Battle.GetActor(Mage).Mp);
        Assert.Equal(60, session.Current.Battle.GetActor(Scout).Hp);
        HashSet<ActorRef> blazeMessages = [], healMessages = [], hits = [], heals = [];
        while (session.Current.BattleScene is not null || session.Current.BattleMovement is not null)
        {
            Assert.False(session.Current.HasBattleControl);
            if (session.Current.BattleMovement is { } movement)
                result = Accept(session, new CompletePresentation(movement.Token, movement.CompletionKind));
            else
            {
                var scene = session.Current.BattleScene!;
                if (scene.Phase == BattleScenePhase.ActionMessage)
                {
                    if (scene.AttackSpell is not null) { blazeMessages.Add(scene.Actor); Assert.Equal(5, session.Current.Battle.GetActor(Mage).Mp); }
                    if (scene.Spell is not null) { healMessages.Add(scene.Actor); Assert.Equal(7, session.Current.Battle.GetActor(Cleric).Mp); }
                }
                if (scene.Phase == BattleScenePhase.ResultMessage)
                {
                    if (scene.AttackSpell is not null) hits.Add(scene.Actor);
                    if (scene.Spell is not null)
                    { heals.Add(scene.Actor); Assert.Null(scene.DisplayedAlly); Assert.Equal(Wounded, scene.DisplayedEnemy); }
                }
                result = scene.Healing is { LogicalComplete: false } healing && !healing.AtTimedInput
                    ? Accept(session, new AdvanceSimulation(scene.Token))
                    : Accept(session, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                        new CompletePresentation(scene.Token, scene.CompletionKind));
            }
            events.AddRange(result.Observations);
        }
        Assert.Single(blazeMessages); Assert.Single(healMessages); Assert.Single(hits); Assert.Single(heals);
        Assert.Equal(3, session.Current.Battle.GetActor(Mage).Mp); Assert.Equal(4, session.Current.Battle.GetActor(Cleric).Mp);
        Assert.InRange(session.Current.Battle.GetActor(Scout).Hp, (ushort)53, (ushort)55);
        Assert.Equal(23, session.Current.Battle.GetActor(Wounded).Hp);
        Assert.Equal(new MapPosition(4, 2), session.Current.Battle.GetActor(Mage).Position);
        Assert.Equal(new MapPosition(5, 5), session.Current.Battle.GetActor(Cleric).Position);
        Assert.Equal(2, events.Count(e => e.Kind == "mp"));
        Assert.DoesNotContain(events, e => e.Kind is "exp" or "gold" or "level");
        Assert.Equal(original.SessionId, session.Current.SessionId); Assert.Equal(original.Battle.Gold, session.Current.Battle.Gold);
        Assert.Equal(Scout, session.Current.Selection!.Actor); Assert.True(session.Current.HasBattleControl);
        var input = Accept(session, new Move(ExplorationDirection.East));
        FinishMovement(session, input); Assert.Equal(new MapPosition(3, 2), session.Current.Selection!.Preview.Destination);
    }

    [Fact]
    public void EnemyBlazeNeedsNoEnemyExperienceKillsOrGoldAndAllyDeathUsesSharedDefeatCleanup()
    {
        var before = Battle(d =>
        {
            var reserve = d["encounters"]![0]!["placements"]![3]!;
            reserve["faction"] = "ally"; reserve["control"] = "player"; reserve["aiStrategy"] = null;
            reserve["processingOrder"] = 1; reserve["initialization"]!["baseAiWord"] = null;
            d["start"]!["actors"]![3]!["initialization"]!["activationWord"] = 0;
            d["encounters"]![0]!["initialization"]!["sourceCombatants"]![3]!["slot"] = 1;
        });
        before = before.With(actors: before.Actors.Select(a => a.Actor == Mage
            ? new BattleActorState(a.Deployment, a.Hp, a.Mp, null, new(4, 2), null, null, activationWord: a.ActivationWord)
            : a.Actor == Scout ? a.With(hp: 1) : a));
        var actor = before.GetActor(Mage);
        var prepared = BattleActionRules.Prepare(new Sf2AttackSpellAction(), before, Mage, actor.Position!,
            new(BattleActionKind.AttackSpell, new("blaze", 1)), Scout, new Sf2BattleProgressionRule());
        Assert.Equal(0, Assert.Single(prepared.Reactions).HpAfter); Assert.Null(prepared.Reward);
        Assert.Equal(before.Gold, prepared.Prepared.Gold);
        Assert.Equal(3, prepared.ConstructionEffects.Count(e => e.RandomRange is not null));
        Assert.DoesNotContain(prepared.ConstructionEffects, e => e.Kind.Contains("exp") || e.Kind == "gold");
        var affected = prepared.ApplyReaction(prepared.ApplySpellCost(prepared.Prepared), prepared.Reactions[0]);
        var cleaned = new BattleDeathBatch([Scout]).Clean(affected, null, new Sf2BattleProgressionRule());
        Assert.Equal((ushort?)1, cleaned.Battle.GetActor(Scout).Defeats); Assert.Null(cleaned.Battle.GetActor(Scout).Position);
        Assert.Null(cleaned.Battle.GetActor(Mage).Kills); Assert.Equal(3, cleaned.Battle.GetActor(Mage).Mp);
        Assert.Contains(cleaned.Effects, e => e.Kind == "defeats" && e.Actor == Scout);
    }

    [Fact]
    public void EnemyCastFailureRetainsAllOriginalResourcesAndRejectsUnsupportedSpellFamilies()
    {
        var before = Battle(); var actor = before.GetActor(Mage);
        var poor = before.With(actors: before.Actors.Select(a => a.Actor == Mage ? a.With(mp: 1) : a));
        Assert.Equal("insufficient-mp", Assert.Throws<BattleRuleException>(() => BattleActionRules.Prepare(new Sf2AttackSpellAction(),
            poor, Mage, new(4, 2), new(BattleActionKind.AttackSpell, new("blaze", 1)), Scout, new Sf2BattleProgressionRule())).Code);
        Assert.Equal(1, poor.GetActor(Mage).Mp); Assert.Equal(actor.Position, poor.GetActor(Mage).Position);
        Assert.Equal(60, poor.GetActor(Scout).Hp); Assert.Equal(before.MainSeed, poor.MainSeed);
        var malformed = Battle(d =>
        {
            d["spells"]![0]!["id"] = "blast"; d["spells"]![0]!["effect"]!["element"] = "wind";
            d["actors"]![1]!["spells"]![0]!["id"] = "blast";
        });
        Assert.Equal("source-action-categories", Assert.Throws<BattleRuleException>(() => Decide(malformed, Mage)).Code);
    }

    [Fact]
    public void UnboundTerminalEnemyDamageRejectsBeforeMovementOrMpCanPublish()
    {
        var before = Battle();
        before = before.With(actors: before.Actors.Select(a => a.Actor == Scout ? a.With(hp: 1) : a));
        var error = Assert.Throws<BattleRuleException>(() => Decide(before, Mage));
        Assert.Equal("battle-outcome-program", error.Code); Assert.True(error.Unsupported);
        Assert.Equal(5, before.GetActor(Mage).Mp); Assert.Equal(new MapPosition(6, 2), before.GetActor(Mage).Position);
        Assert.Equal(1, before.GetActor(Scout).Hp);
    }

    [Fact]
    public void ClericCounterChanceBelongsToDefenderAndDoesNotChangeOrdinaryCounterOrDoubleChance()
    {
        var before = Battle(); var physical = new Sf2PhysicalAction();
        before = before.With(actors: before.Actors.Select(a => a.Actor == Scout ? a.With(position: new(6, 4)) : a));
        var playerAttack = BattleActionRules.Prepare(physical, before, Scout, new(6, 4), new(BattleActionKind.Physical), Cleric,
            new Sf2BattleProgressionRule());
        Assert.Equal((ushort)16, playerAttack.ConstructionEffects.First(e => e.Kind == "rng-counter").RandomRange);
        Assert.Equal((ushort)32, playerAttack.ConstructionEffects.First(e => e.Kind == "rng-double").RandomRange);
        var clericAttack = BattleActionRules.Prepare(physical, before, Cleric, new(6, 5), new(BattleActionKind.Physical), Scout,
            new Sf2BattleProgressionRule());
        Assert.Equal((ushort)32, clericAttack.ConstructionEffects.First(e => e.Kind == "rng-counter").RandomRange);
    }
}
