using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

public sealed record FieldGuidance(Guid SessionId, long Revision, MapId Map, string ObjectiveKey,
    int? Companion, string DestinationKey, string RouteKey);

internal static class FieldGuidanceQuery
{
    internal static FieldGuidance? Query(ScenarioDefinition definition, SessionSnapshot current)
    {
        if (!current.HasOrdinaryFieldTime ||
            current.Exploration is not { } field || definition.Exploration is not { } content) return null;
        var flags = current.Story.Flags;
        // Selected variant definitions omit the base selector; reuse the admitted route on reentry.
        var route = field.Definition.Setup ?? content.Maps.GetValueOrDefault(field.Map)?.Setup;
        MapSetupId? setup = route is null ? null : MapSetupSelector.Select(route, route.DefaultSetup,
            flag => flags.Contains(int.Parse(flag.Value, System.Globalization.CultureInfo.InvariantCulture)));
        var matches = content.Guidance.Where(rule => rule.Map == field.Map &&
            setup == rule.Setup && (rule.Area is null || rule.Area.Contains(field.PlayerEntity.Position)) &&
            rule.Required.All(flags.Contains) && !rule.Forbidden.Any(flags.Contains)).ToArray();
        // Never resolve ambiguous or unsupported content by rule ordering.
        if (matches.Length != 1) return null;
        var rule = matches[0];
        if (rule.Follower is { } reference && (!field.TryResolveEntity(reference, out var follower) || !follower.Visible ||
            follower.Follower?.LeaderSlot != field.PlayerEntity.Slot)) return null;
        return new(current.SessionId, current.Revision, field.Map, rule.ObjectiveKey, rule.Companion,
            rule.DestinationKey, rule.RouteKey);
    }
}
