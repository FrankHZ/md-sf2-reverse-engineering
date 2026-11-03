using Godot;

namespace Sf2.Remake.GodotAdapter.UI;

internal static class StoryTheme
{
    internal const int BodySize = 30, HintSize = 24;
    internal static readonly Color Selected = new("ffe19a");
    // Entry opts in; existing story and gameplay menus keep their accepted treatment.
    internal static Theme CreateEntry()
    {
        var theme = Create();
        theme.SetStylebox("normal", "Button", Box("102c50", "607d9f"));
        theme.SetStylebox("hover", "Button", Box("204c76", "d4c5a0"));
        theme.SetStylebox("pressed", "Button", Box("354c64", "ffe19a"));
        theme.SetStylebox("disabled", "Button", Box("142235", "435267"));
        theme.SetStylebox("focus", "Button", Focus());
        theme.SetColor("font_color", "Button", new("f4f5fc"));
        theme.SetColor("font_hover_color", "Button", Colors.White);
        theme.SetColor("font_pressed_color", "Button", Selected);
        theme.SetColor("font_disabled_color", "Button", new("8e9bab"));
        theme.SetTypeVariation("EntrySelectedButton", "Button");
        theme.SetStylebox("normal", "EntrySelectedButton", Box("294966", "ffe19a"));
        theme.SetStylebox("hover", "EntrySelectedButton", Box("365d7c", "ffe19a"));
        theme.SetColor("font_color", "EntrySelectedButton", Selected);
        foreach (string type in new[] { "EntryKeyButton", "EntrySelectedKeyButton" })
        {
            theme.SetTypeVariation(type, "Button");
            theme.SetStylebox("normal", type, Box(type == "EntryKeyButton" ? "102c50" : "294966", type == "EntryKeyButton" ? "607d9f" : "ffe19a", 6, 6));
            theme.SetStylebox("hover", type, Box("365d7c", "ffe19a", 6, 6));
            theme.SetStylebox("pressed", type, Box("354c64", "ffe19a", 6, 6));
        }
        theme.SetColor("font_color", "EntrySelectedKeyButton", Selected);
        theme.SetStylebox("normal", "LineEdit", Box("081b35", "607d9f"));
        theme.SetStylebox("focus", "LineEdit", Focus());
        theme.SetColor("font_color", "LineEdit", Colors.White);
        theme.SetColor("font_placeholder_color", "LineEdit", new("a7b9cf"));
        return theme;
    }
    internal static void Select(Button button, bool selected, bool key = false) =>
        button.ThemeTypeVariation = key ? selected ? "EntrySelectedKeyButton" : "EntryKeyButton"
            : selected ? "EntrySelectedButton" : "";
    private static StyleBoxFlat Box(string background, string border, float horizontal = 12, float vertical = 10) => new()
    {
        BgColor = new(background), BorderColor = new(border),
        BorderWidthLeft = 2, BorderWidthTop = 2, BorderWidthRight = 2, BorderWidthBottom = 2,
        CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
        ContentMarginLeft = horizontal, ContentMarginRight = horizontal, ContentMarginTop = vertical, ContentMarginBottom = vertical,
    };
    private static StyleBoxFlat Focus() => new()
    {
        BgColor = Colors.Transparent, BorderColor = Selected,
        BorderWidthLeft = 3, BorderWidthTop = 3, BorderWidthRight = 3, BorderWidthBottom = 3,
        CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
    };
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
