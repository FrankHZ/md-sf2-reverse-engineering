using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleMemberEntry(ActorRef Actor, string Name, int? NameIndex, string? CustomName,
    bool Available, bool Defeated, bool Deployed);
public sealed record BattleMemberSnapshot(Guid SessionId, long Revision, bool Available, string? Reason,
    ActorRef? ActiveActor, IReadOnlyList<BattleMemberEntry> Members, BattleInspectionUnit? Unit);

// BattlefieldMenu copies UpdateForce's active list, independently of living board visibility.
// Read retained live actors only; membership never initializes an absent/unjoined actor.
internal static class BattleMemberQuery
{
    internal static BattleMemberSnapshot Query(ScenarioDefinition definition, SessionSnapshot current,
        Guid? session = null, long? revision = null, ActorRef? requested = null)
    {
        BattleMemberSnapshot Reject(string reason) => new(current.SessionId, current.Revision, false, reason,
            null, Array.AsReadOnly(Array.Empty<BattleMemberEntry>()), null);
        if (BattleInspectionQuery.Unavailable(current, session, revision) is { } reason) return Reject(reason);
        var layout = definition.Exploration?.PartyFlags ?? (definition.BattleStory is not null || definition.PrivateDefinitions is not null
            ? BattleStoryDefinition.PartyFlags : null);
        var membership = layout is not null ? MapPartyMembership.Rebuild(current.Story.Flags, layout) :
            current.Battle.Actors.Any(a => a.IsAlly && a.Deployment.Initialization?.AllyPartyMember is not null)
                ? current.Story.PartyLists : null;
        var actors = current.Battle.Actors.Where(a => a.IsAlly && (membership is null
            ? definition.PrivateDefinitions is null
            : a.Deployment.Initialization?.AllyPartyMember is { } member && membership.Active.Contains(member)))
            .OrderBy(a => a.Deployment.Initialization?.AllyPartyMember ?? a.ProcessingOrder).ToArray();
        var scenes = definition.ScenesFor(current.Battle.Definition.Encounter);
        var members = actors.Select(a => new BattleMemberEntry(a.Actor, BattleUnitProjection.Name(definition, current, a),
            BattleUnitProjection.NameIndex(a, scenes), current.Story.Names.GetValueOrDefault(a.Actor.Value),
            a.Initialized, a.Initialized && a.Hp == 0, a.Initialized && a.Hp > 0 && BattleVisibility.OnBoard(current, a))).ToArray();
        var actor = requested is null ? null : actors.FirstOrDefault(a => a.Actor == requested);
        if (requested is not null && actor is null) return Reject("member-not-disclosed");
        return new(current.SessionId, current.Revision, true, actor?.Initialized == false ? "member-unavailable" : null,
            current.Selection!.Actor, Array.AsReadOnly(members), actor?.Initialized == true ? BattleUnitProjection.Details(definition, current, actor) : null);
    }
}
