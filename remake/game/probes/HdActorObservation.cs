using System.Reflection;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Exploration;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Probes;

// Original synthetic colors exercise actual texture consumers without private art.
public partial class HdActorObservation : Node
{
    public override void _Ready()
    {
        try
        {
            Observe();
            GD.Print("HD-ACTOR-OBSERVATION PASS");
            GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }

    private static ExplorationRaster Raster(int width, int height, Color color)
    {
        using var image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        image.Fill(color);
        return Raster(image);
    }

    private static ExplorationRaster Raster(Image image) => Raster(image.GetWidth(), image.GetHeight(), "rgba8", image.GetData());

    private static ExplorationRaster Raster(int width, int height, string format, byte[] bytes)
    {
        return (ExplorationRaster)Activator.CreateInstance(typeof(ExplorationRaster),
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            [width, height, format, bytes], null)!;
    }

    private static ExplorationRaster BrokenPng(int width, int height)
    {
        using var image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        image.Fill(Colors.Red);
        // A complete IHDR passes content shape admission; missing IDAT fails native decode.
        return Raster(width, height, "png", image.SavePngToBuffer()[..33]);
    }

    private static Color TileColor(int tile) => Color.Color8((byte)(tile * 3), (byte)(255 - tile * 3), (byte)(tile * 2));

    private static ExplorationPortraitVisual Portrait()
    {
        using var atlas = Image.CreateEmpty(64, 64, false, Image.Format.Rgba8);
        for (int tile = 0; tile < 64; tile++)
            atlas.FillRect(new(tile % 8 * 8, tile / 8 * 8, 8, 8), TileColor(tile));
        var original = Raster(atlas);
        atlas.Resize(256, 256, Image.Interpolation.Nearest);
        return new(0, original, [new(1, 2, 6, 2)], [new(2, 4, 7, 4)], Raster(atlas));
    }

    private static void ObservePortrait(ExplorationPortraitVisual portrait)
    {
        foreach (bool hd in new[] { false, true, false })
            foreach (bool mirror in new[] { false, true })
                foreach (bool eyes in new[] { false, true })
                    foreach (bool mouth in new[] { false, true })
                    {
                        var tiles = Enumerable.Range(0, 64).ToArray();
                        foreach (var change in eyes ? portrait.Eyes! : [])
                            tiles[change.Y * 8 + change.X] = change.AlternateY * 8 + change.AlternateX;
                        foreach (var change in mouth ? portrait.Mouth! : [])
                            tiles[change.Y * 8 + change.X] = change.AlternateY * 8 + change.AlternateX;
                        using var texture = ExplorationRasterImages.Portrait(portrait, tiles, mirror, hd);
                        int density = hd && portrait.HdRaster is not null ? 4 : 1;
                        Check(texture.GetWidth() == 48 * density && texture.GetHeight() == 64 * density, "portrait crop geometry");
                        using var image = texture.GetImage();
                        for (int y = 0; y < 8; y++)
                            for (int x = 0; x < 6; x++)
                            {
                                int sampleX = (x * 8 + 4) * density;
                                if (mirror) sampleX = image.GetWidth() - 1 - sampleX;
                                Check(image.GetPixel(sampleX, (y * 8 + 4) * density) == TileColor(tiles[y * 8 + x]),
                                    "portrait tile replacement and mirror");
                            }
                    }
    }

    private static void Check(bool value, string label)
    {
        if (!value) throw new InvalidOperationException(label);
    }

    private void Observe()
    {
        var portrait = Portrait();
        ObservePortrait(portrait);
        ObservePortrait(portrait with { HdRaster = null });
        ObserveExploration();
        var sprites = new Dictionary<int, ExplorationSpriteVisual>();
        foreach (int id in new[] { 0, 255 })
        {
            int size = id == 0 ? 24 : 48;
            sprites[id] = new(id, Enumerable.Range(0, 3).Select(_ => Raster(size * 2, size, Colors.Red)).ToArray(), 0, 0,
                Enumerable.Range(0, 3).Select(_ => Raster(size * 8, size * 4, Colors.Blue)).ToArray());
        }
        var visuals = new ExplorationVisuals(new Dictionary<Sf2.Remake.Domain.Maps.MapId, ExplorationMapVisual>(),
            sprites, new Dictionary<int, ExplorationPortraitVisual> { [0] = portrait }, new Dictionary<string, ExplorationAudio>());
        Check(MemberPortrait.ForSprite(visuals, 0, true)?.Hd == true, "portrait independent of absent map");
        using var source = Image.CreateEmpty(24, 24, false, Image.Format.Rgba8);
        source.Fill(Colors.Red);
        source.FillRect(new(0, 16, 24, 8), Colors.Black);
        source.SetPixel(8, 16, Colors.Red);
        using var dense = Image.CreateEmpty(96, 96, false, Image.Format.Rgba8);
        dense.Fill(Colors.Black);
        ExplorationStoryEffects.RemoveShadow(dense, source);
        Check(dense.GetPixel(0, 64).A == 0 && dense.GetPixel(36, 64).A == 1, "shadow word mask at density four");
        var viewport = new BattleMapViewport();
        AddChild(viewport);
        typeof(BattleMapViewport).GetField("_visuals", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(viewport, visuals);
        var assign = typeof(BattleMapViewport).GetMethod("SetSpriteTexture", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var node = new Sprite2D();
        foreach (string mode in new[] { "original", "hd", "original" })
        {
            viewport.PrepareTerrain(mode);
            viewport.SelectTerrain(mode);
            foreach (int id in sprites.Keys)
                for (int direction = 0; direction < 3; direction++)
                    for (int frame = 0; frame < 2; frame++)
                    {
                        assign.Invoke(viewport, [node, id, direction, frame]);
                        int size = id == 0 ? 24 : 48;
                        Check(node.Texture.GetWidth() == size * (mode == "hd" ? 4 : 1), "battle density");
                        Check(node.Texture.GetWidth() * node.Scale.X == size * Maps.WorldViewportLayout.CellSize / 24f, "battle logical extent");
                        Check(node.Offset.Y * node.Scale.Y == (id == 0 ? 0 : -Maps.WorldViewportLayout.CellSize), "battle registration");
                    }
        }
        ObservePreparation(viewport, node, visuals, assign);
        node.Texture = null;
        node.Free();
        viewport.Free();
    }

    private static void ObservePreparation(BattleMapViewport viewport, Sprite2D node, ExplorationVisuals visuals, MethodInfo assign)
    {
        var source = typeof(BattleMapViewport).GetField("_visuals", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var appliedMode = typeof(BattleMapViewport).GetField("_hd", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var sprite = visuals.Sprites[0];
        var bad = BrokenPng(48, 24);
        var badHd = BrokenPng(192, 96);
        void Bind(ExplorationSpriteVisual actor) => source.SetValue(viewport, visuals with
        { Sprites = new Dictionary<int, ExplorationSpriteVisual> { [0] = actor } });
        void Reject(string mode, string label)
        {
            bool before = (bool)appliedMode.GetValue(viewport)!;
            var texture = node.Texture;
            var scale = node.Scale;
            var offset = node.Offset;
            bool rejected = false;
            try { viewport.PrepareTerrain(mode); }
            catch (InvalidOperationException error) when (error.Message == "raster-decode") { rejected = true; }
            Check(rejected, label + " rejected before publication");
            Check((bool)appliedMode.GetValue(viewport)! == before && node.Texture == texture && node.Scale == scale && node.Offset == offset,
                label + " retains applied mode and texture geometry");
            GD.Print("EXPECTED-DECODE-REJECTION ", label);
        }

        Bind(sprite with { Directions = [bad, bad, bad] });
        viewport.PrepareTerrain("hd");
        viewport.SelectTerrain("hd");
        assign.Invoke(viewport, [node, 0, 0, 0]);
        Reject("original", "valid HD to broken Original");

        Bind(sprite);
        viewport.PrepareTerrain("original");
        viewport.SelectTerrain("original");
        assign.Invoke(viewport, [node, 0, 0, 0]);
        Bind(sprite with
        {
            Graphics = new Dictionary<string, IReadOnlyList<ExplorationRaster>> { ["pose"] = sprite.Directions },
            HdGraphics = new Dictionary<string, IReadOnlyList<ExplorationRaster>> { ["pose"] = [badHd, badHd, badHd] },
        });
        Reject("hd", "broken HD alternate");

        Bind(sprite with { HdDirections = null,
            Graphics = new Dictionary<string, IReadOnlyList<ExplorationRaster>> { ["pose"] = sprite.Directions } });
        viewport.PrepareTerrain("hd");
        viewport.SelectTerrain("hd");
        assign.Invoke(viewport, [node, 0, 0, 0]);
        Check(node.Texture.GetWidth() == 24, "missing HD directions fall back to Original");
        Bind(sprite with { HdDirections = null, Directions = [bad, bad, bad] });
        Reject("hd", "broken Original direction fallback");
        Bind(sprite with { Graphics = new Dictionary<string, IReadOnlyList<ExplorationRaster>> { ["pose"] = [bad, bad, bad] } });
        Reject("hd", "broken Original alternate fallback");

        source.SetValue(viewport, visuals with { Portraits = new Dictionary<int, ExplorationPortraitVisual>
        { [0] = visuals.Portraits[0] with { Raster = BrokenPng(64, 64), HdRaster = null } } });
        Reject("hd", "broken Original portrait fallback");
        source.SetValue(viewport, visuals);
    }

    private void ObserveExploration()
    {
        using var strip = Image.CreateEmpty(96, 48, false, Image.Format.Rgba8);
        strip.Fill(Colors.Transparent);
        for (int half = 0; half < 2; half++)
        {
            strip.FillRect(new(half * 48 + 2, 2, 44, 44), half == 0 ? Colors.Red : Colors.Green);
            strip.FillRect(new(half * 48, 16, 24, 8), Colors.Black);
            strip.SetPixel(half * 48 + 8, 16, Colors.Red);
        }
        var original = Raster(strip);
        strip.Resize(384, 192, Image.Interpolation.Nearest);
        strip.FillRect(new(30 * 4, 30 * 4, 4, 4), Colors.Blue);
        strip.FillRect(new(78 * 4, 30 * 4, 4, 4), Colors.Yellow);
        var hdRaster = Raster(strip);
        var originalGroup = new[] { original, original, original };
        var hdGroup = new[] { hdRaster, hdRaster, hdRaster };
        foreach (bool hasAlternateHd in new[] { true, false })
        {
            var visual = new ExplorationSpriteVisual(252, originalGroup, null, 0, hdGroup,
                new Dictionary<string, IReadOnlyList<ExplorationRaster>> { ["pose"] = originalGroup },
                hasAlternateHd ? new Dictionary<string, IReadOnlyList<ExplorationRaster>> { ["pose"] = hdGroup } : null);
            var visuals = new ExplorationVisuals(new Dictionary<MapId, ExplorationMapVisual>(),
                new Dictionary<int, ExplorationSpriteVisual> { [252] = visual },
                new Dictionary<int, ExplorationPortraitVisual>(), new Dictionary<string, ExplorationAudio>());
            var definition = (ExplorationDefinition)Activator.CreateInstance(typeof(ExplorationDefinition),
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                [Array.Empty<ExplorationMapDefinition>(), Array.Empty<StoryProgram>(), null, null, null, visuals, null, null, null], null)!;
            var owner = new Control();
            AddChild(owner);
            var settings = new InputSettings { FormatVersion = 1 };
            using (var audio = new SessionAudio(owner, definition))
            using (var presentation = new ExplorationPresentation(owner, definition, settings, () => { }, audio, new()))
            {
                typeof(ExplorationPresentation).GetField("_spriteGraphics", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(presentation, new Dictionary<int, string> { [0] = "pose" });
                var select = typeof(ExplorationPresentation).GetMethod("Sprite", BindingFlags.Instance | BindingFlags.NonPublic)!;
                foreach (string mode in new[] { "original", "hd", "original" })
                {
                    if (mode == "hd") presentation.PreparePaintedRasters(new MapId("synthetic"));
                    settings.GraphicsMode = mode;
                    for (int half = 0; half < 2; half++)
                        foreach (bool shadowless in new[] { false, true })
                        {
                            var motion = EntityMotionState.At(new(0, 0), 1, 0) with { AnimationCounter = (byte)(half * 16) };
                            var entity = new ExplorationEntity(new("synthetic"), motion, true, Slot: 0, Sprite: 252,
                                SpriteRequest: 1, ShadowRemovedAt: shadowless ? 1 : null);
                            var texture = (ImageTexture)select.Invoke(presentation, [entity, false, new MapId("synthetic"), 0])!;
                            int density = mode == "hd" && hasAlternateHd ? 4 : 1;
                            Check(texture.GetWidth() == 48 * density && texture.GetHeight() == 48 * density,
                                "exploration alternate density and fallback without terrain HD");
                            using var image = texture.GetImage();
                            var expected = density == 4 ? (half == 0 ? Colors.Blue : Colors.Yellow) :
                                (half == 0 ? Colors.Red : Colors.Green);
                            Check(image.GetPixel(30 * density, 30 * density) == expected, "selected alternate frame pixels");
                            Check(image.GetPixel(0, 0).A == 0 && image.GetPixel(0, 16 * density).A == (shadowless ? 0 : 1) &&
                                image.GetPixel(9 * density, 16 * density) == Colors.Black,
                                "actual exploration shadow and transparent coverage");
                        }
                }
            }
            owner.Free();
        }
        GD.Print("EXPLORATION-ALTERNATE-SHADOW PASS");
    }
}
