using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// support.asm chooses the last highest scoring reachable center; no RNG/LastTarget write.
internal static class EnemySupportDecision
{
    internal static BattleAutomaticAction? Resolve(EngineBattleState battle, ActorRef actorRef,
        IReadOnlyList<IBattleActionRule> actions, WeightedMovementGrid? selectedGrid = null)
    {
        var actor = battle.GetActor(actorRef); var key = new SpellRef("dispel", 1);
        if (actor.IsAlly || !actor.Spells.Contains(key)) return null;
        if (!battle.Definition.SupportSpells.TryGetValue(key, out var spell)) throw new BattleRuleException("spell-effect", "spell", true);
        spell.RequireSupported();
        if (actor.Mp < spell.MpCost) return null;
        var rule = actions.Single(r => r.Kind == BattleActionKind.SupportSpell);
        var action = new BattleActionRef(BattleActionKind.SupportSpell, key);
        var centers = BattleActionRules.Invoke(rule, "ai-targets", () => rule.QueryTargets(battle, actorRef, action)).ToHashSet();
        var grid = selectedGrid ?? BattleMovement.Grid(battle, actorRef);
        List<BattleEffect> effects = []; BattleActorState? chosen = null; MapPosition? station = null; int best = 1;
        foreach (var center in battle.Actors.Where(a => centers.Contains(a.Actor)).OrderBy(a => a.ProcessingOrder))
        {
            var position = AiMovementRules.AttackPosition(grid, center.Position!, spell.MinimumRange, spell.MaximumRange,
                p => battle.Actors.Any(a => a.Hp > 0 && a.Position == p));
            if (position is null) continue;
            var targets = Sf2SupportSpellAction.Targets(battle, actorRef, position, spell, center.Actor);
            int score = targets.Count(Contributes);
            effects.Add(new("ai-support-candidate", actorRef, After: score, Target: center.Actor));
            if (score < 2 || score < best) continue;
            best = score; chosen = center; station = position;
        }
        if (chosen is null) return null;
        effects.Add(new("ai-support-target", actorRef, After: best, Target: chosen.Actor));
        return new(battle, effects.AsReadOnly(), station!, BattleMovement.Route(actor.Position!,
            BattleMovement.MoveString(grid, actor.Position!, station!, battle.Definition.Width, battle.Definition.Height)),
            chosen.Actor, ChosenAction: action);

        bool Contributes(BattleActorState target)
        {
            // GetNextUsableAttackSpell asks only its first matching family, then
            // lowers that family's rank for MP. Healing's getter ignores MP.
            var first = target.Spells.Where(battle.Definition.AttackSpells.ContainsKey).Select(s => (SpellRef?)s).FirstOrDefault();
            return first is { } family && target.Spells.Any(s => s.Value == family.Value &&
                battle.Definition.AttackSpells.TryGetValue(s, out var attack) && target.Mp >= attack.MpCost) ||
                target.Spells.Any(battle.Definition.Spells.ContainsKey);
        }
    }
}
