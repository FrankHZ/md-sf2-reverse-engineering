using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

internal static class RaftRuntime
{
    internal const string Boarding = "raft-boarding";
    internal const string Landing = "raft-landing";
    private const int Slot = 31;
    private const int Sprite = 61;
    private static readonly EntityRef FollowerD = new("entity-159");

    internal static ExplorationState Declare(ExplorationState world, IReadOnlyList<int> flags, RaftBerth? berth)
    {
        EntityMotionState motion;
        if (world.TravelMode == FieldTravelMode.Raft)
        {
            world = world.WithEntities(world.AllEntities.Select(actor => actor.Slot == 0
                ? actor with { Sprite = Sprite }
                : world.CurrentEntities.Contains(actor.Slot)
                    ? actor with { Motion = OffMap(actor.Motion), Follower = null, Actions = null }
                    : actor));
            motion = OffMap(world.PlayerEntity.Motion) with { Facing = 2, FlagsA = 0 };
        }
        else
        {
            if (berth?.Map != world.Map || !flags.Contains(64)) return world;
            motion = EntityMotionState.At(berth.Position, 2, 32) with { FlagsA = 0 };
        }
        if (world.AllEntities.Any(e => e.Slot == Slot))
            throw new BattleRuleException("raft-slot-occupied", "map.population", true);
        var actor = new ExplorationEntity(FollowerD, motion,
            true, Slot: Slot, Sprite: Sprite);
        var aliases = world.Aliases.ToDictionary();
        aliases[FollowerD] = Slot;
        return new(world.Definition, world.Layout, world.Player, world.AllEntities.Append(actor), world.Party,
            world.SpriteSize, aliases, world.RoofState, world.Population, world.Quake, world.TravelMode, world.CurrentEntities);
    }

    private static EntityMotionState OffMap(EntityMotionState motion) => motion with
        { X = 0x6000, Y = 0x6000, XDestination = 0x6000, YDestination = 0x6000 };

    internal static bool Aligned(ExplorationState world)
    {
        var player = world.PlayerEntity.Motion;
        var (dx, dy) = (player.Facing & 3) switch { 0 => (384, 0), 1 => (0, -384), 2 => (-384, 0), _ => (0, 384) };
        return world.AllEntities.Any(e => e.Slot == Slot && e.Motion.XDestination == player.XDestination + dx &&
            e.Motion.YDestination == player.YDestination + dy);
    }

    // MapEventType3 / cs_453F2. Followers converge on main, then clone the parked physical raft.
    internal static IEnumerable<StoryProgram> Programs(EntityRef player)
    {
        static EntityActionProgram Approach(int target) => new([new MoveToPhysicalFacing(target, 0),
            new WaitEntityTicks(6), new EntitySound(89), new WaitEntityDestination(),
            new ClonePhysicalPosition(Slot), new IdleEntityAction()]);
        yield return new(Boarding, [new StartEntityMotion(player, Approach(Slot), true, Completion: EntityWaitCompletion.ScriptIdle),
            new MoveVehicleOffMap(FollowerD),
            new StartEntityMotion(player, new([new SetActionSprite(Sprite), new ChangeEntityFlags(true, 8, 0),
                new RefreshEntitySprite("raft-appearance"), new IdleEntityAction()]), true, Completion: EntityWaitCompletion.ScriptIdle),
            new WaitForView(), new ApplyVehicleFollowerActions(Approach(0)), new WaitForVehicleFollowers(),
            new SetFieldTravelMode(FieldTravelMode.Raft), new EndProgram()]);

        EntityAction[] land = [new MoveOwnFacing(1), new EntitySound(89), new WaitEntityDestination(), new IdleEntityAction()];
        yield return new(Landing, [
            new StartEntityMotion(FollowerD, new([new ClonePhysicalPosition(0), new RefreshEntitySprite("raft-park"),
                new IdleEntityAction()]), true, Completion: EntityWaitCompletion.ScriptIdle),
            new SetEntitySprite(player, 0, Ally: true),
            new StartEntityMotion(player, new(land), true, Completion: EntityWaitCompletion.ScriptIdle),
            new WaitForView(), new ApplyVehicleFollowerActions(new([new ClonePhysicalPosition(Slot), .. land])),
            new WaitForVehicleFollowers(), new InstallCaravanFollowers(), new SetFieldTravelMode(FieldTravelMode.Walking),
            new EndProgram()]);
    }
}
