using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Disposable focus/scroll state only; the session owns every offer and resource mutation.
internal sealed partial class ShopServiceView : Control
{
    private readonly Panel _panel = new();
    private readonly ColorRect _shade = new() { Color = new(0, 0, 0, 0.8f), MouseFilter = MouseFilterEnum.Ignore };
    private readonly DiamondMenuView _services = new("shop-menu", ("Buy", "shop.buy"), ("Sell", "shop.sell"),
        ("Repair", "shop.repair"), ("Deals", "shop.deals"));
    private readonly IdentityIcons _icons = new();
    private readonly List<IdentityIcon> _rowIcons = [];
    private object? _assetSource;
    private string? _assetError;
    private readonly List<string?> _identities = [];
    private bool IsMenu => _wait?.Phase == ShopPhase.Menu;
    private readonly Label _title = Text("Title", 30), _hint = Text("Hint", 23);
    private readonly List<Label> _rows = [];
    private readonly List<(string Text, ShopCommand? Command)> _options = [];
    private readonly MenuPointer _pointer = new();
    private GameSession _session = null!;
    private GameInput _input = null!;
    private Func<ShopCommand, long, SessionResult> _submit = null!;
    private ShopWait? _wait;
    private long _revision;
    private string? _language, _error;
    private int _index, _first;
    private bool _release = true;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Stop;
        _shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(_shade); AddChild(_panel); AddChild(_services);
        _panel.Position = new(380, 86); _panel.Size = new(1160, 936);
        _panel.AddChild(_title); _title.Position = new(32, 24); _title.Size = new(1096, 160);
        _title.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        for (int i = 0; i < 6; i++)
        {
            var row = Text("Choice" + i, 27); _rows.Add(row); _panel.AddChild(row);
            row.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            var icon = new IdentityIcon(); _rowIcons.Add(icon); _panel.AddChild(icon);
        }
        _panel.AddChild(_hint); _hint.Position = new(32, 802); _hint.Size = new(1096, 110);
        _hint.AutowrapMode = TextServer.AutowrapMode.WordSmart; Hide();
    }

    internal void Present(GameSession session, GameInput input, Func<ShopCommand, long, SessionResult> submit)
    {
        _session = session; _input = input; _submit = submit;
        if (session.Current.Story.Wait is not ShopWait wait) { Hide(); _wait = null; return; }
        Show();
        if (!ReferenceEquals(_assetSource, session.Definition))
        {
            _assetSource = session.Definition; _assetError = null;
            try
            {
                _services.Configure(session.Definition.Exploration?.Visuals?.ShopMenu);
                _icons.Configure(session.Definition.Exploration?.Visuals?.IdentityIcons);
            }
            catch (InvalidOperationException error) { _assetError = error.Message; }
        }
        bool changed = _wait?.Token != wait.Token;
        if (changed) { _revision = session.Current.Revision; _index = 0; _first = 0; _release = true; _error = null; _pointer.Suspend(); }
        if (changed || _language != GameText.Language)
        {
            _wait = wait; _language = GameText.Language; Build(wait);
        }
        _pointer.ReleaseIfNeutral(); if (!input.GameplayHeld) _release = false;
        _first = Math.Clamp(_first, Math.Max(0, _index - (_rows.Count - 1)), _index);
        _panel.Visible = _shade.Visible = !IsMenu; _services.Visible = IsMenu;
        for (int row = 0; row < _rows.Count; row++)
        {
            int i = row + _first; _rows[row].Visible = i < _options.Count;
            _rowIcons[row].Visible = false;
            if (i >= _options.Count) continue;
            bool iconPage = wait.Phase is ShopPhase.Stock or ShopPhase.Slot or ShopPhase.Confirm;
            _rowIcons[row].Present(_icons, _identities[i], i == _index);
            _rowIcons[row].Position = new(32, 190 + row * 100); _rowIcons[row].Size = new(64, 96);
            _rows[row].Position = new(iconPage ? 116 : 32, 190 + row * 100);
            _rows[row].Size = new(iconPage ? 1012 : 1096, 96);
            _rows[row].Text = (i == _index ? "> " : "   ") + _options[i].Text;
            if (i == _index || _options[i].Command is null)
                _rows[row].AddThemeColorOverride("font_color", _options[i].Command is null ? StoryTheme.Unavailable : StoryTheme.Selected);
            else _rows[row].RemoveThemeColorOverride("font_color");
        }
        string up = input.CompactHint(GameAction.Up), down = input.CompactHint(GameAction.Down);
        string directions = IsMenu ? up + "/" + input.CompactHint(GameAction.Left) : up;
        string otherDirections = IsMenu ? input.CompactHint(GameAction.Right) + "/" + down : down;
        _hint.Text = (_assetError is null ? "" : _assetError + "\n") + (_error is null ? "" : _error + "\n") +
            GameText.Get("shop.hint", directions, otherDirections, input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Cancel));
        if (GameText.Language == "zh-CN") _hint.Text += "\n" + GameText.Get("localization.names");
        if (IsMenu) _services.Present(input, _index, _title.Text, _hint.Text + "\n" +
            GameText.Get(_index >= 2 ? "shop.unavailable" : "shop." + (_index == 0 ? "buy" : "sell")), "shop.leave", [_assetError is null, _assetError is null, false, false]);
    }

    private void Build(ShopWait wait)
    {
        _options.Clear(); _identities.Clear();
        _title.Text = GameText.Get("shop.title", _session.Current.Exploration!.Party.Gold?.ToString() ?? "?");
        void Add(string key, ShopCommand? command) => _options.Add((GameText.Get(key), command));
        switch (wait.Phase)
        {
            case ShopPhase.Menu:
                Add("shop.buy", new ChooseShop(wait.Token, ShopAction.Buy)); Add("shop.sell", new ChooseShop(wait.Token, ShopAction.Sell));
                Add("shop.repair", null); Add("shop.deals", null); break;
            case ShopPhase.Stock:
                foreach (byte id in wait.Definition.Stock)
                {
                    var item = wait.Definition.Items[id];
                    _options.Add((GameText.Get("shop.stock", DisplayNames.Item(id, item.Name), item.Price), new SelectShopItem(wait.Token, id)));
                }
                Add("shop.back", new BackShop(wait.Token)); break;
            case ShopPhase.Member:
                _title.Text += "\n" + GameText.Get(wait.Action == ShopAction.Buy ? "shop.recipient" : "shop.seller");
                foreach (var member in _session.QueryPartyDetails()!.Members)
                    _options.Add((PartyStatusView.MemberName(member, _session), new SelectShopMember(wait.Token, member.Carried.Actor)));
                Add("shop.back", new BackShop(wait.Token)); break;
            case ShopPhase.Slot:
                var owner = _session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == wait.Member);
                var loadout = owner.Carried.SourceLoadout ?? owner.Carried.Progress?.SourceLoadout ?? owner.Deployment!.Definition.SourceLoadout!;
                for (int slot = 0; slot < loadout.Items.Count; slot++)
                {
                    ushort word = loadout.Items[slot]; byte id = (byte)(word & 127); if (id == 127) continue;
                    string name = wait.Definition.Items.TryGetValue(id, out var item) ? DisplayNames.Item(id, item.Name) : GameText.Get("shop.unknown", id);
                    _options.Add((GameText.Get("shop.slot", slot + 1, name, (word & 128) != 0 ? GameText.Get("shop.equipped") : ""),
                        new SelectShopSlot(wait.Token, slot, word)));
                }
                Add("shop.back", new BackShop(wait.Token)); break;
            case ShopPhase.Confirm:
                var offer = wait.Offer!;
                var selected = _session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == offer.Member);
                _title.Text += "\n" + GameText.Get(wait.Action == ShopAction.Buy ? "shop.confirm-buy" : "shop.confirm-sell",
                    DisplayNames.Item(offer.Item, wait.Definition.Items[offer.Item].Name), PartyStatusView.MemberName(selected, _session), offer.Price);
                Add("shop.confirm", new AnswerShop(wait.Token, offer, true)); Add("shop.back", new AnswerShop(wait.Token, offer, false)); break;
            case ShopPhase.Result:
                _title.Text += "\n" + GameText.Get("shop.result." + wait.Result!.Value.ToString().ToLowerInvariant());
                Add("shop.continue", new ContinueShop(wait.Token)); break;
        }
        foreach (var option in _options)
            _identities.Add(option.Command switch
            {
                SelectShopItem item => IdentityIcons.Item(item.Item),
                SelectShopSlot slot => IdentityIcons.Item(slot.Word),
                AnswerShop answer => IdentityIcons.Item(answer.Offer.Item),
                _ => null,
            });
    }

    protected override void Dispose(bool disposing) { if (disposing) _icons.Dispose(); base.Dispose(disposing); }
    internal void Suspend() { _release = true; _pointer.Suspend(); _services.Suspend(); }
    private void Choose(ShopCommand? command)
    {
        if (_assetError is not null && command is not (BackShop or ContinueShop)) { _release = true; _pointer.Suspend(); return; }
        if (command is null) { _release = true; _pointer.Suspend(); _error = GameText.Get("shop.unavailable"); Present(_session, _input, _submit); return; }
        _release = true;
        var result = _submit(command, _revision);
        if (result.Failure is { } failure) _error = GameText.Get("church.failure", failure.Code, failure.Field, failure.Message);
        Present(_session, _input, _submit);
    }
    internal void Handle(GameAction action)
    {
        _pointer.Cancel(); if (_release || _wait is null) return;
        if (action == GameAction.Cancel) Choose(_wait.Phase == ShopPhase.Result ? new ContinueShop(_wait.Token) : new BackShop(_wait.Token));
        else if (IsMenu && action is GameAction.Up or GameAction.Left or GameAction.Right or GameAction.Down)
        { _index = action switch { GameAction.Up => 0, GameAction.Left => 1, GameAction.Right => 2, _ => 3 }; _error = null; }
        else if (action is GameAction.Up or GameAction.Down) { _index = (_index + (action == GameAction.Up ? -1 : 1) + _options.Count) % _options.Count; _error = null; }
        else if (action == GameAction.Confirm) Choose(_options[_index].Command);
        Present(_session, _input, _submit);
    }
    private Rect2 RowRect(int row) => _rowIcons[row].Visible
        ? _rows[row].GetGlobalRect().Merge(_rowIcons[row].GetGlobalRect()) : _rows[row].GetGlobalRect();
    internal void HandlePointer(InputEvent input)
    {
        if (input is InputEventMouseMotion motion) { _pointer.Moved(motion); return; }
        if (input is not InputEventMouseButton button || _wait is null) return;
        int hit = IsMenu ? _services.Hit(button.Position) : Enumerable.Range(0, _rows.Count).FirstOrDefault(row => _rows[row].Visible && RowRect(row).HasPoint(button.Position), -1);
        bool back = button.ButtonIndex == MouseButton.Right || IsMenu && _services.BackHit(button.Position);
        Rect2 rect = back ? button.ButtonIndex == MouseButton.Right ? GetGlobalRect() : _services.BackRect :
            hit < 0 ? default : IsMenu ? _services.HitRect(hit) : RowRect(hit);
        string? clicked = _pointer.Read(button, back ? "back" : hit < 0 ? null : (hit + (IsMenu ? 0 : _first)).ToString(), rect, _wait.Token);
        if (_release || clicked is null) return;
        if (clicked == "back") Choose(_wait.Phase == ShopPhase.Result ? new ContinueShop(_wait.Token) : new BackShop(_wait.Token));
        else { _index = int.Parse(clicked); Choose(_options[_index].Command); }
    }

    internal object Observe() => new { visible = IsVisibleInTree(), wait = _wait, index = _index, first = _first, revision = _revision,
        title = _title.Text, hint = _hint.Text, error = _assetError ?? _error, services = _services.Observe(), panelVisible = _panel.IsVisibleInTree(), veilVisible = _shade.IsVisibleInTree(), requiresRelease = _release,
        panelRect = BattleMapViewport.Rectangle(_panel.GetGlobalRect()), titleRect = BattleMapViewport.Rectangle(_title.GetGlobalRect()),
        titleMinimumHeight = _title.GetMinimumSize().Y, hintRect = BattleMapViewport.Rectangle(_hint.GetGlobalRect()), hintMinimumHeight = _hint.GetMinimumSize().Y,
        icons = _rowIcons.Select(icon => icon.Observe()), rows = _rows.Where(row => row.Visible).Select(row => new { text = row.Text, rect = BattleMapViewport.Rectangle(row.GetGlobalRect()) }) };
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}
