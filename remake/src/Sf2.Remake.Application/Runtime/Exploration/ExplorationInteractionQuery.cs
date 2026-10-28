using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.Application.Runtime.Exploration;

// The same candidate and event predicate serves the hint and the committing command.
internal static class ExplorationInteractionQuery
{
    internal static EntityRef? Query(SessionSnapshot current)
    {
        if (!current.CanWaitAtInput || current.Exploration is not { } world || world.Definition.InputProgram is not null)
            return null;
        foreach (var entity in world.AllEntities)
            if (entity.Entity != world.Player && InReach(current, entity.Entity) && Event(current, entity.Entity)?.Program is not null)
                return entity.Entity;
        return null;
    }

    internal static bool InReach(SessionSnapshot current, EntityRef target)
    {
        var world = current.Exploration!;
        if (target == world.Player || !world.Entities.TryGetValue(target, out var entity) || !entity.Visible) return false;
        var player = world.PlayerEntity;
        int dx = entity.Position.X - player.Position.X, dy = entity.Position.Y - player.Position.Y;
        return Math.Abs(dx) + Math.Abs(dy) == 1 && player.Motion.Facing ==
            (dx > 0 ? 0 : dx < 0 ? 2 : dy < 0 ? 1 : 3);
    }

    internal static ExplorationEvent? Event(SessionSnapshot current, EntityRef target) =>
        current.Exploration!.Definition.Events.FirstOrDefault(entry => entry.Kind == ExplorationEventKind.Interact &&
            (entry.Entity is null || entry.Entity == target) && (entry.RequiredFlag is null ||
                current.Story.Flags.Contains(entry.RequiredFlag.Value) == entry.RequiredFlagValue));
}
