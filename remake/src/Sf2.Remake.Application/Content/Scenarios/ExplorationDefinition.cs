using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Content.Scenarios;

public sealed record ExplorationEntityDefinition(EntityRef Entity, MapPosition Position, byte Facing, ushort Speed,
    bool Visible = true, bool Obstruction = false, int? Sprite = null, EntityActionProgram? Actions = null);
public sealed record ExplorationFollowerDefinition(int Flag, int Character, int Sprite);
public sealed record ExplorationAllySprite(int Character, int Sprite, int? JoinedFlag, int? UnjoinedSprite);
public sealed record ExplorationPopulation(int AllyCount, int NonAllyStart, int PlayerSprite,
    IReadOnlyList<ExplorationFollowerDefinition> Followers, IReadOnlyList<ExplorationAllySprite>? AllySprites = null);
public sealed record ExplorationDoor(MapPosition Trigger, WorkingMapBlockCopy Copy);
public sealed record ExplorationFlagCopy(int Flag, WorkingMapBlockCopy Copy);
public sealed record ExplorationLayoutEvents(IReadOnlyList<ExplorationDoor> Doors,
    IReadOnlyList<ExplorationFlagCopy> Flags, MapBlockCopyActionTable Roofs);
public sealed class ExplorationRaster
{
    private readonly byte[] _bytes;
    internal ExplorationRaster(int width, int height, string format, byte[] bytes)
    { Width = width; Height = height; Format = format; _bytes = [.. bytes]; }
    public int Width { get; }
    public int Height { get; }
    public string Format { get; }
    public byte[] CopyBytes() => [.. _bytes];
}
public sealed record MapOverlayOffset(int X, int Y);
public sealed record ExplorationViewArea(int MinX, int MinY, int MaxX, int MaxY,
    int ForegroundX, int ForegroundY, int BackgroundX, int BackgroundY,
    int ParallaxAX, int ParallaxAY, int ParallaxBX, int ParallaxBY,
    int AutoscrollAX, int AutoscrollAY, int AutoscrollBX, int AutoscrollBY, int Layer);
// Explicit logical settings inherited from the declared start, never host display preferences.
public sealed record ExplorationTextSettings(byte MessageSpeed, byte MouthControl, ushort ViewSpeed);
public sealed record ExplorationTextFont(IReadOnlyList<byte> AsciiToSymbol, IReadOnlyList<byte> Advances);
public sealed record ExplorationMapMusic(int Field, int Battle);
public sealed record ExplorationMapVisual(MapId Map, ExplorationRaster Atlas, int Scale,
    IReadOnlyList<IReadOnlyList<ushort>> Blocks, IReadOnlyList<ExplorationMapMusic> Music);
public sealed record ExplorationSpriteVisual(int Sprite, IReadOnlyList<ExplorationRaster> Directions, int? Portrait, int Speech);
public sealed record PortraitTileChange(byte X, byte Y, byte AlternateX, byte AlternateY);
public sealed record ExplorationPortraitVisual(int Portrait, ExplorationRaster Raster,
    IReadOnlyList<PortraitTileChange>? Eyes = null, IReadOnlyList<PortraitTileChange>? Mouth = null);
public sealed class ExplorationAudio
{
    private readonly byte[] _pcm;
    internal ExplorationAudio(int command, int timerB, int sampleRate, int channels, byte[] pcm, string pcmSha256, int? loopBegin, int? loopEnd)
    { Command = command; TimerB = timerB; SampleRate = sampleRate; Channels = channels; _pcm = [.. pcm]; PcmSha256 = pcmSha256; LoopBegin = loopBegin; LoopEnd = loopEnd; }
    public int Command { get; }
    public int TimerB { get; }
    public string PcmSha256 { get; }
    public int SampleRate { get; }
    public int Channels { get; }
    public int SampleFrames => _pcm.Length / (Channels * 2);
    public int? LoopBegin { get; }
    public int? LoopEnd { get; }
    public byte[] CopyPcm() => [.. _pcm];
}
public sealed record ExplorationVisuals(IReadOnlyDictionary<MapId, ExplorationMapVisual> Maps,
    IReadOnlyDictionary<int, ExplorationSpriteVisual> Sprites, IReadOnlyDictionary<int, ExplorationPortraitVisual> Portraits,
    IReadOnlyDictionary<string, ExplorationAudio> Audio);
public enum ExplorationEventKind { Step, Interact, Warp, SourceZone }
public sealed record ExplorationEvent(ExplorationEventKind Kind, int? X, int? Y,
    EntityRef? Entity, ProgramLocation? Program, MapId? DestinationMap = null,
    MapPosition? Destination = null, byte Facing = 0, ushort? RequiredMarker = null,
    int? RequiredFlag = null, bool RequiredFlagValue = true, byte? EntityFlags = null, MapLoadMode LoadMode = MapLoadMode.Rebuild,
    EntityActionProgram? SourceInit = null);
public sealed record ExplorationBattleRoute(string Encounter, int? UnlockedFlag, int? CompletedFlag,
    int? IntroFlag, ProgramLocation? BeforeProgram, ProgramLocation? StartProgram, ProgramLocation? LoadProgram = null,
    ExplorationOutcomeRoute? Outcome = null);
public sealed record ExplorationOutcomeRoute(ProgramLocation AfterProgram, int JoinMember,
    ProgramLocation DefeatedProgram, ProgramLocation DefeatProgram, ProgramLocation ReturnProgram, MapId BattleMap, byte VictoryFacing,
    MapId EgressMap, MapPosition EgressPosition, byte EgressFacing);

// The source compares the LONG containing palette colors 2 and 3. These are
// logical CRAM words, independent of the host's rendered brightness.
public sealed record PalettePair(ushort Color2, ushort Color3)
{
    public bool Valid => (Color2 & ~0xEEE) == 0 && (Color3 & ~0xEEE) == 0;
    public bool Black => Color2 == 0 && Color3 == 0;
}
public enum FullFadeVisibility { Black, BaseRestored, Transitioning }
public sealed record ExplorationDisplay(byte Period, PalettePair Base, PalettePair Current, FullFadeVisibility Visibility);

public sealed class ExplorationMapDefinition
{
    internal ExplorationMapDefinition(MapId map, WorkingMapLayout layout, OriginalMapTraversal traversal,
        IEnumerable<ExplorationEntityDefinition> entities, IEnumerable<ExplorationEvent> events,
        ProgramLocation? onLoad = null, ExplorationBattleRoute? battle = null, ProgramLocation? inputProgram = null, MapSetupRoute? setup = null,
        ExplorationPopulation? population = null, ExplorationLayoutEvents? layoutEvents = null, IEnumerable<MapOverlayOffset>? overlays = null,
        IEnumerable<WriteFlag>? entryFlags = null, PalettePair? basePalette = null, IEnumerable<ExplorationViewArea>? viewAreas = null)
    {
        Map = map; Layout = layout; Traversal = traversal; Entities = Array.AsReadOnly(entities.ToArray());
        Events = Array.AsReadOnly(events.ToArray()); OnLoad = onLoad; Battle = battle; InputProgram = inputProgram; Setup = setup; Population = population; LayoutEvents = layoutEvents;
        OverlayOffsets = Array.AsReadOnly((overlays ?? traversal.ActiveAreas.Select(_ => new MapOverlayOffset(0, 0))).ToArray());
        EntryFlags = Array.AsReadOnly((entryFlags ?? []).ToArray());
        BasePalette = basePalette; ViewAreas = Array.AsReadOnly((viewAreas ?? []).ToArray());
    }
    public MapId Map { get; }
    public WorkingMapLayout Layout { get; }
    public OriginalMapTraversal Traversal { get; }
    public IReadOnlyList<ExplorationEntityDefinition> Entities { get; }
    public IReadOnlyList<ExplorationEvent> Events { get; }
    public ProgramLocation? OnLoad { get; }
    public ProgramLocation? InputProgram { get; }
    public MapSetupRoute? Setup { get; }
    public ExplorationPopulation? Population { get; }
    public ExplorationLayoutEvents? LayoutEvents { get; }
    public IReadOnlyList<MapOverlayOffset> OverlayOffsets { get; }
    public ExplorationBattleRoute? Battle { get; }
    public IReadOnlyList<WriteFlag> EntryFlags { get; }
    public PalettePair? BasePalette { get; }
    public IReadOnlyList<ExplorationViewArea> ViewAreas { get; }
}

public sealed class ExplorationProvenance
{
    internal ExplorationProvenance(string repository, string commit, string romSha256,
        IEnumerable<EncounterSource> sources, string controlledBoundary)
    { Repository = repository; Commit = commit; RomSha256 = romSha256;
        Sources = Array.AsReadOnly(sources.ToArray()); ControlledBoundary = controlledBoundary; }
    public string Repository { get; }
    public string Commit { get; }
    public string RomSha256 { get; }
    public IReadOnlyList<EncounterSource> Sources { get; }
    public string ControlledBoundary { get; }
}

public enum ExplorationTextTokenKind { Literal, Newline, MemberName, Wait1, Wait2, Unsupported }
public sealed record ExplorationTextToken(ExplorationTextTokenKind Kind, string Value, int? Member = null);

public sealed class ExplorationDefinition
{
    internal ExplorationDefinition(IEnumerable<ExplorationMapDefinition> maps, IEnumerable<StoryProgram> programs, IEnumerable<KeyValuePair<int, string>>? texts = null, ExplorationProvenance? provenance = null, MapPartyFlagLayout? partyFlags = null, ExplorationVisuals? visuals = null, IEnumerable<string>? memberNames = null,
        IEnumerable<KeyValuePair<int, IReadOnlyList<ExplorationTextToken>>>? textTokens = null, ExplorationTextFont? textFont = null)
    {
        Provenance = provenance; PartyFlags = partyFlags; Visuals = visuals; TextFont = textFont;
        MemberNames = Array.AsReadOnly((memberNames ?? []).ToArray());
        Texts = new ReadOnlyDictionary<int, string>((texts ?? []).ToDictionary(pair => pair.Key, pair => pair.Value));
        TextTokens = new ReadOnlyDictionary<int, IReadOnlyList<ExplorationTextToken>>((textTokens ?? [])
            .ToDictionary(pair => pair.Key, pair => (IReadOnlyList<ExplorationTextToken>)Array.AsReadOnly(pair.Value.ToArray())));
        Maps = new ReadOnlyDictionary<MapId, ExplorationMapDefinition>(maps.ToDictionary(map => map.Map));
        Programs = new ReadOnlyDictionary<string, StoryProgram>(programs.ToDictionary(program => program.Id, StringComparer.Ordinal));
    }
    public ExplorationTextFont? TextFont { get; }
    public IReadOnlyDictionary<int, string> Texts { get; }
    public IReadOnlyDictionary<int, IReadOnlyList<ExplorationTextToken>> TextTokens { get; }
    public IReadOnlyList<string> MemberNames { get; }
    public ExplorationProvenance? Provenance { get; }
    public MapPartyFlagLayout? PartyFlags { get; }
    public ExplorationVisuals? Visuals { get; }
    public IReadOnlyDictionary<MapId, ExplorationMapDefinition> Maps { get; }
    public IReadOnlyDictionary<string, StoryProgram> Programs { get; }
}

public sealed class ExplorationStartInput
{
    public ExplorationStartInput(MapId map, EntityRef player, MapPosition position, byte facing, ushort speed,
        IEnumerable<int> flags, BattleStartInput party, ProgramLocation? entryProgram = null,
        IEnumerable<ExplorationEntityStartPhase>? entityPhases = null, ExplorationDisplay? display = null, ExplorationTextSettings? textSettings = null)
    { Map = map; Player = player; Position = position; Facing = facing; Speed = speed;
        Flags = Array.AsReadOnly(flags.ToArray()); Party = party; EntryProgram = entryProgram;
        EntityPhases = Array.AsReadOnly((entityPhases ?? []).ToArray()); Display = display; TextSettings = textSettings; }
    public MapId Map { get; }
    public EntityRef Player { get; }
    public MapPosition Position { get; }
    public byte Facing { get; }
    public ushort Speed { get; }
    public IReadOnlyList<int> Flags { get; }
    public BattleStartInput Party { get; }
    public ProgramLocation? EntryProgram { get; }
    public IReadOnlyList<ExplorationEntityStartPhase> EntityPhases { get; }
    public ExplorationDisplay? Display { get; }
    public ExplorationTextSettings? TextSettings { get; }
}
public sealed record ExplorationEntityStartPhase(EntityRef Entity, int Slot, int ActionCursor, byte NextWaitTicks,
    bool WaitingForMotion, EntityMotionState Motion);
public sealed record ExplorationReadAccepted(ScenarioDefinition Definition, ExplorationStartInput Start) : ScenarioReadResult;
