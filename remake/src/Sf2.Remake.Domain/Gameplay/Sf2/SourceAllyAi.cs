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
        bool controlled = (temporary || actor.Deployment.SourceAllyControl && actor.Control == BattleControl.Automatic) &&
            SilenceRules.SupportedInBattle(actor.Status, battle.Definition.Poison);
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
        var side = BattleOffense.AutomaticSide(actor);
        var allies = slots.Where(p => p.Key is >= 0 and < 30).OrderBy(p => p.Key).Select(p => battle.GetActor(p.Value)).ToArray();
        var opponents = Opponents(battle, slots).Where(IsPlacedOpponent).ToArray();
        var targets = side == OffensiveTargetSide.OwnSide ? allies.Where(IsPlacedOpponent).ToArray() : opponents;
        if (targets.Any(a => !SupportsOpponentOccupancy(a)))
            throw new BattleRuleException("source-occupancy-word", "actors.activationWord", true);
        var grid = BattleMovement.Grid(battle, actorRef);
        bool Occupied(MapPosition p) => battle.Actors.Any(a => a.Hp > 0 && a.Position == p);
        List<BattleEffect> effects = [new("ai-commandset6-ally", actorRef)];
        if (temporary && actions.Count == 0) throw new BattleRuleException("source-ally-actions", "actor.ai", true);
        var search = temporary
            ? EnemySpellDecision.Attack(battle, actorRef, physical, actions, grid, targets)
            : EnemyPhysicalDecision.Score(battle, actorRef, physical, side, grid, targets);
        if (search.Choice is { } attack)
        {
            if (!temporary) attack = BattleOffense.Choose(battle, actorRef, attack);
            return attack with { Effects = Array.AsReadOnly<BattleEffect>([.. effects, .. attack.Effects,
                new("ai-command-attack1", actorRef, After: 0)]) };
        }
        battle = search.Battle;
        effects.AddRange(search.Effects);
        effects.Add(new("ai-command-attack1", actorRef, After: -1));
        if (temporary && side == OffensiveTargetSide.Opponents)
        {
            if (EnemySpellDecision.Heal(battle, actorRef, actions, grid, allies) is { } recovery)
                return recovery with { Effects = Array.AsReadOnly<BattleEffect>([.. effects,
                    new("ai-command-heal1", actorRef, After: 0), .. recovery.Effects]) };
        }
        effects.Add(new("ai-command-heal1", actorRef, After: -1));
        effects.Add(new("ai-command-support", actorRef, After: -1)); // source rejects ally casters
        var costsMap = battle.Definition.Terrain.Select(tile => BattleTerrainRules.MovementCost(tile, actor.Definition.Mover)).ToArray();
        var scores = WeightedMovement.Build(costsMap, actor.Position.Y * 48 + actor.Position.X, 128);
        targets = BattleOffense.PursuitTargets(battle, actor, opponents);
        var pursuit = AiMovementRules.PursueOrdered(costsMap, scores, grid, actor.Position, targets,
            PhysicalTargetRules.Table(actor.Definition.Mover), Occupied, battle.Definition.Width, battle.Definition.Height);
        effects.Add(new("ai-move-target", actorRef, pursuit.TargetCosts[pursuit.TargetIndex], pursuit.Cost,
            Target: targets[pursuit.TargetIndex].Actor));
        effects.Add(new("ai-command-move1", actorRef, After: 0));
        BattleMovement.RequireStop(battle, actorRef, pursuit.Destination, grid);
        return new(battle, effects.AsReadOnly(), pursuit.Destination, BattleMovement.Route(actor.Position, pursuit.MoveString));
    }

    internal static IEnumerable<BattleActorState> Opponents(EngineBattleState battle, IReadOnlyDictionary<int, ActorRef> slots) =>
        slots.Where(slot => slot.Key is >= 128 and < 160).OrderBy(slot => slot.Key)
            .Select(slot => battle.GetActor(slot.Value)).Where(actor => !actor.IsAlly);

    internal static bool IsPlacedOpponent(BattleActorState actor) => actor.Hp > 0 && actor.Position is not null;

    internal static bool SupportsOpponentOccupancy(BattleActorState actor) =>
        !IsPlacedOpponent(actor) || actor.ActivationWord is { } word && (word & 8) == 0;

    // Activation-driven ally control retains its existing physical capability.
    private static bool PhysicalOnly(BattleActorState actor, BattleSourceLoadout loadout) =>
        loadout.Spells.All(spell => spell == 63) && actor.Spells.Count == 0 &&
        actor.Definition.Physical is { MinimumRange: 1, MaximumRange: 1 } &&
        loadout.Items.All(word => word is 127 or 19 or 0 ||
            word <= 511 && actor.Definition.Equipment?.Rings.ContainsKey((byte)(word & 127)) == true &&
            ((word & BattleInventory.Broken) == 0 || BattleEquipment.CanBeDamaged(actor.Definition, (byte)(word & 127))));
}
