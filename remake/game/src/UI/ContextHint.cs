using Godot;

namespace Sf2.Remake.GodotAdapter.UI;

internal sealed partial class ContextHint : PanelContainer
{
    internal Label Text { get; } = new() { Name = "Help", AutowrapMode = TextServer.AutowrapMode.WordSmart,
        MouseFilter = MouseFilterEnum.Ignore };
    public override void _Ready()
    {
        Theme = StoryTheme.Create(); MouseFilter = MouseFilterEnum.Ignore;
        Text.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
        AddChild(Text);
    }
    internal void Arrange(Vector2 viewport)
    {
        float width = Mathf.Min(1440, viewport.X - 64);
        Text.CustomMaximumSize = new(width - 32, -1);
        Size = new(width, Mathf.Max(64, Text.GetMinimumSize().Y + 24));
        Position = new((viewport.X - width) / 2, viewport.Y - 32 - Size.Y);
        Visible = !string.IsNullOrWhiteSpace(Text.Text);
    }
}
