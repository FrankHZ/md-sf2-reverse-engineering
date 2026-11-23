using Godot;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private int _outcome, _reading;

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
}
