using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

public sealed record SupportSpellDefinition(SpellRef Spell, byte MpCost, byte MinimumRange, byte MaximumRange, byte Radius)
{
    internal void RequireSupported()
    {
        if (Spell is not { Value: "dispel", Level: 1 } || MpCost != 5 || MinimumRange != 1 || MaximumRange != 2 || Radius != 1)
            throw new BattleRuleException("support-spell-definition", "spells", true);
    }
}
