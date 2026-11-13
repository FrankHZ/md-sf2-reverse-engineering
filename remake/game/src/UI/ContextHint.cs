using Godot;

namespace Sf2.Remake.GodotAdapter.UI;

internal sealed partial class ContextHint : PanelContainer
{
    internal Label Text { get; } = new() { Name = "Help", AutowrapMode = TextServer.AutowrapMode.WordSmart,
        SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
    internal Label? ActionHint { get; init; }

    public override void _Ready()
    {
        Theme = StoryTheme.Create();
        MouseFilter = MouseFilterEnum.Ignore;
        Text.AddThemeFontSizeOverride("font_size", StoryTheme.HintSize);
        if (ActionHint is null) { AddChild(Text); return; }

        var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        row.AddThemeConstantOverride("separation", 24);
        AddChild(row);
        row.AddChild(Text);
        row.AddChild(ActionHint);
        ActionHint.HorizontalAlignment = HorizontalAlignment.Right;
        ActionHint.VerticalAlignment = VerticalAlignment.Center;
    }

    internal void Arrange(Vector2 viewport)
    {
        float width = Mathf.Min(1440, viewport.X - 64);
        float height;
        if (ActionHint is null)
        {
            Text.CustomMaximumSize = new(width - 32, -1);
            height = Text.GetMinimumSize().Y + 24;
        }
        else height = GetCombinedMinimumSize().Y;
        Size = new(width, Mathf.Max(64, height));
        Position = new((viewport.X - Size.X) / 2, viewport.Y - 32 - Size.Y);
        Visible = ActionHint?.Visible == true || !string.IsNullOrWhiteSpace(Text.Text);
    }
}
