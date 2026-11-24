using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// DETOX and its item callers share the packed status calculation.
internal static class PoisonCureRules
{
    internal const ushort Poison = 2;
    internal const ushort Stun = 1;
    internal const ushort PoisonAndStun = Poison | Stun;

    internal static ushort SpellMask(byte rank) => rank switch
    {
        1 => Poison,
        3 or 4 => PoisonAndStun | CurseRules.Mask,
        _ => PoisonAndStun,
    };

    internal static EngineBattleState PrepareIneffective(EngineBattleState battle, ActorRef actor,
        IReadOnlyList<BattleReaction> reactions, List<BattleEffect> effects)
    {
        uint seed = battle.MainSeed;
        foreach (var reaction in reactions.Where(row => row.Kind == BattleReactionKind.Ineffective))
        {
            var draw = BattleRandom.NextMain(seed, 8);
            effects.Add(new("rng-cure-ineffective", actor, seed, draw.After, 8, draw.Value));
            seed = draw.After;
        }
        return battle.With(mainSeed: seed);
    }
    internal static BattleReaction Reaction(ActorRef actor, BattleActorState target, string action, ushort mask = Poison)
    {
        ushort cleared = (ushort)(target.Status & mask);
        return new(actor, target.Actor, action, cleared != 0 ? BattleReactionKind.Status : BattleReactionKind.Ineffective,
            target.Hp, target.Hp, target.Mp, target.Mp,
            StatusWrite: cleared != 0 ? (ushort)(target.Status & ~cleared) : null, ClearedStatus: cleared);
    }
}
