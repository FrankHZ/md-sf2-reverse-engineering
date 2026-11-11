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
    // Entry and Save/Quit opt in; other story and gameplay menus keep their accepted treatment.
    internal static Theme CreateEntry(bool journey = false)
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
            // Production F1 opts in; shared Save/Quit and other scenes retain their treatment.
            foreach (string type in new[] { "Button", "EntrySelectedButton", "LineEdit" })
                foreach (string state in new[] { "normal", "hover", "pressed", "disabled", "focus" })
                    if (theme.HasStylebox(state, type) && theme.GetStylebox(state, type) is StyleBoxFlat box)
                    {
                        box.CornerRadiusTopLeft = box.CornerRadiusTopRight = 0;
                        box.CornerRadiusBottomLeft = box.CornerRadiusBottomRight = 0;
                    }
            theme.SetStylebox("normal", "Button", Box("102c50", "536c87", radius: 0, borderWidth: 1));
            theme.SetStylebox("normal", "EntrySelectedButton", Box("273d56", "ffe19a", radius: 0, borderWidth: 3));
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
        int radius = 8, int borderWidth = 2) => new()
    {
        BgColor = new(background), BorderColor = new(border),
        BorderWidthLeft = borderWidth, BorderWidthTop = borderWidth, BorderWidthRight = borderWidth, BorderWidthBottom = borderWidth,
        CornerRadiusTopLeft = radius, CornerRadiusTopRight = radius, CornerRadiusBottomLeft = radius, CornerRadiusBottomRight = radius,
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
