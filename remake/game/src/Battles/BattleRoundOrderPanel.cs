using Godot;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

// This panel has no gameplay gestures or hit regions.
internal sealed partial class BattleRoundOrderPanel : Panel
{
    private const int Inset = 14, Gap = 12, RowHeight = 72, PreferredWidth = 950, SlotWidth = 208, NameLines = 2;
    private readonly Label _current = Text("Current", StoryTheme.BodySize);
    private readonly Label _end = Text("Remaining", StoryTheme.HintSize);
    private readonly Label _next = Text("Next", StoryTheme.HintSize);
    private readonly Label[] _slots = [Text("First", StoryTheme.HintSize), Text("Second", StoryTheme.HintSize),
        Text("Third", StoryTheme.HintSize)];
    private BattleRoundOrder? _order;
    private int _overflow;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        FocusMode = FocusModeEnum.None;
        ClipContents = true;
        AddChild(_current); AddChild(_end); AddChild(_next);
        _current.AddThemeColorOverride("font_color", StoryTheme.Selected);
        _end.HorizontalAlignment = HorizontalAlignment.Right;
        foreach (var slot in _slots) AddChild(slot);
        Hide();
    }

    internal void Present(BattleRoundOrder? order)
    {
        _order = order;
        if (order is null) return;
        _current.Text = GameText.Get("battle.order.current", Identity(order.Current));
        _next.Text = GameText.Get("battle.order.next");
        for (int i = 0; i < order.Next.Count; i++)
            _slots[i].Text = GameText.Get("battle.order.slot", i + 1, Identity(order.Next[i]));
    }

    internal void Place(Vector2 corner, float availableWidth, float bottom, Rect2 subject, bool alignRight)
    {
        Visible = _order is not null;
        if (_order is not { } order) return;
        float width = Mathf.Min(PreferredWidth, availableWidth);
        bool hasNext = order.Next.Count > 0;
        float height = RowHeight * (hasNext ? 2 : 1) + Inset * 2;
        var rect = new Rect2(alignRight ? corner.X - width : corner.X, corner.Y, width, height);
        if (rect.Intersects(subject)) rect.Position = new(rect.Position.X, subject.End.Y + Gap);
        if (rect.End.Y > bottom || width < SlotWidth + Inset * 2)
        {
            Hide();
            return;
        }
        Position = rect.Position;
        Size = rect.Size;

        float innerWidth = width - Inset * 2;
        var font = _next.GetThemeFont("font");
        float promptWidth = Mathf.Min(178, Mathf.Ceil(font.GetStringSize(_next.Text, fontSize: StoryTheme.HintSize).X));
        float slotsWidth = innerWidth - promptWidth - Gap;
        int count = FittingSlots(slotsWidth, order.Next.Count, font);
        _overflow = order.Additional + order.Next.Count - count;
        _end.Text = _overflow > 0 ? GameText.Get("battle.order.more", _overflow) : GameText.Get("battle.order.round-end");
        float endWidth = Mathf.Min(innerWidth * 0.45f, Mathf.Ceil(font.GetStringSize(_end.Text, fontSize: StoryTheme.HintSize).X));
        SetRect(_current, Inset, Inset, innerWidth - endWidth - Gap);
        SetRect(_end, width - Inset - endWidth, Inset, endWidth);
        _next.Visible = hasNext;
        SetRect(_next, Inset, Inset + RowHeight, promptWidth);
        float slotWidth = count > 0 ? (slotsWidth - Gap * (count - 1)) / count : 0;
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].Visible = i < count;
            if (i < count) SetRect(_slots[i], Inset + promptWidth + Gap + i * (slotWidth + Gap), Inset + RowHeight, slotWidth);
        }
    }

    private int FittingSlots(float width, int slots, Font font)
    {
        if (slots == 0) return 0;
        using var text = new TextParagraph
        {
            BreakFlags = TextServer.LineBreakFlag.Mandatory | TextServer.LineBreakFlag.WordBound | TextServer.LineBreakFlag.Adaptive,
        };
        for (int count = slots; count > 1; count--)
        {
            text.Width = (width - Gap * (count - 1)) / count;
            if (text.Width < SlotWidth) continue;
            bool fits = true;
            for (int slot = 0; slot < count; slot++)
            {
                text.Clear();
                text.AddString(_slots[slot].Text, font, StoryTheme.HintSize);
                if (text.GetLineCount() <= NameLines) continue;
                fits = false;
                break;
            }
            if (fits) return count;
        }
        return 1;
    }

    private static void SetRect(Label label, float x, float y, float width)
    {
        label.Position = new(x, y);
        label.Size = new(width, RowHeight);
    }

    private static Label Text(string name, int size)
    {
        var label = new Label { Name = name, MouseFilter = MouseFilterEnum.Ignore,
            AutowrapMode = TextServer.AutowrapMode.WordSmart, MaxLinesVisible = NameLines,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis, VerticalAlignment = VerticalAlignment.Center };
        label.AddThemeFontSizeOverride("font_size", size);
        return label;
    }

    private static string Identity(BattleRoundActor? actor)
    {
        if (actor is null) return GameText.Get("battle.order.unknown");
        string side = GameText.Get(actor.Ally ? "battle.inspect.ally" : "battle.inspect.enemy");
        string name = DisplayNames.InspectionName(actor.Ally, actor.NameIndex, actor.Name, actor.CustomName);
        return GameText.Get("battle.order.actor", side, actor.Position.X, actor.Position.Y, name);
    }

    internal object Observe() => new
    {
        visible = IsVisibleInTree(),
        rect = BattleMapViewport.Rectangle(GetGlobalRect()),
        current = _current.Text,
        next = _slots.Where(slot => slot.IsVisibleInTree()).Select(slot => slot.Text).ToArray(),
        prompt = _next.IsVisibleInTree() ? _next.Text : null,
        end = _end.Text,
        overflow = _overflow,
    };
}
