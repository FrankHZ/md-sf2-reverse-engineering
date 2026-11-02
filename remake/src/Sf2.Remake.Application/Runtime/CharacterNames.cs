using System.Globalization;
using System.Text.RegularExpressions;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime;

// Resolve identity once, before formatting: never substitute arbitrary substrings in source text.
public static class CharacterNames
{
    public static string Actor(StoryState story, ActorRef actor, string fallback) =>
        story.Names.GetValueOrDefault(actor.Value) ?? fallback;

    public static string Member(ScenarioDefinition definition, StoryState story, int member) => Member(definition, story.Names, member);

    public static string Member(ScenarioDefinition definition, IReadOnlyDictionary<string, string> overrides, int member)
    {
        var names = definition.Exploration!.MemberNames;
        var actor = definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
            .FirstOrDefault(row => row.Faction == BattleFaction.Ally && row.Initialization?.AllyPartyMember == member)?.Actor;
        string fallback = member >= 0 && member < names.Count ? names[member] : "Name unavailable";
        return actor is { } id ? overrides.GetValueOrDefault(id.Value) ?? fallback : fallback;
    }

    public static string Battle(StoryState story, BattleActorState actor, BattleSceneDefinition? content)
    {
        int member = actor.Deployment.Initialization?.AllyPartyMember ?? actor.ProcessingOrder;
        string fallback = ReadableIdentity(actor.Actor);
        if (actor.IsAlly && content is not null && member >= 0 && member < content.MemberNames.Count &&
            !string.IsNullOrWhiteSpace(content.MemberNames[member])) fallback = Clean(content.MemberNames[member]);
        else if (!actor.IsAlly && content?.EnemyNames?.TryGetValue(actor.Actor, out string? name) == true && !string.IsNullOrWhiteSpace(name))
            fallback = Clean(name);
        return Actor(story, actor.Actor, fallback);
    }

    public static string ReadableIdentity(ActorRef actor) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
        Regex.Replace(actor.Value, "[-_]+", " ").Trim());
    private static string Clean(string name) => Regex.Replace(name, @"\s+", " ").Trim();
}
