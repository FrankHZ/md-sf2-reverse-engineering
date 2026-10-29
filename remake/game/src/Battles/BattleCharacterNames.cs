using System.Globalization;
using System.Text.RegularExpressions;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

// Presentation only: admitted names win, including names supplied by a controlled party.
// Public packages deliberately use readable content identities without a private-name dependency.
internal static class BattleCharacterNames
{
    internal static string Resolve(BattleActorState actor, BattleSceneDefinition? content)
    {
        int member = actor.Deployment.Initialization?.AllyPartyMember ?? actor.ProcessingOrder;
        if (actor.IsAlly && content is not null && member >= 0 && member < content.MemberNames.Count &&
            !string.IsNullOrWhiteSpace(content.MemberNames[member]))
            return Clean(content.MemberNames[member]);
        if (!actor.IsAlly && content?.EnemyNames?.TryGetValue(actor.Actor, out string? name) == true && !string.IsNullOrWhiteSpace(name))
            return Clean(name);
        return ReadableIdentity(actor.Actor);
    }

    internal static string Party(ActorRef actor, int? member, IReadOnlyList<string> names) =>
        member is >= 0 && member < names.Count && !string.IsNullOrWhiteSpace(names[member.Value])
            ? Clean(names[member.Value]) : ReadableIdentity(actor);

    private static string ReadableIdentity(ActorRef actor) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
        Regex.Replace(actor.Value, "[-_]+", " ").Trim());

    private static string Clean(string name) => Regex.Replace(name, @"\s+", " ").Trim();

    // Authored healing cursors retain their engine-authored wording and timing. Only the
    // known leading subject is renamed; no arbitrary substring or private text is rewritten.
    internal static string HealingText(string text, BattleActorState actor, BattleSceneDefinition? content) =>
        content is null && text.StartsWith(actor.Actor.Value + " ", StringComparison.Ordinal)
            ? Resolve(actor, content) + text[actor.Actor.Value.Length..] : text;
}
