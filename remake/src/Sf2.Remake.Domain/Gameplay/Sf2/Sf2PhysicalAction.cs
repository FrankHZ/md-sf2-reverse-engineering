using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed class Sf2PhysicalAction : IPhysicalActionRule
{
    public string Identity => "sf2-physical";
    public BattleActionKind Kind => BattleActionKind.Physical;
    public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) =>
        [new(new(Kind), "ATTACK")];
    public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actor, BattleActionRef action)
    {
        if (action != new BattleActionRef(Kind)) throw new BattleRuleException("physical-action", "action");
        var physical = RequireActor(battle, actor);
        return new(physical.MinimumRange, physical.MaximumRange);
    }
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) =>
        Array.AsReadOnly(battle.Actors.Where(target => target.Hp > 0 && target.Position is not null &&
            target.Faction != battle.GetActor(actor).Faction).Select(target => target.Actor).ToArray());
    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target)
    { _ = RequireAction(battle, actor, action); return RequireTarget(battle, actor, destination, target); }
    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, IBattleProgressionRule progression)
    {
        _ = RequireAction(battle, actor, action);
        return Prepare(battle, actor, destination, target ?? throw new BattleRuleException("physical-target", "target"), progression);
    }
    public int EstimateDamage(EngineBattleState battle, ActorRef actor, ActorRef target)
    {
        var defender = battle.GetActor(target);
        if (defender.Definition.Physical is null)
            throw new BattleRuleException("physical-definition", "target.physical", true);
        return PhysicalStrikeRules.LandDamage(battle.GetActor(actor).Attack, defender.Defense,
            LandMultiplier(battle.Definition.Terrain[defender.Position!.Y * 48 + defender.Position.X], defender.Definition.Mover));
    }

    internal static int LandMultiplier(BattleTerrain terrain, BattleMover mover = BattleMover.Regular)
    {
        if (!Enum.IsDefined(mover)) throw new BattleRuleException("movement-profile", "actor.mover", true);
        // Protection is independent of the movement-cost table, including hovering.
        return terrain.Protection switch
        {
            TerrainProtection.None => 256, TerrainProtection.Light => 230, TerrainProtection.Heavy => 205,
            _ => throw new BattleRuleException("terrain-protection", "terrain.protection", true),
        };
    }

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

    internal static BattleActionResolution Prepare(
        EngineBattleState current, ActorRef actorRef, MapPosition destination, ActorRef targetRef, IBattleProgressionRule progression)
    {
        BattleMovement.RequireStop(current, actorRef, destination);
        _ = RequireActor(current, actorRef);
        var actor = current.GetActor(actorRef);
        var target = RequireTarget(current, actorRef, destination, targetRef);
        ushort actorHp = actor.Hp, targetHp = target.Hp;
        uint seed = current.MainSeed;
        var effects = new List<BattleEffect>();
        var reactions = new List<BattleReaction>();

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
        BattleActionReward? reward = null;
        if (!allyDead && (actor.IsAlly || counterPerformed))
        {
            // Only a surviving ally who actually attacked earns an award (including a counter).
            if (ally.Exp is null) throw new BattleRuleException("unspecified-exp", "actor.exp", true);
            var award = BattleProgressionRules.Award(progression, current.With(mainSeed: seed), ally, BattleActionKind.Physical, reactions.AsReadOnly());
            seed = award.Seed;
            effects.AddRange(award.Effects);
            reward = new(ally.Actor, award.Amount);
        }
        uint? gold = enemyDead ? BattleProgressionRules.Invoke(progression.Identity, "gold", () => progression.Gold(
            current.Gold ?? throw new BattleRuleException("unspecified-gold", "battle.gold", true), enemy.Definition.Physical!.Gold)) : current.Gold;
        if (enemyDead)
        {
            effects.Add(new("gold", ally.Actor, current.Gold, gold));
            if (ally.Kills is null) throw new BattleRuleException("unspecified-kills", "actor.kills", true);
        }
        if (allyDead && ally.Defeats is null)
            throw new BattleRuleException("unspecified-defeats", "actor.defeats", true);
        // Admitted equipment changes effective ATT only. Status-free after-turn does not change this snapshot;
        // the second faction check therefore has the same continuing result as the first.
        var prepared = current.With(actors: current.Actors.Select(a => a.Actor == actorRef
            ? a.With(position: destination) : a), mainSeed: seed, gold: gold);
        return new(prepared, actorRef, destination, reactions.AsReadOnly(), reward,
            effects.AsReadOnly(), []);

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
            int multiplier = LandMultiplier(terrain, defender.Definition.Mover);
            var strike = PhysicalStrikeRules.Resolve(attacker.Attack, defender.Defense,
                hp, multiplier, seed, defender.Definition.Mover == BattleMover.Hovering ? (ushort)8 : (ushort)32,
                profile.Critical.ChanceDenominator,
                profile.Critical.DamageBonusShift, counter);
            reactions.Add(new(attacker.Actor, defender.Actor, kind,
                strike.Dodged ? BattleReactionKind.Dodge : BattleReactionKind.Damage,
                hp, strike.Hp, defender.Mp, defender.Mp, strike.Critical, strike.Damage));
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
            if (current.Definition.Outcome is not null) return;
            if (defeated.Definition.Physical!.Leader)
                throw new BattleRuleException("leader-defeat-program",
                    defeated.IsAlly ? "actor.physical.leader" : "target.physical.leader", true);
            if (current.Actors.Count(a => a.Hp > 0 && a.IsAlly == defeated.IsAlly) == 1)
                throw new BattleRuleException("battle-outcome-program", "battle.outcome", true);
        }
    }

    // Scalar action boundary retained for callers that explicitly evaluate combat without a
    // presentation consumer. The session uses Prepare and replays at semantic scene edges.
    internal static (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) Resolve(
        EngineBattleState current, ActorRef actor, MapPosition destination, ActorRef target)
    {
        var progression = new Sf2BattleProgressionRule();
        var action = BattleActionRules.Prepare(new Sf2PhysicalAction(), current, actor, destination, new(BattleActionKind.Physical), target, progression);
        var battle = action.Prepared;
        var deaths = BattleDeathBatch.Empty;
        foreach (var reaction in action.Reactions)
        {
            var before = battle.GetActor(reaction.Target);
            battle = action.ApplyReaction(battle, reaction);
            deaths = deaths.Append(before, battle.GetActor(reaction.Target));
        }
        var reward = action.ApplyReward(battle, progression);
        var cleaned = deaths.Clean(reward.Battle, action.FirstAlly, progression);
        return (cleaned.Battle, Array.AsReadOnly<BattleEffect>([
            .. action.ConstructionEffects, .. reward.Effects, .. cleaned.Effects]));
    }

}
