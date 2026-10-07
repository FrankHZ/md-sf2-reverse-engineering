using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

// The only candidate is an existing entity's retirement, or no active-state change.
// No story, cursor, callers, wait, party, aliases or publication can be supplied by a policy.
internal sealed record SourceStoryCandidate(ExplorationEntity? RetiredEntity = null);

internal interface ISourceStoryPolicy
{
    string Identity { get; }
    SourceStoryCandidate Apply(ScenarioDefinition definition, SessionSnapshot current,
        SourceStoryInstruction instruction, ProgramLocation location);
}

internal static class SourceStoryRules
{
    internal static ActiveSessionState Apply(ISourceStoryPolicy policy, ScenarioDefinition definition,
        SessionSnapshot current, SourceStoryInstruction instruction, ProgramLocation location) =>
        BattleProgressionRules.Invoke(policy.Identity, $"{instruction.GetType().Name} at {location.Program}:{location.Instruction}", () =>
        {
            var candidate = policy.Apply(definition, current, instruction, location);
            if (candidate is null) throw new InvalidOperationException("Missing story candidate.");
            if (candidate.RetiredEntity is not { } retired) return current.Active;
            var world = current.Exploration;
            var before = world?.AllEntities.SingleOrDefault(entity => entity.Slot == retired.Slot);
            if (world is null || before is null ||
                retired != world.Hide(before, removeAliases: false).AllEntities.Single(entity => entity.Slot == retired.Slot))
                throw new InvalidOperationException("Invalid entity retirement.");
            return new ActiveExploration(world.WithEntity(retired));
        });
}
