using Godot;
namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleInspectionCursor : Control
{
    public override void _Ready() { MouseFilter = MouseFilterEnum.Ignore; QueueRedraw(); }
    public override void _Draw()
    {
        DrawRect(new Rect2(4, 4, 88, 88), Colors.Black, false, 7);
        DrawRect(new Rect2(4, 4, 88, 88), Colors.Cyan, false, 3);
        DrawLine(new(40, 48), new(56, 48), Colors.White, 2);
        DrawLine(new(48, 40), new(48, 56), Colors.White, 2);
    }
}
