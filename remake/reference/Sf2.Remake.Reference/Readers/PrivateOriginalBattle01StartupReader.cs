using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;

namespace Sf2.Remake.Content;

// Transitional port only: production Content owns file trust, parsing and terrain decoding.
public sealed class PrivateOriginalBattle01StartupReader : IOriginalBattle01StartupSource
{
    public const string CompressedTerrainDigest = PrivateBattleEncounterReader.CompressedTerrainDigest;
    private readonly PrivateBattleEncounterReader _reader;
    public PrivateOriginalBattle01StartupReader(string placementPath, string scenePath, string terrainPath)
    { _reader = new(placementPath, scenePath, terrainPath); }

    public OriginalBattle01StartupImportResult Admit() => Project(_reader.Read(), requireComparison: true);

    internal static OriginalBattle01StartupImportResult AdmitSemanticDocumentsForTests(
        byte[] placement, byte[] scene, byte[] decodedTerrain) =>
        Project(PrivateBattleEncounterReader.Decode(placement, scene, decodedTerrain), requireComparison: false);

    private static OriginalBattle01StartupImportResult Project(BattleEncounterReadResult result, bool requireComparison)
    {
        if (result is BattleEncounterReadRejected rejected)
            return new OriginalBattle01StartupImportRejected(new(rejected.Diagnostic.Field, rejected.Diagnostic.Message));
        var source = ((BattleEncounterReadAccepted)result).Definition;
        try
        {
            PrivateBattleEncounterReader.RequireSelectedSource(source);
            var definition = OriginalBattle01StartupDefinition.Project(source);
            return requireComparison && definition.GetAdmissionDiagnostic() is { } failure
                ? new OriginalBattle01StartupImportRejected(failure) : new OriginalBattle01StartupImported(definition);
        }
        catch (PrivateBattleEncounterReader.EncounterReadException error)
        { return new OriginalBattle01StartupImportRejected(new(error.Diagnostic.Field, error.Diagnostic.Message)); }
        catch (ArgumentException error)
        { return new OriginalBattle01StartupImportRejected(new(error.ParamName ?? "definition", "Unsupported selected Battle01 comparison projection.")); }
    }
}
