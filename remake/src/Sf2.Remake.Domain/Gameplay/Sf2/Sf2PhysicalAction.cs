using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed class Sf2PhysicalAction : IPhysicalActionRule
{
    public string Identity => "sf2-physical";
    public BattleActionKind Kind => BattleActionKind.Physical;
    public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) =>
        [new(new(Kind), "ATTACK")];
    public BattleActionAdmission QueryRange(EngineBattleState battle, ActorRef actor)
    {
        var physical = BattleEquipment.Physical(battle.GetActor(actor))
            ?? throw new BattleRuleException("physical-definition", "actor.physical", true);
        return new(physical.MinimumRange, physical.MaximumRange);
    }
    public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actor, BattleActionRef action)
    {
        if (action != new BattleActionRef(Kind)) throw new BattleRuleException("physical-action", "action");
        var physical = RequireActor(battle, actor);
        return new(physical.MinimumRange, physical.MaximumRange);
    }
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) =>
        QueryTargets(battle, actor, action, OffensiveTargetSide.Opponents);
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action, OffensiveTargetSide side)
    {
        var attacker = battle.GetActor(actor);
        var roster = side == OffensiveTargetSide.Opponents ? battle.Actors : BattleOffense.OrderedRoster(battle, attacker, side);
        return Array.AsReadOnly(roster.Where(target => target.Hp > 0 && target.Position is not null &&
            BattleOffense.Matches(attacker, target, side)).Select(target => target.Actor).ToArray());
    }
    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target)
        => RequireTarget(battle, actor, destination, action, target, OffensiveTargetSide.Opponents);
    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target, OffensiveTargetSide side)
    { _ = RequireAction(battle, actor, action); return RequireTarget(battle, actor, destination, target, side); }
    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, IBattleProgressionRule progression) =>
        Prepare(battle, actor, destination, action, target, progression, OffensiveTargetSide.Opponents);
    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, IBattleProgressionRule progression, OffensiveTargetSide side)
    {
        _ = RequireAction(battle, actor, action);
        return Prepare(battle, actor, destination, target ?? throw new BattleRuleException("physical-target", "target"), progression, side);
    }
    public int EstimateDamage(EngineBattleState battle, ActorRef actor, ActorRef target)
    {
        var defender = battle.GetActor(target);
        if (defender.Definition.Physical is null)
            throw new BattleRuleException("physical-definition", "target.physical", true);
        return PhysicalStrikeRules.LandDamage(battle.GetActor(actor).Attack, defender.Defense,
            LandMultiplier(battle.Definition.Terrain[defender.Position!.Y * 48 + defender.Position.X], defender.Definition.Mover),
            ArcherBonus(battle.GetActor(actor).Definition.Mover, defender.Definition.Mover));
    }

    internal static bool ArcherBonus(BattleMover attacker, BattleMover defender) =>
        defender is (BattleMover.Flying or BattleMover.Hovering) &&
        attacker is BattleMover.Archer or BattleMover.CentaurArcher or BattleMover.StealthArcher or BattleMover.BrassGunner;

    internal static int LandMultiplier(BattleTerrain terrain, BattleMover mover = BattleMover.Regular)
    {
        // The source display setting and damage multiplier share mover/protection semantics.
        return BattleTerrainRules.LandEffect(terrain, mover) switch
        { 0 => 256, 15 => 230, 30 => 205, _ => throw new InvalidOperationException() };
    }

    internal static PhysicalActorDefinition RequireActor(EngineBattleState battle, ActorRef actorRef)
    {
        var actor = battle.GetActor(actorRef);
        if (actor.Hp == 0) throw new BattleRuleException("physical-actor", "actor");
        if (BattleEquipment.Physical(actor) is not { } physical)
            throw new BattleRuleException("physical-definition", "actor.physical", true);
        if (battle.Definition.Rewards is null)
            throw new BattleRuleException("battle-rewards", "encounter.rewards", true);
        return physical;
    }

    internal static BattleActorState RequireTarget(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, ActorRef targetRef, OffensiveTargetSide side = OffensiveTargetSide.Opponents)
    {
        var range = RequireActor(battle, actorRef);
        var actor = battle.GetActor(actorRef);
        var target = battle.Actors.SingleOrDefault(a => a.Actor == targetRef);
        if (target is null || target.Hp == 0 || target.Position is null || !BattleOffense.Matches(actor, target, side))
            throw new BattleRuleException("physical-target", "target");
        if (target.Definition.Physical is null)
            throw new BattleRuleException("physical-definition", "target.physical", true);
        if (!InRange(actor, target, destination, range))
            throw new BattleRuleException("target-range", "target");
        return target;
    }

    internal static bool InRange(BattleActorState actor, BattleActorState target, MapPosition destination, PhysicalActorDefinition range) =>
        actor.Actor == target.Actor && destination == target.Position && BattleOffense.SelfPhysicalUsesOrigin(actor) ||
        BattleRange.Contains(destination, target.Position!, range.MinimumRange, range.MaximumRange);

    internal static BattleEffect? MonsterBreathRoll(EngineBattleState battle, ActorRef actor, ActorRef target)
    {
        if (battle.GetActor(actor).Definition.ClassRule != BattleClassRule.Monster) return null;
        var draw = BattleRandom.NextMain(battle.MainSeed, 4);
        return new("rng-kiwi-conversion", actor, draw.Before, draw.After, draw.Range, draw.Value, target);
    }

    internal static SpellRef MonsterBreathSpell(byte level) =>
        new("kiwi", level >= 50 ? (byte)4 : level >= 40 ? (byte)3 : level >= 32 ? (byte)2 : (byte)1);

    internal static BattleActionResolution Prepare(
        EngineBattleState current, ActorRef actorRef, MapPosition destination, ActorRef targetRef,
        IBattleProgressionRule progression, OffensiveTargetSide side = OffensiveTargetSide.Opponents)
    {
        BattleMovement.RequireStop(current, actorRef, destination);
        _ = RequireActor(current, actorRef);
        var actor = current.GetActor(actorRef);
        var target = RequireTarget(current, actorRef, destination, targetRef, side);
        bool self = actorRef == targetRef;
        bool opposing = side == OffensiveTargetSide.Opponents;
        if (!SilenceRules.SupportedInBattle(actor.Status, current.Definition.Poison) ||
            !SilenceRules.SupportedInBattle(target.Status, current.Definition.Poison))
            throw new BattleRuleException("physical-status", "actors.status", true);
        ushort actorHp = actor.Hp, targetHp = target.Hp;
        uint seed = current.MainSeed;
        var effects = new List<BattleEffect>();
        var reactions = new List<BattleReaction>();
        var inventory = current;
        if (MonsterBreathRoll(current, actorRef, targetRef) is { } conversion)
        {
            effects.Add(conversion);
            seed = checked((uint)conversion.After!.Value);
            if (conversion.RandomValue == 0)
            {
                var spell = current.Definition.AttackSpells.GetValueOrDefault(MonsterBreathSpell(actor.Level))
                    ?? throw new BattleRuleException("monster-breath-content", "spells.kiwi", true);
                var breath = Sf2AttackSpellAction.PrepareAttack(current.With(mainSeed: seed),
                    actorRef, destination, spell, targetRef, progression, itemSlot: null, side: side);
                return breath with { ConstructionEffects = Array.AsReadOnly<BattleEffect>([conversion, .. breath.ConstructionEffects]) };
            }
        }

        if (CurseRules.Present(actor.Status))
        {
            bool curseStops = Roll("curse-inaction", CurseRules.InactionRange, actorRef, targetRef) == 0;
            // Scene initialization draws for both bits before choosing the visible cause.
            // Ordinary stun turn skipping is owned by BattleAdvancer.
            bool stunStops = (actor.Status & PoisonCureRules.Stun) != 0 &&
                Roll("stun-inaction", 4, actorRef, targetRef) == 0;
            if (curseStops || stunStops)
                return new(PreparedState(current.Gold), actorRef, destination, [], null, effects.AsReadOnly(), [])
                { Inaction = new(targetRef, DueToCurse: curseStops) };
        }

        bool ineffective = current.Definition.TarosEffectiveness?.PrepareInitial(actor, target) ?? false;
        var first = Hit("physical-first", counter: false);
        bool counterRequested = first.CounterRolled;
        if (opposing && actorHp > 0 && targetHp > 0 && first.DoubleRolled)
        {
            var second = Hit("physical-second", counter: false);
            // Source DetermineDoubleAndCounter only sets successful toggles. A failed second
            // counter draw does not clear the first success; second-hit death still invalidates it.
            counterRequested |= second.CounterRolled;
        }
        var counterRange = BattleEquipment.Physical(target)!;
        bool counterPerformed = opposing && actorHp > 0 && targetHp > 0 && counterRequested && !counterRange.CannotCounter && (target.Status & 0xC1) == 0 &&
            !(current.Definition.TarosEffectiveness?.IsTaros(actor) ?? false) &&
            BattleRange.Contains(target.Position!, destination, counterRange.MinimumRange, counterRange.MaximumRange);
        if (counterPerformed) _ = Hit("physical-counter", counter: true);
        // No third/double-counter dispatch exists. A surviving counter still consumes its own
        // double/counter draws, whose toggles cannot schedule another strike.

        BattleOutcomeSelection.RequireContinuing(current, reactions);
        foreach (var reaction in reactions) BattleOffense.RequireDeathAccounting(current, reaction);
        var recipient = actor.IsAlly ? actor : target.IsAlly && counterPerformed ? target : null;
        BattleActionReward? reward = null;
        if (recipient is not null && BattleOffense.Survives(recipient, reactions))
        {
            var award = BattleProgressionRules.Award(progression, inventory.With(mainSeed: seed),
                inventory.GetActor(recipient.Actor), BattleActionKind.Physical, reactions.AsReadOnly());
            seed = award.Seed;
            effects.AddRange(award.Effects);
            reward = new(recipient.Actor, award.Amount);
        }
        uint? gold = current.Gold;
        foreach (var reaction in reactions.Where(r => BattleOffense.PaysGold(current, r)))
        {
            var casualty = current.GetActor(reaction.Target);
            gold = BattleProgressionRules.Invoke(progression.Identity, "gold", () => progression.Gold(
                current.Gold ?? throw new BattleRuleException("unspecified-gold", "battle.gold", true), casualty.Definition.Physical!.Gold));
            effects.Add(new("gold", reaction.Actor, current.Gold, gold));
        }
        return new(PreparedState(gold), actorRef, destination, reactions.AsReadOnly(), reward,
            effects.AsReadOnly(), []);

        EngineBattleState PreparedState(uint? goldAfter) =>
            inventory.With(actors: inventory.Actors.Select(a => a.Actor == actorRef
                ? a.With(position: destination) : a), mainSeed: seed, gold: goldAfter);

        (bool DoubleRolled, bool CounterRolled) Hit(string kind, bool counter)
        {
            var attacker = counter ? target : actor;
            var defender = counter ? actor : target;
            var targetPosition = counter || self ? destination : target.Position!;
            ushort hp = counter ? actorHp : targetHp;
            var profile = attacker.Definition.Physical!;
            ushort status = defender.Status;
            var terrain = current.Definition.Terrain[targetPosition.Y * 48 + targetPosition.X];
            // Ground protection is separate from movement cost, including the
            // moved original actor's destination when it becomes the counter's target.
            int multiplier = LandMultiplier(terrain, defender.Definition.Mover);
            bool archerBonus = ArcherBonus(attacker.Definition.Mover, defender.Definition.Mover);
            ushort dodgeRange = MuddleRules.Present(attacker.Status) ? (ushort)2 :
                !archerBonus && defender.Definition.Mover is (BattleMover.Hovering or BattleMover.Flying) ? (ushort)8 : (ushort)32;
            var strike = PhysicalStrikeRules.Resolve(attacker.Attack, defender.Defense,
                hp, multiplier, seed, dodgeRange,
                profile.Critical.ChanceDenominator,
                profile.Critical.DamageBonusShift, counter, profile.Critical.InflictsSleep, (status & 0xC1) != 0,
                ineffective: !counter && ineffective, silenceCritical: profile.Critical.InflictsSilence, poisonCritical: profile.Critical.InflictsPoison,
                stunCritical: profile.Critical.InflictsStun, archerBonus: archerBonus);
            int impactIndex = reactions.Count;
            reactions.Add(new(attacker.Actor, defender.Actor, kind,
                strike.Dodged ? BattleReactionKind.Dodge : strike.Ineffective ? BattleReactionKind.Ineffective : BattleReactionKind.Damage,
                hp, strike.Hp, defender.Mp, defender.Mp, strike.Critical && !strike.Ineffective, strike.Damage,
                strike.Dodged || strike.Ineffective ? null : status,
                strike.SleepInflicted ? (ushort)(status | 0xC0) : null,
                InflictedStun: strike.StunInflicted ? (ushort)(status | 1) : null,
                InflictedPoison: strike.PoisonInflicted ? (ushort)(status | 2) : null,
                InflictedSilence: strike.SilenceInflicted && SilenceRules.HasSpells(defender) ? (ushort)(status | SilenceRules.Mask) : null));
            seed = strike.Seed;
            effects.Add(new(kind, attacker.Actor, Target: defender.Actor));
            AddRolls(strike.Rolls, attacker.Actor, defender.Actor);
            ushort attackerHp = self ? strike.Hp : counter ? targetHp : actorHp;
            BattleReaction? recoil = null;
            if (CurseRules.CanRecoil(attacker, strike.Dodged, strike.Hp) &&
                Roll("curse-recoil", CurseRules.RecoilRange, attacker.Actor, defender.Actor) != 0)
            {
                int damage = CurseRules.RecoilDamage(strike.DamageForRecoil);
                ushort after = (ushort)Math.Max(0, attackerHp - damage);
                recoil = new(attacker.Actor, attacker.Actor, "curse-recoil", BattleReactionKind.CurseRecoil,
                    attackerHp, after, attacker.Mp, attacker.Mp, Amount: damage);
                reactions.Add(recoil);
                attackerHp = after;
            }
            bool canFollow = attackerHp > 0 && strike.Hp > 0;
            bool twice = canFollow && Roll("double", profile.Critical.DoubleChanceDenominator, attacker.Actor, defender.Actor) == 0;
            bool response = canFollow && Roll("counter", defender.Definition.Physical!.Critical.CounterChanceDenominator, attacker.Actor, defender.Actor) == 0;
            if (strike.Dodged) effects.Add(new("dodge", defender.Actor, hp, hp));
            else if (strike.Ineffective) effects.Add(new("ineffective", defender.Actor, hp, hp));
            else
            {
                if (strike.Critical) effects.Add(new("critical", attacker.Actor, Target: defender.Actor));
                effects.Add(new("hp", defender.Actor, hp, strike.Hp));
            }
            if (recoil is not null)
                effects.Add(new(recoil.Action, recoil.Actor, recoil.HpBefore, recoil.HpAfter, Target: recoil.Target));
            (actorHp, targetHp) = self ? (attackerHp, attackerHp) :
                counter ? (strike.Hp, attackerHp) : (attackerHp, strike.Hp);
            // WriteBattlesceneScript calls DropEnemyItem after each effect, before
            // second/counter dispatch and GiveExpAndGold. HP/status playback is later.
            var dropped = EnemyItemDropRules.Prepare(inventory.With(mainSeed: seed), attacker.Actor,
                defender.Actor, strike.Hp == 0);
            inventory = dropped.Battle; seed = inventory.MainSeed;
            effects.AddRange(dropped.Effects);
            reactions[impactIndex] = reactions[impactIndex] with { Receipt = dropped.Receipt };
            return (twice, response);
        }

        ushort Roll(string purpose, ushort range, ActorRef roller, ActorRef? subject = null)
        {
            var draw = BattleRandom.NextMain(seed, range);
            seed = draw.After;
            effects.Add(new("rng-" + purpose, roller, draw.Before, draw.After, range, draw.Value, subject));
            return draw.Value;
        }

        void AddRolls(IEnumerable<PhysicalRoll> rolls, ActorRef roller, ActorRef? targetActor = null)
        {
            foreach (var roll in rolls)
                effects.Add(new("rng-" + roll.Purpose, roller, roll.Before, roll.After,
                    roll.Range, roll.Result, targetActor));
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
            battle = action.ApplySleep(battle, reaction);
            battle = action.ApplySilence(battle, reaction);
            battle = action.ApplyPoison(battle, reaction);
            battle = action.ApplyStun(battle, reaction);
            deaths = deaths.Append(before, battle.GetActor(reaction.Target));
        }
        var reward = action.ApplyReward(battle, progression);
        var cleaned = deaths.Clean(reward.Battle, action.FirstAlly, progression);
        return (cleaned.Battle, Array.AsReadOnly<BattleEffect>([
            .. action.ConstructionEffects, .. reward.Effects, .. cleaned.Effects]));
    }

}
