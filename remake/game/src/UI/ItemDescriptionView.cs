using Godot;
using Sf2.Remake.GodotAdapter.Battles;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.UI;

// The viewport size is independent of the selected body and of the overflow hint.
internal sealed partial class ItemDescriptionView : VBoxContainer
{
    private readonly ScrollContainer _scroll = new()
    {
        Name = "ReadingArea", MouseFilter = MouseFilterEnum.Ignore, FocusMode = FocusModeEnum.None,
        HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, SizeFlagsVertical = SizeFlags.ExpandFill,
    };
    private readonly VBoxContainer _body = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill };
    internal readonly Label Body = new()
    {
        Name = "Body", AutowrapMode = TextServer.AutowrapMode.WordSmart,
        SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore,
    };
    private readonly Control _hintSlot = new() { CustomMinimumSize = new(0, 36), MouseFilter = MouseFilterEnum.Ignore };
    private readonly Label _hint = new() { Name = "ReadHint", MouseFilter = MouseFilterEnum.Ignore };
    private object? _subject;

    internal const int BodySize = 30;
    internal int ReadingHeight { get; init; } = 196;
    private double Maximum => _scroll.GetVScrollBar().MaxValue - _scroll.GetVScrollBar().Page;

    public override void _Ready()
    {
        if (Name.ToString().StartsWith('@')) Name = "ItemDescription";
        MouseFilter = MouseFilterEnum.Ignore;
        CustomMinimumSize = new(0, ReadingHeight);
        AddThemeConstantOverride("separation", 0);
        _body.AddThemeConstantOverride("separation", 8);
        Body.ThemeTypeVariation = ReadingSize.Role(BodySize);
        _hint.ThemeTypeVariation = ReadingSize.Role(StoryTheme.HintSize);
        AddChild(_scroll); _scroll.AddChild(_body); _body.AddChild(Body);
        AddChild(_hintSlot); _hintSlot.AddChild(_hint);
        _hint.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _hintSlot.Hide(); _hint.Hide();
        var bar = _scroll.GetVScrollBar();
        bar.FocusMode = FocusModeEnum.None;
        bar.MouseFilter = MouseFilterEnum.Ignore;
        bar.Changed += RefreshHint;
    }

    internal void AddContext(Label heading, Label help)
    {
        _body.AddChild(heading); _body.MoveChild(heading, 0);
        _body.AddChild(help);
    }

    internal void Present(string? text, object? subject = null, GameInput? input = null)
    {
        Visible = text is not null;
        _hintSlot.Visible = input is not null;
        if (input is not null)
            _hint.Text = GameText.Get("ui.read-description", input.CompactHint(GameAction.Left), input.CompactHint(GameAction.Right));
        if (!Equals(_subject, subject) || Body.Text != (text ?? ""))
        {
            _subject = subject;
            Body.Text = text ?? "";
            _scroll.ScrollVertical = 0;
        }
        Body.Visible = !string.IsNullOrWhiteSpace(text);
        RefreshHint();
    }

    private void RefreshHint() => _hint.Visible = _hintSlot.Visible && Maximum > 0;

    internal void Read(int step)
    {
        if (IsVisibleInTree() && Maximum > 0) _scroll.ScrollVertical += step;
    }

    internal void EnableGuiScroll() => _scroll.MouseFilter = MouseFilterEnum.Stop;

    internal bool Wheel(Vector2 point, int step)
    {
        if (!IsVisibleInTree() || !_scroll.GetGlobalRect().HasPoint(point)) return false;
        Read(step);
        return true;
    }

    internal object Observe() => new
    {
        visible = IsVisibleInTree(), text = Body.Text, fontSize = Body.GetThemeFontSize("font_size"),
        rect = BattleMapViewport.Rectangle(_scroll.GetGlobalRect()), minimumHeight = Body.GetMinimumSize().Y,
        scroll = _scroll.ScrollVertical, maximum = Maximum,
        hint = _hint.IsVisibleInTree() ? _hint.Text : null,
        hintRect = _hint.IsVisibleInTree() ? BattleMapViewport.Rectangle(_hint.GetGlobalRect()) : null,
    };
}
