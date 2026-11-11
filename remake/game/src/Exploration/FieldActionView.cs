using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Provisional menu state only. The captured revision owns the eventual single command.
internal sealed partial class FieldActionView : Control
{
    private readonly FieldChoicePanel _page = new();
    private readonly FieldPackPanel _pack = new();
    private readonly ColorRect _shade = new() { Color = new(0, 0, 0, 0.35f), MouseFilter = MouseFilterEnum.Ignore };
    private IdentityIcons _iconOwner = null!;
    private readonly FieldServiceMenu _operations = new("item-menu", ("Use", "prepare.use"),
        ("Give", "prepare.give"), ("Equip", "prepare.equip"), ("Drop", "prepare.drop")) { Name = "ItemOperations" };
    private FieldInventoryChoice?[] _slotItems = [];
    private ActorRef[] _targets = [];
    private ActorRef? _packActor;
    private string? _givingItem;
    private object? _iconSource;
    private readonly List<(Func<string> Label, Action Choose, bool Available)> _options = [];
    private readonly MenuPointer _pointer = new();
    private GameSession _session = null!;
    private GameInput _input = null!;
    private Func<FieldCommand, long, SessionResult> _submit = null!;
    private ActorRef _member;
    private FieldActionChoices _choices = null!;
    private string _stage = "actions", _name = "";
    private Func<string> _titleText = () => "";
    private int _index;
    private long _generation;
    private readonly Stack<Action> _parents = [];
    private Action? _restore;
    private bool _requiresRelease;
    internal bool Open => Visible;
    internal bool ItemOperations => _stage == "item-actions";

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Stop;
        _shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(_shade); AddChild(_page); AddChild(_pack); AddChild(_operations);
        _pack.InvalidatePointer = _page.InvalidatePointer = _operations.InvalidatePointer = _pointer.Cancel;
        Hide();
    }

    private string _entry = "actions";
    internal void Begin(GameSession session, GameInput input, ActorRef member, string name, Func<FieldCommand, long, SessionResult> submit, IdentityIcons icons, string entry = "actions")
    {
        _entry = entry;
        _session = session; _input = input; _member = member; _name = name; _submit = submit; _iconOwner = icons;
        _parents.Clear(); _restore = null; _assetFailure = null; _packActor = null; Show();
        if (_entry == "item" && ConfigureOperations(session) is { } failure)
        {
            _assetFailure = failure;
            Menu("result", () => GameText.Get("prepare.failure", Recovery("field-preparation-unsupported"),
                failure.Code, failure.Field, failure.Message), [(() => GameText.Get("prepare.back"), Hide, true)]);
            return;
        }
        Actions();
    }
    private SessionFailure? ConfigureOperations(GameSession session)
    {
        if (!ReferenceEquals(_iconSource, session.Definition))
        {
            try
            {
                _operations.Configure(session.Definition.Exploration?.Visuals?.ItemMenu);
            }
            catch (InvalidOperationException error)
            {
                return new(SessionFailureKind.AdapterError, error.Message, "presentation.itemMenu", "Item operation icon decoding failed.");
            }
            _iconSource = session.Definition;
        }
        return null;
    }
    private SessionFailure? _assetFailure;

    private void Menu(string stage, Func<string> title, IEnumerable<(Func<string> Label, Action Choose, bool Available)> options)
    {
        if (_restore is { } restore && stage is not ("actions" or "item-actions"))
        {
            int index = _index;
            _parents.Push(() => { restore(); _index = index; Present(); });
        }
        if (stage is "actions" or "item-actions") _parents.Clear();
        var saved = options.ToArray();
        _restore = () => { _restore = null; Menu(stage, title, saved); };
        if (stage is not ("confirm" or "exchange")) _packActor = null;
        if (stage is "actions" or "item-actions" or "give-item" or "use-item" or "drop-item" or "equipment" or "magic") _givingItem = null;
        _stage = stage; _titleText = title; _index = 0; _generation++; _pointer.Suspend(); _requiresRelease = true;
        _options.Clear(); _options.AddRange(saved);
        if (_options.Count == 0) _options.Add((() => GameText.Get("prepare.empty"), Back, true));
        Present();
    }

    private void Back()
    {
        if (_parents.TryPop(out var parent)) parent(); else Hide();
    }

    private void Actions()
    {
        _parents.Clear(); _restore = null;
        _choices = _session.QueryFieldActions(_member);
        if (_choices.Error is { } error) { Message(() => GameText.Get("prepare.unavailable", Recovery(error), error)); return; }
        if (_entry == "magic") { Magic(); return; }
        if (_entry == "item")
        {
            Menu("item-actions", () => GameText.Get("prepare.title", _name), [
                (() => GameText.Get("prepare.use"), Use, true), (() => GameText.Get("prepare.give"), Give, true),
                (() => GameText.Get("prepare.equip"), Equip, true), (() => GameText.Get("prepare.drop"), Drop, true),
                (() => GameText.Get("prepare.back"), Hide, true)]);
            return;
        }
        Menu("actions", () => GameText.Get("prepare.title", _name), [
            (() => GameText.Get("prepare.equip"), Equip, true), (() => GameText.Get("prepare.give"), Give, true),
            (() => GameText.Get("prepare.use"), Use, true), (() => GameText.Get("prepare.magic"), Magic, true),
            (() => GameText.Get("prepare.drop"), Drop, true), (() => GameText.Get("prepare.back"), Hide, true)]);
    }

    private string EquipmentName(BattleEquipmentChoice choice) => choice.Slot == 4 ? GameText.Get("prepare.unarmed") :
        DisplayNames.ItemWord(_choices.Items.Single(item => item.Slot == choice.Slot).Word, choice.Label);
    private void Equip()
    {
        _slotItems = Enumerable.Range(0, 4).Select(slot => _choices.Items.FirstOrDefault(item => item.Slot == slot)).ToArray();
        var options = Enumerable.Range(0, 4).Select(slot =>
        {
            var item = _slotItems[slot]; var choice = _choices.Equipment.FirstOrDefault(row => row.Slot == slot);
            return ((Func<string>)(() => $"{slot + 1}. " + (item is null ? GameText.Get("party.empty") : choice is null ?
                DisplayNames.ItemWord(item.Word, item.Name) + " · " + GameText.Get("field-menu.unavailable") :
                GameText.Get("prepare.equipment-row", EquipmentName(choice), choice.Equipped ? " · " + GameText.Get("party.equipped") : "", choice.Attack))),
                (Action)(() => { if (choice is null) Message(() => GameText.Get("field-menu.unavailable")); else Confirm(new FieldEquip(_member, slot, item!.Word),
                    () => GameText.Get("prepare.confirm-equip", EquipmentName(choice), choice.Attack)); }), choice is not null);
        }).ToList();
        var unarmed = _choices.Equipment.FirstOrDefault(row => row.Slot == 4);
        if (unarmed is not null) options.Add((() => GameText.Get("prepare.equipment-row", GameText.Get("prepare.unarmed"), "", unarmed.Attack),
            () => Confirm(new FieldEquip(_member, 4, null), () => GameText.Get("prepare.confirm-equip", GameText.Get("prepare.unarmed"), unarmed.Attack)), true));
        Menu("equipment", () => GameText.Get("prepare.equipment", _name), options);
    }
    private void Items(string stage, Func<string> title, Func<FieldInventoryChoice, string> label, Action<FieldInventoryChoice> choose, Func<FieldInventoryChoice, bool>? available = null)
    {
        _slotItems = Enumerable.Range(0, 4).Select(slot => _choices.Items.FirstOrDefault(item => item.Slot == slot)).ToArray();
        Menu(stage, title, _slotItems.Select((item, slot) => ((Func<string>)(() => $"{slot + 1}. " + (item is null ? GameText.Get("party.empty") : label(item))),
            (Action)(() => { if (item is null) Message(() => GameText.Get("party.empty")); else choose(item); }), item is not null && (available?.Invoke(item) ?? true))));
    }

    private IReadOnlyList<FieldInventoryChoice> _exchangeItems = [];
    private void Give() => Items("give-item", () => GameText.Get("prepare.give-item"), item => DisplayNames.ItemWord(item.Word, item.Name), item =>
    {
        _givingItem = _name + " · " + DisplayNames.ItemWord(item.Word, item.Name);
        Targets("give-target", () => GameText.Get("prepare.give-target"), true, target =>
        {
            var other = _session.QueryFieldActions(target);
            if (other.Error is { } error) { Message(() => GameText.Get("prepare.unavailable", Recovery(error), error)); return; }
            if (other.Items.Count == 4)
            {
                _exchangeItems = other.Items; _packActor = target;
                Menu("exchange", () => GameText.Get("prepare.exchange"), other.Items.Select(returned =>
                    ((Func<string>)(() => $"{returned.Slot + 1}. {DisplayNames.ItemWord(returned.Word, returned.Name)}"), (Action)(() => Confirm(
                        new FieldGive(_member, item.Slot, item.Word, target, returned.Slot, returned.Word),
                        () => GameText.Get("prepare.confirm-exchange", DisplayNames.ItemWord(item.Word, item.Name), DisplayNames.ItemWord(returned.Word, returned.Name)))), true)));
            }
            else { _packActor = target; Confirm(new FieldGive(_member, item.Slot, item.Word, target), () => GameText.Get("prepare.confirm-give", DisplayNames.ItemWord(item.Word, item.Name))); }
        });
    });

    private void Use() => Items("use-item", () => GameText.Get("prepare.use-item"), item => DisplayNames.ItemWord(item.Word, item.Name) +
        (item.Use is null ? " · " + GameText.Get("party.value-unavailable") : ""), item =>
        {
            if (item.Use is null) { Message(() => GameText.Get("prepare.use-unsupported")); return; }
            if (item.Use == FieldRecoveryEffect.NoEffect)
                Confirm(new FieldUseItem(_member, item.Slot, item.Word), () => GameText.Get("prepare.confirm-no-effect-item", DisplayNames.ItemWord(item.Word, item.Name)));
            else Targets("use-target", () => GameText.Get("prepare.use-target"), true, target => Confirm(new FieldUseItem(_member, item.Slot, item.Word, target),
                () => GameText.Get("prepare.confirm-use", DisplayNames.ItemWord(item.Word, item.Name))));
        }, item => item.Use is not null);

    private void Magic() => Menu("magic", () => GameText.Get("prepare.choose-magic"), _choices.Spells.Select(spell =>
        ((Func<string>)(() => GameText.Get("prepare.spell-row", DisplayNames.Spell(spell.Spell, spell.Name), spell.Spell.Level) + $" · {spell.MpCost} MP"), (Action)(() =>
        {
            if (spell.Effect == FieldRecoveryEffect.NoEffect)
                Confirm(new FieldCast(_member, spell.Spell), () => GameText.Get("prepare.confirm-no-effect-spell", DisplayNames.Spell(spell.Spell, spell.Name)));
            else if (spell.Effect == FieldRecoveryEffect.Egress)
                Confirm(new FieldCast(_member, spell.Spell), () => GameText.Get("prepare.confirm-egress", DisplayNames.Spell(spell.Spell, spell.Name), spell.MpCost));
            else Targets("magic-target", () => GameText.Get("prepare.magic-target"), true, target => Confirm(new FieldCast(_member, spell.Spell, target),
                () => GameText.Get("prepare.confirm-cast", DisplayNames.Spell(spell.Spell, spell.Name), spell.MpCost)));
        }), true)));

    private void Drop() => Items("drop-item", () => GameText.Get("prepare.drop-item"), item => DisplayNames.ItemWord(item.Word, item.Name) +
        (!item.CanDrop ? " · " + GameText.Get("prepare.cannot-drop") : ""), item =>
        {
            if (!item.CanDrop) { Message(() => GameText.Get("prepare.drop-refused")); return; }
            Confirm(new FieldDrop(_member, item.Slot, item.Word), () => GameText.Get("prepare.confirm-drop", DisplayNames.ItemWord(item.Word, item.Name)));
        }, item => item.CanDrop);

    private void Targets(string stage, Func<string> title, bool self, Action<ActorRef> select)
    {
        var members = _session.QueryPartyDetails()!.Members.Where(member => self || member.Carried.Actor != _member).ToArray();
        _targets = members.Select(member => member.Carried.Actor).ToArray();
        Menu(stage, title, members.Select(member => ((Func<string>)(() => PartyStatusView.MemberName(member, _session)), (Action)(() => { _packActor = member.Carried.Actor; select(member.Carried.Actor); }), true)));
    }

    private void Confirm(FieldCommand command, Func<string> title) => Menu("confirm", title,
        [(() => GameText.Get("prepare.confirm"), () =>
        {
            var result = _submit(command, _choices.Revision);
            if (result.Failure is null && !result.Snapshot.CanWaitAtInput) { Hide(); return; }
            Message(result.Failure is { } failure ? () => GameText.Get("prepare.failure", Recovery(failure.Code), failure.Code, failure.Field, failure.Message) :
                result.Observations.Any(row => row.Kind == "field-no-effect") ? () => GameText.Get("prepare.no-effect") : () => GameText.Get("prepare.done"), result.Failure is null);
        }, true), (() => GameText.Get("prepare.cancel"), Back, true)]);

    private static string Recovery(string code) => GameText.Get(code switch
    {
        "discard-custody-unavailable" => "prepare.recovery.custody",
        "field-preparation-unavailable" => "prepare.recovery.settle",
        "field-member-unavailable" => "prepare.recovery.member",
        "field-preparation-unsupported" or "field-loadout-unavailable" or "field-item-unsupported" or
        "field-equipped-item-unsupported" or "equipment-unavailable" or "field-item-effect-unsupported" or
        "field-spell-unsupported" or "field-egress-context" or "egress-map" => "prepare.recovery.unsupported",
        "field-give-exchange-required" => "prepare.recovery.exchange",
        "discard-unsellable" => "prepare.drop-refused",
        _ => "prepare.recovery.retry",
    });

    private void Message(Func<string> message, bool committed = false)
    {
        Menu("result", message, [(() => GameText.Get("prepare.back"), committed ? Actions : Back, true)]);
        if (committed) { _parents.Clear(); _restore = null; }
    }
    internal void Present()
    {
        if (!Open) return;
        _pointer.ReleaseIfNeutral(); if (!_input.GameplayHeld) _requiresRelease = false;
        _operations.Visible = ItemOperations; _page.Visible = !ItemOperations;
        string hint = GameText.Get("prepare.hint", _input.CompactHint(GameAction.Up), _input.CompactHint(GameAction.Down), _input.CompactHint(GameAction.Confirm), _input.CompactHint(GameAction.Cancel));
        hint += "\n" + _input.CompactHint(GameAction.Left) + "/" + _input.CompactHint(GameAction.Right) + " · ↕";
        if (GameText.Language == "zh-CN") hint += "\n" + GameText.Get("localization.names");
        string fallback = GameText.Fallback("prepare.", true); if (fallback.Length > 0) hint += "\n" + fallback;
        if (ItemOperations) _operations.Present(_input, _index, _titleText(), hint, "prepare.back");
        else
        {
            string? Icon(int i) => _stage switch
            {
                "magic" when i < _choices.Spells.Count => IdentityIcons.Spell(_session.Definition, _choices.Spells[i].Spell),
                "use-item" or "give-item" or "drop-item" or "equipment" when i < _slotItems.Length => _slotItems[i] is { } item ? IdentityIcons.Item(item.Word) : null,
                "exchange" when i < _exchangeItems.Count => IdentityIcons.Item(_exchangeItems[i].Word),
                "equipment" when i == 4 => IdentityIcons.Unarmed,
                _ => null,
            };
            string title = _titleText() + (_givingItem is not null && _stage is "give-target" or "exchange" or "confirm" ? "\n" + _givingItem : "");
            _page.Present(title, hint, _input.CompactHint(GameAction.Cancel) + " · " + GameText.Get("prepare.back"),
                _options.Select((option, i) => (option.Label(), Icon(i), option.Available)).ToArray(), _iconOwner, _index);
        }
        var party = _session.QueryPartyDetails();
        ActorRef actor = _stage.EndsWith("-target", StringComparison.Ordinal) && _index < _targets.Length ? _targets[_index] :
            _stage is "exchange" or "confirm" && _packActor is { } recipient ? recipient : _member;
        var member = party?.Members.FirstOrDefault(row => row.Carried.Actor == actor);
        _pack.Visible = member is not null && _stage != "result";
        if (_pack.Visible) _pack.Present(member!, PartyStatusView.MemberName(member!, _session), _iconOwner, ItemOperations ? _operations.Position : _page.Position);
    }
    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); _operations.Suspend(); }
    internal void Handle(GameAction action)
    {
        _pointer.Cancel(); if (_requiresRelease) return;
        if (action is GameAction.Cancel or GameAction.Equipment) { Back(); return; }
        if (action is GameAction.Left or GameAction.Right) { if (ItemOperations) _operations.Wheel(action == GameAction.Left ? -192 : 192); else _page.Wheel(action == GameAction.Left ? -192 : 192); if (_pack.Visible) _pack.Wheel(action == GameAction.Left ? -192 : 192); return; }
        if (action is GameAction.Up or GameAction.Down) _index = (_index + (action == GameAction.Up ? -1 : 1) + _options.Count) % _options.Count;
        else if (action == GameAction.Confirm) _options[_index].Choose();
        Present();
    }
    internal void HandlePointer(InputEvent input)
    {
        if (!GetWindow().HasFocus() || !CanProcess()) { Suspend(); return; }
        if (input is not InputEventMouse mouse) return;
        int hit = ItemOperations ? _operations.Hit(mouse.Position) : _page.Hit(mouse.Position);
        Rect2 backRect = ItemOperations ? _operations.BackRect : _page.BackRect;
        bool back = backRect.HasPoint(mouse.Position);
        if (input is InputEventMouseMotion motion)
        { if (_pointer.Moved(motion) && hit >= 0 && !_requiresRelease) { _index = hit; Present(); } return; }
        if (input is not InputEventMouseButton button) return;
        if (button.Pressed && button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        { if (_pack.Visible && _pack.GetGlobalRect().HasPoint(mouse.Position)) { _pack.Wheel(button.ButtonIndex == MouseButton.WheelUp ? -112 : 112); return; } if (ItemOperations) _operations.Wheel(button.ButtonIndex == MouseButton.WheelUp ? -112 : 112); else _page.Wheel(button.ButtonIndex == MouseButton.WheelUp ? -112 : 112); return; }
        back |= button.ButtonIndex == MouseButton.Right;
        string? clicked = _pointer.Read(button, back ? "back" : hit < 0 ? null : hit.ToString(),
            back ? button.ButtonIndex == MouseButton.Right ? GetGlobalRect() : backRect : hit < 0 ? default : ItemOperations ? _operations.RowRect(hit) : _page.RowRect(hit), _generation);
        if (!_requiresRelease && clicked is not null)
        { if (clicked == "back") Back(); else { _index = int.Parse(clicked); _options[_index].Choose(); } }
        Present();
    }
    internal object Observe() => new { visible = IsVisibleInTree(), stage = _stage, index = _index, title = _titleText(),
        revision = _choices?.Revision, requiresRelease = _requiresRelease, failure = _assetFailure,
        operations = _operations.Observe(), page = _page.Observe(), pack = _pack.Observe() };
}
