using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

// useitem / ExecuteBattleaction_AngelWing / RemoveAndArrangeItems.
// The held slot supplies the effect and determines payment or equipment wear.
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
                ? item.Name : battle.Definition.RetreatItems.TryGetValue(id, out var wing) ? wing.Name :
                battle.Definition.AttackItems.TryGetValue(id, out var attack) ? attack.Name :
                battle.Definition.SupportItems.TryGetValue(id, out var support) ? support.Name : $"Item {id}";
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
        ushort word = items[slot];
        byte id = (byte)(word & 127);
        if (battle.Definition.RetreatItems.TryGetValue(id, out var wing))
            return new(0, 0, RetreatItem: wing);
        if (battle.Definition.AttackItems.TryGetValue(id, out var attack))
        {
            Sf2AttackSpellAction.RequireDamageActor(battle, owner);
            if (attack.Breakable)
            {
                attack.Effect.RequireSupported();
                EquipmentItemUse.RequireOwner(battle, owner, id);
            }
            else attack.Effect.RequireConsumableEffect();
            return new(attack.Effect.MinimumRange, attack.Effect.MaximumRange, AttackItem: attack);
        }
        if (battle.Definition.SupportItems.TryGetValue(id, out var support))
        {
            support.RequireOwner(battle, owner);
            return new(support.Effect.MinimumRange, support.Effect.MaximumRange, SupportItem: support);
        }
        var item = RequireItem(battle, actor, slot);
        return new(item.MinimumRange, item.MaximumRange, Item: item);
    }
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) =>
        RequireAction(battle, actor, action) switch
        {
            { AttackItem: not null } => Sf2AttackSpellAction.TargetChoices(battle, actor),
            { SupportItem: { } support } => Sf2SupportSpellAction.TargetChoices(battle, actor, support.Effect),
            { RetreatItem: not null } or { Item.Effect: HealingItemEffect.FullPartyHp } => [actor],
            _ => HealingTargetRules.QueryTargets(battle, actor),
        };
    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target)
    {
        var admission = RequireAction(battle, actor, action);
        if (admission.AttackItem is { } attack)
        {
            _ = Sf2AttackSpellAction.ResolveTargets(battle, actor, destination, attack.Effect, target);
            return battle.GetActor(target);
        }
        if (admission.SupportItem is { } support)
        {
            _ = Sf2SupportSpellAction.Targets(battle, actor, destination, support.Effect, target);
            return battle.GetActor(target);
        }
        if (admission.Item is { } item)
        {
            _ = HealingTargetRules.ResolveItem(battle, actor, destination, item, target);
            return battle.GetActor(target);
        }
        BattleMovement.RequireStop(battle, actor, destination);
        if (target != actor) throw new BattleRuleException("invalid-retreat-target", "target");
        return battle.GetActor(actor);
    }
    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, IBattleProgressionRule progression)
    {
        var admission = RequireAction(battle, actor, action);
        if (admission.AttackItem is { } attack)
            return Sf2AttackSpellAction.PrepareAttack(battle, actor, destination, attack.Effect,
                target, progression, action.ItemSlot, attack);
        if (admission.SupportItem is { } support)
            return Sf2SupportSpellAction.PrepareEffect(battle, actor, destination, support.Effect, target,
                progression, support, action.ItemSlot);
        if (admission.RetreatItem is { } wing)
        {
            _ = RequireTarget(battle, actor, destination, action,
                target ?? throw new BattleRuleException("invalid-retreat-target", "target"));
            var owner = battle.GetActor(actor);
            var items = owner.SourceLoadout!.Items.ToList();
            int slot = action.ItemSlot!.Value;
            ushort word = items[slot]; items.RemoveAt(slot); items.Add(127);
            var paid = battle.With(actors: battle.Actors.Select(a => a.Actor == actor
                ? a.With(position: destination, sourceLoadout: new(items, owner.SourceLoadout.Spells), refreshStats: true) : a));
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

    private static BattleActionResolution Prepare(
        EngineBattleState battle, ActorRef actorRef, MapPosition destination, int slot, ActorRef targetRef, IBattleProgressionRule progression)
    {
        BattleMovement.RequireStop(battle, actorRef, destination);
        var item = RequireItem(battle, actorRef, slot);
        var actor = battle.GetActor(actorRef);
        var targets = HealingTargetRules.ResolveItem(battle, actorRef, destination, item, targetRef);
        var reactions = targets.Select(target => Reaction(actorRef, item, target)).ToArray();
        List<BattleEffect> effects = [];
        var checkedBattle = item.CuresPoison ? PoisonCureRules.PrepareIneffective(battle, actorRef, reactions, effects) : battle;
        var items = actor.SourceLoadout!.Items.ToList();
        ushort consumed = items[slot];
        items.RemoveAt(slot);
        items.Add(127);
        var consumedActor = actor.With(position: destination, sourceLoadout: new(items, actor.SourceLoadout.Spells), refreshStats: true);
        var paid = checkedBattle.With(actors: checkedBattle.Actors.Select(a => a.Actor == actorRef ? consumedActor : a));
        effects.Add(new("item-consumed", actorRef, consumed, slot));
        var award = BattleProgressionRules.Award(progression, paid, consumedActor, BattleActionKind.Item, reactions);
        effects.AddRange(award.Effects);
        return new(paid.With(mainSeed: award.Seed), actorRef, destination, reactions,
            new(actorRef, award.Amount), effects.AsReadOnly(), [], item);
    }

    internal static BattleReaction Reaction(ActorRef actor, HealingItemDefinition item, BattleActorState target)
    {
        if (item.CuresPoison) return PoisonCureRules.Reaction(actor, target, "item-use");
        int missingHp = target.MaxHp - target.Hp;
        int hp = item.Effect switch
        {
            HealingItemEffect.FullHp or HealingItemEffect.FullPartyHp => missingHp,
            HealingItemEffect.RestoreMp => 0,
            _ => Math.Min(item.Power, missingHp),
        };
        int mp = item.Effect == HealingItemEffect.RestoreMp ? Math.Min(item.Power, target.MaxMp - target.Mp) : 0;
        return new(actor, target.Actor, "item-use", BattleReactionKind.Recovery,
            target.Hp, (ushort)(target.Hp + hp), target.Mp, (byte)(target.Mp + mp), Amount: hp + mp);
    }
}
