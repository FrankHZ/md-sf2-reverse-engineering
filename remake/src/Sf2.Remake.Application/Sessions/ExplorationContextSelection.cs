using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed record SelectExplorationContextCommand : IGameSessionCommand
{
    public SelectExplorationContextCommand(AreaDescriptionAdmission areaDescriptionAdmission)
    {
        if (!Enum.IsDefined(areaDescriptionAdmission))
        {
            throw new ArgumentOutOfRangeException(nameof(areaDescriptionAdmission));
        }

        AreaDescriptionAdmission = areaDescriptionAdmission;
    }

    public AreaDescriptionAdmission AreaDescriptionAdmission { get; }
}

public sealed record ExplorationContextSelectionSnapshot
{
    public ExplorationContextSelectionSnapshot(
        MapPosition position,
        MapSetupId selectedSetup,
        AreaDescriptionSelection areaDescription,
        ZoneEventSelection zoneEvent)
    {
        Position = position ?? throw new ArgumentNullException(nameof(position));
        SelectedSetup = selectedSetup ?? throw new ArgumentNullException(nameof(selectedSetup));
        AreaDescription = areaDescription ??
            throw new ArgumentNullException(nameof(areaDescription));
        ZoneEvent = zoneEvent ?? throw new ArgumentNullException(nameof(zoneEvent));
    }

    public MapPosition Position { get; }

    public MapSetupId SelectedSetup { get; }

    public AreaDescriptionSelection AreaDescription { get; }

    public ZoneEventSelection ZoneEvent { get; }
}

public sealed record GameSessionContextSelected(
    GameSessionSnapshot Snapshot,
    ExplorationContextSelectionSnapshot Selection) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public ExplorationContextSelectionSnapshot Selection { get; } =
        Selection ?? throw new ArgumentNullException(nameof(Selection));
}
