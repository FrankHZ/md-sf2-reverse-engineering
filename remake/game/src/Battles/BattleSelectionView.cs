using Godot;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

// A steady modern focus cue, separate from sprite colors and legal-stop terrain.
// Source control moves the actor; its menu blink is not a persistent gold tint.
internal sealed partial class BattleSelectionView : Control
{
    private ActorRef? _actor;
    private int _draws;
    private const float Inset = 7;
    private const float Leg = 16;
    private const float OutlineWidth = 7;
    private const float StrokeWidth = 3;
    private static readonly Color Outline = new(0.04f, 0.06f, 0.10f);

    public override void _Ready()
    {
        Size = Vector2.One * WorldViewportLayout.CellSize;
        MouseFilter = MouseFilterEnum.Ignore;
        ZIndex = 3;
    }

    internal void Present(BattleActorMarker? actor)
    {
        _actor = actor?.Actor;
        Visible = actor is not null;
        if (actor is not null)
            Position = new Vector2(actor.Position.X, actor.Position.Y) * WorldViewportLayout.CellSize;
        QueueRedraw();
    }

    internal void FollowMovement(ActorRef actor, Vector2 position)
    {
        if (_actor == actor) Position = position;
    }

    public override void _Draw()
    {
        if (_actor is null) return;
        _draws++;
        foreach (float x in new[] { Inset, Size.X - Inset })
            foreach (float y in new[] { Inset, Size.Y - Inset })
            {
                var corner = new Vector2(x, y);
                var horizontal = corner + new Vector2(x < Size.X / 2 ? Leg : -Leg, 0);
                var vertical = corner + new Vector2(0, y < Size.Y / 2 ? Leg : -Leg);
                DrawLine(corner, horizontal, Outline, OutlineWidth);
                DrawLine(corner, vertical, Outline, OutlineWidth);
                DrawLine(corner, horizontal, Colors.White, StrokeWidth);
                DrawLine(corner, vertical, Colors.White, StrokeWidth);
            }
    }

    internal object Observe() => new
    {
        actor = _actor?.Value, visible = IsVisibleInTree(), z = ZIndex, draws = _draws,
        rect = BattleMapViewport.Rectangle(new Rect2(Position, Size)),
        globalRect = BattleMapViewport.Rectangle(GetGlobalRect()),
        inset = Inset, leg = Leg, outlineWidth = OutlineWidth, strokeWidth = StrokeWidth,
        stroke = Colors.White.ToHtml(), outline = Outline.ToHtml(),
    };
}
