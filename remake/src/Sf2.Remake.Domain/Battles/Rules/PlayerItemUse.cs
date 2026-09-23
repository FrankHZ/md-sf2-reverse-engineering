using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// useitem / spellEffect_Heal / AdjustSpellPower / RemoveAndArrangeItems / GiveExpAndGold.
// This bounded item family has no MP cost, promotion scaling or equipment-break roll.
internal static class PlayerItemUse
{
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
        PlayerHealing.RequireTarget(battle, actor, destination,
            item.MinimumRange, item.MaximumRange, target);

    internal static BattleActionResolution Prepare(
        EngineBattleState battle, ActorRef actorRef, MapPosition destination, int slot, ActorRef targetRef)
    {
        BattleMovement.RequireStop(battle, actorRef, destination);
        var item = RequireItem(battle, actorRef, slot);
        var actor = battle.GetActor(actorRef);
        var target = RequireTarget(battle, actorRef, destination, item, targetRef);
        int recovery = Math.Min(item.Power, target.MaxHp - target.Hp);
        int accumulated = actor.Definition.ClassRule == BattleClassRule.UnpromotedPriest
            ? HealingRules.Experience(recovery, target.MaxHp) : 0;
        uint seed = battle.MainSeed;
        List<PhysicalRoll> rolls = [];
        // Same-side actions skip battle EXP halving, including the non-healer minimum.
        int award = BattleRewards.Award(accumulated, false, ref seed, rolls);
        List<BattleEffect> effects = [];
        var items = actor.SourceLoadout!.Items.ToList();
        ushort consumed = items[slot];
        items.RemoveAt(slot); items.Add(127);
        var consumedActor = actor.With(sourceLoadout: new(items, actor.SourceLoadout.Spells));
        effects.Add(new("item-consumed", actorRef, consumed, slot));
        effects.AddRange(rolls.Select(roll => new BattleEffect("rng-" + roll.Purpose, actorRef,
            roll.Before, roll.After, roll.Range, roll.Result)));
        uint validationSeed = seed;
        _ = BattleGrowthRules.Award(consumedActor, award, ref validationSeed, []);
        var prepared = battle.With(actors: battle.Actors.Select(a => a.Actor == actorRef
            ? consumedActor.With(position: destination) : a), mainSeed: seed);
        return new(prepared, actorRef, destination,
            [new(actorRef, targetRef, "item-use", BattleReactionKind.Recovery,
                target.Hp, (ushort)(target.Hp + recovery), target.Mp, target.Mp, Amount: recovery)],
            new(actorRef, award), effects.AsReadOnly(), [], item);
    }
}
