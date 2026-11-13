using System.Text;
using Godot;

namespace Sf2.Remake.GodotAdapter.UI;

internal static class StoryTheme
{
    private static FontFile? _font;
    internal static void PrepareFont(string required)
    {
        var font = _font ?? GD.Load<FontFile>("res://fonts/localization/NotoSansCJKsc-Regular.otf");
        if (font is null) throw new ArgumentException();
        font.AllowSystemFallback = false;
        if (required.EnumerateRunes().Any(rune => !char.IsControl((char)rune.Value) && !font.HasChar(rune.Value)))
            throw new ArgumentException();
        _font = font;
    }
    internal const int BodySize = 30, HintSize = 24;
    internal static readonly Color Selected = new("ffe19a");
    internal static readonly Color Unavailable = new("8e9bab");
    // Journey rows reserve marker space. Standalone actions share the square states.
    internal static Theme CreateEntry(bool journey = false)
    {
        var theme = Create();
        theme.SetStylebox("panel", "TooltipPanel", theme.GetStylebox("panel", "PanelContainer"));
        theme.SetFontSize("font_size", "TooltipLabel", HintSize);
        theme.SetColor("font_color", "TooltipLabel", theme.GetColor("font_color", "Label"));
        theme.SetStylebox("normal", "Button", Box("102c50", "607d9f"));
        theme.SetStylebox("hover", "Button", Box("204c76", "d4c5a0"));
        theme.SetStylebox("pressed", "Button", Box("354c64", "ffe19a"));
        theme.SetStylebox("disabled", "Button", Box("142235", "435267"));
        theme.SetStylebox("focus", "Button", Focus());
        theme.SetColor("font_color", "Button", new("f4f5fc"));
        theme.SetColor("font_hover_color", "Button", Colors.White);
        theme.SetColor("font_pressed_color", "Button", Selected);
        theme.SetColor("font_disabled_color", "Button", Unavailable);
        theme.SetTypeVariation("EntrySelectedButton", "Button");
        theme.SetStylebox("normal", "EntrySelectedButton", Box("294966", "ffe19a"));
        theme.SetStylebox("hover", "EntrySelectedButton", Box("365d7c", "ffe19a"));
        theme.SetColor("font_color", "EntrySelectedButton", Selected);
        theme.SetStylebox("normal", "LineEdit", Box("081b35", "607d9f"));
        theme.SetStylebox("focus", "LineEdit", Focus());
        theme.SetColor("font_color", "LineEdit", Colors.White);
        theme.SetColor("font_placeholder_color", "LineEdit", new("a7b9cf"));
        if (journey)
        {
            theme.SetStylebox("normal", "Button", Box("102c50", "536c87", borderWidth: 1));
            theme.SetStylebox("normal", "EntrySelectedButton", Box("273d56", "ffe19a", borderWidth: 3));
            foreach (string type in new[] { "Button", "EntrySelectedButton" })
                foreach (string state in new[] { "normal", "hover", "pressed", "disabled" })
                    if (theme.HasStylebox(state, type) && theme.GetStylebox(state, type) is StyleBoxFlat box)
                    { box.ContentMarginLeft = 40; box.ContentMarginRight = 20; }
        }
        return theme;
    }
    internal static void Select(Button button, bool selected)
    {
        button.ThemeTypeVariation = selected ? "EntrySelectedButton" : "";
        if (button.GetNodeOrNull<Label>("FocusMarker") is { } marker) marker.Visible = selected;
    }
    internal static void EntryChoice(Button button)
    {
        button.Alignment = HorizontalAlignment.Left;
        // CreateEntry(journey: true) reserves the same marker column in every state.
        var marker = new Label { Name = "FocusMarker", Text = "›", MouseFilter = Control.MouseFilterEnum.Ignore,
            VerticalAlignment = VerticalAlignment.Center, Visible = false };
        button.AddChild(marker);
        marker.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.LeftWide);
        marker.OffsetLeft = 12; marker.OffsetRight = 36;
        marker.AddThemeColorOverride("font_color", Selected);
    }
    private static StyleBoxFlat Box(string background, string border, float horizontal = 12, float vertical = 10,
        int borderWidth = 2) => new()
    {
        BgColor = new(background), BorderColor = new(border),
        BorderWidthLeft = borderWidth, BorderWidthTop = borderWidth, BorderWidthRight = borderWidth, BorderWidthBottom = borderWidth,
        ContentMarginLeft = horizontal, ContentMarginRight = horizontal, ContentMarginTop = vertical, ContentMarginBottom = vertical,
    };
    private static StyleBoxFlat Focus() => new()
    {
        BgColor = Colors.Transparent, BorderColor = Selected,
        BorderWidthLeft = 3, BorderWidthTop = 3, BorderWidthRight = 3, BorderWidthBottom = 3,
    };
    internal static Theme Create()
    {
        var theme = new Theme { DefaultFontSize = BodySize, DefaultFont = _font };
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
