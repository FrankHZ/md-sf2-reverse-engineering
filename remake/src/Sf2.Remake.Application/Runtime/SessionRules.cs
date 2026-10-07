using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Application.Gameplay;

namespace Sf2.Remake.Application.Runtime;

// Composition is selected before start and retained for the life of the session.
public sealed class SessionRules
{
    internal SessionRules(string identity, IHealingRule healing)
        : this(identity, healing, RuleCompositions.SourcePhysical(), RuleCompositions.SourceItem(), RuleCompositions.SourceStay()) { }

    internal SessionRules(string identity, IHealingRule healing, IPhysicalActionRule physical,
        IBattleActionRule item, IBattleActionRule stay)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identity);
        ArgumentNullException.ThrowIfNull(healing);
        ArgumentNullException.ThrowIfNull(physical); ArgumentNullException.ThrowIfNull(item); ArgumentNullException.ThrowIfNull(stay);
        if (physical.Kind != BattleActionKind.Physical || item.Kind != BattleActionKind.Item || stay.Kind != BattleActionKind.Stay)
            throw new ArgumentException("Invalid action rule binding.");
        Identity = identity; Healing = healing; Physical = physical;
        Actions = Array.AsReadOnly<IBattleActionRule>([physical, new HealingAction(healing), item, stay]);
    }
    public string Identity { get; }
    internal IHealingRule Healing { get; }
    internal IPhysicalActionRule Physical { get; }
    internal IReadOnlyList<IBattleActionRule> Actions { get; }
    internal IBattleActionRule Action(BattleActionRef action) => Actions.Single(rule => rule.Kind == action.Kind);

    // The accepted spell strategy participates in the same finite action contract.
    // There is one selected calculator, shared by query, confirmation and scene preparation.
    private sealed class HealingAction(IHealingRule healing) : IBattleActionRule
    {
        public string Identity => healing.Identity;
        public BattleActionKind Kind => BattleActionKind.Healing;
        public IReadOnlyList<BattleActionOffer> QueryActions(EngineBattleState battle, ActorRef actor) =>
            Array.AsReadOnly(battle.GetActor(actor).Spells.Select(spell =>
                new BattleActionOffer(new(Kind, spell), $"{spell.Value.ToUpperInvariant()} {spell.Level}")).ToArray());
        public BattleActionAdmission RequireAction(EngineBattleState battle, ActorRef actor, BattleActionRef action)
        {
            if (action.Kind != Kind || action.Spell is not { } spellRef || action.ItemSlot is not null)
                throw new BattleRuleException("select-spell", "action");
            var spell = healing.RequireSpell(battle, actor, spellRef);
            if (!battle.GetActor(actor).Spells.Contains(spellRef) ||
                !battle.Definition.Spells.TryGetValue(spellRef, out var admitted) || spell != admitted)
                throw new InvalidOperationException("Invalid admitted spell.");
            if (spellRef.Level is < 1 or > 3) throw new BattleRuleException("healing-animation", "spell.level", true);
            if (battle.GetActor(actor).Mp < spell.MpCost) throw new BattleRuleException("insufficient-mp", "actor.mp");
            return new(spell.MinimumRange, spell.MaximumRange, Spell: spell);
        }
        public IReadOnlyList<ActorRef> QueryTargets(EngineBattleState battle, ActorRef actor, BattleActionRef action) =>
            healing.QueryTargets(battle, actor);
        public BattleActorState RequireTarget(EngineBattleState battle, ActorRef actor, MapPosition destination,
            BattleActionRef action, ActorRef target) => healing.RequireTarget(battle, actor, destination,
                RequireAction(battle, actor, action).Spell!, target);
        public BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor, MapPosition destination,
            BattleActionRef action, ActorRef? target) => healing.Prepare(battle, actor, destination,
                action.Spell!.Value, target ?? throw new BattleRuleException("invalid-heal-target", "target"));
    }
}
