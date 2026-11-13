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
    private readonly FieldChoicePanel _page = new();
    private readonly FieldPackPanel _pack = new();
    private readonly ColorRect _shade = new() { Color = new(0, 0, 0, 0.35f), MouseFilter = MouseFilterEnum.Ignore };
    private readonly FieldServiceMenu _services = new("shop-menu", ("Buy", "shop.buy"), ("Sell", "shop.sell"),
        ("Repair", "shop.repair"), ("Deals", "shop.deals"));
    private readonly IdentityIcons _icons = new();
    private object? _assetSource;
    private string? _assetError;
    private readonly List<string?> _identities = [];
    private bool IsMenu => _wait?.Phase == ShopPhase.Menu;
    private string _title = "", _hint = "";
    private readonly Dictionary<ShopPhase, int> _focuses = [];
    private readonly List<(string Text, ShopCommand? Command)> _options = [];
    private readonly MenuPointer _pointer = new();
    private GameSession _session = null!;
    private GameInput _input = null!;
    private Func<ShopCommand, long, SessionResult> _submit = null!;
    private ShopWait? _wait;
    private long _revision;
    private string? _language, _error;
    private int _index;
    private bool _release = true;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Stop;
        _shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(_shade); AddChild(_page); AddChild(_services); AddChild(_pack);
        _pack.InvalidatePointer = _page.InvalidatePointer = _services.InvalidatePointer = _pointer.Cancel;
        Hide();
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
        if (changed) { _revision = session.Current.Revision; _index = _focuses.GetValueOrDefault(wait.Phase); _release = true; _error = null; _pointer.Suspend(); }
        if (changed || _language != GameText.Language)
        {
            _wait = wait; _language = GameText.Language; Build(wait);
        }
        _pointer.ReleaseIfNeutral(); if (!input.GameplayHeld) _release = false;
        _index = Math.Clamp(_index, 0, Math.Max(0, _options.Count - 1));
        _page.Visible = !IsMenu; _services.Visible = IsMenu;
        string reason = _assetError is not null ? GameText.Get("shop.closed") :
            _error ?? (IsMenu && _index is 2 or 3 ? GameText.Get("shop.unavailable") : "");
        _hint = GameText.Get("shop.hint", input.CompactHint(GameAction.Up), input.CompactHint(GameAction.Down), input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Cancel));
        _hint += "\n" + input.CompactHint(GameAction.Left) + "/" + input.CompactHint(GameAction.Right) + " · ↕";
        if (IsMenu) _services.Present(input, _index, _title, reason, _hint,
            "shop.leave", [_assetError is null, _assetError is null, false, false]);
        else _page.Present(reason.Length == 0 ? _title : _title + "\n" + reason, _hint, input.CompactHint(GameAction.Cancel) + " · " + GameText.Get("shop.back"),
            _options.Select((option, i) => (option.Text, _identities[i], option.Command is not null)).ToArray(), _icons, _index);
        var members = session.QueryPartyDetails()!.Members;
        ActorRef? actor = wait.Offer?.Member ?? wait.Member;
        if (wait.Phase == ShopPhase.Member && _options[_index].Command is SelectShopMember memberChoice) actor = memberChoice.Member;
        var member = members.FirstOrDefault(row => row.Carried.Actor == actor);
        _pack.Visible = member is not null && wait.Phase is ShopPhase.Member or ShopPhase.Slot or ShopPhase.Confirm;
        if (_pack.Visible) _pack.Present(member!, PartyStatusView.MemberName(member!, session), _icons, _page.Position);
    }

    private void Build(ShopWait wait)
    {
        _options.Clear(); _identities.Clear();
        _title = GameText.Get("shop.title", _session.Current.Exploration!.Party.Gold?.ToString() ?? "?");
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
                break;
            case ShopPhase.Member:
                _title += "\n" + GameText.Get(wait.Action == ShopAction.Buy ? "shop.recipient" : "shop.seller");
                if (wait.Item is { } itemId) _title += "\n" + GameText.Get("shop.stock", DisplayNames.Item(itemId, wait.Definition.Items[itemId].Name), wait.Definition.Items[itemId].Price);
                foreach (var member in _session.QueryPartyDetails()!.Members)
                    _options.Add((PartyStatusView.MemberName(member, _session), new SelectShopMember(wait.Token, member.Carried.Actor)));
                break;
            case ShopPhase.Slot:
                var owner = _session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == wait.Member);
                var loadout = owner.Carried.SourceLoadout ?? owner.Carried.Progress?.SourceLoadout ?? owner.Deployment!.Definition.SourceLoadout!;
                for (int slot = 0; slot < loadout.Items.Count; slot++)
                {
                    ushort word = loadout.Items[slot]; byte id = (byte)(word & 127);
                    if (id == 127) { _options.Add(($"{slot + 1}. " + GameText.Get("party.empty"), null)); continue; }
                    string name = wait.Definition.Items.TryGetValue(id, out var item) ? DisplayNames.Item(id, item.Name) : GameText.Get("shop.unknown", id);
                    _options.Add((GameText.Get("shop.slot", slot + 1, name, (word & 128) != 0 ? GameText.Get("shop.equipped") : ""),
                        new SelectShopSlot(wait.Token, slot, word)));
                }
                break;
            case ShopPhase.Confirm:
                var offer = wait.Offer!;
                var selected = _session.QueryPartyDetails()!.Members.Single(row => row.Carried.Actor == offer.Member);
                _title += "\n" + GameText.Get(wait.Action == ShopAction.Buy ? "shop.confirm-buy" : "shop.confirm-sell",
                    DisplayNames.Item(offer.Item, wait.Definition.Items[offer.Item].Name), PartyStatusView.MemberName(selected, _session), offer.Price);
                uint balance = _session.Current.Exploration!.Party.Gold ?? 0;
                long after = wait.Action == ShopAction.Buy ? (long)balance - offer.Price : Math.Min(9_999_999L, (long)balance + offer.Price);
                bool full = selected.Inventory.All(slot => slot.Empty == false);
                string? refusal = wait.Action == ShopAction.Buy ? full ? "full" : balance < offer.Price ? "insufficientgold" : null :
                    wait.Definition.Items[offer.Item].Unsellable ? "unsellable" : wait.Definition.Items[offer.Item].Rare || (offer.Word.GetValueOrDefault() & 256) != 0 ? "unsupporteditem" : null;
                _title += "\n" + GameText.Get("party.gold", balance.ToString()) + " → " + (refusal is null ? after : balance);
                if (refusal is not null) _title += "\n" + GameText.Get("shop.result." + refusal);
                Add("shop.confirm", new AnswerShop(wait.Token, offer, true)); Add("shop.back", new AnswerShop(wait.Token, offer, false)); break;
            case ShopPhase.Result:
                _title += "\n" + GameText.Get("shop.result." + wait.Result!.Value.ToString().ToLowerInvariant());
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
        _focuses[_wait!.Phase] = _index; _release = true; _pointer.Suspend();
        var result = _submit(command, _revision);
        if (result.Failure is not null) _error = GameText.Get("shop.failed");
        Present(_session, _input, _submit);
    }
    internal void Handle(GameAction action)
    {
        _pointer.Cancel(); if (_release || _wait is null) return;
        if (action == GameAction.Cancel) Choose(_wait.Phase == ShopPhase.Result ? new ContinueShop(_wait.Token) : new BackShop(_wait.Token));
        else if (action is GameAction.Left or GameAction.Right) { if (IsMenu) _services.Wheel(action == GameAction.Left ? -192 : 192); else _page.Wheel(action == GameAction.Left ? -192 : 192); if (_pack.Visible) _pack.Wheel(action == GameAction.Left ? -192 : 192); return; }
        else if (action is GameAction.Up or GameAction.Down) { _index = (_index + (action == GameAction.Up ? -1 : 1) + _options.Count) % _options.Count; _error = null; }
        else if (action == GameAction.Confirm) Choose(_options[_index].Command);
        Present(_session, _input, _submit);
    }
    internal void HandlePointer(InputEvent input)
    {
        if (_wait is null || input is not InputEventMouse mouse) return;
        int hit = IsMenu ? _services.Hit(mouse.Position) : _page.Hit(mouse.Position);
        Rect2 backRect = IsMenu ? _services.BackRect : _page.BackRect;
        bool back = backRect.HasPoint(mouse.Position);
        if (input is InputEventMouseMotion motion)
        { if (_pointer.Moved(motion) && !_release && (hit >= 0 || IsMenu && back)) { _index = back ? 4 : hit; _error = null; Present(_session, _input, _submit); } return; }
        if (input is not InputEventMouseButton button) return;
        if (button.Pressed && button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        { if (_pack.Visible && _pack.GetGlobalRect().HasPoint(mouse.Position)) { _pack.Wheel(button.ButtonIndex == MouseButton.WheelUp ? -112 : 112); return; } if (IsMenu) _services.Wheel(button.ButtonIndex == MouseButton.WheelUp ? -112 : 112); else _page.Wheel(button.ButtonIndex == MouseButton.WheelUp ? -112 : 112); return; }
        back |= button.ButtonIndex == MouseButton.Right;
        Rect2 rect = back ? button.ButtonIndex == MouseButton.Right ? GetGlobalRect() : backRect : hit < 0 ? default : IsMenu ? _services.RowRect(hit) : _page.RowRect(hit);
        string? clicked = _pointer.Read(button, back ? "back" : hit < 0 ? null : hit.ToString(), rect, _wait.Token);
        if (_release || clicked is null) return;
        if (clicked == "back") Choose(_wait.Phase == ShopPhase.Result ? new ContinueShop(_wait.Token) : new BackShop(_wait.Token));
        else { _index = int.Parse(clicked); Choose(_options[_index].Command); }
    }
    internal object Observe() => new { visible = IsVisibleInTree(), wait = _wait, index = _index, revision = _revision,
        title = _title, hint = _hint, error = _assetError ?? _error, services = _services.Observe(), page = _page.Observe(), pack = _pack.Observe(), requiresRelease = _release };
}
