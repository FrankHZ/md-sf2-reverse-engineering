using Godot;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private enum InspectPage { Cursor, Options, Map, Unit, Speed }
    private InspectPage _inspection;
    private Vector2I _cursor = new(9, 4);
    internal static readonly Vector2I[] UnitCells = [new(9,5), new(11,5), new(10,3)];
    private int _mapFocus, _rate, _draftRate, _outcome, _reading;
    private readonly MenuPointer _boardPointer = new();

    private string InspectName(int i) => i switch { 0 => Bowie.Name, 1 => Sarah.Name, _ => T("基兹莫", "Gizmo") };
    private string Rate(int i) => i switch { 1 => "1.5×", 2 => "2×", _ => "1×" };
    private void InspectAt(InspectPage page, int focus = 0)
    {
        _inspection = page; _focus = focus; Render();
    }

    private void DrawInspection()
    {
        int unit = Array.IndexOf(UnitCells, _cursor);
        if (_inspection is InspectPage.Cursor or InspectPage.Unit)
        {
            var card = Panel(new(_cursor.X >= 13 ? 48 : 1296, 238, 560, _inspection == InspectPage.Unit ? 324 : 186));
            card.Name = "InspectionCard";
            Text(card, new(16, 10, 520, 56), unit < 0 ? T("空地", "Open ground") : InspectName(unit), 36);
            Text(card, new(16, 76, 528, 74), T($"古代之塔   ·   地块 ({_cursor.X-2}, {_cursor.Y})",
                $"Ancient Tower   ·   Cell ({_cursor.X-2}, {_cursor.Y})"), 28);
            if (_inspection == InspectPage.Unit)
                Vitals(card, new(18, 180), unit == 0 ? Bowie : unit == 1 ? Sarah : new(InspectName(2), _enemyHp, 24, 0, 0));
            if (_inspection == InspectPage.Cursor)
            {
                Footer(T("方向键或鼠标选择地块     Enter / A 或左键调查", "Directions or mouse select a cell     Enter / A or click to inspect"));
                Utility(new(1530, 1003, 316, 48), unit < 0 ? T("战场选项", "Battlefield options") : T("查看单位", "Inspect unit"), AdvanceInspection);
            }
            else ReviewFooter(T("Enter / A  返回地块", "Enter / A  Return to cell"), BackInspection);
            return;
        }
        if (_inspection == InspectPage.Map)
        {
            var legend = SidePage(T("战场地图", "Battlefield map"), 3);
            for (int i = 0; i < UnitCells.Length; i++) Item(CommandRow(i), InspectName(i), value: i == 2 ? T("敌军", "Enemy") : T("我军", "Ally"));
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
            case InspectPage.Map: _mapFocus = _focus; _cursor = UnitCells[_mapFocus]; InspectAt(InspectPage.Cursor); break;
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
        _cursor = (_cursor + delta).Clamp(new Vector2I(2, 1), new Vector2I(17, 9));
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
        var origin = PreviewStage.BoardCell(Vector2I.Zero, overview);
        Vector2 local = GetGlobalTransformWithCanvas().AffineInverse() * mouse.Position;
        var cell = (Vector2I)((local - origin.Position) / origin.Size).Floor();
        var rect = GetGlobalTransformWithCanvas() * PreviewStage.BoardCell(cell, overview);
        int unit = Array.IndexOf(UnitCells, cell);
        bool allowed = targeting ? unit >= 0 && (_targetParent == Page.Root ? unit == 2 : unit < 2) :
            overview ? unit >= 0 : cell.X is >= 2 and <= 17 && cell.Y is >= 1 and <= 9;
        // The inspection card moves away from the hovered cell. Other HUD regions own their input.
        if (allowed && _surface.GetChildren().OfType<Control>().Any(control => control.Visible &&
                control.Name != "InspectionCard" && control.GetRect().HasPoint(local))) allowed = false;

        if (mouse is InputEventMouseMotion motion)
        {
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
}
