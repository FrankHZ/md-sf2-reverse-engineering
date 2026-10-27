using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// EGRESS is a finite action, not an ordinary spell scene or a defeat shortcut.
internal sealed class Sf2RetreatAction : IBattleActionRule
{
    public string Identity => "sf2-retreat";
    public BattleActionKind Kind => BattleActionKind.Retreat;
    public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) =>
        Array.AsReadOnly(battle.GetActor(actor).Spells.Where(battle.Definition.RetreatSpells.ContainsKey)
            .Select(spell => new BattleActionOffer(new(Kind, spell), $"{spell.Value.ToUpperInvariant()} {spell.Level}")).ToArray());

    public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actor, BattleActionRef action)
    {
        var caster = battle.GetActor(actor);
        if (action.Kind != Kind || action.ItemSlot is not null || action.Spell is not { } spell ||
            !caster.Spells.Contains(spell)) throw new BattleRuleException("spell-not-known", "action");
        if (!battle.Definition.RetreatSpells.TryGetValue(spell, out var definition))
            throw new BattleRuleException("retreat-spell", "action", true);
        if (caster.Control != BattleControl.Player || !caster.IsAlly || caster.Hp == 0 ||
            caster.Position is null || (caster.Status & 0xC0) != 0)
            throw new BattleRuleException("not-player-control", "actor");
        if (caster.Mp < definition.MpCost) throw new BattleRuleException("insufficient-mp", "actor.mp");
        // The source special branch bypasses CastSpell: no inferred Silence test.
        return new(0, 0, Retreat: definition);
    }

    public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) => [actor];
    public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef target)
    {
        _ = RequireAction(battle, actor, action);
        BattleMovement.RequireStop(battle, actor, destination);
        if (target != actor) throw new BattleRuleException("invalid-retreat-target", "target");
        return battle.GetActor(actor);
    }

    public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, IBattleProgressionRule progression)
    {
        _ = RequireTarget(battle, actor, destination, action,
            target ?? throw new BattleRuleException("invalid-retreat-target", "target"));
        var moved = battle.With(actors: battle.Actors.Select(value => value.Actor == actor ? value.With(position: destination) : value));
        var definition = RequireAction(battle, actor, action).Retreat!;
        return new(moved, actor, destination, [], null, [], [], Retreat: new SpellRetreatPayment(definition.Spell, definition.MpCost));
    }
}
