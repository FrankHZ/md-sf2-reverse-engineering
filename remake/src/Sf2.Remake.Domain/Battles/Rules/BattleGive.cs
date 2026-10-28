using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

public sealed class BattleGiveDefinition
{
    internal BattleGiveDefinition(IEnumerable<KeyValuePair<byte, string>> items) =>
        Items = new ReadOnlyDictionary<byte, string>(items.ToDictionary());
    public IReadOnlyDictionary<byte, string> Items { get; }
}

public sealed record BattleGiveItem(int Slot, ushort Word, string Name);
internal sealed record BattleGiveResult(EngineBattleState Battle, ActorRef Giver, ActorRef Recipient,
    BattleGiveItem Given, BattleGiveItem? Returned);

// Battle caller, not field Give: remove donor, optional remove recipient,
// optional add to donor, add to recipient. All work is temporary until publication.
public static class BattleGive
{
    internal static void RequireActor(EngineBattleState battle, BattleActorState actor, bool giver)
    {
        var definition = battle.Definition.Give ?? throw new BattleRuleException("give-unavailable", "battle.give", true);
        if (!actor.IsAlly || actor.Hp == 0 || actor.Position is null || !actor.Initialized ||
            !battle.Definition.Contains(actor.Position) || ((actor.ActivationWord ?? 0) & 8) != 0 ||
            giver && actor.Control != BattleControl.Player)
            throw new BattleRuleException("give-actor", "actor");
        if (actor.Status != 0) throw new BattleRuleException("give-status", "actor.status", true);
        if (actor.SourceLoadout is not { Items.Count: 4 } loadout)
            throw new BattleRuleException("give-loadout", "actor.sourceLoadout", true);
        foreach (ushort word in loadout.Items)
        {
            if (word == 127) continue;
            if (word > 255 || !definition.Items.ContainsKey((byte)(word & 127)))
                throw new BattleRuleException("give-item-unsupported", "actor.sourceLoadout", true);
            if ((word & 128) != 0 && actor.Definition.Equipment?.Weapons.ContainsKey((byte)(word & 127)) != true)
                throw new BattleRuleException("give-equipped-item", "actor.sourceLoadout", true);
        }
        if (actor.Definition.Equipment is not null)
        {
            BattleEquipment.ValidateLoadout(actor.Definition, loadout);
            if (actor.Attack != Math.Min(200, actor.BaseAttack + BattleEquipment.Contribution(actor.Definition, loadout)))
                throw new BattleRuleException("give-stats", "actor.attack");
        }
    }

    public static IReadOnlyList<BattleGiveItem> Items(EngineBattleState battle, ActorRef actor, bool giver = true)
    {
        var state = battle.GetActor(actor);
        RequireActor(battle, state, giver);
        return Array.AsReadOnly(state.SourceLoadout!.Items.Select((word, slot) => (word, slot))
            .Where(row => (row.word & 127) != 127)
            .Select(row => new BattleGiveItem(row.slot, row.word, battle.Definition.Give!.Items[(byte)(row.word & 127)])).ToArray());
    }

    internal static BattleActorState RequireTarget(EngineBattleState battle, ActorRef giver,
        MapPosition destination, ActorRef recipient)
    {
        RequireActor(battle, battle.GetActor(giver), true);
        BattleMovement.RequireStop(battle, giver, destination);
        var target = battle.Actors.SingleOrDefault(actor => actor.Actor == recipient)
            ?? throw new BattleRuleException("give-target-missing", "target");
        RequireActor(battle, target, false);
        if (recipient == giver || !BattleRange.Contains(destination, target.Position!, 1, 1))
            throw new BattleRuleException("give-target-range", "target");
        return target;
    }

    internal static BattleGiveItem RequireSlot(EngineBattleState battle, ActorRef actor, int slot, bool giver = true) =>
        Items(battle, actor, giver).SingleOrDefault(item => item.Slot == slot)
            ?? throw new BattleRuleException("give-item-slot", "item.slot");

    internal static BattleGiveResult Prepare(EngineBattleState battle, ActorRef giver, MapPosition destination,
        int slot, ushort expectedWord, ActorRef recipient, int? returnSlot, ushort? expectedReturnWord)
    {
        var target = RequireTarget(battle, giver, destination, recipient);
        var given = RequireSlot(battle, giver, slot);
        if (given.Word != expectedWord) throw new BattleRuleException("stale-give-item", "item.slot");
        bool full = target.SourceLoadout!.Items.All(word => (word & 127) != 127);
        if (full != returnSlot.HasValue) throw new BattleRuleException("give-capacity-changed", "target.items");
        var returned = returnSlot is { } selected ? RequireSlot(battle, recipient, selected, false) : null;
        if (returned?.Word != expectedReturnWord) throw new BattleRuleException("stale-give-return", "return.slot");
        var donor = Remove(battle.GetActor(giver), slot);
        if (returned is not null)
        {
            target = Remove(target, returned.Slot);
            donor = Add(donor, returned.Word);
        }
        target = Add(target, given.Word);
        donor = donor.With(position: destination);
        return new(battle.With(actors: battle.Actors.Select(actor => actor.Actor == giver ? donor :
            actor.Actor == recipient ? target : actor)), giver, recipient, given, returned);
    }

    private static BattleActorState Remove(BattleActorState actor, int slot)
    {
        var words = actor.SourceLoadout!.Items.ToList(); words.RemoveAt(slot); words.Add(127);
        var loadout = new BattleSourceLoadout(words, actor.SourceLoadout.Spells);
        return actor.With(sourceLoadout: loadout, attack: actor.Definition.Equipment is null ? actor.Attack :
            (byte)Math.Min(200, actor.BaseAttack + BattleEquipment.Contribution(actor.Definition, loadout)));
    }

    private static BattleActorState Add(BattleActorState actor, ushort word)
    {
        var words = actor.SourceLoadout!.Items.ToArray();
        int slot = Array.FindIndex(words, value => (value & 127) == 127);
        if (slot < 0) throw new BattleRuleException("give-full", "target.items");
        words[slot] = (ushort)(word & 0x17F); // AddItem retains identity/broken only; admission excludes broken.
        return actor.With(sourceLoadout: new(words, actor.SourceLoadout.Spells));
    }
}
