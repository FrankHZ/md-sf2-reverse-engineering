using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Domain.Gameplay.Authored;

// An author-selected algorithm example; source growth and accounting remain independently owned.
internal sealed class AuthoredBattleProgressionRule : Sf2BattleProgressionRule
{
    public override string Identity => "authored-fixed-award";
    public override int Award(EngineBattleState battle, BattleActorState recipient, BattleActionKind kind,
        IReadOnlyList<BattleReaction> reactions, ref uint seed, List<BattleEffect> effects) => 7;
}
