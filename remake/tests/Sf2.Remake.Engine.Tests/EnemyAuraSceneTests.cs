using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class EnemyAuraSceneTests
{
    [Theory]
    [InlineData(3, 19)]
    [InlineData(22, 7)]
    public void LaterEnemyRecipientsKeepTheirOwnTextIdentityAndIdleWork(int firstIdle, int lastIdle)
    {
        var original = AuraHealingTests.Battle();
        var caster = AuraHealingTests.Caster;
        ActorRef[] targets = [new("a-edge"), AuraHealingTests.Center, caster];
        var rows = targets.Select((id, index) =>
        {
            var actor = original.GetActor(id);
            var definition = new BattleActorDefinition(id, BattleClassRule.Ordinary, 1, 100, 80,
                10, 10, 10, false, 5, id == caster ? [new("aura", 1)] : []);
            return actor.Deployment with { Definition = definition, Faction = BattleFaction.Enemy,
                Control = BattleControl.Automatic, SourceEnemy = (byte)(10 + index) };
        }).ToArray();
        var initialization = new BattleInitializationDefinition([], BattleRegionProgram.None,
            sourceCombatants: targets.Select((id, i) => (id, i)).ToDictionary(r => 128 + r.i, r => r.id), enemyCount: 3);
        var definition = new BattleDefinition("enemy-recovery", original.Definition.Map,
            original.Definition.Width, original.Definition.Height, original.Definition.Terrain, rows,
            [original.Definition.Spells[new("aura", 1)]], initialization: initialization);
        var actors = rows.Select(row => new BattleActorState(row, 10, 80, null, row.Position, null, null,
            activationWord: 1));
        var battle = new EngineBattleState(definition, actors, original.MainSeed, original.ThinkingSeed,
            original.Round, original.Queue, original.Cursor, original.Gold);
        var cast = new BattleSceneAnimation(0, 0, 4, false, null,
            [new(0, 1, 0, 0, null), new(0, 1, 0, 0, null)]);
        int Idle(ActorRef id) => id == targets[0] ? firstIdle : id == caster ? lastIdle : 11;
        var visuals = targets.ToDictionary(id => id, id => new BattleSceneActorVisual(0, 0, null, [], [],
            new Dictionary<string, BattleSceneAnimation> { ["cast"] = cast }, IdleTicks: Idle(id)));
        var names = targets.ToDictionary(id => id, id => "Species " + Array.IndexOf(targets, id));
        var content = new BattleSceneDefinition(definition.Encounter, "", "", new Dictionary<string, ExplorationRaster>(),
            new Dictionary<BattleClassRule, BattleSceneActorVisual>(), visuals,
            new Dictionary<int, string> { [274] = "{NAME} invokes {SPELL} {#}.", [298] = "{NAME} gains {#}." },
            [], new([], [], []), EnemyNames: names);
        var action = new Sf2HealingRule().Prepare(battle, caster, battle.GetActor(caster).Position!,
            new("aura", 1), AuraHealingTests.Center, new Sf2BattleProgressionRule());
        var before = new SessionSnapshot(Guid.NewGuid(), 0, 0, battle, null, SessionStopReason.PresentationWait);
        var result = BattleSceneContinuation.Begin(before, action, [], sceneContent: content);
        HashSet<ActorRef> seen = [];
        var events = result.Observations.ToList();
        bool idleSeen = false;
        while (result.Snapshot.BattleScene is { Phase: not BattleScenePhase.End } scene)
        {
            var healing = Assert.IsType<HealingSceneCursor>(scene.Healing);
            if (scene.Phase == BattleScenePhase.ResultMessage)
            {
                var target = scene.Target!.Value;
                seen.Add(target);
                Assert.Equal(target, healing.Display.Target);
                Assert.Equal(names[target], healing.Display.TargetName);
                Assert.Equal(298, healing.Display.RecoverySourceText);
                Assert.Equal(274, healing.Display.ActionSourceText);
                Assert.Equal(names[target] + " gains 15.", healing.RecoveryText);
                Assert.Equal(Idle(target), healing.TargetIdleTicks);
                Assert.Equal(target, scene.DisplayedEnemy);
                Assert.Null(scene.DisplayedAlly);
            }
            if (scene.Phase == BattleScenePhase.MakeIdle)
            {
                idleSeen = true;
                Assert.Contains(healing.Work, work => work.Caller == "bsc05:ally-idle-sleep" && work.Count == lastIdle);
            }
            result = BattleSceneContinuation.Submit(result.Snapshot,
                !healing.LogicalComplete && !healing.AtTimedInput ? new AdvanceSimulation(scene.Token) :
                scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                new CompletePresentation(scene.Token, scene.CompletionKind));
            Assert.Null(result.Failure);
            events.AddRange(result.Observations);
        }
        Assert.Equal(targets, seen);
        Assert.True(idleSeen);
        Assert.Single(events, e => e.Kind == "mp");
        Assert.Equal(73, result.Snapshot.Battle.GetActor(caster).Mp);
        Assert.Equal(targets, events.Where(e => e.Kind == "healing-recovery").Select(e => e.Target!.Value));
        Assert.DoesNotContain(events, e => e.Kind == "exp");
    }
}
