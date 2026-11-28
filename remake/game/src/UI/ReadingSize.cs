using Godot;

namespace Sf2.Remake.GodotAdapter.UI;

internal static class ReadingSize
{
    // Label variants inherit the presenter's theme, including for rows created after Apply.
    private static readonly int[] LabelSizes = [24, 26, 28, 30, 32];
    internal static string Role(int size) => $"ReadingLabel{size}";
    internal static int Pixels(int standard, bool large, bool secondary = false) =>
        large ? Mathf.RoundToInt(standard * (secondary ? 1.08f : 1.2f)) : standard;

    internal static void Configure(Theme theme, bool large)
    {
        theme.DefaultFontSize = Pixels(StoryTheme.BodySize, large);
        foreach (int standard in LabelSizes)
        {
            var role = Role(standard);
            theme.SetTypeVariation(role, "Label");
            theme.SetFontSize("font_size", role, Pixels(standard, large));
        }
    }

    // Bind once when the presenter is ready. Field/battle transfers can detach and re-enter
    // the same view, and a preview can host another GameRoot in a separate viewport.
    internal static void Follow(Control view, Action<bool> apply)
    {
        GameRoot? host = null;
        void Enter()
        {
            for (Node? node = view.GetParent(); node is not null; node = node.GetParent())
                if (node is GameRoot owner) { host = owner; break; }
            if (host is not null) host.ReadingSizeChanged += apply;
            apply(host?.LargeTextEnabled ?? true);
        }
        void Exit()
        {
            if (host is not null) host.ReadingSizeChanged -= apply;
            host = null;
        }
        view.TreeEntered += Enter;
        view.TreeExiting += Exit;
        Enter();
    }
}
