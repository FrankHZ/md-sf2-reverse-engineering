using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal enum BattleReactionKind { Damage, Dodge, Recovery, Resource }
internal sealed record BattleReaction(ActorRef Actor, ActorRef Target, string Action,
    BattleReactionKind Kind, ushort HpBefore, ushort HpAfter, byte MpBefore, byte MpAfter,
    bool Critical = false, int Amount = 0);
internal sealed record BattleActionReward(ActorRef Actor, int Amount);
internal sealed record BattleAutomaticAction(EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects,
    MapPosition Destination, BattleActionResolution? Scene = null);

// Construction and replay are separate source boundaries. Prepared state contains movement,
// construction RNG, gold and inventory; reactions and giveExp have not run yet.
internal sealed record BattleActionResolution(EngineBattleState Prepared, ActorRef Actor,
    MapPosition Destination, IReadOnlyList<BattleReaction> Reactions, BattleActionReward? Reward,
    IReadOnlyList<BattleEffect> ConstructionEffects, IReadOnlyList<BattleEffect> CompletionEffects,
    HealingItemDefinition? Item = null, HealingSpellDefinition? Spell = null)
{
    internal EngineBattleState ApplyReaction(EngineBattleState current, BattleReaction reaction) =>
        current.With(actors: current.Actors.Select(actor => actor.Actor == reaction.Target
            ? actor.With(hp: reaction.HpAfter, mp: Spell is null ? reaction.MpAfter : actor.Mp) : actor));

    internal EngineBattleState ApplySpellCost(EngineBattleState current) =>
        current.With(actors: current.Actors.Select(actor => actor.Actor == Actor
            ? actor.With(mp: checked((byte)(actor.Mp - Spell!.MpCost))) : actor));

    internal (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) ApplyReward(EngineBattleState current)
    {
        if (Reward is not { } reward) return (current, Array.Empty<BattleEffect>());
        uint seed = current.MainSeed;
        List<BattleEffect> effects = [];
        var actor = BattleGrowthRules.Award(current.GetActor(reward.Actor), reward.Amount, ref seed, effects);
        return (current.With(mainSeed: seed, actors: current.Actors.Select(row => row.Actor == actor.Actor ? actor : row)),
            effects.AsReadOnly());
    }

    internal (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) CreditReward(EngineBattleState current)
    {
        if (Reward is not { } reward) return (current, Array.Empty<BattleEffect>());
        List<BattleEffect> effects = [];
        var actor = BattleGrowthRules.Credit(current.GetActor(reward.Actor), reward.Amount, effects);
        return (current.With(actors: current.Actors.Select(row => row.Actor == actor.Actor ? actor : row)), effects.AsReadOnly());
    }

    internal (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) ApplyGrowth(EngineBattleState current)
    {
        if (Reward is not { } reward) return (current, Array.Empty<BattleEffect>());
        uint seed = current.MainSeed;
        List<BattleEffect> effects = [];
        var actor = BattleGrowthRules.Grow(current.GetActor(reward.Actor), ref seed, effects);
        return (current.With(mainSeed: seed, actors: current.Actors.Select(row => row.Actor == actor.Actor ? actor : row)), effects.AsReadOnly());
    }

    internal ActorRef FirstAlly => Prepared.GetActor(Reactions[0].Actor).IsAlly
        ? Reactions[0].Actor : Reactions[0].Target;
}
