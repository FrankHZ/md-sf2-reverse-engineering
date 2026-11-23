using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed class Sf2AttackSpellAction : IBattleActionRule
{
    public string Identity => "sf2-attack-spell";
    public BattleActionKind Kind => BattleActionKind.AttackSpell;
    public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) =>
        battle.GetActor(actor).Spells.Where(key =>
            battle.Definition.AttackSpells.TryGetValue(key, out var spell) && !spell.IsMonsterBreath)
            .Select(spell => new BattleActionOffer(new(Kind, spell), $"{spell.Value.ToUpperInvariant()} {spell.Level}")).ToArray();

    public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actorRef, BattleActionRef action)
    {
        var actor = battle.GetActor(actorRef);
        bool sword = TarosEffectiveness.IsSwordUse(actor, action);
        if (action.Kind != Kind || action.Spell is not { } key || !sword && (action.ItemSlot is not null || !actor.Spells.Contains(key)))
            throw new BattleRuleException("spell-not-known", "spell");
        if (!battle.Definition.AttackSpells.TryGetValue(key, out var spell))
            throw new BattleRuleException("spell-effect", "spell", true);
        spell.RequireSupported(enemy: !actor.IsAlly);
        if (actor.Hp == 0 || (actor.IsAlly ? actor.Control != BattleControl.Player :
            actor.Control != BattleControl.Automatic || actor.Definition.ClassRule != BattleClassRule.Ordinary ||
            key.Value == "bolt" && !sword))
            throw new BattleRuleException("attack-spell-caster", "actor", true);
        RequireDamageActor(battle, actor);
        if (!sword && actor.Mp < spell.MpCost) throw new BattleRuleException("insufficient-mp", "actor.mp");
        return new(spell.MinimumRange, spell.MaximumRange, AttackSpell: spell);
    }

    internal static void RequireDamageActor(EngineBattleState battle, BattleActorState actor)
    {
        if (!SilenceRules.SupportedInBattle(actor.Status, battle.Definition.Poison) || (actor.Status & 0xC0) != 0)
            throw new BattleRuleException("attack-spell-status", "actor.status", true);
        if (actor.Definition.Physical is null)
            throw new BattleRuleException("physical-definition", "actor.physical", true);
        if (battle.Definition.Rewards is null)
            throw new BattleRuleException("battle-rewards", "encounter.rewards", true);
        if (actor.IsAlly && actor.Exp is null) throw new BattleRuleException("unspecified-exp", "actor.exp", true);
    }

    private static bool Eligible(BattleActorState actor, BattleActorState target, OffensiveTargetSide side) =>
        BattleOffense.Matches(actor, target, side) && target.Initialized && target.Hp > 0 && target.Position is not null &&
        (target.ActivationWord & 8) != 8;

    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) =>
        TargetChoices(battle, actor);

    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action, OffensiveTargetSide side) =>
        TargetChoices(battle, actor, side);

    internal static IReadOnlyList<ActorRef> TargetChoices(EngineBattleState battle, ActorRef actor,
        OffensiveTargetSide side = OffensiveTargetSide.Opponents) =>
        BattleOffense.OrderedRoster(battle, battle.GetActor(actor), side)
            .Where(target => Eligible(battle.GetActor(actor), target, side)).Select(target => target.Actor).ToArray();

    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target) =>
        RequireTarget(battle, actor, destination, action, target, OffensiveTargetSide.Opponents);
    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target, OffensiveTargetSide side)
    {
        var spell = RequireAction(battle, actor, action).AttackSpell!;
        _ = ResolveTargets(battle, actor, destination, spell, target, side);
        return battle.GetActor(target);
    }

    internal static IReadOnlyList<BattleActorState> ResolveTargets(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, AttackSpellDefinition spell, ActorRef center, OffensiveTargetSide side = OffensiveTargetSide.Opponents)
    {
        var actor = battle.GetActor(actorRef);
        var target = battle.Actors.SingleOrDefault(unit => unit.Actor == center);
        if (target is null || !Eligible(actor, target, side)) throw new BattleRuleException("attack-spell-target", "target");
        bool converted = spell.IsMonsterBreath && side == OffensiveTargetSide.OwnSide;
        if (!converted && !BattleRange.Contains(destination, target.Position!, spell.MinimumRange, spell.MaximumRange))
            throw new BattleRuleException("target-range", "target");
        var centerPosition = center == actorRef ? destination : target.Position!;
        var targets = Area(BattleOffense.OrderedRoster(battle, actor, side), actor, destination, centerPosition, spell.Radius, side)
            .OrderBy(unit => unit.ProcessingOrder).ToArray();
        foreach (var unit in targets)
        {
            if (!SilenceRules.SupportedInBattle(unit.Status, battle.Definition.Poison))
                throw new BattleRuleException("attack-spell-target-status", "actors.status", true);
            if (unit.Definition.Physical is null) throw new BattleRuleException("physical-definition", "target.physical", true);
        }
        return targets;
    }

    internal static IEnumerable<BattleActorState> Area(IEnumerable<BattleActorState> roster, BattleActorState actor,
        MapPosition actorPosition, MapPosition center, int radius, OffensiveTargetSide side) =>
        roster.Where(unit => Eligible(actor, unit, side) &&
            BattleRange.Contains(center, unit.Actor == actor.Actor ? actorPosition : unit.Position!, 0, radius));

    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actorRef, MapPosition destination,
        BattleActionRef action, ActorRef? center, IBattleProgressionRule progression) =>
        Prepare(battle, actorRef, destination, action, center, progression, OffensiveTargetSide.Opponents);
    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actorRef, MapPosition destination,
        BattleActionRef action, ActorRef? center, IBattleProgressionRule progression, OffensiveTargetSide side)
    {
        var spell = RequireAction(battle, actorRef, action).AttackSpell!;
        return PrepareAttack(battle, actorRef, destination, spell, center, progression, action.ItemSlot, side: side);
    }

    internal static BattleActionResolution PrepareAttack(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, AttackSpellDefinition spell, ActorRef? center, IBattleProgressionRule progression,
        int? itemSlot, AttackItemDefinition? item = null, OffensiveTargetSide side = OffensiveTargetSide.Opponents)
    {
        BattleMovement.RequireStop(battle, actorRef, destination);
        var actor = battle.GetActor(actorRef);
        var targets = ResolveTargets(battle, actorRef, destination, spell,
            center ?? throw new BattleRuleException("attack-spell-target", "target"), side);
        string reactionAction = item is null ? "attack-spell" : "attack-item";
        uint seed = battle.MainSeed;
        var inventory = battle;
        List<BattleEffect> effects = [];
        List<BattleReaction> reactions = [];
        uint? gold = battle.Gold;
        // DetermineIneffectiveAttack reads the first sorted target once. InflictDamage
        // consults that toggle only for targets with the Taros enemy definition.
        var taros = battle.Definition.TarosEffectiveness;
        bool ineffective = !spell.IsInstantDeath && taros?.IsTaros(targets[0]) == true;
        foreach (var target in targets)
        {
            var reaction = spell.IsInstantDeath ? ResolveInstantDeath(target) : ResolveDamage(target);
            ushort hp = reaction.HpAfter;
            BattleOffense.RequireDeathAccounting(battle, reaction);
            if (BattleOffense.PaysGold(battle, reaction))
            {
                gold = BattleProgressionRules.Invoke(progression.Identity, "gold", () => progression.Gold(
                    gold ?? throw new BattleRuleException("unspecified-gold", "battle.gold", true), target.Definition.Physical!.Gold));
            }
            // The ordinary source post-effect drop helper remains ordered before the next target and award.
            var dropped = EnemyItemDropRules.Prepare(inventory.With(mainSeed: seed), actorRef, target.Actor, hp == 0);
            inventory = dropped.Battle; seed = inventory.MainSeed;
            effects.AddRange(dropped.Effects);
            reactions.Add(reaction with { Receipt = dropped.Receipt });
        }
        BattleOutcomeSelection.RequireContinuing(battle, reactions);
        EquipmentItemUse? equipmentUse = null;
        if (item is not null)
        {
            // Every drop sees the pre-payment pack, including the selected slot.
            int slot = itemSlot!.Value;
            var holder = inventory.GetActor(actorRef);
            if (item.Breakable)
            {
                var wear = EquipmentItemUse.Apply(slot, holder, seed, effects);
                equipmentUse = wear.Use;
                holder = wear.Owner;
                seed = wear.Seed;
            }
            else
            {
                effects.Add(new("item-consumed", actorRef, holder.SourceLoadout!.Items[slot], slot));
                holder = BattleEquipment.RemoveItem(holder, slot);
            }
            inventory = inventory.With(actors: inventory.Actors.Select(unit => unit.Actor == actorRef ? holder : unit));
        }
        bool earnsExp = actor.IsAlly && BattleOffense.Survives(actor, reactions);
        var award = earnsExp ? BattleProgressionRules.Award(progression, inventory.With(mainSeed: seed),
            inventory.GetActor(actorRef), BattleActionKind.AttackSpell, reactions)
            : (Amount: 0, Seed: seed, Effects: (IReadOnlyList<BattleEffect>)Array.Empty<BattleEffect>());
        effects.AddRange(award.Effects);
        if (gold != battle.Gold) effects.Add(new("gold", actorRef, battle.Gold, gold));
        var prepared = inventory.With(mainSeed: award.Seed, gold: gold,
            actors: inventory.Actors.Select(unit => unit.Actor == actorRef ? unit.With(position: destination) : unit));
        return new(prepared, actorRef, destination, reactions.AsReadOnly(), earnsExp ? new(actorRef, award.Amount) : null,
            effects.AsReadOnly(), [], AttackSpell: spell, AttackItemSlot: itemSlot, AttackItem: item, EquipmentUse: equipmentUse);

        BattleReaction ResolveInstantDeath(BattleActorState target)
        {
            bool hit = AttackSpellDefinition.InstantDeathSucceeds(target, Roll(8, "desoul", target.Actor));
            return new(actorRef, target.Actor, reactionAction,
                hit ? BattleReactionKind.InstantDeath : BattleReactionKind.Ineffective,
                target.Hp, hit ? (ushort)0 : target.Hp, target.Mp, target.Mp, false, 0, target.Status);
        }

        BattleReaction ResolveDamage(BattleActorState target)
        {
            int power = SpellPower(actor, spell, itemSlot.HasValue);
            int quarter = power / 4;
            int damage = ResistedDamage(target, spell, power);
            bool critical = Roll(spell.CriticalRange, "spell-critical", target.Actor) == 0;
            if (ineffective && taros!.IsTaros(target))
                return new(actorRef, target.Actor, reactionAction, BattleReactionKind.Ineffective,
                    target.Hp, target.Hp, target.Mp, target.Mp, false, 0, target.Status);
            if (critical) damage += quarter;
            ushort range = checked((ushort)(damage / 8 + 1));
            damage = Math.Max(1, damage - Roll(range, "spell-variance", target.Actor) - Roll(range, "spell-variance", target.Actor));
            ushort hp = (ushort)Math.Max(0, target.Hp - damage);
            return new(actorRef, target.Actor, reactionAction, BattleReactionKind.Damage,
                target.Hp, hp, target.Mp, target.Mp, critical, damage, target.Status);
        }

        int Roll(ushort range, string purpose, ActorRef target)
        {
            var draw = BattleRandom.NextMain(seed, range); seed = draw.After;
            effects.Add(new("rng-" + purpose, actorRef, draw.Before, draw.After, range, draw.Value, target));
            return draw.Value;
        }
    }

    private static int SpellPower(BattleActorState actor, AttackSpellDefinition spell, bool item) =>
        item ? spell.Power : actor.Definition.CastPower(spell.Power);

    internal static int PotentialDamage(BattleActorState actor, BattleActorState target, AttackSpellDefinition spell, bool rawPower = false) =>
        ResistedDamage(target, spell, SpellPower(actor, spell, rawPower));

    private static int ResistedDamage(BattleActorState target, AttackSpellDefinition spell, int power)
    {
        int quarter = power / 4;
        int resistance = target.Definition.Resistance >> (int)spell.Element & 3;
        return resistance switch { 1 => power - quarter, 2 => power / 2, 3 => power + quarter, _ => power };
    }
}
