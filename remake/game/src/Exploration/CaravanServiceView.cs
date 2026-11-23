using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Disposable selection and gesture state. Membership and all phases belong to Application.
internal sealed partial class CaravanServiceView : Control
{
    private readonly FieldChoicePanel _page = new();
    private readonly FieldPackPanel _pack = new();
    private readonly IdentityIcons _icons = new();
    private readonly MenuPointer _pointer = new();
    private readonly Dictionary<CaravanPhase, int> _focus = [];
    private readonly List<(string Text, CaravanCommand Command, PartyMemberDetails? Member, bool Available)> _options = [];
    private GameSession _session = null!;
    private GameInput _input = null!;
    private Func<CaravanCommand, long, SessionResult> _submit = null!;
    private CaravanWait? _wait;
    private long _revision;
    private int _index;
    private bool _release = true;
    private string _title = "", _hint = "";
    private string? _language, _error;
    private object? _assetSource;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        Theme = StoryTheme.Create();
        MouseFilter = MouseFilterEnum.Stop;
        var shade = new ColorRect { Color = new(0, 0, 0, 0.35f), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(shade);
        AddChild(_page);
        AddChild(_pack);
        _page.InvalidatePointer = _pack.InvalidatePointer = _pointer.Cancel;
        Hide();
    }

    internal void Present(GameSession session, GameInput input, Func<CaravanCommand, long, SessionResult> submit)
    {
        _session = session;
        _input = input;
        _submit = submit;
        if (session.Current.Story.Wait is not CaravanWait wait)
        {
            Hide();
            _wait = null;
            Suspend();
            return;
        }

        if (!ReferenceEquals(_assetSource, session.Definition))
        {
            _assetSource = session.Definition;
            _icons.Configure(session.Definition.Exploration?.Visuals?.IdentityIcons);
        }

        Show();
        bool changed = _wait?.Token != wait.Token;
        if (changed)
        {
            _revision = session.Current.Revision;
            _index = _focus.GetValueOrDefault(wait.Phase);
            _release = true;
            _error = null;
            _pointer.Suspend();
        }
        if (changed || _language != GameText.Language)
        {
            _wait = wait;
            _language = GameText.Language;
            Build(wait);
        }

        _pointer.ReleaseIfNeutral();
        if (!input.GameplayHeld)
        {
            _release = false;
        }
        _index = Math.Clamp(_index, 0, Math.Max(0, _options.Count - 1));
        _hint = GameText.Get("shop.hint", input.CompactHint(GameAction.Up), input.CompactHint(GameAction.Down), input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Cancel));
        _page.Present(_title + (_error is null ? "" : "\n" + _error), _hint,
            _options.Select(row => (row.Text, row.Command switch
            {
                SelectDepotItem item => IdentityIcons.Item(item.ExpectedItem),
                SelectDepotSlot slot => IdentityIcons.Item(slot.ExpectedWord),
                SelectCaravanItemSlot { ExpectedWord: { } word } => IdentityIcons.Item(word),
                _ => (string?)null,
            }, row.Available)).ToArray(), _icons, _index);
        var member = _options.Count > 0 ? _options[_index].Member : null;
        _pack.Visible = member is not null;
        if (member is not null)
        {
            _pack.Present(member, PartyStatusView.MemberName(member, session), _icons, _page.Position);
        }
    }

    private void Build(CaravanWait wait)
    {
        _options.Clear();
        var lists = MapPartyMembership.Rebuild(_session.Current.Story.Flags, _session.Definition.Exploration!.PartyFlags!);
        _title = GameText.Get("caravan.title", lists.Active.Count, _session.Definition.Exploration.PartyFlags!.Capacity);
        void Add(string key, CaravanCommand command, bool available = true) => _options.Add((GameText.Get(key), command, null, available));
        if (wait.Action == CaravanAction.Depot)
        {
            BuildDepot(wait);
            return;
        }
        if (wait.Action == CaravanAction.Item)
        {
            BuildItems(wait);
            return;
        }
        switch (wait.Phase)
        {
            case CaravanPhase.Menu:
                Add("caravan.join", new ChooseCaravan(wait.Token, CaravanAction.Join));
                Add("caravan.depot", new ChooseCaravan(wait.Token, CaravanAction.Depot));
                Add("caravan.item", new ChooseCaravan(wait.Token, CaravanAction.Item));
                Add("caravan.purge", new ChooseCaravan(wait.Token, CaravanAction.Purge));
                break;
            case CaravanPhase.Member:
            case CaravanPhase.Relief:
                bool reserve = wait.Action == CaravanAction.Join && wait.Phase == CaravanPhase.Member;
                _title += "\n" + GameText.Get(wait.Phase == CaravanPhase.Relief ? "caravan.relief" : reserve ? "caravan.choose-join" : "caravan.choose-purge");
                var members = _session.QueryPartyDetails()!.Members;
                foreach (int index in reserve ? lists.Reserve : lists.Active)
                {
                    var member = members.SingleOrDefault(row => row.Deployment?.Initialization?.AllyPartyMember == index);
                    string name = member is null ? GameText.Get("caravan.missing", index) : PartyStatusView.MemberName(member, _session);
                    string vitals = member is null ? "" : $" · HP {member.Carried.Hp}/{member.MaxHp} · MP {member.Carried.Mp}/{member.MaxMp}";
                    _options.Add((name + vitals, new SelectCaravanMember(wait.Token, index), member, member is not null));
                }
                break;
            case CaravanPhase.ConfirmDead:
                var dead = _session.QueryPartyDetails()!.Members.Single(row => row.Deployment?.Initialization?.AllyPartyMember == wait.Member);
                _title += "\n" + GameText.Get("caravan.dead", PartyStatusView.MemberName(dead, _session));
                Add("shop.confirm", new AnswerCaravan(wait.Token, true));
                Add("shop.back", new AnswerCaravan(wait.Token, false));
                break;
            case CaravanPhase.Result:
                _title += "\n" + GameText.Get("caravan.result." + wait.Result!.Value.ToString().ToLowerInvariant());
                Add("shop.continue", new ContinueCaravan(wait.Token));
                break;
        }
    }

    internal void Suspend()
    {
        _release = true;
        _pointer.Suspend();
    }

    private CaravanCommand Back() => _wait!.Phase is CaravanPhase.Result or CaravanPhase.DepotResult or CaravanPhase.DepotLook or CaravanPhase.ItemResult ? new ContinueCaravan(_wait.Token) : new BackCaravan(_wait.Token);

    private void Choose(CaravanCommand command)
    {
        _focus[_wait!.Phase] = _index;
        _release = true;
        _pointer.Suspend();
        var result = _submit(command, _revision);
        Present(_session, _input, _submit);
        if (result.Failure is not null)
        {
            _error = GameText.Get(result.Failure.Code switch
            {
                "depot-equipped-curse" or "caravan-equipped-curse" or "equipped-curse" => "depot.cursed",
                "caravan-item-not-usable" => "caravan.items.not-usable",
                "field-item-effect-unsupported" => "caravan.items.unsupported",
                "discard-unsellable" => "prepare.drop-refused",
                _ => "caravan.failed",
            });
            Present(_session, _input, _submit);
        }
    }

    internal void Handle(GameAction action)
    {
        _pointer.Cancel();
        if (_release || _wait is null)
        {
            return;
        }
        if (action == GameAction.Cancel)
        {
            Choose(Back());
        }
        else if (action is GameAction.Left or GameAction.Right)
        {
            _page.Wheel(action == GameAction.Left ? -192 : 192);
            if (_pack.Visible)
            {
                _pack.Wheel(action == GameAction.Left ? -192 : 192);
            }
        }
        else if (_options.Count > 0 && action is GameAction.Up or GameAction.Down)
        {
            _index = (_index + (action == GameAction.Up ? -1 : 1) + _options.Count) % _options.Count;
        }
        else if (_options.Count > 0 && action == GameAction.Confirm)
        {
            Choose(_options[_index].Command);
        }
        Present(_session, _input, _submit);
    }

    internal void HandlePointer(InputEvent input)
    {
        if (_wait is null || input is not InputEventMouse mouse)
        {
            return;
        }
        int hit = _page.Hit(mouse.Position);
        if (input is InputEventMouseMotion motion)
        {
            if (_pointer.Moved(motion) && !_release && hit >= 0)
            {
                _index = hit;
                Present(_session, _input, _submit);
            }
            return;
        }
        if (input is not InputEventMouseButton button)
        {
            return;
        }
        if (button.Pressed && button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            int step = button.ButtonIndex == MouseButton.WheelUp ? -112 : 112;
            if (_pack.Visible && _pack.GetGlobalRect().HasPoint(mouse.Position))
            {
                _pack.Wheel(step);
            }
            else
            {
                _page.Wheel(step);
            }
            return;
        }
        bool back = button.ButtonIndex == MouseButton.Right;
        Rect2 rect = back ? GetGlobalRect() : hit < 0 ? default : _page.RowRect(hit);
        string? clicked = _pointer.Read(button, back ? "back" : hit < 0 ? null : hit.ToString(), rect, _wait.Token);
        if (_release || clicked is null)
        {
            return;
        }
        if (clicked == "back")
        {
            Choose(Back());
        }
        else
        {
            _index = int.Parse(clicked);
            Choose(_options[_index].Command);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _icons.Dispose();
        }
        base.Dispose(disposing);
    }

    internal object Observe() => new
    {
        visible = IsVisibleInTree(),
        wait = _wait,
        index = _index,
        revision = _revision,
        title = _title,
        hint = _hint,
        error = _error,
        page = _page.Observe(),
        pack = _pack.Observe(),
        requiresRelease = _release
    };
}
