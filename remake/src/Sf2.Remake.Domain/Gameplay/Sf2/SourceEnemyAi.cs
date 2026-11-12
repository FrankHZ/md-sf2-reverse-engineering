using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed class SourceEnemyAi : IBattleDecisionRule
{
    public string Identity => "sf2-source-orders";
    public void RequireDeployment(BattleDeploymentDefinition deployment)
    {
        if (deployment.Initialization is null)
            throw new BattleRuleException("missing-source-orders", "placements.initialization");
    }
    public void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input)
    {

    }
    public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, IPhysicalActionRule physical) =>
        Resolve(battle, actor, physical, [physical]);
    public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, IPhysicalActionRule physical,
        IReadOnlyList<IBattleActionRule> actions) => Resolve(battle, actor, physical, actions);

    // Explicit source-default overload for standalone policy comparisons.
    internal static BattleAutomaticAction Resolve(EngineBattleState current, ActorRef actor) =>
        BattleDecisionRules.Decide(new SourceEnemyAi(), current, actor, new Sf2PhysicalAction(), new Sf2BattleProgressionRule());

    internal static BattleAutomaticAction Resolve(
        EngineBattleState current, ActorRef actorRef, IPhysicalActionRule physical, IReadOnlyList<IBattleActionRule>? actions = null)
    {
        actions ??= [physical];
        var actor = current.GetActor(actorRef);
        if (actor.IsAlly || actor.Position is null || actor.Hp == 0 ||
            !(SilenceRules.Supported(actor.Status, current.Definition.Poison) && (actor.Status & 0xC0) == 0) ||
            actor.Deployment.Initialization is not { } source || current.Regions is null)
            throw new BattleRuleException("source-ai-state", "actor.ai", true);
        if (actor.ActivationWord is not { } word)
            throw new BattleRuleException("missing-activation-word", "actor.activation", true);
        if (actor.PrimaryOrder is not { } primary || actor.SecondaryOrder is not { } secondary)
            throw new BattleRuleException("source-ai-orders", "placements.orders", true);
        bool unconditional = source.PrimaryRegion == 15 && source.SecondaryRegion == 15;
        var prepared = current.With(regions: new(current.Regions.Flags, 0), actors: current.Actors.Select(
            unit => unit.Actor == actorRef ? unit.With(activationWord: word) : unit));
        List<BattleEffect> effects = [new("regions-tested-cleared", actorRef, current.Regions.Tested, 0)];
        if (word != actor.ActivationWord) effects.Add(new("activation-word", actorRef, actor.ActivationWord, word));
        if (!unconditional && (word & 1) == 0)
        {
            var occupants = current.Actors.Where(unit => unit.Hp > 0 && unit.Position is not null)
                .Select(unit => new StandbyOccupant<ActorRef>(unit.Actor, unit.Position!, unit.ActivationWord)).ToArray();
            var decision = AiStandbyRules.Decide(actor.Position, actor.Deployment.Position, primary, secondary,
                source.PrimaryRegion, source.SecondaryRegion, (ushort)(current.ThinkingSeed >> 16), actor.AiMemory,
                () => WeightedMovement.Build(Costs(), actor.Position.Y * 48 + actor.Position.X, actor.Definition.Move * 2),
                occupants, current.Definition.Width, current.Definition.Height);
            foreach (var roll in decision.Rolls)
                effects.Add(new("thinking-rng", actorRef, Image(roll.Before), Image(roll.After), roll.Range, roll.Value));
            effects.Add(new("ai-memory", actorRef, actor.AiMemory, decision.MemoryAfter));
            effects.Add(new("source-standby", actorRef));
            BattleMovement.RequireStop(prepared, actorRef, decision.Destination, decision.Grid);
            prepared = prepared.With(thinkingSeed: Image(decision.SeedAfter), actors: prepared.Actors.Select(
                unit => unit.Actor == actorRef ? unit.With(aiMemory: decision.MemoryAfter) : unit));
            return new(prepared, effects.AsReadOnly(), decision.Destination, BattleMovement.Route(actor.Position, decision.MoveString));
        }

        // GetAiCommandset reads the dedicated combatant byte; the activation filler nibble
        // happens to match it in Battle01 but is zero in Battle02.
        int commandset = source.AiCommandset;
        if (commandset is not (2 or 6 or 7 or 8 or 9 or 14)) throw new BattleRuleException("source-commandset", "actor.activationWord", true);
        // Ordinary unequipped Herb0 cannot be selected: attack-item checks equipped/AI bits,
        // healing-item skips it with AI bit13 clear (Healing Rain has a separate source branch).
        // Neither path draws RNG. Admitted ATT-only weapons have no AI use action;
        // other items/enabled bits and spells outside the finite BLAZE1/HEAL1 family remain Unsupported.
        if (!BattleEquipment.OrdinaryAiLoadout(actor.Definition, actor.SourceLoadout, allowHerb: true, allowEarlySpells: true))
            throw new BattleRuleException("source-action-categories", "actor.loadout", true);
        var targets = current.Actors.Where(unit => unit.Hp > 0 && unit.Position is not null && unit.Faction != actor.Faction)
            .OrderBy(unit => unit.ProcessingOrder).ToArray();
        // Ordered commands use the same opposing-occupancy contract as ordinary attack/pursuit.
        if (targets.Any(unit => unit.ActivationWord is null || (unit.ActivationWord.Value & 8) != 0))
            throw new BattleRuleException("source-occupancy-word", "actors.activationWord", true);
        // StartAiControl promotes a dead follow target before every commandset,
        // including set6, which has no move-order command of its own.
        if (primary != 255 && (primary & 64) == 0)
        {
            if (!prepared.Definition.Initialization!.SourceCombatants.TryGetValue(primary, out var followed))
                throw new BattleRuleException("source-follow-binding", "placements.orders", true);
            if (prepared.GetActor(followed).Hp == 0)
            {
                effects.Add(new("ai-order-promoted", actorRef, primary, secondary));
                prepared = prepared.With(actors: prepared.Actors.Select(unit => unit.Actor == actorRef
                    ? unit.With(primaryOrder: secondary, secondaryOrder: 255) : unit));
                primary = secondary; secondary = 255;
            }
        }
        if (commandset is 2 or 9 or 14 || commandset == 7 && (primary != 255 || secondary != 255))
        {
            var ordered = new SourceMoveOrderAi(prepared, actorRef, physical, actions).Resolve(commandset);
            return ordered with { Effects = Array.AsReadOnly<BattleEffect>([.. effects, .. ordered.Effects]) };
        }
        if (commandset == 7) effects.Add(new("ai-command-move-order1", actorRef, After: -1));
        var legal = BattleMovement.Grid(prepared, actorRef);
        bool Occupied(MapPosition position) => current.Actors.Any(unit => unit.Hp > 0 && unit.Position == position);
        if (EnemySpellDecision.Attack(prepared, actorRef, physical, actions) is { } attack)
            return attack with { Effects = Array.AsReadOnly<BattleEffect>([
                .. effects, new("ai-command-attack1", actorRef, After: 0), .. attack.Effects]) };
        effects.Add(new("ai-command-attack1", actorRef, After: -1));
        if (EnemySpellDecision.Heal(prepared, actorRef, actions) is { } heal)
            return heal with { Effects = Array.AsReadOnly<BattleEffect>([
                .. effects, new("ai-command-heal1", actorRef, After: 0), .. heal.Effects]) };
        effects.Add(new("ai-command-heal1", actorRef, After: -1));
        if (EnemySupportDecision.Resolve(prepared, actorRef, actions) is { } support)
            return support with { Effects = Array.AsReadOnly<BattleEffect>([
                .. effects, new("ai-command-support", actorRef, After: 0), .. support.Effects]) };
        effects.Add(new("ai-command-support", actorRef, After: -1));
        if (commandset == 8 || targets.Length == 0)
            return new(prepared, Array.AsReadOnly<BattleEffect>([.. effects, new("ai-stay", actorRef)]), actor.Position!, [actor.Position!]);
        var costs = Costs();
        var scores = WeightedMovement.Build(costs, actor.Position.Y * 48 + actor.Position.X, 128);
        var pursuit = AiMovementRules.PursueOrdered(costs, scores, legal, actor.Position, targets,
            PhysicalTargetRules.Table(actor.Definition.Mover), Occupied, current.Definition.Width, current.Definition.Height);
        var targetRef = targets[pursuit.TargetIndex].Actor;
        effects.Add(new("ai-move-target", actorRef, pursuit.TargetCosts[pursuit.TargetIndex], pursuit.Cost, Target: targetRef));
        effects.Add(new(pursuit.Destination == actor.Position ? "ai-move-stay" : "ai-move", actorRef, Target: targetRef));
        effects.Add(new("ai-command-move1", actorRef, After: 0));
        BattleMovement.RequireStop(prepared, actorRef, pursuit.Destination, legal);
        return new(prepared, effects.AsReadOnly(), pursuit.Destination, BattleMovement.Route(actor.Position, pursuit.MoveString));

        sbyte[] Costs() => current.Definition.Terrain.Select(tile => BattleTerrainRules.MovementCost(tile, actor.Definition.Mover)).ToArray();
        uint Image(ushort seed) => ((uint)seed << 16) | (current.ThinkingSeed & 65535);
    }
}

internal sealed class StayBattleDecision : IBattleDecisionRule
{
    public string Identity => "sf2-stay";
    public void RequireDeployment(BattleDeploymentDefinition deployment) { }
    public void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input) { }
    public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, IPhysicalActionRule physical) =>
        new(battle, Array.AsReadOnly<BattleEffect>([new("ai-stay", actor)]),
            battle.GetActor(actor).Position!, Array.AsReadOnly([battle.GetActor(actor).Position!]), QueueOnly: true);
}
