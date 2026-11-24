using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Exploration;

namespace Sf2.Remake.GodotAdapter.UI;

internal sealed record MemberPortrait(int Id, ExplorationPortraitVisual Visual, bool Hd)
{
    internal static MemberPortrait? ForSprite(ExplorationVisuals? visuals, int? sprite, bool hd) =>
        sprite is { } id && visuals?.Sprites.GetValueOrDefault(id)?.Portrait is { } portrait &&
        visuals.Portraits.TryGetValue(portrait, out var visual)
            ? new(portrait, visual, hd && visual.HdRaster is not null)
            : null;
}

internal sealed record MemberRosterEntry(ActorRef Actor, string Name, string Condition, bool Available,
    MemberPortrait? Portrait);

// Local presentation state only. Callers decide membership and what opening a row can do.
internal sealed partial class MemberRosterView : ScrollContainer
{
    private sealed record Row(MarginContainer Control, MemberPageButton Button, Label Marker, Label Text, TextureRect Image)
    {
        private MemberPortrait? _portrait;

        internal void PresentPortrait(MemberPortrait? portrait, Action? clearBorrower)
        {
            if (_portrait == portrait) return;
            clearBorrower?.Invoke();
            var previous = Image.Texture;
            Image.Texture = portrait is null ? null : ExplorationRasterImages.Portrait(portrait.Visual, hd: portrait.Hd);
            Image.Visible = Image.Texture is not null;
            previous?.Dispose();
            _portrait = portrait;
        }
    }

    private readonly VBoxContainer _column = new() { Name = "Members", SizeFlagsHorizontal = SizeFlags.ExpandFill };
    private readonly List<Row> _rows = [];
    private MemberRosterEntry[] _entries = [];
    private bool _revealPending;
    internal required Func<ScrollContainer, Action, MemberPageButton> ButtonFor { get; init; }
    internal required Action<int> FocusMember { get; init; }
    internal Action? BeforePortraitChange { get; set; }
    internal int Index { get; private set; }
    internal ActorRef? Actor => _entries.Length == 0 ? null : _entries[Index].Actor;
    internal Texture2D? Portrait => Actor is null ? null : _rows[Index].Image.Texture;

    public override void _Ready()
    {
        HorizontalScrollMode = ScrollMode.Disabled;
        VerticalScrollMode = ScrollMode.Auto;
        FocusMode = FocusModeEnum.None;
        MouseFilter = MouseFilterEnum.Stop;
        GetVScrollBar().FocusMode = FocusModeEnum.None;
        AddChild(_column);
    }

    public override void _ExitTree()
    {
        foreach (var row in _rows) row.PresentPortrait(null, BeforePortraitChange);
    }

    internal void Reset()
    {
        _entries = [];
        Index = 0;
        _revealPending = false;
        ScrollVertical = 0;
    }

    internal void Present(MemberRosterEntry[] entries)
    {
        var previous = Actor;
        int previousIndex = Index;
        int index = previous is { } actor ? Find(entries, actor) : 0;
        bool removed = index < 0;
        if (removed) index = NearestSurvivor(entries);
        bool countChanged = entries.Length != _entries.Length;
        _entries = entries;
        Index = index;
        while (_rows.Count < entries.Length) AddRow();
        if (countChanged)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                var control = _rows[i].Control;
                control.FocusNeighborTop = control.GetPathTo(_rows[(i + entries.Length - 1) % entries.Length].Control);
                control.FocusNeighborBottom = control.GetPathTo(_rows[(i + 1) % entries.Length].Control);
            }
        }
        for (int i = 0; i < _rows.Count; i++)
        {
            var row = _rows[i];
            bool visible = i < entries.Length;
            row.Control.Visible = visible;
            row.Button.Subject = visible ? entries[i].Actor : null;
            row.PresentPortrait(visible ? entries[i].Portrait : null, BeforePortraitChange);
            if (!visible) continue;
            var entry = entries[i];
            row.Text.Text = entry.Condition.Length == 0 ? entry.Name : entry.Name + " · " + entry.Condition;
        }
        ShowFocus();
        if (removed || index != previousIndex) Reveal();
    }

    // Preserve the old order when finding a neighboring survivor. Prefer the following row on a tie.
    private int NearestSurvivor(MemberRosterEntry[] entries)
    {
        for (int distance = 1; distance < _entries.Length; distance++)
        {
            int next = Index + distance, prior = Index - distance;
            if (next < _entries.Length && Find(entries, _entries[next].Actor) is >= 0 and var after) return after;
            if (prior >= 0 && Find(entries, _entries[prior].Actor) is >= 0 and var before) return before;
        }
        return Math.Min(Index, Math.Max(0, entries.Length - 1));
    }

    private static int Find(MemberRosterEntry[] entries, ActorRef actor) => Array.FindIndex(entries, entry => entry.Actor == actor);

    internal void Select(int index)
    {
        if (_entries.Length == 0) return;
        Index = index;
        ShowFocus();
        Reveal();
    }

    internal Control? SelectedRow => Actor is null ? null : _rows[Index].Control;

    internal void MoveFocus(int step)
    {
        SelectedRow?.FindValidFocusNeighbor(step < 0 ? Side.Top : Side.Bottom)?.GrabFocus();
    }

    private void Reveal()
    {
        if (_revealPending) return;
        _revealPending = true;
        // Container layout for wrapped text and membership changes settles before revealing the row.
        Callable.From(() =>
        {
            if (!_revealPending) return;
            _revealPending = false;
            if (IsVisibleInTree() && Actor is not null) EnsureControlVisible(_rows[Index].Control);
        }).CallDeferred();
    }

    private void ShowFocus()
    {
        for (int i = 0; i < _entries.Length; i++)
        {
            bool selected = _rows[i].Control.HasFocus();
            _rows[i].Marker.Text = selected ? "›" : "";
            _rows[i].Text.Modulate = !_entries[i].Available ? StoryTheme.Unavailable :
                selected ? StoryTheme.Selected : Colors.White;
        }
    }

    private void AddRow()
    {
        var row = new HBoxContainer { CustomMinimumSize = new(0, 72),
            MouseFilter = MouseFilterEnum.Ignore };
        row.AddThemeConstantOverride("separation", 8);
        var marker = new Label { CustomMinimumSize = new(18, 0), MouseFilter = MouseFilterEnum.Ignore,
            VerticalAlignment = VerticalAlignment.Center };
        marker.AddThemeColorOverride("font_color", StoryTheme.Selected);
        var image = new TextureRect { CustomMinimumSize = new(48, 64), Visible = false,
            MouseFilter = MouseFilterEnum.Ignore, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, TextureFilter = TextureFilterEnum.Nearest };
        var label = new Label { MouseFilter = MouseFilterEnum.Ignore, SizeFlagsHorizontal = SizeFlags.ExpandFill,
            VerticalAlignment = VerticalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        label.AddThemeFontSizeOverride("font_size", 28);
        row.AddChild(marker); row.AddChild(image); row.AddChild(label);
        int index = _rows.Count;
        var frame = MemberPageButton.Stack(row);
        frame.Name = "Member" + index;
        frame.FocusMode = FocusModeEnum.All;
        frame.FocusEntered += () => FocusMember(index);
        var button = ButtonFor(this, () => frame.GrabFocus());
        frame.AddChild(button);
        _column.AddChild(frame);
        _rows.Add(new(frame, button, marker, label, image));
    }

    internal IEnumerable<object> ObserveRows() => _entries.Select((entry, i) => new
    {
        actor = entry.Actor.Value, name = entry.Name, condition = entry.Condition, available = entry.Available,
        focused = _rows[i].Marker.Text.Length != 0, portrait = _rows[i].Image.IsVisibleInTree(),
        text = _rows[i].Text.Text, lines = _rows[i].Text.GetLineCount(),
        rect = BattleMapViewport.Rectangle(_rows[i].Control.GetGlobalRect()),
    });
}
