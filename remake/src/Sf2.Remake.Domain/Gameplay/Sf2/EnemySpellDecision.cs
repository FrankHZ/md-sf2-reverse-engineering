using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Finite learned-spell AI for the existing enemy commands and temporary ally turns. Decisions stage only
// source AI state/RNG. The selected shared action rule admits and constructs the action.
internal static class EnemySpellDecision
{
    internal static bool AllyAttackFamily(SpellRef spell) =>
        spell.Value is "blaze" or "freeze" or "bolt" or "blast" or "katon" or "raijin";

    private sealed record RangedCandidates(BattleActionRef Action, string Label,
        (BattleActorState Target, MapPosition? Position)[] Reachable, PhysicalTargetPriority[] Priorities);

    internal static int? AllyAttackItemSlot(BattleDefinition definition, BattleActorState actor)
    {
        var items = actor.SourceLoadout?.Items;
        if (items is null) return null;
        for (int slot = 0; slot < items.Count; slot++)
        {
            ushort word = items[slot];
            byte id = (byte)(word & 127);
            if (id == 127) continue;
            // Admitted ally words have no AI-use bit. Retail's use-spell test
            // accepts NOTHING ($3f), so the first nonempty word ends this scan.
            return (word & 128) != 0 && definition.AttackItems.TryGetValue(id, out var attack) &&
                attack.Effect.Spell.Value is ("blaze" or "freeze" or "bolt" or "blast") ? slot : null;
        }
        return null;
    }

    internal static int? AllyRainSlot(BattleDefinition definition, BattleActorState actor)
    {
        if (!definition.HealingItems.TryGetValue(8, out var rain) || rain.Effect != HealingItemEffect.FullPartyHp)
            return null;
        var items = actor.SourceLoadout?.Items;
        if (items is not null)
            for (int slot = 0; slot < items.Count; slot++)
                if ((items[slot] & 127) == rain.ItemId) return slot;
        return null;
    }

    internal static EnemyPhysicalDecision.Search Attack(EngineBattleState battle, ActorRef actorRef,
        IPhysicalActionRule physical, IReadOnlyList<IBattleActionRule> actions, WeightedMovementGrid? selectedGrid = null,
        IReadOnlyList<BattleActorState>? orderedTargets = null)
    {
        var actor = battle.GetActor(actorRef);
        var side = BattleOffense.AutomaticSide(actor);
        var known = actor.Spells.FirstOrDefault(spell => actor.IsAlly || side == OffensiveTargetSide.OwnSide
            ? AllyAttackFamily(spell) : spell.Value is "blaze" or "freeze" or "blast" or "aqua");
        if (known != default && !battle.Definition.AttackSpells.ContainsKey(known))
            throw new BattleRuleException("spell-effect", "spell", true);
        var key = actor.Spells.Where(spell => spell.Value == known.Value)
            .OrderByDescending(spell => spell.Level)
            .FirstOrDefault(spell => battle.Definition.AttackSpells.TryGetValue(spell, out var entry) && entry.MpCost <= actor.Mp);
        int? swordSlot = actor.IsAlly ? null : TarosEffectiveness.SwordSlot(actor);
        if (swordSlot.HasValue && battle.Definition.AttackSpells.ContainsKey(TarosEffectiveness.SwordSpell))
            key = TarosEffectiveness.SwordSpell;
        else swordSlot = null;

        var grid = selectedGrid ?? BattleMovement.Grid(battle, actorRef);
        var ordered = orderedTargets ?? BattleOffense.OrderedRoster(battle, actor, side).ToArray();
        List<BattleEffect> effects = [];
        uint thinking = battle.ThinkingSeed;
        // The source scores in item, learned spell, physical order.
        RangedCandidates? item = null;
        if (BattleControlRules.UsesTemporaryAllyAi(actor) && AllyAttackItemSlot(battle.Definition, actor) is { } slot)
        {
            var effect = battle.Definition.AttackItems[(byte)(actor.SourceLoadout!.Items[slot] & 127)].Effect;
            item = Score(effect, new(BattleActionKind.Item, ItemSlot: slot), "item");
        }
        RangedCandidates? spell = null;
        if (key != default)
        {
            var effect = battle.Definition.AttackSpells[key];
            effect.RequireSupported(enemy: !actor.IsAlly);
            spell = Score(effect, new(BattleActionKind.AttackSpell, key, swordSlot), "spell");
        }
        var ranged = spell ?? item;
        var physicalSearch = EnemyPhysicalDecision.Score(battle.With(thinkingSeed: thinking), actorRef, physical, side, grid, ordered);
        if (ranged is null)
            return physicalSearch.Choice is { } onlyPhysical
                ? physicalSearch with { Choice = BattleOffense.Choose(battle, actorRef, onlyPhysical) } : physicalSearch;

        var prepared = physicalSearch.Battle;
        effects.AddRange(physicalSearch.Effects);
        if (item is not null && spell is not null)
        {
            // determinebattleaction @loc_15 uses the thinking channel after all scores.
            uint image = prepared.ThinkingSeed;
            var category = BattleRandom.NextThinkingWord((ushort)(image >> 16), 2);
            uint after = ((uint)category.After << 16) | (image & 65535);
            effects.Add(new("thinking-ai-attack-category", actorRef, image, after, category.Range, category.Value));
            prepared = prepared.With(thinkingSeed: after);
            ranged = category.Value == 1 ? item : spell;
        }
        else if (key is not { Value: "aqua", Level: 1 })
        {
            // AQUA1 alone bypasses the main-stream category draw.
            var category = BattleRandom.NextMain(battle.MainSeed, 6);
            effects.Add(new("rng-ai-attack-category", actorRef, category.Before, category.After, category.Range, category.Value));
            prepared = prepared.With(mainSeed: category.After);
            if (physicalSearch.HasReachable && !(item is not null || swordSlot.HasValue
                    ? category.Value is 3 or 5 : category.Value is 2 or 4))
                return Finish(physicalSearch.Choice);
        }
        var table = ranged.Action.ItemSlot.HasValue ? PhysicalTargetRules.Table(actor.Definition.Mover) : PhysicalPriorityTable.Mage;
        var result = PhysicalTargetRules.Select(ranged.Priorities, table,
            preferTargetClass: !actor.IsAlly, hasTargetClasses: side == OffensiveTargetSide.Opponents);
        if (result is not { } selection)
        {
            if (side == OffensiveTargetSide.Opponents) throw new BattleRuleException("ai-target-selection", "ai.spells", true);
            return Finish(null);
        }
        var chosen = ranged.Reachable[selection.Index];
        effects.Add(new($"ai-{ranged.Label}-target", actorRef, selection.RawPriority, selection.CappedPriority, Target: chosen.Target.Actor));
        return Finish(new(prepared, effects.AsReadOnly(), chosen.Position!, BattleMovement.Route(actor.Position!,
            BattleMovement.MoveString(grid, actor.Position!, chosen.Position!, battle.Definition.Width, battle.Definition.Height)),
            chosen.Target.Actor, ChosenAction: ranged.Action));

        EnemyPhysicalDecision.Search Finish(BattleAutomaticAction? selected) =>
            new(selected is null ? null : BattleOffense.Choose(battle, actorRef,
                selected with { Battle = prepared, Effects = effects.AsReadOnly() }),
                prepared, effects.AsReadOnly(), true);

        RangedCandidates? Score(AttackSpellDefinition effect, BattleActionRef action, string label)
        {
            var rule = actions.Single(r => r.Kind == action.Kind);
            var targets = BattleActionRules.Invoke(rule, "ai-targets", () => rule.QueryTargets(battle, actorRef, action, side)).ToHashSet();
            var candidates = ordered.Where(a => targets.Contains(a.Actor))
                .Select(target => (Target: target, Position: AiMovementRules.AttackPosition(grid, target.Position!,
                    effect.MinimumRange, effect.MaximumRange, Occupied)))
                .Where(c => c.Position is not null).ToArray();
            if (candidates.Length == 0) return null;
            var priorities = new PhysicalTargetPriority[candidates.Length];
            for (int i = candidates.Length - 1; i >= 0; i--)
            {
                var candidate = candidates[i];
                int cost = grid.CostAt(candidate.Position!)!.Value;
                int total = 0;
                foreach (var affected in Sf2AttackSpellAction.Area(ordered, actor, actor.Position!,
                        candidate.Target.Position!, effect.Radius, side))
                    total += PhysicalTargetRules.Score(actor, affected, (byte)cost,
                        Sf2AttackSpellAction.PotentialDamage(actor, affected, effect, rawPower: true),
                        ref thinking, effects);
                byte priority = unchecked((byte)total);
                priorities[i] = new((byte)cost, priority, Sf2ClassRules.SourceClass(candidate.Target.Definition.ClassRule));
                effects.Add(new($"ai-{label}-candidate", actorRef, cost, priority, Target: candidate.Target.Actor));
            }
            return new(action, label, candidates, priorities);
        }

        bool Occupied(MapPosition position) => battle.Actors.Any(a => a.Hp > 0 && a.Position == position);
    }

    internal static BattleAutomaticAction? Heal(EngineBattleState battle, ActorRef actorRef,
        IReadOnlyList<IBattleActionRule> actions, WeightedMovementGrid? selectedGrid = null,
        IReadOnlyList<BattleActorState>? orderedTargets = null)
    {
        var actor = battle.GetActor(actorRef);
        int? rainSlot = BattleControlRules.UsesTemporaryAllyAi(actor) ? AllyRainSlot(battle.Definition, actor) : null;
        if (rainSlot is not null && battle.Definition.Initialization?.SourceCombatants.TryGetValue(128, out var first) == true)
        {
            var enemy = battle.GetActor(first);
            if (2 * enemy.Hp <= enemy.MaxHp) return Rain();
        }

        var key = actor.Spells.FirstOrDefault(s => s.Level == 1 &&
            (s.Value is "heal" or "aura" || actor.IsAlly && s.Value == "detox"));
        // The source healing command tries only HEAL/AURA. A preceding DETOX
        // entry reaches the item fallback without testing a later healing spell.
        if (key == default || key.Value == "detox") return RainFallback();
        if (!battle.Definition.Spells.TryGetValue(key, out var spell)) throw new BattleRuleException("spell-effect", "spell", true);
        if (spell.CuresPoison) throw new BattleRuleException("enemy-poison-cure", "spell", true);
        if (actor.Mp < spell.MpCost) return RainFallback();
        byte entryLevel = actor.Spells.Where(s => s.Value == key.Value).Max(s => s.Level);
        var rule = actions.Single(r => r.Kind == BattleActionKind.Healing);
        var allowed = BattleActionRules.Invoke(rule, "ai-targets", () =>
            rule.QueryTargets(battle, actorRef, new BattleActionRef(BattleActionKind.Healing, key))).ToHashSet();
        var grid = selectedGrid ?? BattleMovement.Grid(battle, actorRef);
        // Healing target selection consumes no RNG. Descending mover/critical-commandset
        // priority retains source slot order; unreachable casting positions are skipped.
        var ordered = orderedTargets ?? battle.Actors.OrderBy(a => a.ProcessingOrder).ToArray();
        var patients = ordered.Where(a => allowed.Contains(a.Actor) && a.Position is not null).ToArray();
        int TargetPriority(BattleActorState center) => !spell.IsAura ? Priority(center) : patients
            .Where(neighbor => BattleRange.Contains(center.Position!, neighbor.Position!, 0, spell.Radius))
            .Sum(neighbor => Priority(neighbor) + 4);
        foreach (var target in patients.Where(a => 3 * a.Hp <= 2 * a.MaxHp && a.MaxHp - a.Hp > 2)
            .OrderByDescending(TargetPriority))
        {
            byte? level = HealLevel(entryLevel, actor.Mp, target.MaxHp - target.Hp, target.Actor == actorRef);
            if (!level.HasValue) continue;
            // The source overwrites its local spell entry before the station check.
            entryLevel = level.Value;
            key = new SpellRef(key.Value, entryLevel);
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

        BattleAutomaticAction? RainFallback() =>
            rainSlot.HasValue && 3 * actor.Hp <= 2 * actor.MaxHp ? Rain() : null;

        BattleAutomaticAction Rain() => new(battle, [new("ai-rain-target", actorRef, Target: actorRef)],
            actor.Position!, [actor.Position!], actorRef,
            ChosenAction: new(BattleActionKind.Item, ItemSlot: rainSlot));
    }

    // DetermineHealingSpellLevel checks an unmasked packed entry with a five-bit shift.
    // The rank-2 trial reads HEAL4 (20 MP), then the missing ATLAS3 entry defaults to HEAL1 (3 MP).
    internal static byte? HealLevel(byte entryLevel, byte mp, int missingHp, bool self)
    {
        if (missingHp <= 2 || mp < 3) return null;
        if (entryLevel == 1) return 1;
        if (entryLevel == 2) return mp >= 5 ? (byte)1 : null;
        if (entryLevel == 4)
        {
            // The bad shift probes byte keys $20 (ATLAS1), $00 (HEAL1),
            // $E0 (absent -> HEAL1) and $C0 (HEAL4), in descending rank.
            if (missingHp > 28 && mp >= 14) return 4;
            if (missingHp >= 15) return 3;
            return mp < 20 ? null : self ? (byte)1 : (byte)2;
        }
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
