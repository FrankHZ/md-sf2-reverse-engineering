using Godot;

namespace Sf2.Remake.GodotAdapter.UI;

internal static class StoryTheme
{
    internal const int BodySize = 30, HintSize = 24;
    internal static readonly Color Selected = new("ffe19a");
    internal static Theme Create()
    {
        var theme = new Theme { DefaultFontSize = BodySize };
        theme.SetColor("font_color", "Label", new Color("f4f5fc"));
        theme.SetStylebox("panel", "PanelContainer", new StyleBoxFlat
        {
            BgColor = new("08275bf5"), BorderColor = Selected,
            BorderWidthLeft = 2, BorderWidthTop = 2, BorderWidthRight = 2, BorderWidthBottom = 2,
            ShadowColor = new("05122bb0"), ShadowSize = 3,
            ContentMarginLeft = 14, ContentMarginTop = 12, ContentMarginRight = 14, ContentMarginBottom = 12,
        });
        theme.SetStylebox("panel", "Panel", theme.GetStylebox("panel", "PanelContainer"));
        return theme;
    }
}
