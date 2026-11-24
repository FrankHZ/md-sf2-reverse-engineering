using Sf2.Remake.Application.Runtime;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    internal void LoadCaravanPreview()
    {
        const string selection = "SF2_CARAVAN_PREVIEW_SEED";
        var seed = System.Environment.GetEnvironmentVariable(selection);
        if (seed is null || !Path.IsPathFullyQualified(seed))
        {
            Fail(new(SessionFailureKind.ContentError, "caravan-preview-seed", selection,
                "Select the absolute path of the registered Caravan approach save."));
            return;
        }

        // Continue reads the seed afresh. The restored field admits Talk through normal input.
        _seedFile = seed;
        ShowContinue(loadSeed: true);
    }
}
