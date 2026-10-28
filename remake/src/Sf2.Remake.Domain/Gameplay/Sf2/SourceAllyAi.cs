using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// StartAiControl's ally branch enters commandset6 directly. In particular, it does
// not clear checked regions, test activation/standby, promote orders or set path modes.
internal sealed class SourceAllyAi : IBattleDecisionRule
{
    public string Identity => "sf2-source-ally-physical";
    public void RequireDeployment(BattleDeploymentDefinition deployment)
    {
        if (!deployment.SourceAllyControl || deployment.Faction != BattleFaction.Ally)
            throw new BattleRuleException("source-ally-control", "placements.controlPolicy", true);
    }
    public void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input)
    {
        if (input.ActivationWord is null) throw new BattleRuleException("missing-activation-word", "actor.activation", true);
    }
    public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actorRef, IPhysicalActionRule physical)
    {
        var actor = battle.GetActor(actorRef);
        if (!actor.IsAlly || !actor.Deployment.SourceAllyControl || actor.Hp == 0 || actor.Position is null ||
            actor.Control != BattleControl.Automatic || !(actor.Status == 0 || battle.Definition.Poison && actor.Status == 2) ||
            battle.StartPolicy?.Difficulty is > 0)
            throw new BattleRuleException("source-ally-ai-state", "actor.ai", true);
        // Finite unarmed, no-item, no-spell commandset. Unsupported categories cannot
        // silently fall through as failed HEAL/item/attack commands.
        if (actor.SourceLoadout is not { } loadout || loadout.Items.Any(i => i != 127) ||
            loadout.Spells.Any(s => s != 63) || actor.Spells.Count != 0 ||
            actor.Definition.Physical is not { MinimumRange: 1, MaximumRange: 1 })
            throw new BattleRuleException("source-ally-action-categories", "actor.loadout", true);
        var slots = battle.Definition.Initialization?.SourceCombatants ??
            throw new BattleRuleException("source-ally-slot", "battle.sourceCombatants", true);
        if (!slots.Any(p => p.Key is >= 0 and < 30 && p.Value == actorRef))
            throw new BattleRuleException("source-ally-slot", "actor.sourceSlot", true);
        var targets = slots.Where(p => p.Key is >= 128 and < 160).OrderBy(p => p.Key)
            .Select(p => battle.GetActor(p.Value)).Where(a => !a.IsAlly && a.Hp > 0 && a.Position is not null).ToArray();
        if (targets.Any(a => a.ActivationWord is null || (a.ActivationWord.Value & 8) != 0))
            throw new BattleRuleException("source-occupancy-word", "actors.activationWord", true);
        var grid = BattleMovement.Grid(battle, actorRef);
        bool Occupied(MapPosition p) => battle.Actors.Any(a => a.Hp > 0 && a.Position == p);
        var candidates = targets.Select(target => (Target: target,
            Position: AiMovementRules.AttackPosition(grid, target.Position!, 1, Occupied)))
            .Where(c => c.Position is not null).ToArray();
        List<BattleEffect> effects = [new("ai-commandset6-ally", actorRef)];
        if (candidates.Length > 0)
        {
            _ = BattleActionRules.Invoke(physical, "ai-admission", () => physical.RequireAction(battle, actorRef, new(BattleActionKind.Physical)));
            uint thinking = battle.ThinkingSeed;
            var priorities = new byte[candidates.Length];
            var costs = candidates.Select(c => (byte)grid.CostAt(c.Position!)!.Value).ToArray();
            // Ally difficulty0 selects Script3, just like the admitted raw enemy helper.
            // The reachable list and RNG visits are in source slot and reverse slot order.
            for (int i = candidates.Length - 1; i >= 0; i--)
            {
                var candidate = candidates[i];
                int damage = BattleActionRules.Invoke(physical, "ai-estimate", () => physical.EstimateDamage(battle, actorRef, candidate.Target.Actor));
                var draw = BattleRandom.NextThinkingWord((ushort)(thinking >> 16), 3);
                uint after = ((uint)draw.After << 16) | (thinking & 65535);
                priorities[i] = PhysicalTargetRules.ScriptThree(costs[i], Math.Max(0, candidate.Target.Hp - damage), draw.Value);
                effects.Add(new("thinking-rng", actorRef, thinking, after, 3, draw.Value, candidate.Target.Actor));
                effects.Add(new("ai-candidate", actorRef, costs[i], priorities[i], Target: candidate.Target.Actor));
                thinking = after;
            }
            byte maximum = priorities.Max();
            var cohort = Enumerable.Range(0, candidates.Length).Reverse().Where(i => priorities[i] == maximum).ToArray();
            int selected = cohort[0], movement = -1;
            // Ally skips the enemy >=15 class-preference pass. The final signed byte
            // comparison chooses greatest movement, replacing equality in reverse order.
            foreach (int i in cohort)
                if (unchecked((sbyte)costs[i]) >= movement) { movement = unchecked((sbyte)costs[i]); selected = i; }
            var chosen = candidates[selected];
            effects.Add(new("ai-target", actorRef, maximum, Math.Min(15, (int)maximum), Target: chosen.Target.Actor));
            effects.Add(new("ai-command-attack1", actorRef, After: 0));
            // aiCommand_Attack writes LastTarget only for enemy attackers.
            return new(battle.With(thinkingSeed: thinking), effects.AsReadOnly(), chosen.Position!,
                BattleMovement.Route(actor.Position, BattleMovement.MoveString(grid, actor.Position, chosen.Position!,
                    battle.Definition.Width, battle.Definition.Height)), chosen.Target.Actor);
        }
        effects.Add(new("ai-command-attack1", actorRef, After: -1));
        effects.Add(new("ai-command-heal1", actorRef, After: -1));
        effects.Add(new("ai-command-support", actorRef, After: -1)); // explicit ally early return
        var costsMap = battle.Definition.Terrain.Select(tile => BattleTerrainRules.MovementCost(tile, actor.Definition.Mover)).ToArray();
        // MOVE1's final d0 is a cost, not the actor ID. Reuse only the admitted complete
        // 0..127 cost domain that bypasses its class pass; unreachable/128 stays Unsupported.
        var pursuit = AiMovementRules.Pursue(costsMap, grid, actor.Position, targets.Select(a => a.Position!).ToArray(),
            Occupied, battle.Definition.Width, battle.Definition.Height);
        effects.Add(new("ai-move-target", actorRef, pursuit.TargetCosts[pursuit.TargetIndex], pursuit.Cost,
            Target: targets[pursuit.TargetIndex].Actor));
        effects.Add(new("ai-command-move1", actorRef, After: 0));
        BattleMovement.RequireStop(battle, actorRef, pursuit.Destination, grid);
        return new(battle, effects.AsReadOnly(), pursuit.Destination, BattleMovement.Route(actor.Position, pursuit.MoveString));
    }
}
