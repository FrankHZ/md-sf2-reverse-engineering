using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

public sealed record SupportSpellDefinition(SpellRef Spell, byte MpCost, byte MinimumRange, byte MaximumRange, byte Radius)
{
    public bool TargetsTeammates => Spell.Value is "boost" or "attack";
    public bool ChangesStats => Spell.Value is "boost" or "slow" or "attack";

    internal void RequireSupported()
    {
        if ((Spell.Value, Spell.Level, MpCost, MinimumRange, MaximumRange, Radius) is not
            (("dispel", 1, 5, 1, 2, 1) or ("boost", 1, 2, 0, 1, 1) or
             ("attack", 1, 7, 1, 3, 0) or ("slow", 1, 3, 1, 1, 1) or ("slow", 2, 6, 1, 2, 2)))
            throw new BattleRuleException("support-spell-definition", "spells", true);
    }

    // The SLOW resistance gate precedes the word write. An active field then consumes
    // a second, always-failing effectiveness roll. BOOST skips the resistance gate.
    internal bool Resolve(BattleActorState target, Func<string, int> roll, out ushort? constructionWord)
    {
        constructionWord = null;
        int resistance = target.Definition.Resistance >> 14;
        if (!ChangesStats) return roll("rng-dispel") >= (SilenceRules.HasSpells(target) ? 5 + resistance : 8);
        if (!TargetsTeammates && roll("rng-slow") < (resistance == 0 ? 0 : 5 + resistance)) return false;
        ushort mask = Spell.Value switch
        {
            "boost" => BattleStatCounters.Boost,
            "attack" => BattleStatCounters.Attack,
            _ => BattleStatCounters.Slow,
        };
        constructionWord = (ushort)(target.Status | mask);
        if ((target.Status & mask) == 0) return true;
        roll($"rng-{Spell.Value}-recast");
        return false;
    }
}
