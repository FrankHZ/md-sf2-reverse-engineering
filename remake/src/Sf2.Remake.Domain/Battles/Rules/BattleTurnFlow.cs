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
            return new BattleActorState(deployment, input.Hp, input.Mp, input.Exp,
                input.PositionOverride ?? deployment.Position, input.Kills, input.Defeats, status: input.Status);
        });
        var initial = new EngineBattleState(definition, actors, start.MainSeed, start.ThinkingSeed, 0, [], 0, start.Gold, start.NewBattle);
        return start.NewBattle is null ? initial : BattleInitializationRules.Initialize(initial);
    }

    // Encounter policy is validated at Content admission and reusable session start, even for dead actors.
    internal static void ValidateDeployment(BattleDeploymentDefinition deployment)
    {
        var actor = deployment.Definition;
        Require((deployment.Control, deployment.AiStrategy) is (BattleControl.Player, null) or
            (BattleControl.Automatic, BattleAiStrategy.Stay or BattleAiStrategy.AttackThenApproach or BattleAiStrategy.SourceOrders),
            "control-ai", "placements.control/aiStrategy", true);
        Require((deployment.Faction == BattleFaction.Ally) == (deployment.Control == BattleControl.Player),
            "control-side", "placements.faction/control", true);
        if (deployment.AiStrategy == BattleAiStrategy.SourceOrders)
            Require(deployment.Initialization is not null, "missing-source-orders", "placements.initialization");
        if (deployment.AiStrategy == BattleAiStrategy.AttackThenApproach)
        {
            Require(actor.Physical is not null, "physical-definition", "actors.physical", true);
            Require(actor.Spells.Count == 0, "ai-action-categories", "actors.spells", true);
            Require(actor.Move is >= 1 and <= 63, "ai-movement-domain", "actors.move", true);
        }
    }

    // Used by the real Content admission and again by the reusable public session start boundary.
    internal static void ValidateStart(BattleDefinition definition, BattleStartInput start)
    {
        Require(start.Encounter == definition.Encounter, "missing-encounter", "start.encounter");
        Require(start.Gold is null or <= 9999999, "numeric-range", "start.gold");
        if (start.NewBattle is null) Require(start.Gold is not null, "missing-accounting", "start.gold");
        var inputs = new Dictionary<ActorRef, BattleActorStartInput>();
        foreach (var input in start.Actors)
        {
            if (input is null) throw new BattleRuleException("missing-start-actor", "start.actors");
            Require(inputs.TryAdd(input.Actor, input), "duplicate-start-actor", "start.actors.actor");
            Require(definition.Deployments.Any(d => d.Actor == input.Actor), "missing-actor", "start.actors.actor");
        }
        Require(definition.Deployments.All(d => inputs.ContainsKey(d.Actor)), "missing-start-actor", "start.actors");
        BattleInitializationRules.Validate(definition, start);
        var occupied = new HashSet<MapPosition>();
        int allies = 0, enemies = 0, turns = 0;
        foreach (var deployment in definition.Deployments)
        {
            ValidateDeployment(deployment);
            var input = inputs[deployment.Actor]; var actor = deployment.Definition;
            Require(input.Hp <= actor.MaxHp && input.Mp <= actor.MaxMp && (input.Exp is null || input.Exp <= (start.NewBattle is null ? 99 : 200)) &&
                (input.Kills is null or <= 9999) && (input.Defeats is null or <= 9999), "numeric-range", "start.actors.resources");
            Require(input.Status == 0, "actor-status", "start.actors.status", true);
            if (start.NewBattle is null) Require(input.Exp is not null && input.Kills is not null && input.Defeats is not null,
                "missing-accounting", "start.actors");
            var position = input.PositionOverride ?? deployment.Position;
            Require(definition.Contains(position), "start-placement-bounds", "start.actors.positionOverride");
            var tile = definition.Terrain[position.Y * 48 + position.X];
            Require(BattleTerrainRules.MovementCost(tile, actor.Mover) > 0, "blocked-placement", "start.actors.positionOverride");
            if (input.Hp == 0)
            {
                Require(actor.Physical?.Leader != true, "leader-defeat-program", "start.actors", true);
                continue;
            }
            Require(occupied.Add(position), "occupied-placement", "start.actors");
            if (deployment.Faction == BattleFaction.Ally) allies++; else enemies++;
            turns += actor.ExtraRoundAction ? 2 : 1;
        }
        Require(allies > 0 && enemies > 0, "battle-outcome", "start.actors", true);
        Require(turns <= 64, "turn-buffer-capacity", "start.actors");
    }

    private static void Require(bool condition, string code, string field, bool unsupported = false)
    { if (!condition) throw new BattleRuleException(code, field, unsupported); }

    internal static EngineBattleState GenerateRound(EngineBattleState battle)
    {
        if (battle.Definition.Initialization is not null) battle = BattleActivationRules.BeforeRound(battle);
        var generated = TurnOrderRules.Generate(battle.Actors.Select(a =>
            new TurnOrderCandidate<ActorRef>(a.Actor, a.ProcessingOrder, a.Position is not null, a.Hp, a.Definition.Agility, a.Definition.ExtraRoundAction)),
            (ushort)(battle.MainSeed >> 16));
        return battle.With(mainSeed: ((uint)generated.NextSeed << 16) | (battle.MainSeed & 0xFFFF),
            round: checked(battle.Round + 1), queue: generated.Slots, cursor: 0);
    }

    // An absent actor marks the source sentinel; negative AGI entries behind it do not run.
    internal static bool AtRoundEnd(EngineBattleState battle) =>
        battle.Cursor >= battle.Queue.Count || battle.Queue[battle.Cursor].Actor is null;

    internal static BattleActorState QueuedActor(EngineBattleState battle) =>
        battle.GetActor(battle.Queue[battle.Cursor].Actor!.Value);

    internal static EngineBattleState ConsumeEntry(EngineBattleState battle) =>
        battle.With(cursor: checked(battle.Cursor + 1));
}
