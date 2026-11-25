using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal sealed partial class ExplorationPresentation
{
    private ScenePaletteState? _scenePalette;
    private IReadOnlyList<ushort>? _sceneTerrain;
    private WaitToken? _reducedPulse;
    private IReadOnlyList<ushort>? _reducedTerrain;
    private WaitToken? _sceneEnd;
    private int SceneActorOffset => _scenePalette?.ActorOffset ?? 0;
    private Color? SceneBackdrop => _sceneTerrain is { } bank
        ? new Color((bank[0] & 14) / 14f, ((bank[0] >> 4) & 14) / 14f, ((bank[0] >> 8) & 14) / 14f) : null;

    private bool ProjectScenePalette(SessionSnapshot current, List<SessionCommand> completions)
    {
        _scenePalette = current.Story.Display?.Scene;
        _sceneTerrain = _scenePalette?.Terrain;
        if (_scenePalette is not null && current.Story.Wait is not FullFadeWait) _owner.Modulate = Colors.White;
        if (current.Story.Wait is SceneEndWait ended)
        {
            _owner.Modulate = Colors.Black;
            if (_sceneEnd != ended.Token) { _audio.CutMusic(); _sceneEnd = ended.Token; }
            return true;
        }
        if (_reducedFlash && current.Story.Wait is ScenePaletteWait { Operation: PulseScenePalette pulse } storm)
        {
            if (_reducedPulse != storm.Token)
            {
                _reducedPulse = storm.Token;
                _reducedTerrain = Array.AsReadOnly(storm.Previous.Terrain.Select((color, index) =>
                {
                    int target = pulse.Colors[index], result = 0;
                    for (int shift = 0; shift <= 8; shift += 4)
                        result |= ((((color >> shift) & 14) * 3 + ((target >> shift) & 14)) / 4 & 14) << shift;
                    return (ushort)result;
                }).ToArray());
            }
            _sceneTerrain = storm.LogicalDone ? storm.Previous.Terrain : _reducedTerrain;
        }
        if (current.Story.Wait is not ScenePaletteWait palette) return false;
        if (_cue != palette.Token)
        { _gesture = null; _mosaic = null; _storyEffect = null; _shivering = _nodding = false; }
        ActiveCue = "ScenePalette";
        _cue = palette.Token;
        _cueResource = palette.Operation is PulseScenePalette ? "bank-pulse" : "bank-fade";
        if (palette.LogicalDone && !palette.ActualDone)
        {
            completions.Add(current.Story.EntrySkip is null
                ? new CompletePresentation(palette.Token, PresentationCueKind.ScenePalette)
                : new CompleteSkippedEntryPresentation(palette.Token, PresentationCueKind.ScenePalette));
            CompletedCueToken = palette.Token.Value;
            CompletedCueKind = "ScenePalette";
        }
        return true;
    }

    private static void OffsetSprite(Image image, int offset)
    {
        if (offset == 0) return;
        for (int y = 0; y < image.GetHeight(); y++)
            for (int x = 0; x < image.GetWidth(); x++)
            {
                var color = image.GetPixel(x, y);
                image.SetPixel(x, y, new(Math.Clamp(color.R + offset / 7f, 0, 1),
                    Math.Clamp(color.G + offset / 7f, 0, 1), Math.Clamp(color.B + offset / 7f, 0, 1), color.A));
            }
    }
}
