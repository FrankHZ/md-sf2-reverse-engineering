using Godot;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    // F4 starts at the command decision. Both enemies are adjacent; F8 has its own deployment.
    private readonly Vector2I[] _battleCells = [new(7, 9), new(8, 9), new(9, 9), new(8, 8)];
    private Vector2I _moveOrigin = new(8, 9), _moveCursor;
    private HashSet<Vector2I> _stops = [];
    private int _secondEnemyHp = 12;
    private readonly List<int> _targets = [];

    private UnitSummary BattleUnit(int id) => id switch
    {
        0 => Bowie, 1 => Sarah, 2 => Gizmo with { Name = Gizmo.Name + " A" },
        3 => Gizmo with { Name = Gizmo.Name + " B", Hp = _secondEnemyHp },
        _ => throw new ArgumentOutOfRangeException(nameof(id))
    };

    private void BeginMovement()
    {
        _moveCursor = _battleCells[1] = _moveOrigin;
        _stage.Camera.Reveal(_moveCursor);
        var enemies = _battleCells.Skip(2).Where((_, i) => BattleUnit(i + 2).Hp > 0).Select(p => new MapPosition(p.X, p.Y));
        _stops = _stage.MovementCells(_moveOrigin, enemies);
        _stops.Remove(_battleCells[0]);
        Show(Page.Move);
    }

    // Original ring visitation order for the staged rod, herb and HEAL levels.
    private static readonly Vector2I[][] Rings =
    [
        [Vector2I.Zero],
        [Vector2I.Down, Vector2I.Right, Vector2I.Up, Vector2I.Left],
        [new(0,-2), new(-1,-1), new(-2,0), new(-1,1), new(0,2), new(1,1), new(2,0), new(1,-1)]
    ];

    private void ProjectBattleRange()
    {
        _targets.Clear();
        var marks = new List<Vector2I>();
        bool active = _scene == 3 && !_showMembers && _journey == JourneyPage.Closed;
        if (active && _page == Page.Move) marks.AddRange(_stops);
        else if (active)
        {
            var action = _page == Page.Target ? _targetParent : _page;
            int row = _page == Page.Target ? _offer : _focus;
            bool attack = action == Page.Root && (_page == Page.Target || row == 0);
            if (attack || action == Page.Spells && row < 2 || action == Page.Items && row == 0)
            {
                var (min, max) = _stage.ActionRange(attack, action == Page.Spells, row);
                for (int radius = max; radius >= min; radius--)
                    foreach (var offset in Rings[radius])
                    {
                        var cell = _battleCells[1] + offset;
                        if (!new Rect2I(Vector2I.Zero, _stage.Camera.MapSize).HasPoint(cell)) continue;
                        if (_stage.RangeTileVisible(cell)) marks.Add(cell);
                        int unit = Array.IndexOf(_battleCells, cell);
                        if (unit >= 0 && (unit >= 2) == attack && BattleUnit(unit).Hp > 0) _targets.Add(unit);
                    }
            }
        }
        _stage.SetBattleBoard(_battleCells, _enemyHp, _secondEnemyHp, marks, _page == Page.Move, _steadyIcons);
    }

    private void MoveCursor(Vector2I cell, bool reveal = true)
    {
        if (!new Rect2I(Vector2I.Zero, _stage.Camera.MapSize).HasPoint(cell) || cell == _moveCursor) return;
        _moveCursor = cell;
        if (reveal) _stage.Camera.Reveal(cell);
        Render();
    }

    private void Target()
    {
        ProjectBattleRange();
        _targetParent = _page;
        _target = _targets.FirstOrDefault(-1);
        if (_target >= 0) _stage.Camera.Reveal(_battleCells[_target]);
        Show(Page.Target);
    }

    private void CycleTarget(int direction)
    {
        if (_targets.Count < 2) return;
        int index = _targets.IndexOf(_target);
        _target = _targets[(index + direction + _targets.Count) % _targets.Count];
        _stage.Camera.Reveal(_battleCells[_target]);
        Render();
    }
}
