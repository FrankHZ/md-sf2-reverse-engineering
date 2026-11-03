using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

public sealed record Search(bool FromMenu = true) : SessionCommand;
public enum FieldSearchKind { Empty, Description, Item, Gold, Full, Unavailable, Unhandled }
public sealed record FieldItemReceipt(MapId Map, MapPosition Position, int Flag, bool Chest);
public sealed record FieldSearchResult(FieldSearchKind Kind, IReadOnlyList<int> Texts,
    FieldItemReceipt? Receipt = null, ActorRef? Recipient = null, ushort? ItemWord = null,
    uint? Gold = null, string? Message = null, string? ItemName = null);
public sealed record FieldSearchWait(WaitToken Token, FieldSearchResult Result) : ProgramWait(Token);

internal static class FieldSearch
{
    internal static bool Available(SessionSnapshot current) => current.CanWaitAtInput &&
        current.Exploration is { Definition.Search: not null, Definition.InputProgram: null } &&
        current.Story.Warp is null && current.Story.ScriptWarp is null && !current.Story.BattleRequest;

    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current, Search command)
    {
        try
        {
            if (!Available(current)) throw new BattleRuleException("field-search-unavailable", "search");
            var world = current.Exploration!; var search = world.Definition.Search!;
            var target = current.Story.LogicalView is { } view
                ? world.AllEntities.SingleOrDefault(entity => entity.Slot == view.TargetSlot)
                : world.PlayerEntity;
            if (target is null) throw new BattleRuleException("search-view-target", "search");
            var offset = (target.Motion.Facing & 3) switch { 0 => (384, 0), 1 => (0, -384), 2 => (-384, 0), _ => (0, 384) };
            var position = new MapPosition((target.Motion.X + offset.Item1) / 384, (target.Motion.Y + offset.Item2) / 384);
            if (position.X is < 0 or >= 64 || position.Y is < 0 or >= 64)
                throw new BattleRuleException("search-bounds", "search");
            int marker = world.Layout[position.X, position.Y] & 0x3C00;
            bool chest = marker == 0x1800;
            int[]? prefix = marker switch { 0x1800 => [403], 0x2C00 => [404], 0x3000 => [405], 0x3400 => [427], 0x1C00 => [423], _ => null };
            if (prefix is null)
            {
                var description = search.Descriptions.FirstOrDefault(row => row.Position == position && (!row.MenuOnly || command.FromMenu));
                return Feedback(current, new(description is null ? command.FromMenu ? FieldSearchKind.Empty : FieldSearchKind.Unhandled : FieldSearchKind.Description,
                    description?.Texts ?? (command.FromMenu ? [423, 412] : [])));
            }
            var placement = (chest ? search.Chests : search.OtherItems).FirstOrDefault(row => row.Position == position);
            bool repeated = placement is not null && current.Story.Flags.Contains(placement.Flag);
            ushort? word = repeated ? (ushort)127 : placement?.ItemWord ?? (placement?.Gold is null ? (ushort)127 : null);
            var receipt = placement is null ? null : new FieldItemReceipt(world.Map, position, placement.Flag, chest);
            var flags = current.Story.Flags;
            var party = world.Party;
            FieldSearchResult result;
            if (word is { } item && (item & 127) != 127)
            {
                if (definition.FieldPreparation?.Items.TryGetValue((byte)(item & 127), out var itemDefinition) != true)
                    throw new BattleRuleException("search-item-content", "search.item", true);
                var members = PartyDetailsQuery.Query(definition, current)!.Members
                    .OrderBy(member => member.Deployment?.Initialization?.AllyPartyMember ?? member.Deployment?.ProcessingOrder ?? int.MaxValue).ToArray();
                PartyMemberDetails? recipient = null;
                foreach (var member in members)
                {
                    _ = FieldInventory.Member(definition, current, member.Carried.Actor);
                    _ = FieldInventory.Items(definition, member);
                    if (FieldInventory.Loadout(member).Items.Any(value => (value & 127) == 127)) { recipient = member; break; }
                }
                if (recipient is null)
                    return Feedback(current, new(FieldSearchKind.Full, prefix, receipt, ItemWord: item,
                        ItemName: itemDefinition!.Name));
                party = FieldInventory.Replace(party, [FieldInventory.WithLoadout(recipient,
                    BattleInventory.Add(FieldInventory.Loadout(recipient), item))]);
                result = new(FieldSearchKind.Item, prefix, receipt, recipient.Carried.Actor, item,
                    ItemName: itemDefinition!.Name);
            }
            else if (!repeated && placement?.Gold is { } gold)
            {
                if (party.Gold is null) throw new BattleRuleException("search-gold-unknown", "party.gold", true);
                party = FieldInventory.Replace(party, [], gold: (uint)Math.Min(999999UL, (ulong)party.Gold.Value + gold));
                result = new(FieldSearchKind.Gold, prefix, receipt, Gold: gold);
            }
            else result = new(FieldSearchKind.Empty, [.. prefix, marker switch { 0x1800 => 408, 0x2C00 => 409, 0x3000 => 410, _ => 412 }], receipt);
            if (placement is not null) flags = flags.Append(placement.Flag).Distinct().Order().ToArray();
            world = world.WithParty(party);
            if (chest && placement is not null) world = world.WithLayout(SetChest(world.Layout, position, search.OpenChest));
            // Publish grant, source flag and working block in the same authoritative revision.
            return Feedback(current, result, world, flags);
        }
        catch (BattleRuleException error)
        {
            return new(current, [], error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Rejected,
                new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.IllegalCommand,
                    error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }

    internal static SessionResult Feedback(SessionSnapshot current, FieldSearchResult result,
        ExplorationState? world = null, IReadOnlyList<int>? flags = null, IReadOnlyList<SessionObservation>? prior = null)
    {
        if (result.Kind == FieldSearchKind.Unhandled) return new(current, [], current.StopReason);
        var wait = new FieldSearchWait(new(current.ObservationSequence + 1), result);
        var next = new SessionSnapshot(current.SessionId, current.Revision + 1, current.ObservationSequence + 1,
            new ActiveExploration(world ?? current.Exploration!), current.Story.Copy(null, wait, flags: flags), SessionStopReason.PresentationWait);
        return new(next, [.. (prior ?? []), new(next.ObservationSequence, next.Revision, "field-search", Detail: result.Kind.ToString())], next.StopReason);
    }

    internal static WorkingMapLayout RestoreChests(WorkingMapLayout layout, FieldSearchDefinition? search, IReadOnlyList<int> flags)
    {
        foreach (var chest in search?.Chests ?? [])
            if (flags.Contains(chest.Flag)) layout = SetChest(layout, chest.Position, search!.OpenChest);
        return layout;
    }
    private static WorkingMapLayout SetChest(WorkingMapLayout layout, MapPosition position, ushort word)
    {
        var words = layout.Words.ToArray(); words[position.Y * 64 + position.X] = word;
        return new(words);
    }
}
