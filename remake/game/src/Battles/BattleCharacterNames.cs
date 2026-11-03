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

    internal static string HealingText(Sf2.Remake.Application.Runtime.Battles.HealingDisplay display, bool recovery) =>
        recovery ? Sf2.Remake.GodotAdapter.Localization.GameText.Get("battle.heal.recovery", display.TargetName, display.RecoveredHp) :
        Sf2.Remake.GodotAdapter.Localization.GameText.Get("battle.heal.action", display.CasterName, display.Spell.Value.ToUpperInvariant(), display.Spell.Level);
}
