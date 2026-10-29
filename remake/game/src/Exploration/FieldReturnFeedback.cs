using Sf2.Remake.Application.Runtime;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Ephemeral notice from an actual outcome event; never a second result/resource authority.
internal sealed class FieldReturnFeedback
{
    private string? _outcome;
    internal void Begin(SessionResult result)
    {
        _outcome = result.Observations.FirstOrDefault(row => row.Kind == "outcome-program-started")?.Detail switch
        { "Victory" => "Victory", "Defeat" => "Defeat", _ =>
            result.Observations.Any(row => row.Kind == "retreat-exploration-entry") ? "Returned from battle" : null };
    }
    internal string Notice(SessionSnapshot current) => _outcome is null ? "" :
        current.CanWaitAtInput ? $"{_outcome}. Back in the field." : $"{_outcome}. Returning to the field…";
    internal void Dismiss() => _outcome = null;
}
