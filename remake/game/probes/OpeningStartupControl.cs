using System.Reflection;
using Godot;
using Sf2.Remake.GodotAdapter;

// Controlled verification operands only. The ordinary executable has no seed override.
public partial class OpeningStartupControl : Node
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private object? _retiredOwner;
    public void RetainOwner(Node host) => _retiredOwner = typeof(GameRoot).GetField("_opening", Private)!.GetValue(host);
    public void DeliverRetiredCompletion(Node host)
    {
        if (_retiredOwner is null) throw new InvalidOperationException("No retained opening owner");
        typeof(GameRoot).GetMethod("FinishOpening", Private)!.Invoke(host, [_retiredOwner, "completed", null]);
    }
    public bool RetiredLoadCompleted() => _retiredOwner?.GetType().GetField("Loading", Private)!.GetValue(_retiredOwner)
        is not Task task || task.IsCompleted;
    public void SetNewGameSeeds(Node host)
    {
        if (host.FindChild("NameEntryView", true, false) is null)
            throw new InvalidOperationException("Set controlled seeds at the ordinary naming boundary");
        typeof(GameRoot).GetField("_newMainSeed", Private)!.SetValue(host, (uint?)0x12345678);
        typeof(GameRoot).GetField("_newThinkingSeed", Private)!.SetValue(host, (uint?)0x9abcdef0);
    }
}
