using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.AttackSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class MonsterBreathTests
{
    private const uint BreathSeed = 0x00011234;
    private const uint StrikeSeed = 0x40001234;
    private static readonly BattleActionRef Attack = new(BattleActionKind.Physical);

    [Theory]
    [InlineData(1, 1, 23)]
    [InlineData(31, 1, 23)]
    [InlineData(32, 2, 30)]
    [InlineData(39, 2, 30)]
    [InlineData(40, 3, 38)]
    [InlineData(49, 3, 38)]
    [InlineData(50, 4, 50)]
    [InlineData(99, 4, 50)]
    public void PhysicalCommitUsesStoredLevelAndSourceOrderedFireDamage(byte level, byte rank, int damage)
    {
        var battle = Arena(BattleClassRule.Monster, level, BreathSeed);
        var actor = battle.GetActor(Caster);
        Assert.Empty(new Sf2AttackSpellAction().QueryActions(battle, Caster));
        var selected = new BattleActionRef(BattleActionKind.AttackSpell, new("kiwi", rank));
        Assert.Equal("spell-not-known", Assert.Throws<BattleRuleException>(() =>
            new Sf2AttackSpellAction().RequireAction(battle, Caster, selected)).Code);
        var action = Commit(battle);
        var spell = Assert.IsType<AttackSpellDefinition>(action.AttackSpell);
        Assert.Equal(new SpellRef("kiwi", rank), spell.Spell);
        Assert.Equal(((byte)1, (byte)1, (byte)0, (byte)0, AttackSpellElement.Fire),
            (spell.MinimumRange, spell.MaximumRange, spell.Radius, spell.MpCost, spell.Element));

        var reaction = Assert.Single(action.Reactions);
        Assert.Equal(Center, reaction.Target);
        Assert.True(reaction.Critical);
        Assert.Equal(damage, reaction.Amount);
        Assert.Equal(90 - damage, reaction.HpAfter);
        Assert.Empty(action.Prepared.GetActor(Caster).Spells);
        Assert.Equal(new byte[] { 63, 63, 63, 63 }, action.Prepared.GetActor(Caster).SourceLoadout!.Spells);
        Assert.Equal(0, action.Prepared.GetActor(Caster).Mp);
        Assert.Equal(actor.Status, action.Prepared.GetActor(Caster).Status);
        var draws = action.ConstructionEffects.Where(e => e.RandomRange.HasValue).Take(4).ToArray();
        Assert.Equal(new[] { "rng-kiwi-conversion", "rng-spell-critical", "rng-spell-variance", "rng-spell-variance" },
            draws.Select(e => e.Kind));
        Assert.Equal(new ushort?[] { 4, 16 }, draws.Take(2).Select(e => e.RandomRange));
        Assert.Equal(BreathSeed, draws[0].Before);
        Assert.Equal<ushort?>(0, draws[0].RandomValue);
        Assert.Equal(draws.Select(e => e.After).Take(3), draws.Skip(1).Select(e => e.Before));
        Assert.Equal(battle.ThinkingSeed, action.Prepared.ThinkingSeed);
    }

    [Fact]
    public void NonzeroLeavesThePhysicalStrikeAndOnlyMonsterCommitDrawsTheGate()
    {
        var battle = Arena(BattleClassRule.Monster, 20, StrikeSeed);
        var rule = new Sf2PhysicalAction();
        var position = battle.GetActor(Caster).Position!;
        Assert.Equal<byte?>(1, rule.QueryRange(battle, Caster).MaximumRange);
        Assert.Throws<BattleRuleException>(() => rule.RequireTarget(battle, Caster, position, Attack, Wing));
        Assert.Equal(StrikeSeed, battle.MainSeed);
        var action = Commit(battle);
        Assert.Null(action.AttackSpell);
        Assert.Equal("physical-first", action.Reactions[0].Action);
        Assert.Equal("rng-kiwi-conversion", action.ConstructionEffects[0].Kind);
        Assert.Equal<ushort?>(1, action.ConstructionEffects[0].RandomValue);
        var dodge = action.ConstructionEffects.First(e => e.Kind == "rng-dodge");
        Assert.Equal(action.ConstructionEffects[0].After, dodge.Before);

        var tortoise = Commit(Arena(BattleClassRule.UnpromotedTortoise, 20, BreathSeed));
        Assert.Null(tortoise.AttackSpell);
        Assert.DoesNotContain(tortoise.ConstructionEffects, e => e.Kind == "rng-kiwi-conversion");
    }

    [Fact]
    public void BreathSceneFlowsFromAttackMessageToAnimationWithoutSpellPayment()
    {
        var battle = Arena(BattleClassRule.Monster, 20, BreathSeed);
        var current = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(battle, null),
            new StoryState(), SessionStopReason.PlayerInput);
        var result = BattleSceneContinuation.Begin(current, Commit(battle), []);
        var scene = result.Snapshot.BattleScene!;
        Assert.True(scene.IsMonsterBreath);
        result = BattleSceneContinuation.Submit(result.Snapshot, new CompletePresentation(scene.Token, scene.CompletionKind));
        Assert.Null(result.Failure);
        Assert.Equal(BattleScenePhase.ActionMessage, result.Snapshot.BattleScene!.Phase);
        result = BattleSceneContinuation.Submit(result.Snapshot, new Acknowledge(result.Snapshot.BattleScene.Token));
        Assert.Null(result.Failure);
        Assert.Equal(BattleScenePhase.ActionAnimation, result.Snapshot.BattleScene!.Phase);
        Assert.DoesNotContain(result.Observations, e => e.Kind == "mp");
        Assert.Equal(SilenceRules.Mask, result.Snapshot.Battle.GetActor(Caster).Status);
    }

    private static BattleActionResolution Commit(EngineBattleState battle) =>
        BattleActionRules.Prepare(new Sf2PhysicalAction(), battle, Caster, battle.GetActor(Caster).Position!,
            Attack, Center, new Sf2BattleProgressionRule());

    private static EngineBattleState Arena(BattleClassRule kind, byte level, uint seed)
    {
        var original = Battle(d =>
        {
            d["encounters"]![0]!["placements"]![3]!["x"] = 3;
            d["actors"]![3]!["maxHp"] = 90;
            d["actors"]![3]!["resistance"] = 0x380; // FIRE major and NEUTRAL weakness
            d["start"]!["actors"]![3]!["hp"] = 90;
        });
        var source = original.Definition;
        var old = original.GetActor(Caster);
        var definition = new BattleActorDefinition(Caster, kind, level, old.MaxHp, 0, old.Attack, old.Defense,
            old.Agility, false, 7, [], old.Definition.Physical! with { Promoted = kind == BattleClassRule.Monster },
            kind == BattleClassRule.Monster ? BattleMover.Hovering : BattleMover.Stealth,
            new([127, 127, 127, 127], [63, 63, 63, 63]));
        var deployment = old.Deployment with { Definition = definition };
        var spells = new ushort[] { 28, 36, 45, 58 }.Select((power, index) =>
            new AttackSpellDefinition(new("kiwi", (byte)(index + 1)), 0, power, 1, 1, 0, AttackSpellElement.Fire, 76));
        var encounter = new BattleDefinition(source.Encounter, source.Map, source.Width, source.Height, source.Terrain,
            source.Deployments.Select(d => d.Actor == Caster ? deployment : d), source.Spells.Values, source.Rewards,
            attackSpells: spells);
        var actors = original.Actors.Select(actor => actor.Actor == Caster
            ? new BattleActorState(deployment, old.Hp, 0, old.Exp, old.Position, old.Kills, old.Defeats, status: SilenceRules.Mask)
            : actor);
        return new(encounter, actors, seed, original.ThinkingSeed, original.Round, original.Queue, original.Cursor, original.Gold);
    }
}
