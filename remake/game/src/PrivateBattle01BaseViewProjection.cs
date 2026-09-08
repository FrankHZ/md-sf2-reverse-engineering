using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

// A rendered value of the admitted fixed layout, never another battle/layout authority.
internal sealed class PrivateBattle01BaseViewProjection
{
    private PrivateBattle01BaseViewProjection(OriginalBattle01AdmissionDefinition definition,
        int scale, byte[] pixels)
    {
        Definition = definition;
        RasterScale = scale;
        RgbaBytes = Array.AsReadOnly(pixels);
    }

    internal OriginalBattle01AdmissionDefinition Definition { get; }
    internal int RasterScale { get; }
    internal int PixelWidth => Definition.BattleAreaWidth * PrivateOriginalMapBaseViewProjection.BlockPixelSize;
    internal int PixelHeight => Definition.BattleAreaHeight * PrivateOriginalMapBaseViewProjection.BlockPixelSize;
    internal IReadOnlyList<byte> RgbaBytes { get; }

    internal static PrivateBattle01BaseViewProjection Create(OriginalBattle01AdmissionDefinition definition,
        IReadOnlyList<byte> atlasRgbaBytes, int scale)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!OriginalMapRuntimeAdmission.HasExactAcceptedBattle01Admission(definition))
            throw new ArgumentException("Map 57 base art requires the unchanged admitted Battle01 layout.", nameof(definition));
        ValidateReferencedSlots(definition.WorkingLayout, definition.BlockCatalog);
        return new(definition, scale, PrivateOriginalMapBaseViewProjection.RenderAtlasRegion(
            definition.WorkingLayout, definition.BlockCatalog, atlasRgbaBytes, scale,
            definition.BattleAreaX, definition.BattleAreaY, definition.BattleAreaWidth, definition.BattleAreaHeight));
    }

    internal static void ValidateReferencedSlots(WorkingMapLayout layout, OriginalMapBlockCatalog blocks)
    {
        // Inspect only records reached by this layout. Unused seed blocks 1/2
        // contain slot4 references and are deliberately not rendered or admitted as empty art.
        foreach (int index in layout.Words.Select(word => word & OriginalMapTraversal.LayoutBlockIndexMask).Distinct())
            if (PrivateOriginalMapBaseViewProjection.ReferencesUnloadedAtlasSlot(blocks.Resolve(index)))
                throw new ArgumentException("The Map 57 layout references an unloaded atlas slot.", nameof(layout));
    }
}
