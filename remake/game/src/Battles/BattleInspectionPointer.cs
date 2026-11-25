using Godot;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleInspectionView
{
    private bool _pointerFocus;
    private bool _hoverOnLeft;
    private float _hoverColumnWidth = BattleStatusCard.BaseWidth;
    private Vector2 _edgeSpeed, _edgeMouse;
    private double _edgeDelay;
    private void StopEdge() { _edgeSpeed = Vector2.Zero; _edgeDelay = 0; }
    private void PresentHover()
    {
        var marker = _snapshot!.Units.FirstOrDefault(u => u.Position == _cursor);
        var query = marker is null ? null : _session!.QueryBattleInspection(_snapshot.SessionId, _snapshot.Revision, marker.Actor);
        var tile = _session!.Current.Battle.Definition.Terrain[_cursor.Y * 48 + _cursor.X];
        _hover.PresentInspected(query?.Unit, marker is null ? "" : UnitName(marker));
        if (query?.Land is { } land) _terrainPanel.Present(land);
        else if (marker is not null) _terrainPanel.Present(_cursor, "—");
        else
        {
            bool blocked = tile.Surface is TerrainSurface.Barrier or TerrainSurface.HighSky or TerrainSurface.Impassable or TerrainSurface.Water;
            string effect = blocked ? GameText.Get("battle.inspect.ground-impassable") :
                LandValue(BattleTerrainRules.LandEffect(tile, BattleMover.Regular));
            _terrainPanel.Present(_cursor, effect);
        }
        ArrangeHover(_map.CellRect(_cursor));
    }
    internal void ArrangeHover(Rect2 cell)
    {
        if (!Active || _page != "Cursor" || !_terrainPanel.IsVisibleInTree()) return;
        float margin = StoryTheme.GameplayInset, top = StoryTheme.BelowMenu;
        float viewportWidth = GetViewportRect().Size.X;
        if (_hover.Visible)
        {
            _hover.Fit(viewportWidth - 2 * margin);
            _hover.Size = _hover.RequestedSize;
            _hoverColumnWidth = _hover.Size.X;
        }
        float terrainOffset = _hover.Visible ? _hover.Size.Y + BattleTerrainPanel.CardGap : 0;
        float width = _hover.Visible ? Mathf.Max(_hover.Size.X, _terrainPanel.Size.X) : _terrainPanel.Size.X;
        float right = viewportWidth - margin - Mathf.Max(_hoverColumnWidth, width);
        var occupied = new Vector2(width, terrainOffset + _terrainPanel.Size.Y);
        var protection = cell.Grow(16);
        // For an empty cell, try the last column first so the reading stays with the inspected area.
        bool preferLeft = !_hover.Visible && _hoverOnLeft;
        float x = preferLeft ? margin : right;
        if (cell.HasArea() && new Rect2(new Vector2(x, top), occupied).Intersects(protection))
            x = preferLeft ? right : margin;
        _hoverOnLeft = x == margin;
        _hover.Position = new(x, top);
        _terrainPanel.Position = new(x, top + terrainOffset);
    }
    private static string LandValue(int? percent) => percent is { } value ? GameText.Get("battle.inspect.land-percent", value) : "—";
    private bool CursorUiCovers(Vector2 position) => _footer.GetGlobalRect().HasPoint(position) ||
        _hover.IsVisibleInTree() && _hover.GetGlobalRect().HasPoint(position) ||
        _terrainPanel.IsVisibleInTree() && _terrainPanel.GetGlobalRect().HasPoint(position);
    private bool HostPointerCovered => GetViewport().GuiGetHoveredControl() is { } control &&
        control.IsVisibleInTree() && !IsAncestorOf(control);
    private void BoardPointer(InputEventMouse mouse)
    {
        bool allowed = _map.TryCell(mouse.Position, out var cell);
        if (mouse is InputEventMouseMotion move)
        {
            if (!_pointer.Moved(move)) return;
            bool rim = mouse.Position.Y < 8 || mouse.Position.Y >= GetViewportRect().Size.Y - 8;
            TrackEdge(mouse.Position, (allowed || rim && GetGlobalRect().HasPoint(mouse.Position)) && move.ButtonMask == 0);
            if (allowed) { _cursor = cell; _pointerFocus = true; Project(); }
        }
        else if (mouse is InputEventMouseButton button)
        {
            StopEdge();
            if (button.ButtonIndex != MouseButton.Left) { _pointer.Read(button, null, default, _page); return; }
            if (allowed && button.Pressed) { _cursor = cell; _pointerFocus = true; Project(); }
            if (_pointer.Read(button, allowed ? cell.ToString() : null, allowed ? _map.CellRect(cell) : default, _page) is not null)
                Handle(GameAction.Confirm);
        }
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
        if (!Active || _page != "Cursor" || !IsVisibleInTree() || !GetWindow().HasFocus() || !CanProcess() || CursorUiCovers(_edgeMouse) || HostPointerCovered ||
            Godot.Input.IsMouseButtonPressed(MouseButton.Left) || Godot.Input.IsMouseButtonPressed(MouseButton.Right)) { StopEdge(); return; }
        double travel = Math.Max(0, delta - _edgeDelay); _edgeDelay = Math.Max(0, _edgeDelay - delta);
        if (travel <= 0 || !_map.PanInspection(_edgeSpeed, travel)) return;
        if (_map.TryCell(_edgeMouse, out var cell)) { _cursor = cell; _pointerFocus = true; Project(); }
    }
}
