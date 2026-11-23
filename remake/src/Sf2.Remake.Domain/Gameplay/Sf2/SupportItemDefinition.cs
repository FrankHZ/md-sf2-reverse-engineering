using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

public sealed record SupportItemDefinition(byte ItemId, string Name, SupportSpellDefinition Effect)
{
    internal void RequireOwner(EngineBattleState battle, BattleActorState owner)
    {
        Effect.RequireSupported();
        if (!(Effect.TargetsTeammates || Effect.AbsorbsMp))
            throw new BattleRuleException("equipment-use-class", "item.slot");
        EquipmentItemUse.RequireOwner(battle, owner, ItemId);
    }
}
