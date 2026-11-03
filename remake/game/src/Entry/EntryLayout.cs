using Godot;

namespace Sf2.Remake.GodotAdapter.Entry;

// The 1080p canvas scales/letterboxes through the existing project viewport policy.
internal static class EntryLayout
{
    internal static readonly Rect2 Window = new(1216, 64, 640, 952);
    internal static readonly Rect2 Greeting = new(1216, 64, 640, 440);
}
