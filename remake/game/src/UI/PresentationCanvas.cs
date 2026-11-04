using Godot;

namespace Sf2.Remake.GodotAdapter.UI;

// Three viewport-sized composition boundaries, not an input or scene owner.
// World-local Z stays on canvas 0; it cannot escape into these UI canvases.
internal sealed partial class PresentationCanvas : CanvasLayer
{
    internal const int Battle = 1, Field = 2, Host = 3;
    private Control? _owner;

    internal static Control Mount(Node owner, string name, int layer)
    {
        var canvas = new PresentationCanvas { Name = name, Layer = layer, _owner = owner as Control };
        var content = new Control { Name = "Content", MouseFilter = Control.MouseFilterEnum.Ignore };
        canvas.AddChild(content);
        owner.AddChild(canvas);
        content.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        return content;
    }

    public override void _EnterTree()
    {
        if (_owner is not null) _owner.VisibilityChanged += SyncVisibility;
        SyncVisibility();
    }

    public override void _Ready() => SyncVisibility();

    private void SyncVisibility() => Visible = _owner?.IsVisibleInTree() ?? true;

    public override void _ExitTree()
    {
        if (_owner is not null) _owner.VisibilityChanged -= SyncVisibility;
    }
}
