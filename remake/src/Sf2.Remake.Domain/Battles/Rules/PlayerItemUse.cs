using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// Explicit source-default entry for scalar item behavior without a session.
internal static class PlayerItemUse
{
    internal static BattleActionResolution PrepareSourceDefault(EngineBattleState battle, ActorRef actor,
        MapPosition destination, int slot, ActorRef target) =>
        BattleActionRules.Prepare(new Sf2ItemAction(), battle, actor, destination,
            new(BattleActionKind.Item, ItemSlot: slot), target, new Sf2BattleProgressionRule());
}
