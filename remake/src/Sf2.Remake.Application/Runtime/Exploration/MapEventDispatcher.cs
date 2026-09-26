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
            reached = world.Definition.Events.FirstOrDefault(entry => entry.Kind is ExplorationEventKind.Warp or ExplorationEventKind.SourceZone &&
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

    internal static SessionSnapshot Interact(ScenarioDefinition definition, SessionSnapshot current, ExplorationEvent entry, EntityRef entity,
        List<SessionObservation> observations)
    {
        var story = current.Story.Copy(entry.Program);
        if (entry.EntityFlags is { } flags)
            story = story.Copy(story.Cursor, (flags & 1) != 0 ? new EntityEventFacingWait(new(current.ObservationSequence + 1)) : null,
                entityEvent: new(entity, ProgramRunner.Entity(current, entity).Motion.Facing, flags));
        current = ProgramRunner.Commit(current, current.Active, story, observations, "interaction-started", entity.Value);
        if (story.TextSettings is null || story.EntityEvent is null) return current;
        // The source portrait opens before the facing wait and before entity suppression.
        current = current.WithStory(story.Copy(story.Cursor, entityServices: true));
        current = ExplorationPortraitRunner.Open(definition, current, entity, 0, observations, true);
        return current.Story.Wait is PortraitMovementWait ? current : EnterBody(current, observations);
    }

    internal static SessionSnapshot EnterBody(SessionSnapshot current, List<SessionObservation> observations)
    {
        var context = current.Story.EntityEvent!;
        bool face = (context.Flags & 1) != 0;
        return ProgramRunner.Commit(current, current.Active, current.Story.Copy(current.Story.Cursor,
            face ? new EntityEventFacingWait(new(current.ObservationSequence + 1)) : null,
            entityServices: face), observations, "interaction-portrait-ready");
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
        if (current.Story.EventCaller is ZoneEventContext zone) return FinishZone(current, zone, observations);
        if (current.Story.EntityEvent is not { } context) return current;
        var world = current.Exploration!;
        if (!context.Returning && (context.Flags & 2) != 0 && world.Entities.TryGetValue(context.Entity, out var entity))
            world = world.WithEntity(entity with { Motion = entity.Motion with { Facing = context.OriginalFacing } });
        if (current.Story.TextSettings is not null && !context.Returning)
        {
            current = ProgramRunner.Commit(current, new ActiveExploration(world), current.Story.Copy(null,
                entityEvent: context with { Returning = true }), observations, "interaction-facing-restored");
            current = ExplorationPortraitRunner.Close(current, observations, true);
            if (current.Story.Wait is PortraitMovementWait) return current;
        }
        if (current.Story.LogicalText is { Open: true } window)
        {
            ExplorationTextRunner.ValidateContext(current);
            // loc_476C4 closes with the event's live service flag (Trap6 may have enabled it).
            // Restore facing once above; the close continuation clears the context.
            return ProgramRunner.Commit(current, new ActiveExploration(world), current.Story.Copy(null,
                new TextCloseWait(new(current.ObservationSequence + 1), CallerReturn: true),
                logicalText: window with { AnimationCounter = 0, AnimationLength = 8, Moving = true }),
                observations, "interaction-closing", context.Entity.Value);
        }
        return ProgramRunner.Commit(current, new ActiveExploration(world),
            current.Story.Copy(null, textWindow: new ClosedTextWindow(), clearEntityEvent: true,
                entityServices: current.Story.TextSettings is not null ? true : null), observations, "interaction-finished", context.Entity.Value);
    }

    internal static SessionSnapshot EnterZone(SessionSnapshot current, ExplorationEvent entry,
        bool entityServices, List<SessionObservation> observations)
    {
        var world = current.Exploration!;
        var actions = entry.SourceInit ?? throw new Sf2.Remake.Domain.Battles.BattleRuleException("zone-init", "event.actions", true);
        // ApplyInitActscript writes the script pointer after the producing pass. Preserve
        // physical motion and its timer; the init stream runs in subsequent entity service.
        world = world.WithEntity(world.PlayerEntity with { Actions = actions, ActionCursor = 0,
            WaitingForMotion = false, Follower = null });
        return ProgramRunner.Commit(current, new ActiveExploration(world), current.Story.Copy(entry.Program,
            eventCaller: new ZoneEventContext(), entityServices: entityServices), observations, "zone-entered");
    }

    private static SessionSnapshot FinishZone(SessionSnapshot current, ZoneEventContext context,
        List<SessionObservation> observations)
    {
        if (!context.Returning)
        {
            current = ProgramRunner.Commit(current, current.Active, current.Story.Copy(null,
                eventCaller: context with { Returning = true }), observations, "zone-returning");
            if (current.Story.TextSettings is not null)
            {
                current = ExplorationPortraitRunner.Close(current, observations, true);
                if (current.Story.Wait is not null) return current;
            }
        }
        if (current.Story.LogicalText is { Open: true } window)
        {
            ExplorationTextRunner.ValidateContext(current);
            return ProgramRunner.Commit(current, current.Active, current.Story.Copy(null,
                new TextCloseWait(new(current.ObservationSequence + 1), CallerReturn: true),
                logicalText: window with { AnimationCounter = 0, AnimationLength = 8, Moving = true }),
                observations, "zone-closing");
        }
        // loc_47576 always WaitForVInt before checking physical arrival, including
        // an empty/skipped handler or a map-blocked marker whose player never moved.
        return ProgramRunner.Commit(current, current.Active, current.Story.Copy(null,
            new ZoneArrivalWait(new(current.ObservationSequence + 1)), textWindow: new ClosedTextWindow()),
            observations, "zone-arrival-wait");
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
