using Godot;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private enum GiveStep { Closed, Item, Recipient, Exchange, Confirm, Result }
    private const int PackCapacity = 4;
    // Each review inventory is compact and ordered by slot. Weapons stay equipped in this supplies pilot.
    private readonly Dictionary<Member, List<int>> _packs = new()
    {
        [Member.Bowie] = [71, 0, 1], [Member.Sarah] = [85, 0, 1], [Member.Chester] = [0, 1, 0, 1]
    };
    private Member[] _candidates = [];
    private int _candidateIndex;
    private static readonly Vector2 ReceiverOrigin = new(976, 244);
    private Panel _receiverPanel = null!;
    private GiveStep _giveStep;
    private int _giveFocus, _givenSlot, _exchangeSlot;
    private int _givenId, _returnedId;
    private bool Preparing => _showMembers && _giveStep != GiveStep.Closed;
    private int ChoiceFocus
    {
        get => Preparing ? _giveFocus : _focus;
        set { if (Preparing) _giveFocus = value; else _focus = value; }
    }
    private Member Candidate => _candidates[_candidateIndex];
    private List<int> DonorPack => _packs[_member];
    private List<int> RecipientPack => _packs[Candidate];
    private string Donor => MemberUnit(_member).Name;
    private string Recipient => MemberUnit(Candidate).Name;

    private static int SlotId(List<int> pack, int slot) => slot < pack.Count ? pack[slot] : -1;
    private static bool IsWeapon(int id) => id is 71 or 85;
    private static string? ItemArt(int id) => id < 0 ? null : $"identity-icon.item.{id}";
    private string ItemName(int id) => id switch
    {
        0 => T("药草", "Medical Herb"), 1 => T("回复种子", "Healing Seed"),
        71 => T("木剑", "Wooden Sword"), 85 => T("木杖", "Wooden Rod"),
        _ => GameText.Get("party.empty")
    };
    private string SlotLabel(int id, int slot) => $"{slot + 1}. {ItemName(id)}";

    private void OpenGive()
    {
        _candidates = Enum.GetValues<Member>().Where(member => member != _member).ToArray();
        _candidateIndex = 0;
        int supply = DonorPack.FindIndex(id => !IsWeapon(id));
        GivePage(GiveStep.Item, Math.Max(0, supply));
    }

    private void GivePage(GiveStep step, int focus = 0)
    {
        _giveStep = step;
        _giveFocus = focus;
        Render();
    }

    private void BackGive()
    {
        switch (_giveStep)
        {
            case GiveStep.Recipient: GivePage(GiveStep.Item, _givenSlot); break;
            case GiveStep.Exchange: GivePage(GiveStep.Recipient, _candidateIndex); break;
            case GiveStep.Confirm:
                if (_exchangeSlot >= 0) GivePage(GiveStep.Exchange, _exchangeSlot);
                else GivePage(GiveStep.Recipient, _candidateIndex);
                break;
            default: GivePage(GiveStep.Closed); break;
        }
    }

    private void AdvanceGive()
    {
        switch (_giveStep)
        {
            case GiveStep.Item:
                _givenSlot = _giveFocus;
                _givenId = DonorPack[_givenSlot];
                _exchangeSlot = -1;
                GivePage(GiveStep.Recipient, _candidateIndex);
                break;
            case GiveStep.Recipient:
                _exchangeSlot = -1;
                if (RecipientPack.Count == PackCapacity)
                    GivePage(GiveStep.Exchange, RecipientPack.FindIndex(id => !IsWeapon(id)));
                else GivePage(GiveStep.Confirm, 1);
                break;
            case GiveStep.Exchange:
                _exchangeSlot = _giveFocus;
                _returnedId = RecipientPack[_exchangeSlot];
                GivePage(GiveStep.Confirm, 1);
                break;
            case GiveStep.Confirm:
                if (_giveFocus != 0) { BackGive(); return; }
                DonorPack.RemoveAt(_givenSlot);
                if (_exchangeSlot >= 0)
                {
                    RecipientPack.RemoveAt(_exchangeSlot);
                    DonorPack.Add(_returnedId);
                }
                RecipientPack.Add(_givenId);
                GivePage(GiveStep.Result);
                break;
            case GiveStep.Result:
                _member = Candidate;
                GivePage(GiveStep.Closed);
                break;
        }
    }

    private string TransferSummary(bool after)
    {
        int destination = after || _exchangeSlot >= 0 ? RecipientPack.Count : RecipientPack.Count + 1;
        string given = $"{Donor} #{_givenSlot + 1}  →  {Recipient} #{destination}     {ItemName(_givenId)}";
        if (_exchangeSlot < 0) return given;
        return given + $"\n{Recipient} #{_exchangeSlot + 1}  →  {Donor} #{DonorPack.Count}     {ItemName(_returnedId)}";
    }

    private void PrepareGive()
    {
        Put(_surface, new ColorRect { Color = new Color(0, 0, 0, 0.50f), MouseFilter = MouseFilterEnum.Ignore },
            new(0, 72, 1920, 1008));
        string title = _giveStep switch
        {
            GiveStep.Item => T("整备 / 选择交付物品", "Prepare / Choose an item to give"),
            GiveStep.Recipient => T("整备 / 选择接收人", "Prepare / Choose a recipient"),
            GiveStep.Exchange => T("背包已满 / 选择换回物品", "Full pack / Choose an item in return"),
            GiveStep.Confirm => _exchangeSlot >= 0 ? T("确认交换", "Confirm exchange") : T("确认交付", "Confirm transfer"),
            _ => _exchangeSlot >= 0 ? T("交换完成", "Exchange complete") : T("交付完成", "Transfer complete")
        };
        var heading = Panel(new(224, 148, 1472, 76));
        Text(heading, new(24, 8, 1424, 60), title, 32);
        GivePack(new(224, 244), Donor, DonorPack, true);
        _receiverPanel = GivePack(ReceiverOrigin, Recipient, RecipientPack, false);

        var tray = Panel(new(224, 698, 1472, 180));
        string summary = _giveStep switch
        {
            GiveStep.Item => T("选择要交给同伴的消耗品。", "Choose a supply to give to the other member."),
            GiveStep.Recipient => $"{Donor}    {SlotLabel(_givenId, _givenSlot)}",
            GiveStep.Exchange => $"{Donor}  →  {Recipient}     {SlotLabel(_givenId, _givenSlot)}",
            _ => TransferSummary(_giveStep == GiveStep.Result)
        };
        Text(tray, new(24, 14, 822, 136), summary, 30);
        switch (_giveStep)
        {
            case GiveStep.Recipient:
                for (int i = 0; i < _candidates.Length; i++)
                {
                    Member member = _candidates[i];
                    Item(new(1100, 715 + 75 * i, 572, 68), MemberUnit(member).Name,
                        value: $"{_packs[member].Count}/{PackCapacity}");
                }
                break;
            case GiveStep.Confirm:
                Item(new(1100, 715, 572, 68), T("确认", "Confirm"));
                Item(new(1100, 790, 572, 68), T("取消", "Cancel"));
                break;
            case GiveStep.Result:
                Item(new(1094, 750, 578, 78), T($"查看{Recipient}的背包", $"View {Recipient}'s pack"));
                break;
            default:
                _detail = Text(tray, new(858, 16, 588, 140), "", 26);
                break;
        }
        Footer(T("↑ ↓ 选择     Enter / A 确认     Esc / B 返回", "↑ ↓ Select     Enter / A Confirm     Esc / B Back"));
        Utility(new(1642, 1003, 204, 48), T("返回", "Back"), BackGive);
    }

    private Panel GivePack(Vector2 origin, string owner, List<int> pack, bool donor)
    {
        var panel = Panel(new(origin, new(720, 434)));
        panel.Name = donor ? "GiverPack" : "ReceiverPack";
        Text(panel, new(20, 6, 436, 64), owner, 32);
        Text(panel, new(452, 8, 248, 60), (donor ? T("交出", "Giver") : T("接收", "Recipient")) +
            $"    {pack.Count}/{PackCapacity}", 24, "cad6e5").HorizontalAlignment = HorizontalAlignment.Right;
        bool selecting = donor ? _giveStep == GiveStep.Item : _giveStep == GiveStep.Exchange;
        for (int i = 0; i < PackCapacity; i++)
        {
            int id = SlotId(pack, i);
            string note = IsWeapon(id) ? GameText.Get("party.equipped") : "";
            Rect2 row = new(16, 76 + 86 * i, 688, 80);
            if (selecting)
            {
                bool enabled = id >= 0 && !IsWeapon(id);
                Item(new(origin + row.Position, row.Size), SlotLabel(id, i), ItemArt(id), enabled,
                    IsWeapon(id) ? T("此预览可交接消耗品。", "This preview transfers supplies.") :
                    id < 0 ? T("这个槽位是空的。", "This slot is empty.") : "", note);
                continue;
            }
            bool marked = donor ? _giveStep != GiveStep.Item && i == _givenSlot :
                _giveStep == GiveStep.Confirm && i == _exchangeSlot;
            if (_giveStep == GiveStep.Result)
                marked = (!donor || _exchangeSlot >= 0) && i == pack.Count - 1;
            var line = Put(panel, new Control { Name = $"Slot{i}", MouseFilter = MouseFilterEnum.Ignore }, row);
            var key = ItemArt(id);
            var (label, art) = ItemContent(line, SlotLabel(id, i), key, note, true);
            art.Texture = key is null ? null : _stage.Icon(key, marked);
            label.AddThemeColorOverride("font_color", new Color(marked ? "ffe19a" : id < 0 ? "8e9bab" : "f4f5fc"));
        }
        return panel;
    }
}
