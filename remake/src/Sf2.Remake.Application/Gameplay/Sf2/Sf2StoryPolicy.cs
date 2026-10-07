using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Gameplay.Sf2;

internal sealed class Sf2StoryPolicy : ISourceStoryPolicy
{
    public string Identity => "sf2-story";

    public SourceStoryCandidate Apply(ScenarioDefinition definition, SessionSnapshot current,
        SourceStoryInstruction instruction, ProgramLocation location)
    {
        if (instruction is not RetiredMap3EntityScratch)
            throw new BattleRuleException("program-instruction", "program", true);
        var reloaded = current.Exploration;
        if (location != new ProgramLocation("byte-513a8", 1) || reloaded?.Map.Value != "map-3" ||
            current.Story.Continuation is not (ProgramContinuation.MapLoaded or ProgramContinuation.OutcomeMapLoaded) ||
            current.Story.TextWindow is not ClosedTextWindow || !current.Story.Flags.Contains(603))
            throw new BattleRuleException("inactive-window-scratch-context", "program.retiredMap3Entity", true);
        if (reloaded.TryResolveEntity(new("entity-142"), out var existing))
            return new(reloaded.Hide(existing, removeAliases: false).AllEntities.Single(entity => entity.Slot == existing.Slot));
        if (!current.Story.Flags.Contains(1) || !reloaded.AllEntities.Any(entity =>
                entity.Entity.Value == "entity-142" && !entity.Visible && entity.Motion.X == 0x7000 && entity.Motion.Y == 0x7000))
            throw new BattleRuleException("inactive-window-scratch-context", "program.retiredMap3Entity", true);
        // Accepted #425: normal reload drains old presentation work and clears windows;
        // the later window is rebuilt before publication. The real hide/FF tombstone stays.
        // These four stores affect inactive scratch, without another visible side effect.
        return new();
    }
}
