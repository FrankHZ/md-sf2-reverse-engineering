using Godot;
using Sf2.Remake.GodotAdapter.Entry;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private enum JourneyPage { Closed, Menu, Settings, Slots, Name, Confirm, Receipt, Quit, WindowHelp }
    private enum SlotTask { New, Continue, Save, SaveAndQuit }
    private sealed record SampleSlot(string Name, int Scene, bool BackupOnly = false);
    private readonly SampleSlot?[] _slots = [new("Bowie", FieldScene, true), null];
    private JourneyPage _journey;
    private JourneyPage _coveredHostPage;
    private SlotTask _slotTask;
    private bool _fromMenu, _draftEnglish, _draftSteady;
    private int _priorFocus, _coveredHostFocus, _slot;
    private string _draftName = "", _partyName = "Bowie";
    private NameEntryView? _editor;

    private void Journey(JourneyPage next, int focus = 0)
    {
        _journey = next;
        _focus = focus;
        Render();
    }

    private void OpenHost()
    {
        if (_journey == JourneyPage.Menu) { CloseJourney(); return; }
        if (_journey != JourneyPage.Closed || _showMembers) return;
        _priorFocus = _focus;
        _fromMenu = true;
        Journey(JourneyPage.Menu);
    }

    private void OpenSlots(SlotTask task)
    {
        if (_journey == JourneyPage.Closed)
        {
            _priorFocus = _focus;
            _fromMenu = false;
        }
        _slotTask = task;
        _draftName = "";
        _slot = 0;
        Journey(JourneyPage.Slots);
    }

    private void CloseJourney() => Journey(JourneyPage.Closed, _priorFocus);
    private string SlotTitle => _slotTask switch
    {
        SlotTask.New => GameText.Get("entry.new"), SlotTask.Continue => GameText.Get("entry.continue"),
        SlotTask.SaveAndQuit => T("记录并退出", "Save and quit"),
        _ => T("记录冒险", "Save adventure")
    };
    private string Destination(int scene) => scene == InspectScene ? T("古代之塔 · 战场", "Ancient Tower · battlefield")
        : T("格兰西尔 · 城镇", "Granseal · town");
    private string SlotName(int index) => T($"槽位 {index + 1}", $"Slot {index + 1}");
    private string Occupant(int index) => _slots[index] is { } saved ? $"{saved.Name}   ·   {Destination(saved.Scene)}"
        : T("空槽位", "Empty slot");
    private bool EntrySlots => _scene == 0 && !_fromMenu;
    private bool CanSaveSample => _scene == FieldScene && _field is FieldPage.Commands or FieldPage.Walking
        || _scene == InspectScene && _inspection is InspectPage.Cursor or InspectPage.Options || _scene == 2 && _page == Page.Root;

    private void DrawJourney()
    {
        if (_journey == JourneyPage.Name)
        {
            _editor = new NameEntryView { Name = "PreviewName", Draft = _draftName,
                Done = name => { _draftName = name; Journey(JourneyPage.Confirm, 1); },
                Back = name => { _draftName = name; Journey(JourneyPage.Slots, _slot); } };
            _surface.AddChild(_editor);
            // Keep typed input across preview language changes and redraws.
            var edit = _editor.FindChild("NameInput", true, false) as LineEdit;
            if (edit is not null) edit.TextChanged += value => _draftName = value;
            return;
        }
        if (!EntrySlots)
            Put(_surface, new ColorRect { Color = new(0, 0, 0, 0.48f), MouseFilter = MouseFilterEnum.Ignore }, new(0, 72, 1920, 1008));
        bool slots = _journey is JourneyPage.Slots or JourneyPage.Confirm or JourneyPage.Receipt;
        float x = EntrySlots ? 1216 : 560, width = EntrySlots ? 640 : 800;
        string title = slots ? SlotTitle : _journey switch {
            JourneyPage.Settings => T("设置", "Settings"), JourneyPage.Quit => T("退出冒险", "Quit adventure"),
            JourneyPage.WindowHelp => T("窗口恢复", "Window recovery"), _ => T("系统菜单", "System menu") };
        string body = "";
        switch (_journey)
        {
            case JourneyPage.Menu: body = _scene is 3 or InspectScene or OutcomeScene ? Destination(InspectScene) :
                _scene == 0 ? T("女巫入口", "Witch entrance") : Destination(FieldScene); break;
            case JourneyPage.Settings: body = T("修改后选择「应用」生效。", "Choose Apply to use these preferences."); break;
            case JourneyPage.Slots: body = T("选择预览槽位", "Choose a preview slot"); break;
            case JourneyPage.Confirm:
                body = _slotTask switch {
                    SlotTask.New => T($"以 {EffectiveName} 开始新冒险。", $"Begin a new adventure as {EffectiveName}."),
                    SlotTask.Save or SlotTask.SaveAndQuit => T($"记录 {_partyName} 在{Destination(_scene)}的进度。", $"Record {_partyName} at {Destination(_scene)}."),
                    _ => T("主记录损坏。要从备份继续吗？", "The primary record is damaged. Continue from its backup?") };
                body += $"\n{SlotName(_slot)}   ·   {Occupant(_slot)}";
                if (_slotTask != SlotTask.Continue && _slots[_slot] != null)
                    body += T("\n确认将替换此槽位的记录。", "\nConfirm to replace the record in this slot.");
                break;
            case JourneyPage.Receipt: body = $"{SlotName(_slot)}   ·   {Occupant(_slot)}"; break;
            case JourneyPage.Quit: body = T("结束本次场景并返回女巫入口？预览槽位将保留。", "Leave this scene for the Witch entrance? Preview slots will be retained."); break;
            case JourneyPage.WindowHelp: body = T("窗口过小。恢复后可继续刚才的操作。", "The window is too small. Restore to continue where you left off."); break;
        }
        int rows = _journey switch { JourneyPage.Menu => 5, JourneyPage.Settings => 4,
            JourneyPage.Slots => 3, JourneyPage.Quit when CanSaveSample => 3, JourneyPage.Confirm or JourneyPage.Quit => 2, _ => 1 };
        int rowHeight = _journey == JourneyPage.Slots ? 104 : 72;
        int bodyHeight = _journey == JourneyPage.Confirm ? 180 : 88;
        int rowStart = 86 + bodyHeight;
        var frame = Panel(new(x, 170, width, rowStart + rows * (rowHeight + 8) + 12));
        Text(frame, new(24, 12, width - 48, 58), title, 36);
        Text(frame, new(24, 76, width - 48, bodyHeight), body, 27, "cad6e5");
        void Row(string name, bool allowed = true)
        {
            Item(new(x + 20, 170 + rowStart + _choices.Count * (rowHeight + 8), width - 40, rowHeight), name, enabled: allowed);
        }
        switch (_journey)
        {
            case JourneyPage.Menu:
                Row(T("继续", "Resume")); Row(T("设置", "Settings"));
                Row(CanSaveSample ? T("记录冒险", "Save adventure") : T("记录冒险 · 当前阶段不可用", "Save · unavailable in this phase"), CanSaveSample);
                Row(GameText.Get("entry.new")); Row(T("退出冒险", "Quit adventure")); break;
            case JourneyPage.Settings:
                Row(T("界面语言", "Language") + "     " + (_draftEnglish ? "English" : "中文"));
                Row(T("图标动效", "Icon animation") + "     " + (_draftSteady ? T("关", "Off") : T("开", "On")));
                Row(T("应用", "Apply")); Row(T("返回", "Back")); break;
            case JourneyPage.Slots:
                for (int i = 0; i < _slots.Length; i++)
                    Row($"{SlotName(i)}   ·   {Occupant(i)}" + (_slots[i]?.BackupOnly == true ? T("\n主记录损坏 · 有备份", "\nDamaged record · backup available") : ""),
                        _slotTask != SlotTask.Continue || _slots[i] != null);
                Row(T("返回", "Back")); break;
            case JourneyPage.Confirm: Row(T("确认", "Confirm")); Row(T("取消", "Cancel")); break;
            case JourneyPage.Quit:
                if (CanSaveSample) Row(T("记录并退出", "Save and quit"));
                Row(T("直接退出", "Quit without saving")); Row(T("取消", "Cancel")); break;
            case JourneyPage.WindowHelp: Row(T("恢复窗口", "Restore window")); break;
            case JourneyPage.Receipt: Row(T("完成", "Done")); break;
        }
        if (_fromMenu && _journey is JourneyPage.Menu or JourneyPage.Settings or JourneyPage.Slots)
            Utility(new(1492, 1003, 356, 48), T("预览窗口恢复", "Preview window recovery"), () => {
                _coveredHostPage = _journey; _coveredHostFocus = _focus; Journey(JourneyPage.WindowHelp);
            });
    }

    private string EffectiveName => _draftName.Length == 0 ? "Bowie" : _draftName;
    private void AdvanceJourney()
    {
        switch (_journey)
        {
            case JourneyPage.Menu:
                switch (_focus)
                {
                    case 0: CloseJourney(); break;
                    case 1: _draftEnglish = _english; _draftSteady = _steadyIcons; Journey(JourneyPage.Settings); break;
                    case 2: OpenSlots(SlotTask.Save); break;
                    case 3: OpenSlots(SlotTask.New); break;
                    case 4: Journey(JourneyPage.Quit, CanSaveSample ? 2 : 1); break;
                }
                break;
            case JourneyPage.Settings:
                if (_focus == 0) { _draftEnglish = !_draftEnglish; Render(); }
                else if (_focus == 1) { _draftSteady = !_draftSteady; Render(); }
                else {
                    if (_focus == 2) { _steadyIcons = _draftSteady; _english = _draftEnglish; GameText.Initialize(_english ? "en" : "zh-CN"); }
                    Journey(JourneyPage.Menu, 1);
                }
                break;
            case JourneyPage.Slots:
                if (_focus == 2) { BackJourney(); break; }
                _slot = _focus;
                if (_slotTask == SlotTask.New) Journey(JourneyPage.Name);
                else if (_slotTask == SlotTask.Continue && _slots[_slot] is { BackupOnly: false } ready)
                    { _partyName = ready.Name; ChooseScene(ready.Scene); }
                else Journey(JourneyPage.Confirm, 1);
                break;
            case JourneyPage.Confirm:
                if (_focus == 1) { BackJourney(); break; }
                if (_slotTask == SlotTask.Continue && _slots[_slot] is { } backup)
                    { _partyName = backup.Name; ChooseScene(backup.Scene); break; }
                _slots[_slot] = new(_slotTask == SlotTask.New ? EffectiveName : _partyName, _scene == InspectScene ? InspectScene : FieldScene);
                if (_slotTask == SlotTask.New) { _partyName = EffectiveName; ChooseScene(FieldScene); }
                else Journey(JourneyPage.Receipt);
                break;
            case JourneyPage.Quit:
                if (CanSaveSample && _focus == 0) OpenSlots(SlotTask.SaveAndQuit);
                else if (_focus == (CanSaveSample ? 1 : 0)) ChooseScene(0);
                else BackJourney();
                break;
            default: BackJourney(); break;
        }
    }

    private void BackJourney()
    {
        switch (_journey)
        {
            case JourneyPage.Settings: Journey(JourneyPage.Menu, 1); break;
            case JourneyPage.WindowHelp: Journey(_coveredHostPage, _coveredHostFocus); break;
            case JourneyPage.Quit: if (_fromMenu) Journey(JourneyPage.Menu, 4); else CloseJourney(); break;
            case JourneyPage.Name: Journey(JourneyPage.Slots, _slot); break;
            case JourneyPage.Confirm:
                if (_slotTask == SlotTask.New) Journey(JourneyPage.Name); else Journey(JourneyPage.Slots, _slot);
                break;
            case JourneyPage.Slots:
                if (_slotTask == SlotTask.SaveAndQuit) { Journey(JourneyPage.Quit, 0); break; }
                goto case JourneyPage.Receipt;
            case JourneyPage.Receipt:
                if (_slotTask == SlotTask.SaveAndQuit) { ChooseScene(0); break; }
                if (_fromMenu) Journey(JourneyPage.Menu, _slotTask == SlotTask.New ? 3 : 2); else CloseJourney();
                break;
            default: CloseJourney(); break;
        }
    }
}
