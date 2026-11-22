using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

public sealed record SupportItemDefinition(byte ItemId, string Name, SupportSpellDefinition Effect)
{
    internal void RequireOwner(EngineBattleState battle, BattleActorState owner)
    {
        Effect.RequireSupported();
        if (!(Effect.TargetsTeammates || Effect.AbsorbsMp) || !BattleEquipment.CanBeDamaged(owner.Definition, ItemId) ||
            !BattleEquipment.IsEligible(owner.Definition, ItemId))
            throw new BattleRuleException("equipment-use-class", "item.slot");
        BattleEquipment.ValidateLoadout(owner.Definition, owner.SourceLoadout);
        if (!owner.Initialized || (owner.ActivationWord & 8) == 8 ||
            !SilenceRules.Supported(owner.Status, battle.Definition.Poison))
            throw new BattleRuleException("equipment-use-actor", "actor");
        if (battle.Definition.Rewards is null || owner.Exp is null)
            throw new BattleRuleException("equipment-use-reward", "actor.exp", true);
    }
}

public enum EquipmentWear { None, Damaged, Destroyed }

internal sealed record SupportItemUse(SupportItemDefinition Definition, int Slot, ushort Word, EquipmentWear Wear)
{
    internal static (SupportItemUse Use, BattleActorState Owner, uint Seed) Apply(
        SupportItemDefinition item, int slot, BattleActorState owner, uint seed, List<BattleEffect> effects)
    {
        var loadout = owner.SourceLoadout!;
        ushort word = loadout.Items[slot];
        if ((word & BattleInventory.Broken) != 0)
        {
            effects.Add(new("item-destroyed", owner.Actor, word, slot));
            return (new(item, slot, word, EquipmentWear.Destroyed), BattleEquipment.RemoveItem(owner, slot), seed);
        }

        var draw = BattleRandom.NextMain(seed, 4);
        effects.Add(new("rng-item-wear", owner.Actor, draw.Before, draw.After, draw.Range, draw.Value));
        if (draw.Value != 0) return (new(item, slot, word, EquipmentWear.None), owner, draw.After);

        ushort damaged = (ushort)(word | BattleInventory.Broken);
        effects.Add(new("item-damaged", owner.Actor, word, damaged));
        return (new(item, slot, word, EquipmentWear.Damaged),
            owner.With(sourceLoadout: BattleInventory.SetWord(loadout, slot, damaged)), draw.After);
    }
}
