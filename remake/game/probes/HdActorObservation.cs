using System.Reflection;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
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
        return (ExplorationRaster)Activator.CreateInstance(typeof(ExplorationRaster),
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            [width, height, "rgba8", image.GetData()], null)!;
    }

    private static void Check(bool value, string label)
    {
        if (!value) throw new InvalidOperationException(label);
    }

    private void Observe()
    {
        var portrait = new ExplorationPortraitVisual(0, Raster(64, 64, Colors.Red),
            HdRaster: Raster(256, 256, Colors.Blue));
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
        foreach (bool hd in new[] { false, true, false })
        {
            using var texture = ExplorationRasterImages.Portrait(portrait, Enumerable.Range(0, 64).Reverse().ToArray(), true, hd);
            Check(texture.GetWidth() == (hd ? 192 : 48) && texture.GetHeight() == (hd ? 256 : 64), "portrait crop geometry");
        }
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
        node.Texture = null;
        node.Free();
        viewport.Free();
    }
}
