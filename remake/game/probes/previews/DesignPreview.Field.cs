using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    internal const int FieldScene = 6, InspectScene = 7, OutcomeScene = 8;
    private enum FieldPage { Commands, Walking, Caster, Spells, Search, Owner, Operations, Pack, Drop, Discarded }
    private FieldPage _field;
    private Member _caster;
    private Member _fieldOwner;
    private int _operation, _packSlot;
    private int[] _dropQuote = [];
    private static readonly string[] FieldOperations = ["use", "give", "equip", "drop"];
    private bool _facingSign = true;

    private void FieldAt(FieldPage page, int focus = 0)
    {
        _field = page; _focus = focus; Render();
    }

    private void ReviewFooter(string hint, Action back)
    {
        Footer(hint);
        Utility(new(1642, 1003, 204, 48), T("返回", "Back"), back);
    }

    private void DrawField()
    {
        if (_field >= FieldPage.Owner) { DrawFieldPack(); return; }
        if (_field == FieldPage.Search)
        {
            var reader = _dialogue = new StoryDialoguePanel { Name = "SearchResult" };
            _surface.AddChild(reader);
            reader.Body.Text = T("告示牌上写着：\n「通往古代之塔的道路在城镇以北。」", "The sign reads:\n“The road to the Ancient Tower lies north of town.”");
            reader.Choices.Present(false, false, false, false);
            reader.Present(true, false, false, new(864, 480, 96, 96), T("Enter / A 或点击读完", "Enter / A or click to finish reading"));
            return;
        }
        if (_field == FieldPage.Walking)
        {
            Utility(new(1592, 112, 248, 64), GameText.Get("field-menu.title"), () => FieldAt(FieldPage.Commands));
            Utility(new(32, 1003, 438, 48), Facing, () => { _facingSign = !_facingSign; Render(); });
            FooterHintRight(T("← → 转向     Enter / A  打开探索指令", "← → Turn     Enter / A  Field commands"));
            return;
        }
        string heading = _field == FieldPage.Commands ? GameText.Get("field-menu.title") :
            _field == FieldPage.Caster ? T("魔法 / 选择队员", "Magic / Member") : T("魔法 / ", "Magic / ") + MemberUnit(_caster).Name;
        var panel = SidePage(heading, _field == FieldPage.Spells ? 2 : _field == FieldPage.Caster ? 3 : 4);
        if (_field == FieldPage.Commands)
        {
            foreach (string id in new[] { "member", "magic", "item", "search" })
                Item(CommandRow(_choices.Count), GameText.Get("field-menu." + id), "field." + id,
                    id != "search" || _facingSign, id == "search" && !_facingSign ? T("当前面前没有可调查的物件。", "There is nothing to search here.") : "");
        }
        else if (_field == FieldPage.Caster)
        {
            foreach (Member m in Enum.GetValues<Member>())
                Item(CommandRow(_choices.Count), MemberUnit(m).Name, enabled: m == Member.Sarah,
                    reason: m == Member.Sarah ? "" : T("这位队员尚未习得魔法。", "This member has no learned magic."));
        }
        else
        {
            for (int i = 0; i < 2; i++)
                Item(CommandRow(i), T($"回复   Lv. {i + 1}", $"HEAL   Lv. {i + 1}"), "identity-icon.spell.0", false,
                    T("此处无法使用回复魔法。", "HEAL cannot be used here."), i == 0 ? "3 MP" : "5 MP");
        }
        FieldReason(panel);
        ReviewFooter(T("↑ ↓ 选择     Enter / A  确认", "↑ ↓ Select     Enter / A  Confirm"), BackField);
    }

    private void FooterHintRight(string hint)
        => Text(_surface, new(628, 999, 1212, 56), hint, 26).HorizontalAlignment = HorizontalAlignment.Right;
    private string Facing => _facingSign ? T("面向告示牌", "Facing the sign") : T("面向空地", "Facing open ground");

    private void FieldReason(Panel frame)
        => _detail = Text(frame, new(16, frame.Size.Y, 528, 0), "", 26);

    private void AdvanceField()
    {
        switch (_field)
        {
            case FieldPage.Walking: FieldAt(FieldPage.Commands); break;
            case FieldPage.Commands:
                switch (_focus)
                {
                    case 0: ToggleMembers(); break;
                    case 1: FieldAt(FieldPage.Caster, (int)Member.Sarah); break;
                    case 2: FieldAt(FieldPage.Owner); break;
                    case 3: FieldAt(FieldPage.Search); break;
                }
                break;
            case FieldPage.Caster: _caster = (Member)_focus; FieldAt(FieldPage.Spells); break;
            case FieldPage.Search: BackField(); break;
            case FieldPage.Owner: _fieldOwner = (Member)_focus; FieldAt(FieldPage.Operations); break;
            case FieldPage.Operations:
                _operation = _focus;
                if (_operation == 1) { _showMembers = true; _member = _fieldOwner; OpenGive(); }
                else FieldAt(FieldPage.Pack);
                break;
            case FieldPage.Pack:
                _packSlot = _focus;
                _dropQuote = _packs[_fieldOwner].ToArray();
                FieldAt(FieldPage.Drop, 1);
                break;
            case FieldPage.Drop:
                if (_focus == 1 || !_dropQuote.SequenceEqual(_packs[_fieldOwner])) { FieldAt(FieldPage.Pack, _packSlot); break; }
                _packs[_fieldOwner].RemoveAt(_packSlot);
                FieldAt(FieldPage.Discarded); break;
            case FieldPage.Discarded: FieldAt(FieldPage.Pack, _packSlot); break;
        }
    }

    private void BackField()
    {
        switch (_field)
        {
            case FieldPage.Spells: FieldAt(FieldPage.Caster, (int)_caster); break;
            case FieldPage.Caster: FieldAt(FieldPage.Commands, 1); break;
            case FieldPage.Owner: FieldAt(FieldPage.Commands, 2); break;
            case FieldPage.Operations: FieldAt(FieldPage.Owner, (int)_fieldOwner); break;
            case FieldPage.Pack: FieldAt(FieldPage.Operations, _operation); break;
            case FieldPage.Drop:
            case FieldPage.Discarded: FieldAt(FieldPage.Pack, _packSlot); break;
            case FieldPage.Walking: FieldAt(FieldPage.Commands); break;
            default: FieldAt(FieldPage.Walking); break;
        }
    }

    private void DrawFieldPack()
    {
        string owner = MemberUnit(_fieldOwner).Name;
        string title = _field == FieldPage.Owner ? T("道具 / 选择队员", "Items / Member") :
            owner + " / " + (_field == FieldPage.Operations ? GameText.Get("field-menu.item") : GameText.Get("prepare." + FieldOperations[_operation]));
        var frame = SidePage(title, _field switch { FieldPage.Owner or FieldPage.Drop => 3, FieldPage.Discarded => 2, _ => 4 });
        if (_field == FieldPage.Owner)
        {
            foreach (Member m in Enum.GetValues<Member>())
                Item(CommandRow(_choices.Count), MemberUnit(m).Name, value: $"{_packs[m].Count} / {PackCapacity}");
        }
        else if (_field == FieldPage.Operations)
        {
            foreach (string op in FieldOperations) Item(CommandRow(_choices.Count), GameText.Get("prepare." + op), "op." + op);
        }
        else if (_field == FieldPage.Pack)
        {
            for (int i = 0; i < PackCapacity; i++)
            {
                int id = SlotId(_packs[_fieldOwner], i);
                string reason = id < 0 ? T("空槽位", "Empty slot") : _operation == 0 ? T("此处没有使用效果。", "This item has no field effect here.") :
                    IsWeapon(id) ? GameText.Get("party.equipped") : _operation == 2 ? T("此道具无法装备。", "This item cannot be equipped.") : "";
                Item(CommandRow(i), SlotLabel(id, i), ItemArt(id), reason.Length == 0, reason,
                    IsWeapon(id) ? GameText.Get("party.equipped") : "", slot: true);
            }
            FieldReason(frame);
        }
        else
        {
            string what = $"{owner} · {SlotLabel(_dropQuote[_packSlot], _packSlot)}";
            if (_field == FieldPage.Drop)
                ReadingActions(frame, T($"丢弃 {what}？", $"Discard {what}?"), T("丢弃", "Discard"), T("取消", "Cancel"));
            else ReadingActions(frame, T($"已丢弃 {what}。", $"Discarded {what}."), T("返回道具栏", "Return to pack"));
        }
        ReviewFooter(T("↑ ↓ 选择     Enter / A  确认", "↑ ↓ Select     Enter / A  Confirm"), BackField);
    }
}
