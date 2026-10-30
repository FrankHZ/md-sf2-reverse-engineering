using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal static class PartyDetailsText
{
    internal static string Read(PartyMemberDetails member, string name)
    {
        var actor = member.Carried;
        string spells = member.Spells is null ? "Unavailable" : member.Spells.Count == 0 ? "None learned" :
            string.Join("\n", member.Spells.Select(spell => spell.Name ?? $"Name unavailable · Level {spell.Spell.Level}"));
        string inventory = string.Join("\n", member.Inventory.Select(slot => $"{slot.Slot + 1}. " +
            (slot.Empty == true ? "Empty" : slot.Empty is null ? "Inventory unavailable" : slot.Name ?? "Item name unavailable") +
            (slot.Equipped ? " · Equipped" : "") + (slot.Broken ? " · Broken" : "")));
        return $"{name}\n" + (actor.Absent ? "Absent from the field\n" : actor.Hp == 0 ? "Defeated\n" : "") +
            $"Level: {member.Level?.ToString() ?? "Unavailable"}    EXP: {actor.Exp?.ToString() ?? "Unavailable"}\n" +
            $"Attack: {member.Attack?.ToString() ?? "Unavailable"}    Defense: {member.Defense?.ToString() ?? "Unavailable"}    Agility: {member.Agility?.ToString() ?? "Unavailable"}\n" +
            (member.MaxHp is null || member.MaxMp is null ? "Maximum HP/MP unavailable\n" : "") +
            $"\nLearned magic\n{spells}\n\nInventory\n{inventory}";
    }
}
