using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content.Scenarios;

// Controlled authored admission. These bindings do not allocate or repair source identities.
public sealed record CaravanFollowerAction(int Flag, int LeaderSlot, int OffsetX, int OffsetY);
public sealed record CaravanDefinition(EntityRef FollowerA, IReadOnlyList<int> CurrentEntities,
    IReadOnlyList<CaravanFollowerAction> FollowerActions, int PlayerSprite);
public enum FieldTravelMode { Walking, Caravan }

internal abstract record CaravanInstruction : StoryInstruction;
internal sealed record ApplyCaravanFollowers(EntityActionProgram Actions) : CaravanInstruction;
internal sealed record ParkCaravanEntity(EntityRef Entity) : CaravanInstruction;
internal sealed record WaitForCaravanFollowers : CaravanInstruction;
internal sealed record CommitCaravanMode(FieldTravelMode Mode) : CaravanInstruction;

public sealed record MoveToPhysicalFacing(int Slot, int Distance) : EntityAction;
public sealed record ClonePhysicalPosition(int Slot) : EntityAction;
public sealed record WaitEntityDestination : EntityAction;
public sealed record SetActionSprite(int Sprite) : EntityAction;
public sealed record EntitySound(int Command) : EntityAction;
