using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class PhysicalBattleAction
{
    internal static PhysicalActorDefinition RequireActor(EngineBattleState battle, ActorRef actorRef)
    {
        var actor = battle.GetActor(actorRef);
        if (actor.Hp == 0) throw new BattleRuleException("physical-actor", "actor");
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
        if (target is null || target.Hp == 0 || target.IsAlly == battle.GetActor(actorRef).IsAlly)
            throw new BattleRuleException("physical-target", "target");
        if (target.Definition.Physical is null)
            throw new BattleRuleException("physical-definition", "target.physical", true);
        var range = battle.GetActor(actorRef).Definition.Physical!;
        if (!BattleRange.Contains(destination, target.Position!, range.MinimumRange, range.MaximumRange))
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
        var counterRange = target.Definition.Physical!;
        bool counterPerformed = targetHp > 0 && counterRequested &&
            BattleRange.Contains(target.Position!, destination, counterRange.MinimumRange, counterRange.MaximumRange);
        if (counterPerformed) _ = Hit("physical-counter", counter: true);
        // No third/double-counter dispatch exists. A surviving counter still consumes its own
        // double/counter draws, whose toggles cannot schedule another strike.

        bool targetDead = targetHp == 0, actorDead = actorHp == 0;
        RequireContinuing(target, targetDead);
        RequireContinuing(actor, actorDead);
        var ally = actor.IsAlly ? actor : target;
        var enemy = actor.IsAlly ? target : actor;
        bool allyDead = actor.IsAlly ? actorDead : targetDead;
        bool enemyDead = actor.IsAlly ? targetDead : actorDead;
        int? exp = ally.Exp;
        if (!allyDead && (actor.IsAlly || counterPerformed))
        {
            // Only a surviving ally who actually attacked earns an award (including a counter).
            if (ally.Exp is null) throw new BattleRuleException("unspecified-exp", "actor.exp", true);
            var awardRolls = new List<PhysicalRoll>();
            int award = BattleRewards.Award(accumulated, current.Definition.Rewards!.HalvedExperience, ref seed, awardRolls);
            AddRolls(awardRolls, ally.Actor);
            exp = Math.Min(200, ally.Exp.Value + award);
            if (exp >= 100) throw new BattleRuleException("level-up", "actor.exp", true);
            effects.Add(new("exp", ally.Actor, ally.Exp, exp));
        }
        uint? gold = enemyDead ? BattleRewards.Gold(current.Gold ?? throw new BattleRuleException("unspecified-gold", "battle.gold", true),
            enemy.Definition.Physical!.Gold) : current.Gold;
        if (enemyDead)
        {
            effects.Add(new("gold", ally.Actor, current.Gold, gold));
            effects.Add(new("kills", ally.Actor, ally.Kills, BattleRewards.Kills(ally.Kills ?? throw new BattleRuleException("unspecified-kills", "actor.kills", true))));
            effects.Add(new("death-cleanup", enemy.Actor, 1, 0));
        }
        if (allyDead)
        {
            effects.Add(new("defeats", ally.Actor, ally.Defeats, BattleRewards.Defeats(ally.Defeats ?? throw new BattleRuleException("unspecified-defeats", "actor.defeats", true))));
            effects.Add(new("death-cleanup", ally.Actor, 1, 0));
        }
        // Admitted equipment changes effective ATT only. Status-free after-turn does not change this snapshot;
        // the second faction check therefore has the same continuing result as the first.
        var actors = current.Actors.Select(a => a.Actor == actorRef || a.Actor == targetRef
            ? a.With(hp: a.Actor == actorRef ? actorHp : targetHp,
                position: a.Actor == actorRef ? destination : a.Position,
                exp: a.Actor == ally.Actor ? (byte?)exp : a.Exp,
                kills: a.Actor == ally.Actor && enemyDead ? BattleRewards.Kills(a.Kills ?? throw new BattleRuleException("unspecified-kills", "actor.kills", true)) : a.Kills,
                defeats: a.Actor == ally.Actor && allyDead ? BattleRewards.Defeats(a.Defeats ?? throw new BattleRuleException("unspecified-defeats", "actor.defeats", true)) : a.Defeats)
            : a);
        return (current.With(actors: actors, mainSeed: seed, gold: gold), effects.AsReadOnly());

        PhysicalStrike Hit(string kind, bool counter)
        {
            var attacker = counter ? target : actor;
            var defender = counter ? actor : target;
            var targetPosition = counter ? destination : target.Position!;
            ushort hp = counter ? actorHp : targetHp;
            var profile = attacker.Definition.Physical!;
            var terrain = current.Definition.Terrain[targetPosition.Y * 48 + targetPosition.X];
            // Ground protection is separate from movement cost, including the
            // moved original actor's destination when it becomes the counter's target.
            int multiplier = BattleTerrainRules.LandMultiplier(terrain, defender.Definition.Mover);
            var strike = PhysicalStrikeRules.Resolve(attacker.Attack, defender.Definition.Defense,
                hp, multiplier, seed, defender.Definition.Mover == BattleMover.Hovering ? (ushort)8 : (ushort)32,
                profile.Critical.ChanceDenominator,
                profile.Critical.DamageBonusShift, counter);
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
            else targetHp = strike.Hp;
            if (attacker.IsAlly)
            {
                int killExp = BattleRewards.KillExperience(attacker.Definition.Level, profile.Promoted, defender.Definition.Level);
                // Each hit truncates its damage EXP separately, then adds to one capped action
                // accumulator. Enemy strikes never earn ally EXP, regardless of action direction.
                accumulated = Math.Min(49, accumulated + BattleRewards.DamageExperience(strike.Damage, defender.Definition.MaxHp, killExp));
                if (strike.Hp == 0) accumulated = Math.Min(49, accumulated + killExp);
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
                    defeated.IsAlly ? "actor.physical.leader" : "target.physical.leader", true);
            if (current.Actors.Count(a => a.Hp > 0 && a.IsAlly == defeated.IsAlly) == 1)
                throw new BattleRuleException("battle-outcome-program", "battle.outcome", true);
        }
    }

}
