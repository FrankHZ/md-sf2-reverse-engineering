using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Content.Scenarios;

public sealed record ExplorationEntityDefinition(EntityRef Entity, MapPosition Position, byte Facing, ushort Speed,
    bool Visible = true, bool Obstruction = false);
public enum ExplorationEventKind { Step, Interact, Warp }
public sealed record ExplorationEvent(ExplorationEventKind Kind, int? X, int? Y,
    EntityRef? Entity, ProgramLocation? Program, MapId? DestinationMap = null,
    MapPosition? Destination = null, byte Facing = 0, ushort? RequiredMarker = null,
    int? RequiredFlag = null, bool RequiredFlagValue = true);
public sealed record ExplorationBattleRoute(string Encounter, int? UnlockedFlag, int? CompletedFlag,
    int? IntroFlag, ProgramLocation? BeforeProgram, ProgramLocation? StartProgram);

public sealed class ExplorationMapDefinition
{
    internal ExplorationMapDefinition(MapId map, WorkingMapLayout layout, OriginalMapTraversal traversal,
        IEnumerable<ExplorationEntityDefinition> entities, IEnumerable<ExplorationEvent> events,
        ProgramLocation? onLoad = null, ExplorationBattleRoute? battle = null, ProgramLocation? inputProgram = null, MapSetupRoute? setup = null)
    {
        Map = map; Layout = layout; Traversal = traversal; Entities = Array.AsReadOnly(entities.ToArray());
        Events = Array.AsReadOnly(events.ToArray()); OnLoad = onLoad; Battle = battle; InputProgram = inputProgram; Setup = setup;
    }
    public MapId Map { get; }
    public WorkingMapLayout Layout { get; }
    public OriginalMapTraversal Traversal { get; }
    public IReadOnlyList<ExplorationEntityDefinition> Entities { get; }
    public IReadOnlyList<ExplorationEvent> Events { get; }
    public ProgramLocation? OnLoad { get; }
    public ProgramLocation? InputProgram { get; }
    public MapSetupRoute? Setup { get; }
    public ExplorationBattleRoute? Battle { get; }
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

public sealed class ExplorationDefinition
{
    internal ExplorationDefinition(IEnumerable<ExplorationMapDefinition> maps, IEnumerable<StoryProgram> programs, IEnumerable<KeyValuePair<int, string>>? texts = null, ExplorationProvenance? provenance = null)
    {
        Provenance = provenance;
        Texts = new ReadOnlyDictionary<int, string>((texts ?? []).ToDictionary(pair => pair.Key, pair => pair.Value));
        Maps = new ReadOnlyDictionary<MapId, ExplorationMapDefinition>(maps.ToDictionary(map => map.Map));
        Programs = new ReadOnlyDictionary<string, StoryProgram>(programs.ToDictionary(program => program.Id, StringComparer.Ordinal));
    }
    public IReadOnlyDictionary<int, string> Texts { get; }
    public ExplorationProvenance? Provenance { get; }
    public IReadOnlyDictionary<MapId, ExplorationMapDefinition> Maps { get; }
    public IReadOnlyDictionary<string, StoryProgram> Programs { get; }
}

public sealed class ExplorationStartInput
{
    public ExplorationStartInput(MapId map, EntityRef player, MapPosition position, byte facing, ushort speed,
        IEnumerable<int> flags, BattleStartInput party, ProgramLocation? entryProgram = null)
    { Map = map; Player = player; Position = position; Facing = facing; Speed = speed;
        Flags = Array.AsReadOnly(flags.ToArray()); Party = party; EntryProgram = entryProgram; }
    public MapId Map { get; }
    public EntityRef Player { get; }
    public MapPosition Position { get; }
    public byte Facing { get; }
    public ushort Speed { get; }
    public IReadOnlyList<int> Flags { get; }
    public BattleStartInput Party { get; }
    public ProgramLocation? EntryProgram { get; }
}
public sealed record ExplorationReadAccepted(ScenarioDefinition Definition, ExplorationStartInput Start) : ScenarioReadResult;
