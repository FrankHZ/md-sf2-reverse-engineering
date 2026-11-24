using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Exploration;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.UI;

// Shared browse surface. Only the field caller supplies a primary action.
internal sealed partial class MemberPageView : Control
{
    private const int PageWidth = 1712, RosterWidth = 304, SummaryColumnWidth = 392;
    internal PartyDetailsPanel Details { get; }
    private readonly Label _selector = Text("MemberSelector", 28), _hint = Text("PartyHint", StoryTheme.HintSize), _close = Text("Close", 26);
    private readonly Label _previous = Text("Previous", 32), _next = Text("Next", 32);
    private readonly Label _portraitNote = Text("PortraitNote", 24);
    private readonly Label _kills = Text("Kills", 30), _defeats = Text("Defeats", 30), _gold = Text("GoldLabel", 30);
    private readonly Label _killsValue = Text("KillsValue", 30), _defeatsValue = Text("DefeatsValue", 30), _goldValue = Text("Gold", 30);
    private readonly PanelContainer _recordFrame = new() { Name = "RecordFrame", MouseFilter = MouseFilterEnum.Ignore };
    private readonly PanelContainer _goldFrame = new() { Name = "GoldFrame", MouseFilter = MouseFilterEnum.Ignore };
    private readonly PanelContainer _footer = new() { Name = "PartyFooter", MouseFilter = MouseFilterEnum.Ignore };
    private readonly TextureRect _portrait = new() { Name = "MemberPortrait", ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, TextureFilter = TextureFilterEnum.Nearest, MouseFilter = MouseFilterEnum.Ignore };
    private readonly MemberRosterView _roster = new() { Name = "Roster" };
    private readonly MenuPointer _pointer = new();
    private readonly Label _prepare = Text("Prepare", 28);
    internal Action? Primary { get; set; }
    internal Action? Back { get; set; }
    internal ActorRef? Actor => _roster.Actor;
    internal int Index => _roster.Index;
    internal int Count { get; private set; }
    internal string Hint => _hint.Text;
    private int? _portraitId;
    private bool _requiresRelease;

    internal MemberPageView()
    {
        var portrait = new CenterContainer();
        _portrait.CustomMinimumSize = new(144, 192);
        _portraitNote.CustomMinimumSize = new(160, 0);
        _portraitNote.HorizontalAlignment = HorizontalAlignment.Center;
        _portraitNote.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        portrait.AddChild(_portrait);
        portrait.AddChild(_portraitNote);
        Details = new() { Name = "MemberDetails", PortraitSlot = portrait, SizeFlagsHorizontal = SizeFlags.ExpandFill };
    }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); MouseFilter = MouseFilterEnum.Stop; Theme = StoryTheme.Create();
        // The shared sheet and footer inherit the same column spacing.
        Theme.SetConstant("separation", "HBoxContainer", 24);
        var shade = new ColorRect { Color = new(0, 0, 0, 0.7f), MouseFilter = MouseFilterEnum.Ignore };
        shade.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(shade);
        // Anchor only the design canvas. Containers own every rectangle inside it.
        var page = new VBoxContainer
        {
            AnchorLeft = 0.5f, AnchorRight = 0.5f, AnchorTop = 0.5f, AnchorBottom = 1,
            OffsetLeft = -PageWidth / 2, OffsetRight = PageWidth / 2,
            OffsetTop = -492, OffsetBottom = -StoryTheme.GameplayInset
        };
        page.AddThemeConstantOverride("separation", 12);
        AddChild(page);
        var content = new VBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        content.AddThemeConstantOverride("separation", 12);
        page.AddChild(content);
        _selector.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _selector.CustomMinimumSize = new(RosterWidth, 52);
        _selector.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        content.AddChild(_selector);

        content.AddChild(BuildBody());
        content.AddChild(BuildSummary());
        page.AddChild(_footer);
        BuildFooter();
    }

    private HBoxContainer BuildBody()
    {
        var body = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill, CustomMaximumSize = new(-1, 720) };
        var rosterFrame = new PanelContainer { CustomMinimumSize = new(RosterWidth, 0) };
        rosterFrame.AddThemeStyleboxOverride("panel", StoryTheme.PanelFrame(16, 16));
        body.AddChild(rosterFrame);
        var rosterColumn = new VBoxContainer();
        rosterColumn.AddThemeConstantOverride("separation", 16);
        rosterFrame.AddChild(rosterColumn);
        _roster.SizeFlagsVertical = SizeFlags.ExpandFill;
        rosterColumn.AddChild(_roster);
        var arrows = new HBoxContainer();
        rosterColumn.AddChild(arrows);
        _previous.Text = "◀"; _next.Text = "▶";
        _previous.CustomMinimumSize = _next.CustomMinimumSize = new(64, 48);
        arrows.AddChild(_previous);
        arrows.AddSpacer(false);
        arrows.AddChild(_next);

        body.AddChild(Details);
        return body;
    }

    private HBoxContainer BuildSummary()
    {
        // Reserve the same reading area in battle when the field accounting frames are hidden.
        var summary = new HBoxContainer { CustomMinimumSize = new(0, 72) };
        foreach (var frame in new[] { _recordFrame, _goldFrame })
        {
            frame.AddThemeStyleboxOverride("panel", StoryTheme.PanelFrame(0, 12));
            summary.AddChild(frame);
        }
        var records = new HBoxContainer();
        records.AddThemeConstantOverride("separation", 0);
        _recordFrame.AddChild(records);
        records.AddChild(SummaryCell(_kills, _killsValue));
        records.AddChild(SummaryCell(_defeats, _defeatsValue));
        _goldFrame.AddChild(SummaryCell(_gold, _goldValue));
        _recordFrame.Hide();
        _goldFrame.Hide();
        return summary;
    }

    private void BuildFooter()
    {
        var footerRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
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
    }

    private static MarginContainer SummaryCell(Label label, Label value)
    {
        var cell = new MarginContainer { CustomMinimumSize = new(SummaryColumnWidth, 48) };
        cell.AddThemeConstantOverride("margin_left", 24);
        cell.AddThemeConstantOverride("margin_right", 24);
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 12);
        cell.AddChild(row);
        label.CustomMinimumSize = new(152, 48);
        value.SizeFlagsVertical = SizeFlags.Fill;
        value.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        value.HorizontalAlignment = HorizontalAlignment.Right;
        row.AddChild(label);
        row.AddChild(value);
        return cell;
    }

    public override void _ExitTree() => _portrait.Texture = null;

    internal void Reset(int focusPane = 0)
    {
        _roster.Reset();
        Details.FocusPane = focusPane;
        Details.ResetReading();
        Suspend();
    }

    internal void PresentRoster(MemberRosterEntry[] entries, string title, GameInput input)
    {
        _pointer.ReleaseIfNeutral();
        if (!input.GameplayHeld) _requiresRelease = false;
        var previous = _roster.Actor;
        Count = entries.Length;
        _roster.Present(entries, focused: Details.FocusPane == 0);
        if (previous != _roster.Actor) Details.ResetReading();
        _selector.Text = title + $"   {(Count == 0 ? 0 : Index + 1)} / {Count}";
        _portraitId = Count == 0 ? null : entries[Index].Portrait?.Id;
        _portrait.Texture = _roster.Portrait;
        _portrait.Visible = _portrait.Texture is not null;
        _portraitNote.Visible = !_portrait.Visible;
        _portraitNote.Text = GameText.Get("party.portrait-unavailable");
    }

    internal void PresentRecords(string kills, string defeats, string gold)
    {
        _recordFrame.Show(); _goldFrame.Show();
        _kills.Text = GameText.Get("party.kills"); _defeats.Text = GameText.Get("party.defeats");
        _gold.Text = GameText.Get("party.gold-label");
        _killsValue.Text = kills; _defeatsValue.Text = defeats; _goldValue.Text = gold;
    }

    internal string FocusLabel => GameText.Get(Details.FocusPane switch
    { 1 => "party.magic", 2 => "party.items", _ => "party.member" });

    internal void PresentFooter(string hint, string back, string? primary = null)
    {
        _hint.Text = hint;
        _close.Text = back;
        _prepare.Visible = primary is not null;
        _prepare.Text = primary ?? "";
    }

    private void Select(int index)
    {
        if (Count == 0) return;
        _roster.Select(index);
        Details.ResetReading(keepItemSlot: Details.FocusPane == 2);
    }
    internal void Suspend() { _requiresRelease = true; _pointer.Suspend(); }

    internal void Handle(GameAction action)
    {
        _pointer.Cancel();
        if (_requiresRelease) return;
        if (action == GameAction.Cancel) { Back?.Invoke(); return; }
        if (action == GameAction.Confirm) { Primary?.Invoke(); return; }
        if (action == GameAction.Target) { Details.FocusPane = (Details.FocusPane + 1) % 3; }
        int step = action is GameAction.Left or GameAction.Up ? -1 : action is GameAction.Right or GameAction.Down ? 1 : 0;
        if (action is GameAction.Up or GameAction.Down && Details.FocusPane == 2)
            Details.SelectItemSlot(Details.ItemSlot + step);
        else if (action is GameAction.Up or GameAction.Down && Details.FocusPane == 1) Details.ScrollFocused(step * 112);
        else if (step != 0) Select(Index + step);
    }

    internal void HandlePointer(InputEvent @event)
    {
        if (@event is not (InputEventMouseButton or InputEventMouseMotion)) return;
        if (!GetWindow().HasFocus() || !CanProcess() || !IsVisibleInTree()) { Suspend(); return; }
        if (@event is InputEventMouseMotion motion) { _pointer.Moved(motion); return; }
        var button = (InputEventMouseButton)@event;
        if (button.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            int step = button.ButtonIndex == MouseButton.WheelUp ? -112 : 112;
            if (button.Pressed && !_requiresRelease)
            {
                if (!_roster.Wheel(button.Position, step)) Details.Wheel(button.Position, step);
            }
            _pointer.Cancel(); return;
        }
        string? id = null; Rect2 rect = default;
        int memberIndex = -1;
        if (button.ButtonIndex == MouseButton.Right) { id = "close"; rect = GetGlobalRect(); }
        if (button.ButtonIndex == MouseButton.Left)
        {
            foreach (var (key, control) in new[] { ("close", _close), ("previous", _previous), ("next", _next), ("selector", _selector), ("prepare", _prepare) })
                if (control.IsVisibleInTree() && control.GetGlobalRect().HasPoint(button.Position)) { id = key; rect = control.GetGlobalRect(); break; }
            if (_roster.Hit(button.Position) is (>= 0 and var index, var rowId, var rowRect))
            { memberIndex = index; id = rowId; rect = rowRect; }
            if (id is null && Details.HitItem(button.Position) is { Slot: >= 0 } item)
            { id = $"item:{item.Slot}"; rect = item.Rect; }
        }
        string? clicked = _pointer.Read(button, id, rect, (_roster.Actor, Details.FocusPane, _roster.ScrollVertical));
        if (_requiresRelease || clicked is null) return;
        if (clicked == "close") { Back?.Invoke(); return; }
        if (clicked == "prepare") { Primary?.Invoke(); return; }
        if (clicked == "selector") { Details.FocusPane = 0; }
        else if (clicked is "previous" or "next") Select(Index + (clicked == "previous" ? -1 : 1));
        else if (clicked.StartsWith("item:", StringComparison.Ordinal))
        { Details.FocusPane = 2; Details.SelectItemSlot(int.Parse(clicked.AsSpan(5))); }
        else if (memberIndex >= 0) { Select(memberIndex); Details.FocusPane = 0; }
    }

    internal object Observe(bool visible, object? preparation = null, object? projection = null, SessionFailure? failure = null) => new { visible, index = Index, count = Count, requiresRelease = _requiresRelease,
        header = _selector.Text, hint = _hint.Text, hintRect = Rect(_hint), hintMinimumHeight = _hint.GetMinimumSize().Y, failure, memberLayerVisible = IsVisibleInTree(), selectorOpen = _roster.IsVisibleInTree(), focusPane = Details.FocusPane,
        preparation, prepareVisible = _prepare.IsVisibleInTree(), prepareRect = Rect(_prepare),
        card = Details.Card(), layout = Details.Observe(), portraitId = _portraitId, portraitVisible = _portrait.IsVisibleInTree(),
        portraitRect = Rect(_portrait), recordRect = Rect(_recordFrame), goldRect = Rect(_goldFrame), kills = _killsValue.Text, defeats = _defeatsValue.Text, gold = _goldValue.Text,
        recordValues = new[] { _killsValue, _defeatsValue }.Select(label => new { text = label.Text,
            rect = Rect(label), minimumWidth = label.GetMinimumSize().X, fontSize = label.GetThemeFontSize("font_size") }),
        selectorRect = Rect(_selector), previousRect = Rect(_previous), nextRect = Rect(_next), closeRect = Rect(_close),
        rosterRect = Rect(_roster), rosterScroll = _roster.ScrollVertical,
        projection, rows = _roster.ObserveRows() };
    private static object Rect(Control control) => BattleMapViewport.Rectangle(control.GetGlobalRect());
    private static Label Text(string name, int size)
    { var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore }; label.AddThemeFontSizeOverride("font_size", size); return label; }
}
