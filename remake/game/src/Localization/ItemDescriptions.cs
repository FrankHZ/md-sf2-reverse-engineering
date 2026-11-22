using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.GodotAdapter.Localization;

internal static class ItemDescriptions
{
    internal static string Read(GameSession session, byte? item, PartyMemberDetails? holder = null, bool broken = false) =>
        Read(session.Definition, session.Current, item, holder?.Deployment?.Definition, broken);

    internal static string Read(ScenarioDefinition definition, SessionSnapshot snapshot, byte? item,
        BattleActorDefinition? holder, bool broken)
    {
        if (item is null or 127) return "";
        var facts = ItemDescriptionQuery.Read(definition, snapshot, item.Value, holder);
        List<string> lines = [];
        if (facts.Weapon is { } weapon)
            lines.Add(GameText.Get("item-description.weapon", weapon.AttackBonus, weapon.MinimumRange, weapon.MaximumRange));
        if (facts.RingStat is { } stat)
            lines.Add(GameText.Get("item-description.equip." + stat.ToString().ToLowerInvariant(), facts.RingBonus));
        if (facts.HasMovementSound) lines.Add(GameText.Get("item-description.movement-sound"));
        if (facts.EquipEligible == false) lines.Add(GameText.Get("item-description.class"));

        string battle = BattleEffect(facts);
        if (battle.Length > 0)
            lines.Add(snapshot.Mode == SessionMode.Battle ? battle : GameText.Get("item-description.battle", battle));
        string field = FieldEffect(facts.FieldUse);
        if (field.Length > 0) lines.Add(GameText.Get("item-description.field", field));
        if (broken && facts.CanBreakOnUse && facts.Support is not null)
            lines.Add(GameText.Get("item-description.damaged"));
        return string.Join("\n", lines);
    }

    private static string BattleEffect(ItemDescriptionFacts facts)
    {
        if (facts.Recovery is { } recovery)
        {
            if (recovery.CuresPoison) return GameText.Get("item-description.poison");
            return recovery.Effect switch
            {
                HealingItemEffect.RestoreHp => GameText.Get("item-description.hp", recovery.Power),
                HealingItemEffect.FullHp => GameText.Get("item-description.full-hp"),
                HealingItemEffect.RestoreMp => GameText.Get("item-description.mp", recovery.Power),
                HealingItemEffect.FullPartyHp => GameText.Get("item-description.party-hp"),
                _ => "",
            };
        }
        if (facts.Attack is not null) return GameText.Get("item-description.damage");
        if (facts.Retreat) return GameText.Get("item-description.retreat");
        return facts.Support?.Effect.Spell.Value switch
        {
            "attack" => GameText.Get("item-description.raise-attack"),
            "boost" => GameText.Get("item-description.raise-defense-agility"),
            _ => "",
        };
    }

    private static string FieldEffect(FieldRecoveryEffect? effect) => effect switch
    {
        FieldRecoveryEffect.CurePoison => GameText.Get("item-description.poison"),
        FieldRecoveryEffect.CurePoisonAndParalysis => GameText.Get("item-description.poison-paralysis"),
        FieldRecoveryEffect.MapEvent => GameText.Get("item-description.location"),
        FieldRecoveryEffect.IncreaseAttack => GameText.Get("item-description.growth.attack"),
        FieldRecoveryEffect.IncreaseDefense => GameText.Get("item-description.growth.defense"),
        FieldRecoveryEffect.IncreaseAgility => GameText.Get("item-description.growth.agility"),
        FieldRecoveryEffect.IncreaseMove => GameText.Get("item-description.growth.move"),
        FieldRecoveryEffect.IncreaseHp => GameText.Get("item-description.growth.hp"),
        FieldRecoveryEffect.IncreaseMp => GameText.Get("item-description.growth.mp"),
        FieldRecoveryEffect.LevelUp => GameText.Get("item-description.level"),
        _ => "",
    };
}
