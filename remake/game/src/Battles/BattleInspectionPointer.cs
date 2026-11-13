using Godot;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleInspectionView
{
    private bool _pointerFocus;
    private Vector2 _edgeSpeed, _edgeMouse;
    private double _edgeDelay;
    private void StopEdge() { _edgeSpeed = Vector2.Zero; _edgeDelay = 0; }
    private void PresentHover()
    {
        var marker = _snapshot!.Units.FirstOrDefault(u => u.Position == _cursor);
        var query = marker is null ? null : _session!.QueryBattleInspection(_snapshot.SessionId, _snapshot.Revision, marker.Actor);
        var tile = _session!.Current.Battle.Definition.Terrain[_cursor.Y * 48 + _cursor.X];
        string land;
        if (marker is null)
        {
            bool blocked = tile.Surface is TerrainSurface.Barrier or TerrainSurface.HighSky or TerrainSurface.Impassable or TerrainSurface.Water;
            land = blocked ? GameText.Get("battle.inspect.ground-impassable") :
                GameText.Get("battle.inspect.ground-effect", LandValue(BattleTerrainRules.LandEffect(tile, BattleMover.Regular)));
        }
        else land = LandValue(query?.Land?.Percent);
        _hover.Present(query?.Unit, marker is null ? GameText.Get("battle.inspect.empty") : UnitName(marker), land, _cursor);
        var cell = _map.CellRect(_cursor);
        _hover.Position = new(cell.GetCenter().X >= GetViewportRect().Size.X - 650 ? 32 : GetViewportRect().Size.X - 592, 238);
    }
    private static string LandValue(int? percent) => percent is { } value ? GameText.Get("battle.inspect.land-percent", value) : "—";
    private bool CursorUiCovers(Vector2 position) => _hint.GetGlobalRect().HasPoint(position) ||
        _hover.IsVisibleInTree() && _hover.GetGlobalRect().HasPoint(position);
    private bool HostPointerCovered => GetViewport().GuiGetHoveredControl() is { } control &&
        control.IsVisibleInTree() && !IsAncestorOf(control);
    private bool BoardPointer(InputEvent input)
    {
        if (input is not InputEventMouse mouse) return false;
        if (_page == "Cursor" && (CursorUiCovers(mouse.Position) || HostPointerCovered))
        { _pointer.Cancel(); StopEdge(); return true; }
        if (_page is "Full" or "MemberFull" or "Members" or "Map")
        {
            if (_page == "Map" && input is InputEventMouseMotion motion && _pointer.Moved(motion))
            {
                int index = _overview.Hit(mouse.Position);
                if (index >= 0) { _knownIndex = index; Project(); }
            }
            if (input is InputEventMouseButton button)
            {
                bool back = button.ButtonIndex == MouseButton.Right || _hint.GetGlobalRect().HasPoint(mouse.Position);
                int hit = _page == "Map" ? _overview.Hit(mouse.Position) : -1;
                string? id = back ? "back" : hit >= 0 ? "unit:" + hit : null;
                if (_pointer.Read(button, id, back ? GetGlobalRect() : _overview.GetGlobalRect(), _page) is { } clicked)
                {
                    if (clicked == "back") Handle(GameAction.Cancel);
                    else { _knownIndex = hit; _cursor = _snapshot!.Units[hit].Position; _page = "Cursor"; _pointerFocus = false; Project(); }
                }
            }
            return false;
        }
        if (_page != "Cursor") return false;
        bool allowed = _map.TryCell(mouse.Position, out var cell);
        if (input is InputEventMouseMotion move)
        {
            if (!_pointer.Moved(move)) return true;
            bool rim = mouse.Position.Y < 8 || mouse.Position.Y >= GetViewportRect().Size.Y - 8;
            TrackEdge(mouse.Position, (allowed || rim && GetGlobalRect().HasPoint(mouse.Position)) && move.ButtonMask == 0);
            if (allowed) { _cursor = cell; _pointerFocus = true; Project(); }
        }
        else if (input is InputEventMouseButton button)
        {
            StopEdge();
            if (button.ButtonIndex == MouseButton.Right)
            {
                if (_pointer.Read(button, "back", GetGlobalRect(), _page) is not null) Handle(GameAction.Cancel);
                return true;
            }
            if (allowed && button.Pressed) { _cursor = cell; _pointerFocus = true; Project(); }
            if (_pointer.Read(button, allowed ? cell.ToString() : null, allowed ? _map.CellRect(cell) : default, _page) is not null)
                Handle(GameAction.Confirm);
        }
        return true;
    }
    private void TrackEdge(Vector2 position, bool allowed)
    {
        if (!allowed) { StopEdge(); return; }
        var rect = _map.GetGlobalRect();
        static float Axis(float p, float lo, float hi) => Mathf.Clamp((p - hi + 32) / 32, 0, 1) - Mathf.Clamp((lo + 32 - p) / 32, 0, 1);
        var strength = new Vector2(Axis(position.X, rect.Position.X, rect.End.X), Axis(position.Y, rect.Position.Y, rect.End.Y));
        if (_snapshot!.Width * 96 <= rect.Size.X) strength.X = 0;
        if (_snapshot.Height * 96 <= rect.Size.Y) strength.Y = 0;
        if (strength == Vector2.Zero) { StopEdge(); return; }
        if (strength.Sign() != _edgeSpeed.Sign()) _edgeDelay = 0.18;
        _edgeMouse = position; _edgeSpeed = strength.Normalized() * Mathf.Lerp(192, 576, Mathf.Max(Mathf.Abs(strength.X), Mathf.Abs(strength.Y)));
    }
    internal void AdvancePointer(double delta)
    {
        if (_edgeSpeed == Vector2.Zero) return;
        if (!Active || _page != "Cursor" || !IsVisibleInTree() || !GetWindow().HasFocus() || !CanProcess() || HostPointerCovered ||
            Godot.Input.IsMouseButtonPressed(MouseButton.Left) || Godot.Input.IsMouseButtonPressed(MouseButton.Right)) { StopEdge(); return; }
        double travel = Math.Max(0, delta - _edgeDelay); _edgeDelay = Math.Max(0, _edgeDelay - delta);
        if (travel <= 0 || !_map.PanInspection(_edgeSpeed, travel)) return;
        if (_map.TryCell(_edgeMouse, out var cell)) { _cursor = cell; _pointerFocus = true; Project(); }
    }
}
