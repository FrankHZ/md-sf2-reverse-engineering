using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

public sealed record PartyInventorySlot(int Slot, string? Name, bool? Empty, bool Equipped, bool Broken);
public sealed record PartySpell(SpellRef Spell, string? Name);
public sealed record PartyMemberDetails(BattleActorStartInput Carried, BattleDeploymentDefinition? Deployment,
    ushort? MaxHp, byte? MaxMp, byte? Level, byte? Attack, byte? Defense, byte? Agility,
    IReadOnlyList<PartySpell>? Spells, IReadOnlyList<PartyInventorySlot> Inventory);
public sealed record PartyDetails(uint? Gold, IReadOnlyList<PartyMemberDetails> Members);

// A read of carried field state, never a battle initializer or a command. In particular,
// death/absence does not erase retained resources, and an absent loadout is not four empty slots.
internal static class PartyDetailsQuery
{
    internal static PartyDetails? Query(ScenarioDefinition definition, SessionSnapshot current)
    {
        if (current.Mode != SessionMode.Exploration || current.Exploration is not { } field) return null;
        var encounter = definition.Encounters[field.Party.Encounter];
        var membership = definition.Exploration?.PartyFlags is { } layout
            ? MapPartyMembership.Rebuild(current.Story.Flags, layout) : current.Story.PartyLists;
        var members = new List<PartyMemberDetails>();
        foreach (var carried in field.Party.Actors)
        {
            var deployment = encounter.Deployments.FirstOrDefault(row => row.Actor == carried.Actor) ??
                definition.Encounters.Values.SelectMany(row => row.Deployments)
                    .FirstOrDefault(row => row.Actor == carried.Actor && row.Faction == BattleFaction.Ally);
            if (deployment?.Faction == BattleFaction.Enemy) continue;
            if (membership is not null && (deployment?.Initialization?.AllyPartyMember is { } member
                ? !membership.Joined.Contains(member) : definition.PrivateDefinitions is not null)) continue;
            var actor = deployment?.Definition;
            var progress = carried.Progress;
            var loadout = carried.SourceLoadout ?? progress?.SourceLoadout ?? actor?.SourceLoadout;
            byte? attack = actor is null ? null : BattleEquipment.Attack(actor, progress, loadout);
            var spells = progress?.Spells ?? actor?.Spells;
            members.Add(new(carried, deployment, progress?.MaxHp ?? actor?.MaxHp,
                progress?.MaxMp ?? actor?.MaxMp, progress?.Level ?? actor?.Level, attack,
                progress?.Defense ?? actor?.Defense, progress?.Agility ?? actor?.Agility,
                spells?.Select(spell => new PartySpell(spell, SpellName(definition, spell))).ToArray(),
                Enumerable.Range(0, 4).Select(slot =>
                {
                    if (loadout is null || slot >= loadout.Items.Count) return new PartyInventorySlot(slot, null, null, false, false);
                    ushort word = loadout.Items[slot]; byte item = (byte)(word & 127);
                    return new PartyInventorySlot(slot, item == 127 ? null : ItemName(definition, encounter, actor, item),
                        item == 127, item != 127 && (word & 128) != 0, item != 127 && (word & 256) != 0);
                }).ToArray()));
        }
        return new(field.Party.Gold, members.AsReadOnly());
    }

    private static string? SpellName(ScenarioDefinition definition, SpellRef spell) =>
        definition.PrivateDefinitions is { } source ? source.Spells.GetValueOrDefault(spell)?.DisplayName :
        $"{spell.Value.Replace('-', ' ').Replace('_', ' ').ToUpperInvariant()} {spell.Level}";

    private static string? ItemName(ScenarioDefinition definition, BattleDefinition encounter, BattleActorDefinition? actor, byte item) =>
        definition.PrivateDefinitions?.Items.GetValueOrDefault(item)?.DisplayName ??
        actor?.Equipment?.Weapons.GetValueOrDefault(item)?.Name ??
        encounter.HealingItems.GetValueOrDefault(item)?.Name ?? encounter.RetreatItems.GetValueOrDefault(item)?.Name ??
        encounter.ItemDrops?.Items.GetValueOrDefault(item)?.Name ?? encounter.Give?.Items.GetValueOrDefault(item) ??
        encounter.Discard?.Items.GetValueOrDefault(item)?.Name;
}
