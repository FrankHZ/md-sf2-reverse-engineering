using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using static Sf2.Remake.Content.Scenarios.ScenarioJson;

namespace Sf2.Remake.Content.Scenarios;

internal static class ScenePaletteReader
{
    private static IReadOnlyList<ushort> Colors(JsonElement row, string name)
    {
        var colors = Array(row, name).Select(value =>
        {
            Require(value.TryGetUInt16(out ushort color) && PaletteColorTransition.Valid(color), "palette-word", name);
            return color;
        }).ToArray();
        Require(colors.Length == 16, "scene-palette-color-count", name);
        return System.Array.AsReadOnly(colors);
    }
    internal static ScenePaletteDefinition? Resource(JsonElement map)
    {
        if (!map.TryGetProperty("scenePalette", out var row)) return null;
        Object(row, "scenePalette", "indices", "colors");
        byte[] indices = Convert.FromBase64String(Text(row, "indices"));
        Require(indices.Length == 128 * 320 && indices.All(index => index < 16), "scene-palette-indices", "map.scenePalette");
        return new(indices, Colors(row, "colors"));
    }
    internal static StoryInstruction? Instruction(JsonElement row, string opcode)
    {
        if (opcode == "end-scene")
        { Object(row, opcode, "op"); return new EndScene(); }
        if (opcode == "scene-palette-fade")
        {
            Object(row, opcode, "op", "banks", "offsets", "period");
            int banks = Number(row, "banks", 1, 15);
            Require(banks is 1 or 15, "scene-palette-banks", "program.banks");
            var offsets = Array(row, "offsets").Select(value =>
            {
                Require(value.TryGetInt32(out int offset) && offset is >= -7 and <= 7, "scene-palette-offset", "program.offsets");
                return offset;
            }).ToArray();
            Require(offsets.Length is > 0 and <= 8, "scene-palette-offset-count", "program.offsets");
            return new FadeScenePalette(banks, System.Array.AsReadOnly(offsets), Number(row, "period", 1, 60));
        }
        if (opcode == "scene-palette-pulse")
        {
            Object(row, opcode, "op", "bank", "colors", "ticks");
            return new PulseScenePalette(Number(row, "bank", 0, 0), Colors(row, "colors"), Number(row, "ticks", 1, 60));
        }
        return null;
    }
}
