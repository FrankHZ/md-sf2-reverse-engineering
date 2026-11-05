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
    private readonly Panel _panel = new();
    private readonly Label _title = Text("Title", 32), _hint = Text("Hint", 24);
    private readonly List<Label> _rows = [];
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

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Stop;
        var shade = new ColorRect { Color = new(0, 0, 0, 0.8f), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade); AddChild(_panel);
        _panel.Position = new(424, 128); _panel.Size = new(1072, 824);
        _panel.AddChild(_title); _title.Position = new(32, 24); _title.Size = new(1008, 112);
        _title.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _panel.AddChild(_hint); _hint.Position = new(32, 640); _hint.Size = new(1008, 164);
        _hint.AutowrapMode = TextServer.AutowrapMode.WordSmart; _hint.CustomMaximumSize = new(1008, -1);
        Hide();
    }

    private string _entry = "actions";
    internal void Begin(GameSession session, GameInput input, ActorRef member, string name, Func<FieldCommand, long, SessionResult> submit, string entry = "actions")
    {
        _entry = entry;
        _session = session; _input = input; _member = member; _name = name; _submit = submit;
        _parents.Clear(); _restore = null; Show(); Actions();
    }

    private void Menu(string stage, Func<string> title, IEnumerable<(Func<string> Label, Action Choose, bool Available)> options)
    {
        if (_restore is { } restore && stage is not ("actions" or "item-actions" or "result"))
        {
            int index = _index;
            _parents.Push(() => { restore(); _index = index; Present(); });
        }
        if (stage is "actions" or "item-actions" or "result") _parents.Clear();
        var saved = options.ToArray();
        _restore = () => { _restore = null; Menu(stage, title, saved); };
        _stage = stage; _titleText = title; _index = 0; _generation++; _pointer.Suspend(); _requiresRelease = true;
        _options.Clear(); _options.AddRange(saved);
        if (_options.Count == 0) _options.Add((() => GameText.Get("prepare.empty"), Back, true));
        while (_rows.Count < _options.Count)
        {
            var row = Text("Choice" + _rows.Count, 28); _rows.Add(row); _panel.AddChild(row);
        }
        for (int i = 0; i < _rows.Count; i++)
        { _rows[i].Visible = i < _options.Count; _rows[i].Position = new(32, 160 + i * 68); _rows[i].Size = new(1008, 60); }
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

    private static string EquipmentName(BattleEquipmentChoice choice) => choice.Slot == 4 ? GameText.Get("prepare.unarmed") : choice.Label;
    private void Equip() => Menu("equipment", () => GameText.Get("prepare.equipment", _name), _choices.Equipment.Select(choice =>
        ((Func<string>)(() => GameText.Get("prepare.equipment-row", EquipmentName(choice),
            choice.Equipped ? " · " + GameText.Get("party.equipped") : "", choice.Attack)), (Action)(() => Confirm(
            new FieldEquip(_member, choice.Slot, choice.Slot == 4 ? null : _choices.Items.Single(item => item.Slot == choice.Slot).Word),
            () => GameText.Get("prepare.confirm-equip", EquipmentName(choice), choice.Attack))), true)));

    private void Give() => Menu("give-item", () => GameText.Get("prepare.give-item"), _choices.Items.Select(item =>
        ((Func<string>)(() => $"{item.Slot + 1}. {item.Name}"), (Action)(() => Targets("give-target", () => GameText.Get("prepare.give-target"), true, target =>
        {
            var other = _session.QueryFieldActions(target);
            if (other.Error is { } error) { Message(() => GameText.Get("prepare.unavailable", Recovery(error), error)); return; }
            if (other.Items.Count == 4)
                Menu("exchange", () => GameText.Get("prepare.exchange"), other.Items.Select(returned =>
                    ((Func<string>)(() => $"{returned.Slot + 1}. {returned.Name}"), (Action)(() => Confirm(
                        new FieldGive(_member, item.Slot, item.Word, target, returned.Slot, returned.Word),
                        () => GameText.Get("prepare.confirm-exchange", item.Name, returned.Name))), true)));
            else Confirm(new FieldGive(_member, item.Slot, item.Word, target), () => GameText.Get("prepare.confirm-give", item.Name));
        })), true)));

    private void Use() => Menu("use-item", () => GameText.Get("prepare.use-item"), _choices.Items.Select(item =>
        ((Func<string>)(() => $"{item.Slot + 1}. {item.Name}" + (item.Use is null ? " · " + GameText.Get("party.value-unavailable") : "")), (Action)(() =>
        {
            if (item.Use is null) { Message(() => GameText.Get("prepare.use-unsupported")); return; }
            if (item.Use == FieldRecoveryEffect.NoEffect)
                Confirm(new FieldUseItem(_member, item.Slot, item.Word), () => GameText.Get("prepare.confirm-no-effect-item", item.Name));
            else Targets("use-target", () => GameText.Get("prepare.use-target"), true, target => Confirm(new FieldUseItem(_member, item.Slot, item.Word, target),
                () => GameText.Get("prepare.confirm-use", item.Name)));
        }), item.Use is not null)));

    private void Magic() => Menu("magic", () => GameText.Get("prepare.choose-magic"), _choices.Spells.Select(spell =>
        ((Func<string>)(() => GameText.Get("prepare.spell-row", spell.Name, spell.Spell.Level)), (Action)(() =>
        {
            if (spell.Effect == FieldRecoveryEffect.NoEffect)
                Confirm(new FieldCast(_member, spell.Spell), () => GameText.Get("prepare.confirm-no-effect-spell", spell.Name));
            else if (spell.Effect == FieldRecoveryEffect.Egress)
                Confirm(new FieldCast(_member, spell.Spell), () => GameText.Get("prepare.confirm-egress", spell.Name, spell.MpCost));
            else Targets("magic-target", () => GameText.Get("prepare.magic-target"), true, target => Confirm(new FieldCast(_member, spell.Spell, target),
                () => GameText.Get("prepare.confirm-cast", spell.Name, spell.MpCost)));
        }), true)));

    private void Drop() => Menu("drop-item", () => GameText.Get("prepare.drop-item"), _choices.Items.Select(item =>
        ((Func<string>)(() => $"{item.Slot + 1}. {item.Name}" + (!item.CanDrop ? " · " + GameText.Get("prepare.cannot-drop") : "")), (Action)(() =>
        {
            if (!item.CanDrop) { Message(() => GameText.Get("prepare.drop-refused")); return; }
            Confirm(new FieldDrop(_member, item.Slot, item.Word), () => GameText.Get("prepare.confirm-drop", item.Name));
        }), item.CanDrop)));

    private void Targets(string stage, Func<string> title, bool self, Action<ActorRef> select) => Menu(stage, title,
        _session.QueryPartyDetails()!.Members.Where(member => self || member.Carried.Actor != _member)
            .Select(member => ((Func<string>)(() => PartyStatusView.MemberName(member, _session)), (Action)(() => select(member.Carried.Actor)), true)));

    private void Confirm(FieldCommand command, Func<string> title) => Menu("confirm", title,
        [(() => GameText.Get("prepare.confirm"), () =>
        {
            var result = _submit(command, _choices.Revision);
            if (result.Failure is null && !result.Snapshot.CanWaitAtInput) { Hide(); return; }
            Message(result.Failure is { } failure ? () => GameText.Get("prepare.failure", Recovery(failure.Code), failure.Code, failure.Field, failure.Message) :
                result.Observations.Any(row => row.Kind == "field-no-effect") ? () => GameText.Get("prepare.no-effect") : () => GameText.Get("prepare.done"));
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

    private void Message(Func<string> message) => Menu("result", message, [(() => GameText.Get("prepare.back"), _choices.Error is not null ? Hide : Actions, true)]);
    internal void Present()
    {
        if (!Open) return;
        _title.Text = _titleText();
        _title.Size = new(1008, Math.Max(112, _title.GetMinimumSize().Y));
        float rowTop = Math.Max(160, _title.Position.Y + _title.Size.Y + 24);
        for (int i = 0; i < _options.Count; i++) _rows[i].Position = new(32, rowTop + i * 68);
        _hint.Position = new(32, Math.Max(640, rowTop + _options.Count * 68 + 24));
        _pointer.ReleaseIfNeutral(); if (!_input.GameplayHeld) _requiresRelease = false;
        for (int i = 0; i < _options.Count; i++)
        {
            _rows[i].Text = (i == _index ? "▶ " : "   ") + _options[i].Label();
            if (_options[i].Available && i != _index) _rows[i].RemoveThemeColorOverride("font_color");
            else _rows[i].AddThemeColorOverride("font_color", !_options[i].Available ? StoryTheme.Unavailable : StoryTheme.Selected);
        }
        _hint.Text = GameText.Get("prepare.hint", _input.CompactHint(GameAction.Up), _input.CompactHint(GameAction.Down),
            _input.CompactHint(GameAction.Confirm), _input.CompactHint(GameAction.Cancel));
        if (GameText.Language == "zh-CN") _hint.Text += "\n" + GameText.Get("localization.names");
        string fallback = GameText.Fallback("prepare.", true);
        if (fallback.Length > 0) _hint.Text += "\n" + fallback;
        float height = Math.Max(164, _hint.GetMinimumSize().Y);
        _hint.Size = new(1008, height); _panel.Size = new(1072, Math.Max(824, _hint.Position.Y + height + 20));
    }
    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); }
    internal void Handle(GameAction action)
    {
        _pointer.Cancel(); if (_requiresRelease) return;
        if (action is GameAction.Cancel or GameAction.Equipment) { Back(); return; }
        if (action is GameAction.Up or GameAction.Down)
            _index = (_index + (action == GameAction.Up ? -1 : 1) + _options.Count) % _options.Count;
        else if (action == GameAction.Confirm) _options[_index].Choose();
        Present();
    }
    internal void HandlePointer(InputEvent input)
    {
        if (!GetWindow().HasFocus() || !CanProcess()) { Suspend(); return; }
        if (input is InputEventMouseMotion motion) { _pointer.Moved(motion); return; }
        if (input is not InputEventMouseButton button) return;
        int hit = _rows.FindIndex(row => row.Visible && row.GetGlobalRect().HasPoint(button.Position));
        bool back = button.ButtonIndex == MouseButton.Right;
        string? clicked = _pointer.Read(button, back ? "back" : hit < 0 ? null : hit.ToString(),
            back ? GetGlobalRect() : hit < 0 ? default : _rows[hit].GetGlobalRect(), _generation);
        if (!_requiresRelease && clicked is not null)
        {
            if (clicked == "back") Back();
            else { _index = int.Parse(clicked); _options[_index].Choose(); }
        }
        Present();
    }
    internal object Observe() => new { visible = IsVisibleInTree(), stage = _stage, index = _index,
        title = _title.Text, revision = _choices?.Revision, requiresRelease = _requiresRelease,
        rows = _rows.Where(row => row.Visible).Select(row => new { text = row.Text, rect = BattleMapViewport.Rectangle(row.GetGlobalRect()) }) };
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}
