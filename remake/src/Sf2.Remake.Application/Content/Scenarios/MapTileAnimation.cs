namespace Sf2.Remake.Application.Content.Scenarios;

public sealed record MapTileReplacement(ExplorationRaster Original, ExplorationRaster Hd);

public sealed record MapTilePhase(int Source, int Ticks,
    IReadOnlyDictionary<int, MapTileReplacement> Blocks,
    IReadOnlyDictionary<BlockNeighborhood, ExplorationRaster> Contexts);

public sealed class MapTileAnimation(int target, int count, IReadOnlyList<MapTilePhase> phases)
{
    public int Target { get; } = target;
    public int Count { get; } = count;
    public IReadOnlyList<MapTilePhase> Phases { get; } = phases;
    private readonly int _period = phases.Sum(phase => phase.Ticks);

    // The first enabled service submits the transfer. The next one displays it.
    public int PhaseAt(long enabledTicks)
    {
        if (enabledTicks < 2) return -1;
        long tick = (enabledTicks - 2) % _period;
        for (int i = 0; i < Phases.Count; i++)
        {
            if (tick < Phases[i].Ticks) return i;
            tick -= Phases[i].Ticks;
        }
        throw new InvalidOperationException("map-animation-period");
    }
}
