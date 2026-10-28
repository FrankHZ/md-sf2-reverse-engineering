using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class BattleEquipmentReader
{
    internal static IReadOnlyList<BattleWeaponDefinition> Weapons(JsonElement root, bool connected)
    {
        if (!root.TryGetProperty("weapons", out _)) return [];
        Require(!connected, "equipment-standalone-only", "weapons", true);
        var weapons = new List<BattleWeaponDefinition>();
        foreach (var row in Array(root, "weapons"))
        {
            Object(row, "weapon", "id", "name", "type", "classMask", "cursed", "attackBonus", "minimumRange", "maximumRange");
            Require(Text(row, "type") == "weapon" && !Boolean(row, "cursed"), "weapon-effects", "weapons", true);
            uint mask = 0;
            Require(row.GetProperty("classMask").ValueKind == JsonValueKind.Number && row.GetProperty("classMask").TryGetUInt32(out mask) && mask != 0, "weapon-class-mask", "weapons.classMask");
            byte minimum = (byte)Number(row, "minimumRange", 1, 3);
            var weapon = new BattleWeaponDefinition((byte)Number(row, "id", 0, 126), Text(row, "name"), mask,
                (byte)Number(row, "attackBonus", 0, 200), minimum, (byte)Number(row, "maximumRange", minimum, 3));
            Require(weapons.All(existing => existing.ItemId != weapon.ItemId), "duplicate-weapon", "weapons.id");
            weapons.Add(weapon);
        }
        return weapons.AsReadOnly();
    }

    internal static BattleEquipmentDefinition? Actor(JsonElement actor, IReadOnlyList<BattleWeaponDefinition> weapons,
        IEnumerable<byte> declaredWeapons)
    {
        if (!actor.TryGetProperty("equipment", out var equipment)) return null;
        Object(equipment, "actor.equipment", "baseAttack");
        Require(weapons.Count > 0 && declaredWeapons.All(id => weapons.Any(w => w.ItemId == id)),
            "equipment-weapon-metadata", "actor.equipment", true);
        return new((byte)Number(equipment, "baseAttack", 0, 200), weapons);
    }

    internal static void Validate(BattleActorDefinition actor)
    {
        if (actor.Equipment is null) return;
        Require(actor.Physical is { Promoted: false }, "equipment-physical", "actor.physical", true);
        BattleEquipment.ValidateLoadout(actor, actor.SourceLoadout);
        Require(actor.Attack == Math.Min(200, actor.Equipment.BaseAttack + BattleEquipment.Contribution(actor, actor.SourceLoadout)),
            "equipment-initial-attack", "actor.attack");
    }
}
