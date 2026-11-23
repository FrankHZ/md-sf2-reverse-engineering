using Godot;

namespace Sf2.Remake.GodotAdapter.UI;

internal static class BattleCardFrame
{
    // Project-authored frame. The corners retain their shape as the card widens.
    internal static StyleBoxTexture Create()
    {
        const int size = 48, corner = 22;
        using var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        image.Fill(new("512f22"));
        image.FillRect(new(2, 2, 44, 44), new("b67836"));
        image.FillRect(new(4, 4, 40, 40), new("ffe19a"));
        image.FillRect(new(6, 6, 36, 36), new("102444"));
        image.FillRect(new(8, 8, 32, 32), new("80572d"));
        image.FillRect(new(10, 10, 28, 28), new("08275b"));
        var highlight = new Color("f4c572");
        foreach (bool right in new[] { false, true })
        foreach (bool bottom in new[] { false, true })
        {
            Accent(1, 1, 16, 3);
            Accent(1, 1, 3, 16);
            Accent(6, 6, 6, 3);
            Accent(6, 6, 3, 6);

            void Accent(int x, int y, int width, int height) =>
                image.FillRect(new(right ? size - x - width : x, bottom ? size - y - height : y,
                    width, height), highlight);
        }
        return new StyleBoxTexture
        {
            Texture = ImageTexture.CreateFromImage(image),
            TextureMarginLeft = corner, TextureMarginRight = corner,
            TextureMarginTop = corner, TextureMarginBottom = corner,
            ContentMarginLeft = 18, ContentMarginRight = 18,
            ContentMarginTop = 14, ContentMarginBottom = 14,
        };
    }
}
