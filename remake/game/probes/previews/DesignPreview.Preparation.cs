using Godot;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    private enum GiveStep { Closed, Item, Recipient, Exchange, Confirm, Result }
    private const int PackCapacity = 4;
    // Each review inventory is compact and ordered by slot. Weapons stay equipped in this supplies pilot.
    private readonly List<int> _bowiePack = [71, 0, 1];
    private readonly List<int> _sarahPack = [85, 0, 1];
    private GiveStep _giveStep;
    private int _giveFocus, _givenSlot, _exchangeSlot;
    private int _givenId, _returnedId;
    private bool Preparing => _showMembers && _giveStep != GiveStep.Closed;
    private int ChoiceFocus
    {
        get => Preparing ? _giveFocus : _focus;
        set { if (Preparing) _giveFocus = value; else _focus = value; }
    }
    private List<int> DonorPack => _sarahSelected ? _sarahPack : _bowiePack;
    private List<int> RecipientPack => _sarahSelected ? _bowiePack : _sarahPack;
    private string Donor => (_sarahSelected ? Sarah : Bowie).Name;
    private string Recipient => (_sarahSelected ? Bowie : Sarah).Name;

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
            case GiveStep.Exchange: GivePage(GiveStep.Recipient); break;
            case GiveStep.Confirm:
                if (_exchangeSlot >= 0) GivePage(GiveStep.Exchange, _exchangeSlot);
                else GivePage(GiveStep.Recipient);
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
                GivePage(GiveStep.Recipient);
                break;
            case GiveStep.Recipient:
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
                _sarahSelected = !_sarahSelected;
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
        GivePack(new(976, 244), Recipient, RecipientPack, false);

        var tray = Panel(new(224, 698, 1472, 180));
        string summary = _giveStep switch
        {
            GiveStep.Item => T("选择要交给同伴的消耗品。", "Choose a supply to give to the other member."),
            GiveStep.Recipient or GiveStep.Exchange => $"{Donor}  →  {Recipient}     {SlotLabel(_givenId, _givenSlot)}",
            _ => TransferSummary(_giveStep == GiveStep.Result)
        };
        Text(tray, new(24, 14, 822, 136), summary, 30);
        switch (_giveStep)
        {
            case GiveStep.Recipient:
                Item(new(1094, 750, 578, 78), T($"交给{Recipient}", $"Give to {Recipient}"));
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

    private void GivePack(Vector2 origin, string owner, List<int> pack, bool donor)
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
            float left = 20;
            if (ItemArt(id) is { } key && _stage.Icon(key, marked) is { } icon)
            {
                Put(panel, new TextureRect { Texture = icon, TextureFilter = TextureFilterEnum.Nearest,
                    ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                    MouseFilter = MouseFilterEnum.Ignore }, new(left, row.Position.Y + 4, 48, 72));
                left += 64;
            }
            Text(panel, new(left, row.Position.Y, 500, 80), (marked ? "›  " : "") + SlotLabel(id, i),
                30, marked ? "ffe19a" : id < 0 ? "8e9bab" : "f4f5fc");
            if (note.Length > 0) Text(panel, new(556, row.Position.Y, 136, 80), note, 24, "cad6e5")
                .HorizontalAlignment = HorizontalAlignment.Right;
        }
    }
}
