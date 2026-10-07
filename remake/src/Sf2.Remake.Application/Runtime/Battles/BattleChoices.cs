using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

public enum BattlePresentationKind { Healing }
public sealed record BattleTargetChoice(ActorRef Actor, bool Enabled, SessionFailure? Reason);
public sealed record BattleSpellChoice(SpellRef Spell, string Label, BattlePresentationKind Presentation,
    byte? MinimumRange, byte? MaximumRange, bool Enabled, SessionFailure? Reason,
    IReadOnlyList<BattleTargetChoice> Targets);
public sealed record BattleChoicesSnapshot(Guid SessionId, long Revision, ActorRef? Actor,
    BattleSelectionStage? Stage, MapPosition? Origin, IReadOnlyList<BattleSpellChoice> Spells,
    SessionFailure? Failure = null);

// Only HEAL is migrated here. Item/physical/Stay projection belongs to slice 2.
internal static class BattleChoices
{
    internal static T Invoke<T>(SessionRules rules, string operation, Func<T> call)
    {
        try { return call(); }
        catch (BattleRuleException) { throw; }
        catch (Exception) { throw new HealingRuleFault(rules.Healing.Identity, operation); }
    }

    internal static SessionFailure Failure(BattleRuleException error) =>
        new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.IllegalCommand,
            error.Code, error.Field, error.Code.Replace('-', ' '));

    internal static HealingSpellDefinition RequireSpell(SessionRules rules, EngineBattleState battle,
        ActorRef actor, SpellRef spellRef) => Invoke(rules, "spell-admission", () =>
    {
        var spell = rules.Healing.RequireSpell(battle, actor, spellRef);
        if (!battle.GetActor(actor).Spells.Contains(spellRef) ||
            !battle.Definition.Spells.TryGetValue(spellRef, out var admitted) || spell != admitted)
            throw new InvalidOperationException("Invalid admitted spell.");
        if (spellRef.Level is < 1 or > 3)
            throw new BattleRuleException("healing-animation", "spell.level", true);
        if (battle.GetActor(actor).Mp < spell.MpCost)
            throw new BattleRuleException("insufficient-mp", "actor.mp");
        return spell;
    });

    internal static BattleActorState RequireTarget(SessionRules rules, EngineBattleState battle, ActorRef actor,
        MapPosition destination, HealingSpellDefinition spell, ActorRef target, BattleSceneDefinition? content,
        bool requireContent) => Invoke(rules, "target-admission", () =>
    {
        var patient = rules.Healing.RequireTarget(battle, actor, destination, spell, target);
        if (!ReferenceEquals(patient, battle.GetActor(target)))
            throw new InvalidOperationException("Invalid admitted target.");
        HealingSceneCursor.RequireContent(battle, actor, target, content, requireContent);
        return patient;
    });

    internal static BattleChoicesSnapshot Query(SessionSnapshot current, SessionRules rules,
        BattleSceneDefinition? content, bool requireContent)
    {
        var selection = current.Selection;
        if (!current.HasBattleControl || selection is null)
            return new(current.SessionId, current.Revision, null, null, null, [],
                new(SessionFailureKind.IllegalCommand, "not-player-control", "phase", "not player control"));
        var battle = current.Battle;
        List<BattleSpellChoice> options = [];
        foreach (var spellRef in battle.GetActor(selection.Actor).Spells)
        {
            SessionFailure? failure = null;
            HealingSpellDefinition? spell = null;
            List<BattleTargetChoice> targets = [];
            try
            {
                BattleMovement.RequireStop(battle, selection.Actor, selection.Preview.Destination);
                spell = RequireSpell(rules, battle, selection.Actor, spellRef);
                var candidates = Invoke(rules, "target-query", () =>
                {
                    var result = rules.Healing.QueryTargets(battle, selection.Actor).ToArray();
                    if (result.Distinct().Count() != result.Length || result.Any(actor => !battle.Actors.Any(row => row.Actor == actor)))
                        throw new InvalidOperationException("Invalid target query.");
                    return result;
                });
                foreach (var actor in candidates)
                {
                    SessionFailure? reason = null;
                    try { _ = RequireTarget(rules, battle, selection.Actor, selection.Preview.Destination,
                        spell, actor, content, requireContent); }
                    catch (BattleRuleException error) { reason = Failure(error); }
                    catch (HealingRuleFault error) { reason = error.Failure; }
                    targets.Add(new(actor, reason is null, reason));
                }
                if (!targets.Any(target => target.Enabled))
                    failure = targets.FirstOrDefault(target => target.Reason?.Kind == SessionFailureKind.UnsupportedCapability)?.Reason
                        ?? targets.FirstOrDefault(target => target.Reason?.Kind == SessionFailureKind.InvariantFailure)?.Reason
                        ?? new(SessionFailureKind.IllegalCommand, "no-healing-target", "target", "no healing target");
            }
            catch (BattleRuleException error) { failure = Failure(error); }
            catch (HealingRuleFault error) { failure = error.Failure; }
            options.Add(new(spellRef, $"{spellRef.Value.ToUpperInvariant()} {spellRef.Level}", BattlePresentationKind.Healing,
                spell?.MinimumRange, spell?.MaximumRange, failure is null, failure, targets.AsReadOnly()));
        }
        return new(current.SessionId, current.Revision, selection.Actor, selection.Stage,
            selection.Preview.Destination, options.AsReadOnly());
    }
}

internal sealed class HealingRuleFault(string identity, string operation) : Exception
{
    internal SessionFailure Failure => new(SessionFailureKind.InvariantFailure, "healing-rule-invariant",
        $"rules.healing.{operation}", $"HEAL rule {identity} failed at {operation}.");
}
