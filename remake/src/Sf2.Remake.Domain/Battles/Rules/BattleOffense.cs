using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal enum OffensiveTargetSide { Opponents, OwnSide }

internal static class BattleOffense
{
    internal static OffensiveTargetSide AutomaticSide(BattleActorState actor) =>
        MuddleRules.Confused(actor.Status) ? OffensiveTargetSide.OwnSide : OffensiveTargetSide.Opponents;

    internal static bool Matches(BattleActorState actor, BattleActorState target, OffensiveTargetSide side) =>
        (actor.Faction == target.Faction) == (side == OffensiveTargetSide.OwnSide);

    internal static IEnumerable<BattleActorState> OrderedRoster(EngineBattleState battle, BattleActorState actor,
        OffensiveTargetSide side) => side == OffensiveTargetSide.Opponents
        ? battle.Actors.OrderBy(unit => unit.ProcessingOrder)
        : (battle.Definition.Initialization?.SourceCombatants
            ?? throw new BattleRuleException("muddle-source-slots", "placements.sourceSlot", true))
            .Where(slot => actor.IsAlly ? slot.Key is >= 0 and < 30 : slot.Key is >= 128 and < 160)
            .OrderBy(slot => slot.Key).Select(slot => battle.GetActor(slot.Value));

    internal static bool OwnTarget(EngineBattleState battle, BattleActorState actor, ActorRef target) =>
        OrderedRoster(battle, actor, OffensiveTargetSide.OwnSide).Any(unit => unit.Actor == target &&
            Matches(actor, unit, OffensiveTargetSide.OwnSide) && unit.Hp > 0 && unit.Position is not null);

    internal static bool SelfPhysicalUsesOrigin(BattleActorState actor) =>
        BattleEquipment.EquippedWeaponItem(actor) is null &&
        (BattleEquipment.Physical(actor)?.MaximumRange == 1 ||
            actor.Definition.ClassRule == BattleClassRule.BrassGunner);

    internal static int FixedSlot(BattleActorState actor) => actor.IsAlly ? 0 : 128;

    internal static BattleActorState[] PursuitTargets(EngineBattleState battle, BattleActorState actor,
        BattleActorState[] ordinary)
    {
        if (AutomaticSide(actor) == OffensiveTargetSide.Opponents) return ordinary;
        var fixedTarget = FixedPursuitTarget(battle, actor)
            ?? throw new BattleRuleException("muddle-pursuit-slot", "placements.sourceSlot", true);
        return fixedTarget.Position is null ? [] : [fixedTarget];
    }

    internal static BattleActorState? FixedPursuitTarget(EngineBattleState battle, BattleActorState actor)
    {
        if (battle.Definition.Initialization?.SourceCombatants.TryGetValue(FixedSlot(actor), out var target) != true)
            return null;
        var unit = battle.GetActor(target);
        bool available = unit.Hp > 0 ? unit.Position is not null : unit is { Position: null, Status: 0 };
        return unit.Initialized && available && Matches(actor, unit, OffensiveTargetSide.OwnSide) ? unit : null;
    }

    internal static BattleAutomaticAction Choose(EngineBattleState before, ActorRef actorRef, BattleAutomaticAction decision)
    {
        var actor = before.GetActor(actorRef);
        var target = decision.Target!.Value;
        var side = AutomaticSide(actor);
        var prepared = decision.Battle;
        bool blocked = side == OffensiveTargetSide.OwnSide && IsProtected(before, actor, target);
        var effects = decision.Effects;
        if (side == OffensiveTargetSide.OwnSide && !blocked && actorRef == target)
        {
            var roll = GuardRoll(actorRef, prepared.MainSeed);
            blocked = roll.RandomValue == 1;
            prepared = prepared.With(mainSeed: checked((uint)roll.After!.Value));
            effects = Array.AsReadOnly<BattleEffect>([.. effects, roll]);
        }
        if (blocked)
            return decision with { Battle = prepared, Effects = effects, Side = side, Guarded = true,
                Destination = actor.Position!, Path = [actor.Position!] };

        if (!actor.IsAlly)
            prepared = prepared.With(actors: prepared.Actors.Select(unit => unit.Actor == actorRef
                ? unit.With(lastTarget: decision.ChosenAction?.ItemSlot is not null ? actorRef : target) : unit));
        return decision with { Battle = prepared, Effects = effects, Side = side };
    }

    internal static bool IsProtected(EngineBattleState battle, BattleActorState actor, ActorRef target) =>
        battle.Definition.Initialization?.SourceCombatants.TryGetValue(FixedSlot(actor), out var protectedActor) == true &&
        protectedActor == target;

    internal static BattleEffect GuardRoll(ActorRef actor, uint seed)
    {
        var draw = BattleRandom.NextMain(seed, 2);
        return new("rng-muddle-self-guard", actor, draw.Before, draw.After, draw.Range, draw.Value, actor);
    }

    internal static BattleActionResolution PrepareGuarded(EngineBattleState battle, ActorRef actorRef,
        IBattleProgressionRule progression)
    {
        var actor = battle.GetActor(actorRef);
        var draw = BattleRandom.NextMain(battle.MainSeed, 16);
        List<BattleEffect> effects = [new("rng-muddle-inaction-message", actorRef, draw.Before, draw.After, draw.Range, draw.Value)];
        var prepared = battle.With(mainSeed: draw.After);
        BattleActionReward? reward = null;
        if (actor.IsAlly)
        {
            var award = BattleProgressionRules.Award(progression, prepared, actor, BattleActionKind.Physical, []);
            prepared = prepared.With(mainSeed: award.Seed);
            reward = new(actorRef, award.Amount);
            effects.AddRange(award.Effects);
            BattleProgressionRules.Preflight(progression, actor, reward.Amount, award.Seed, battle.Definition.Poison);
        }
        return new(prepared, actorRef, actor.Position!, [], reward, effects.AsReadOnly(), [])
        { MuddleMessage = 322 + (draw.Value <= 9 ? draw.Value : 0), Side = OffensiveTargetSide.OwnSide };
    }

    internal static bool PaysGold(EngineBattleState battle, BattleReaction reaction) =>
        reaction.HpAfter == 0 && battle.GetActor(reaction.Actor).IsAlly && !battle.GetActor(reaction.Target).IsAlly;

    internal static void RequireDeathAccounting(EngineBattleState battle, BattleReaction reaction)
    {
        if (reaction.HpAfter != 0) return;
        var casualty = battle.GetActor(reaction.Target);
        if (casualty.IsAlly && casualty.Defeats is null)
            throw new BattleRuleException("unspecified-defeats", "target.defeats", true);
        if (PaysGold(battle, reaction) && battle.GetActor(reaction.Actor).Kills is null)
            throw new BattleRuleException("unspecified-kills", "actor.kills", true);
    }

    internal static bool Survives(BattleActorState actor, IReadOnlyList<BattleReaction> reactions) =>
        actor.Hp > 0 && !reactions.Any(r => r.Target == actor.Actor && r.HpAfter == 0);
}
