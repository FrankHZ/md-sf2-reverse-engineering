using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class FieldItemEvents
{
    // Item use tests the player's occupied tile. Search and Talk test a facing offset instead.
    internal static ExplorationEvent? Match(SessionSnapshot current, byte item)
    {
        var world = current.Exploration!;
        var player = world.PlayerEntity;
        return world.Definition.Events.FirstOrDefault(entry => entry.Kind == ExplorationEventKind.UseItem &&
            entry.Item == item && (entry.ItemFacing is null || entry.ItemFacing == player.Motion.Facing) &&
            MapEventDispatcher.Matches(entry, player.Position, world.Layout[player.Position.X, player.Position.Y], current.Story.Flags));
    }

    // GetItemInventoryLocation rebuilds the joined list, then scans each member backwards.
    // It does not restrict the lookup to the selected carrier, active force or living members.
    internal static (PartyMemberDetails Member, int Slot)? Find(ScenarioDefinition definition, SessionSnapshot current, byte item)
    {
        var members = PartyDetailsQuery.Query(definition, current)!.Members
            .OrderBy(member => member.Deployment?.Initialization?.AllyPartyMember ?? member.Deployment?.ProcessingOrder ?? int.MaxValue);
        foreach (var member in members)
        {
            var words = FieldInventory.Loadout(member).Items;
            for (int slot = words.Count - 1; slot >= 0; slot--)
                if ((words[slot] & 127) == item) return (member, slot);
        }
        return null;
    }

    internal static ActiveSessionState Remove(ScenarioDefinition definition, SessionSnapshot current, byte item)
    {
        if (Find(definition, current, item) is not { } found) return current.Active;
        var changed = FieldInventory.WithLoadout(found.Member, BattleInventory.Remove(FieldInventory.Loadout(found.Member), found.Slot));
        var field = current.Exploration!;
        return new ActiveExploration(field.WithParty(FieldInventory.Replace(field.Party, [changed])));
    }
}
