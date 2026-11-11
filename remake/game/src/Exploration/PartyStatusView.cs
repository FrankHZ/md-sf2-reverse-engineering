using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Disposable member selection/focus only; original member-screen summary + aligned details.
internal sealed partial class PartyStatusView : Control
{
    private readonly PartyDetailsPanel _details = new() { Name = "MemberDetails", FieldLayout = true };
    private readonly Control _memberLayer = new() { Name = "MemberLayer" };
    private readonly Label _selector = Text("MemberSelector", 28), _hint = Text("PartyHint", 22), _close = Text("Close", 26);
    private readonly Label _previous = Text("Previous", 32), _next = Text("Next", 32);
    private readonly Label _portraitNote = Text("PortraitNote", 24), _kills = Text("Kills", 30), _defeats = Text("Defeats", 30), _gold = Text("Gold", 32);
    private readonly Label _killsValue = Text("KillsValue", 30), _defeatsValue = Text("DefeatsValue", 30);
    private readonly Panel _recordFrame = new(), _goldFrame = new(), _rosterFrame = new();
    private readonly TextureRect _portrait = new() { Name = "MemberPortrait", ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, TextureFilter = TextureFilterEnum.Nearest, MouseFilter = MouseFilterEnum.Ignore };
    private readonly ScrollContainer _roster = new() { Name = "Roster", HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        VerticalScrollMode = ScrollContainer.ScrollMode.Auto, FocusMode = FocusModeEnum.None };
    private readonly VBoxContainer _rows = new() { Name = "Members", SizeFlagsHorizontal = SizeFlags.ExpandFill };
    private readonly MenuPointer _pointer = new();
    private readonly Label _prepare = Text("Prepare", 28);
    private readonly FieldActionView _actions = new() { Name = "FieldActions" };
    private readonly IdentityIcons _icons = new();
    private object? _iconSource;
    private SessionFailure? _assetFailure;
    internal Func<Sf2.Remake.Application.Runtime.Exploration.FieldCommand, long, SessionResult>? SubmitField { get; set; }
    private readonly List<Label> _labels = [];
    private GameSession? _session;
    private GameInput? _input;
    private PartyDetails? _shown;
    private ImageTexture? _portraitTexture;
    private int? _portraitId;
    private int _index, _count;
    private bool _requiresRelease;
    internal bool Open => Visible;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Stop; Theme = StoryTheme.Create();
        var shade = new ColorRect { Color = new(0, 0, 0, 0.7f), MouseFilter = MouseFilterEnum.Ignore };
        _memberLayer.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(_memberLayer);
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _memberLayer.AddChild(shade);
        foreach (var node in new Control[] { _recordFrame, _goldFrame, _details,
            _portrait, _portraitNote, _kills, _defeats, _killsValue, _defeatsValue, _gold, _selector, _hint, _close, _previous, _next, _rosterFrame, _roster }) _memberLayer.AddChild(node);
        _roster.AddChild(_rows);
        _memberLayer.AddChild(_prepare); AddChild(_actions);
        _selector.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _selector.HorizontalAlignment = _portraitNote.HorizontalAlignment = _gold.HorizontalAlignment = HorizontalAlignment.Center;
        _portraitNote.AutowrapMode = TextServer.AutowrapMode.WordSmart;

        _killsValue.HorizontalAlignment = _defeatsValue.HorizontalAlignment = HorizontalAlignment.Right;
        _previous.Text = "◀"; _next.Text = "▶";
        Hide();
    }

    public override void _ExitTree() { _portrait.Texture = null; _portraitTexture?.Dispose(); _icons.Dispose(); _iconSource = null; }
    private string _entry = "actions";
    internal void Begin(GameSession session, GameInput input, string entry = "actions")
    {
        _entry = entry;
        if (!ReferenceEquals(_iconSource, session.Definition))
        {
            _iconSource = session.Definition; _assetFailure = null;
            try { _icons.Configure(session.Definition.Exploration?.Visuals?.IdentityIcons); }
            catch (InvalidOperationException error)
            { _assetFailure = new(SessionFailureKind.AdapterError, error.Message, "presentation.identityIcons", "Member identity icon decoding failed."); }
        }
        _details.ConfigureIcons(_icons, session.Definition);
        _session = session; _input = input; _index = 0; _details.FocusPane = entry == "magic" ? 1 : entry == "item" ? 2 : 0;
        _requiresRelease = true; _pointer.Suspend(); _roster.ScrollVertical = 0; _details.ResetReading();
        _actions.Hide();
        Show(); Present(session, input);
    }

    internal void Present(GameSession session, GameInput input)
    {
        if (!Open) return;
        _memberLayer.Visible = !_actions.Open;
        if (_actions.Open) { _actions.Present(); return; }
        _session = session; _input = input; _pointer.ReleaseIfNeutral();
        if (!input.GameplayHeld) _requiresRelease = false;
        if (!session.Current.CanWaitAtInput || session.QueryPartyDetails() is not { } party) { Hide(); return; }
        _prepare.Text = GameText.Get(_entry == "magic" ? "prepare.magic" : _entry == "item" ? "field-menu.item" : "party.prepare"); _close.Text = GameText.Get("party.close");
        _kills.Text = GameText.Get("party.kills"); _defeats.Text = GameText.Get("party.defeats");
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
            _labels[i].Text = (i == _index && _details.FocusPane == 0 ? "› " : "   ") + MemberName(row, session) +
                (row.Carried.Absent ? " · " + GameText.Get("party.absent") : row.Carried.Hp == 0 ? " · " + GameText.Get("party.defeated") : "");
            if (i == _index && _details.FocusPane == 0) _labels[i].AddThemeColorOverride("font_color", StoryTheme.Selected);
            else _labels[i].RemoveThemeColorOverride("font_color");
        }
        var member = _count == 0 ? null : party.Members[_index];
        string name = member is null ? GameText.Get("party.unavailable") : MemberName(member, session);
        _selector.Text = GameText.Get("party.member") + $"   {(_count == 0 ? 0 : _index + 1)} / {_count}";
        _details.Present(member, name);
        _killsValue.Text = member?.Carried.Kills?.ToString() ?? GameText.Get("party.value-unavailable");
        _defeatsValue.Text = member?.Carried.Defeats?.ToString() ?? GameText.Get("party.value-unavailable");
        _killsValue.AddThemeFontSizeOverride("font_size", member?.Carried.Kills is null ? 24 : 30);
        _defeatsValue.AddThemeFontSizeOverride("font_size", member?.Carried.Defeats is null ? 24 : 30);
        _gold.Text = GameText.Get("party.gold", party.Gold?.ToString() ?? GameText.Get("party.value-unavailable"));
        PresentPortrait(session, member);
        string focus = GameText.Get(_details.FocusPane switch { 1 => "party.magic", 2 => "party.items", _ => "party.member" });
        _hint.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _hint.CustomMaximumSize = new(1584, -1);
        _hint.Text = GameText.Get("party.hint", input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Up),
            input.CompactHint(GameAction.Down), focus, input.CompactHint(GameAction.Target),
            input.CompactHint(GameAction.Cancel), input.CompactHint(GameAction.Equipment));
        if (GameText.Language == "zh-CN") _hint.Text += "\n" + GameText.Get("localization.names");
        string fallback = GameText.Fallback("party.", true);
        if (fallback.Length > 0) _hint.Text += " · " + fallback;
        if (_assetFailure is { } failure) _hint.Text += "\n" + GameText.Get("prepare.failure",
            GameText.Get("prepare.recovery.unsupported"), failure.Code, failure.Field, failure.Message);
        Arrange();
    }

    internal static string MemberName(PartyMemberDetails member, GameSession session)
    {
        int? binding = member.Deployment?.Initialization?.AllyPartyMember;
        if (member.Deployment is null)
            return CharacterNames.Actor(session.Current.Story, member.Carried.Actor, GameText.Get("party.name-unavailable"));
        return binding is { } index ? DisplayNames.Member(session.Definition, session.Current.Story, index)
            : CharacterNames.Actor(session.Current.Story, member.Carried.Actor, session.Definition.PrivateDefinitions is null
                ? CharacterNames.ReadableIdentity(member.Carried.Actor) : GameText.Get("party.name-unavailable"));
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
        _portraitNote.Visible = !_portrait.Visible; _portraitNote.Text = GameText.Get("party.portrait-unavailable");
    }

    private void Arrange()
    {
        float left = (Size.X - 1712) / 2, top = Math.Max(112, (Size.Y - 856) / 2);
        _selector.Position = new(left, top - 64); _selector.Size = new(304, 48);
        _selector.HorizontalAlignment = HorizontalAlignment.Left;
        _previous.Position = new(left + 16, top + 664); _previous.Size = new(64, 48);
        _next.Position = new(left + 224, top + 664); _next.Size = new(64, 48);
        _rosterFrame.Show(); _roster.Show();
        _rosterFrame.Position = new(left, top); _rosterFrame.Size = new(304, 720);
        _roster.Position = new(left + 16, top + 16); _roster.Size = new(272, 624);
        _details.Position = new(left + 328, top); _details.Size = new(1384, 720); _details.Arrange();
        _portrait.Position = new(left + 360, top + 24); _portrait.Size = new(144, 192);
        _portraitNote.Position = new(left + 352, top + 40); _portraitNote.Size = new(160, 160);
        _recordFrame.Position = new(left, top + 744); _recordFrame.Size = new(640, 72);
        _kills.Position = new(left + 16, top + 756); _kills.Size = new(128, 48);
        _killsValue.Position = new(left + 144, top + 756); _killsValue.Size = new(136, 48);
        _defeats.Position = new(left + 328, top + 756); _defeats.Size = new(128, 48);
        _defeatsValue.Position = new(left + 456, top + 756); _defeatsValue.Size = new(160, 48);
        _goldFrame.Position = new(left + 664, top + 744); _goldFrame.Size = new(400, 72);
        _gold.Position = new(left + 680, top + 752); _gold.Size = new(368, 56);
        _prepare.Position = new(left + 1120, top + 752); _prepare.Size = new(264, 64);
        _close.Position = new(left + 1432, top + 752); _close.Size = new(264, 64);
        _hint.Position = new(left, top + 832); _hint.Size = new(1712, 0);
        _prepare.RemoveThemeColorOverride("font_color"); _close.RemoveThemeColorOverride("font_color");
    }

    private void Select(int index)
    { if (_count == 0) return; _index = (index + _count) % _count; _details.ResetReading(); _roster.EnsureControlVisible(_labels[_index]); }
    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); _actions.Suspend(); }
    private void Prepare()
    {
        if (_shown is null || _count == 0 || SubmitField is null) return;
        var member = _shown.Members[_index];
        _actions.Begin(_session!, _input!, member.Carried.Actor, MemberName(member, _session!), SubmitField, _icons, _entry);
    }
    internal void Handle(GameAction action, GameSession session, GameInput input)
    {
        if (_actions.Open) { _actions.Handle(action); _requiresRelease = true; return; }
        _pointer.Cancel(); if (_requiresRelease) return;
        if (action is GameAction.Cancel or GameAction.Equipment) { Hide(); return; }
        if (action == GameAction.Confirm) { Prepare(); return; }
        if (action == GameAction.Target) { _details.FocusPane = (_details.FocusPane + 1) % 3; }
        int step = action is GameAction.Left or GameAction.Up ? -1 : action is GameAction.Right or GameAction.Down ? 1 : 0;
        if (_details.FocusPane != 0 && action is GameAction.Up or GameAction.Down) _details.ScrollFocused(step * 112);
        else if (step != 0) Select(_index + step);
        Present(session, input);
    }

    internal void HandlePointer(InputEvent @event)
    {
        if (_actions.Open) { _actions.HandlePointer(@event); _requiresRelease = true; return; }
        if (!Open || @event is not (InputEventMouseButton or InputEventMouseMotion)) return;
        if (!GetWindow().HasFocus() || !CanProcess() || !IsVisibleInTree()) { Suspend(); return; }
        if (@event is InputEventMouseMotion motion) { _pointer.Moved(motion); return; }
        var button = (InputEventMouseButton)@event;
        if (button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            int step = button.ButtonIndex == MouseButton.WheelUp ? -112 : 112;
            if (button.Pressed && !_requiresRelease)
            { if (_roster.GetGlobalRect().HasPoint(button.Position)) _roster.ScrollVertical += step;
              else _details.Wheel(button.Position, step); }
            _pointer.Cancel(); return;
        }
        string? id = null; Rect2 rect = default;
        if (button.ButtonIndex == MouseButton.Right) { id = "close"; rect = GetGlobalRect(); }
        if (button.ButtonIndex == MouseButton.Left)
        {
            foreach (var (key, control) in new[] { ("close", _close), ("previous", _previous), ("next", _next), ("selector", _selector), ("prepare", _prepare) })
                if (control.GetGlobalRect().HasPoint(button.Position)) { id = key; rect = control.GetGlobalRect(); break; }
            if (_roster.GetGlobalRect().HasPoint(button.Position))
                for (int i = 0; i < _count; i++)
                    if (_labels[i].GetGlobalRect().HasPoint(button.Position)) { id = i.ToString(); rect = _labels[i].GetGlobalRect(); break; }
        }
        string? clicked = _pointer.Read(button, id, rect, (_index, _details.FocusPane, _roster.ScrollVertical));
        if (_requiresRelease || clicked is null) return;
        if (clicked == "close") { Hide(); return; }
        if (clicked == "prepare") { Prepare(); return; }
        if (clicked == "selector") { _details.FocusPane = 0; }
        else if (clicked is "previous" or "next") Select(_index + (clicked == "previous" ? -1 : 1));
        else { Select(int.Parse(clicked)); _details.FocusPane = 0; }
        Present(_session!, _input!);
    }

    internal object Observe() => new { visible = IsVisibleInTree(), index = _index, count = _count, requiresRelease = _requiresRelease,
        header = _selector.Text, hint = _hint.Text, hintRect = Rect(_hint), hintMinimumHeight = _hint.GetMinimumSize().Y, failure = _assetFailure, memberLayerVisible = _memberLayer.IsVisibleInTree(), selectorOpen = _roster.IsVisibleInTree(), focusPane = _details.FocusPane,
        preparation = _actions.Observe(), prepareRect = Rect(_prepare),
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
