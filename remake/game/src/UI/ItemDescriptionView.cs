using Godot;
using Sf2.Remake.GodotAdapter.Battles;

namespace Sf2.Remake.GodotAdapter.UI;

// The reading body's minimum size is independent of the current selection and text.
internal sealed partial class ItemDescriptionView : ScrollContainer
{
    private readonly Label _text = new()
    {
        Name = "Body", AutowrapMode = TextServer.AutowrapMode.WordSmart,
        SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore,
    };

    internal const int BodySize = 30;
    internal int ReadingHeight { get; init; } = 196;

    public override void _Ready()
    {
        Name = "ItemDescription"; MouseFilter = MouseFilterEnum.Ignore; FocusMode = FocusModeEnum.None;
        HorizontalScrollMode = ScrollMode.Disabled;
        CustomMinimumSize = new(0, ReadingHeight);
        GetVScrollBar().FocusMode = FocusModeEnum.None;
        _text.AddThemeFontSizeOverride("font_size", BodySize);
        AddChild(_text);
    }

    internal void Present(string? text)
    {
        Visible = text is not null;
        if (_text.Text == (text ?? "")) return;
        _text.Text = text ?? "";
        ScrollVertical = 0;
    }

    internal bool Wheel(Vector2 point, int step)
    {
        if (!IsVisibleInTree() || !GetGlobalRect().HasPoint(point)) return false;
        ScrollVertical += step;
        return true;
    }

    internal object Observe() => new
    {
        visible = IsVisibleInTree(), text = _text.Text, fontSize = _text.GetThemeFontSize("font_size"),
        rect = BattleMapViewport.Rectangle(GetGlobalRect()), minimumHeight = _text.GetMinimumSize().Y,
        scroll = ScrollVertical,
    };
}
