using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Disposable selection, focus and scrolling only. The query reads current carried state.
internal sealed partial class PartyStatusView : Control
{
    private readonly BattleStatusCard _card = new() { Name = "MemberCard" };
    private readonly Label _header = Text("PartyHeader", 30), _hint = Text("PartyHint", 24);
    private readonly Label _body = Text("Details", 30), _close = Text("Close", 26);
    private readonly Panel _rosterFrame = new(), _detailsFrame = new();
    private readonly ScrollContainer _roster = Scroll("Roster"), _details = Scroll("DetailsScroll");
    private readonly VBoxContainer _rows = new() { Name = "Members", SizeFlagsHorizontal = SizeFlags.ExpandFill };
    private readonly MenuPointer _pointer = new();
    private readonly List<Label> _labels = [];
    private GameSession? _session;
    private GameInput? _input;
    private PartyDetails? _shown;
    private int _index;
    private bool _requiresRelease, _detailFocus;
    private int _count;
    internal bool Open => Visible;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Stop;
        Theme = StoryTheme.Create();
        var shade = new ColorRect { Color = new(0, 0, 0, 0.7f), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        foreach (var node in new Control[] { _header, _rosterFrame, _detailsFrame, _roster, _card, _details, _hint, _close }) AddChild(node);
        _roster.AddChild(_rows); _details.AddChild(_body);
        _body.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _body.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _body.MouseFilter = MouseFilterEnum.Pass;
        _close.Text = "Close ×";
        _close.HorizontalAlignment = HorizontalAlignment.Center;
        Hide();
    }

    internal void Begin(GameSession session, GameInput input)
    {
        _session = session; _input = input; _index = 0; _detailFocus = false;
        _requiresRelease = true; _pointer.Suspend(); _roster.ScrollVertical = 0; _details.ScrollVertical = 0;
        Show(); Present(session, input);
    }

    internal void Present(GameSession session, GameInput input)
    {
        if (!Open) return;
        _session = session; _input = input;
        _pointer.ReleaseIfNeutral();
        if (!input.GameplayHeld) _requiresRelease = false;
        if (!session.Current.CanWaitAtInput || session.QueryPartyDetails() is not { } party) { Hide(); return; }
        _shown = party; _count = party.Members.Count;
        _index = Math.Clamp(_index, 0, Math.Max(0, _count - 1));
        while (_labels.Count < _count)
        {
            var label = Text("Member" + _labels.Count, 28);
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            label.CustomMinimumSize = new(0, 88); label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _labels.Add(label); _rows.AddChild(label);
        }
        for (int i = 0; i < _labels.Count; i++)
        {
            _labels[i].Visible = i < _count;
            if (i >= _count) continue;
            var member = party.Members[i];
            _labels[i].Text = (i == _index ? "▸ " : "   ") + MemberName(member, session) +
                (member.Carried.Absent ? " · Absent" : member.Carried.Hp == 0 ? " · Defeated" : "");
            _labels[i].Modulate = i == _index ? StoryTheme.Selected : Colors.White;
        }
        _header.Text = $"Party · Read only     Gold: {party.Gold?.ToString() ?? "Unavailable"}     {(_count == 0 ? 0 : _index + 1)}/{_count}";
        _card.Hide();
        if (_count > 0)
        {
            var member = party.Members[_index];
            if (member.MaxHp is { } maxHp && member.MaxMp is { } maxMp)
                _card.Present(member.Carried.Actor, MemberName(member, session), member.Carried.Absent ? "Absent member" : "Party member",
                    member.Carried.Hp, maxHp, member.Carried.Mp, maxMp);
            _body.Text = PartyDetailsText.Read(member, MemberName(member, session));
        }
        else _body.Text = "Party information unavailable: no carried members.";
        _hint.Text = $"{input.Hint(GameAction.Up)} / {input.Hint(GameAction.Down)}: {(_detailFocus ? "Scroll details" : "Member")}   " +
            $"{input.Hint(GameAction.Target)}: {(_detailFocus ? "Roster" : "Details")}   " +
            $"{input.Hint(GameAction.Cancel)} / {input.Hint(GameAction.Equipment)}: Close · Mouse: select / wheel to scroll";
        Arrange();
    }

    private static string MemberName(PartyMemberDetails member, GameSession session)
    {
        int? binding = member.Deployment?.Initialization?.AllyPartyMember;
        var names = session.Definition.Exploration!.MemberNames;
        if (member.Deployment is null || session.Definition.PrivateDefinitions is not null &&
            (binding is null || binding < 0 || binding >= names.Count || string.IsNullOrWhiteSpace(names[binding.Value])))
            return "Name unavailable";
        return BattleCharacterNames.Party(member.Carried.Actor, binding, names);
    }

    private void Arrange()
    {
        var size = GetViewportRect().Size;
        const float left = 96, top = 120, rosterWidth = 520, gap = 32;
        float right = left + rosterWidth + gap, width = size.X - right - left;
        _header.Position = new(left, 48); _header.Size = new(size.X - 420, 48);
        _close.Position = new(size.X - 280, 44); _close.Size = new(184, 56);
        _close.Modulate = StoryTheme.Selected;
        _rosterFrame.Position = new(left, top); _rosterFrame.Size = new(rosterWidth, size.Y - top - 108);
        _roster.Position = new(left + 20, top + 16); _roster.Size = _rosterFrame.Size - new Vector2(40, 32);
        _detailsFrame.Position = new(right, top); _detailsFrame.Size = new(width, size.Y - top - 108);
        _card.Position = new(right + 20, top + 16);
        _details.Position = new(right + 20, top + 224); _details.Size = new(width - 40, size.Y - top - 348);
        _hint.Position = new(left, size.Y - 84); _hint.Size = new(size.X - left * 2, 64);
        _hint.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _rosterFrame.Modulate = _detailFocus ? Colors.White : StoryTheme.Selected;
        _detailsFrame.Modulate = _detailFocus ? StoryTheme.Selected : Colors.White;
    }

    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); }
    internal void Handle(GameAction action, GameSession session, GameInput input)
    {
        _pointer.Cancel();
        if (_requiresRelease) return;
        if (action is GameAction.Cancel or GameAction.Equipment or GameAction.Confirm) { Hide(); return; }
        if (action == GameAction.Target) _detailFocus = !_detailFocus;
        int step = action is GameAction.Left or GameAction.Up ? -1 : action is GameAction.Right or GameAction.Down ? 1 : 0;
        if (_detailFocus && action is GameAction.Up or GameAction.Down) _details.ScrollVertical += step * 112;
        else if (_count > 0 && step != 0)
        {
            _index = (_index + step + _count) % _count;
            _details.ScrollVertical = 0; _roster.EnsureControlVisible(_labels[_index]);
        }
        Present(session, input);
    }

    internal void HandlePointer(InputEvent @event)
    {
        if (!Open || @event is not (InputEventMouseButton or InputEventMouseMotion)) return;
        if (!GetWindow().HasFocus() || !CanProcess() || !IsVisibleInTree()) { Suspend(); return; }
        if (@event is InputEventMouseMotion motion) { _pointer.Moved(motion); return; }
        var button = (InputEventMouseButton)@event;
        if (button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            var scroll = _roster.GetGlobalRect().HasPoint(button.Position) ? _roster :
                _details.GetGlobalRect().HasPoint(button.Position) ? _details : null;
            if (button.Pressed && scroll is not null) scroll.ScrollVertical += button.ButtonIndex == MouseButton.WheelUp ? -112 : 112;
            _pointer.Cancel(); GetViewport().SetInputAsHandled(); return;
        }
        string? id = null; Rect2 rect = default;
        if (button.ButtonIndex == MouseButton.Left && _close.GetGlobalRect().HasPoint(button.Position)) { id = "close"; rect = _close.GetGlobalRect(); }
        else if (button.ButtonIndex == MouseButton.Left && _roster.GetGlobalRect().HasPoint(button.Position))
            for (int i = 0; i < _count; i++)
                if (_labels[i].GetGlobalRect().HasPoint(button.Position)) { id = i.ToString(); rect = _labels[i].GetGlobalRect(); break; }
        string? clicked = _pointer.Read(button, id, rect, (_index, _roster.ScrollVertical));
        if (!_requiresRelease && clicked is not null)
        {
            if (clicked == "close") Hide();
            else { _index = int.Parse(clicked); _detailFocus = false; _details.ScrollVertical = 0; Present(_session!, _input!); }
        }
        GetViewport().SetInputAsHandled();
    }

    internal object Observe() => new { visible = IsVisibleInTree(), index = _index, count = _count, requiresRelease = _requiresRelease,
        header = _header.Text, hint = _hint.Text, unavailable = _count == 0 ? _body.Text : null, card = _card.Observe(),
        details = _body.Text, detailFocus = _detailFocus, projection = _shown?.Members.Select(member => new
        { actor = member.Carried.Actor.Value, hp = member.Carried.Hp, mp = member.Carried.Mp, exp = member.Carried.Exp,
            absent = member.Carried.Absent, member.MaxHp, member.MaxMp, member.Level, member.Attack, member.Defense, member.Agility,
            member.Spells, member.Inventory }),
        rosterRect = BattleMapViewport.Rectangle(_roster.GetGlobalRect()), detailRect = BattleMapViewport.Rectangle(_details.GetGlobalRect()),
        closeRect = BattleMapViewport.Rectangle(_close.GetGlobalRect()), rosterScroll = _roster.ScrollVertical, detailScroll = _details.ScrollVertical,
        detailHeight = _body.Size.Y, rows = _labels.Take(_count).Select(label => new { text = label.Text, rect = BattleMapViewport.Rectangle(label.GetGlobalRect()) }) };

    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
    private static ScrollContainer Scroll(string name) => new() { Name = name, HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        VerticalScrollMode = ScrollContainer.ScrollMode.Auto, FocusMode = FocusModeEnum.None };
}
