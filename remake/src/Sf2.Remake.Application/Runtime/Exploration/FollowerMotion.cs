using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class FollowerMotion
{
    // AddFollower installs a copy of eas_Follower1 with a physical leader slot and pixel offsets.
    internal static ExplorationEntity Install(ExplorationEntity entity, int leaderSlot, int x, int y) => entity with
    {
        Follower = new(leaderSlot, x, y), Actions = null, ActionCursor = 0, WaitingForMotion = false,
        Motion = entity.Motion with { XSpeed = 32, YSpeed = 32, XAcceleration = 0, YAcceleration = 0,
            FlagsA = (byte)((entity.Motion.FlagsA & 0x10) | 0x60), WaitTimer = 0 },
    };

    internal static ExplorationEntity Tick(ExplorationEntity entity, IReadOnlyDictionary<int, ExplorationEntity> slots,
        WorkingMapLayout layout)
    {
        if (entity.Follower is not { } follow || entity.Actions is not null || entity.Motion.IsMoving) return entity;
        var leader = slots[follow.LeaderSlot].Motion;
        return entity with { Motion = EntityMotion.Follow(entity.Motion, leader, follow.OffsetX, follow.OffsetY,
            (x, y) => x is >= 0 and < 64 && y is >= 0 and < 64 && layout[x, y] < 0xC000) };
    }
}
