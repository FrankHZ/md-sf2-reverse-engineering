using Godot;

namespace Sf2.Remake.GodotAdapter.Entry;

// Special-screen pixels have their own integer scale, independent of world cells.
internal sealed partial class EntryBackdrop : Control
{
    internal EntryPresentationAssets? Assets;
    internal bool Witch, Arrival, Speaking, ReducedFlash;
    private double _elapsed;
    internal string Frame { get; private set; } = "title";
    internal Rect2 Art { get; private set; }
    public override void _Ready() { MouseFilter = MouseFilterEnum.Ignore; TextureFilter = TextureFilterEnum.Nearest; }
    public override void _Process(double delta) { _elapsed += delta; QueueRedraw(); }
    public override void _Draw()
    {
        DrawRect(new(Vector2.Zero, GetViewportRect().Size), Colors.Black);
        if (Assets is null) return;
        float scale = Mathf.Max(1, Mathf.Floor(Mathf.Min(GetViewportRect().Size.Y / 224, 4)));
        Vector2 origin = Witch ? new(160, (1080 - 224 * scale) / 2) : new((1920 - 256 * scale) / 2, (1080 - 224 * scale) / 2);
        Art = new(origin, new(256 * scale, 224 * scale));
        if (Witch)
        {
            Frame = Arrival ? "witch-arrival" : !ReducedFlash && _elapsed % 3.4 < .12 ? "witch-blink" :
                Speaking && !ReducedFlash && _elapsed % .22 < .11 ? "witch-mouth" : "witch";
            DrawTextureRect(Assets.Textures[Frame], Art, false);
        }
        else
        {
            Frame = "title";
            float progress = ReducedFlash ? 1 : (float)Math.Min(1, _elapsed / .9);
            DrawTextureRect(Assets.Textures["numeral"], new(Art.Position + new Vector2(0, Mathf.Round((1 - progress) * 24)), Art.Size), false, new Color(1, 1, 1, progress));
            DrawTextureRect(Assets.Textures["title"], Art, false);
        }
    }
}
