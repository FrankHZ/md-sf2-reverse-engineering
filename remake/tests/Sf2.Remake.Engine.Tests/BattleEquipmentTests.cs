using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleEquipmentTests
{
    private static readonly ActorRef Champion = new("champion"), Guardian = new("guardian");
    private static EngineBattleState Battle(Action<JsonNode>? change = null) => Start("battle-equipment", change).Current.Battle;

    [Fact]
    public void SlotsRetainDuplicateIdentityNonweaponCarryAndAllOtherState()
    {
        var before = Battle(d => d["actors"]![0]!["items"] = new JsonArray(178, 61, 50, 256)); var actor = before.GetActor(Champion);
        var options = BattleEquipment.Choices(actor);
        Assert.Equal(new[] { 0, 1, 2, 4 }, options.Select(o => o.Slot));
        Assert.Equal(new byte[] { 25, 30, 25, 20 }, options.Select(o => o.Attack));
        var otherSlot = BattleEquipment.Equip(actor, 2);
        Assert.Equal(new ushort[] { 50, 61, 178, 256 }, otherSlot.SourceLoadout!.Items);
        Assert.Equal(25, otherSlot.Attack); Assert.Equal(20, otherSlot.BaseAttack);
        Assert.Equal(actor.Hp, otherSlot.Hp); Assert.Equal(actor.Mp, otherSlot.Mp); Assert.Equal(actor.Exp, otherSlot.Exp);
        Assert.Same(actor.Definition, otherSlot.Definition); Assert.Equal(actor.Position, otherSlot.Position);
        Assert.Equal(actor.SourceLoadout!.Spells, otherSlot.SourceLoadout.Spells);
        var same = BattleEquipment.Equip(otherSlot, 2);
        Assert.Equal(otherSlot.SourceLoadout.Items, same.SourceLoadout!.Items); Assert.Equal(25, same.Attack);
        var unarmed = BattleEquipment.Equip(same, 4);
        Assert.Equal(new ushort[] { 50, 61, 50, 256 }, unarmed.SourceLoadout!.Items);
        Assert.Equal(20, unarmed.Attack); Assert.Equal(1, BattleEquipment.Physical(unarmed)!.MaximumRange);
        var achilles = BattleEquipment.Equip(unarmed, 1);
        Assert.Equal(30, achilles.Attack); Assert.Equal(20, achilles.BaseAttack);
        Assert.Equal(new ushort[] { 50, 189, 50, 256 }, achilles.SourceLoadout!.Items);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void UnarmedDependsOnEligibleSlotCountNotFreeInventorySlots(int count)
    {
        var actor = Battle(d =>
        {
            d["actors"]![0]!["attack"] = 20;
            d["actors"]![0]!["items"] = new JsonArray(
                Enumerable.Range(0, 4).Select(i => JsonValue.Create(i < count ? 50 : 0) as JsonNode).ToArray());
        }).GetActor(Champion);
        var choices = BattleEquipment.Choices(actor);
        Assert.Equal(count is 0 or 4 ? count : count + 1, choices.Count);
        Assert.Equal(count is > 0 and < 4, choices.Any(o => o.Slot == 4));
        if (count is 0 or 4) Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(actor, 4));
    }

    [Fact]
    public void ClassMaskAndSlotOrderDecideOffersWithoutUsingLabels()
    {
        var battle = Battle(d =>
        {
            d["weapons"]![1]!["classMask"] = 16;
            d["weapons"]![0]!["name"] = "Changed authored name";
            d["actors"]![0]!["items"] = new JsonArray(61, 50, 178, 0);
        });
        var actor = battle.GetActor(Champion);
        Assert.Equal(new[] { 1, 2, 4 }, BattleEquipment.Choices(actor).Select(o => o.Slot));
        Assert.Equal("Changed authored name", BattleEquipment.Choices(actor)[0].Label);
        Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(actor, 0));
        Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(actor, 3));
        Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(actor, -1));
    }

    [Theory]
    [InlineData(0, 25, 2)] [InlineData(1, 30, 1)] [InlineData(4, 20, 1)]
    public void GrowthReappliesLiveEquipmentOnceAndKeepsCurrentResources(int slot, int attack, int range)
    {
        var actor = BattleEquipment.Equip(Battle().GetActor(Champion), slot).With(hp: 7, exp: 100);
        var rule = new Sf2BattleProgressionRule();
        var result = BattleProgressionRules.Grow(rule, actor, 0x1234);
        Assert.Equal(2, result.Actor.Level); Assert.Equal(20, result.Actor.BaseAttack);
        Assert.Equal(attack, result.Actor.Attack); Assert.Equal(range, BattleEquipment.Physical(result.Actor)!.MaximumRange);
        Assert.Equal(7, result.Actor.Hp); Assert.Equal(actor.Mp, result.Actor.Mp); Assert.Equal(actor.MaxHp, result.Actor.MaxHp);
        Assert.Equal(actor.SourceLoadout!.Items, result.Actor.SourceLoadout!.Items);
        Assert.Same(result.Actor.SourceLoadout, result.Actor.Progress!.SourceLoadout);
        var restored = new BattleActorState(result.Actor.Deployment, 7, 0, 0, actor.Position, 0, 0, progress: result.Actor.Progress);
        Assert.Equal(attack, restored.Attack);
        var again = BattleProgressionRules.Grow(rule, result.Actor.With(exp: 100), 0x4321);
        Assert.Equal(attack, again.Actor.Attack);
        Assert.Equal(slot, BattleEquipment.EquippedSlot(again.Actor.Definition, again.Actor.SourceLoadout));
    }

    [Theory]
    [InlineData(1, 10)] [InlineData(4, 0)]
    public void NonzeroBaseGrowthUsesChangedEquipmentInsteadOfFrozenGrowthBonus(int slot, int bonus)
    {
        var equipped = BattleEquipment.Equip(Battle().GetActor(Champion), slot);
        var stats = equipped.Definition.Growth!.Stats.ToArray();
        stats[2] = new(20, 40, [new(128, 128), new(256, 128), .. Enumerable.Repeat(new StatGrowthFraction(256, 0), 27)]);
        var definition = equipped.Definition.WithGrowth(equipped.Definition.Growth with { Stats = stats, AttackBonus = 77 });
        var actor = new BattleActorState(equipped.Deployment with { Definition = definition }, 7, 0, 100,
            equipped.Position, 0, 0, sourceLoadout: equipped.SourceLoadout);
        var result = BattleProgressionRules.Grow(new Sf2BattleProgressionRule(), actor, 0x1234);
        Assert.Equal(30, result.Actor.BaseAttack);
        Assert.Equal(30 + bonus, result.Actor.Attack);
        Assert.Equal(7, result.Actor.Hp); Assert.Equal(0, result.Actor.Mp);
        Assert.Equal(actor.SourceLoadout!.Items, result.Actor.SourceLoadout!.Items);
        Assert.Equal(2, result.Effects.Count(e => e.Kind.StartsWith("rng-growth-", StringComparison.Ordinal)));
    }

    [Fact]
    public void CounterUsesCounterattackersLiveMinimumRangeAndSurvivesActionValidation()
    {
        var battle = Battle(d =>
        {
            d["encounters"]![0]!.AsObject().Remove("tarosEffectiveness");
            d["weapons"]![0]!["minimumRange"] = 2;
        });
        var champion = battle.GetActor(Champion);
        Assert.Equal(1, champion.Definition.Physical!.MinimumRange);
        Assert.Equal(2, BattleEquipment.Physical(champion)!.MinimumRange);
        var closeRange = battle.With(actors: battle.Actors.Select(a => a.Actor == Champion ? BattleEquipment.Equip(a, 1) : a));
        int observedCounters = 0;
        for (uint high = 1; high <= 64; high++)
        {
            uint seed = high << 16 | 0x1234;
            var blocked = PhysicalBattleAction.PrepareSourceDefault(battle.With(mainSeed: seed), Guardian,
                battle.GetActor(Guardian).Position!, Champion);
            Assert.DoesNotContain(blocked.Reactions, r => r.Action == "physical-counter");
            var allowed = PhysicalBattleAction.PrepareSourceDefault(closeRange.With(mainSeed: seed), Guardian,
                battle.GetActor(Guardian).Position!, Champion);
            observedCounters += allowed.Reactions.Count(r => r.Action == "physical-counter" && r.Actor == Champion);
        }
        Assert.True(observedCounters > 0);
    }

    [Fact]
    public void ActualPhysicalPreparationUsesLiveRangeDamageAndTarosConjunction()
    {
        var before = Battle().With(mainSeed: 5000u << 16 | 0x1234); var actor = before.GetActor(Champion);
        var ranged = before.With(actors: before.Actors.Select(a => a.Actor == Champion ? a.With(position: new(1, 3)) : a));
        var ineffective = PhysicalBattleAction.PrepareSourceDefault(ranged, Champion, new(1, 3), Guardian);
        Assert.Equal(BattleReactionKind.Ineffective, ineffective.Reactions[0].Kind);
        var equipped = BattleEquipment.Equip(actor, 1);
        var changed = before.With(actors: before.Actors.Select(a => a.Actor == Champion ? equipped : a));
        var effective = PhysicalBattleAction.PrepareSourceDefault(changed, Champion, actor.Position!, Guardian);
        Assert.Equal(BattleReactionKind.Damage, effective.Reactions[0].Kind);
        Assert.True(effective.Reactions[0].HpAfter < effective.Reactions[0].HpBefore);
        Assert.Throws<BattleRuleException>(() => PhysicalBattleAction.PrepareSourceDefault(changed, Champion, new(1, 3), Guardian));
        var unarmed = changed.With(actors: changed.Actors.Select(a => a.Actor == Champion ? BattleEquipment.Equip(a, 4) : a));
        Assert.Equal(BattleReactionKind.Ineffective,
            PhysicalBattleAction.PrepareSourceDefault(unarmed, Champion, actor.Position!, Guardian).Reactions[0].Kind);
    }

    [Theory]
    [InlineData("curse")] [InlineData("ring")] [InlineData("range")] [InlineData("class-mask")]
    [InlineData("stat")] [InlineData("two-equipped")] [InlineData("broken")] [InlineData("equipped-herb")]
    [InlineData("wrong-class")] [InlineData("unknown-class")] [InlineData("missing-metadata")]
    public void InvalidOrUnsupportedAdmissionIsExplicit(string variation)
    {
        var d = Document("battle-equipment");
        switch (variation)
        {
            case "curse": d["weapons"]![0]!["cursed"] = true; break;
            case "ring": d["weapons"]![0]!["type"] = "ring"; break;
            case "range": d["weapons"]![0]!["maximumRange"] = 0; break;
            case "class-mask": d["weapons"]![0]!["classMask"] = "all"; break;
            case "stat": d["actors"]![0]!["attack"] = 26; break;
            case "two-equipped": d["actors"]![0]!["items"] = new JsonArray(178, 189); break;
            case "broken": d["actors"]![0]!["items"] = new JsonArray(434, 61); break;
            case "equipped-herb": d["actors"]![0]!["items"] = new JsonArray(178, 128); break;
            case "wrong-class": d["weapons"]![0]!["classMask"] = 16; break;
            case "unknown-class": d["actors"]![0]!["classRule"] = "ordinary"; break;
            case "missing-metadata": d["weapons"]!.AsArray().RemoveAt(1); break;
        }
        Assert.IsType<ScenarioReadRejected>(Reader(d).Read());
    }

    [Fact]
    public void ChangedBaseBonusAndLiveInvalidStateCannotStackOrBypassAdmission()
    {
        var actor = Battle(d =>
        {
            d["actors"]![0]!["equipment"]!["baseAttack"] = 195;
            d["actors"]![0]!["attack"] = 200;
            d["weapons"]![0]!["attackBonus"] = 9;
        }).GetActor(Champion);
        Assert.Equal(200, BattleEquipment.Equip(actor, 1).Attack);
        Assert.Equal(195, BattleEquipment.Equip(actor, 4).Attack);
        Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(actor.With(status: 1), 1));
        Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(actor.With(hp: 0), 1));
        Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(actor.With(attack: 99), 1));
        Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(actor.With(sourceLoadout: new([178, 189, 127, 127], [63, 63, 63, 63])), 1));
    }
}
