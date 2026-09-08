using Sf2.Remake.Application.Content;

namespace Sf2.Remake.Application.Sessions;

public abstract record PrivateOriginalBattle01StartupResult;
public sealed record PrivateOriginalBattle01StartupPrepared : PrivateOriginalBattle01StartupResult
{
    internal PrivateOriginalBattle01StartupPrepared(PrivateOriginalBattle01PendingAdmission pending,
        OriginalBattle01StartupDefinition inputs, OriginalBattle01ControlledPartyPreset party)
    {
        Pending = pending; Inputs = inputs; Party = party;
    }
    public PrivateOriginalBattle01PendingAdmission Pending { get; }
    public OriginalBattle01StartupDefinition Inputs { get; }
    public OriginalBattle01ControlledPartyPreset Party { get; }
}
public sealed record PrivateOriginalBattle01StartupRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01StartupResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01StartupResult PreparePrivateOriginalBattle01Startup(
        PrivateOriginalBattle01PendingAdmission? pending, IOriginalBattle01StartupSource? source,
        OriginalBattle01ControlledPartyPreset? party)
    {
        if (PrivateOriginalBattle01 is not null)
            return Rejected("battle", "Battle 01 has already initialized; exploration startup is closed.");
        if (pending is null || PrivateOriginalBattle01Admission is null)
            return Rejected("pending", "A current Battle 01 pending admission is required.");
        if (!ReferenceEquals(pending, PrivateOriginalBattle01Admission) ||
            !ReferenceEquals(pending.SourceSnapshot, PrivateOriginalMapSnapshot))
            return Rejected("pending", "The request must name this session's exact current pending admission.");
        if (source is null) return Rejected("source", "An explicit startup source is required.");
        if (party is null) return Rejected("party", "The named controlled comparison party and RNG input are required.");
        if (party.GetAdmissionDiagnostic() is { } partyFailure)
            return new PrivateOriginalBattle01StartupRejected(partyFailure);
        OriginalBattle01StartupImportResult imported = source.Admit();
        if (imported is OriginalBattle01StartupImportRejected failure)
            return new PrivateOriginalBattle01StartupRejected(failure.Diagnostic);
        if (imported is not OriginalBattle01StartupImported { Definition: not null } accepted)
            return Rejected("source", "The source did not return startup inputs.");
        if (accepted.Definition.GetAdmissionDiagnostic() is { } inputFailure)
            return new PrivateOriginalBattle01StartupRejected(inputFailure);
        // No cache or battle state is installed. Repeated valid preparation retains the same pending.
        return new PrivateOriginalBattle01StartupPrepared(pending, accepted.Definition, party);
    }

    private static PrivateOriginalBattle01StartupRejected Rejected(string field, string message) => new(new(field, message));
}
