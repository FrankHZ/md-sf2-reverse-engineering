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
        var shade = new ColorRect { Color = new(0, 0, 0, 0.8f), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade); AddChild(_panel);
        _panel.Position = new(380, 86); _panel.Size = new(1160, 936);
        _panel.AddChild(_title); _title.Position = new(32, 24); _title.Size = new(1096, 160);
        _title.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        for (int i = 0; i < 8; i++)
        {
            var row = Text("Choice" + i, 27); _rows.Add(row); _panel.AddChild(row);
            row.Position = new(32, 200 + i * 62); row.Size = new(1096, 58);
        }
        _panel.AddChild(_hint); _hint.Position = new(32, 714); _hint.Size = new(1096, 200);
        _hint.AutowrapMode = TextServer.AutowrapMode.WordSmart; Hide();
    }

    internal void Present(GameSession session, GameInput input, Func<ShopCommand, long, SessionResult> submit)
    {
        _session = session; _input = input; _submit = submit;
        if (session.Current.Story.Wait is not ShopWait wait) { Hide(); _wait = null; return; }
        Show();
        bool changed = _wait?.Token != wait.Token;
        if (changed) { _revision = session.Current.Revision; _index = 0; _first = 0; _release = true; _error = null; _pointer.Suspend(); }
        if (changed || _language != GameText.Language)
        {
            _wait = wait; _language = GameText.Language; Build(wait);
        }
        _pointer.ReleaseIfNeutral(); if (!input.GameplayHeld) _release = false;
        _first = Math.Clamp(_first, Math.Max(0, _index - 7), _index);
        for (int row = 0; row < _rows.Count; row++)
        {
            int i = row + _first; _rows[row].Visible = i < _options.Count;
            if (i >= _options.Count) continue;
            _rows[row].Text = (i == _index ? "> " : "   ") + _options[i].Text;
            if (i == _index || _options[i].Command is null)
                _rows[row].AddThemeColorOverride("font_color", _options[i].Command is null ? StoryTheme.Unavailable : StoryTheme.Selected);
            else _rows[row].RemoveThemeColorOverride("font_color");
        }
        _hint.Text = (_error is null ? "" : _error + "\n") + GameText.Get("shop.hint", input.CompactHint(GameAction.Up),
            input.CompactHint(GameAction.Down), input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Cancel));
        if (GameText.Language == "zh-CN") _hint.Text += "\n" + GameText.Get("localization.names");
    }

    private void Build(ShopWait wait)
    {
        _options.Clear();
        _title.Text = GameText.Get("shop.title", _session.Current.Exploration!.Party.Gold?.ToString() ?? "?");
        void Add(string key, ShopCommand? command) => _options.Add((GameText.Get(key), command));
        switch (wait.Phase)
        {
            case ShopPhase.Menu:
                Add("shop.buy", new ChooseShop(wait.Token, ShopAction.Buy)); Add("shop.sell", new ChooseShop(wait.Token, ShopAction.Sell));
                Add("shop.repair", null); Add("shop.deals", null); Add("shop.leave", new BackShop(wait.Token)); break;
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
    }

    internal void Suspend() { _release = true; _pointer.Suspend(); }
    private void Choose(ShopCommand? command)
    {
        if (command is null) { _error = GameText.Get("shop.unavailable"); Present(_session, _input, _submit); return; }
        _release = true;
        var result = _submit(command, _revision);
        if (result.Failure is { } failure) _error = GameText.Get("church.failure", failure.Code, failure.Field, failure.Message);
        Present(_session, _input, _submit);
    }
    internal void Handle(GameAction action)
    {
        _pointer.Cancel(); if (_release || _wait is null) return;
        if (action == GameAction.Cancel) Choose(_wait.Phase == ShopPhase.Result ? new ContinueShop(_wait.Token) : new BackShop(_wait.Token));
        else if (action is GameAction.Up or GameAction.Down) { _index = (_index + (action == GameAction.Up ? -1 : 1) + _options.Count) % _options.Count; _error = null; }
        else if (action == GameAction.Confirm) Choose(_options[_index].Command);
        Present(_session, _input, _submit);
    }
    internal void HandlePointer(InputEvent input)
    {
        if (input is InputEventMouseMotion motion) { _pointer.Moved(motion); return; }
        if (input is not InputEventMouseButton button || _wait is null) return;
        int hit = _rows.FindIndex(row => row.Visible && row.GetGlobalRect().HasPoint(button.Position));
        string? clicked = _pointer.Read(button, hit < 0 ? null : (hit + _first).ToString(), hit < 0 ? default : _rows[hit].GetGlobalRect(), _wait.Token);
        if (!_release && clicked is not null) { _index = int.Parse(clicked); Choose(_options[_index].Command); }
    }
    internal object Observe() => new { visible = IsVisibleInTree(), wait = _wait, index = _index, first = _first, revision = _revision,
        title = _title.Text, hint = _hint.Text, error = _error, requiresRelease = _release,
        panelRect = BattleMapViewport.Rectangle(_panel.GetGlobalRect()), titleRect = BattleMapViewport.Rectangle(_title.GetGlobalRect()),
        titleMinimumHeight = _title.GetMinimumSize().Y, hintRect = BattleMapViewport.Rectangle(_hint.GetGlobalRect()), hintMinimumHeight = _hint.GetMinimumSize().Y,
        rows = _rows.Where(row => row.Visible).Select(row => new { text = row.Text, rect = BattleMapViewport.Rectangle(row.GetGlobalRect()) }) };
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}
