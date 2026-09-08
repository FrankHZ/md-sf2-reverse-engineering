using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public abstract record PrivateOriginalBattle01NextPlayerControlResult;
public sealed record PrivateOriginalBattle01NextPlayerControlEntered(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01NextPlayerControlResult;
public sealed record PrivateOriginalBattle01NextPlayerControlUnavailable(Battle01FirstControlDecision Decision)
    : PrivateOriginalBattle01NextPlayerControlResult;
public sealed record PrivateOriginalBattle01NextPlayerControlRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01NextPlayerControlResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01NextPlayerControlResult EnterPrivateOriginalBattle01NextPlayerControl(
        PrivateOriginalBattle01SessionSnapshot? expected, int actorIndex)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return NextControlRejected("battle");
        if (expected is null || !ReferenceEquals(expected, current)) return NextControlRejected("snapshot");
        Battle01FirstControlTransition transition;
        try { transition = Battle01NextPlayerControl.Enter(current.Battle, actorIndex); }
        catch (ArgumentException error) { return NextControlRejected(error.ParamName ?? "control"); }
        if (transition.State is null) return new PrivateOriginalBattle01NextPlayerControlUnavailable(transition.Decision);
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, transition.State,
            current.SourceLocomotion, current.SourceBridge);
        var result = new PrivateOriginalBattle01NextPlayerControlEntered(next);
        PrivateOriginalBattle01 = next;
        return result;
    }

    private static PrivateOriginalBattle01NextPlayerControlRejected NextControlRejected(string field) =>
        new(new(field, "Next player control cannot enter this snapshot (" + field + "); committed STAY is retained."));
}
