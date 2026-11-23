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
        if (spell.AbsorbsMp) throw new BattleRuleException("spell-effect", "spell", true);
        if (actor.Hp == 0 || actor.Position is null || !MuddleRules.SupportedRankOne(actor.Status, battle.Definition.Poison) ||
            (actor.Status & 0xC0) != 0 || (!actor.IsAlly && actor.Control != BattleControl.Automatic))
            throw new BattleRuleException("support-spell-caster", "actor", true);
        if (battle.Definition.Rewards is null) throw new BattleRuleException("battle-rewards", "encounter.rewards", true);
        if (actor.IsAlly && actor.Exp is null) throw new BattleRuleException("unspecified-exp", "actor.exp", true);
        if (actor.Mp < spell.MpCost) throw new BattleRuleException("insufficient-mp", "actor.mp");
        return new(spell.MinimumRange, spell.MaximumRange, SupportSpell: spell);
    }
    private static bool Eligible(BattleActorState actor, BattleActorState target, SupportSpellDefinition spell) => (target.Faction == actor.Faction) == spell.TargetsTeammates &&
        target.Initialized && target.Hp > 0 && target.Position is not null && (target.ActivationWord & 8) != 8;
    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) =>
        TargetChoices(battle, actor, battle.Definition.SupportSpells[action.Spell!.Value]);

    internal static IReadOnlyList<ActorRef> TargetChoices(EngineBattleState battle, ActorRef actor, SupportSpellDefinition spell)
    {
        var caster = battle.GetActor(actor);
        return battle.Actors.Where(target => Eligible(caster, target, spell) && (target.Actor != actor || spell.MinimumRange == 0))
            .OrderBy(target => target.ProcessingOrder).Select(target => target.Actor).ToArray();
    }
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
        if (target is null || !Eligible(caster, target, spell)) throw new BattleRuleException("support-spell-target", "target");
        MapPosition Position(BattleActorState unit) => unit.Actor == actor ? destination : unit.Position!;
        if (!BattleRange.Contains(destination, Position(target), spell.MinimumRange, spell.MaximumRange))
            throw new BattleRuleException("target-range", "target");
        var targets = battle.Actors.Where(a => Eligible(caster, a, spell) && BattleRange.Contains(Position(target), Position(a), 0, spell.Radius))
            .OrderBy(a => a.ProcessingOrder).ToArray();
        if (targets.Any(a => !MuddleRules.SupportedRankOne(a.Status, battle.Definition.Poison)))
            throw new BattleRuleException("support-spell-target-status", "actors.status", true);
        if (spell.Effect == SupportSpellEffect.Muddle &&
            targets.Any(a => !MuddleRules.CanTurn(battle.Definition, a.Deployment, a.SourceLoadout, a.ActivationWord)))
            throw new BattleRuleException("muddle-target-turn", "actors.control", true);
        return targets;
    }
    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? center, IBattleProgressionRule progression) =>
        PrepareEffect(battle, actor, destination, RequireAction(battle, actor, action).SupportSpell!, center, progression);

    internal static BattleActionResolution PrepareEffect(EngineBattleState battle, ActorRef actor, MapPosition destination,
        SupportSpellDefinition spell, ActorRef? center, IBattleProgressionRule progression,
        SupportItemDefinition? item = null, int? slot = null)
    {
        BattleMovement.RequireStop(battle, actor, destination);
        var targets = Targets(battle, actor, destination, spell, center ?? throw new BattleRuleException("support-spell-target", "target"));
        uint seed = battle.MainSeed; List<BattleEffect> effects = []; List<BattleReaction> reactions = [];
        Dictionary<ActorRef, ushort> writes = [];
        var caster = battle.GetActor(actor);
        if (spell.AbsorbsMp)
        {
            var target = targets.Single();
            var draw = BattleRandom.NextMain(seed, 3);
            seed = draw.After;
            effects.Add(new("rng-mp-absorb", actor, draw.Before, draw.After, 3, draw.Value, target.Actor));
            reactions.AddRange(SupportSpellDefinition.MpTransfer(caster, target, draw.Value));
        }
        else foreach (var target in targets)
        {
            int Roll(string kind)
            {
                var draw = BattleRandom.NextMain(seed, 8);
                seed = draw.After;
                effects.Add(new(kind, actor, draw.Before, draw.After, 8, draw.Value, target.Actor));
                return draw.Value;
            }
            var (reactionWord, constructionWord) = spell.Resolve(target, Roll);
            if (constructionWord.HasValue) writes.Add(target.Actor, constructionWord.Value);
            reactions.Add(new(actor, target.Actor, item is null ? "support-spell" : "item-use",
                reactionWord.HasValue ? BattleReactionKind.Status : BattleReactionKind.Ineffective,
                target.Hp, target.Hp, target.Mp, target.Mp, StatusWrite: reactionWord));
        }
        var prepared = battle.With(actors: battle.Actors.Select(a =>
        {
            if (writes.TryGetValue(a.Actor, out ushort word)) a = a.With(status: word);
            return a.Actor == actor ? a.With(position: destination) : a;
        }));
        EquipmentItemUse? equipmentUse = null;
        if (item is not null)
        {
            var wear = EquipmentItemUse.Apply(slot!.Value, prepared.GetActor(actor), seed, effects);
            equipmentUse = wear.Use;
            seed = wear.Seed;
            prepared = prepared.With(actors: prepared.Actors.Select(a => a.Actor == actor ? wear.Owner : a));
        }
        var award = caster.IsAlly ? BattleProgressionRules.Award(progression, battle.With(mainSeed: seed), caster,
            BattleActionKind.SupportSpell, reactions)
            : (Amount: 0, Seed: seed, Effects: (IReadOnlyList<BattleEffect>)Array.Empty<BattleEffect>());
        effects.AddRange(award.Effects);
        return new(prepared.With(mainSeed: award.Seed),
            actor, destination, reactions.AsReadOnly(), caster.IsAlly ? new(actor, award.Amount) : null, effects.AsReadOnly(), [],
            SupportSpell: spell, CasterCostStatus: item is null ? caster.Status : null, EquipmentUse: equipmentUse);
    }
}
