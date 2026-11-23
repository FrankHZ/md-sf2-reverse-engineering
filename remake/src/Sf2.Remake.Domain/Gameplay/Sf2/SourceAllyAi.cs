using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// StartAiControl's ally branch enters commandset6 directly. In particular, it does
// not clear checked regions, test activation/standby, promote orders or set path modes.
internal sealed class SourceAllyAi : IBattleDecisionRule
{
    internal static readonly BattleStrategyRef Strategy = new("source-ally-physical");

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
    public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actorRef, IPhysicalActionRule physical) =>
        Decide(battle, actorRef, physical, []);

    public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actorRef, IPhysicalActionRule physical,
        IReadOnlyList<IBattleActionRule> actions)
    {
        var actor = battle.GetActor(actorRef);
        bool temporary = BattleControlRules.UsesTemporaryAllyAi(actor);
        bool controlled = temporary ? SilenceRules.SupportedInBattle(actor.Status, battle.Definition.Poison) :
            actor.Deployment.SourceAllyControl && actor.Control == BattleControl.Automatic &&
            SilenceRules.Supported(actor.Status, battle.Definition.Poison);
        if (!actor.IsAlly || !controlled || actor.Hp == 0 || actor.Position is null ||
            (actor.Status & 0xC1) != 0 || battle.StartPolicy?.Difficulty is > 0)
            throw new BattleRuleException("source-ally-ai-state", "actor.ai", true);

        if (actor.SourceLoadout is not { } loadout ||
            (temporary ? loadout.Items.Any(word => word > 511) : !PhysicalOnly(actor, loadout)))
            throw new BattleRuleException("source-ally-action-categories", "actor.loadout", true);
        if (actor.Definition.Equipment is not null) BattleEquipment.ValidateLoadout(actor.Definition, loadout);
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
        List<BattleEffect> effects = [new("ai-commandset6-ally", actorRef)];
        if (temporary && actions.Count == 0) throw new BattleRuleException("source-ally-actions", "actor.ai", true);
        var attack = temporary
            ? EnemySpellDecision.Attack(battle, actorRef, physical, actions, grid, targets)
            : EnemyPhysicalDecision.TryResolve(battle, actorRef, physical, grid, targets);
        if (attack is not null)
            return attack with { Effects = Array.AsReadOnly<BattleEffect>([.. effects, .. attack.Effects,
                new("ai-command-attack1", actorRef, After: 0)]) };
        effects.Add(new("ai-command-attack1", actorRef, After: -1));
        if (temporary)
        {
            var allies = slots.Where(p => p.Key is >= 0 and < 30).OrderBy(p => p.Key).Select(p => battle.GetActor(p.Value)).ToArray();
            if (EnemySpellDecision.Heal(battle, actorRef, actions, grid, allies) is { } recovery)
                return recovery with { Effects = Array.AsReadOnly<BattleEffect>([.. effects,
                    new("ai-command-heal1", actorRef, After: 0), .. recovery.Effects]) };
        }
        effects.Add(new("ai-command-heal1", actorRef, After: -1));
        effects.Add(new("ai-command-support", actorRef, After: -1)); // source rejects ally casters
        var costsMap = battle.Definition.Terrain.Select(tile => BattleTerrainRules.MovementCost(tile, actor.Definition.Mover)).ToArray();
        var scores = WeightedMovement.Build(costsMap, actor.Position.Y * 48 + actor.Position.X, 128);
        var pursuit = AiMovementRules.PursueOrdered(costsMap, scores, grid, actor.Position, targets,
            PhysicalTargetRules.Table(actor.Definition.Mover), Occupied, battle.Definition.Width, battle.Definition.Height);
        effects.Add(new("ai-move-target", actorRef, pursuit.TargetCosts[pursuit.TargetIndex], pursuit.Cost,
            Target: targets[pursuit.TargetIndex].Actor));
        effects.Add(new("ai-command-move1", actorRef, After: 0));
        BattleMovement.RequireStop(battle, actorRef, pursuit.Destination, grid);
        return new(battle, effects.AsReadOnly(), pursuit.Destination, BattleMovement.Route(actor.Position, pursuit.MoveString));
    }

    // Activation-driven ally control retains its existing physical capability.
    private static bool PhysicalOnly(BattleActorState actor, BattleSourceLoadout loadout) =>
        loadout.Spells.All(spell => spell == 63) && actor.Spells.Count == 0 &&
        actor.Definition.Physical is { MinimumRange: 1, MaximumRange: 1 } &&
        loadout.Items.All(word => word is 127 or 19 or 0 ||
            word <= 511 && actor.Definition.Equipment?.Rings.ContainsKey((byte)(word & 127)) == true &&
            ((word & BattleInventory.Broken) == 0 || BattleEquipment.CanBeDamaged(actor.Definition, (byte)(word & 127))));
}
