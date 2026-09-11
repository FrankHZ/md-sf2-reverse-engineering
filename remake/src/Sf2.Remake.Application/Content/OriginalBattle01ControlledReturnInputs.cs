namespace Sf2.Remake.Application.Content;

public sealed record OriginalBattle01ControlledReturnInputs(string Id, byte EgressMap, bool Flag64, bool Flag640)
{
    public const string GransealFirstAttemptComparisonId = "battle01-granseal-first-attempt-return-comparison";
    public static OriginalBattle01ControlledReturnInputs GransealFirstAttemptComparison { get; } =
        new(GransealFirstAttemptComparisonId, 3, false, false);

    public OriginalBattle01StartupDiagnostic? GetAdmissionDiagnostic() =>
        Id != GransealFirstAttemptComparisonId || EgressMap != 3 || Flag64 || Flag640
            ? new("return.inputs", "The explicit Granseal first-attempt egress3/F64false/F640false comparison is required.")
            : null;
}
