using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content.Scenarios;

// Modern, read-only disclosure content. These predicates never become story progress.
public sealed record FieldGuidanceRule(MapId Map, MapSetupId? Setup, IReadOnlyList<int> Required,
    IReadOnlyList<int> Forbidden, EntityRef? Follower, string ObjectiveKey, int? Companion,
    string DestinationKey, string RouteKey, OriginalMapTraversalArea? Area = null);
