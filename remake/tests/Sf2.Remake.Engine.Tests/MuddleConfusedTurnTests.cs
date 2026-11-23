using System.Text.Json.Nodes;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.SilenceSpellTests;
using EnemyParty = Sf2.Remake.Engine.Tests.EnemySpellDecisionTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class MuddleConfusedTurnTests
{
    [Fact]
    public void AllyHitsOwnSideWithHalfExperienceAndKeepsTheSuccessfulFollowUpDraws()
    {
        var battle = Battle(d =>
        {
            d["actors"]![0]!["spells"] = new JsonArray();
            d["encounters"]![0]!["rewards"]!["halvedExperience"] = true;
        });
        battle = Status(Stationary(battle, Mage), Mage, 0x18);
        var (end, events) = Play(battle, Mage, Priest, 2787);
        var strike = Assert.Single(events, e => e.Kind == "physical-first");
        Assert.Equal(Priest, strike.Target);
        Assert.Equal((ushort?)0, Assert.Single(events, e => e.Kind == "rng-double").RandomValue);
        Assert.Equal((ushort?)0, Assert.Single(events, e => e.Kind == "rng-counter").RandomValue);
        Assert.DoesNotContain(events, e => e.Kind is "physical-second" or "physical-counter");
        var hit = Assert.Single(events, e => e.Kind == "hp" && e.Actor == Priest);
        var plus = Assert.Single(events, e => e.Kind == "rng-exp-plus");
        var minus = Assert.Single(events, e => e.Kind == "rng-exp-minus");
        int raw = BattleRewards.DamageExperience((int)(hit.Before!.Value - hit.After!.Value), 30, 50);
        int expected = Math.Max(1, raw / 2 + (plus.RandomValue == 0 ? 1 : 0) - (minus.RandomValue == 0 ? 1 : 0));
        Assert.True(raw > expected);
        Assert.Equal((byte?)expected, end.GetActor(Mage).Exp);
        Assert.Equal(battle.Gold, end.Gold);
        Assert.Equal(battle.GetActor(Smoke).Hp, end.GetActor(Smoke).Hp);
        Assert.Equal(BattleControl.Player, end.GetActor(Mage).Control);
        Assert.Equal(0, end.GetActor(Mage).Status);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ProtectedCenterGuardsOnEitherSideWithoutASelfDraw(bool enemy)
    {
        var battle = enemy ? EnemyParty.Battle(d =>
        {
            d["encounters"]![0]!["placements"]![2]!["y"] = 3;
            d["encounters"]![0]!["placements"]![2]!["initialization"]!["commandset"] = 8;
        }) : Battle();
        var actor = enemy ? EnemyParty.Cleric : Priest;
        var protectedTarget = enemy ? EnemyParty.Mage : Mage;
        var next = enemy ? EnemyParty.Scout : Mage;
        battle = Status(Stationary(battle, actor), actor, 0x18);
        var (end, events) = Play(battle, actor, next, 2521);
        Assert.Equal(protectedTarget, Assert.Single(events, e => e.Kind == "ai-target").Target);
        Assert.DoesNotContain(events, e => e.Kind is "rng-muddle-self-guard" or "movement" or "hp" or "mp");
        var line = Assert.Single(events, e => e.Kind == "rng-muddle-inaction-message");
        Assert.Equal((ushort?)16, line.RandomRange);
        Assert.Equal(2521L << 16, line.Before);
        var message = Assert.Single(events, e => e.Kind == "muddle-inaction");
        Assert.Equal((322 + (line.RandomValue <= 9 ? line.RandomValue!.Value : 0)).ToString(), message.Detail);
        Assert.Equal(battle.GetActor(actor).LastTarget, end.GetActor(actor).LastTarget);
        Assert.Equal(battle.GetActor(actor).Mp, end.GetActor(actor).Mp);
        Assert.Equal(enemy ? 0 : 1, events.Count(e => e.Kind == "exp"));
    }

    [Theory]
    [InlineData(194u, false)]
    [InlineData(2521u, true)]
    public void SelfPhysicalGuardIsDrawnOnceAndOnlyASuccessfulHitChangesTheActor(uint seed, bool guarded)
    {
        var battle = Battle(d =>
        {
            d["encounters"]![0]!["placements"]![0]!["x"] = 0;
            d["encounters"]![0]!["placements"]![1]!["x"] = 7;
        });
        battle = Status(battle, Priest, 0x18);
        var before = battle.GetActor(Priest);
        var (end, events) = Play(battle, Priest, Mage, seed);
        var roll = Assert.Single(events, e => e.Kind == "rng-muddle-self-guard");
        Assert.Equal(guarded ? (ushort)1 : (ushort)0, roll.RandomValue);
        Assert.Equal(Priest, roll.Target);
        Assert.Equal(before.Position, end.GetActor(Priest).Position);
        Assert.Equal(guarded, events.Any(e => e.Kind == "muddle-inaction"));
        Assert.Equal(guarded ? 0 : 1, events.Count(e => e.Kind == "hp" && e.Actor == Priest));
        if (!guarded) Assert.True(end.GetActor(Priest).Hp < before.Hp);
    }

    [Theory]
    [InlineData(false, false, 1680u)]
    [InlineData(false, true, 1680u)]
    [InlineData(true, false, 2715u)]
    public void SelfAttackPreservesTheSameActorsPaymentWearGrowthAndDeath(bool item, bool lethal, uint seed)
    {
        var battle = Battle(d =>
        {
            d["encounters"]![0]!["placements"]![0]!["x"] = 0;
            d["encounters"]![0]!["placements"]![1]!["x"] = 7;
        });
        var priest = battle.GetActor(Priest);
        var blaze = new SpellRef("blaze", 1);
        var physical = priest.Definition.Physical! with { Promoted = true };
        var equipment = new BattleEquipmentDefinition(10, [], rings:
            [new(24, "Black Ring", 1u << 17, RingStat.Attack, 10, DamageOnUse: true, Cursed: true)]);
        var curve = Enumerable.Repeat(new StatGrowthFraction(128, 128), 29).ToArray();
        var stats = new byte[] { 30, 20, 10, 5, 60 }.Select(v => new StatGrowth(v, (byte)(v + 10), curve)).ToArray();
        var growth = new BattleGrowthDefinition(17, 0, stats, [], new Dictionary<byte, SpellRef>());
        var loadout = new BattleSourceLoadout(item ? [152, 127, 127, 127] : [127, 127, 127, 127],
            item ? [63, 63, 63, 63] : [11, 63, 63, 63]);
        var definition = new BattleActorDefinition(Priest, BattleClassRule.Wizard, 5, 30, 20, 10, 5, 60, false, 2,
            item ? [] : [blaze], physical, BattleMover.Mage, loadout, growth, equipment);
        var owner = new BattleActorState(priest.Deployment with { Definition = definition }, lethal ? (ushort)1 : (ushort)30,
            20, 99, priest.Position, 0, 0, status: item ? (ushort)0x1C : (ushort)0x18, activationWord: 0);
        battle = Replace(battle, owner, attackItems: item
            ? [new(24, "Black Ring", new(new("blaze", 2), 0, 10, 1, 2, 1, AttackSpellElement.Fire), Breakable: true)] : []);

        var (end, events) = Play(battle, Priest, Mage, seed);
        var played = end.GetActor(Priest);
        Assert.Single(events, e => e.Kind == "hp" && e.Actor == Priest);
        Assert.Equal(Priest, Assert.Single(events, e => e.Kind == (item ? "ai-item-target" : "ai-spell-target")).Target);
        Assert.Equal(item ? 20 : 18, played.Mp);
        Assert.Equal(item ? 0 : 1, events.Count(e => e.Kind == "mp" && e.Actor == Priest));
        if (item)
        {
            Assert.Equal(408, played.SourceLoadout!.Items[0]);
            Assert.Single(events, e => e.Kind == "item-damaged");
        }
        if (lethal)
        {
            Assert.Equal(0, played.Hp);
            Assert.Null(played.Position);
            Assert.Equal(0, played.Status);
            Assert.Equal((ushort?)1, played.Defeats);
            Assert.Equal((byte?)99, played.Exp);
            Assert.DoesNotContain(events, e => e.Kind is "exp" or "rng-muddle-expiry");
        }
        else
        {
            Assert.InRange(played.Hp, 1, 29);
            Assert.Single(events, e => e.Kind == "level");
            Assert.Equal(6, played.Level);
            Assert.Equal(item ? CurseRules.Mask : 0, played.Status);
        }
        Assert.Equal((ushort?)0, played.Kills);
        Assert.Equal(battle.Gold, end.Gold);
    }

    [Fact]
    public void EnemyPhysicalKillKeepsCustodyAndDoesNotCreditThePlayer()
    {
        var battle = EnemyParty.Battle(d =>
        {
            d["actors"]![1]!["spells"] = new JsonArray();
            d["encounters"]![0]!["placements"]![3]!["x"] = 5;
            d["encounters"]![0]!["placements"]![3]!["y"] = 2;
        });
        var victim = battle.GetActor(EnemyParty.Wounded).With(hp: 1, status: 0x10,
            sourceLoadout: new([24, 127, 127, 127], [63, 63, 63, 63]));
        var drops = new EnemyItemDropDefinition(26, new Dictionary<ActorRef, byte> { [victim.Actor] = 130 },
            [new(26, 130, 24, 10)], [new(24, "Black Ring", true)]);
        battle = Replace(battle, victim, drops: drops);
        battle = Status(Stationary(battle, EnemyParty.Mage), EnemyParty.Mage, 0x18);
        var (end, events) = Play(battle, EnemyParty.Mage, EnemyParty.Scout, 2787);
        Assert.Equal(victim.Actor, Assert.Single(events, e => e.Kind == "physical-first").Target);
        Assert.Equal(0, end.GetActor(victim.Actor).Hp);
        Assert.Equal(0, end.GetActor(victim.Actor).Status);
        Assert.Null(end.GetActor(victim.Actor).Position);
        Assert.Equal(24, end.GetActor(victim.Actor).SourceLoadout!.Items[0]);
        Assert.Equal(battle.Gold, end.Gold);
        Assert.Equal(battle.GetActor(EnemyParty.Scout).Kills, end.GetActor(EnemyParty.Scout).Kills);
        Assert.Empty(end.ItemCustody!.OnceFlags);
        Assert.Empty(end.ItemCustody.Deals);
        Assert.DoesNotContain(events, e => e.Kind is "exp" or "kills" or "gold" or "item-dropped" or "rng-item-drop");
        Assert.Null(end.DeathCredit?.Ally);
    }

    [Fact]
    public void AreaCanHitProtectedSlotAndEndTheBattleThroughTheDeathBatch()
    {
        var battle = EnemyParty.Battle(d =>
        {
            d["actors"]![1]!["spells"]![0]!["level"] = 2;
            d["spells"]![0]!["level"] = 2;
            d["spells"]![0]!["effect"]!["radius"] = 1;
            d["encounters"]![0]!["placements"]![3]!["x"] = 5;
            d["encounters"]![0]!["placements"]![3]!["y"] = 2;
        });
        battle = Stationary(battle, EnemyParty.Mage);
        var caster = battle.GetActor(EnemyParty.Mage).With(hp: 1, status: 0x18);
        battle = Replace(battle, caster, outcome: new(EnemyParty.Scout, EnemyParty.Wounded));
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == EnemyParty.Cleric ? a.With(hp: 0, clearPosition: true) :
            a.Actor == EnemyParty.Wounded ? a.With(hp: 1, status: 0x10) : a));
        var (end, events) = Play(battle, EnemyParty.Mage, EnemyParty.Scout, 1680,
            scene =>
            {
                if (scene.Phase != BattleScenePhase.Reaction || !scene.HasReaction) return;
                Assert.Null(scene.DisplayedAlly);
                Assert.Equal(scene.ReactionTarget, scene.DisplayedEnemy);
            });
        Assert.Equal(EnemyParty.Wounded, Assert.Single(events, e => e.Kind == "ai-spell-target").Target);
        Assert.Equal(3, end.GetActor(caster.Actor).Mp);
        Assert.Equal(BattleOutcomeKind.Victory, BattleOutcomeRules.Check(end));
        foreach (var casualty in new[] { caster.Actor, EnemyParty.Wounded })
        {
            Assert.Equal(0, end.GetActor(casualty).Hp);
            Assert.Null(end.GetActor(casualty).Position);
            Assert.Equal(0, end.GetActor(casualty).Status);
            Assert.Single(events, e => e.Actor == casualty && e.Kind == "death-cleanup");
        }
        Assert.DoesNotContain(events, e => e.Kind is "muddle-inaction" or "gold" or "kills");
    }

    [Fact]
    public void PromotedMonsterConvertsAnAdmittedOwnSideStrikeAfterTheGuard()
    {
        var battle = Battle(d =>
        {
            d["actors"]![0]!["spells"] = new JsonArray();
            d["actors"]![1]!["maxHp"] = 60;
            d["start"]!["actors"]![1]!["hp"] = 60;
        });
        var mage = battle.GetActor(Mage);
        var physical = mage.Definition.Physical! with { Promoted = true };
        var profile = new BattleActorDefinition(Mage, BattleClassRule.Monster, 25, 30, 20, 10, 5, 100,
            false, 0, [], physical, BattleMover.Hovering, mage.SourceLoadout);
        var kiwi = new BattleActorState(mage.Deployment with { Definition = profile }, mage.Hp, mage.Mp, mage.Exp,
            mage.Position, 0, 0, status: 0x18, activationWord: 0);
        var breath = new AttackSpellDefinition(new("kiwi", 1), 0, 28, 1, 1, 0, AttackSpellElement.Fire);
        battle = Replace(battle, kiwi, extraSpells: [breath]);
        var (end, events) = Play(battle, Mage, Priest, 1);
        Assert.Equal(Priest, Assert.Single(events, e => e.Kind == "ai-target").Target);
        Assert.Equal((ushort?)0, Assert.Single(events, e => e.Kind == "rng-kiwi-conversion").RandomValue);
        Assert.Single(events, e => e.Kind == "attack-spell" && e.Target == Priest);
        Assert.DoesNotContain(events, e => e.Kind is "mp" or "physical-first" or "muddle-inaction");
        Assert.Equal(20, end.GetActor(Mage).Mp);
        Assert.InRange(end.GetActor(Priest).Hp, 1, 59);
    }

    [Fact]
    public void HighPriorityEnemyTieRetainsCategoryDrawAndContinuesToMove()
    {
        var battle = EnemyParty.Battle(d =>
        {
            d["encounters"]![0]!["placements"]![1]!["initialization"]!["commandset"] = 6;
            d["encounters"]![0]!["placements"]![2]!["y"] = 3;
            d["encounters"]![0]!["placements"]![3]!["x"] = 5;
            d["encounters"]![0]!["placements"]![3]!["y"] = 2;
        });
        battle = Stationary(battle, EnemyParty.Mage);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == EnemyParty.Mage
            ? a.With(status: 0x18, activationWord: 0x3001)
            : a.IsAlly ? a : a.With(hp: 1)));
        var (end, events) = Play(battle, EnemyParty.Mage, EnemyParty.Scout, 1680);
        var category = Assert.Single(events, e => e.Kind == "rng-ai-attack-category");
        Assert.Equal((ushort?)2, category.RandomValue);
        Assert.Equal(2, events.Count(e => e.Kind == "ai-spell-candidate" && e.After == 17));
        Assert.Equal(EnemyParty.Mage, Assert.Single(events, e => e.Kind == "ai-move-target").Target);
        Assert.Equal(category.After, Assert.Single(events, e => e.Kind == "rng-muddle-expiry").Before);
        Assert.DoesNotContain(events, e => e.Kind is "hp" or "mp" or "muddle-inaction");
        Assert.Equal(battle.GetActor(EnemyParty.Mage).LastTarget, end.GetActor(EnemyParty.Mage).LastTarget);
    }

    [Fact]
    public void AbsenceOfReachableAttacksPursuesItsOwnFixedSlot()
    {
        var battle = Battle(d =>
        {
            d["actors"]![1]!["move"] = 1;
            d["encounters"]![0]!["placements"]![0]!["x"] = 0;
            d["encounters"]![0]!["placements"]![0]!["y"] = 3;
            d["encounters"]![0]!["placements"]![1]!["x"] = 7;
            d["terrains"]![0]!["rows"]![3] = "pppppppppp";
        });
        var member = battle.GetActor(Priest);
        var physical = member.Definition.Physical! with { MinimumRange = 2, MaximumRange = 2 };
        var profile = new BattleActorDefinition(Priest, member.Definition.ClassRule, 5, 30, 20, 10, 5, 60,
            false, 1, member.Spells, physical, member.Definition.Mover);
        var owner = new BattleActorState(member.Deployment with { Definition = profile }, 30, 20, 0,
            member.Position, 0, 0, status: 0x18, activationWord: 0, sourceLoadout: member.SourceLoadout);
        battle = Replace(battle, owner);
        var (end, events) = Play(battle, Priest, Mage, 4500);
        Assert.Equal(Mage, Assert.Single(events, e => e.Kind == "ai-move-target").Target);
        Assert.True(end.GetActor(Priest).Position!.X < owner.Position!.X);
        Assert.DoesNotContain(events, e => e.Kind is "physical-first" or "rng-muddle-self-guard" or "muddle-inaction");
    }

    internal static EngineBattleState Stationary(EngineBattleState before, ActorRef actorRef,
        bool enhanced = false, BattleOutcomeDefinition? outcome = null)
    {
        var actor = before.GetActor(actorRef);
        var d = actor.Definition;
        var definition = new BattleActorDefinition(d.Actor, d.ClassRule, d.Level, d.MaxHp, d.MaxMp, d.Attack,
            d.Defense, d.Agility, d.ExtraRoundAction, 0, d.Spells, d.Physical, d.Mover, d.SourceLoadout,
            d.Growth, d.Equipment, d.Resistance, enhancedSpellPower: enhanced);
        var stationary = new BattleActorState(actor.Deployment with { Definition = definition },
            actor.Hp, actor.Mp, actor.Exp, actor.Position, actor.Kills, actor.Defeats, lastTarget: actor.LastTarget,
            status: actor.Status, activationWord: actor.ActivationWord, sourceLoadout: actor.SourceLoadout);
        return Replace(before, stationary, outcome: outcome, drops: before.Definition.ItemDrops);
    }

    private static EngineBattleState Replace(EngineBattleState before, BattleActorState actor,
        AttackItemDefinition[]? attackItems = null, BattleOutcomeDefinition? outcome = null,
        EnemyItemDropDefinition? drops = null, AttackSpellDefinition[]? extraSpells = null)
    {
        var d = before.Definition;
        var updated = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain,
            d.Deployments.Select(row => row.Actor == actor.Actor ? actor.Deployment : row),
            d.Spells.Values, d.Rewards, d.Initialization, outcome ?? d.Outcome,
            attackSpells: d.AttackSpells.Values.Concat(extraSpells ?? []),
            supportSpells: d.SupportSpells.Values, attackItems: attackItems, itemDrops: drops);
        return new(updated, before.Actors.Select(a => a.Actor == actor.Actor ? actor : a), before.MainSeed, before.ThinkingSeed,
            before.Round, before.Queue, before.Cursor, before.Gold, before.StartPolicy, before.Regions,
            itemCustody: drops is null ? before.ItemCustody : new([], new Dictionary<byte, byte>()));
    }

    internal static (EngineBattleState Battle, List<SessionObservation> Events) Play(
        EngineBattleState battle, ActorRef actor, ActorRef next, uint seed, Action<BattleSceneState>? observe = null)
    {
        battle = battle.With(mainSeed: seed << 16, queue: [new(actor, 30), new(next, 20), new(null, 255)], cursor: 0);
        var rules = RuleCompositions.Sf2();
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null), new StoryState(),
            SessionStopReason.SimulationWait);
        var result = BattleAdvancer.Advance(snapshot, [], rules);
        List<SessionObservation> events = [];
        for (int i = 0; i < 200; i++)
        {
            Assert.True(result.Failure is null, result.Failure?.ToString());
            events.AddRange(result.Observations);
            var current = result.Snapshot;
            if (current.BattleMovement is { } move)
                result = BattleMovementContinuation.Submit(current, new CompletePresentation(move.Token, move.CompletionKind), rules);
            else if (current.BattleScene is { } scene)
            {
                observe?.Invoke(scene);
                result = BattleSceneContinuation.Submit(current, scene.RequiresAcknowledgement
                    ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind), rules);
            }
            else
            {
                Assert.True(current.Selection?.Actor == next || BattleOutcomeRules.Check(current.Battle) is not null);
                return (current.Battle, events);
            }
        }
        throw new InvalidOperationException("Automatic turn did not settle.");
    }
}
