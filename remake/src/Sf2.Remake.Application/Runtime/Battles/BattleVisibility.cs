using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

// Shared ordinary-board disclosure; no retained reveal history. Scene poses are display-only.
public static class BattleVisibility
{
    public static Exploration.ExplorationEntity? StagedEntity(SessionSnapshot current, BattleActorState actor)
    {
        int slot = actor.IsAlly ? actor.Deployment.Initialization?.AllyPartyMember ?? actor.ProcessingOrder : actor.ProcessingOrder;
        var reference = new EntityRef("entity-" + slot.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return current.BattleFieldScene?.Entities.TryGetValue(reference, out var entity) == true && entity.Entity == reference ? entity : null;
    }

    public static bool OnBoard(SessionSnapshot current, BattleActorState actor) => actor.Position is not null &&
        (current.BattleFieldScene is null || StagedEntity(current, actor) is { Visible: true });
}
