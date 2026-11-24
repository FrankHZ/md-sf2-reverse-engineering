using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class HealingTargetRules
{
    internal static IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor)
    {
        var faction = battle.GetActor(actor).Faction;
        return Array.AsReadOnly(battle.Actors.Where(target => target.Hp > 0 && target.Position is not null &&
            (target.ActivationWord.GetValueOrDefault() & 8) == 0 && target.Faction == faction).Select(target => target.Actor).ToArray());
    }

    internal static BattleActorState RequireTarget(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, byte minimumRange, byte maximumRange, ActorRef targetRef)
    {
        var actor = battle.GetActor(actorRef);
        var target = battle.Actors.SingleOrDefault(a => a.Actor == targetRef);
        if (target is null || target.Hp == 0 || target.Position is null || (target.ActivationWord.GetValueOrDefault() & 8) != 0 ||
            target.Faction != actor.Faction)
            throw new BattleRuleException("invalid-heal-target", "target");
        var targetPosition = targetRef == actorRef ? destination : target.Position!;
        if (!BattleRange.Contains(destination, targetPosition, minimumRange, maximumRange))
            throw new BattleRuleException("target-range", "target");
        return target;
    }

    // Source BuildSpellRangeGrid uses the teammate occupancy grid, even for AURA4's
    // range-zero center. Its later all-ally scan has a different neutral-bit rule.
    internal static BattleActorState RequireCenter(EngineBattleState battle, ActorRef actor,
        MapPosition destination, HealingSpellDefinition spell, ActorRef center)
    {
        if (spell.IsAura && (battle.Actors.Any(unit => unit.Actor == center && (unit.ActivationWord.GetValueOrDefault() & 8) != 0) ||
            spell.Targets == HealingTargets.AuraAllAllies && center != actor))
            throw new BattleRuleException("invalid-aura-center", "target");
        var target = RequireTarget(battle, actor, destination, spell.MinimumRange, spell.MaximumRange, center);
        if (spell.Targets == HealingTargets.CureRadius)
            ValidateSourceSlots(battle.Definition, "cure-source-slots");
        return target;
    }

    internal static IReadOnlyList<BattleActorState> Resolve(EngineBattleState battle, ActorRef actor,
        MapPosition destination, HealingSpellDefinition spell, ActorRef center)
    {
        var target = RequireCenter(battle, actor, destination, spell, center);
        if (spell.Targets == HealingTargets.Single) return [target];
        var caster = battle.GetActor(actor);
        if (spell.IsAura) RequireAuraCaster(battle, caster);
        var origin = center == actor ? destination : target.Position!;
        var placed = PlacedSide(battle, caster);
        return spell.Targets == HealingTargets.AuraAllAllies ? placed : placed.Where(row =>
            (row.ActivationWord.GetValueOrDefault() & 8) == 0 &&
            BattleRange.Contains(origin, row.Actor == actor ? destination : row.Position!, 0, spell.Radius)).ToArray();
    }

    internal static void RequireAuraCaster(EngineBattleState battle, BattleActorState caster)
    {
        ValidateAura(battle.Definition);
        bool supportedStatus = MuddleRules.SupportedRankOne(caster.Status, battle.Definition.Poison);
        if ((caster.IsAlly ? caster.Control != BattleControl.Player : caster.Control != BattleControl.Automatic) ||
            !supportedStatus || (caster.Status & 0xC0) != 0)
            throw new BattleRuleException("aura-caster", "actor", true);
    }

    internal static IReadOnlyList<BattleActorState> ResolveItem(EngineBattleState battle, ActorRef actor,
        MapPosition destination, HealingItemDefinition item, ActorRef center)
    {
        bool party = item.Effect == HealingItemEffect.FullPartyHp;
        if (party && center != actor) throw new BattleRuleException("invalid-aura-center", "target");
        var target = RequireTarget(battle, actor, destination, item.MinimumRange, item.MaximumRange, center);
        if (!party) return [target];
        ValidateAreaRoster(battle.Definition);
        return PlacedSide(battle, target);
    }

    private static BattleActorState[] PlacedSide(EngineBattleState battle, BattleActorState center)
    {
        var actors = battle.Actors.ToDictionary(row => row.Actor);
        return battle.Definition.Initialization!.SourceCombatants.OrderBy(row => row.Key)
            .Where(row => center.IsAlly ? row.Key < 30 : row.Key >= 128).Select(row => actors[row.Value])
            .Where(row => row.Hp > 0 && row.Position is not null).ToArray();
    }

    private static void ValidateAreaRoster(BattleDefinition definition)
    {
        // bsc0B uses a signed delta; IncreaseAndClampWord also branches on the
        // sign of current HP + recovery. Keep this finite family in their common domain.
        if (definition.Deployments.Any(d => d.Definition.MaxHp is 0 or > 32767))
            throw new BattleRuleException("aura-hp-domain", "actors.maxHp", true);
        ValidateSourceSlots(definition, "aura-source-slots");
    }

    private static void ValidateSourceSlots(BattleDefinition definition, string error)
    {
        var slots = definition.Initialization?.SourceCombatants;
        var deployments = definition.Deployments.ToDictionary(d => d.Actor);
        if (slots is null || slots.Count != definition.Deployments.Count ||
            slots.Values.Distinct().Count() != slots.Count || slots.Any(row =>
                !deployments.TryGetValue(row.Value, out var d) ||
                    !(d.Faction == BattleFaction.Ally ? row.Key is >= 0 and < 30 : row.Key is >= 128 and < 160)))
            throw new BattleRuleException(error, "initialization.sourceCombatants", true);
    }

    internal static void ValidateAura(BattleDefinition definition)
    {
        if (!definition.Spells.Values.Any(s => s.IsAura)) return;
        ValidateAreaRoster(definition);
        foreach (var spell in definition.Spells.Values.Where(s => s.IsAura))
        {
            int level = spell.Spell.Level;
            if (level is < 1 or > 4 || spell.MpCost != new[] { 7, 11, 15, 20 }[level - 1] ||
                spell.Power != new[] { 15, 15, 30, 255 }[level - 1] || spell.MinimumRange != 0 ||
                spell.MaximumRange != (level == 4 ? 0 : 3) || spell.Radius != (level == 4 ? 0 : level == 1 ? 1 : 2) ||
                spell.Targets != (level == 4 ? HealingTargets.AuraAllAllies : HealingTargets.AuraRadius))
                throw new BattleRuleException("aura-definition", "spell", true);
        }
        if (definition.Deployments.Any(d => d.Definition.Spells.Any(s => definition.Spells.TryGetValue(s, out var spell) && spell.IsAura) &&
            (d.Faction == BattleFaction.Ally ? d.Control != BattleControl.Player || d.SourceAllyControl ||
                !Sf2ClassRules.PlayerHealer(d.Definition.ClassRule) : d.SourceEnemy is null ||
                d.Control != BattleControl.Automatic || d.Definition.ClassRule != BattleClassRule.Ordinary ||
                d.Definition.Spells.Any(s => s.Value == "aura" && s.Level != 1))))
            throw new BattleRuleException("aura-caster", "placements", true);
    }
}
