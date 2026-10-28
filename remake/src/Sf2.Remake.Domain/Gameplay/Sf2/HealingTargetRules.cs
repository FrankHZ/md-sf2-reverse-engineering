using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal static class HealingTargetRules
{
    internal static IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor)
    {
        var faction = battle.GetActor(actor).Faction;
        return Array.AsReadOnly(battle.Actors.Where(target => target.Hp > 0 && target.Position is not null &&
            target.Faction == faction).Select(target => target.Actor).ToArray());
    }

    internal static BattleActorState RequireTarget(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, byte minimumRange, byte maximumRange, ActorRef targetRef)
    {
        var actor = battle.GetActor(actorRef);
        var target = battle.Actors.SingleOrDefault(a => a.Actor == targetRef);
        if (target is null || target.Hp == 0 || target.Position is null || target.Faction != actor.Faction)
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
        var target = RequireTarget(battle, actor, destination, spell.MinimumRange, spell.MaximumRange, center);
        if (spell.IsAura && ((target.ActivationWord & 8) != 0 ||
            spell.Targets == HealingTargets.AuraAllAllies && center != actor))
            throw new BattleRuleException("invalid-aura-center", "target");
        return target;
    }

    internal static IReadOnlyList<BattleActorState> Resolve(EngineBattleState battle, ActorRef actor,
        MapPosition destination, HealingSpellDefinition spell, ActorRef center)
    {
        var target = RequireCenter(battle, actor, destination, spell, center);
        if (!spell.IsAura) return [target];
        ValidateAura(battle.Definition);
        var caster = battle.GetActor(actor);
        if (!caster.IsAlly || caster.Control != BattleControl.Player || caster.Status is not (0 or 2))
            throw new BattleRuleException("aura-caster", "actor", true);
        var origin = center == actor ? destination : target.Position!;
        // Admitted live states have unique, in-map occupancy. No raw-memory overlap emulation.
        var actors = battle.Actors.ToDictionary(row => row.Actor);
        var targets = battle.Definition.Initialization!.SourceCombatants.OrderBy(row => row.Key)
            .Where(row => row.Key < 30).Select(row => actors[row.Value])
            .Where(row => row.Hp > 0 && row.Position is not null &&
                (spell.Targets == HealingTargets.AuraAllAllies || (row.ActivationWord & 8) == 0 &&
                    BattleRange.Contains(origin, row.Actor == actor ? destination : row.Position!, 0, spell.Radius)))
            .ToArray();
        return targets;
    }

    internal static void ValidateAura(BattleDefinition definition)
    {
        if (!definition.Spells.Values.Any(s => s.IsAura)) return;
        // bsc0B uses a signed delta; IncreaseAndClampWord also branches on the
        // sign of current HP + recovery. Keep this finite family in their common domain.
        if (definition.Deployments.Any(d => d.Definition.MaxHp is 0 or > 32767))
            throw new BattleRuleException("aura-hp-domain", "actors.maxHp", true);
        var slots = definition.Initialization?.SourceCombatants;
        var deployments = definition.Deployments.ToDictionary(d => d.Actor);
        if (slots is null || slots.Count != definition.Deployments.Count ||
            slots.Values.Distinct().Count() != slots.Count || slots.Any(row =>
                !deployments.TryGetValue(row.Value, out var d) ||
                    !(d.Faction == BattleFaction.Ally ? row.Key is >= 0 and < 30 : row.Key is >= 128 and < 160)))
            throw new BattleRuleException("aura-source-slots", "initialization.sourceCombatants", true);
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
            (d.Faction != BattleFaction.Ally || d.Control != BattleControl.Player || d.SourceAllyControl ||
                d.Definition.ClassRule != BattleClassRule.UnpromotedPriest)))
            throw new BattleRuleException("aura-caster", "placements", true);
    }
}
