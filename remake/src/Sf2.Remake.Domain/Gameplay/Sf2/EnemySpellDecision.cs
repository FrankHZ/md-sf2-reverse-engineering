using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Finite ordinary early payloads, not a general spell/item AI. Decisions stage only
// source AI state/RNG. The selected shared action rule admits and constructs the action.
internal static class EnemySpellDecision
{
    internal static BattleAutomaticAction? Attack(EngineBattleState battle, ActorRef actorRef,
        IPhysicalActionRule physical, IReadOnlyList<IBattleActionRule> actions, WeightedMovementGrid? selectedGrid = null)
    {
        var actor = battle.GetActor(actorRef);
        var known = actor.Spells.FirstOrDefault(spell => spell.Value is "blaze" or "freeze" or "blast" or "aqua");
        if (known != default && !battle.Definition.AttackSpells.ContainsKey(known))
            throw new BattleRuleException("spell-effect", "spell", true);
        var key = actor.Spells.Where(spell => spell.Value == known.Value)
            .OrderByDescending(spell => spell.Level)
            .FirstOrDefault(spell => battle.Definition.AttackSpells.TryGetValue(spell, out var entry) && entry.MpCost <= actor.Mp);
        int? swordSlot = TarosEffectiveness.SwordSlot(actor);
        if (swordSlot.HasValue && battle.Definition.AttackSpells.ContainsKey(TarosEffectiveness.SwordSpell))
            key = TarosEffectiveness.SwordSpell;
        else swordSlot = null;
        if (key == default)
            return EnemyPhysicalDecision.TryResolve(battle, actorRef, physical, selectedGrid);
        var spell = battle.Definition.AttackSpells[key];
        spell.RequireSupported(enemy: true);
        var rule = actions.Single(r => r.Kind == BattleActionKind.AttackSpell);
        var action = new BattleActionRef(BattleActionKind.AttackSpell, key, swordSlot);
        var targets = BattleActionRules.Invoke(rule, "ai-targets", () => rule.QueryTargets(battle, actorRef, action)).ToHashSet();
        var grid = selectedGrid ?? BattleMovement.Grid(battle, actorRef);
        var candidates = battle.Actors.Where(a => targets.Contains(a.Actor)).OrderBy(a => a.ProcessingOrder)
            .Select(target => (Target: target, Position: AiMovementRules.AttackPosition(grid, target.Position!,
                spell.MinimumRange, spell.MaximumRange, Occupied)))
            .Where(c => c.Position is not null).ToArray();
        if (candidates.Length == 0) return EnemyPhysicalDecision.TryResolve(battle, actorRef, physical, selectedGrid);
        var priorities = new PhysicalTargetPriority[candidates.Length];
        List<BattleEffect> effects = [];
        uint thinking = battle.ThinkingSeed;
        for (int i = candidates.Length - 1; i >= 0; i--)
        {
            var candidate = candidates[i];
            int cost = grid.CostAt(candidate.Position!)!.Value;
            int total = 0;
            foreach (var affected in battle.Actors.Where(unit => targets.Contains(unit.Actor) &&
                    BattleRange.Contains(candidate.Target.Position!, unit.Position!, 0, spell.Radius)).OrderBy(unit => unit.ProcessingOrder))
                total += PhysicalTargetRules.Score(actor, affected, (byte)cost,
                    Sf2AttackSpellAction.PotentialDamage(actor, affected, spell), ref thinking, effects);
            byte priority = unchecked((byte)total);
            priorities[i] = new((byte)cost, priority, Sf2ClassRules.SourceClass(candidate.Target.Definition.ClassRule));
            effects.Add(new("ai-spell-candidate", actorRef, cost, priority, Target: candidate.Target.Actor));
        }
        var prepared = battle.With(thinkingSeed: thinking);
        // Source scores spells before physical attacks, even when the category draw
        // later selects physical. Category selection uses MAIN, not thinking RNG.
        var attack = EnemyPhysicalDecision.TryResolve(prepared, actorRef, physical, selectedGrid);
        if (attack is not null) { effects.AddRange(attack.Effects); prepared = attack.Battle; }
        if (key.Value != "aqua")
        {
            var category = BattleRandom.NextMain(battle.MainSeed, 6);
            effects.Add(new("rng-ai-attack-category", actorRef, category.Before, category.After, 6, category.Value));
            prepared = prepared.With(mainSeed: category.After);
            if (attack is not null && !(swordSlot.HasValue ? category.Value is 3 or 5 : category.Value is 2 or 4))
                return attack with { Battle = prepared, Effects = effects.AsReadOnly() };
        }
        // determinebattleaction.asm @loc_34 overrides the mover table for CAST_SPELL.
        var selection = PhysicalTargetRules.Select(priorities, swordSlot.HasValue ? PhysicalTargetRules.Table(actor.Definition.Mover) : PhysicalPriorityTable.Mage)
            ?? throw new BattleRuleException("ai-target-selection", "ai.spells", true);
        var chosen = candidates[selection.Index];
        prepared = prepared.With(actors: prepared.Actors.Select(a => a.Actor == actorRef ? a.With(lastTarget: swordSlot.HasValue ? actorRef : chosen.Target.Actor) : a));
        effects.Add(new("ai-spell-target", actorRef, selection.RawPriority, selection.CappedPriority, Target: chosen.Target.Actor));
        return new(prepared, effects.AsReadOnly(), chosen.Position!, BattleMovement.Route(actor.Position!,
            BattleMovement.MoveString(grid, actor.Position!, chosen.Position!, battle.Definition.Width, battle.Definition.Height)),
            chosen.Target.Actor, ChosenAction: action);

        bool Occupied(MapPosition position) => battle.Actors.Any(a => a.Hp > 0 && a.Position == position);
    }

    internal static BattleAutomaticAction? Heal(EngineBattleState battle, ActorRef actorRef,
        IReadOnlyList<IBattleActionRule> actions, WeightedMovementGrid? selectedGrid = null)
    {
        var actor = battle.GetActor(actorRef); var key = new SpellRef("heal", 1);
        if (!actor.Spells.Contains(key)) return null;
        if (!battle.Definition.Spells.TryGetValue(key, out var spell)) throw new BattleRuleException("spell-effect", "spell", true);
        if (spell.CuresPoison) throw new BattleRuleException("enemy-poison-cure", "spell", true);
        if (actor.Mp < 3) return null;
        byte entryLevel = actor.Spells.Where(s => s.Value == "heal").Max(s => s.Level);
        var rule = actions.Single(r => r.Kind == BattleActionKind.Healing);
        var allowed = BattleActionRules.Invoke(rule, "ai-targets", () =>
            rule.QueryTargets(battle, actorRef, new BattleActionRef(BattleActionKind.Healing, key))).ToHashSet();
        var grid = selectedGrid ?? BattleMovement.Grid(battle, actorRef);
        // Healing target selection consumes no RNG. Descending mover/critical-commandset
        // priority retains source slot order; unreachable casting positions are skipped.
        foreach (var target in battle.Actors.Where(a => allowed.Contains(a.Actor) && a.Hp > 0 && a.Position is not null &&
            a.Faction == actor.Faction && 3 * a.Hp <= 2 * a.MaxHp && a.MaxHp - a.Hp > 2)
            .OrderByDescending(Priority).ThenBy(a => a.ProcessingOrder))
        {
            byte? level = HealLevel(entryLevel, actor.Mp, target.MaxHp - target.Hp, target.Actor == actorRef);
            if (!level.HasValue) continue;
            // The source overwrites its local spell entry before the station check.
            entryLevel = level.Value;
            key = new SpellRef("heal", entryLevel);
            if (!battle.Definition.Spells.TryGetValue(key, out spell))
                throw new BattleRuleException("spell-effect", "spell", true);
            var action = new BattleActionRef(BattleActionKind.Healing, key);
            var position = target.Actor == actorRef ? actor.Position : AiMovementRules.AttackPosition(grid, target.Position!,
                spell.MinimumRange, spell.MaximumRange, p => battle.Actors.Any(a => a.Hp > 0 && a.Position == p));
            if (position is null) continue;
            return new(battle, [new("ai-heal-target", actorRef, After: Priority(target), Target: target.Actor)], position,
                BattleMovement.Route(actor.Position!, BattleMovement.MoveString(grid, actor.Position!, position,
                    battle.Definition.Width, battle.Definition.Height)), target.Actor, ChosenAction: action);
        }
        return null;
    }

    // DetermineHealingSpellLevel checks an unmasked packed entry with a five-bit shift.
    // The rank-2 trial reads HEAL4 (20 MP), then the missing ATLAS3 entry defaults to HEAL1 (3 MP).
    internal static byte? HealLevel(byte entryLevel, byte mp, int missingHp, bool self)
    {
        if (missingHp <= 2 || mp < 3) return null;
        if (entryLevel == 1) return 1;
        if (entryLevel == 2) return mp >= 5 ? (byte)1 : null;
        if (entryLevel != 3) throw new BattleRuleException("enemy-heal-level", "spell.level", true);
        if (missingHp >= 15 && mp >= 20) return 3;
        if (missingHp < 15 && mp < 10) return null;
        return !self && mp >= 11 ? (byte)2 : (byte)1;
    }

    private static int Priority(BattleActorState target) => target.Deployment.Initialization?.AiCommandset is 13 or 14 ? 13 :
        target.Definition.Mover switch { BattleMover.Healer => 12, BattleMover.Mage => 11, BattleMover.CentaurArcher => 9, BattleMover.StealthArcher => 10, BattleMover.Archer => 8,
            BattleMover.Hovering => 7, BattleMover.Flying => 6, BattleMover.BrassGunner => 5, BattleMover.Stealth => 4,
            BattleMover.Centaur => 3, BattleMover.Regular => 2, _ => 0 };
}
