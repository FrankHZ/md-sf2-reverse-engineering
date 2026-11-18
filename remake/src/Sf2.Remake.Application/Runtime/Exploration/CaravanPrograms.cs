using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Exploration;

// Fixed source operations, not an itinerary. SF2DISASM c834c652, vehiclefunctions.asm,
// MapEventType2/cs_45284/eas_452BA/byte_45316. Logical and physical operands stay distinct.
internal static class CaravanPrograms
{
    internal const string Boarding = "caravan-boarding";
    internal const string Landing = "caravan-landing";
    internal const string Service = "caravan-world-service";
    internal static (int Leader, int X, int Y, bool Wait) Formation(int index, bool overworld) =>
        overworld ? index switch
        { 0 => (0, -24, 0, true), 1 => (0, -24, -24, false), 2 => (0, -24, 24, false),
          3 => (1, -24, 0, true), _ => (4, -24, 0, true) }
            : (Math.Min(index, 3), -24, 0, true);
    internal static EntityActionProgram Follower(int index, bool overworld)
    {
        var (leader, x, y, wait) = Formation(index, overworld);
        return new([new SetEntitySpeed(32, 32), new SetEntityAcceleration(0, 0),
            new ChangeEntityFlags(false, 0xEF, 0x60), new FollowPhysical(leader, x, y),
            .. wait ? new EntityAction[] { new WaitEntityDestination() } : [], new JumpEntityAction(3)]);
    }

    internal static IEnumerable<StoryProgram> Create(EntityRef followerA, EntityRef player, bool service = false)
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
            new StartEntityMotion(followerA, new([new MoveToPhysicalFacing(0, 1),
                new WaitEntityDestination(), new IdleEntityAction()]), true, Completion: EntityWaitCompletion.ScriptIdle),
            new StartEntityMotion(player, shrink(1), true, Completion: EntityWaitCompletion.ScriptIdle),
            new MoveVehicleOffMap(followerA),
            new StartEntityMotion(player, new([new SetActionSprite(62), new ChangeEntityFlags(true, 8, 0),
                new RefreshEntitySprite("caravan-appearance"), new IdleEntityAction()]), true,
                Completion: EntityWaitCompletion.ScriptIdle),
            new WaitForView(),
            new ApplyVehicleFollowerActions(shrink(0), SkipCaravan: true), new WaitForVehicleFollowers(SkipCaravan: true),
            new SetFieldTravelMode(FieldTravelMode.Caravan), new EndProgram()]);
        EntityActionProgram grow(bool follower) => new([
            .. follower ? new EntityAction[] { new ClonePhysicalPosition(1), new RefreshEntitySprite("caravan-clone") } : [],
            new MoveOwnFacing(1), new ChangeEntityFlags(true, 8, 8), new EntitySound(89),
            .. new ushort[] { 12, 14, 16, 18, 20, 22 }.SelectMany(size => new EntityAction[]
                { new SetGlobalSpriteSize(size), new RefreshEntitySprite("caravan-grow"), new WaitEntityTicks(1) }),
            new ChangeEntityFlags(true, 8, 0), new SetGlobalSpriteSize(16), new RefreshEntitySprite("caravan-grown"),
            new WaitEntityDestination(), new IdleEntityAction()]);
        yield return new(Landing, [
            new StartEntityMotion(player, new([new IdleEntityAction()]), false),
            new StartEntityMotion(followerA, new([new ClonePhysicalPosition(0), new RefreshEntitySprite("caravan-park"),
                new IdleEntityAction()]), true, Completion: EntityWaitCompletion.ScriptIdle),
            new RestoreCaravanPlayer(),
            new StartEntityMotion(player, grow(false), true, Completion: EntityWaitCompletion.ScriptIdle),
            new WaitForView(), new ApplyVehicleFollowerActions(grow(true), SkipCaravan: true), new WaitForVehicleFollowers(SkipCaravan: true),
            new InstallCaravanFollowers(), new SetFieldTravelMode(FieldTravelMode.Walking), new EndProgram()]);
        if (!service) yield break;
        // ProcessPlayerAction's aligned Caravan entry uses different actions from boarding:
        // the vehicle stays put, Bowie parks at64,64, then everyone grows at physical1.
        EntityActionProgram shrinkIn() => new([
            new MoveToPhysicalFacing(1, 0), new WaitEntityTicks(6),
            new ChangeEntityFlags(true, 8, 8), new EntitySound(89),
            .. new ushort[] { 22, 20, 18, 16, 14, 12 }.SelectMany((size, index) =>
                new EntityAction[] { new SetGlobalSpriteSize(size), new RefreshEntitySprite("caravan-service-shrink") }
                    .Concat(index < 5 ? [new WaitEntityTicks(1)] : [])),
            new WaitEntityDestination(), new ClonePhysicalPosition(0), new IdleEntityAction()]);
        EntityActionProgram growOut() => new([
            new ClonePhysicalPosition(1), new ChangeEntityFlags(true, 8, 8), new EntitySound(89),
            .. new ushort[] { 12, 14, 16, 18, 20, 22 }.SelectMany(size => new EntityAction[]
                { new SetGlobalSpriteSize(size), new RefreshEntitySprite("caravan-service-grow"), new WaitEntityTicks(1) }),
            new ChangeEntityFlags(true, 8, 0), new SetGlobalSpriteSize(16), new RefreshEntitySprite("caravan-service-grown"),
            new WaitEntityDestination(), new IdleEntityAction()]);
        yield return new(Service, [new PresentCue(PresentationCueKind.SoundFade), new SetCameraEntity(null),
            new StartEntityMotion(player, new([new IdleEntityAction()]), false),
            new StartEntityMotion(followerA, new([new IdleEntityAction()]), false),
            new StartEntityMotion(player, shrinkIn(), true, Completion: EntityWaitCompletion.ScriptIdle),
            new MoveVehicleOffMap(player),
            new ApplyVehicleFollowerActions(shrinkIn(), SkipCaravan: true), new WaitForVehicleFollowers(SkipCaravan: true),
            new SetFieldTravelMode(FieldTravelMode.Caravan), new WaitProgramTicks(3),
            new PresentCue(PresentationCueKind.Sound, Resource: "MUSIC_HEADQUARTERS"), new OpenCaravan(),
            new PresentCue(PresentationCueKind.SoundFade),
            new StartEntityMotion(player, growOut(), true, Completion: EntityWaitCompletion.ScriptIdle),
            new ApplyVehicleFollowerActions(growOut(), SkipCaravan: true), new WaitForVehicleFollowers(SkipCaravan: true),
            new InstallCaravanFollowers(), new SetFieldTravelMode(FieldTravelMode.Walking), new SetCameraEntity(player),
            new PresentCue(PresentationCueKind.PreviousMusic), new EndProgram()],
            "disasm/code/gameflow/exploration/explorationvints.asm:loc_25B40");
    }
}
