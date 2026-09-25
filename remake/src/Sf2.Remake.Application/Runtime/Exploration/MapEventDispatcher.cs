using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal sealed record FieldMoveOutcome(bool Moved, bool DoorOpened, ExplorationEvent? Event);

internal static class MapEventDispatcher
{
    internal static (ExplorationState World, FieldMoveOutcome Outcome) Move(ExplorationState world,
        ExplorationDirection direction, IReadOnlyList<int> flags)
    {
        var player = world.PlayerEntity;
        byte facing = direction switch
        { ExplorationDirection.East => 0, ExplorationDirection.North => 1, ExplorationDirection.West => 2, _ => 3 };
        world = world.WithEntity(player with { Motion = player.Motion with { Facing = facing } });
        player = world.PlayerEntity;
        var candidate = world.Definition.Traversal.ResolveCandidateTarget(world.Layout, player.Position, direction);
        var others = world.AllEntities.Where(entity => entity.Slot != player.Slot && entity.Visible).ToArray();
        // PR559: obstruction precedes the door copy and the marker, even for an
        // otherwise non-passable candidate. A blocked traversal returns the origin.
        if (world.Population is not null && (player.Motion.FlagsA & 0x20) != 0 && candidate is not null &&
            EntityMotion.FieldObstructed(candidate.X * 384, candidate.Y * 384, others.Select(entity => entity.Motion)))
            return (world, new(false, false, null));
        bool opened = false;
        ExplorationEvent? reached = null;
        if (candidate is not null && world.Definition.Traversal.IsWithinActiveArea(candidate))
        {
            var before = world;
            world = OpenDoor(world, candidate);
            opened = !ReferenceEquals(before, world);
            reached = world.Definition.Events.FirstOrDefault(entry => entry.Kind == ExplorationEventKind.Warp &&
                Matches(entry, candidate, world.Layout[candidate.X, candidate.Y], flags));
        }
        var traversal = world.Definition.Traversal.TryMove(world.Layout, player.Position, direction);
        bool occupied = world.Population is null && others.Any(entity =>
            entity.Motion.XDestination / 384 == traversal.Position.X && entity.Motion.YDestination / 384 == traversal.Position.Y);
        var motion = traversal.Outcome == OriginalMapTraversalOutcome.Moved && !occupied
            ? EntityMotion.Start(player.Motion, (short)(traversal.Position.X * 384), (short)(traversal.Position.Y * 384),
                others.Select(entity => entity.Motion), world.Population is not null) : null;
        if (motion is not null)
        {
            world = world.WithEntity(player with { Motion = motion,
                Actions = new EntityActionProgram([new StopEntityActions()]), ActionCursor = 0, WaitingForMotion = true });
            reached ??= world.Definition.Events.FirstOrDefault(entry => entry.Kind == ExplorationEventKind.Step &&
                Matches(entry, traversal.Position, world.Layout[traversal.Position.X, traversal.Position.Y], flags));
        }
        return (world, new(motion is not null, opened, reached));
    }

    private static bool Matches(ExplorationEvent entry, MapPosition position, ushort word, IReadOnlyList<int> flags) =>
        (entry.X is null || entry.X == position.X) && (entry.Y is null || entry.Y == position.Y) &&
        (entry.RequiredMarker is null || (word & 0x3C00) == entry.RequiredMarker) &&
        (entry.RequiredFlag is null || flags.Contains(entry.RequiredFlag.Value) == entry.RequiredFlagValue);

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
        if (current.Story.LogicalText is { Open: true } window)
        {
            ExplorationTextRunner.ValidateContext(current);
            // loc_476C4 closes while the event still owns the suppressed entity slot.
            // Restore facing once above; the close continuation clears the context.
            return ProgramRunner.Commit(current, new ActiveExploration(world), current.Story.Copy(null,
                new TextCloseWait(new(current.ObservationSequence + 1), EntityEventReturn: true),
                logicalText: window with { AnimationCounter = 0, AnimationLength = 8, Moving = true }),
                observations, "interaction-closing", context.Entity.Value);
        }
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
