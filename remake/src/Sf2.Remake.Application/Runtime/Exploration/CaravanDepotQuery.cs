using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

public sealed record DepotAppraisal(byte Item, string Name, string Kind, uint? SaleValue,
    IReadOnlyList<ActorRef>? Eligible, bool WiderAppraisalUnavailable);

public static class CaravanDepotQuery
{
    public static string Name(ScenarioDefinition definition, byte item) => Terms(definition, item).Name;

    // Only admitted weapon eligibility is projected. Rings/special descriptions remain explicit gaps.
    public static DepotAppraisal Look(ScenarioDefinition definition, SessionSnapshot current, byte item)
    {
        var terms = Terms(definition, item);
        var source = definition.PrivateDefinitions?.Items.GetValueOrDefault(item);
        var members = PartyDetailsQuery.Query(definition, current)!.Members;
        bool weapon = members.Any(member => member.Deployment?.Definition.Equipment?.Weapons.ContainsKey(item) == true);
        var eligible = weapon ? members.Where(member => member.Deployment?.Definition.Equipment?.Weapons.ContainsKey(item) == true &&
                BattleEquipment.Choices(member.Deployment.Definition, FieldInventory.BaseAttack(member),
                    new BattleSourceLoadout([item, 127, 127, 127], [])).Any(choice => choice.Slot == 0))
            .Select(member => member.Carried.Actor).ToArray() : null;
        bool consumable = source?.ItemTypeExpression == "CONSUMABLE";
        return new(item, terms.Name, weapon ? "weapon" : consumable ? "consumable" : "other",
            source is null ? null : (uint)source.Price * 3 / 4, eligible, !weapon && !consumable);
    }

    internal static BattleDiscardItem Terms(ScenarioDefinition definition, byte item)
    {
        if (item < 127 && definition.PrivateDefinitions?.Items.TryGetValue(item, out var source) == true)
        {
            var flags = source.ItemTypeExpression.Split('|', StringSplitOptions.TrimEntries);
            return new(item, source.DisplayName, flags.Contains("RARE"), flags.Contains("UNSELLABLE"));
        }
        return definition.FieldPreparation?.Items.GetValueOrDefault(item)
            ?? throw new BattleRuleException("depot-item-unavailable", "depot.item", true);
    }

    internal static bool Cursed(ScenarioDefinition definition, ushort word) =>
        definition.PrivateDefinitions?.Items.GetValueOrDefault((byte)(word & 127))?.ItemTypeExpression
            .Split('|', StringSplitOptions.TrimEntries).Contains("CURSED") == true;
}
