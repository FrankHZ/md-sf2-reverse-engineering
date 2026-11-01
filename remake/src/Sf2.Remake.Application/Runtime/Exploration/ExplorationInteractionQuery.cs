using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.Application.Runtime.Exploration;

// The same candidate and event predicate serves the hint and the committing command.
internal static class ExplorationInteractionQuery
{
    internal static EntityRef? Query(SessionSnapshot current)
    {
        if (!current.CanWaitAtInput || current.Exploration is not { } world || world.Definition.InputProgram is not null)
            return null;
        foreach (var entity in world.AllEntities.OrderBy(entity => entity.Slot))
            if (entity.Entity != world.Player && InReach(current, entity.Entity) && Event(current, entity.Entity)?.Program is not null)
                return entity.Entity;
        return null;
    }

    internal static bool InReach(SessionSnapshot current, EntityRef target)
    {
        var world = current.Exploration!;
        if (target == world.Player || !world.Entities.TryGetValue(target, out var entity) || !entity.Visible || entity.Follower is not null) return false;
        var player = world.PlayerEntity;
        var offset = (player.Motion.Facing & 3) switch { 0 => (384, 0), 1 => (0, -384), 2 => (-384, 0), _ => (0, 384) };
        int x = player.Motion.X, y = player.Motion.Y;
        // GetActivatedEntity skips any consecutive desk/counter blocks, then scans
        // physical entities within one tile (strictly), excluding installed followers.
        for (int cell = 0; cell < 64; cell++)
        {
            x += offset.Item1; y += offset.Item2;
            if (x < 0 || y < 0 || x >= 64 * 384 || y >= 64 * 384) return false;
            if ((world.Layout[x / 384, y / 384] & 0x3C00) != 0x2800)
                return Math.Abs(entity.Motion.X - x) + Math.Abs(entity.Motion.Y - y) < 384;
        }
        return false;
    }

    internal static ExplorationEvent? Event(SessionSnapshot current, EntityRef target) =>
        current.Exploration!.Definition.Events.FirstOrDefault(entry => entry.Kind == ExplorationEventKind.Interact &&
            (entry.Entity is null || entry.Entity == target) && (entry.RequiredFlag is null ||
                current.Story.Flags.Contains(entry.RequiredFlag.Value) == entry.RequiredFlagValue));
}
