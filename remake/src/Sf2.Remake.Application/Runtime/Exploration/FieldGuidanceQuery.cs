using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

public sealed record FieldGuidance(Guid SessionId, long Revision, MapId Map, string ObjectiveKey,
    int? Companion, string DestinationKey, string RouteKey, FieldGuidanceAnchor? Anchor);

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
        var anchor = rule.Anchor;
        if (anchor is not null && !AnchorAvailable(field, flags, anchor)) anchor = null;
        return new(current.SessionId, current.Revision, field.Map, rule.ObjectiveKey, rule.Companion,
            rule.DestinationKey, rule.Anchor is not null && anchor is null ? "guidance.route-unavailable" : rule.RouteKey, anchor);
    }

    private static bool AnchorAvailable(ExplorationState field, IReadOnlyList<int> flags, FieldGuidanceAnchor anchor)
    {
        if (!field.Definition.Events.Any(e => e.Kind == ExplorationEventKind.Warp &&
            (e.X is null || e.X == anchor.Position.X) && (e.Y is null || e.Y == anchor.Position.Y) &&
            e.DestinationMap == anchor.DestinationMap && e.Destination == anchor.Destination &&
            e.Unavailable is null && (e.RequiredFlag is null || flags.Contains(e.RequiredFlag.Value) == e.RequiredFlagValue) &&
            (e.RequiredMarker is null || (field.Layout[anchor.Position.X, anchor.Position.Y] & 0x3C00) == e.RequiredMarker))) return false;
        var area = field.Definition.Traversal.SelectActiveArea(field.PlayerEntity.Position)?.Area;
        if (area?.Contains(anchor.Position) != true || !area.Contains(anchor.Approach) ||
            (field.Layout[anchor.Approach.X, anchor.Approach.Y] & 0xC000) == 0xC000) return false;
        // A public warp can occupy a collision cell; its local approach must still be clear.
        return !field.AllEntities.Any(e => e.Entity != field.Player && e.Visible &&
            (e.Motion.FlagsA & 0x80) != 0 && (e.Position == anchor.Approach || e.Position == anchor.Position));
    }
}
