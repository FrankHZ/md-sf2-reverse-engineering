using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PrivateActionBindingTests
{
    // Explicit controlled action input, consumed only by tests/ordinary startup selection.
    internal static ScenarioReadAccepted AdmittedActions() => Assert.IsType<ScenarioReadAccepted>(
        PrivateBattleScenarioTests.Selected(Path.Combine(AppContext.BaseDirectory, "controlled", "battle01-actions.json")).Read());

    private static ScenarioReadAccepted WithHealingScene(ScenarioReadAccepted admitted)
    {
        var scenes = BattleSceneContentReader.Read(PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE_SCENE_CONTENT"),
            admitted.Definition.PrivateDefinitions!);
        return admitted with { Definition = new ScenarioDefinition(admitted.Definition.Package, admitted.Definition.Encounters.Values,
            admitted.Definition.PrivateDefinitions, battleScenes: scenes) };
    }

    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD")]
    public void ActualDefinitionsBindEquipmentProwessGoldAndDistinctEffectiveAttack()
    {
        var admitted = AdmittedActions();
        var initial = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session.Current.Battle;
        Assert.Equal(new byte[] { 9, 9, 8 }, initial.Actors.Take(3).Select(actor => actor.Attack));
        Assert.All(initial.Actors.Take(3), actor =>
        {
            Assert.Same(PhysicalCriticalRule.OneIn16WithQuarterBonus, actor.Definition.Physical!.Critical);
            Assert.Equal((byte)1, actor.Definition.Physical.MinimumRange); Assert.Equal((byte)1, actor.Definition.Physical.MaximumRange);
        });
        Assert.True(initial.Actors[0].Definition.Physical!.Leader);
        Assert.All(initial.Actors.Skip(3), actor =>
        {
            Assert.Equal((byte)7, actor.Definition.Attack); Assert.Equal((byte)8, actor.Attack);
            Assert.Equal((ushort)60, actor.Definition.Physical!.Gold);
            Assert.Same(PhysicalCriticalRule.OneIn32WithHalfBonus, actor.Definition.Physical.Critical);
        });
        var gold = admitted.Definition.PrivateDefinitions!.EnemyGold[39];
        Assert.Equal(48922, gold.RomAddress); Assert.Equal("data/stats/enemies/enemygold.asm", gold.SourcePath);
        Assert.True(initial.Definition.Rewards!.HalvedExperience);
    }

    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD")]
    public void EachActualPartyActorUsesItsSourcePhysicalOperandsAndKillsCleanUpAtomically()
    {
        var admitted = AdmittedActions();
        var initialized = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session.Current.Battle;
        // Small controlled rule seam: explicit positions/HP/seed, never a running-session mutation.
        // Accounting is retained directly from the external controlled start, with no null fill.
        foreach (int allyIndex in new[] { 0, 1, 2 })
        {
            var actorRef = new ActorRef("ally-" + allyIndex); var targetRef = new ActorRef("enemy-0");
            var actors = initialized.Actors.Select(actor => new BattleActorState(actor.Deployment,
                actor.Actor == targetRef ? (ushort)1 : actor.Hp, actor.Mp, actor.Exp,
                actor.Actor == actorRef ? new MapPosition(8, 10) : actor.Actor == targetRef ? new(8, 9) : actor.Position,
                actor.Kills, actor.Defeats, attack: actor.Attack, activationWord: actor.ActivationWord));
            var before = new EngineBattleState(initialized.Definition, actors, 0xAF881234, 0x01340099,
                9, [], 0, admitted.Start.Gold, initialized.StartPolicy, initialized.Regions);
            var frozen = before.Actors.Select(actor => (actor.Actor, actor.Position, actor.Hp, actor.Exp, actor.Kills, actor.Defeats)).ToArray();
            var resolved = PhysicalBattleAction.Resolve(before, actorRef, new(8, 10), targetRef);
            Assert.Equal(frozen, before.Actors.Select(actor => (actor.Actor, actor.Position, actor.Hp, actor.Exp, actor.Kills, actor.Defeats)));
            Assert.Equal(new ushort?[] { 8, 16, 1, 1, 16, 16 }, resolved.Effects.Where(row => row.Kind.StartsWith("rng-", StringComparison.Ordinal)).Select(row => row.RandomRange));
            Assert.Equal((ushort)0, resolved.Battle.GetActor(targetRef).Hp); Assert.Null(resolved.Battle.GetActor(targetRef).Position);
            Assert.Equal(60u, resolved.Battle.Gold); Assert.Equal((ushort)1, resolved.Battle.GetActor(actorRef).Kills);
            Assert.Equal(before.ThinkingSeed, resolved.Battle.ThinkingSeed);
            Assert.Single(resolved.Effects, effect => effect.Kind == "death-cleanup");
        }
    }

    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD")]
    public void ActualHoveringTargetRetainsSourceLandReductionAndExactFirstPlayerAward()
    {
        var admitted = AdmittedActions();
        var initial = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session.Current.Battle;
        var attacker = new ActorRef("ally-0"); var target = new ActorRef("enemy-4");
        var actors = initial.Actors.Select(actor => new BattleActorState(actor.Deployment,
            actor.Actor == attacker ? (ushort)9 : actor.Hp, actor.Mp, actor.Exp,
            actor.Actor == attacker ? new MapPosition(11, 15) : actor.Actor == target ? new(11, 14) : actor.Position,
            actor.Kills, actor.Defeats, attack: actor.Attack, activationWord: actor.ActivationWord));
        var before = new EngineBattleState(initial.Definition, actors, 0xAF881234, 0x01340000, 6, [], 0,
            admitted.Start.Gold, initial.StartPolicy, initial.Regions);
        var result = PhysicalBattleAction.Resolve(before, attacker, new(11, 15), target);
        // Accepted ManualAttack reference: hovering on source terrain1 uses230, not256.
        Assert.Equal((ushort)2, result.Battle.GetActor(target).Hp); Assert.Equal((byte)15, result.Battle.GetActor(attacker).Exp);
        Assert.Equal(0xE9F01234u, result.Battle.MainSeed); Assert.Equal(before.ThinkingSeed, result.Battle.ThinkingSeed);
        Assert.Equal(new ushort?[] { 7, 14, 0, 0, 12, 31, 15, 14 }, result.Effects.Where(row => row.Kind.StartsWith("rng-", StringComparison.Ordinal)).Select(row => row.RandomValue));
        Assert.Equal((ushort)5, before.GetActor(target).Hp); Assert.Equal((byte)0, before.GetActor(attacker).Exp);
    }

    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD", "SF2_PRIVATE_BATTLE_SCENE_CONTENT")]
    public void PrivatePriestHealsThroughCommonCommandsAndEgressKeepsItsUnsupportedEffect()
    {
        var admitted = WithHealingScene(AdmittedActions()); var session = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session;
        var actor = session.Current.Selection!.Actor;
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("heal", 1))); Accept(session, new SelectTarget(actor));
        var result = FinishBattleScenes(session, Accept(session, new Confirm()));
        Assert.Equal((byte)7, result.Snapshot.Battle.GetActor(actor).Mp); Assert.InRange(result.Snapshot.Battle.GetActor(actor).Exp!.Value, 9, 11);
        Assert.Single(result.Observations, row => row.Kind == "after-turn" && row.Actor == actor);
        Stay(session); Assert.Equal(new ActorRef("ally-0"), session.Current.Selection!.Actor);
        Accept(session, new Confirm()); var before = session.Current;
        Assert.Equal("spell-effect", Send(session, new SelectSpell(new("egress", 1))).Failure!.Code);
        Assert.Same(before, session.Current);
    }
    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD")]
    public void ActualNonleaderDefeatRequiresItsOwnCounterAndRetainsOtherAccounting()
    {
        var admitted = AdmittedActions();
        var initial = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session.Current.Battle;
        var attacker = new ActorRef("enemy-4"); var target = new ActorRef("ally-2");
        foreach (bool known in new[] { false, true })
        {
            var actors = initial.Actors.Select(actor => new BattleActorState(actor.Deployment,
                actor.Actor == target ? (ushort)1 : actor.Hp, actor.Mp, actor.Exp,
                actor.Actor == attacker ? new MapPosition(11, 14) : actor.Actor == target ? new(11, 15) : actor.Position,
                actor.Kills, actor.Actor == target && !known ? null : actor.Defeats,
                attack: actor.Attack, activationWord: actor.ActivationWord));
            var before = new EngineBattleState(initial.Definition, actors, 0x07821234, 0x01340099, 6, [], 0,
                admitted.Start.Gold, initial.StartPolicy, initial.Regions);
            if (!known)
            {
                Assert.Equal("unspecified-defeats", Assert.Throws<BattleRuleException>(() =>
                    PhysicalBattleAction.Resolve(before, attacker, new(11, 14), target)).Code);
                Assert.Null(before.GetActor(target).Defeats); Assert.Equal((ushort)1, before.GetActor(target).Hp);
                continue;
            }
            var result = PhysicalBattleAction.Resolve(before, attacker, new(11, 14), target);
            Assert.Equal((ushort)0, result.Battle.GetActor(target).Hp); Assert.Null(result.Battle.GetActor(target).Position);
            Assert.Equal((ushort)1, result.Battle.GetActor(target).Defeats); Assert.Null(result.Battle.GetActor(attacker).Kills);
            Assert.Equal(before.Gold, result.Battle.Gold); Assert.Equal(before.ThinkingSeed, result.Battle.ThinkingSeed);
            Assert.Single(result.Effects, effect => effect.Kind == "death-cleanup");
            Assert.DoesNotContain(result.Effects, effect => effect.Kind is "exp" or "gold" or "physical-counter");
        }
    }

    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD", "SF2_PRIVATE_BATTLE_SCENE_CONTENT")]
    public void LearnedSourceLevelExposesLowerLevelsAndKeepsUnsupportedSpellEffectsExplicit()
    {
        var document = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "controlled", "battle01-actions.json")))!;
        // Separate controlled content input, not a spellbook mutation on a running session.
        document["allies"]![1]!["spells"] = new System.Text.Json.Nodes.JsonArray(10, 128, 63, 63);
        string file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(file, document.ToJsonString());
            var admitted = WithHealingScene(Assert.IsType<ScenarioReadAccepted>(PrivateBattleScenarioTests.Selected(file).Read()));
            var session = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session;
            var actor = session.Current.Selection!.Actor;
            Assert.Equal(new[] { new SpellRef("egress", 1), new("heal", 1), new("heal", 2), new("heal", 3) }, session.Current.Battle.GetActor(actor).Definition.Spells);
            Accept(session, new Confirm()); var before = session.Current;
            Assert.Equal("spell-effect", Send(session, new SelectSpell(new("egress", 1))).Failure!.Code); Assert.Same(before, session.Current);
            Accept(session, new SelectSpell(new("heal", 2))); Accept(session, new SelectTarget(actor));
            var healed = FinishBattleScenes(session, Accept(session, new Confirm())); Assert.Equal((byte)5, healed.Snapshot.Battle.GetActor(actor).Mp);
        }
        finally { File.Delete(file); }
    }

}
