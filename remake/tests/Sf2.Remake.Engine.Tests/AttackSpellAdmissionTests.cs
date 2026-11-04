using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class AttackSpellAdmissionTests
{
    [Theory]
    [InlineData("element")] [InlineData("family")] [InlineData("rank")] [InlineData("range")]
    [InlineData("radius")] [InlineData("resistance")] [InlineData("duplicate-effect")]
    public void UnsupportedDefinitionsRejectBeforePublication(string fault)
    {
        var d = Document("attack-spells");
        switch (fault)
        {
            case "element": d["spells"]![2]!["effect"]!["element"] = "fire"; break;
            case "family": d["spells"]![0]!["id"] = "freeze"; break;
            case "rank": d["spells"]![0]!["level"] = 3; break;
            case "range": d["spells"]![0]!["minimumRange"] = 2; d["spells"]![0]!["maximumRange"] = 1; break;
            case "radius": d["spells"]![1]!["effect"]!["radius"] = 2; break;
            case "resistance": d["actors"]![3]!["resistance"] = 65536; break;
            case "duplicate-effect":
                var heal = Document("practice-yard")["spells"]![0]!.DeepClone();
                heal["id"] = "blaze"; heal["level"] = 1; d["spells"]!.AsArray().Add(heal); break;
        }
        Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(d)));
    }

    [Theory]
    [InlineData(1)] [InlineData(40)] [InlineData(99)]
    public void LearnedDamageSpellHasNoInventedWalkthroughLevelLimit(int level)
    {
        var s = Start("attack-spells", d => d["actors"]![0]!["level"] = level);
        Assert.True(s.QueryBattleChoices().Spells[1].Enabled);
        Assert.Null(AttackSpellSessionTests.Cast(s).Failure);
    }

    [Fact]
    public void NoReachableCenterAndDeadOrNeutralCenterNeverConsumeResources()
    {
        var s = Start("attack-spells", d =>
        {
            foreach (var p in d["encounters"]![0]!["placements"]!.AsArray().Skip(2)) p!["x"] = 7;
        });
        var initial = s.Current; Assert.All(s.QueryBattleChoices().Spells, c => Assert.False(c.Enabled));
        Accept(s, new Confirm()); Accept(s, new SelectSpell(new("blaze", 2))); var before = s.Current;
        Assert.Equal("target-range", Send(s, new SelectTarget(new("center"))).Failure!.Code); Assert.Same(before, s.Current);
        Assert.Same(initial.Battle, s.Current.Battle);
        var b = AttackSpellTests.Battle();
        foreach (var invalid in new[] { b.GetActor(new("center")).With(hp: 0), b.GetActor(new("center")).With(activationWord: 8) })
        {
            var changed = b.With(actors: b.Actors.Select(a => a.Actor == invalid.Actor ? invalid : a));
            var rule = new Sf2AttackSpellAction();
            Assert.DoesNotContain(invalid.Actor, rule.QueryTargets(changed, new("mage"), new(BattleActionKind.AttackSpell, new("blaze", 2))));
            Assert.Equal("attack-spell-target", Assert.Throws<BattleRuleException>(() =>
                rule.RequireTarget(changed, new("mage"), new(2, 2), new(BattleActionKind.AttackSpell, new("blaze", 2)), invalid.Actor)).Code);
        }
    }

    [Fact]
    public void ConnectedOutcomeGrowthDropAndWalkingReturnRetainAttackDefinitionAndLiveAccounting()
    {
        var d = Document("battle-drops"); var b = d["battle"]!;
        b["spells"] = Document("attack-spells")["spells"]!.DeepClone();
        b["actors"]![0]!["maxMp"] = 20;
        b["actors"]![0]!["spells"] = JsonNode.Parse("""[{"id":"blaze","level":1}]""");
        b["start"]!["actors"]![0]!["mp"] = 20; b["start"]!["actors"]![0]!["exp"] = 99;
        b["actors"]![2]!["items"] = JsonNode.Parse("[61]");
        b["encounters"]![0]!["itemDrops"]!["rows"]![0]!["item"] = 61;
        var stats = new JsonArray();
        for (int i = 0; i < 5; i++) stats.Add(new JsonObject { ["start"] = 1, ["projected"] = 1, ["curve"] = new JsonArray() });
        d["world"]!["growth"] = new JsonArray(new JsonObject { ["actor"] = "leader", ["classId"] = 0,
            ["stats"] = stats, ["spells"] = new JsonArray() });
        d["start"]!["map"] = "arena"; d["start"]!["flags"]!.AsArray().Add(401);
        d["world"]!["programs"]!.AsArray().Single(p => (string?)p!["id"] == "before")!["instructions"] = JsonNode.Parse("[{\"op\":\"end\"}]");
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(d))).Session;
        Settle(session);
        var definition = session.Current.Battle.Definition;
        Assert.NotNull(definition.Outcome);
        Assert.NotNull(session.Current.Battle.GetActor(new("leader")).Definition.Growth);
        Assert.Equal(3, definition.AttackSpells.Count); Assert.NotNull(definition.ItemDrops);
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("blaze", 1)));
        Accept(session, new SelectTarget(new("enemy"))); var result = Accept(session, new Confirm());
        Assert.Contains(result.Observations, e => e.Kind == "item-received");
        Assert.Equal((ushort)61, session.Current.Battle.GetActor(new("leader")).SourceLoadout!.Items[0]);
        while (session.Current.BattleScene is not { Phase: BattleScenePhase.RewardMessage })
            Accept(session, AttackSpellSessionTests.Next(session.Current));
        Assert.Equal(18, session.Current.Battle.GetActor(new("leader")).Mp);
        Settle(session);
        Assert.Equal(SessionMode.Exploration, session.Current.Mode); Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        var leader = session.Current.Exploration!.Party.Actors.Single(a => a.Actor == new ActorRef("leader"));
        Assert.Equal(2, leader.Progress!.Level); Assert.Equal((ushort)61, leader.SourceLoadout!.Items[0]);
        Assert.Equal(84u, session.Current.Exploration.Party.Gold);
        Accept(session, new Move(ExplorationDirection.West)); Settle(session);
        Assert.Equal(SessionMode.Battle, session.Current.Mode); Assert.True(session.Current.HasBattleControl);
        Assert.Equal(3, session.Current.Battle.Definition.AttackSpells.Count);
        Assert.True(session.QueryBattleChoices().Spells.Single(s => s.Spell == new SpellRef("blaze", 1)).Enabled);
        Assert.Equal((ushort)61, session.Current.Battle.GetActor(new("leader")).SourceLoadout!.Items[0]);
    }

    private static void Settle(GameSession session)
    {
        for (int i = 0; i < 1000; i++)
        {
            if (session.Current.BattleScene is not null) { Accept(session, AttackSpellSessionTests.Next(session.Current)); continue; }
            if (session.Current.Story.Wait is { } wait)
            {
                Accept(session, wait switch
                {
                    DialogueWait => new Acknowledge(wait.Token),
                    FullFadeWait { LogicalDone: true } fade => new CompletePresentation(wait.Token, fade.Kind),
                    PresentationWait presentation => new CompletePresentation(wait.Token, presentation.Cue.Kind),
                    _ => new AdvanceSimulation(wait.Token),
                });
                continue;
            }
            return;
        }
        Assert.Fail("Finite return did not settle.");
    }

    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN",
        "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD", "SF2_PRIVATE_CONTROLLED_START")]
    public void PinnedLearnedBlazeAndBlastReachTheirOwnActionAdmissionWithoutImportingStoryOrEnemyAi()
    {
        string Input(string name) => PrivateInputFactAttribute.RequireInput(name);
        var original = Assert.IsType<ScenarioReadAccepted>(PrivateBattleScenarioTests.Selected().Read()).Definition.PrivateDefinitions!.Encounter;
        var start = ControlledBattleStartReader.Read(Input("SF2_PRIVATE_CONTROLLED_START"));
        start = start with { Allies = start.Allies.Select((a, i) => i switch
        {
            1 => a with { ClassId = 4, Level = 10, MaxMp = 20, Mp = 20, Spells = new byte[] { 14, 63, 63, 63 } },
            2 => a with { ClassId = 3, Level = 5, MaxMp = 20, Mp = 20, Items = new ushort[] { 213, 127, 127, 127 }, Spells = new byte[] { 75, 63, 63, 63 } },
            _ => a,
        }).ToArray() };
        var definitions = PrivateBattleDefinitionReader.Read(Input("SF2_PRIVATE_STATIC_DATA"), Input("SF2_PRIVATE_ENEMY_DATA"),
            Input("SF2_PRIVATE_ENEMY_GOLD"), original, start);
        var admitted = PrivateBattleScenarioReader.Assemble(definitions, start);
        var battle = BattleTurnFlow.Start(admitted.Definition.Encounters.Values.Single(), admitted.Start);
        var action = new Sf2AttackSpellAction();
        Assert.Equal(AttackSpellElement.Wind, action.RequireAction(battle, new("ally-1"), new(BattleActionKind.AttackSpell, new("blast", 1))).AttackSpell!.Element);
        Assert.Equal(AttackSpellElement.Fire, action.RequireAction(battle, new("ally-2"), new(BattleActionKind.AttackSpell, new("blaze", 2))).AttackSpell!.Element);
        Assert.Contains(new SpellRef("blaze", 1), battle.GetActor(new("ally-2")).Spells);
        Assert.Equal((ushort)192, (ushort)(battle.GetActor(new("enemy-0")).Definition.Resistance & 192));
    }
}
