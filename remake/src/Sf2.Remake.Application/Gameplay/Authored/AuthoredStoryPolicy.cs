using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Gameplay.Authored;

internal sealed class AuthoredStoryPolicy : ISourceStoryPolicy
{
    public string Identity => "authored-story";
    public SourceStoryCandidate Apply(ScenarioDefinition definition, SessionSnapshot current,
        SourceStoryInstruction instruction, ProgramLocation location) =>
        throw new BattleRuleException("source-story-unsupported", "program.sourceStory", true);
}
