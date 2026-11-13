using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed class Sf2AttackSpellAction : IBattleActionRule
{
    public string Identity => "sf2-attack-spell";
    public BattleActionKind Kind => BattleActionKind.AttackSpell;
    public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) =>
        battle.GetActor(actor).Spells.Where(battle.Definition.AttackSpells.ContainsKey)
            .Select(spell => new BattleActionOffer(new(Kind, spell), $"{spell.Value.ToUpperInvariant()} {spell.Level}")).ToArray();

    public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actorRef, BattleActionRef action)
    {
        var actor = battle.GetActor(actorRef);
        if (action.Kind != Kind || action.ItemSlot is not null || action.Spell is not { } key || !actor.Spells.Contains(key))
            throw new BattleRuleException("spell-not-known", "spell");
        if (!battle.Definition.AttackSpells.TryGetValue(key, out var spell))
            throw new BattleRuleException("spell-effect", "spell", true);
        spell.RequireSupported(enemy: !actor.IsAlly);
        if (actor.Hp == 0 || (actor.IsAlly ? actor.Control != BattleControl.Player :
            actor.Control != BattleControl.Automatic || actor.Definition.ClassRule != BattleClassRule.Ordinary ||
                key is not ({ Value: "blaze" or "freeze" or "blast" or "aqua", Level: 1 } or { Value: "blaze", Level: 2 })))
            throw new BattleRuleException("attack-spell-caster", "actor", true);
        if (!(SilenceRules.Supported(actor.Status, battle.Definition.Poison) && (actor.Status & 0xC0) == 0))
            throw new BattleRuleException("attack-spell-status", "actor.status", true);
        if (actor.Definition.Physical is null)
            throw new BattleRuleException("physical-definition", "actor.physical", true);
        if (battle.Definition.Rewards is null)
            throw new BattleRuleException("battle-rewards", "encounter.rewards", true);
        if (actor.IsAlly && actor.Exp is null) throw new BattleRuleException("unspecified-exp", "actor.exp", true);
        if (actor.Mp < spell.MpCost) throw new BattleRuleException("insufficient-mp", "actor.mp");
        return new(spell.MinimumRange, spell.MaximumRange, AttackSpell: spell);
    }

    private static bool Eligible(BattleActorState actor, BattleActorState target) =>
        target.Faction != actor.Faction && target.Initialized && target.Hp > 0 && target.Position is not null &&
        (target.ActivationWord & 8) != 8;

    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) =>
        battle.Actors.Where(target => Eligible(battle.GetActor(actor), target))
            .OrderBy(target => target.ProcessingOrder).Select(target => target.Actor).ToArray();

    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target)
    {
        var spell = RequireAction(battle, actor, action).AttackSpell!;
        _ = ResolveTargets(battle, actor, destination, spell, target);
        return battle.GetActor(target);
    }

    internal static IReadOnlyList<BattleActorState> ResolveTargets(EngineBattleState battle, ActorRef actorRef,
        MapPosition destination, AttackSpellDefinition spell, ActorRef center)
    {
        var actor = battle.GetActor(actorRef);
        var target = battle.Actors.SingleOrDefault(unit => unit.Actor == center);
        if (target is null || !Eligible(actor, target)) throw new BattleRuleException("attack-spell-target", "target");
        if (!BattleRange.Contains(destination, target.Position!, spell.MinimumRange, spell.MaximumRange))
            throw new BattleRuleException("target-range", "target");
        var targets = battle.Actors.Where(unit => Eligible(actor, unit) &&
            BattleRange.Contains(target.Position!, unit.Position!, 0, spell.Radius)).OrderBy(unit => unit.ProcessingOrder).ToArray();
        foreach (var unit in targets)
        {
            if (!PoisonTurnRules.SupportedPhysicalStatus(unit.Status) || !battle.Definition.Poison && (unit.Status & 2) != 0)
                throw new BattleRuleException("attack-spell-target-status", "actors.status", true);
            if (unit.Definition.Physical is null) throw new BattleRuleException("physical-definition", "target.physical", true);
        }
        return targets;
    }

    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actorRef, MapPosition destination,
        BattleActionRef action, ActorRef? center, IBattleProgressionRule progression)
    {
        BattleMovement.RequireStop(battle, actorRef, destination);
        var spell = RequireAction(battle, actorRef, action).AttackSpell!;
        var actor = battle.GetActor(actorRef);
        var targets = ResolveTargets(battle, actorRef, destination, spell,
            center ?? throw new BattleRuleException("attack-spell-target", "target"));
        uint seed = battle.MainSeed;
        var inventory = battle;
        List<BattleEffect> effects = [];
        List<BattleReaction> reactions = [];
        uint? gold = battle.Gold;
        // DetermineIneffectiveAttack reads the first sorted target once. InflictDamage
        // consults that toggle only for targets with the Taros enemy definition.
        var taros = battle.Definition.TarosEffectiveness;
        bool ineffective = taros?.IsTaros(targets[0]) == true;
        foreach (var target in targets)
        {
            int power = actor.Definition.Physical!.Promoted ? spell.Power * 5 / 4 : spell.Power;
            int quarter = power / 4;
            int damage = PotentialDamage(actor, target, spell);
            bool critical = Roll(spell.CriticalRange, "spell-critical", target.Actor) == 0;
            if (ineffective && taros!.IsTaros(target))
            {
                reactions.Add(new(actorRef, target.Actor, "attack-spell", BattleReactionKind.Ineffective,
                    target.Hp, target.Hp, target.Mp, target.Mp, false, 0, target.Status));
                continue;
            }
            if (critical) damage += quarter;
            ushort range = checked((ushort)(damage / 8 + 1));
            damage = Math.Max(1, damage - Roll(range, "spell-variance", target.Actor) - Roll(range, "spell-variance", target.Actor));
            ushort hp = (ushort)Math.Max(0, target.Hp - damage);
            if (hp == 0 && actor.IsAlly)
            {
                if (actor.Kills is null) throw new BattleRuleException("unspecified-kills", "actor.kills", true);
                gold = BattleProgressionRules.Invoke(progression.Identity, "gold", () => progression.Gold(
                    gold ?? throw new BattleRuleException("unspecified-gold", "battle.gold", true), target.Definition.Physical!.Gold));
            }
            if (hp == 0 && target.IsAlly && target.Defeats is null)
                throw new BattleRuleException("unspecified-defeats", "target.defeats", true);
            // The ordinary source post-effect drop helper remains ordered before the next target and award.
            var dropped = EnemyItemDropRules.Prepare(inventory.With(mainSeed: seed), actorRef, target.Actor, hp == 0);
            inventory = dropped.Battle; seed = inventory.MainSeed;
            effects.AddRange(dropped.Effects);
            reactions.Add(new(actorRef, target.Actor, "attack-spell", BattleReactionKind.Damage,
                target.Hp, hp, target.Mp, target.Mp, critical, damage, target.Status, Receipt: dropped.Receipt));
        }
        BattleOutcomeSelection.RequireContinuing(battle, reactions);
        var award = actor.IsAlly ? BattleProgressionRules.Award(progression, inventory.With(mainSeed: seed), inventory.GetActor(actorRef), Kind, reactions)
            : (Amount: 0, Seed: seed, Effects: (IReadOnlyList<BattleEffect>)Array.Empty<BattleEffect>());
        effects.AddRange(award.Effects);
        if (gold != battle.Gold) effects.Add(new("gold", actorRef, battle.Gold, gold));
        var prepared = inventory.With(mainSeed: award.Seed, gold: gold,
            actors: inventory.Actors.Select(unit => unit.Actor == actorRef ? unit.With(position: destination) : unit));
        return new(prepared, actorRef, destination, reactions.AsReadOnly(), actor.IsAlly ? new(actorRef, award.Amount) : null,
            effects.AsReadOnly(), [], AttackSpell: spell);

        int Roll(ushort range, string purpose, ActorRef target)
        {
            var draw = BattleRandom.NextMain(seed, range); seed = draw.After;
            effects.Add(new("rng-" + purpose, actorRef, draw.Before, draw.After, range, draw.Value, target));
            return draw.Value;
        }
    }

    internal static int PotentialDamage(BattleActorState actor, BattleActorState target, AttackSpellDefinition spell)
    {
        int power = actor.Definition.Physical!.Promoted ? spell.Power * 5 / 4 : spell.Power;
        int quarter = power / 4;
        int resistance = target.Definition.Resistance >> (int)spell.Element & 3;
        return resistance switch { 1 => power - quarter, 2 => power / 2, 3 => power + quarter, _ => power };
    }
}
