using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// Explicit source-default scalar entry for controlled comparisons without a session.
// Production callers use the session's selected IPhysicalActionRule.
internal static class PhysicalBattleAction
{
    internal static BattleActionResolution PrepareSourceDefault(EngineBattleState battle, ActorRef actor,
        MapPosition destination, ActorRef target) =>
        BattleActionRules.Prepare(new Sf2PhysicalAction(), battle, actor, destination,
            new(BattleActionKind.Physical), target, new Sf2BattleProgressionRule());

    internal static (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) ResolveSourceDefault(
        EngineBattleState battle, ActorRef actor, MapPosition destination, ActorRef target) =>
        Sf2PhysicalAction.Resolve(battle, actor, destination, target);
}
