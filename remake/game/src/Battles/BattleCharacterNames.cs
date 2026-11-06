using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.Battles;

// Presentation only: admitted names win, including names supplied by a controlled party.
// Public packages deliberately use readable content identities without a private-name dependency.
internal static class BattleCharacterNames
{
    internal static string Resolve(BattleActorState actor, BattleSceneDefinition? content, StoryState story) =>
        DisplayNames.Battle(story, actor, content);

    internal static string HealingText(Sf2.Remake.Application.Runtime.Battles.HealingDisplay display, bool recovery, GameSession session) =>
        recovery ? Sf2.Remake.GodotAdapter.Localization.GameText.Get("battle.heal.recovery", Resolve(session.Current.Battle.GetActor(display.Target), session.ActiveBattleScenes, session.Current.Story), display.RecoveredHp) :
        Sf2.Remake.GodotAdapter.Localization.GameText.Get("battle.heal.action", Resolve(session.Current.Battle.GetActor(display.Caster), session.ActiveBattleScenes, session.Current.Story), DisplayNames.Spell(display.Spell), display.Spell.Level);
}
