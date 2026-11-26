using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattlePartitionTests
{
    [Fact]
    public void AreaCureUsesTheSameReducedSourcePartition()
    {
        var original = AuraHealingTests.Battle();
        var definition = original.Definition.WithDeployments(original.Definition.Deployments.Where(d => d.Actor.Value != "a-edge"));
        var battle = new EngineBattleState(definition, original.Actors.Where(a => a.Actor.Value != "a-edge"),
            original.MainSeed, original.ThinkingSeed, original.Round, original.Queue, original.Cursor, original.Gold,
            original.StartPolicy, original.Regions);
        var cure = new HealingSpellDefinition(new("detox", 4), 16, 0, 0, 3, HealingTargets.CureRadius, 2, CuresPoison: true);
        var targets = HealingTargetRules.Resolve(battle, AuraHealingTests.Caster,
            battle.GetActor(AuraHealingTests.Caster).Position!, cure, AuraHealingTests.Center);
        Assert.Equal(new[] { "b-center", "m-full", "z-priest" }, targets.Select(a => a.Actor.Value));
        Assert.All(targets, a => Assert.Equal(BattleFaction.Ally, a.Faction));
    }

    [Fact]
    public void ReducedPartitionKeepsOriginalSlotsAndOrderedAuraRecipients()
    {
        var original = AuraHealingTests.Battle();
        var rows = original.Definition.Deployments.Where(d => d.Actor.Value != "a-edge").ToArray();
        var definition = original.Definition.WithDeployments(rows);
        var slots = definition.Initialization!.SourceCombatants;
        Assert.Equal(rows.Length, slots.Count);
        Assert.Equal(original.Definition.Initialization!.SourceCombatants.Where(p => p.Value.Value != "a-edge"), slots);
        var battle = new EngineBattleState(definition, original.Actors.Where(a => a.Actor.Value != "a-edge"),
            original.MainSeed, original.ThinkingSeed, original.Round, original.Queue, original.Cursor, original.Gold,
            original.StartPolicy, original.Regions);
        var action = AuraHealingTests.Prepare(battle, 2);
        Assert.Equal(new[] { "b-center", "m-full", "z-priest" }, action.Reactions.Select(r => r.Target.Value));
        Assert.Equal(original.MainSeed, battle.MainSeed);
        Assert.Equal(original.ThinkingSeed, battle.ThinkingSeed);
        Assert.Contains(original.Definition.Initialization.SourceCombatants.Values, a => a.Value == "a-edge");
    }

    [Fact]
    public void ReplacementDefinitionsKeepSourceSlotsAndInitializationOperands()
    {
        var original = AuraHealingTests.Battle().Definition;
        var definition = original.WithDeployments(original.Deployments.Reverse().Select(d => d with
            { Position = new(d.Position.X + 1, d.Position.Y) }));
        Assert.Same(original.Initialization, definition.Initialization);
        Assert.Equal(original.Deployments.Select(d => d.Actor), definition.Deployments.Select(d => d.Actor));
    }

    [Fact]
    public void AddedActorDoesNotInventASourceSlot()
    {
        var original = AuraHealingTests.Battle().Definition;
        var row = original.Deployments[0];
        var added = row with { Definition = new(new("unmapped"), BattleClassRule.Ordinary,
            1, 10, 0, 4, 2, 3, false, 2, []), ProcessingOrder = 99 };
        var definition = original.WithDeployments(original.Deployments.Append(added));
        Assert.Equal(original.Initialization!.SourceCombatants, definition.Initialization!.SourceCombatants);
        Assert.Equal("aura-source-slots", Assert.Throws<BattleRuleException>(() => HealingTargetRules.ValidateAura(definition)).Code);
    }
}
