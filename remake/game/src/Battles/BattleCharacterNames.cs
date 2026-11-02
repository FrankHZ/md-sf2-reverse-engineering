using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

// Presentation only: admitted names win, including names supplied by a controlled party.
// Public packages deliberately use readable content identities without a private-name dependency.
internal static class BattleCharacterNames
{
    internal static string Resolve(BattleActorState actor, BattleSceneDefinition? content, StoryState story) =>
        CharacterNames.Battle(story, actor, content);

    // Authored healing cursors retain their engine-authored wording and timing. Only the
    // known leading subject is renamed; no arbitrary substring or private text is rewritten.
    internal static string HealingText(string text, BattleActorState actor, BattleSceneDefinition? content, StoryState story) =>
        content is null && text.StartsWith(actor.Actor.Value + " ", StringComparison.Ordinal)
            ? Resolve(actor, content, story) + text[actor.Actor.Value.Length..] : text;
}
