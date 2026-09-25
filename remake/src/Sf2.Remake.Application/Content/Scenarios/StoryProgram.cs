using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content.Scenarios;

public readonly record struct EntityRef(string Value);
public readonly record struct ProgramLocation(string Program, int Instruction);
public enum TextDisplayMode { Continued, Single }
public enum PresentationCueKind { FadeIn, FadeOut, FlashWhite, RestorePalette, CameraPosition, CameraEntity, Sound, Gesture, EntityEffect, CameraWait, SoundFade, BattleLoad, SoundWait, PreviousMusic }
public enum MapLoadMode { Rebuild, Preserve }

public abstract record StoryInstruction;
public sealed record EndProgram(bool SourceMapScript = false) : StoryInstruction;
public sealed record JumpProgram(ProgramLocation Target) : StoryInstruction;
public sealed record BranchFlag(int Flag, bool WhenSet, ProgramLocation Target) : StoryInstruction;
public sealed record BranchEntityCoordinates(EntityRef Entity, short X, short Y, bool WhenEqual, ProgramLocation Target) : StoryInstruction;
public sealed record CallProgram(ProgramLocation Target, bool ActivateEntities = false) : StoryInstruction;
public sealed record ReturnProgram : StoryInstruction;
public sealed record ResetPartyBattleStats : StoryInstruction;
public sealed record ReturnBattleMap : StoryInstruction;
public sealed record RetiredMap3EntityScratch : StoryInstruction;
public sealed record WriteFlag(int Flag, bool Value) : StoryInstruction;
public sealed record SetTextCursor(int Text) : StoryInstruction;
public sealed record ShowText(TextDisplayMode Mode, EntityRef? Speaker, byte SpeakerFlags = 0, bool UseEventSpeaker = false,
    bool WaitForAcknowledgement = true, bool ExplicitWindows = false) : StoryInstruction;
public sealed record OpenPortrait(EntityRef? Entity, byte Flags = 0) : StoryInstruction;
public sealed record ClosePortrait : StoryInstruction;
public sealed record WaitForTextInput : StoryInstruction;
public sealed record CloseText : StoryInstruction;
public sealed record WaitForView : StoryInstruction;
public sealed record ChooseYesNo(int ResultFlag) : StoryInstruction;
public sealed record SetEntityFacing(EntityRef Entity, byte Facing, bool RefreshSprite = false) : StoryInstruction;
public sealed record SetEntitySprite(EntityRef Entity, int Sprite) : StoryInstruction;
public sealed record SetEntityPriority(EntityRef Entity, bool Value) : StoryInstruction;
public sealed record SetEntityPosition(EntityRef Entity, MapPosition Position, byte Facing) : StoryInstruction;
public sealed record SetEntityVisibility(EntityRef Entity, bool Visible) : StoryInstruction;
public sealed record HideMapEntity(EntityRef Entity, bool RemoveAliases) : StoryInstruction;
public sealed record SetDialogueSpeaker(EntityRef? Entity) : StoryInstruction;
public sealed record SetCameraTarget(MapPosition Position) : StoryInstruction;
public sealed record SetCameraEntity(EntityRef? Entity) : StoryInstruction;
public sealed record LoadSceneMap(MapId Map, MapPosition Camera) : StoryInstruction;
public sealed record LoadSceneEntities(ExplorationPopulation Population, MapPosition PlayerPosition, byte Facing,
    IReadOnlyList<ExplorationEntityDefinition> Entities) : StoryInstruction;
public enum EntityScriptInstallation { Preserve, SlotTimer, SlotTimerClearCollision }
public sealed record StartEntityMotion(EntityRef Entity, EntityActionProgram Actions, bool Wait,
    EntityScriptInstallation Installation = EntityScriptInstallation.Preserve) : StoryInstruction;
public sealed record WaitForEntity(EntityRef Entity) : StoryInstruction;
public sealed record JoinPartyMember(int Member) : StoryInstruction;
public sealed record FollowEntity(EntityRef Entity, EntityRef Leader, int OffsetX, int OffsetY) : StoryInstruction;
public sealed record WaitProgramTicks(int Ticks) : StoryInstruction;
public sealed record PresentCue(PresentationCueKind Kind, string? Resource = null,
    EntityRef? Entity = null, MapPosition? Position = null, FullBlackFade? FullBlack = null) : StoryInstruction;
// ExecuteFading is synchronous, full-mask black, with one final VInt service.
// A supplied period is temporary; null consumes the live period.
public sealed record FullBlackFade(byte? Period = null);
public sealed record TransferToMap(MapId Map, MapPosition Position, byte Facing, MapLoadMode Mode) : StoryInstruction;
public sealed record UnsupportedInstruction(string Opcode, string Source) : StoryInstruction;

public abstract record EntityAction;
public sealed record MoveEntityRelative(int X, int Y, bool Wait = true) : EntityAction;
public sealed record MoveEntityAbsolute(MapPosition Position, bool FieldInput = false) : EntityAction;
public sealed record RandomWalkEntity(MapPosition Origin, int Radius) : EntityAction;
public sealed record FaceEntity(byte Facing) : EntityAction;
public sealed record WaitEntityTicks(byte Ticks) : EntityAction;
public sealed record StopEntityActions : EntityAction;
public sealed record IdleEntityAction : EntityAction;
public sealed record SetEntitySpeed(ushort X, ushort Y) : EntityAction;
public sealed record SetEntityAcceleration(byte X, byte Y) : EntityAction;
public sealed record ChangeEntityFlags(bool FlagsB, byte Mask, byte Value) : EntityAction;
public sealed record SetGlobalSpriteSize(ushort Size) : EntityAction;
public sealed record RefreshEntitySprite(string Source) : EntityAction;
public sealed record JumpEntityAction(int Instruction) : EntityAction;
public sealed record UnsupportedEntityAction(string Opcode, string Source) : EntityAction;
public sealed record EntityActionProgram
{
    internal EntityActionProgram(IEnumerable<EntityAction> actions) => Actions = Array.AsReadOnly(actions.ToArray());
    public IReadOnlyList<EntityAction> Actions { get; }
}

public sealed class StoryProgram
{
    internal StoryProgram(string id, IEnumerable<StoryInstruction> instructions, string? source = null, bool entitiesRunning = true)
    { Id = id; Instructions = Array.AsReadOnly(instructions.ToArray()); Source = source; EntitiesRunning = entitiesRunning; }
    public string Id { get; }
    public IReadOnlyList<StoryInstruction> Instructions { get; }
    public string? Source { get; }
    public bool EntitiesRunning { get; }
}
