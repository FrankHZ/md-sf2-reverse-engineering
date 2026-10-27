using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

// Fixed source operations, not an itinerary. SF2DISASM c834c652, vehiclefunctions.asm,
// MapEventType2/cs_45284/eas_452BA/byte_45316. Logical and physical operands stay distinct.
internal static class CaravanPrograms
{
    internal const string Boarding = "caravan-boarding";
    internal const string Landing = "caravan-landing";
    internal static EntityActionProgram Follower(int index, bool overworld)
    {
        var (leader, x, y, wait) = overworld ? index switch
        { 0 => (0, -24, 0, true), 1 => (0, -24, -24, false), 2 => (0, -24, 24, false),
          3 => (1, -24, 0, true), _ => (4, -24, 0, true) }
            : (Math.Min(index, 3), -24, 0, true);
        return new([new SetEntitySpeed(32, 32), new SetEntityAcceleration(0, 0),
            new ChangeEntityFlags(false, 0xEF, 0x60), new FollowPhysical(leader, x, y),
            .. wait ? new EntityAction[] { new WaitEntityDestination() } : [], new JumpEntityAction(3)]);
    }

    internal static IEnumerable<StoryProgram> Create(CaravanDefinition caravan, EntityRef player)
    {
        EntityActionProgram shrink(int target) => new([
            new MoveToPhysicalFacing(target, 0), new WaitEntityTicks(6),
            new ChangeEntityFlags(true, 8, 8), new EntitySound(89),
            .. new ushort[] { 22, 20, 18, 16, 14, 12 }.SelectMany((size, index) =>
                new EntityAction[] { new SetGlobalSpriteSize(size), new RefreshEntitySprite("caravan-shrink") }
                    .Concat(index < 5 ? [new WaitEntityTicks(1)] : [])),
            new WaitEntityDestination(), new ClonePhysicalPosition(1),
            new RefreshEntitySprite("caravan-clone"), new IdleEntityAction()]);
        yield return new(Boarding, [
            new StartEntityMotion(player, new([new IdleEntityAction()]), false),
            new StartEntityMotion(caravan.FollowerA, new([new MoveToPhysicalFacing(0, 1),
                new WaitEntityDestination(), new IdleEntityAction()]), true, Completion: EntityWaitCompletion.ScriptIdle),
            new StartEntityMotion(player, shrink(1), true, Completion: EntityWaitCompletion.ScriptIdle),
            new ParkCaravanEntity(caravan.FollowerA),
            new StartEntityMotion(player, new([new SetActionSprite(62), new ChangeEntityFlags(true, 8, 0),
                new RefreshEntitySprite("caravan-appearance"), new IdleEntityAction()]), true,
                Completion: EntityWaitCompletion.ScriptIdle),
            new WaitForView(),
            new ApplyCaravanFollowers(shrink(0)), new WaitForCaravanFollowers(),
            new CommitCaravanMode(FieldTravelMode.Caravan), new EndProgram()]);
        EntityActionProgram grow(bool follower) => new([
            .. follower ? new EntityAction[] { new ClonePhysicalPosition(1), new RefreshEntitySprite("caravan-clone") } : [],
            new MoveOwnFacing(1), new ChangeEntityFlags(true, 8, 8), new EntitySound(89),
            .. new ushort[] { 12, 14, 16, 18, 20, 22 }.SelectMany(size => new EntityAction[]
                { new SetGlobalSpriteSize(size), new RefreshEntitySprite("caravan-grow"), new WaitEntityTicks(1) }),
            new ChangeEntityFlags(true, 8, 0), new SetGlobalSpriteSize(16), new RefreshEntitySprite("caravan-grown"),
            new WaitEntityDestination(), new IdleEntityAction()]);
        yield return new(Landing, [
            new StartEntityMotion(player, new([new IdleEntityAction()]), false),
            new StartEntityMotion(caravan.FollowerA, new([new ClonePhysicalPosition(0), new RefreshEntitySprite("caravan-park"),
                new IdleEntityAction()]), true, Completion: EntityWaitCompletion.ScriptIdle),
            new RestoreCaravanPlayer(),
            new StartEntityMotion(player, grow(false), true, Completion: EntityWaitCompletion.ScriptIdle),
            new WaitForView(), new ApplyCaravanFollowers(grow(true)), new WaitForCaravanFollowers(),
            new InstallCaravanFollowers(), new CommitCaravanMode(FieldTravelMode.Walking), new EndProgram()]);
    }
}
