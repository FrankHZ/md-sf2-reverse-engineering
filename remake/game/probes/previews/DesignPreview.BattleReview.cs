using Godot;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private enum InspectPage { Cursor, Options, Map, Unit, Speed }
    private InspectPage _inspection;
    private Vector2I _cursor = new(7, 8);
    internal static readonly Vector2I[] UnitCells = [new(7,9), new(9,9), new(8,7)];
    private int _mapFocus, _rate, _draftRate, _outcome, _reading;
    private readonly MenuPointer _boardPointer = new();
    private Vector2 _edgeMouse, _edgeSpeed;
    private double _edgeDelay;
    private bool FreeInspection => _scene == InspectScene && _inspection == InspectPage.Cursor &&
        _journey == JourneyPage.Closed && !_showMembers;

    private UnitSummary InspectedUnit(int i) => i switch { 0 => Bowie, 1 => Sarah, _ => Gizmo };
    private string Rate(int i) => i switch { 1 => "1.5×", 2 => "2×", _ => "1×" };
    private void InspectAt(InspectPage page, int focus = 0)
    {
        _inspection = page; _focus = focus; Render();
    }

    private void DrawInspection()
    {
        int unit = Array.IndexOf(UnitCells, _cursor);
        if (_inspection == InspectPage.Unit)
        {
            Put(_surface, new ColorRect { Color = new Color(0, 0, 0, 0.5f), MouseFilter = MouseFilterEnum.Ignore },
                new(0, 72, 1920, 1008));
            DrawUnitSheet(new(266, 186), unit switch { 0 => Member.Bowie, 1 => Member.Sarah, _ => null });
            ReviewFooter(T("Enter / A  或  Esc / B  返回原地块", "Enter / A or Esc / B  Return to cell"), BackInspection);
            return;
        }
        if (_inspection == InspectPage.Cursor)
        {
            var card = Panel(new(_stage.BoardCell(_cursor, false).GetCenter().X >= 1248 ? 48 : 1296,
                238, 560, unit < 0 ? 150 : 250));
            card.Name = "InspectionCard";
            Text(card, new(16, 10, 252, 56), unit < 0 ? T("空地", "Empty cell") : InspectedUnit(unit).Name, 36);
            var (percent, groundBlocked) = _stage.LandEffect(unit, _cursor);
            string effectLabel = unit < 0 ? T("地面效果", "Ground effect") : T("地形效果", "Land effect");
            string value = percent is { } effect ? $"{effect}%" : "—";
            var land = Text(card, new(272, 10, 256, 56),
                groundBlocked ? T("地面不可通行", "Ground impassable") : $"{effectLabel}  {value}", 28, "cad6e5");
            land.Name = "LandEffect";
            land.HorizontalAlignment = HorizontalAlignment.Right;
            if (unit >= 0)
                Vitals(card, new(16, 66), InspectedUnit(unit));
            Text(card, new(16, unit < 0 ? 72 : 172, 528, 58), T($"古代之塔   ·   地块 ({_cursor.X}, {_cursor.Y})",
                $"Ancient Tower   ·   Cell ({_cursor.X}, {_cursor.Y})"), 26, "cad6e5");
            Footer(T("方向键或鼠标选格     棋盘边缘滚动     Enter / A 或左键确认",
                "Directions or mouse select     Hover at board edge to pan     Enter / A or click to confirm"));
            Utility(new(1530, 1003, 316, 48), unit < 0 ? T("战场选项", "Battlefield options") : T("单位详情", "Unit details"), AdvanceInspection);
            return;
        }
        if (_inspection == InspectPage.Map)
        {
            var legend = SidePage(T("战场地图", "Battlefield map"), 3);
            for (int i = 0; i < UnitCells.Length; i++) Item(CommandRow(i), InspectedUnit(i).Name, value: i == 2 ? T("敌军", "Enemy") : T("我军", "Ally"));
            Text(_surface, new(legend.Position.X, legend.Position.Y + legend.Size.Y + 16, 560, 80),
                T("选择单位，在地图上定位。", "Select a unit to locate it on the map."), 26);
        }
        else if (_inspection == InspectPage.Speed)
        {
            SidePage(T("战斗 / 演出速度", "Battle / Playback speed"), 3);
            foreach (string row in new[] { T("演出速度", "Playback speed"), T("应用", "Apply"), T("取消", "Cancel") })
                Item(CommandRow(_choices.Count), row, value: _choices.Count == 0 ? Rate(_draftRate) : "");
        }
        else
        {
            SidePage(T("战场选项", "Battlefield options"), 4);
            string[] labels = [GameText.Get("field-menu.member"), T("地图", "Map"), T("速度", "Speed"), T("退出", "Quit")];
            string[] icons = ["member", "map", "speed", "quit"];
            for (int i = 0; i < labels.Length; i++) Item(CommandRow(i), labels[i], "battlefield-menu." + icons[i]);
        }
        ReviewFooter(T("↑ ↓ 选择     Enter / A  确认", "↑ ↓ Select     Enter / A  Confirm"), BackInspection);
    }

    private void AdvanceInspection()
    {
        switch (_inspection)
        {
            case InspectPage.Cursor: InspectAt(Array.IndexOf(UnitCells, _cursor) < 0 ? InspectPage.Options : InspectPage.Unit); break;
            case InspectPage.Options:
                if (_focus == 0) ToggleMembers();
                else if (_focus == 1) InspectAt(InspectPage.Map, _mapFocus);
                else if (_focus == 2) { _draftRate = _rate; InspectAt(InspectPage.Speed); }
                else { _priorFocus = _focus; _fromMenu = false; Journey(JourneyPage.Quit, 2); }
                break;
            case InspectPage.Map:
                _mapFocus = _focus; _cursor = UnitCells[_mapFocus];
                _stage.Camera.Reveal(_cursor); InspectAt(InspectPage.Cursor); break;
            case InspectPage.Speed:
                if (_focus == 0) { _draftRate = (_draftRate + 1) % 3; Render(); }
                else { if (_focus == 1) _rate = _draftRate; BackInspection(); }
                break;
            default: BackInspection(); break;
        }
    }

    private void BackInspection()
    {
        if (_inspection == InspectPage.Map) { _mapFocus = _focus; InspectAt(InspectPage.Options, 1); }
        else if (_inspection == InspectPage.Speed) InspectAt(InspectPage.Options, 2);
        else if (_inspection == InspectPage.Cursor) InspectAt(InspectPage.Options);
        else InspectAt(InspectPage.Cursor);
    }

    private string OutcomeName(int i) => i switch { 0 => T("胜利", "Victory"), 1 => T("撤退", "Retreat"), _ => T("败北", "Defeat") };
    private string[] OutcomeLines() => _outcome switch {
        0 => [T("莎拉获得 80 点经验。", "Sarah earns 80 EXP."), T("莎拉升到 5 级！\n最大 HP +1，防御力 +1。", "Sarah reaches level 5!\nMaximum HP +1. Defense +1."),
            T("敌军已被击退。队伍返回城镇。", "The enemy is defeated. The party returns to town.")],
        1 => [T("鲍伊使用撤退。", "Bowie casts Egress."), T("队伍脱离战场，回到城镇。", "The party leaves the battlefield and returns to town.")],
        _ => [T("鲍伊倒下了。", "Bowie has fallen."), T("鲍伊在教堂醒来。", "Bowie wakes up at the church.")] };

    private void DrawOutcome()
    {
        if (_reading == 0)
        {
            SidePage(T("战后结果", "Battle outcomes"), 3);
            for (int i = 0; i < 3; i++) Item(CommandRow(i), OutcomeName(i));
            Footer(T("↑ ↓ 选择结果     Enter / A  观看", "↑ ↓ Select outcome     Enter / A  Read"));
            return;
        }
        var lines = OutcomeLines();
        var reader = _dialogue = new StoryDialoguePanel { Name = "OutcomeReader" };
        _surface.AddChild(reader);
        reader.Body.Text = lines[_reading-1];
        reader.Choices.Present(false, false, false, false);
        string next = _reading < lines.Length ? T("继续", "Next") : _outcome == 2 ? T("前往教堂", "To church") : T("返回城镇", "To town");
        reader.Present(true, false, false, new(864, 480, 96, 96), T("Enter / A 或点击  ", "Enter / A or click   ") + next);
    }

    private void AdvanceOutcome()
    {
        if (_reading == 0) _outcome = _focus;
        if (_reading == OutcomeLines().Length) { ChooseScene(_outcome == 2 ? 2 : FieldScene); return; }
        _reading++; Render();
    }

    private bool ExtraDirection(InputEvent input)
    {
        if (_journey != JourneyPage.Closed || _showMembers) return false;
        if (_scene == FieldScene && _field == FieldPage.Walking && (input.IsActionPressed("ui_left") || input.IsActionPressed("ui_right")))
        { _facingSign = !_facingSign; Render(); return true; }
        if (_scene != InspectScene || _inspection != InspectPage.Cursor) return false;
        Vector2I delta = input.IsActionPressed("ui_left") ? Vector2I.Left : input.IsActionPressed("ui_right") ? Vector2I.Right :
            input.IsActionPressed("ui_up") ? Vector2I.Up : input.IsActionPressed("ui_down") ? Vector2I.Down : Vector2I.Zero;
        if (delta == Vector2I.Zero) return false;
        _cursor = (_cursor + delta).Clamp(Vector2I.Zero, _stage.Camera.MapSize - Vector2I.One);
        _stage.Camera.Reveal(_cursor);
        Render(); return true;
    }

    private bool BoardPointerInput(InputEventMouse mouse)
    {
        bool targeting = _scene == 3 && _page == Page.Target;
        bool inspecting = _scene == InspectScene && _inspection is InspectPage.Cursor or InspectPage.Map;
        if (_showMembers || _journey != JourneyPage.Closed || !(targeting || inspecting))
        {
            if (mouse is InputEventMouseButton { Pressed: false }) _boardPointer.ReleaseIfNeutral();
            return false;
        }
        bool overview = inspecting && _inspection == InspectPage.Map;
        var origin = _stage.BoardCell(Vector2I.Zero, overview);
        Vector2 local = GetGlobalTransformWithCanvas().AffineInverse() * mouse.Position;
        var cell = (Vector2I)((local - origin.Position) / origin.Size).Floor();
        var rect = GetGlobalTransformWithCanvas() * _stage.BoardCell(cell, overview);
        int unit = Array.IndexOf(UnitCells, cell);
        bool allowed = targeting ? unit >= 0 && (_targetParent == Page.Root ? unit == 2 : unit < 2) :
            overview ? unit >= 0 : PreviewInspectionCamera.View.HasPoint(local) && new Rect2I(Vector2I.Zero, _stage.Camera.MapSize).HasPoint(cell);
        // The inspection card moves away from the hovered cell. Other HUD regions own their input.
        bool overHud = _surface.GetChildren().OfType<Control>().Any(control => control.Visible &&
            control.Name != "InspectionCard" && control.GetRect().HasPoint(local));
        if (overHud) allowed = false;

        if (mouse is InputEventMouseMotion motion)
        {
            if (FreeInspection && motion.Relative != Vector2.Zero)
            {
                // The tabs and footer occupy the top/bottom chrome. The outer eight pixels
                // are clear of their buttons and can also be reached by pushing to the window edge.
                bool outerRim = local.Y < 8 || local.Y >= Size.Y - 8;
                bool panSurface = new Rect2(Vector2.Zero, Size).HasPoint(local) &&
                    (outerRim || PreviewInspectionCamera.View.HasPoint(local) && !overHud);
                TrackEdge(local, panSurface && motion.ButtonMask == 0);
            }
            if (!allowed) _boardPointer.Cancel();
            _boardPointer.Moved(motion);
            if (motion.Relative == Vector2.Zero || !allowed) return false;
        }
        else if (mouse is not InputEventMouseButton { ButtonIndex: MouseButton.Left }) return false;

        if (allowed && (mouse is InputEventMouseMotion || mouse is InputEventMouseButton { Pressed: true }))
        {
            if (targeting && _target != unit) { _target = unit; Render(); }
            else if (overview && _focus != unit) { _focus = unit; Refresh(); }
            else if (inspecting && !overview && _cursor != cell) { _cursor = cell; Render(); }
        }
        if (mouse is InputEventMouseButton button &&
            _boardPointer.Read(button, allowed ? cell.ToString() : null, rect, _surface) is not null)
            Activate();
        return allowed;
    }

    private void StopEdgePan()
    {
        _edgeSpeed = Vector2.Zero;
        _edgeDelay = 0;
    }

    private void TrackEdge(Vector2 position, bool allowed)
    {
        var view = PreviewInspectionCamera.View;
        if (!allowed || _playSuspended) { StopEdgePan(); return; }
        static float Axis(float p, float lo, float hi) =>
            Mathf.Clamp((p - hi + 32) / 32, 0, 1) - Mathf.Clamp((lo + 32 - p) / 32, 0, 1);
        var strength = new Vector2(Axis(position.X, view.Position.X, view.End.X), Axis(position.Y, view.Position.Y, view.End.Y));
        if (_stage.Camera.MapSize.X * 96 <= view.Size.X) strength.X = 0;
        if (_stage.Camera.MapSize.Y * 96 <= view.Size.Y) strength.Y = 0;
        if (strength == Vector2.Zero) { StopEdgePan(); return; }
        if (strength.Sign() != _edgeSpeed.Sign()) _edgeDelay = 0.18;
        _edgeMouse = position.Clamp(view.Position, view.End - Vector2.One);
        _edgeSpeed = strength.Normalized() * Mathf.Lerp(192, 576, Mathf.Max(Mathf.Abs(strength.X), Mathf.Abs(strength.Y)));
    }

    private void AdvanceEdgePan(double delta)
    {
        if (_edgeSpeed == Vector2.Zero) return;
        if (!FreeInspection || _playSuspended) { StopEdgePan(); return; }
        double travel = Math.Max(0, delta - _edgeDelay);
        _edgeDelay = Math.Max(0, _edgeDelay - delta);
        if (!_stage.Camera.Pan(_edgeSpeed, travel)) return;
        var cell = (Vector2I)((_edgeMouse - _stage.Camera.Origin) / 96).Floor();
        if (cell != _cursor && new Rect2I(Vector2I.Zero, _stage.Camera.MapSize).HasPoint(cell))
        {
            _cursor = cell;
            Render();
        }
        else _stage.QueueRedraw();
    }
}
