using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class FollowerMotion
{
    // Current-map entry retains the scene but places the walking party at its new entrance.
    // Follow physical links within the current exploration list. A parked slot can still hold
    // a dormant link, and a listed slot can be script-owned or detached.
    internal static ExplorationState Arrive(ExplorationState world, MapPosition position, byte facing)
    {
        var party = new HashSet<int> { world.PlayerEntity.Slot };
        bool added;
        do
        {
            added = false;
            foreach (var entity in world.AllEntities)
                if (entity is { Follower: { } follow, Actions: null } &&
                    world.CurrentEntities.Contains(entity.Slot) && party.Contains(follow.LeaderSlot))
                    added |= party.Add(entity.Slot);
        } while (added);
        return world.WithEntities(world.AllEntities.Select(entity => !party.Contains(entity.Slot) ? entity : entity with
        {
            Motion = entity.Motion with
            {
                X = (short)(position.X * 384), Y = (short)(position.Y * 384),
                XDestination = (short)(position.X * 384), YDestination = (short)(position.Y * 384),
                XVelocity = 0, YVelocity = 0, XTravel = 0, YTravel = 0, WaitTimer = 0,
                Facing = entity.Slot == world.PlayerEntity.Slot ? facing : entity.Motion.Facing,
            },
            Actions = null, ActionSource = null, ActionCursor = 0, WaitingForMotion = false,
        }));
    }

    // AddFollower installs a copy of eas_Follower1 with a physical leader slot and pixel offsets.
    internal static ExplorationEntity Install(ExplorationEntity entity, int leaderSlot, int x, int y, bool waitForDestination = true) => entity with
    {
        Follower = new(leaderSlot, x, y, waitForDestination), Actions = null, ActionSource = null, ActionCursor = 0, WaitingForMotion = false,
        Motion = entity.Motion with { XSpeed = 32, YSpeed = 32, XAcceleration = 0, YAcceleration = 0,
            FlagsA = (byte)((entity.Motion.FlagsA & 0x10) | 0x60), WaitTimer = 0 },
    };

    internal static ExplorationEntity Tick(ExplorationEntity entity, IReadOnlyDictionary<int, ExplorationEntity> slots,
        ExplorationState world, bool inBattle = false)
    {
        if (entity.Follower is not { } follow || entity.Actions is not null || (follow.WaitForDestination && entity.Motion.IsMoving)) return entity;
        var target = slots[follow.LeaderSlot];
        // Older settled visits can still link to a slot parked by Hide.
        if (!target.Visible && target.Motion.X >= 0x7000) return entity;
        var leader = target.Motion;
        return entity with { Motion = EntityMotion.Follow(entity.Motion, leader, follow.OffsetX, follow.OffsetY,
            (x, y) => FieldLayout.Word(world, leader, x, y, inBattle) is < 0xC000) };
    }
}
