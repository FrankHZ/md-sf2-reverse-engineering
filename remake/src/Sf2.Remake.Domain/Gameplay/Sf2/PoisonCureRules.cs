using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// Finite DETOX1/Antidote effect. Selection, payment and scene publication retain their owners.
internal static class PoisonCureRules
{
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
    internal static BattleReaction Reaction(ActorRef actor, BattleActorState target, string action)
    {
        bool cured = (target.Status & 2) != 0;
        return new(actor, target.Actor, action, cured ? BattleReactionKind.Status : BattleReactionKind.Ineffective,
            target.Hp, target.Hp, target.Mp, target.Mp, StatusWrite: cured ? (ushort)(target.Status & ~2) : null);
    }
}
