using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed class OriginalBattle01ControlledArrivalInputs
{
    public const string GransealFirstAttemptComparisonId = "battle01-granseal-first-attempt-arrival-comparison";
    private static readonly int[] ComparisonFlagIds = [609, 506, 543, 220, 65, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 82, 83, 84];

    public OriginalBattle01ControlledArrivalInputs(string id, ushort stepCounter,
        IEnumerable<KeyValuePair<int, bool>> flags, IEnumerable<Battle01EntrySlot> dormantSlots)
    {
        ArgumentNullException.ThrowIfNull(flags); ArgumentNullException.ThrowIfNull(dormantSlots);
        Id = id; StepCounter = stepCounter;
        Flags = new ReadOnlyDictionary<int, bool>(flags.ToDictionary(pair => pair.Key, pair => pair.Value));
        DormantSlots = Array.AsReadOnly(dormantSlots.ToArray());
    }

    public string Id { get; }
    public ushort StepCounter { get; }
    public IReadOnlyDictionary<int, bool> Flags { get; }
    public IReadOnlyList<Battle01EntrySlot> DormantSlots { get; }
    public static OriginalBattle01ControlledArrivalInputs GransealFirstAttemptComparison { get; } = new(
        GransealFirstAttemptComparisonId, 0, ComparisonFlagIds.Select(id => KeyValuePair.Create(id, false)),
        Enumerable.Range(3, 27).Select(id => new Battle01EntrySlot((byte)id, 0, 0, 0, 0, 0, 0, 0,
            new(0, 0, 0, 0, 0, 0), new(0, 0, 0, 0, 0, 0), [127, 127, 127, 127], [63, 63, 63, 63])));

    public OriginalBattle01StartupDiagnostic? GetAdmissionDiagnostic()
    {
        if (Id != GransealFirstAttemptComparisonId || StepCounter != 0)
            return new("arrival.inputs", "Select the explicit Granseal arrival comparison and stored step word0.");
        if (Flags.Count != ComparisonFlagIds.Length || ComparisonFlagIds.Any(id => !Flags.TryGetValue(id, out bool set) || set))
            return new("arrival.flags", "All named setup/follower/chest conditions must be explicitly false.");
        if (DormantSlots.Count != 27 || DormantSlots.Where((slot, i) =>
                slot is null || slot.Id != i + 3 || !slot.IsNeutralDormant).Any())
            return new("arrival.dormant", "Supply all27 explicit neutral dormant rows, including Peter7 and Lemon28.");
        return null;
    }
}

public sealed record OriginalMapReturnRoofRow(int Ordinal, int TriggerX, int TriggerY,
    int SourceX, int SourceY, int Width, int Height, int DestinationX, int DestinationY);
public sealed record OriginalMapReturnFlagCopy(int Flag, int SourceX, int SourceY,
    int Width, int Height, int DestinationX, int DestinationY);
public sealed record OriginalMapReturnChest(int X, int Y, int Flag, int Item);

// Catalog-owned facts from the existing canonical parse; unlike the house roof this binds no warp.
public sealed class OriginalMapReturnEntryLoadDefinition
{
    public OriginalMapReturnEntryLoadDefinition(OriginalMapExplorationRuntimeDefinition runtime,
        string flagResource, IEnumerable<OriginalMapReturnFlagCopy> flags,
        string chestResource, IEnumerable<OriginalMapReturnChest> chests,
        string roofResource, IEnumerable<OriginalMapReturnRoofRow> roofs,
        OriginalMapStepCopyDefinition churchDoor)
    {
        Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        FlagResource = flagResource; ChestResource = chestResource; RoofResource = roofResource;
        FlagCopies = Array.AsReadOnly(flags.ToArray()); Chests = Array.AsReadOnly(chests.ToArray());
        Roofs = Array.AsReadOnly(roofs.ToArray());
        ChurchDoor = churchDoor ?? throw new ArgumentNullException(nameof(churchDoor));
    }
    public OriginalMapExplorationRuntimeDefinition Runtime { get; }
    public OriginalMapAreaDefinition Area => Runtime.AreaCatalog.Records[0];
    public string FlagResource { get; }
    public string ChestResource { get; }
    public string RoofResource { get; }
    public IReadOnlyList<OriginalMapReturnFlagCopy> FlagCopies { get; }
    public IReadOnlyList<OriginalMapReturnChest> Chests { get; }
    public IReadOnlyList<OriginalMapReturnRoofRow> Roofs { get; }
    public OriginalMapStepCopyDefinition ChurchDoor { get; }
    // Keep the complete source order: the church is ordinal8, not the first filtered record.
    public MapBlockCopyActionTable RoofActions => new(Roofs.Select(row => new MapBlockCopyActionRecord(
        new(row.TriggerX, row.TriggerY), row.SourceX == 255
            ? MapBlockRegionMutation.Clear(row.DestinationX, row.DestinationY, row.Width, row.Height)
            : MapBlockRegionMutation.CopyFrom(new(row.SourceX, row.SourceY,
                row.DestinationX, row.DestinationY, row.Width, row.Height)))));
    public OriginalMapReturnRoofRow SelectRoof(MapPosition position) => Roofs.First(row =>
        position.X >= row.DestinationX - Area.SecondLayerForegroundStart.X &&
        position.X < row.DestinationX - Area.SecondLayerForegroundStart.X + row.Width &&
        position.Y >= row.DestinationY - Area.SecondLayerForegroundStart.Y &&
        position.Y < row.DestinationY - Area.SecondLayerForegroundStart.Y + row.Height);
}
