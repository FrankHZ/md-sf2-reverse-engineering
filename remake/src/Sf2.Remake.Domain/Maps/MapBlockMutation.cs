using System.Collections.ObjectModel;

namespace Sf2.Remake.Domain.Maps;

public enum MapBlockMutationKind
{
    SetBlocks,
    SetBlocksVar,
}

public enum MapViewUpdateChannel
{
    Channel0,
    Channel1,
}

public sealed record MapViewUpdateState(
    bool Channel0Requested,
    bool Channel1Requested)
{
    public bool IsRequested(MapViewUpdateChannel channel) => channel switch
    {
        MapViewUpdateChannel.Channel0 => Channel0Requested,
        MapViewUpdateChannel.Channel1 => Channel1Requested,
        _ => throw new ArgumentOutOfRangeException(nameof(channel)),
    };
}

public sealed record MapBlockMutationCommand
{
    public MapBlockMutationCommand(
        MapBlockMutationKind kind,
        WorkingMapBlockCopy copy)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        Kind = kind;
        Copy = copy ?? throw new ArgumentNullException(nameof(copy));
    }

    public MapBlockMutationKind Kind { get; }

    public WorkingMapBlockCopy Copy { get; }
}

public sealed class MapBlockMutationResult
{
    private readonly ReadOnlyCollection<MapViewUpdateChannel> _updateMarks;

    internal MapBlockMutationResult(
        WorkingMapLayout layout,
        MapViewUpdateState updateState,
        IEnumerable<MapViewUpdateChannel> updateMarks)
    {
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        UpdateState = updateState ?? throw new ArgumentNullException(nameof(updateState));
        ArgumentNullException.ThrowIfNull(updateMarks);
        _updateMarks = updateMarks.ToList().AsReadOnly();
    }

    public WorkingMapLayout Layout { get; }

    public MapViewUpdateState UpdateState { get; }

    public IReadOnlyList<MapViewUpdateChannel> UpdateMarks => _updateMarks;
}

public static class MapBlockMutationReducer
{
    private static readonly MapViewUpdateChannel[] SetBlocksMarks =
    [
        MapViewUpdateChannel.Channel0,
        MapViewUpdateChannel.Channel1,
    ];

    public static MapBlockMutationResult Apply(
        WorkingMapLayout layout,
        MapViewUpdateState updateState,
        MapBlockMutationCommand command)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(updateState);
        ArgumentNullException.ThrowIfNull(command);

        WorkingMapLayout updatedLayout = layout.ApplyBlockCopy(command.Copy);
        return command.Kind switch
        {
            MapBlockMutationKind.SetBlocks => new MapBlockMutationResult(
                updatedLayout,
                new MapViewUpdateState(
                    Channel0Requested: true,
                    Channel1Requested: true),
                SetBlocksMarks),
            MapBlockMutationKind.SetBlocksVar => new MapBlockMutationResult(
                updatedLayout,
                updateState,
                []),
            _ => throw new ArgumentOutOfRangeException(nameof(command)),
        };
    }
}
