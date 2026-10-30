using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.Content.Scenarios;

// Product entry has content and two seeds, never an external checkpoint/start file.
public sealed class OpeningNewGameReader(string worldPath, string placementPath, string scenePath, string terrainPath,
    string staticPath, string enemyPath, string goldPath, uint mainSeed, uint thinkingSeed,
    string? battleSceneContentPath = null) : IScenarioSource
{
    public ScenarioReadResult Read() => new PrivateExplorationReader(worldPath,
        new PrivateBattleContentReader(placementPath, scenePath, terrainPath, staticPath, enemyPath, goldPath,
            mainSeed, thinkingSeed, battleSceneContentPath)).Read();
}
