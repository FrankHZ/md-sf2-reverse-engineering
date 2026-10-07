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
        Require(ReferenceEquals(Prepared.Definition, before.Definition) && Prepared.ThinkingSeed == before.ThinkingSeed &&
            Prepared.Round == before.Round && Prepared.Cursor == before.Cursor && Prepared.Gold == before.Gold &&
            ReferenceEquals(Prepared.StartPolicy, before.StartPolicy) && ReferenceEquals(Prepared.Regions, before.Regions) &&
            Prepared.Queue.SequenceEqual(before.Queue) && Prepared.Actors.Count == before.Actors.Count);
        for (int index = 0; index < before.Actors.Count; index++)
        {
            var old = before.Actors[index]; var candidate = Prepared.Actors[index];
            Require(ReferenceEquals(candidate.Deployment, old.Deployment) && candidate.Hp == old.Hp &&
                candidate.Mp == old.Mp && candidate.Exp == old.Exp && candidate.Kills == old.Kills && candidate.Defeats == old.Defeats &&
                candidate.LastTarget == old.LastTarget && candidate.Attack == old.Attack && candidate.Status == old.Status &&
                candidate.ActivationWord == old.ActivationWord && candidate.AiMemory == old.AiMemory &&
                Equals(candidate.Progress, old.Progress) && ReferenceEquals(candidate.SourceLoadout, old.SourceLoadout) &&
                candidate.Position == (old.Actor == actorRef ? destination : old.Position));
        }
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
