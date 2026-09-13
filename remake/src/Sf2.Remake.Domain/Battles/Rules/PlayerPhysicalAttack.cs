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
        _ = RequireActor(current, actorRef);
        var actor = current.GetActor(actorRef);
        var target = RequireTarget(current, actorRef, destination, targetRef);
        ushort actorHp = actor.Hp, targetHp = target.Hp;
        uint seed = current.MainSeed;
        int accumulated = 0;
        var effects = new List<BattleEffect>();

        var first = Hit("physical-first", counter: false);
        bool counterRequested = first.CounterRolled;
        if (targetHp > 0 && first.DoubleRolled)
        {
            var second = Hit("physical-second", counter: false);
            // Source DetermineDoubleAndCounter only sets successful toggles. A failed second
            // counter draw does not clear the first success; second-hit death still invalidates it.
            counterRequested |= second.CounterRolled;
        }
        if (targetHp > 0 && counterRequested) _ = Hit("physical-counter", counter: true);
        // No third/double-counter dispatch exists. A surviving counter still consumes its own
        // double/counter draws, whose toggles cannot schedule another strike.

        bool targetDead = targetHp == 0, actorDead = actorHp == 0;
        RequireContinuing(target, targetDead);
        RequireContinuing(actor, actorDead);
        int exp = actor.Exp;
        if (!actorDead)
        {
            // End skips EXP and its two random draws when the original ally actor died.
            var awardRolls = new List<PhysicalRoll>();
            int award = BattleRewards.Award(accumulated, current.Definition.Rewards!.HalvedExperience, ref seed, awardRolls);
            AddRolls(awardRolls, actorRef);
            exp = Math.Min(200, actor.Exp + award);
            if (exp >= 100) throw new BattleRuleException("level-up", "actor.exp", true);
            effects.Add(new("exp", actorRef, actor.Exp, exp));
        }
        uint gold = targetDead ? BattleRewards.Gold(current.Gold, target.Definition.Physical!.Gold) : current.Gold;
        if (targetDead)
        {
            effects.Add(new("gold", actorRef, current.Gold, gold));
            effects.Add(new("kills", actorRef, actor.Kills, BattleRewards.Kills(actor.Kills)));
            effects.Add(new("death-cleanup", targetRef, 1, 0));
        }
        if (actorDead)
        {
            effects.Add(new("defeats", actorRef, actor.Defeats, BattleRewards.Defeats(actor.Defeats)));
            effects.Add(new("death-cleanup", actorRef, 1, 0));
        }
        // No status/equipment effects are admitted. After-turn does not change this snapshot;
        // the second faction check therefore has the same continuing result as the first.
        effects.Add(new("after-turn", actorRef));
        var actors = current.Actors.Select(a => a.Actor == actorRef
            ? a.With(hp: actorHp, exp: (byte)exp, position: destination,
                kills: targetDead ? BattleRewards.Kills(a.Kills) : a.Kills,
                defeats: actorDead ? BattleRewards.Defeats(a.Defeats) : a.Defeats)
            : a.Actor == targetRef ? a.With(hp: targetHp) : a);
        return (current.With(actors: actors, mainSeed: seed, gold: gold), effects.AsReadOnly());

        PhysicalStrike Hit(string kind, bool counter)
        {
            var attacker = counter ? target : actor;
            var defender = counter ? actor : target;
            var targetPosition = counter ? destination : target.Position!;
            ushort hp = counter ? actorHp : targetHp;
            var profile = attacker.Definition.Physical!;
            byte terrain = current.Definition.Terrain[targetPosition.Y * 48 + targetPosition.X];
            // Regular movement's land nibble is separate from movement cost, including the
            // moved original actor's destination when it becomes the counter's target.
            int multiplier = terrain switch { 1 => 230, 2 => 256, >= 3 and <= 6 => 205,
                _ => throw new BattleRuleException("physical-terrain", "target.terrain", true) };
            var strike = PhysicalStrikeRules.Resolve(attacker.Definition.Attack, defender.Definition.Defense,
                hp, multiplier, seed, 32, profile.Prowess == 0 ? (ushort)32 : (ushort)16,
                profile.Prowess == 0 ? 1 : 2, counter);
            seed = strike.Seed;
            effects.Add(new(kind, attacker.Actor, Target: defender.Actor));
            AddRolls(strike.Rolls, attacker.Actor, defender.Actor);
            if (strike.Dodged) effects.Add(new("dodge", defender.Actor, hp, hp));
            else
            {
                if (strike.Critical) effects.Add(new("critical", attacker.Actor, Target: defender.Actor));
                effects.Add(new("hp", defender.Actor, hp, strike.Hp));
            }
            if (counter) actorHp = strike.Hp;
            else
            {
                targetHp = strike.Hp;
                int killExp = BattleRewards.KillExperience(actor.Definition.Level, profile.Promoted, target.Definition.Level);
                // Each hit truncates its damage EXP separately, then adds to one capped action
                // accumulator. Enemy counter damage never earns EXP for the original player.
                accumulated = Math.Min(49, accumulated + BattleRewards.DamageExperience(strike.Damage, target.Definition.MaxHp, killExp));
                if (targetHp == 0) accumulated = Math.Min(49, accumulated + killExp);
            }
            return strike;
        }

        void AddRolls(IEnumerable<PhysicalRoll> rolls, ActorRef roller, ActorRef? targetActor = null)
        {
            foreach (var roll in rolls)
                effects.Add(new("rng-" + roll.Purpose, roller, roll.Before, roll.After,
                    roll.Range, roll.Result, targetActor));
        }

        void RequireContinuing(BattleActorState defeated, bool dead)
        {
            if (!dead) return;
            if (defeated.Definition.Physical!.Leader)
                throw new BattleRuleException("leader-defeat-program",
                    defeated.Definition.IsAlly ? "actor.physical.leader" : "target.physical.leader", true);
            if (current.Actors.Count(a => a.Hp > 0 && a.Definition.IsAlly == defeated.Definition.IsAlly) == 1)
                throw new BattleRuleException("battle-outcome-program", "battle.outcome", true);
        }
    }
}
