using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;

namespace Sf2.Remake.Content.Persistence;

// Admits independently configured immutable content. The saved payload selects no paths or trust roots.
public sealed class OpeningRestoreReader(string worldPath, string placementPath, string scenePath, string terrainPath,
    string staticPath, string enemyPath, string goldPath, string? battleSceneContentPath = null) : IScenarioSource
{
    public ScenarioReadResult Read() => new PrivateExplorationReader(worldPath,
        new PrivateBattleContentReader(placementPath, scenePath, terrainPath, staticPath, enemyPath, goldPath,
            0, 0, battleSceneContentPath), initializeParty: false).Read();
}
