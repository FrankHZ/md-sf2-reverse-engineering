using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class FollowerMotion
{
    // Current-map entry retains the scene but places the walking party at its new entrance.
    // Follow physical links, not character IDs/flag order. A script-owned or detached member
    // is a separate scene actor until content installs following again.
    internal static ExplorationState Arrive(ExplorationState world, MapPosition position, byte facing)
    {
        var party = new HashSet<int> { world.PlayerEntity.Slot };
        bool added;
        do
        {
            added = false;
            foreach (var entity in world.AllEntities)
                if (entity is { Follower: { } follow, Actions: null } && party.Contains(follow.LeaderSlot))
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
            Actions = null, ActionCursor = 0, WaitingForMotion = false,
        }));
    }

    // AddFollower installs a copy of eas_Follower1 with a physical leader slot and pixel offsets.
    internal static ExplorationEntity Install(ExplorationEntity entity, int leaderSlot, int x, int y, bool waitForDestination = true) => entity with
    {
        Follower = new(leaderSlot, x, y, waitForDestination), Actions = null, ActionCursor = 0, WaitingForMotion = false,
        Motion = entity.Motion with { XSpeed = 32, YSpeed = 32, XAcceleration = 0, YAcceleration = 0,
            FlagsA = (byte)((entity.Motion.FlagsA & 0x10) | 0x60), WaitTimer = 0 },
    };

    internal static ExplorationEntity Tick(ExplorationEntity entity, IReadOnlyDictionary<int, ExplorationEntity> slots,
        WorkingMapLayout layout)
    {
        if (entity.Follower is not { } follow || entity.Actions is not null || (follow.WaitForDestination && entity.Motion.IsMoving)) return entity;
        var leader = slots[follow.LeaderSlot].Motion;
        return entity with { Motion = EntityMotion.Follow(entity.Motion, leader, follow.OffsetX, follow.OffsetY,
            (x, y) => x is >= 0 and < 64 && y is >= 0 and < 64 && layout[x, y] < 0xC000) };
    }
}
