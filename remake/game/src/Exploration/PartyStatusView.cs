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
    private const float SummaryHeight = 72, SummaryColumnWidth = 392;
    private readonly PartyDetailsPanel _details = new() { Name = "MemberDetails", FieldLayout = true };
    private readonly Control _memberLayer = new() { Name = "MemberLayer" };
    private readonly Label _selector = Text("MemberSelector", 28), _hint = Text("PartyHint", StoryTheme.HintSize), _close = Text("Close", 26);
    private readonly Label _previous = Text("Previous", 32), _next = Text("Next", 32);
    private readonly Label _portraitNote = Text("PortraitNote", 24);
    private readonly Label _kills = Text("Kills", 30), _defeats = Text("Defeats", 30), _gold = Text("GoldLabel", 30);
    private readonly Label _killsValue = Text("KillsValue", 30), _defeatsValue = Text("DefeatsValue", 30), _goldValue = Text("Gold", 30);
    private readonly Panel _rosterFrame = new();
    private readonly Panel _recordFrame = new() { Name = "RecordFrame", MouseFilter = MouseFilterEnum.Ignore };
    private readonly Panel _goldFrame = new() { Name = "GoldFrame", MouseFilter = MouseFilterEnum.Ignore };
    private readonly PanelContainer _footer = new() { Name = "PartyFooter", MouseFilter = MouseFilterEnum.Ignore };
    private readonly TextureRect _portrait = new() { Name = "MemberPortrait", ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, TextureFilter = TextureFilterEnum.Nearest, MouseFilter = MouseFilterEnum.Ignore };
    private readonly MemberRosterView _roster = new() { Name = "Roster" };
    private readonly MenuPointer _pointer = new();
    private readonly Label _prepare = Text("Prepare", 28);
    private readonly FieldActionView _actions = new() { Name = "FieldActions" };
    private readonly IdentityIcons _icons = new();
    private object? _iconSource;
    private SessionFailure? _assetFailure;
    internal Func<Sf2.Remake.Application.Runtime.Exploration.FieldCommand, long, SessionResult>? SubmitField { get; set; }
    private GameSession? _session;
    private GameInput? _input;
    private PartyDetails? _shown;
    private int? _portraitId;
    private int Index => _roster.Index;
    private int Count => _shown?.Members.Count ?? 0;
    private bool _requiresRelease;
    internal bool Open => Visible;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Stop; Theme = StoryTheme.Create();
        var shade = new ColorRect { Color = new(0, 0, 0, 0.7f), MouseFilter = MouseFilterEnum.Ignore };
        _memberLayer.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(_memberLayer);
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _memberLayer.AddChild(shade);
        foreach (var node in new Control[] { _recordFrame, _goldFrame, _details,
            _portrait, _portraitNote, _selector, _previous, _next, _rosterFrame, _roster, _footer }) _memberLayer.AddChild(node);
        foreach (var (frame, label, value) in new[]
        {
            (_recordFrame, _kills, _killsValue), (_recordFrame, _defeats, _defeatsValue), (_goldFrame, _gold, _goldValue)
        })
        {
            label.VerticalAlignment = value.VerticalAlignment = VerticalAlignment.Center;
            value.HorizontalAlignment = HorizontalAlignment.Right;
            frame.AddChild(label); frame.AddChild(value);
        }
        var footerRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        footerRow.AddThemeConstantOverride("separation", 24);
        _footer.AddChild(footerRow);
        _hint.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _hint.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        footerRow.AddChild(_hint);
        foreach (var action in new[] { _prepare, _close })
        {
            action.CustomMinimumSize = new(144, 64);
            action.HorizontalAlignment = HorizontalAlignment.Right;
            footerRow.AddChild(action);
        }
        AddChild(_actions);
        _selector.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _portraitNote.HorizontalAlignment = HorizontalAlignment.Center;
        _portraitNote.AutowrapMode = TextServer.AutowrapMode.WordSmart;

        _previous.Text = "◀"; _next.Text = "▶";
        Hide();
    }

    public override void _ExitTree() { _portrait.Texture = null; _icons.Dispose(); _iconSource = null; }
    private string _entry = "actions";
    internal void PrepareGraphics(GameSession session)
    {
        if (ReferenceEquals(_iconSource, session.Definition)) return;
        var visuals = session.Definition.Exploration?.Visuals;
        _icons.Configure(visuals?.IdentityIcons, visuals?.HdIdentityIcons);
        _iconSource = session.Definition; _assetFailure = null;
    }
    internal void Begin(GameSession session, GameInput input, string entry = "actions")
    {
        _entry = entry;
        try { PrepareGraphics(session); }
        catch (InvalidOperationException error)
        { _assetFailure = new(SessionFailureKind.AdapterError, error.Message, "presentation.identityIcons", "Member identity icon decoding failed."); }
        _details.ConfigureIcons(_icons, session.Definition);
        _session = session; _input = input; _roster.Reset(); _details.FocusPane = entry == "magic" ? 1 : entry == "item" ? 2 : 0;
        _requiresRelease = true; _pointer.Suspend(); _details.ResetReading();
        _actions.Hide();
        Show(); Present(session, input);
    }

    internal void Present(GameSession session, GameInput input)
    {
        _icons.Mode = input.Settings.GraphicsMode;
        if (!Open) return;
        _memberLayer.Visible = !_actions.Open;
        if (_actions.Open) { _actions.Present(); return; }
        _session = session; _input = input; _pointer.ReleaseIfNeutral();
        if (!input.GameplayHeld) _requiresRelease = false;
        if (!session.Current.CanWaitAtInput || session.QueryPartyDetails() is not { } party) { Hide(); return; }
        _prepare.Text = GameText.Get(_entry == "magic" ? "prepare.magic" : _entry == "item" ? "field-menu.item" : "party.prepare"); _close.Text = GameText.Get("party.close");
        _kills.Text = GameText.Get("party.kills"); _defeats.Text = GameText.Get("party.defeats");
        _gold.Text = GameText.Get("party.gold-label");
        var entries = party.Members.Select(member =>
        {
            bool available = member.Deployment is not null && member.Carried.Initialized != false;
            string condition = !available ? GameText.Get("party.value-unavailable") :
                member.Carried.Absent ? GameText.Get("party.absent") : member.Carried.Hp == 0 ? GameText.Get("party.defeated") : "";
            return new MemberRosterEntry(member.Carried.Actor, MemberName(member, session),
                condition, available, PortraitFor(session, member));
        }).ToArray();
        var previous = _roster.Actor;
        _shown = party;
        _roster.Present(entries, focused: _details.FocusPane == 0);
        if (previous != _roster.Actor) _details.ResetReading();
        var member = Count == 0 ? null : party.Members[Index];
        string name = member is null ? GameText.Get("party.unavailable") : entries[Index].Name;
        _selector.Text = GameText.Get("party.member") + $"   {(Count == 0 ? 0 : Index + 1)} / {Count}";
        _details.Present(member, name, session);
        string unavailable = GameText.Get("party.value-unavailable");
        _killsValue.Text = member?.Carried.Kills?.ToString() ?? unavailable;
        _defeatsValue.Text = member?.Carried.Defeats?.ToString() ?? unavailable;
        _goldValue.Text = party.Gold?.ToString() ?? unavailable;
        _portraitId = member is null ? null : entries[Index].Portrait?.Id;
        _portrait.Texture = _roster.Portrait;
        _portrait.Visible = _portrait.Texture is not null;
        _portraitNote.Visible = !_portrait.Visible;
        _portraitNote.Text = GameText.Get("party.portrait-unavailable");
        string focus = GameText.Get(_details.FocusPane switch { 1 => "party.magic", 2 => "party.items", _ => "party.member" });
        _hint.Text = GameText.Get("party.hint", input.CompactHint(GameAction.Confirm), input.CompactHint(GameAction.Up),
            input.CompactHint(GameAction.Down), focus, input.CompactHint(GameAction.Target),
            input.CompactHint(GameAction.Cancel), input.CompactHint(GameAction.Equipment));
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

    private MemberPortrait? PortraitFor(GameSession session, PartyMemberDetails member)
    {
        var visuals = session.Definition.Exploration?.Visuals;
        int? binding = member.Deployment?.Initialization?.AllyPartyMember;
        var appearance = session.Current.Exploration?.Population?.AllySprites?.FirstOrDefault(row => row.Character == binding);
        int? sprite = appearance is null ? null : appearance.JoinedFlag is { } joined && !session.Current.Story.Flags.Contains(joined)
            ? appearance.UnjoinedSprite : appearance.ForClass(member.Carried.Progress?.ClassId);
        bool hd = _input?.Settings.GraphicsMode == "hd" && session.Current.Exploration is { } field &&
            visuals?.Maps.GetValueOrDefault(field.Map)?.HdBlocks is not null;
        return MemberPortrait.ForSprite(visuals, sprite, hd);
    }

    private void Arrange()
    {
        float left = (Size.X - 1712) / 2, top = Math.Max(112, (Size.Y - 856) / 2);
        _footer.Size = new(1712, _footer.GetCombinedMinimumSize().Y);
        _footer.Position = new(left, Size.Y - StoryTheme.GameplayInset - _footer.Size.Y);
        float summaryCenter = Math.Min(top + 776, _footer.Position.Y - 12 - SummaryHeight / 2);
        float summaryTop = summaryCenter - SummaryHeight / 2;
        float sheetHeight = Math.Min(720, summaryTop - 8 - top);
        _selector.Position = new(left, top - 64); _selector.Size = new(304, 48);
        _selector.HorizontalAlignment = HorizontalAlignment.Left;
        _previous.Position = new(left + 16, top + sheetHeight - 56); _previous.Size = new(64, 48);
        _next.Position = new(left + 224, top + sheetHeight - 56); _next.Size = new(64, 48);
        _rosterFrame.Show(); _roster.Show();
        _rosterFrame.Position = new(left, top); _rosterFrame.Size = new(304, sheetHeight);
        _roster.Position = new(left + 16, top + 16); _roster.Size = new(272, sheetHeight - 96);
        _details.Position = new(left + 328, top); _details.Size = new(1384, sheetHeight); _details.Arrange();
        _portrait.Position = new(left + 360, top + 24); _portrait.Size = new(144, 192);
        _portraitNote.Position = new(left + 352, top + 40); _portraitNote.Size = new(160, 160);
        _recordFrame.Position = new(left, summaryTop); _recordFrame.Size = new(2 * SummaryColumnWidth, SummaryHeight);
        _goldFrame.Position = new(left + 2 * SummaryColumnWidth + 24, summaryTop); _goldFrame.Size = new(SummaryColumnWidth, SummaryHeight);
        ArrangeSummaryRow(_kills, _killsValue, 0);
        ArrangeSummaryRow(_defeats, _defeatsValue, SummaryColumnWidth);
        ArrangeSummaryRow(_gold, _goldValue, 0);
    }

    private static void ArrangeSummaryRow(Label label, Label value, float column)
    {
        const float inset = 24, labelWidth = 152, gap = 12, rowHeight = 48;
        label.Position = new(column + inset, (SummaryHeight - rowHeight) / 2); label.Size = new(labelWidth, rowHeight);
        value.Position = label.Position + new Vector2(labelWidth + gap, 0);
        value.Size = new(SummaryColumnWidth - 2 * inset - labelWidth - gap, rowHeight);
    }

    private void Select(int index)
    {
        if (Count == 0) return;
        _roster.Select(index);
        _details.ResetReading(keepItemSlot: _details.FocusPane == 2);
    }
    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); _actions.Suspend(); }
    private void Prepare()
    {
        if (_shown is null || Count == 0 || SubmitField is null) return;
        var member = _shown.Members[Index];
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
        if (action is GameAction.Up or GameAction.Down && _details.FocusPane == 2)
            _details.SelectItemSlot(_details.ItemSlot + step);
        else if (action is GameAction.Up or GameAction.Down && _details.FocusPane == 1) _details.ScrollFocused(step * 112);
        else if (step != 0) Select(Index + step);
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
            {
                if (!_roster.Wheel(button.Position, step)) _details.Wheel(button.Position, step);
            }
            _pointer.Cancel(); return;
        }
        string? id = null; Rect2 rect = default;
        int memberIndex = -1;
        if (button.ButtonIndex == MouseButton.Right) { id = "close"; rect = GetGlobalRect(); }
        if (button.ButtonIndex == MouseButton.Left)
        {
            foreach (var (key, control) in new[] { ("close", _close), ("previous", _previous), ("next", _next), ("selector", _selector), ("prepare", _prepare) })
                if (control.GetGlobalRect().HasPoint(button.Position)) { id = key; rect = control.GetGlobalRect(); break; }
            if (_roster.Hit(button.Position) is (>= 0 and var index, var rowId, var rowRect))
            { memberIndex = index; id = rowId; rect = rowRect; }
            if (id is null && _details.HitItem(button.Position) is { Slot: >= 0 } item)
            { id = $"item:{item.Slot}"; rect = item.Rect; }
        }
        string? clicked = _pointer.Read(button, id, rect, (_roster.Actor, _details.FocusPane, _roster.ScrollVertical));
        if (_requiresRelease || clicked is null) return;
        if (clicked == "close") { Hide(); return; }
        if (clicked == "prepare") { Prepare(); return; }
        if (clicked == "selector") { _details.FocusPane = 0; }
        else if (clicked is "previous" or "next") Select(Index + (clicked == "previous" ? -1 : 1));
        else if (clicked.StartsWith("item:", StringComparison.Ordinal))
        { _details.FocusPane = 2; _details.SelectItemSlot(int.Parse(clicked.AsSpan(5))); }
        else if (memberIndex >= 0) { Select(memberIndex); _details.FocusPane = 0; }
        Present(_session!, _input!);
    }

    internal object Observe() => new { visible = IsVisibleInTree(), index = Index, count = Count, requiresRelease = _requiresRelease,
        header = _selector.Text, hint = _hint.Text, hintRect = Rect(_hint), hintMinimumHeight = _hint.GetMinimumSize().Y, failure = _assetFailure, memberLayerVisible = _memberLayer.IsVisibleInTree(), selectorOpen = _roster.IsVisibleInTree(), focusPane = _details.FocusPane,
        preparation = _actions.Observe(), prepareRect = Rect(_prepare),
        card = _details.Card(), layout = _details.Observe(), portraitId = _portraitId, portraitVisible = _portrait.IsVisibleInTree(),
        portraitRect = Rect(_portrait), recordRect = Rect(_recordFrame), goldRect = Rect(_goldFrame), kills = _killsValue.Text, defeats = _defeatsValue.Text, gold = _goldValue.Text,
        recordValues = new[] { _killsValue, _defeatsValue }.Select(label => new { text = label.Text,
            rect = Rect(label), minimumWidth = label.GetMinimumSize().X, fontSize = label.GetThemeFontSize("font_size") }),
        selectorRect = Rect(_selector), previousRect = Rect(_previous), nextRect = Rect(_next), closeRect = Rect(_close),
        rosterRect = Rect(_roster), rosterScroll = _roster.ScrollVertical,
        projection = _shown?.Members.Select(member => new { actor = member.Carried.Actor.Value, hp = member.Carried.Hp, mp = member.Carried.Mp,
            exp = member.Carried.Exp, member.Carried.Kills, member.Carried.Defeats, absent = member.Carried.Absent,
            member.MaxHp, member.MaxMp, member.Level, member.Attack, member.Defense, member.Agility, member.Spells, member.Inventory }),
        rows = _roster.ObserveRows() };
    private static object Rect(Control control) => BattleMapViewport.Rectangle(control.GetGlobalRect());
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}
