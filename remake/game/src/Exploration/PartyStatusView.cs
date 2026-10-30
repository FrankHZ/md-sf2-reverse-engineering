using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Disposable member selection/focus only; original member-screen summary + aligned details.
internal sealed partial class PartyStatusView : Control
{
    private readonly PartyDetailsPanel _details = new() { Name = "MemberDetails" };
    private readonly Label _selector = Text("MemberSelector", 28), _hint = Text("PartyHint", 24), _close = Text("Close", 26);
    private readonly Label _previous = Text("Previous", 32), _next = Text("Next", 32);
    private readonly Label _portraitNote = Text("PortraitNote", 24), _kills = Text("Kills", 30), _defeats = Text("Defeats", 30), _gold = Text("Gold", 32);
    private readonly Label _killsValue = Text("KillsValue", 30), _defeatsValue = Text("DefeatsValue", 30);
    private readonly Panel _portraitFrame = new(), _recordFrame = new(), _goldFrame = new(), _selectorFrame = new(), _rosterFrame = new();
    private readonly TextureRect _portrait = new() { Name = "MemberPortrait", ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, TextureFilter = TextureFilterEnum.Nearest, MouseFilter = MouseFilterEnum.Ignore };
    private readonly ScrollContainer _roster = new() { Name = "Roster", HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        VerticalScrollMode = ScrollContainer.ScrollMode.Auto, FocusMode = FocusModeEnum.None };
    private readonly VBoxContainer _rows = new() { Name = "Members", SizeFlagsHorizontal = SizeFlags.ExpandFill };
    private readonly MenuPointer _pointer = new();
    private readonly List<Label> _labels = [];
    private GameSession? _session;
    private GameInput? _input;
    private PartyDetails? _shown;
    private ImageTexture? _portraitTexture;
    private int? _portraitId;
    private int _index, _count;
    private bool _requiresRelease, _selectorOpen;
    internal bool Open => Visible;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Stop; Theme = StoryTheme.Create();
        var shade = new ColorRect { Color = new(0, 0, 0, 0.7f), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        foreach (var node in new Control[] { _portraitFrame, _recordFrame, _goldFrame, _selectorFrame, _details,
            _portrait, _portraitNote, _kills, _defeats, _killsValue, _defeatsValue, _gold, _selector, _hint, _close, _previous, _next, _rosterFrame, _roster }) AddChild(node);
        _roster.AddChild(_rows);
        _selector.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _selector.HorizontalAlignment = _portraitNote.HorizontalAlignment = _gold.HorizontalAlignment = HorizontalAlignment.Center;
        _portraitNote.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _kills.Text = "Kills"; _defeats.Text = "Defeats";
        _killsValue.HorizontalAlignment = _defeatsValue.HorizontalAlignment = HorizontalAlignment.Right;
        _close.Text = "Close ×"; _previous.Text = "◀"; _next.Text = "▶";
        Hide();
    }

    public override void _ExitTree() { _portrait.Texture = null; _portraitTexture?.Dispose(); }
    internal void Begin(GameSession session, GameInput input)
    {
        _session = session; _input = input; _index = 0; _details.FocusPane = 0; _selectorOpen = false;
        _requiresRelease = true; _pointer.Suspend(); _roster.ScrollVertical = 0; _details.ResetReading();
        Show(); Present(session, input);
    }

    internal void Present(GameSession session, GameInput input)
    {
        if (!Open) return;
        _session = session; _input = input; _pointer.ReleaseIfNeutral();
        if (!input.GameplayHeld) _requiresRelease = false;
        if (!session.Current.CanWaitAtInput || session.QueryPartyDetails() is not { } party) { Hide(); return; }
        _shown = party; _count = party.Members.Count; _index = Math.Clamp(_index, 0, Math.Max(0, _count - 1));
        while (_labels.Count < _count)
        {
            var label = Text("Member" + _labels.Count, 28);
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart; label.CustomMinimumSize = new(0, 72);
            label.SizeFlagsHorizontal = SizeFlags.ExpandFill; _labels.Add(label); _rows.AddChild(label);
        }
        for (int i = 0; i < _labels.Count; i++)
        {
            _labels[i].Visible = i < _count;
            if (i >= _count) continue;
            var row = party.Members[i];
            _labels[i].Text = (i == _index ? "▸ " : "   ") + MemberName(row, session) +
                (row.Carried.Absent ? " · Absent" : row.Carried.Hp == 0 ? " · Defeated" : "");
            _labels[i].Modulate = i == _index ? StoryTheme.Selected : Colors.White;
        }
        var member = _count == 0 ? null : party.Members[_index];
        string name = member is null ? "Party information unavailable" : MemberName(member, session);
        _selector.Text = $"{(_count == 0 ? 0 : _index + 1)} / {_count}    {name}   ▾";
        _details.Present(member, name);
        _killsValue.Text = member?.Carried.Kills?.ToString() ?? "Unavailable";
        _defeatsValue.Text = member?.Carried.Defeats?.ToString() ?? "Unavailable";
        _killsValue.AddThemeFontSizeOverride("font_size", member?.Carried.Kills is null ? 24 : 30);
        _defeatsValue.AddThemeFontSizeOverride("font_size", member?.Carried.Defeats is null ? 24 : 30);
        _gold.Text = $"GOLD\n{party.Gold?.ToString() ?? "Unavailable"}";
        PresentPortrait(session, member);
        string focus = _details.FocusPane switch { 1 => "Magic", 2 => "Items", _ => "Member" };
        _hint.Text = $"Read only · {input.Hint(GameAction.Up)} / {input.Hint(GameAction.Down)}: {focus}   " +
            $"{input.Hint(GameAction.Target)}: Focus   {input.Hint(GameAction.Cancel)} / {input.Hint(GameAction.Equipment)}: Close\n" +
            "Mouse: member selector / arrows · Wheel: scroll the open list";
        Arrange();
    }

    private static string MemberName(PartyMemberDetails member, GameSession session)
    {
        int? binding = member.Deployment?.Initialization?.AllyPartyMember;
        var names = session.Definition.Exploration!.MemberNames;
        if (member.Deployment is null || session.Definition.PrivateDefinitions is not null &&
            (binding is null || binding < 0 || binding >= names.Count || string.IsNullOrWhiteSpace(names[binding.Value]))) return "Name unavailable";
        return BattleCharacterNames.Party(member.Carried.Actor, binding, names);
    }

    private void PresentPortrait(GameSession session, PartyMemberDetails? member)
    {
        var visuals = session.Definition.Exploration?.Visuals;
        var appearance = session.Current.Exploration?.Population?.AllySprites?.FirstOrDefault(row =>
            row.Character == member?.Deployment?.Initialization?.AllyPartyMember);
        int? sprite = appearance is null ? null : appearance.JoinedFlag is { } joined && !session.Current.Story.Flags.Contains(joined)
            ? appearance.UnjoinedSprite : appearance.Sprite;
        int? portrait = sprite is { } id ? visuals?.Sprites.GetValueOrDefault(id)?.Portrait : null;
        if (portrait != _portraitId)
        {
            _portrait.Texture = null; _portraitTexture?.Dispose(); _portraitTexture = null; _portraitId = portrait;
            if (portrait is { } value && visuals?.Portraits.TryGetValue(value, out var visual) == true)
                _portrait.Texture = _portraitTexture = ExplorationRasterImages.Portrait(visual);
        }
        _portrait.Visible = _portraitTexture is not null;
        _portraitNote.Visible = !_portrait.Visible; _portraitNote.Text = "Portrait unavailable";
    }

    private void Arrange()
    {
        _selectorFrame.Position = new(168, 36); _selectorFrame.Size = new(1416, 80);
        _previous.Position = new(188, 54); _previous.Size = new(56, 48);
        _next.Position = new(1512, 54); _next.Size = new(56, 48);
        _selector.Position = new(256, 42); _selector.Size = new(1232, 72);
        _selector.Modulate = _details.FocusPane == 0 ? StoryTheme.Selected : Colors.White;
        _close.Position = new(1616, 56); _close.Size = new(136, 48); _close.Modulate = StoryTheme.Selected;
        _portraitFrame.Position = new(168, 144); _portraitFrame.Size = new(336, 352);
        _portrait.Position = new(240, 192); _portrait.Size = new(192, 256);
        _portraitNote.Position = new(192, 280); _portraitNote.Size = new(288, 80);
        _recordFrame.Position = new(168, 520); _recordFrame.Size = new(336, 224);
        _kills.Position = new(192, 562); _kills.Size = new(128, 52);
        _killsValue.Position = new(328, 562); _killsValue.Size = new(152, 52);
        _defeats.Position = new(192, 646); _defeats.Size = new(128, 52);
        _defeatsValue.Position = new(328, 646); _defeatsValue.Size = new(152, 52);
        _goldFrame.Position = new(168, 768); _goldFrame.Size = new(336, 192);
        _gold.Position = new(192, 808); _gold.Size = new(288, 108);
        _details.Position = new(528, 144); _details.Size = new(1224, 816); _details.Arrange();
        _hint.Position = new(168, 980); _hint.Size = new(1584, 72);
        _rosterFrame.Visible = _roster.Visible = _selectorOpen;
        _rosterFrame.Position = new(256, 124); _rosterFrame.Size = new(1232, 680);
        _roster.Position = new(276, 144); _roster.Size = new(1192, 640);
    }

    private void Select(int index)
    { if (_count == 0) return; _index = (index + _count) % _count; _details.ResetReading(); if (_selectorOpen) _roster.EnsureControlVisible(_labels[_index]); }
    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); }
    internal void Handle(GameAction action, GameSession session, GameInput input)
    {
        _pointer.Cancel(); if (_requiresRelease) return;
        if (action is GameAction.Cancel or GameAction.Equipment or GameAction.Confirm) { Hide(); return; }
        if (action == GameAction.Target) { _details.FocusPane = (_details.FocusPane + 1) % 3; _selectorOpen = false; }
        int step = action is GameAction.Left or GameAction.Up ? -1 : action is GameAction.Right or GameAction.Down ? 1 : 0;
        if (_details.FocusPane != 0 && action is GameAction.Up or GameAction.Down) _details.ScrollFocused(step * 112);
        else if (step != 0) Select(_index + step);
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
            int step = button.ButtonIndex == MouseButton.WheelUp ? -112 : 112;
            if (button.Pressed && !_requiresRelease)
            { if (_selectorOpen && _roster.GetGlobalRect().HasPoint(button.Position)) _roster.ScrollVertical += step;
              else if (!_selectorOpen) _details.Wheel(button.Position, step); }
            _pointer.Cancel(); return;
        }
        string? id = null; Rect2 rect = default;
        if (button.ButtonIndex == MouseButton.Left)
        {
            foreach (var (key, control) in new[] { ("close", _close), ("previous", _previous), ("next", _next), ("selector", _selector) })
                if (control.GetGlobalRect().HasPoint(button.Position)) { id = key; rect = control.GetGlobalRect(); break; }
            if (_selectorOpen && _roster.GetGlobalRect().HasPoint(button.Position))
                for (int i = 0; i < _count; i++)
                    if (_labels[i].GetGlobalRect().HasPoint(button.Position)) { id = i.ToString(); rect = _labels[i].GetGlobalRect(); break; }
        }
        string? clicked = _pointer.Read(button, id, rect, (_index, _selectorOpen, _roster.ScrollVertical));
        if (_requiresRelease || clicked is null) return;
        if (clicked == "close") { Hide(); return; }
        if (clicked == "selector") { _selectorOpen = !_selectorOpen; _details.FocusPane = 0; }
        else if (clicked is "previous" or "next") Select(_index + (clicked == "previous" ? -1 : 1));
        else { Select(int.Parse(clicked)); _selectorOpen = false; _details.FocusPane = 0; }
        Present(_session!, _input!);
    }

    internal object Observe() => new { visible = IsVisibleInTree(), index = _index, count = _count, requiresRelease = _requiresRelease,
        header = _selector.Text, hint = _hint.Text, selectorOpen = _selectorOpen, focusPane = _details.FocusPane,
        card = _details.Card(), layout = _details.Observe(), portraitId = _portraitId, portraitVisible = _portrait.IsVisibleInTree(),
        portraitRect = Rect(_portrait), recordRect = Rect(_recordFrame), goldRect = Rect(_goldFrame), kills = _killsValue.Text, defeats = _defeatsValue.Text, gold = _gold.Text,
        recordValues = new[] { _killsValue, _defeatsValue }.Select(label => new { text = label.Text,
            rect = Rect(label), minimumWidth = label.GetMinimumSize().X, fontSize = label.GetThemeFontSize("font_size") }),
        selectorRect = Rect(_selector), previousRect = Rect(_previous), nextRect = Rect(_next), closeRect = Rect(_close),
        rosterRect = Rect(_roster), rosterScroll = _roster.ScrollVertical,
        projection = _shown?.Members.Select(member => new { actor = member.Carried.Actor.Value, hp = member.Carried.Hp, mp = member.Carried.Mp,
            exp = member.Carried.Exp, member.Carried.Kills, member.Carried.Defeats, absent = member.Carried.Absent,
            member.MaxHp, member.MaxMp, member.Level, member.Attack, member.Defense, member.Agility, member.Spells, member.Inventory }),
        rows = _labels.Take(_count).Select(label => new { text = label.Text, rect = Rect(label) }) };
    private static object Rect(Control control) => BattleMapViewport.Rectangle(control.GetGlobalRect());
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}
