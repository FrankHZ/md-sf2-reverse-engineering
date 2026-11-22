using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

public sealed record SupportSpellDefinition(SpellRef Spell, byte MpCost, byte MinimumRange, byte MaximumRange, byte Radius)
{
    public bool TargetsTeammates => Spell.Value is "boost" or "attack";
    public bool AbsorbsMp => Spell.Value == "spoit";
    public bool ChangesStats => Spell.Value is "boost" or "slow" or "attack";

    internal void RequireSupported()
    {
        if ((Spell.Value, Spell.Level, MpCost, MinimumRange, MaximumRange, Radius) is not
            (("dispel", 1, 5, 1, 2, 1) or ("boost", 1, 2, 0, 1, 1) or
             ("attack", 1, 7, 1, 3, 0) or ("slow", 1, 3, 1, 1, 1) or ("slow", 2, 6, 1, 2, 2) or
             ("sleep", 1, 4, 1, 2, 1) or ("spoit", 1, 0, 1, 2, 0)))
            throw new BattleRuleException("support-spell-definition", "spells", true);
    }

    internal static BattleReaction[] MpTransfer(BattleActorState caster, BattleActorState target, int roll)
    {
        int drained = Math.Min(3 + roll, target.Mp);
        return
        [
            new(caster.Actor, target.Actor, "item-use", BattleReactionKind.Resource,
                target.Hp, target.Hp, target.Mp, (byte)(target.Mp - drained), Amount: drained),
            new(caster.Actor, caster.Actor, "item-use", BattleReactionKind.Resource,
                caster.Hp, caster.Hp, caster.Mp, (byte)Math.Min(caster.MaxMp, caster.Mp + drained), Amount: drained),
        ];
    }

    // The SLOW resistance gate precedes the word write. An active field then consumes
    // a second, always-failing effectiveness roll. BOOST skips the resistance gate.
    internal (ushort? ReactionWord, ushort? ConstructionWord) Resolve(BattleActorState target, Func<string, int> roll)
    {
        int resistance = target.Definition.Resistance >> 14;
        if (Spell.Value == "sleep")
            return (roll("rng-sleep") >= 5 + resistance ? (ushort)(target.Status | 0xC0) : null, null);
        if (!ChangesStats)
            return (roll("rng-dispel") >= (SilenceRules.HasSpells(target) ? 5 + resistance : 8)
                ? (ushort)(target.Status | SilenceRules.Mask) : null, null);
        if (!TargetsTeammates && roll("rng-slow") < (resistance == 0 ? 0 : 5 + resistance)) return (null, null);
        ushort mask = Spell.Value switch
        {
            "boost" => BattleStatCounters.Boost,
            "attack" => BattleStatCounters.Attack,
            _ => BattleStatCounters.Slow,
        };
        ushort word = (ushort)(target.Status | mask);
        if ((target.Status & mask) == 0) return (word, word);
        roll($"rng-{Spell.Value}-recast");
        return (null, word);
    }
}
