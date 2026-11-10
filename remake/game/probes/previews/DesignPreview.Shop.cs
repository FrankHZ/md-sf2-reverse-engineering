using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Previews;

public partial class DesignPreview
{
    internal const int ShopScene = 5; // F5 opens the member overlay; F6 is the shop.
    private enum ShopStep { Services, Stock, Member, Slot, Quote, Receipt, Closed }
    private static readonly int[] Stock = [0, 1, 3, 4];
    private ShopStep _shopStep;
    private bool _buying;
    private int _shopGold = 150, _stockIndex, _saleSlot, _tradeItem, _tradeSlot, _goldBefore;
    private Member _shopMember;
    private int[] _quotedPack = [];
    private Panel? _shopPackPanel;
    private Label? _saleSummary;
    private Member _drawnPack;

    // The four Galam supply IDs and the two sample starting weapons have fixed buy terms.
    private static int BasePrice(int id) => id switch
    {
        0 => 10, 1 => 200, 3 => 20, 4 => 40, 71 or 85 => 60,
        _ => throw new ArgumentOutOfRangeException(nameof(id))
    };
    private int TradePrice(int id) => _buying ? BasePrice(id) : BasePrice(id) * 3 / 4;
    private string GoldNow => T($"金币   {_shopGold}", $"Gold   {_shopGold}");
    private List<int> TradePack => _packs[_shopMember];
    private string TradeOwner => MemberUnit(_shopMember).Name;
    private string TradeAction => GameText.Get(_buying ? "shop.buy" : "shop.sell");
    private string FundsReason => T("金币不足。", "Not enough gold.");
    private string FullReason => T("四个物品槽都已占用。", "All four slots are occupied.");

    private void ShopPage(ShopStep step, int focus = 0)
    {
        _shopStep = step;
        _focus = focus;
        Render();
    }

    private void QuoteShop()
    {
        _goldBefore = _shopGold;
        _quotedPack = TradePack.ToArray();
        _tradeSlot = _buying ? TradePack.Count : _saleSlot;
        ShopPage(ShopStep.Quote, 1);
    }

    private string InvalidQuote()
    {
        if (_goldBefore != _shopGold || !_quotedPack.SequenceEqual(TradePack))
            return T("库存或余额已变动，请重新选择。", "Inventory or funds changed. Choose again.");
        if (_buying && TradePack.Count >= PackCapacity) return FullReason;
        if (_buying && _shopGold < TradePrice(_tradeItem)) return FundsReason;
        return "";
    }

    private void AdvanceShop()
    {
        switch (_shopStep)
        {
            case ShopStep.Services:
                if (_focus == 4) { ShopPage(ShopStep.Closed); break; }
                _buying = _focus == 0;
                _shopMember = Member.Bowie;
                _stockIndex = _saleSlot = 0;
                ShopPage(_buying ? ShopStep.Stock : ShopStep.Member);
                break;
            case ShopStep.Stock:
                _stockIndex = _focus;
                _tradeItem = Stock[_focus];
                ShopPage(ShopStep.Member, (int)_shopMember);
                break;
            case ShopStep.Member:
                _shopMember = (Member)_focus;
                if (_buying) QuoteShop();
                else ShopPage(ShopStep.Slot, _saleSlot);
                break;
            case ShopStep.Slot:
                _saleSlot = _focus;
                _tradeItem = TradePack[_saleSlot];
                QuoteShop();
                break;
            case ShopStep.Quote:
                if (_focus == 1) { BackShop(); break; }
                if (InvalidQuote().Length > 0) { Render(); break; }
                if (_buying) { TradePack.Add(_tradeItem); _shopGold -= TradePrice(_tradeItem); }
                else { TradePack.RemoveAt(_tradeSlot); _shopGold += TradePrice(_tradeItem); }
                ShopPage(ShopStep.Receipt);
                break;
            case ShopStep.Receipt: ShopPage(ShopStep.Services, _buying ? 0 : 1); break;
            case ShopStep.Closed: ShopPage(ShopStep.Services); break;
        }
    }

    private void BackShop()
    {
        switch (_shopStep)
        {
            case ShopStep.Services: ShopPage(ShopStep.Closed); break;
            case ShopStep.Member when _buying: ShopPage(ShopStep.Stock, _stockIndex); break;
            case ShopStep.Quote:
            case ShopStep.Slot: ShopPage(ShopStep.Member, (int)_shopMember); break;
            default: ShopPage(ShopStep.Services, _buying ? 0 : 1); break;
        }
    }

    private void Shop()
    {
        _shopPackPanel = null;
        _saleSummary = null;
        if (_shopStep != ShopStep.Closed)
            Put(_surface, new ColorRect { Color = new Color(0, 0, 0, 0.35f), MouseFilter = MouseFilterEnum.Ignore },
                new(0, 72, 1920, 1008));
        string shop = T("道具店", "Item shop");
        switch (_shopStep)
        {
            case ShopStep.Services:
                SidePage(shop, 5, GoldNow);
                string[] ids = ["buy", "sell", "repair", "deals"];
                for (int i = 0; i < ids.Length; i++)
                    Item(CommandRow(i), GameText.Get("shop." + ids[i]), "shop." + ids[i], i < 2,
                        i < 2 ? "" : GameText.Get("shop.unavailable"));
                Item(CommandRow(4), GameText.Get("shop.leave"));
                break;
            case ShopStep.Stock:
                SidePage(T("购买 / 商品", "Buy / Stock"), Stock.Length, GoldNow);
                for (int i = 0; i < Stock.Length; i++)
                {
                    int id = Stock[i], asking = BasePrice(id);
                    Item(CommandRow(i), ItemName(id), ItemArt(id), _shopGold >= asking,
                        _shopGold < asking ? FundsReason : "", $"{asking} G", slot: true);
                }
                break;
            case ShopStep.Member:
                SidePage(_buying ? T("购买 / 接收人", "Buy / Recipient") : T("出售 / 持有人", "Sell / Owner"), 3, GoldNow);
                foreach (var member in Enum.GetValues<Member>())
                {
                    int occupied = _packs[member].Count;
                    bool enabled = _buying ? occupied < PackCapacity : occupied > 0;
                    Item(CommandRow((int)member), MemberUnit(member).Name, enabled: enabled,
                        reason: enabled ? "" : _buying ? FullReason : T("背包为空。", "The pack is empty."),
                        value: $"{occupied}/{PackCapacity}");
                }
                ShopPack((Member)_focus);
                break;
            case ShopStep.Slot:
                SidePage(T("出售 / 物品槽", "Sell / Slot"), 2, GoldNow);
                _saleSummary = Text(_surface, new(CommandOrigin, new(CommandSize.X, 148)), "", 30);
                ShopPack(_shopMember);
                break;
            case ShopStep.Quote:
            case ShopStep.Receipt:
                bool quote = _shopStep == ShopStep.Quote;
                int price = TradePrice(_tradeItem);
                int after = _goldBefore + (_buying ? -price : price);
                string title = quote ? T("交易确认", "Review trade") : T("交易完成", "Trade complete");
                var panel = SidePage(title, quote ? 5 : 4, GoldNow);
                Text(panel, new(16, 64, CommandSize.X, 76), $"{TradeAction}   {ItemName(_tradeItem)}    {price} G", 30);
                string receiver = $"{TradeOwner}  ·  {T("物品槽", "Slot")} {_tradeSlot + 1}";
                if (!_buying && IsWeapon(_tradeItem))
                    receiver += T("  ·  卸下武器", "  ·  Unequip weapon");
                Text(panel, new(16, 141, CommandSize.X, 80), receiver, 28);
                Text(panel, new(16, 221, CommandSize.X, 60), T($"余额   {_goldBefore} → {after} G", $"Balance   {_goldBefore} → {after} G"), 28);
                ShopPack(_shopMember);
                if (quote)
                {
                    string refusal = InvalidQuote();
                    Item(CommandRow(3), T("确认交易", "Confirm trade"), enabled: refusal.Length == 0, reason: refusal);
                    Item(CommandRow(4), T("取消", "Cancel"), reason: refusal);
                }
                else Item(CommandRow(3), T("返回商店", "Return to shop"));
                break;
            case ShopStep.Closed:
                SidePage(shop, 1);
                Item(CommandRow(0), T("与商人交谈", "Talk to merchant"));
                break;
        }
        // Reasons occupy the existing hint area and never move the item or hit target.
        var hints = Panel(new(32, 997, 1856, 60), true);
        Text(hints, new(18, 0, 710, 60), T("↑ ↓ 选择     Enter / A 确认", "↑ ↓ Select     Enter / A Confirm"), 24, "cad6e5");
        _detail = Text(hints, new(740, 0, 860, 60), "", 24);
        Utility(new(1654, 1003, 204, 48), T("Esc / B 返回", "Esc / B Back"), BackShop);
    }

    private void RefreshShopPack()
    {
        if (_shopStep == ShopStep.Slot && _saleSummary is not null)
        {
            int id = SlotId(TradePack, _focus);
            _saleSummary.Text = $"{TradeOwner}   ·   {SlotLabel(id, _focus)}";
            if (id >= 0)
                _saleSummary.Text += T($"\n卖价   {TradePrice(id)} G", $"\nSell price   {TradePrice(id)} G") +
                    (IsWeapon(id) ? "   ·   " + GameText.Get("party.equipped") : "");
        }
        if (_shopStep != ShopStep.Member || _shopPackPanel is null || _drawnPack == (Member)_focus) return;
        _surface.RemoveChild(_shopPackPanel);
        _shopPackPanel.QueueFree();
        ShopPack((Member)_focus);
    }

    private void ShopPack(Member member)
    {
        var pack = _packs[member];
        var origin = new Vector2(704, PanelOrigin.Y);
        bool buyingForMember = _buying && _shopStep == ShopStep.Member;
        var panel = Panel(new(origin, new(560, buyingForMember ? 462 : 424)));
        panel.Name = "ShopPack";
        _shopPackPanel = panel;
        _drawnPack = member;
        Text(panel, new(16, 6, 350, 52), MemberUnit(member).Name, 32);
        Text(panel, new(368, 6, 176, 52), T($"背包   {pack.Count}/4", $"Pack   {pack.Count}/4"), 24)
            .HorizontalAlignment = HorizontalAlignment.Right;
        for (int i = 0; i < PackCapacity; i++)
        {
            int id = SlotId(pack, i);
            string value = !_buying && id >= 0 ? $"{TradePrice(id)} G" : IsWeapon(id) ? GameText.Get("party.equipped") : "";
            Rect2 row = new(16, 64 + 88 * i, CommandSize.X, CommandSize.Y);
            string name = SlotLabel(id, i);
            if (_shopStep == ShopStep.Slot)
                Item(new(origin + row.Position, row.Size), name, ItemArt(id), id >= 0,
                    id < 0 ? T("这个槽位是空的。", "This slot is empty.") : "", value, slot: true);
            else
            {
                var line = Put(panel, new Control { Name = $"Slot{i}", MouseFilter = MouseFilterEnum.Ignore }, row);
                var key = ItemArt(id);
                var (label, art) = ItemContent(line, name, key, value, true);
                bool marked = i == _tradeSlot && (_shopStep == ShopStep.Quote ||
                    (_shopStep == ShopStep.Receipt && _buying && id == _tradeItem));
                art.Texture = key is null ? null : _stage.Icon(key, marked);
                label.AddThemeColorOverride("font_color", marked ? StoryTheme.Selected :
                    id < 0 ? StoryTheme.Unavailable : new Color("f4f5fc"));
            }
        }
        if (buyingForMember)
            Text(panel, new(16, 410, CommandSize.X, 44), $"{ItemName(_tradeItem)}   ·   {TradePrice(_tradeItem)} G", 24, "cad6e5");
    }
}
