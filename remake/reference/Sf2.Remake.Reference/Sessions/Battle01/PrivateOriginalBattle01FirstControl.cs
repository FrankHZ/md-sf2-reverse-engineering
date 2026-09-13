using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public abstract record PrivateOriginalBattle01FirstControlResult;
public sealed record PrivateOriginalBattle01FirstControlEntered(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01FirstControlResult;
public sealed record PrivateOriginalBattle01FirstControlUnavailable(Battle01FirstControlDecision Decision)
    : PrivateOriginalBattle01FirstControlResult;
public sealed record PrivateOriginalBattle01FirstControlRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01FirstControlResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01FirstControlResult EnterPrivateOriginalBattle01FirstControl(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return ControlRejected("battle", "A current Battle01 snapshot is required.");
        if (expected is null || !ReferenceEquals(expected, current))
            return ControlRejected("snapshot", "The request must name this session's exact current battle snapshot.");
        Battle01FirstControlTransition transition;
        try { transition = Battle01FirstControl.Enter(current.Battle, actorIndex); }
        catch (ArgumentException error)
        {
            return ControlRejected(error.ParamName ?? "control", "The current battle cannot enter first player control.");
        }
        if (transition.State is null) return new PrivateOriginalBattle01FirstControlUnavailable(transition.Decision);
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, transition.State,
            current.SourceLocomotion, current.SourceBridge);
        var result = new PrivateOriginalBattle01FirstControlEntered(next);
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static PrivateOriginalBattle01FirstControlRejected ControlRejected(string field, string message) => new(new(field, message));
}
