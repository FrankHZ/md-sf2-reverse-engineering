using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleSceneDisplayIdentityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActiveSceneKeepsEnemyTableIdentityWithinItsEncounterAndAllowsOldMetadata(bool metadata)
    {
        var read = Admitted("stone-court");
        var first = read.Definition.Encounters[read.Start.Encounter];
        var second = new BattleDefinition("another-encounter", first.Map, first.Width, first.Height,
            first.Terrain, first.Deployments, first.Spells.Values, first.Rewards);
        var enemy = first.Deployments.First(row => row.Faction == BattleFaction.Enemy).Actor;
        BattleSceneDefinition Scene(string encounter, byte id) => new(encounter, "background", "ground",
            new Dictionary<string, ExplorationRaster>(), new Dictionary<BattleClassRule, BattleSceneActorVisual>(),
            new Dictionary<ActorRef, BattleSceneActorVisual>(), new Dictionary<int, string>(), [],
            EnemyNames: new Dictionary<ActorRef, string> { [enemy] = "Same canonical name" },
            EnemyNameIds: metadata ? new Dictionary<ActorRef, byte> { [enemy] = id } : null);
        var scene1 = Scene(first.Encounter, 0); var scene2 = Scene(second.Encounter, 39);
        var definition = new ScenarioDefinition("scoped-enemy-display", [first, second], encounterScenes: [scene1, scene2]);
        var start = read.Start;
        var sessions = new[] { first, second }.Select(encounter => Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new BattleStartInput(encounter.Encounter, start.Actors, start.MainSeed, start.ThinkingSeed, start.Gold))).Session).ToArray();
        Assert.Same(scene1, sessions[0].ActiveBattleScenes); Assert.Same(scene2, sessions[1].ActiveBattleScenes);
        for (int i = 0; i < sessions.Length; i++)
        {
            var session = sessions[i]; var before = session.Current;
            Assert.Equal(metadata ? (byte?)(i == 0 ? 0 : 39) : null, session.ActiveBattleScenes!.EnemyNameIds?.GetValueOrDefault(enemy));
            Assert.Equal("Same canonical name", CharacterNames.Battle(before.Story, before.Battle.GetActor(enemy), session.ActiveBattleScenes));
            Assert.Same(before, session.Current);
        }
    }
}
