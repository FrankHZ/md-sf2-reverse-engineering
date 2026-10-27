using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Explicit authored source-slot binding. Direction 0/right, 1/up, 2/left, 3/down
// is independent of display facing. Null direction denotes an ordinary target.
public sealed record PrismCombatantBinding(byte Slot, byte? Direction);
public sealed class PrismLaserDefinition
{
    internal PrismLaserDefinition(IReadOnlyDictionary<ActorRef, PrismCombatantBinding> combatants) =>
        Combatants = new ReadOnlyDictionary<ActorRef, PrismCombatantBinding>(combatants.ToDictionary());
    public IReadOnlyDictionary<ActorRef, PrismCombatantBinding> Combatants { get; }
    internal void RequireDefinition(BattleDefinition battle)
    {
        bool valid = battle.Deployments.Count == Combatants.Count && Combatants.Values.Select(v => v.Slot).Distinct().Count() == Combatants.Count &&
            Combatants.Values.Any(v => v.Direction is not null) && battle.TarosEffectiveness is null &&
            battle.Deployments.All(d => Combatants.TryGetValue(d.Actor, out var binding) &&
                (d.Faction == BattleFaction.Ally ? binding.Slot < 30 : binding.Slot is >= 128 and <= 159) &&
                d.Definition.MaxHp is > 0 and < 32768 &&
                (binding.Direction is null ? d.AiStrategy?.Value != "prism-laser" :
                    binding.Direction <= 3 && d.Faction == BattleFaction.Enemy && d.Control == BattleControl.Automatic &&
                    d.AiStrategy?.Value == "prism-laser" && d.Initialization is null)) &&
            (battle.LeaderCleanup is null || battle.LeaderCleanup.Enemies.All(a =>
                Combatants[a].Slot == battle.LeaderCleanup.EnemySlots[a]));
        if (!valid) throw new BattleRuleException("prism-definition", "encounter.prismLaser", true);
    }
}

public sealed record PrismLaserShot(MapPosition Origin, byte Direction, ActorRef? Focus,
    IReadOnlyList<ActorRef> Targets);

internal sealed class PrismLaserDecision : IBattleDecisionRule
{
    public string Identity => "sf2-prism-laser";
    public void RequireDeployment(BattleDeploymentDefinition deployment)
    {
        // Selected authored binding declares ordinary enemy control and unconditional 15/15.
        // Non15 activation/standby, swarm and special control remain closed.
        if (deployment.Faction != BattleFaction.Enemy || deployment.Control != BattleControl.Automatic || deployment.Initialization is not null)
            throw new BattleRuleException("prism-control", "placements.initialization", true);
    }
    public void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input) { }
    public BattleAutomaticAction Decide(EngineBattleState battle, ActorRef actor, IPhysicalActionRule physical)
    {
        var flower = PrismLaser.RequireActor(battle, actor);
        var focus = PrismLaser.Targets(battle, actor, alliesOnly: true).FirstOrDefault();
        return new(battle, [], flower.Position!, [flower.Position!], focus?.Actor,
            ChosenAction: focus is null ? null : new(BattleActionKind.PrismLaser));
    }
}

internal sealed class PrismLaser : IBattleActionRule
{
    public string Identity => "sf2-prism-laser";
    public BattleActionKind Kind => BattleActionKind.PrismLaser;
    public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) => [];
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) => [];
    public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actor, BattleActionRef action)
    {
        if (action != new BattleActionRef(Kind)) throw new BattleRuleException("prism-action", "action", true);
        RequireActor(battle, actor); return new();
    }
    // Focus can be dead, departed or neutral now: construction must not redecide.
    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target)
    {
        RequireAction(battle, actor, action);
        var focus = battle.GetActor(target);
        if (!focus.IsAlly) throw new BattleRuleException("prism-focus", "target", true);
        return focus;
    }
    internal static BattleActorState RequireActor(EngineBattleState battle, ActorRef actor)
    {
        var binding = battle.Definition.PrismLaser ?? throw new BattleRuleException("prism-binding", "battle.prismLaser", true);
        binding.RequireDefinition(battle.Definition);
        var flower = battle.GetActor(actor);
        if (!binding.Combatants.TryGetValue(actor, out var source) || source.Direction is null || flower.IsAlly ||
            flower.Control != BattleControl.Automatic || flower.Hp == 0 || flower.Status != 0 ||
            flower.Position is not { X: > 0 and < 47, Y: > 0 and < 47 } ||
            flower.ActivationWord is not { } word || (word & ~1) != 0 ||
            battle.StartPolicy is { AllyAutoBattle: true } or { OpponentControl: true })
            throw new BattleRuleException("prism-actor-state", "actor", true);
        return flower;
    }
    internal static IReadOnlyList<BattleActorState> Targets(EngineBattleState battle, ActorRef actor, bool alliesOnly)
    {
        var flower = RequireActor(battle, actor);
        var binding = battle.Definition.PrismLaser!;
        var occupants = new Dictionary<MapPosition, BattleActorState>();
        foreach (var unit in battle.Actors)
        {
            if (unit.Hp == 0 || unit.Position is not { X: >= 0 and < 48, Y: >= 0 and < 48 } position) continue;
            if (unit.Hp > unit.MaxHp || unit.MaxHp >= 32768 || unit.Status != 0 || unit.ActivationWord is not { } word || (word & ~9) != 0)
                throw new BattleRuleException("prism-target-state", "actors", true);
            if ((word & 8) != 0) continue;
            // Source overwrites overlapping squares. This bounded admission rejects
            // that ambiguous authored state explicitly, including cross-faction overlap.
            if (!occupants.TryAdd(position, unit)) throw new BattleRuleException("prism-overlap", "actors.position", true);
        }
        int direction = binding.Combatants[actor].Direction!.Value;
        (int dx, int dy) = direction switch { 0 => (1, 0), 1 => (0, -1), 2 => (-1, 0), _ => (0, 1) };
        List<BattleActorState> targets = [];
        for (int x = flower.Position!.X + dx, y = flower.Position.Y + dy; x >= 0 && x < 48 && y >= 0 && y < 48; x += dx, y += dy)
            if (occupants.TryGetValue(new(x, y), out var target) && (!alliesOnly || target.IsAlly)) targets.Add(target);
        return alliesOnly ? targets.AsReadOnly() : Array.AsReadOnly(targets.OrderBy(t => binding.Combatants[t.Actor].Slot).ToArray());
    }
    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, IBattleProgressionRule progression)
    {
        RequireAction(battle, actor, action);
        if (destination != battle.GetActor(actor).Position) throw new BattleRuleException("prism-movement", "destination", true);
        var targets = Targets(battle, actor, alliesOnly: false);
        uint seed = battle.MainSeed;
        List<BattleReaction> reactions = [];
        List<BattleEffect> effects = [];
        foreach (var unit in targets)
        {
            int amount = 16;
            for (int i = 0; i < 2; i++)
            {
                var draw = BattleRandom.NextMain(seed, 3); seed = draw.After; amount -= draw.Value;
                effects.Add(new("rng-prism-damage", actor, draw.Before, draw.After, 3, draw.Value, unit.Actor));
            }
            amount = Math.Max(1, amount);
            ushort hp = (ushort)Math.Max(0, unit.Hp - amount);
            if (hp == 0 && unit.IsAlly && unit.Defeats is null)
                throw new BattleRuleException("unspecified-defeats", "actor.defeats", true);
            reactions.Add(new(actor, unit.Actor, "prism-laser", BattleReactionKind.Damage,
                unit.Hp, hp, unit.Mp, unit.Mp, Amount: amount, StatusWrite: unit.Status));
        }
        return new(battle.With(mainSeed: seed), actor, destination, reactions.AsReadOnly(), null,
            effects.AsReadOnly(), [], Laser: new(destination, battle.Definition.PrismLaser!.Combatants[actor].Direction!.Value,
                target, Array.AsReadOnly(targets.Select(t => t.Actor).ToArray())));
    }
    internal static void Validate(BattleActionResolution result, EngineBattleState before, ActorRef actor,
        MapPosition destination, ActorRef? focus)
    {
        var targets = Targets(before, actor, alliesOnly: false);
        void Require(bool condition) { if (!condition) throw new InvalidOperationException("Invalid prepared Prism laser."); }
        Require(destination == before.GetActor(actor).Position && result.Laser is { } shot && shot.Origin == destination &&
            shot.Direction == before.Definition.PrismLaser!.Combatants[actor].Direction && shot.Focus == focus &&
            shot.Targets.SequenceEqual(targets.Select(t => t.Actor)) && result.Reactions.Count == targets.Count &&
            result.Reward is null && result.Item is null && result.Spell is null && result.Retreat is null &&
            result.CompletionEffects.Count == 0 && result.ConstructionEffects.Count == targets.Count * 2);
        uint seed = before.MainSeed;
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i]; int damage = 16;
            for (int j = 0; j < 2; j++)
            {
                var draw = BattleRandom.NextMain(seed, 3); seed = draw.After; damage -= draw.Value;
                Require(result.ConstructionEffects[2 * i + j] == new BattleEffect("rng-prism-damage", actor,
                    draw.Before, draw.After, 3, draw.Value, target.Actor));
            }
            Require(result.Reactions[i] == new BattleReaction(actor, target.Actor, "prism-laser", BattleReactionKind.Damage,
                target.Hp, (ushort)Math.Max(0, target.Hp - damage), target.Mp, target.Mp, Amount: damage, StatusWrite: target.Status));
        }
        Require(result.Prepared.MainSeed == seed);
    }
}
