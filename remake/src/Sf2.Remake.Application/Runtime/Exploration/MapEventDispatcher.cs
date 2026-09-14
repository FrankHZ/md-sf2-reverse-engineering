using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class MapEventDispatcher
{
    internal static SessionSnapshot Interact(SessionSnapshot current, ExplorationEvent entry, EntityRef entity,
        List<SessionObservation> observations)
    {
        var story = current.Story.Copy(entry.Program);
        if (entry.EntityFlags is { } flags)
            story = story.Copy(story.Cursor, (flags & 1) != 0 ? new EntityEventFacingWait(new(current.ObservationSequence + 1)) : null,
                entityEvent: new(entity, ProgramRunner.Entity(current, entity).Motion.Facing, flags));
        return ProgramRunner.Commit(current, current.Active, story, observations, "interaction-started", entity.Value);
    }

    internal static ActiveSessionState Face(SessionSnapshot current, ActiveSessionState active)
    {
        var context = current.Story.EntityEvent!;
        var world = ((ActiveExploration)active).World;
        var entity = world.Entities[context.Entity];
        return new ActiveExploration(world.WithEntity(entity with
            { Motion = entity.Motion with { Facing = (byte)((world.PlayerEntity.Motion.Facing + 2) & 3) } }));
    }

    internal static SessionSnapshot Finish(SessionSnapshot current, List<SessionObservation> observations)
    {
        if (current.Story.EntityEvent is not { } context) return current;
        var world = current.Exploration!;
        if ((context.Flags & 2) != 0 && world.Entities.TryGetValue(context.Entity, out var entity))
            world = world.WithEntity(entity with { Motion = entity.Motion with { Facing = context.OriginalFacing } });
        return ProgramRunner.Commit(current, new ActiveExploration(world),
            current.Story.Copy(null, textWindow: new ClosedTextWindow(), clearEntityEvent: true), observations, "interaction-finished", context.Entity.Value);
    }

    internal static ExplorationState OpenDoor(ExplorationState world, MapPosition candidate)
    {
        if ((world.Layout[candidate.X, candidate.Y] & 0x3C00) != 0x400) return world;
        var door = world.Definition.LayoutEvents?.Doors.FirstOrDefault(row => row.Trigger == candidate);
        return door is null ? world : world.WithLayout(world.Layout.ApplyBlockCopy(door.Copy));
    }

    internal static ExplorationState Roof(ExplorationState world)
    {
        if (world.Definition.LayoutEvents is not { } layoutEvents) return world;
        var position = world.PlayerEntity.Position;
        var result = MapBlockCopyActionReducer.Apply(world.Layout, world.RoofState, new(false, false),
            new(position.X, position.Y), false, layoutEvents.Roofs);
        return world.WithLayout(result.Layout, result.LifecycleState);
    }

    internal static ExplorationState RoofOnLoad(ExplorationState world)
    {
        if (world.Definition.LayoutEvents is not { } events) return world;
        var area = world.Definition.Traversal.SelectActiveArea(world.PlayerEntity.Position)!;
        var offset = world.Definition.OverlayOffsets[area.OneBasedRecordOrdinal - 1];
        int x = world.PlayerEntity.Motion.X + offset.X * 384, y = world.PlayerEntity.Motion.Y + offset.Y * 384;
        for (int index = 0; index < events.Roofs.Records.Count; index++)
        {
            var mutation = events.Roofs.Records[index].Mutation;
            if (x < mutation.DestinationX * 384 || y < mutation.DestinationY * 384 ||
                x > (mutation.DestinationX + mutation.Width - 1) * 384 || y > (mutation.DestinationY + mutation.Height - 1) * 384) continue;
            var result = MapBlockCopyLifecycleReducer.Activate(world.Layout, world.RoofState, new(false, false), index + 1, mutation);
            return world.WithLayout(result.Layout, result.LifecycleState);
        }
        return world;
    }
}
