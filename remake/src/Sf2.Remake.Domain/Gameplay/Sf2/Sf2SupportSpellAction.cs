using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Gameplay.Sf2;

internal sealed class Sf2SupportSpellAction : IBattleActionRule
{
    public string Identity => "sf2-dispel";
    public BattleActionKind Kind => BattleActionKind.SupportSpell;
    public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) =>
        battle.GetActor(actor).Spells.Where(battle.Definition.SupportSpells.ContainsKey)
            .Select(s => new BattleActionOffer(new(Kind, s), $"{s.Value.ToUpperInvariant()} {s.Level}")).ToArray();
    public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actorRef, BattleActionRef action)
    {
        var actor = battle.GetActor(actorRef);
        if (action.Kind != Kind || action.ItemSlot is not null || action.Spell is not { } key || !actor.Spells.Contains(key))
            throw new BattleRuleException("spell-not-known", "spell");
        if (!battle.Definition.SupportSpells.TryGetValue(key, out var spell)) throw new BattleRuleException("spell-effect", "spell", true);
        spell.RequireSupported();
        if (actor.Hp == 0 || actor.Position is null || !SilenceRules.Supported(actor.Status, battle.Definition.Poison) ||
            (actor.Status & 0xC0) != 0 || (!actor.IsAlly && actor.Control != BattleControl.Automatic))
            throw new BattleRuleException("support-spell-caster", "actor", true);
        if (battle.Definition.Rewards is null) throw new BattleRuleException("battle-rewards", "encounter.rewards", true);
        if (actor.IsAlly && actor.Exp is null) throw new BattleRuleException("unspecified-exp", "actor.exp", true);
        if (actor.Mp < spell.MpCost) throw new BattleRuleException("insufficient-mp", "actor.mp");
        return new(spell.MinimumRange, spell.MaximumRange, SupportSpell: spell);
    }
    private static bool Eligible(BattleActorState actor, BattleActorState target) => target.Faction != actor.Faction &&
        target.Initialized && target.Hp > 0 && target.Position is not null && (target.ActivationWord & 8) != 8;
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) =>
        battle.Actors.Where(a => Eligible(battle.GetActor(actor), a)).OrderBy(a => a.ProcessingOrder).Select(a => a.Actor).ToArray();
    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target)
    {
        _ = Targets(battle, actor, destination, RequireAction(battle, actor, action).SupportSpell!, target);
        return battle.GetActor(target);
    }
    internal static IReadOnlyList<BattleActorState> Targets(EngineBattleState battle, ActorRef actor,
        MapPosition destination, SupportSpellDefinition spell, ActorRef center)
    {
        var caster = battle.GetActor(actor); var target = battle.Actors.SingleOrDefault(a => a.Actor == center);
        if (target is null || !Eligible(caster, target)) throw new BattleRuleException("support-spell-target", "target");
        if (!BattleRange.Contains(destination, target.Position!, spell.MinimumRange, spell.MaximumRange))
            throw new BattleRuleException("target-range", "target");
        var targets = battle.Actors.Where(a => Eligible(caster, a) && BattleRange.Contains(target.Position!, a.Position!, 0, spell.Radius))
            .OrderBy(a => a.ProcessingOrder).ToArray();
        if (targets.Any(a => !SilenceRules.Supported(a.Status, battle.Definition.Poison)))
            throw new BattleRuleException("support-spell-target-status", "actors.status", true);
        return targets;
    }
    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? center, IBattleProgressionRule progression)
    {
        var spell = RequireAction(battle, actor, action).SupportSpell!;
        var targets = Targets(battle, actor, destination, spell, center ?? throw new BattleRuleException("support-spell-target", "target"));
        uint seed = battle.MainSeed; List<BattleEffect> effects = []; List<BattleReaction> reactions = [];
        foreach (var target in targets)
        {
            var draw = BattleRandom.NextMain(seed, 8); seed = draw.After;
            effects.Add(new("rng-dispel", actor, draw.Before, draw.After, 8, draw.Value, target.Actor));
            int threshold = SilenceRules.HasSpells(target) ? 5 + (target.Definition.Resistance >> 14 & 3) : 8;
            bool success = draw.Value >= threshold;
            reactions.Add(new(actor, target.Actor, "support-spell", success ? BattleReactionKind.Status : BattleReactionKind.Ineffective,
                target.Hp, target.Hp, target.Mp, target.Mp, StatusWrite: success ? (ushort)(target.Status | SilenceRules.Mask) : null));
        }
        var caster = battle.GetActor(actor);
        var award = caster.IsAlly ? BattleProgressionRules.Award(progression, battle.With(mainSeed: seed), caster, Kind, reactions)
            : (Amount: 0, Seed: seed, Effects: (IReadOnlyList<BattleEffect>)Array.Empty<BattleEffect>());
        effects.AddRange(award.Effects);
        return new(battle.With(mainSeed: award.Seed, actors: battle.Actors.Select(a => a.Actor == actor ? a.With(position: destination) : a)),
            actor, destination, reactions.AsReadOnly(), caster.IsAlly ? new(actor, award.Amount) : null, effects.AsReadOnly(), [], SupportSpell: spell);
    }
}
