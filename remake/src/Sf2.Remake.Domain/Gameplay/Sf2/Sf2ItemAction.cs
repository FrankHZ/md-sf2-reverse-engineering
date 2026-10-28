using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// useitem / ExecuteBattleaction_AngelWing / RemoveAndArrangeItems.
// This bounded item family has no MP cost, promotion scaling or equipment-break roll.
internal sealed class Sf2ItemAction : IBattleActionRule
{
    public string Identity => "sf2-item";
    public BattleActionKind Kind => BattleActionKind.Item;
    public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actorRef)
    {
        var items = battle.GetActor(actorRef).SourceLoadout?.Items;
        if (items is null) return [];
        return Array.AsReadOnly(items.Select((word, slot) =>
        {
            byte id = (byte)(word & 127);
            bool empty = id == 127;
            string label = empty ? "Empty" : battle.Definition.HealingItems.TryGetValue(id, out var item)
                ? item.Name : battle.Definition.RetreatItems.TryGetValue(id, out var wing) ? wing.Name : $"Item {id}";
            return new BattleActionOffer(new(Kind, ItemSlot: slot), label, empty);
        }).ToArray());
    }
    public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actor, BattleActionRef action)
    {
        if (action.Kind != Kind || action.Spell is not null || action.ItemSlot is not { } slot)
            throw new BattleRuleException("item-slot", "item.slot");
        var owner = battle.GetActor(actor);
        if (owner.Control != BattleControl.Player || !owner.IsAlly || owner.Hp == 0 ||
            owner.Position is null || (owner.Status & 0xC0) != 0)
            throw new BattleRuleException("not-player-control", "actor");
        var items = owner.SourceLoadout?.Items;
        if (items is null || slot < 0 || slot >= items.Count)
            throw new BattleRuleException("item-slot", "item.slot");
        if (battle.Definition.RetreatItems.TryGetValue((byte)(items[slot] & 127), out var wing))
            return new(0, 0, RetreatItem: wing);
        var item = RequireItem(battle, actor, slot);
        return new(item.MinimumRange, item.MaximumRange, Item: item);
    }
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) =>
        RequireAction(battle, actor, action).RetreatItem is not null ? [actor] : HealingTargetRules.QueryTargets(battle, actor);
    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target)
    {
        var admission = RequireAction(battle, actor, action);
        if (admission.RetreatItem is null) return RequireTarget(battle, actor, destination, admission.Item!, target);
        BattleMovement.RequireStop(battle, actor, destination);
        if (target != actor) throw new BattleRuleException("invalid-retreat-target", "target");
        return battle.GetActor(actor);
    }
    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, IBattleProgressionRule progression)
    {
        var admission = RequireAction(battle, actor, action);
        if (admission.RetreatItem is { } wing)
        {
            _ = RequireTarget(battle, actor, destination, action,
                target ?? throw new BattleRuleException("invalid-retreat-target", "target"));
            var owner = battle.GetActor(actor);
            var items = owner.SourceLoadout!.Items.ToList();
            int slot = action.ItemSlot!.Value;
            ushort word = items[slot]; items.RemoveAt(slot); items.Add(127);
            var paid = battle.With(actors: battle.Actors.Select(a => a.Actor == actor
                ? a.With(position: destination, sourceLoadout: new(items, owner.SourceLoadout.Spells)) : a));
            return new(paid, actor, destination, [], null, [new("item-consumed", actor, word, slot)], [],
                Retreat: new ItemRetreatPayment(wing.ItemId, wing.Name, slot, word));
        }
        return Prepare(battle, actor, destination, action.ItemSlot!.Value,
            target ?? throw new BattleRuleException("invalid-heal-target", "target"), progression);
    }

    internal static HealingItemDefinition RequireItem(EngineBattleState battle, ActorRef actorRef, int slot)
    {
        var actor = battle.GetActor(actorRef);
        if (!actor.IsAlly || actor.Hp == 0) throw new BattleRuleException("item-actor", "actor", true);
        var items = actor.SourceLoadout?.Items;
        if (items is null || slot < 0 || slot >= items.Count)
            throw new BattleRuleException("item-slot", "item.slot");
        byte id = (byte)(items[slot] & 127);
        if (id == 127) throw new BattleRuleException("empty-item-slot", "item.slot");
        if (!battle.Definition.HealingItems.TryGetValue(id, out var definition))
            throw new BattleRuleException("item-effect", "item", true);
        return definition;
    }

    internal static BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor,
        MapPosition destination, HealingItemDefinition item, ActorRef target) =>
        HealingTargetRules.RequireTarget(battle, actor, destination,
            item.MinimumRange, item.MaximumRange, target);

    internal static BattleActionResolution Prepare(
        EngineBattleState battle, ActorRef actorRef, MapPosition destination, int slot, ActorRef targetRef, IBattleProgressionRule progression)
    {
        BattleMovement.RequireStop(battle, actorRef, destination);
        var item = RequireItem(battle, actorRef, slot);
        var actor = battle.GetActor(actorRef);
        var target = RequireTarget(battle, actorRef, destination, item, targetRef);
        int recovery = Math.Min(item.Power, target.MaxHp - target.Hp);
        BattleReaction[] reactions = [new(actorRef, targetRef, "item-use", BattleReactionKind.Recovery,
            target.Hp, (ushort)(target.Hp + recovery), target.Mp, target.Mp, Amount: recovery)];
        var award = BattleProgressionRules.Award(progression, battle, actor, BattleActionKind.Item, reactions);
        List<BattleEffect> effects = [];
        var items = actor.SourceLoadout!.Items.ToList();
        ushort consumed = items[slot];
        items.RemoveAt(slot); items.Add(127);
        var consumedActor = actor.With(sourceLoadout: new(items, actor.SourceLoadout.Spells));
        effects.Add(new("item-consumed", actorRef, consumed, slot));
        effects.AddRange(award.Effects);
        var prepared = battle.With(actors: battle.Actors.Select(a => a.Actor == actorRef
            ? consumedActor.With(position: destination) : a), mainSeed: award.Seed);
        return new(prepared, actorRef, destination, reactions, new(actorRef, award.Amount), effects.AsReadOnly(), [], item);
    }
}
