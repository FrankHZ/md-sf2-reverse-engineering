using Godot;
using Sf2.Remake.GodotAdapter.Battles;

namespace Sf2.Remake.GodotAdapter.UI;

// Display geometry and reading position only: no session, tokens, waits, input or clock.
internal sealed partial class StoryDialoguePanel : Control
{
    private readonly Panel _frame = new() { Name = "Frame", MouseFilter = MouseFilterEnum.Ignore };
    private readonly ScrollContainer _scroll = new() { Name = "ReadingArea",
        HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever, MouseFilter = MouseFilterEnum.Ignore,
        FocusMode = FocusModeEnum.None };
    internal Label Body { get; } = new() { Name = "Dialogue", AutowrapMode = TextServer.AutowrapMode.WordSmart,
        SizeFlagsHorizontal = SizeFlags.ExpandFill, VisibleCharactersBehavior = TextServer.VisibleCharactersBehavior.CharsAfterShaping,
        MouseFilter = MouseFilterEnum.Ignore };
    internal Label Hint { get; } = new() { Name = "DialogueHint", AutowrapMode = TextServer.AutowrapMode.WordSmart,
        MouseFilter = MouseFilterEnum.Ignore };
    internal StoryChoiceRow Choices { get; } = new() { Name = "Choice" };
    internal TextureRect Portrait { get; } = new() { Name = "Portrait", ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        StretchMode = TextureRect.StretchModeEnum.Scale, TextureFilter = TextureFilterEnum.Nearest,
        MouseFilter = MouseFilterEnum.Ignore };
    private bool _placed, _top, _portrait, _right;
    private float _portraitY, _readThrough;
    private Rect2 _subject;
    private string _hint = "";
    internal bool MoreBelow => _placed && _readThrough + 1 < Body.Size.Y;

    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_frame); AddChild(_scroll); _scroll.AddChild(Body);
        AddChild(Portrait); AddChild(Choices); AddChild(Hint);
        Hint.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
        Hide();
    }
    internal void Present(bool visible, bool portrait, bool right, Rect2 subject, string hint, bool lifetimeOpen = true)
    {
        Visible = visible; _portrait = portrait; _right = right; _hint = hint;
        if (!lifetimeOpen) { _placed = false; ResetReading(); Portrait.Texture = null; }
        if (!visible) return;
        if (!_placed) _subject = subject;
        Arrange();
    }
    public override void _Process(double delta) { if (IsVisibleInTree()) Arrange(); }
    private void Arrange()
    {
        Vector2 viewport = GetViewportRect().Size;
        float width = Mathf.Min(1440, viewport.X - 64);
        float bodyWidth = width - 48 - (_portrait ? 216 : 0);
        Body.CustomMinimumSize = new(bodyWidth, 0); Body.CustomMaximumSize = new(bodyWidth, -1);
        Body.Size = new(bodyWidth, 0);
        float bodyHeight = Body.GetMinimumSize().Y;
        Hint.Text = _hint;
        Hint.CustomMaximumSize = new(bodyWidth, -1); Hint.Size = new(bodyWidth, 0);
        float hintHeight = Mathf.Max(34, Hint.GetMinimumSize().Y);
        float footer = hintHeight + 16 + (Choices.Visible ? 60 : 0);
        float height = Mathf.Clamp(Mathf.Max(_portrait ? 304 : 280, bodyHeight + 48 + footer), 280, viewport.Y - 64);
        Size = new(width, height);
        if (!_placed)
        {
            var bottom = new Rect2(new((viewport.X - width) / 2, viewport.Y - 32 - height), Size);
            var top = new Rect2(new((viewport.X - width) / 2, 32), Size);
            _top = _subject.HasArea() && bottom.Intersects(_subject.Grow(16)) && !top.Intersects(_subject.Grow(16));
            _placed = true;
        }
        Position = new((viewport.X - width) / 2, _top ? 32 : viewport.Y - 32 - height);
        _frame.Size = Size;
        _scroll.Position = new(_portrait && !_right ? 240 : 24, 24);
        _scroll.Size = new(bodyWidth, height - 48 - footer);
        Hint.Position = new(_scroll.Position.X, height - 24 - hintHeight); Hint.Size = new(bodyWidth, hintHeight);
        Choices.Position = new(_scroll.Position.X, Hint.Position.Y - 60); Choices.Size = new(bodyWidth, 48);
        if (IsVisibleInTree()) _readThrough = Mathf.Max(_readThrough, _scroll.ScrollVertical + _scroll.Size.Y);
        Hint.Text = _hint;
        Portrait.Visible = _portrait && Portrait.Texture is not null;
        // The source's -10..0 movement remains visible inside its reserved panel column.
        Portrait.Position = new(_right ? width - 216 : 24, 24 + Mathf.Clamp(_portraitY, -10, 0) * 2);
        Portrait.Size = new(192, 256);
    }
    internal Rect2 MountPortrait(Texture2D texture, float sourceY)
    {
        Portrait.Texture = texture; _portraitY = sourceY; if (IsVisibleInTree()) Arrange();
        return Portrait.GetGlobalRect();
    }
    internal void SetHint(string hint) { _hint = hint; if (IsVisibleInTree()) Arrange(); }
    internal void ResetReading() { _scroll.ScrollVertical = 0; _readThrough = 0; }
    internal bool ReadMore()
    {
        if (!MoreBelow) return false;
        _scroll.ScrollVertical += (int)Mathf.Max(1, _scroll.Size.Y - Body.GetLineHeight() - Mathf.Max(0, Body.GetThemeConstant("line_spacing")));
        return true;
    }
    internal object Observe() => new
    {
        visible = IsVisibleInTree(), placement = _top ? "top" : "bottom",
        panel = BattleMapViewport.Rectangle(GetGlobalRect()), body = BattleMapViewport.Rectangle(_scroll.GetGlobalRect()),
        content = BattleMapViewport.Rectangle(Body.GetGlobalRect()), lines = Body.GetLineCount(), lineHeight = Body.GetLineHeight(),
        fontSize = Body.GetThemeFontSize("font_size"), scroll = _scroll.ScrollVertical, readThrough = _readThrough, moreBelow = MoreBelow,
        portrait = BattleMapViewport.Rectangle(Portrait.GetGlobalRect()), portraitVisible = Portrait.IsVisibleInTree(),
        portraitAboveFrame = Portrait.GetIndex() > _frame.GetIndex(), portraitTexture = Portrait.Texture?.GetInstanceId().ToString(),
        choice = BattleMapViewport.Rectangle(Choices.GetGlobalRect()), choicesVisible = Choices.IsVisibleInTree(),
        hint = Hint.Text, hintRect = BattleMapViewport.Rectangle(Hint.GetGlobalRect()), speakerName = (string?)null,
    };
}
