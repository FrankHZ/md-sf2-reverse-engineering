using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal enum BattleReactionKind { Damage, Dodge, Recovery, Resource }
internal sealed record BattleReaction(ActorRef Actor, ActorRef Target, string Action,
    BattleReactionKind Kind, ushort HpBefore, ushort HpAfter, byte MpBefore, byte MpAfter,
    bool Critical = false, int Amount = 0);
internal sealed record BattleActionReward(ActorRef Actor, int Amount);
internal sealed record BattleAutomaticAction(EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects,
    MapPosition Destination, IReadOnlyList<MapPosition> Path, ActorRef? Target = null);

// Construction and replay are separate source boundaries. Prepared state contains movement,
// construction RNG, gold and inventory; reactions and giveExp have not run yet.
internal sealed record BattleActionResolution(EngineBattleState Prepared, ActorRef Actor,
    MapPosition Destination, IReadOnlyList<BattleReaction> Reactions, BattleActionReward? Reward,
    IReadOnlyList<BattleEffect> ConstructionEffects, IReadOnlyList<BattleEffect> CompletionEffects,
    HealingItemDefinition? Item = null, HealingSpellDefinition? Spell = null)
{
    internal void Validate(EngineBattleState before, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, BattleActionAdmission admission)
    {
        switch (action.Kind)
        {
            case BattleActionKind.Healing when action.Spell is { } spell && target is { } patient &&
                admission.Spell is { } definition && spell == definition.Spell && action.ItemSlot is null:
                ValidateHealing(before, actor, destination, definition, patient);
                break;
            case BattleActionKind.Physical when action.Spell is null && action.ItemSlot is null && target is { } victim:
                ValidatePhysical(before, actor, destination, victim);
                break;
            case BattleActionKind.Item when action.Spell is null && action.ItemSlot is { } slot &&
                target is { } recipient && admission.Item is { } item:
                ValidateItem(before, actor, destination, slot, item, recipient);
                break;
            case BattleActionKind.Stay when action == new BattleActionRef(BattleActionKind.Stay) && target is null:
                ValidateState(before, actor, destination);
                Require(Spell is null && Item is null && Reactions.Count == 0 && Reward is null &&
                    ConstructionEffects.Count == 0 && CompletionEffects.Count == 0 && Prepared.MainSeed == before.MainSeed);
                break;
            default: throw new InvalidOperationException("Invalid action reference or admission.");
        }
    }

    private static void Require(bool valid)
    { if (!valid) throw new InvalidOperationException("Invalid prepared action transition."); }

    private void ValidateState(EngineBattleState before, ActorRef actor, MapPosition destination,
        BattleSourceLoadout? consumedLoadout = null, bool allowGold = false)
    {
        BattleMovement.RequireStop(before, actor, destination);
        Require(Actor == actor && Destination == destination &&
            ReferenceEquals(Prepared.Definition, before.Definition) && Prepared.ThinkingSeed == before.ThinkingSeed &&
            Prepared.Round == before.Round && Prepared.Cursor == before.Cursor &&
            (allowGold || Prepared.Gold == before.Gold) && ReferenceEquals(Prepared.StartPolicy, before.StartPolicy) &&
            ReferenceEquals(Prepared.Regions, before.Regions) && Prepared.Queue.SequenceEqual(before.Queue) &&
            Prepared.Actors.Count == before.Actors.Count);
        for (int index = 0; index < before.Actors.Count; index++)
        {
            var old = before.Actors[index]; var candidate = Prepared.Actors[index];
            Require(ReferenceEquals(candidate.Deployment, old.Deployment) && candidate.Hp == old.Hp &&
                candidate.Mp == old.Mp && candidate.Exp == old.Exp && candidate.Kills == old.Kills && candidate.Defeats == old.Defeats &&
                candidate.LastTarget == old.LastTarget && candidate.Attack == old.Attack && candidate.Status == old.Status &&
                candidate.ActivationWord == old.ActivationWord && candidate.AiMemory == old.AiMemory &&
                Equals(candidate.Progress, old.Progress) && candidate.Position == (old.Actor == actor ? destination : old.Position));
            if (old.Actor == actor && consumedLoadout is not null)
                Require(candidate.SourceLoadout is { } loadout && loadout.Items.SequenceEqual(consumedLoadout.Items) &&
                    loadout.Spells.SequenceEqual(consumedLoadout.Spells));
            else Require(ReferenceEquals(candidate.SourceLoadout, old.SourceLoadout));
        }
    }

    private void ValidateRandom(EngineBattleState before, Func<BattleEffect, bool> allowed)
    {
        uint seed = before.MainSeed;
        foreach (var effect in ConstructionEffects)
        {
            Require(allowed(effect));
            if (!effect.Kind.StartsWith("rng-", StringComparison.Ordinal)) continue;
            Require(effect.Kind.Length > 4 && effect.RandomRange is not null);
            var draw = BattleRandom.NextMain(seed, effect.RandomRange!.Value);
            Require(effect.Before == seed && effect.After == draw.After && effect.RandomValue == draw.Value);
            seed = draw.After;
        }
        Require(Prepared.MainSeed == seed);
    }

    private void ValidateReward(EngineBattleState before, ActorRef? eligible)
    {
        Require(Reward is null || eligible is { } actor && Reward.Actor == actor && Reward.Amount is > 0 and <= 200);
        if (Reward is not { } reward) return;
        var recipient = before.GetActor(reward.Actor);
        if (recipient.Exp is null) throw new BattleRuleException("unspecified-exp", "actor.exp", true);
        uint seed = Prepared.MainSeed;
        _ = BattleGrowthRules.Award(recipient, reward.Amount, ref seed, []);
    }

    private void ValidatePhysical(EngineBattleState before, ActorRef actorRef, MapPosition destination, ActorRef targetRef)
    {
        var actor = before.GetActor(actorRef); var target = before.GetActor(targetRef);
        Require(actorRef != targetRef && actor.Faction != target.Faction && actor.Hp > 0 && target.Hp > 0 &&
            target.Position is not null && Spell is null && Item is null && Reactions.Count is > 0 and <= 3 && CompletionEffects.Count == 0);
        ushort actorHp = actor.Hp, targetHp = target.Hp;
        bool counter = false;
        for (int index = 0; index < Reactions.Count; index++)
        {
            var reaction = Reactions[index];
            bool reversed = reaction.Actor == targetRef && reaction.Target == actorRef;
            Require(!counter && (reversed ? index > 0 : index < 2 && reaction.Actor == actorRef && reaction.Target == targetRef));
            counter = reversed;
            var attacker = reversed ? target : actor; var defender = reversed ? actor : target;
            var profile = attacker.Definition.Physical ?? throw new BattleRuleException("physical-definition", "actor.physical", true);
            var from = reversed ? target.Position! : destination; var to = reversed ? destination : target.Position!;
            ushort hp = reversed ? actorHp : targetHp;
            Require(actorHp > 0 && targetHp > 0 && BattleRange.Contains(from, to, profile.MinimumRange, profile.MaximumRange) &&
                reaction.Action == (reversed ? "physical-counter" : index == 0 ? "physical-first" : "physical-second") &&
                reaction.Kind is BattleReactionKind.Damage or BattleReactionKind.Dodge && reaction.HpBefore == hp &&
                reaction.Amount >= 0 && reaction.HpAfter == Math.Max(0, hp - reaction.Amount) &&
                reaction.MpBefore == defender.Mp && reaction.MpAfter == defender.Mp &&
                (reaction.Kind != BattleReactionKind.Dodge || reaction.Amount == 0 && !reaction.Critical));
            if (reversed) actorHp = reaction.HpAfter; else targetHp = reaction.HpAfter;
        }
        var ally = actor.IsAlly ? actor : target; var enemy = actor.IsAlly ? target : actor;
        bool allyDead = (actor.IsAlly ? actorHp : targetHp) == 0;
        bool enemyDead = (actor.IsAlly ? targetHp : actorHp) == 0;
        if (enemyDead && before.Gold is null) throw new BattleRuleException("unspecified-gold", "battle.gold", true);
        if (enemyDead && ally.Kills is null) throw new BattleRuleException("unspecified-kills", "actor.kills", true);
        if (allyDead && ally.Defeats is null) throw new BattleRuleException("unspecified-defeats", "actor.defeats", true);
        Require(enemyDead ? Prepared.Gold >= before.Gold && Prepared.Gold <= 9_999_999 : Prepared.Gold == before.Gold);
        ValidateState(before, actorRef, destination, allowGold: enemyDead);
        ValidateReward(before, !allyDead && Reactions.Any(reaction => reaction.Actor == ally.Actor) ? ally.Actor : null);
        List<BattleEffect> facts = [];
        foreach (var reaction in Reactions)
        {
            facts.Add(new(reaction.Action, reaction.Actor, Target: reaction.Target));
            if (reaction.Kind == BattleReactionKind.Dodge)
                facts.Add(new("dodge", reaction.Target, reaction.HpBefore, reaction.HpAfter));
            else
            {
                if (reaction.Critical) facts.Add(new("critical", reaction.Actor, Target: reaction.Target));
                facts.Add(new("hp", reaction.Target, reaction.HpBefore, reaction.HpAfter));
            }
        }
        if (enemyDead) facts.Add(new("gold", ally.Actor, before.Gold, Prepared.Gold));
        Require(ConstructionEffects.Where(effect => !effect.Kind.StartsWith("rng-", StringComparison.Ordinal)).SequenceEqual(facts));
        ValidateRandom(before, effect => !effect.Kind.StartsWith("rng-", StringComparison.Ordinal) ||
            (effect.Actor == actorRef || effect.Actor == targetRef) &&
            (effect.Target is null || effect.Target != effect.Actor && (effect.Target == actorRef || effect.Target == targetRef)));
    }

    private void ValidateItem(EngineBattleState before, ActorRef actorRef, MapPosition destination,
        int slot, HealingItemDefinition item, ActorRef targetRef)
    {
        var actor = before.GetActor(actorRef); var target = before.GetActor(targetRef);
        var loadout = actor.SourceLoadout ?? throw new BattleRuleException("item-slot", "item.slot");
        Require(slot >= 0 && slot < loadout.Items.Count && (loadout.Items[slot] & 127) != 127 &&
            before.Definition.HealingItems.TryGetValue((byte)(loadout.Items[slot] & 127), out var admitted) && item == admitted &&
            Item == item && Spell is null && Reactions.Count == 1 && CompletionEffects.Count == 0);
        var items = loadout.Items.ToList(); items.RemoveAt(slot); items.Add(127);
        ValidateState(before, actorRef, destination, new(items, loadout.Spells));
        var reaction = Reactions[0];
        Require(target.Hp > 0 && target.Faction == actor.Faction && target.Position is not null &&
            BattleRange.Contains(destination, targetRef == actorRef ? destination : target.Position, item.MinimumRange, item.MaximumRange) &&
            reaction.Actor == actorRef && reaction.Target == targetRef && reaction.Action == "item-use" &&
            reaction.Kind == BattleReactionKind.Recovery && !reaction.Critical && reaction.HpBefore == target.Hp &&
            reaction.HpAfter >= target.Hp && reaction.HpAfter <= target.MaxHp && reaction.Amount == reaction.HpAfter - target.Hp &&
            reaction.MpBefore == target.Mp && reaction.MpAfter == target.Mp);
        ValidateReward(before, actorRef);
        ValidateRandom(before, effect => effect.Kind == "item-consumed"
            ? effect == new BattleEffect("item-consumed", actorRef, loadout.Items[slot], slot)
            : effect.Kind.StartsWith("rng-", StringComparison.Ordinal) && effect.Actor == actorRef && effect.Target is null);
        Require(ConstructionEffects.Count(effect => effect.Kind == "item-consumed") == 1);
    }

    // The finite HEAL authority boundary: construction may move only the selected
    // actor and advance main RNG. HP, MP and progress are later scene publications.
    internal void ValidateHealing(EngineBattleState before, ActorRef actorRef, MapPosition destination,
        HealingSpellDefinition spell, ActorRef targetRef)
    {
        void Require(bool valid) { if (!valid) throw new InvalidOperationException("Invalid prepared HEAL transition."); }
        BattleMovement.RequireStop(before, actorRef, destination);
        var actor = before.GetActor(actorRef);
        var target = before.GetActor(targetRef);
        Require(Actor == actorRef && Destination == destination && Spell == spell && Item is null);
        ValidateState(before, actorRef, destination);
        Require(Reactions.Count == 1 && CompletionEffects.Count == 0);
        var reaction = Reactions[0];
        Require(target.Hp > 0 && target.Faction == actor.Faction && target.Position is not null &&
            BattleRange.Contains(destination, targetRef == actorRef ? destination : target.Position, spell.MinimumRange, spell.MaximumRange));
        Require(reaction.Actor == actorRef && reaction.Target == targetRef && reaction.Action == "heal" &&
            reaction.Kind == BattleReactionKind.Recovery && !reaction.Critical && reaction.HpBefore == target.Hp &&
            reaction.HpAfter >= target.Hp && reaction.HpAfter <= target.MaxHp &&
            reaction.Amount == reaction.HpAfter - target.Hp && reaction.MpBefore == target.Mp && reaction.MpAfter == target.Mp);
        Require(actor.Mp >= spell.MpCost);
        Require(Reward is null || Reward.Actor == actorRef && Reward.Amount is > 0 and <= 200);
        uint seed = before.MainSeed;
        // The rule owns draw count, purpose and range. This boundary checks that
        // construction contains only random facts and carries the actual chain.
        foreach (var effect in ConstructionEffects)
        {
            Require(effect.Kind.StartsWith("rng-", StringComparison.Ordinal) && effect.Kind.Length > 4 &&
                effect.Actor == actorRef && effect.Target is null && effect.RandomRange is not null);
            var draw = BattleRandom.NextMain(seed, effect.RandomRange!.Value);
            Require(effect.Before == seed && effect.After == draw.After &&
                effect.RandomValue == draw.Value);
            seed = draw.After;
        }
        Require(Prepared.MainSeed == seed);
        if (Reward is { } reward) _ = BattleGrowthRules.Award(actor, reward.Amount, ref seed, []);
    }

    internal EngineBattleState ApplyReaction(EngineBattleState current, BattleReaction reaction) =>
        current.With(actors: current.Actors.Select(actor => actor.Actor == reaction.Target
            ? actor.With(hp: reaction.HpAfter, mp: Spell is null ? reaction.MpAfter : actor.Mp) : actor));

    internal EngineBattleState ApplySpellCost(EngineBattleState current) =>
        current.With(actors: current.Actors.Select(actor => actor.Actor == Actor
            ? actor.With(mp: checked((byte)(actor.Mp - Spell!.MpCost))) : actor));

    internal (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) ApplyReward(EngineBattleState current)
    {
        if (Reward is not { } reward) return (current, Array.Empty<BattleEffect>());
        uint seed = current.MainSeed;
        List<BattleEffect> effects = [];
        var actor = BattleGrowthRules.Award(current.GetActor(reward.Actor), reward.Amount, ref seed, effects);
        return (current.With(mainSeed: seed, actors: current.Actors.Select(row => row.Actor == actor.Actor ? actor : row)),
            effects.AsReadOnly());
    }

    internal (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) CreditReward(EngineBattleState current)
    {
        if (Reward is not { } reward) return (current, Array.Empty<BattleEffect>());
        List<BattleEffect> effects = [];
        var actor = BattleGrowthRules.Credit(current.GetActor(reward.Actor), reward.Amount, effects);
        return (current.With(actors: current.Actors.Select(row => row.Actor == actor.Actor ? actor : row)), effects.AsReadOnly());
    }

    internal (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) ApplyGrowth(EngineBattleState current)
    {
        if (Reward is not { } reward) return (current, Array.Empty<BattleEffect>());
        uint seed = current.MainSeed;
        List<BattleEffect> effects = [];
        var actor = BattleGrowthRules.Grow(current.GetActor(reward.Actor), ref seed, effects);
        return (current.With(mainSeed: seed, actors: current.Actors.Select(row => row.Actor == actor.Actor ? actor : row)), effects.AsReadOnly());
    }

    internal ActorRef FirstAlly => Prepared.GetActor(Reactions[0].Actor).IsAlly
        ? Reactions[0].Actor : Reactions[0].Target;
}
