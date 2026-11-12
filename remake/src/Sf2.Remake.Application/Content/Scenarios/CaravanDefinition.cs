using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Content.Scenarios;

// Controlled authored admission. These bindings do not allocate or repair source identities.
public sealed record CaravanPlayerAppearance(ActorRef Actor, BattleClassRule Class, int Sprite, int DefeatedSprite);
public sealed record CaravanDefinition(EntityRef FollowerA, IReadOnlyList<int> CurrentEntities,
    bool Overworld, CaravanPlayerAppearance PlayerAppearance, short InitialStepCounter = 0);
public enum FieldTravelMode { Walking = 0, Caravan = 1, Raft = 2 }

internal abstract record VehicleInstruction : StoryInstruction;
internal sealed record ApplyVehicleFollowerActions(EntityActionProgram Actions) : VehicleInstruction;
internal sealed record MoveVehicleOffMap(EntityRef Entity) : VehicleInstruction;
internal sealed record WaitForVehicleFollowers : VehicleInstruction;
internal sealed record RestoreCaravanPlayer : VehicleInstruction;
internal sealed record InstallCaravanFollowers : VehicleInstruction;
internal sealed record SetFieldTravelMode(FieldTravelMode Mode) : VehicleInstruction;

public sealed record FollowPhysical(int Slot, int X, int Y) : EntityAction;
public sealed record MoveOwnFacing(int Distance) : EntityAction;
public sealed record MoveToPhysicalFacing(int Slot, int Distance) : EntityAction;
public sealed record ClonePhysicalPosition(int Slot) : EntityAction;
public sealed record WaitEntityDestination : EntityAction;
public sealed record SetActionSprite(int Sprite) : EntityAction;
public sealed record EntitySound(int Command) : EntityAction;
