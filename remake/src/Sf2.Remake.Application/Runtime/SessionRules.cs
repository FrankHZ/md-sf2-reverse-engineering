using System.Collections.ObjectModel;
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
        IBattleActionRule item, IBattleActionRule stay,
        IEnumerable<KeyValuePair<BattleStrategyRef, IBattleDecisionRule>>? decisions = null,
        IBattleProgressionRule? progression = null, IBattleOutcomeRule? outcome = null,
        Exploration.IOutcomeReturnPolicy? outcomeReturn = null, Exploration.ISourceStoryPolicy? sourceStory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identity);
        ArgumentNullException.ThrowIfNull(healing);
        ArgumentNullException.ThrowIfNull(physical); ArgumentNullException.ThrowIfNull(item); ArgumentNullException.ThrowIfNull(stay);
        if (physical.Kind != BattleActionKind.Physical || item.Kind != BattleActionKind.Item || stay.Kind != BattleActionKind.Stay)
            throw new ArgumentException("Invalid action rule binding.");
        var resolved = new Dictionary<BattleStrategyRef, IBattleDecisionRule>();
        foreach (var binding in decisions ?? RuleCompositions.SourceDecisions())
        {
            if (string.IsNullOrWhiteSpace(binding.Key.Value) || binding.Value is null ||
                string.IsNullOrWhiteSpace(binding.Value.Identity)) _bindingFailure = "ai-binding";
            else if (!resolved.TryAdd(binding.Key, binding.Value)) _bindingFailure = "duplicate-ai-binding";
        }
        _decisions = new ReadOnlyDictionary<BattleStrategyRef, IBattleDecisionRule>(resolved);
        Identity = identity; Healing = healing; Physical = physical;
        Progression = progression ?? RuleCompositions.SourceProgression();
        Outcome = outcome ?? RuleCompositions.SourceOutcome();
        OutcomeReturn = outcomeReturn ?? RuleCompositions.SourceOutcomeReturn();
        SourceStory = sourceStory ?? RuleCompositions.SourceStory();
        Actions = Array.AsReadOnly<IBattleActionRule>([physical, new HealingAction(healing), item, stay]);
    }
    internal IBattleProgressionRule Progression { get; }
    internal IBattleOutcomeRule Outcome { get; }
    internal Exploration.IOutcomeReturnPolicy OutcomeReturn { get; }
    internal Exploration.ISourceStoryPolicy SourceStory { get; }
    public string Identity { get; }
    internal IHealingRule Healing { get; }
    internal IPhysicalActionRule Physical { get; }
    internal IReadOnlyList<IBattleActionRule> Actions { get; }
    internal IBattleActionRule Action(BattleActionRef action) => Actions.Single(rule => rule.Kind == action.Kind);

    private readonly IReadOnlyDictionary<BattleStrategyRef, IBattleDecisionRule> _decisions;
    // Retain configuration errors for the typed session-start boundary, before any publication.
    private readonly string? _bindingFailure;

    internal SessionRules Bind(IEnumerable<BattleDefinition> encounters, IEnumerable<BattleActorStartInput>? inputs = null)
    {
        if (_bindingFailure is { } code) throw new BattleRuleException(code, "rules.decisions", true);
        foreach (var encounter in encounters)
            foreach (var deployment in encounter.Deployments)
            {
                BattleTurnFlow.ValidateDeployment(deployment);
                if (deployment.Control != BattleControl.Automatic) continue;
                var rule = Decision(deployment.AiStrategy!.Value);
                BattleDecisionRules.Invoke(rule, "deployment-admission", () =>
                { rule.RequireDeployment(deployment); return true; });
                if (inputs?.FirstOrDefault(input => input.Actor == deployment.Actor) is { } input)
                    RequireStart(deployment, input);
            }
        return this;
    }

    internal IBattleDecisionRule Decision(BattleStrategyRef strategy) =>
        _decisions.TryGetValue(strategy, out var rule) ? rule :
            throw new BattleRuleException("ai-strategy", "placements.aiStrategy", true);

    internal void RequireStart(BattleDeploymentDefinition deployment, BattleActorStartInput input)
    {
        if (deployment.Control != BattleControl.Automatic) return;
        var rule = Decision(deployment.AiStrategy!.Value);
        BattleDecisionRules.Invoke(rule, "start-admission", () =>
        { rule.RequireStart(deployment, input); return true; });
    }

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
            BattleActionRef action, ActorRef? target, IBattleProgressionRule progression) => healing.Prepare(battle, actor, destination,
                action.Spell!.Value, target ?? throw new BattleRuleException("invalid-heal-target", "target"), progression);
    }
}
