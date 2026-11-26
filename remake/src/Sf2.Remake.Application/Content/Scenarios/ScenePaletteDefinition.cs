using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.Application.Content.Scenarios;

// Optional original terrain indices. A slot retains its identity even when RGB values coincide.
public sealed class ScenePaletteDefinition
{
    private readonly byte[] _indices;
    public IReadOnlyList<ushort> Base { get; }
    public IReadOnlyList<ScenePaletteHdBank> HdBanks { get; }
    public ScenePaletteDefinition(IEnumerable<byte> indices, IEnumerable<ushort> colors,
        IEnumerable<ScenePaletteHdBank>? hdBanks = null)
    {
        _indices = indices.ToArray();
        Base = Array.AsReadOnly(colors.ToArray());
        HdBanks = Array.AsReadOnly((hdBanks ?? []).ToArray());
        if (_indices.Length != 128 * 320 || _indices.Any(index => index > 15) ||
            Base.Count != 16 || Base.Any(color => !PaletteColorTransition.Valid(color)))
            throw new ArgumentException("scene-palette-resource");
    }
    public byte Slot(int x, int y) => _indices[y * 128 + x];
}

public sealed record ScenePaletteHdBank(IReadOnlyList<ushort> Colors,
    IReadOnlyList<ExplorationRaster> Blocks,
    IReadOnlyDictionary<BlockNeighborhood, ExplorationRaster>? Contexts);

public abstract record ScenePaletteOperation : StoryInstruction;
public sealed record FadeScenePalette(int Banks, IReadOnlyList<int> Offsets, int Period) : ScenePaletteOperation;
public sealed record PulseScenePalette(int Bank, IReadOnlyList<ushort> Colors, int Ticks) : ScenePaletteOperation;
public sealed record EndScene : StoryInstruction;
