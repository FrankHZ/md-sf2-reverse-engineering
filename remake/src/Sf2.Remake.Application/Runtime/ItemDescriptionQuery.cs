using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Runtime;

public sealed record ItemDescriptionFacts(BattleWeaponDefinition? Weapon, RingStat? RingStat, byte RingBonus,
    bool HasMovementSound, bool? EquipEligible, bool CanBreakOnUse,
    FieldRecoveryEffect? FieldUse, HealingItemDefinition? Recovery, AttackItemDefinition? Attack,
    SupportItemDefinition? Support, bool Retreat);

// Read only. Consult the mounted effects for the current encounter and the resolved holder class.
public static class ItemDescriptionQuery
{
    public static ItemDescriptionFacts Read(ScenarioDefinition definition, SessionSnapshot snapshot, byte item,
        BattleActorDefinition? holder)
    {
        var field = snapshot.Exploration;
        var encounter = snapshot.Mode == SessionMode.Battle ? snapshot.Battle.Definition :
            field is null ? null : definition.Encounters.GetValueOrDefault(field.Party.Encounter);
        var equipment = holder?.Equipment;
        if (equipment?.ClassMask(item) is null)
            equipment = encounter?.Deployments.Select(row => row.Definition.Equipment)
                .FirstOrDefault(candidate => candidate?.ClassMask(item) is not null);
        var ring = equipment?.Rings.GetValueOrDefault(item);
        var weapon = equipment?.Weapons.GetValueOrDefault(item);
        return new(weapon, ring?.Stat, ring?.Bonus ?? 0,
            ring?.MovementSound is not null,
            holder?.Equipment?.ClassMask(item) is null ? null : BattleEquipment.IsEligible(holder, item),
            ring?.DamageOnUse == true || weapon?.DamageOnUse == true,
            field is null || snapshot.Mode != SessionMode.Exploration ? null : definition.FieldPreparation?.Effect(item, field.Map),
            encounter?.HealingItems.GetValueOrDefault(item), encounter?.AttackItems.GetValueOrDefault(item),
            encounter?.SupportItems.GetValueOrDefault(item), encounter?.RetreatItems.ContainsKey(item) == true);
    }
}
