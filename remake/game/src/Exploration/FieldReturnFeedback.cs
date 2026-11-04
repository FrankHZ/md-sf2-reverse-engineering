using Sf2.Remake.Application.Runtime;
using Sf2.Remake.GodotAdapter.Localization;

namespace Sf2.Remake.GodotAdapter.Exploration;

// Ephemeral notice from an actual outcome event; never a second result/resource authority.
internal sealed class FieldReturnFeedback
{
    private enum Outcome { Victory, Defeat, Retreat }
    private Outcome? _outcome;
    internal void Begin(SessionResult result)
    {
        _outcome = result.Observations.FirstOrDefault(row => row.Kind == "outcome-program-started")?.Detail switch
        { "Victory" => Outcome.Victory, "Defeat" => Outcome.Defeat, _ =>
            result.Observations.Any(row => row.Kind == "retreat-exploration-entry") ? Outcome.Retreat : null };
    }
    internal string Notice(SessionSnapshot current) => _outcome is null ? "" :
        GameText.Get((_outcome, current.CanWaitAtInput) switch
        {
            (Outcome.Victory, true) => "field.return.victory.ready",
            (Outcome.Victory, false) => "field.return.victory.pending",
            (Outcome.Defeat, true) => "field.return.defeat.ready",
            (Outcome.Defeat, false) => "field.return.defeat.pending",
            (_, true) => "field.return.retreat.ready",
            _ => "field.return.retreat.pending",
        });
    internal void Dismiss() => _outcome = null;
}
