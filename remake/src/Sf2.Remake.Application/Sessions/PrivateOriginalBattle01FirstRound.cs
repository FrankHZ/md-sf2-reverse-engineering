using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Sessions;

public abstract record PrivateOriginalBattle01FirstRoundResult;
public sealed record PrivateOriginalBattle01FirstRoundEntered(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01FirstRoundResult;
public sealed record PrivateOriginalBattle01FirstRoundRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01FirstRoundResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01FirstRoundResult EnterPrivateOriginalBattle01FirstRound(
        PrivateOriginalBattle01SessionSnapshot? expected)
    {
        var current = PrivateOriginalBattle01;
        if (current is null) return RoundRejected("battle", "A current initialized Battle 01 is required.");
        if (current.Battle.Phase != Battle01Phase.BeforeFirstRound)
            return RoundRejected("phase", "The first round is already generated; actor control has not been entered.");
        if (expected is null || !ReferenceEquals(expected, current))
            return RoundRejected("snapshot", "The request must name this session's exact current initialized battle snapshot.");
        Battle01InitializedState battle;
        try { battle = Battle01FirstRound.Enter(current.Battle); }
        catch (ArgumentException error)
        {
            return RoundRejected("round." + (error.ParamName ?? "state"),
                "The initialized battle cannot enter the bounded first-round transition.");
        }
        var next = new PrivateOriginalBattle01SessionSnapshot(current.Preparation, battle,
            current.SourceLocomotion, current.SourceBridge);
        var applied = new PrivateOriginalBattle01FirstRoundEntered(next);
        // All projection and allocation precedes the single authoritative snapshot replacement.
        PrivateOriginalBattle01 = next;
        return applied;
    }

    private static PrivateOriginalBattle01FirstRoundRejected RoundRejected(string field, string message) =>
        new(new(field, message));
}
