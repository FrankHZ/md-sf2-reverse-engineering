using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class PlayerPhysicalAttack
{
    internal static PhysicalActorDefinition RequireActor(EngineBattleState battle, ActorRef actorRef)
    {
        var actor = battle.GetActor(actorRef);
        if (actor.Hp == 0 || !actor.Definition.IsAlly) throw new BattleRuleException("physical-actor", "actor");
        if (actor.Definition.Physical is not { } physical)
            throw new BattleRuleException("physical-definition", "actor.physical", true);
        if (battle.Definition.Rewards is null)
            throw new BattleRuleException("battle-rewards", "encounter.rewards", true);
        return physical;
    }

    internal static BattleActorState RequireTarget(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, ActorRef targetRef)
    {
        _ = RequireActor(battle, actorRef);
        var target = battle.Actors.SingleOrDefault(a => a.Actor == targetRef);
        if (target is null || target.Hp == 0 || target.Definition.IsAlly)
            throw new BattleRuleException("physical-target", "target");
        if (target.Definition.Physical is null)
            throw new BattleRuleException("physical-definition", "target.physical", true);
        if (Math.Abs(destination.X - target.Position!.X) + Math.Abs(destination.Y - target.Position.Y) != 1)
            throw new BattleRuleException("target-range", "target");
        return target;
    }

    internal static (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) Resolve(
        EngineBattleState current, ActorRef actorRef, MapPosition destination, ActorRef targetRef)
    {
        BattleMovement.RequireStop(current, actorRef, destination);
        var profile = RequireActor(current, actorRef);
        var actor = current.GetActor(actorRef);
        var target = RequireTarget(current, actorRef, destination, targetRef);
        // Regular movement's decoded land nibble is separate from movement cost.
        byte terrain = current.Definition.Terrain[target.Position!.Y * 48 + target.Position.X];
        int multiplier = terrain switch { 1 => 230, 2 => 256, >= 3 and <= 6 => 205,
            _ => throw new BattleRuleException("physical-terrain", "target.terrain", true) };
        var strike = PhysicalStrikeRules.Resolve(actor.Definition.Attack, target.Definition.Defense,
            target.Hp, multiplier, current.MainSeed, 32, profile.Prowess == 0 ? (ushort)32 : (ushort)16,
            profile.Prowess == 0 ? 1 : 2);
        // Both natural rolls were consumed on temporary state, in source order. All admitted
        // actors are adjacent, ordinary, unarmed, opposing-side and status-free.
        if (strike.Double) throw new BattleRuleException("physical-double", "action.followup", true);
        if (strike.Counter) throw new BattleRuleException("physical-counter", "action.followup", true);
        bool dead = strike.Hp == 0;
        if (dead && target.Definition.Physical!.Leader)
            throw new BattleRuleException("leader-defeat-program", "target.physical.leader", true);
        if (dead && current.Actors.Count(a => a.Hp > 0 && !a.Definition.IsAlly) == 1)
            throw new BattleRuleException("battle-outcome-program", "battle.outcome", true);

        int killExp = BattleRewards.KillExperience(actor.Definition.Level, profile.Promoted, target.Definition.Level);
        int accumulated = BattleRewards.DamageExperience(strike.Damage, target.Definition.MaxHp, killExp);
        if (dead) accumulated = Math.Min(49, accumulated + killExp);
        uint seed = strike.Seed;
        var rolls = strike.Rolls.ToList();
        int award = BattleRewards.Award(accumulated, current.Definition.Rewards!.HalvedExperience, ref seed, rolls);
        int exp = Math.Min(200, actor.Exp + award);
        if (exp >= 100) throw new BattleRuleException("level-up", "actor.exp", true);
        uint gold = dead ? BattleRewards.Gold(current.Gold, target.Definition.Physical!.Gold) : current.Gold;
        var effects = new List<BattleEffect>();
        foreach (var roll in rolls) effects.Add(new("rng-" + roll.Purpose, actorRef, roll.Before, roll.After, roll.Range, roll.Result));
        if (strike.Dodged) effects.Add(new("dodge", targetRef, target.Hp, target.Hp));
        else
        {
            if (strike.Critical) effects.Add(new("critical", actorRef, 0, 1));
            effects.Add(new("hp", targetRef, target.Hp, strike.Hp));
        }
        effects.Add(new("exp", actorRef, actor.Exp, exp));
        if (dead)
        {
            effects.Add(new("gold", actorRef, (int)current.Gold, (int)gold));
            effects.Add(new("death-cleanup", targetRef, 1, 0));
            effects.Add(new("kills", actorRef, actor.Kills, BattleRewards.Kills(actor.Kills)));
        }
        // No status or equipment branch is admitted by Content in this slice. The ordinary
        // after-turn refresh has no resource or RNG effect; both faction checks still continue.
        effects.Add(new("after-turn", actorRef, 0, 0));
        var actors = current.Actors.Select(a => a.Actor == actorRef
            ? a.With(exp: (byte)exp, position: destination, kills: dead ? BattleRewards.Kills(a.Kills) : a.Kills)
            : a.Actor == targetRef ? a.With(hp: strike.Hp, remove: dead) : a);
        return (current.With(actors: actors, mainSeed: seed, gold: gold), effects.AsReadOnly());
    }
}
