using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class BattleTurnFlow
{
    internal static EngineBattleState Start(BattleDefinition definition, BattleStartInput start)
    {
        ValidateStart(definition, start);
        var inputs = start.Actors.ToDictionary(actor => actor.Actor);
        // Deployment order owns turn/candidate order; the external input array is only keyed data.
        var actors = definition.Deployments.Select(deployment =>
        {
            var input = inputs[deployment.Actor];
            return new BattleActorState(deployment.Definition, input.Hp, input.Mp, input.Exp,
                input.PositionOverride ?? deployment.Position, input.Kills, input.Defeats);
        });
        return new(definition, actors, start.MainSeed, start.ThinkingSeed, 0, [], 0, start.Gold);
    }

    // Used by the real Content admission and again by the reusable public session start boundary.
    internal static void ValidateStart(BattleDefinition definition, BattleStartInput start)
    {
        Require(start.Encounter == definition.Encounter, "missing-encounter", "start.encounter");
        Require(start.Gold <= 9999999, "numeric-range", "start.gold");
        var inputs = new Dictionary<ActorRef, BattleActorStartInput>();
        foreach (var input in start.Actors)
        {
            if (input is null) throw new BattleRuleException("missing-start-actor", "start.actors");
            Require(inputs.TryAdd(input.Actor, input), "duplicate-start-actor", "start.actors.actor");
            Require(definition.Deployments.Any(d => d.Actor == input.Actor), "missing-actor", "start.actors.actor");
        }
        Require(definition.Deployments.All(d => inputs.ContainsKey(d.Actor)), "missing-start-actor", "start.actors");
        var occupied = new HashSet<MapPosition>();
        int allies = 0, enemies = 0, turns = 0;
        foreach (var deployment in definition.Deployments)
        {
            var input = inputs[deployment.Actor]; var actor = deployment.Definition;
            Require(input.Hp <= actor.MaxHp && input.Mp <= actor.MaxMp && input.Exp <= 99 &&
                input.Kills <= 9999 && input.Defeats <= 9999, "numeric-range", "start.actors.resources");
            Require(input.Status == "none", "actor-status", "start.actors.status", true);
            var position = input.PositionOverride ?? deployment.Position;
            Require(definition.Contains(position), "start-placement-bounds", "start.actors.positionOverride");
            byte tile = definition.Terrain[position.Y * 48 + position.X];
            Require(tile < 16 && WeightedMovement.OrdinaryCosts[tile] > 0, "blocked-placement", "start.actors.positionOverride");
            if (input.Hp == 0)
            {
                Require(actor.Physical?.Leader != true, "leader-defeat-program", "start.actors", true);
                continue;
            }
            Require(occupied.Add(position), "occupied-placement", "start.actors");
            if (actor.IsAlly) allies++; else enemies++;
            turns += actor.Agility >= 128 ? 2 : 1;
        }
        Require(allies > 0 && enemies > 0, "battle-outcome", "start.actors", true);
        Require(turns <= 64, "turn-buffer-capacity", "start.actors");

        static void Require(bool condition, string code, string field, bool unsupported = false)
        { if (!condition) throw new BattleRuleException(code, field, unsupported); }
    }

    internal static EngineBattleState GenerateRound(EngineBattleState battle)
    {
        var generated = TurnOrderRules.Generate(battle.Actors.Select(a =>
            new TurnOrderCandidate(a.Definition.Slot, true, a.Hp, a.Definition.Agility)),
            (ushort)(battle.MainSeed >> 16));
        return battle.With(mainSeed: ((uint)generated.NextSeed << 16) | (battle.MainSeed & 0xFFFF),
            round: checked(battle.Round + 1), queue: generated.Slots, cursor: 0);
    }

    // The original combatant-byte sentinel starts a round; negative AGI entries behind it do not run.
    internal static bool AtRoundEnd(EngineBattleState battle) =>
        battle.Cursor >= battle.Queue.Count || battle.Queue[battle.Cursor].ActorSlot == 255;

    internal static BattleActorState QueuedActor(EngineBattleState battle) =>
        battle.Actors.Single(a => a.Definition.Slot == battle.Queue[battle.Cursor].ActorSlot);

    internal static EngineBattleState ConsumeEntry(EngineBattleState battle) =>
        battle.With(cursor: checked(battle.Cursor + 1));
}
