using Godot;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Previews;

internal sealed partial class PreviewStage
{
    private Vector2I[] _actors = [];
    private readonly Node2D _ground = new() { Name = "BoardGround", ShowBehindParent = true };
    private readonly MultiMeshInstance2D _rangeTiles = new()
    {
        Name = "RangeTiles", ShowBehindParent = true,
        Texture = CreateRangeTile(),
        Multimesh = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
            Mesh = new QuadMesh { Size = new Vector2(92, 92) } }
    };
    private bool _firstEnemyVisible, _secondEnemyVisible, _steady;
    private double _pulse;
    private const float SteadyOpacity = 0.89f, PulseAmplitude = 0.11f;

    private static ImageTexture CreateRangeTile()
    {
        // Each texel covers 2×2 logical pixels in the 92px quad.
        using var tile = Image.CreateEmpty(46, 46, false, Image.Format.Rgba8);
        tile.Fill(new Color("18202b"));
        tile.FillRect(new Rect2I(1, 1, 44, 44), Colors.White);
        tile.FillRect(new Rect2I(3, 3, 40, 40), new Color(Colors.White, 0.68f));
        return ImageTexture.CreateFromImage(tile);
    }

    internal bool RangeTileVisible(Vector2I cell) =>
        _battle?.Terrain[cell.Y * 48 + cell.X].Surface != TerrainSurface.Barrier;

    internal (int Min, int Max) ActionRange(bool attack, bool spell, int row)
    {
        if (attack)
        {
            var rod = _battle?.Deployments.FirstOrDefault(d => d.ProcessingOrder == 1)?.Definition.Physical;
            return (rod?.MinimumRange ?? 1, rod?.MaximumRange ?? 1);
        }
        if (spell)
        {
            var heal = _battle?.Spells.GetValueOrDefault(new("heal", (byte)(row + 1)));
            return (heal?.MinimumRange ?? 0, heal?.MaximumRange ?? row + 1);
        }
        var herb = _battle?.HealingItems.GetValueOrDefault((byte)0);
        return (herb?.MinimumRange ?? 0, herb?.MaximumRange ?? 1);
    }

    internal HashSet<Vector2I> MovementCells(Vector2I origin, IEnumerable<MapPosition> enemies)
    {
        var actor = _battle?.Deployments.FirstOrDefault(d => d.ProcessingOrder == 1)?.Definition;
        var size = Camera.MapSize;
        var terrain = _battle?.Terrain ?? Enumerable.Range(0, 48 * 48).Select(i =>
            new BattleTerrain(i % 48 < size.X && i / 48 < size.Y ? TerrainSurface.Open : TerrainSurface.Barrier,
                TerrainProtection.None)).ToArray();
        return BattleTerrainRules.ReachableCells(terrain, new(origin.X, origin.Y), actor?.Mover ?? BattleMover.Healer,
            actor?.Move ?? 5, enemies).Select(p => new Vector2I(p.X, p.Y)).ToHashSet();
    }

    internal void SetBattleBoard(Vector2I[] actors, int firstHp, int secondHp, IReadOnlyList<Vector2I> marks,
        bool movement, bool steady)
    {
        _actors = actors;
        _firstEnemyVisible = firstHp > 0;
        _secondEnemyVisible = secondHp > 0;
        _steady = steady;
        _rangeTiles.Visible = marks.Count > 0;
        var mesh = _rangeTiles.Multimesh;
        mesh.InstanceCount = marks.Count;
        _rangeTiles.Position = Camera.Origin;
        for (int i = 0; i < marks.Count; i++)
            mesh.SetInstanceTransform2D(i, new Transform2D(0, ((Vector2)marks[i] + Vector2.One * 0.5f) * PreviewInspectionCamera.Tile));
        _rangeTiles.Modulate = new Color(new Color(movement ? "78e9f3" : "ffda7a"), SteadyOpacity);
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (!_rangeTiles.IsVisibleInTree() || _steady) return;
        _pulse = (_pulse + delta) % 1.2;
        _rangeTiles.Modulate = new Color(_rangeTiles.Modulate, SteadyOpacity + PulseAmplitude * Mathf.Cos((float)(_pulse * Math.Tau / 1.2)));
    }
}
