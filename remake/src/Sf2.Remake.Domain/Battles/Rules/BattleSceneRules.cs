namespace Sf2.Remake.Domain.Battles;

internal sealed record BattleReactionMotion(short X, short Y);
internal sealed record BattleReactionPlayback(EngineBattleState Battle,
    IReadOnlyList<BattleReactionMotion> Motion, IReadOnlyList<BattleEffect> Effects);

internal static class BattleSceneRules
{
    // bsc0A/bsc0B: a damaging reaction's DBF loop has twelve states. Each state
    // obtains horizontal and vertical offsets from the common main RNG. This is
    // an engine rule; renderer frame rate and reduced-flash settings are not inputs.
    internal static BattleReactionPlayback Reaction(EngineBattleState current, BattleReaction reaction)
    {
        if (reaction.Kind != BattleReactionKind.Damage)
            return new(current, Array.Empty<BattleReactionMotion>(), Array.Empty<BattleEffect>());
        bool ally = current.GetActor(reaction.Target).IsAlly;
        ushort range = ally ? (ushort)5 : (ushort)7;
        int origin = ally ? 2 : 3;
        uint seed = current.MainSeed;
        List<BattleReactionMotion> motion = [];
        List<BattleEffect> effects = [];
        for (int state = 0; state < 12; state++)
        {
            var horizontal = BattleRandom.NextMain(seed, range);
            var vertical = BattleRandom.NextMain(horizontal.After, range);
            motion.Add(new((short)((horizontal.Value - origin) * 2), (short)((vertical.Value - origin) * 2)));
            effects.Add(new("rng-reaction-x", reaction.Target, horizontal.Before, horizontal.After, range, horizontal.Value));
            effects.Add(new("rng-reaction-y", reaction.Target, vertical.Before, vertical.After, range, vertical.Value));
            seed = vertical.After;
        }
        return new(current.With(mainSeed: seed), motion.AsReadOnly(), effects.AsReadOnly());
    }
}
