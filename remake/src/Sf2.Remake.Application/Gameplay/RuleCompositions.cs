using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Gameplay.Authored;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Gameplay;

public static class RuleCompositions
{
    // Project authors select this factory, then rebuild/restart the ordinary host.
    public static SessionRules ForGame() => Sf2();
    public static SessionRules Sf2() => new("sf2", new Sf2HealingRule());
    public static SessionRules AuthoredHealingA() => new("authored-healing-a", new CappedHealingRule());
    public static SessionRules AuthoredHealingB() => new("authored-healing-b", new HalfMissingHealingRule());
    internal static IPhysicalActionRule SourcePhysical() => new Sf2PhysicalAction();
    internal static IBattleActionRule SourceItem() => new Sf2ItemAction();
    internal static IBattleActionRule SourceStay() => new Sf2StayAction();
}
