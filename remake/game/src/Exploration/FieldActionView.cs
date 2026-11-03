using Godot;
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
    private readonly List<(string Label, Action Choose)> _options = [];
    private readonly MenuPointer _pointer = new();
    private GameSession _session = null!;
    private GameInput _input = null!;
    private Func<FieldCommand, long, SessionResult> _submit = null!;
    private ActorRef _member;
    private FieldActionChoices _choices = null!;
    private string _stage = "actions", _name = "";
    private int _index;
    private long _generation;
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
        _panel.AddChild(_hint); _hint.Position = new(32, 736); _hint.Size = new(1008, 72);
        _hint.AutowrapMode = TextServer.AutowrapMode.WordSmart; _hint.CustomMaximumSize = new(1008, -1);
        Hide();
    }

    internal void Begin(GameSession session, GameInput input, ActorRef member, string name, Func<FieldCommand, long, SessionResult> submit)
    {
        _session = session; _input = input; _member = member; _name = name; _submit = submit;
        Show(); Actions();
    }

    private void Menu(string stage, string title, IEnumerable<(string Label, Action Choose)> options)
    {
        _stage = stage; _title.Text = title; _index = 0; _generation++; _pointer.Suspend(); _requiresRelease = true;
        _options.Clear(); _options.AddRange(options);
        if (_options.Count == 0) _options.Add(("Nothing available · Back", Actions));
        while (_rows.Count < _options.Count)
        {
            var row = Text("Choice" + _rows.Count, 28); _rows.Add(row); _panel.AddChild(row);
        }
        for (int i = 0; i < _rows.Count; i++)
        { _rows[i].Visible = i < _options.Count; _rows[i].Position = new(32, 160 + i * 68); _rows[i].Size = new(1008, 60); }
        Present();
    }

    private void Actions()
    {
        _choices = _session.QueryFieldActions(_member);
        if (_choices.Error is { } error) { Message("Preparation unavailable: " + error.Replace('-', ' ')); return; }
        Menu("actions", _name + " · Prepare", [
            ("Equip", Equip), ("Give / exchange", Give), ("Use item", Use), ("Magic", Magic), ("Drop", Drop), ("Back", Hide)]);
    }

    private void Equip() => Menu("equipment", _name + " · Equip", _choices.Equipment.Select(choice =>
        ($"{choice.Label}{(choice.Equipped ? " · Equipped" : "")}    ATT {choice.Attack}", (Action)(() => Confirm(
            new FieldEquip(_member, choice.Slot, choice.Slot == 4 ? null : _choices.Items.Single(item => item.Slot == choice.Slot).Word),
            $"Equip {choice.Label}? Attack will be {choice.Attack}.")))));

    private void Give() => Menu("give-item", "Give which item?", _choices.Items.Select(item =>
        ($"{item.Slot + 1}. {item.Name}", (Action)(() => Targets("give-target", "Give to whom?", true, target =>
        {
            var other = _session.QueryFieldActions(target);
            if (other.Error is { } error) { Message(error.Replace('-', ' ')); return; }
            if (other.Items.Count == 4)
                Menu("exchange", "Full pack · Choose an item in return", other.Items.Select(returned =>
                    ($"{returned.Slot + 1}. {returned.Name}", (Action)(() => Confirm(
                        new FieldGive(_member, item.Slot, item.Word, target, returned.Slot, returned.Word),
                        $"Exchange {item.Name} for {returned.Name}? Moved weapons become unequipped.")))));
            else Confirm(new FieldGive(_member, item.Slot, item.Word, target), $"Give {item.Name}? The moved item becomes unequipped.");
        })))));

    private void Use() => Menu("use-item", "Use which item?", _choices.Items.Select(item =>
        ($"{item.Slot + 1}. {item.Name}{(item.Use is null ? " · Unavailable" : "")}", (Action)(() =>
        {
            if (item.Use is null) { Message("This item's field effect is not supported yet."); return; }
            if (item.Use == FieldRecoveryEffect.NoEffect)
                Confirm(new FieldUseItem(_member, item.Slot, item.Word), $"Use {item.Name}? This item has no effect here and will be kept.");
            else Targets("use-target", "Use on whom?", true, target => Confirm(new FieldUseItem(_member, item.Slot, item.Word, target),
                $"Use {item.Name}? One item is consumed, including when there is no poison to cure."));
        }))));

    private void Magic() => Menu("magic", "Choose magic", _choices.Spells.Select(spell =>
        ($"{spell.Name} · Level {spell.Spell.Level}", (Action)(() =>
        {
            if (spell.Effect == FieldRecoveryEffect.NoEffect)
                Confirm(new FieldCast(_member, spell.Spell), $"Cast {spell.Name}? Nothing happens here; MP is kept.");
            else Targets("magic-target", "Cast on whom?", true, target => Confirm(new FieldCast(_member, spell.Spell, target),
                $"Cast {spell.Name}? Cost: {spell.MpCost} MP, including when there is no poison to cure."));
        }))));

    private void Drop() => Menu("drop-item", "Drop which item?", _choices.Items.Select(item =>
        ($"{item.Slot + 1}. {item.Name}{(!item.CanDrop ? " · Cannot drop" : "")}", (Action)(() =>
        {
            if (!item.CanDrop) { Message("This item cannot be dropped."); return; }
            Confirm(new FieldDrop(_member, item.Slot, item.Word), $"Drop {item.Name}? This removes it from the pack.");
        }))));

    private void Targets(string stage, string title, bool self, Action<ActorRef> select) => Menu(stage, title,
        _session.QueryPartyDetails()!.Members.Where(member => self || member.Carried.Actor != _member)
            .Select(member => (PartyStatusView.MemberName(member, _session), (Action)(() => select(member.Carried.Actor)))));

    private void Confirm(FieldCommand command, string title) => Menu("confirm", title,
        [("Confirm", () =>
        {
            var result = _submit(command, _choices.Revision);
            Message(result.Failure is { } failure ? "Could not complete: " + failure.Message :
                result.Observations.Any(row => row.Kind == "field-no-effect") ? "But nothing happened." : "Done.");
        }), ("Cancel", Actions)]);

    private void Message(string message) => Menu("result", message, [("Back", Actions)]);
    internal void Present()
    {
        if (!Open) return;
        _pointer.ReleaseIfNeutral(); if (!_input.GameplayHeld) _requiresRelease = false;
        for (int i = 0; i < _options.Count; i++)
        { _rows[i].Text = (i == _index ? "▸ " : "   ") + _options[i].Label; _rows[i].Modulate = i == _index ? StoryTheme.Selected : Colors.White; }
        _hint.Text = $"{_input.CompactHint(GameAction.Up)} / {_input.CompactHint(GameAction.Down)}: Choose   " +
            $"{_input.CompactHint(GameAction.Confirm)}: Select   {_input.CompactHint(GameAction.Cancel)}: Back\nMouse: select a row";
    }
    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); }
    internal void Handle(GameAction action)
    {
        _pointer.Cancel(); if (_requiresRelease) return;
        if (action is GameAction.Cancel or GameAction.Equipment) { if (_stage == "actions") Hide(); else Actions(); return; }
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
        string? clicked = _pointer.Read(button, hit < 0 ? null : hit.ToString(), hit < 0 ? default : _rows[hit].GetGlobalRect(), _generation);
        if (!_requiresRelease && clicked is not null) { _index = int.Parse(clicked); _options[_index].Choose(); }
        Present();
    }
    internal object Observe() => new { visible = IsVisibleInTree(), stage = _stage, index = _index,
        title = _title.Text, revision = _choices?.Revision, requiresRelease = _requiresRelease,
        rows = _rows.Where(row => row.Visible).Select(row => new { text = row.Text, rect = BattleMapViewport.Rectangle(row.GetGlobalRect()) }) };
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}
