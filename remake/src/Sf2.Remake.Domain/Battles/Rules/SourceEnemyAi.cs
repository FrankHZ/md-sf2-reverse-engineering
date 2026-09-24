using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class SourceEnemyAi
{
    internal static BattleAutomaticAction Resolve(
        EngineBattleState current, ActorRef actorRef)
    {
        var actor = current.GetActor(actorRef);
        if (actor.IsAlly || actor.Position is null || actor.Hp == 0 || actor.Status != 0 ||
            actor.Deployment.Initialization is not { } source || current.Regions is null)
            throw new BattleRuleException("source-ai-state", "actor.ai", true);
        if (actor.ActivationWord is not { } word)
            throw new BattleRuleException("missing-activation-word", "actor.activation", true);
        if (source.PrimaryOrder is not { } primary || source.SecondaryOrder is not { } secondary)
            throw new BattleRuleException("source-ai-orders", "placements.orders", true);
        // The follow-order replacement and move-order programs require their own reached implementation.
        if (primary != 255 || secondary != 255)
            throw new BattleRuleException("source-move-order", "placements.orders", true);
        if (source.PrimaryRegion == 15 && source.SecondaryRegion == 15) word |= 1;
        var prepared = current.With(regions: new(current.Regions.Flags, 0), actors: current.Actors.Select(
            unit => unit.Actor == actorRef ? unit.With(activationWord: word) : unit));
        List<BattleEffect> effects = [new("regions-tested-cleared", actorRef, current.Regions.Tested, 0)];
        if (word != actor.ActivationWord) effects.Add(new("activation-word", actorRef, actor.ActivationWord, word));
        if ((word & 1) == 0)
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

        int commandset = (word >> 4) & 15;
        if (commandset is not (6 or 7)) throw new BattleRuleException("source-commandset", "actor.activationWord", true);
        if (actor.SourceLoadout is not { } loadout || loadout.Items.Any(item => item != 127) || loadout.Spells.Any(spell => spell != 63))
            throw new BattleRuleException("source-action-categories", "actor.loadout", true);
        if (commandset == 7) effects.Add(new("ai-command-move-order1", actorRef, After: -1));
        var targets = current.Actors.Where(unit => unit.Hp > 0 && unit.Position is not null && unit.Faction != actor.Faction)
            .OrderBy(unit => unit.ProcessingOrder).ToArray();
        // Active target/obstruction semantics require actual opposing words, never a guessed neutral bit.
        if (targets.Any(unit => unit.ActivationWord is null || (unit.ActivationWord.Value & 8) != 0))
            throw new BattleRuleException("source-occupancy-word", "actors.activationWord", true);
        var legal = BattleMovement.Grid(prepared, actorRef);
        bool Occupied(MapPosition position) => current.Actors.Any(unit => unit.Hp > 0 && unit.Position == position);
        if (EnemyPhysicalDecision.TryResolve(prepared, actorRef) is { } attack)
            return attack with { Effects = Array.AsReadOnly<BattleEffect>([
                .. effects, new("ai-command-attack1", actorRef, After: 0), .. attack.Effects]) };
        effects.Add(new("ai-command-attack1", actorRef, After: -1));
        effects.Add(new("ai-command-heal1", actorRef, After: -1));
        effects.Add(new("ai-command-support", actorRef, After: -1));
        var pursuit = AiMovementRules.Pursue(Costs(), legal, actor.Position, targets.Select(unit => unit.Position!).ToArray(),
            Occupied, current.Definition.Width, current.Definition.Height);
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
