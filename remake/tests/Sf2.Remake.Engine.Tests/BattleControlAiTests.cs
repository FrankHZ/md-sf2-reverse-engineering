using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleControlAiTests
{
    [Fact]
    public void OneActorDefinitionSupportsPlayerControlAndBothAutomaticPoliciesByDeployment()
    {
        var document = Document("stone-court");
        // Existing EnemyActionTests seed-55 counter case supplies independent damage/reward/RNG values.
        document["start"]!["mainSeed"] = 0x00371234u;
        foreach (int index in new[] { 0, 2 })
        {
            document["start"]!["actors"]![index]!["hp"] = 500;
            document["actors"]![index]!["maxHp"] = 500;
            document["actors"]![index]!["defense"] = 4;
        }
        document["actors"]![0]!["attack"] = 18;
        document["actors"]![0]!["physical"]!["critical"] = new JsonObject { ["chance"] = "one-in-32", ["damageBonus"] = "half" };
        document["actors"]![2]!["attack"] = 30; document["actors"]![2]!["move"] = 1;
        var encounter = document["encounters"]![0]!;
        var attacking = encounter.DeepClone(); attacking["id"] = "attack-watch";
        attacking["placements"]![2]!["aiStrategy"] = "attack-then-approach";
        var player = encounter.DeepClone(); player["id"] = "player-watch";
        player["placements"]![2]!["faction"] = "ally";
        player["placements"]![2]!["control"] = "player";
        player["placements"]![2]!["aiStrategy"] = null;
        document["encounters"]!.AsArray().Add(attacking);
        document["encounters"]!.AsArray().Add(player);
        var admitted = Assert.IsType<ScenarioReadAccepted>(Reader(document).Read());
        var staySession = Open(admitted.Start.Encounter);
        var attackSession = Open("attack-watch");
        var playerSession = Open("player-watch");
        var before = staySession.Current.Battle;
        var raider = new ActorRef("raider");
        foreach (var session in new[] { staySession, attackSession, playerSession })
        {
            Assert.Same(before.GetActor(raider).Definition, session.Current.Battle.GetActor(raider).Definition);
            Assert.Equal(new ActorRef("swordsman"), session.Current.Selection!.Actor);
            Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        }
        var stayed = Stay(staySession);
        Assert.Contains(stayed.Observations, o => o.Kind == "ai-stay" && o.Actor == raider);
        Assert.DoesNotContain(stayed.Observations, o => o.Actor == raider && o.Kind is "physical-first" or "ai-command-move1");
        Assert.Equal((500, 500), ((int)stayed.Snapshot.Battle.GetActor(new("swordsman")).Hp, stayed.Snapshot.Battle.GetActor(raider).Hp));
        Assert.Equal(before.MainSeed, stayed.Snapshot.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, stayed.Snapshot.Battle.ThinkingSeed);
        Assert.Null(stayed.Snapshot.Battle.GetActor(raider).LastTarget);
        var attacked = Stay(attackSession);
        Assert.Equal((500, 500), ((int)attacked.Snapshot.Battle.GetActor(new("swordsman")).Hp, attacked.Snapshot.Battle.GetActor(raider).Hp));
        Assert.Equal(0x557E1234u, attacked.Snapshot.Battle.MainSeed);
        attacked = FinishBattleScenes(attackSession, attacked);
        Assert.Equal((478, 493, 1), ((int)attacked.Snapshot.Battle.GetActor(new("swordsman")).Hp,
            attacked.Snapshot.Battle.GetActor(raider).Hp, attacked.Snapshot.Battle.GetActor(new("swordsman")).Exp!.Value));
        Assert.Equal(0x557E1234u, ConstructionSeed(attacked));
        Assert.Equal(0x02EF0042u, attacked.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(new ActorRef("swordsman"), attacked.Snapshot.Battle.GetActor(raider).LastTarget);
        Assert.Equal(new ActorRef("lookout"), attacked.Snapshot.Selection!.Actor);
        Assert.Equal(attacked.Snapshot.Selection.Actor, stayed.Snapshot.Selection!.Actor);
        var controlled = Stay(playerSession);
        Assert.Equal(raider, controlled.Snapshot.Selection!.Actor);
        Assert.Equal(BattleControl.Player, controlled.Snapshot.Battle.GetActor(raider).Control);
        Assert.Null(controlled.Snapshot.Battle.GetActor(raider).AiStrategy);
        Assert.Equal(before.MainSeed, controlled.Snapshot.Battle.MainSeed);
        Assert.DoesNotContain(controlled.Observations, o => o.Kind.StartsWith("ai-", StringComparison.Ordinal));
        var selected = playerSession.Current;
        Accept(playerSession, new Confirm()); Accept(playerSession, new Cancel());
        Assert.Same(selected.Battle, playerSession.Current.Battle);
        Assert.Equal(raider, playerSession.Current.Selection!.Actor);
        Assert.Same(stayed.Snapshot, staySession.Current);
        Assert.Same(attacked.Snapshot, attackSession.Current);

        GameSession Open(string id) => Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition,
            new BattleStartInput(id, admitted.Start.Actors.Reverse(), admitted.Start.MainSeed,
                admitted.Start.ThinkingSeed, admitted.Start.Gold))).Session;
    }

    [Theory]
    [InlineData("missing-control", "missing-field", SessionFailureKind.ContentError)]
    [InlineData("missing-strategy", "missing-field", SessionFailureKind.ContentError)]
    [InlineData("automatic-null", "control-ai", SessionFailureKind.UnsupportedCapability)]
    [InlineData("player-strategy", "control-ai", SessionFailureKind.UnsupportedCapability)]
    [InlineData("unknown-control", "control-mode", SessionFailureKind.UnsupportedCapability)]
    [InlineData("source-strategy", "ai-strategy", SessionFailureKind.UnsupportedCapability)]
    [InlineData("number-strategy", "string-required", SessionFailureKind.ContentError)]
    [InlineData("actor-controller", "unknown-or-duplicate-field", SessionFailureKind.ContentError)]
    [InlineData("missing-physical", "physical-definition", SessionFailureKind.UnsupportedCapability)]
    [InlineData("spellbook", "ai-action-categories", SessionFailureKind.UnsupportedCapability)]
    public void MissingOrIncompatiblePolicyCannotSilentlyStartAsStay(string shape, string code, SessionFailureKind kind)
    {
        var document = Document("stone-court");
        var placements = document["encounters"]![0]!["placements"]!;
        var enemy = placements[2]!;
        switch (shape)
        {
            case "missing-control": enemy.AsObject().Remove("control"); break;
            case "missing-strategy": enemy.AsObject().Remove("aiStrategy"); break;
            case "automatic-null": enemy["aiStrategy"] = null; break;
            case "player-strategy": placements[0]!["aiStrategy"] = "stay"; break;
            case "unknown-control": enemy["control"] = "remote"; break;
            case "source-strategy": enemy["aiStrategy"] = "commandset06-script3"; break;
            case "number-strategy": enemy["aiStrategy"] = 6; break;
            case "actor-controller": document["actors"]![2]!["controller"] = "stay"; break;
            case "missing-physical":
                enemy["aiStrategy"] = "attack-then-approach";
                document["actors"]![2]!.AsObject().Remove("physical"); break;
            case "spellbook":
                enemy["aiStrategy"] = "attack-then-approach";
                document["actors"]![2]!["spells"]!.AsArray().Add(new JsonObject { ["id"] = "mend", ["level"] = 1 });
                document["spells"]!.AsArray().Add(new JsonObject {
                    ["id"] = "mend", ["level"] = 1, ["mpCost"] = 3, ["minimumRange"] = 0, ["maximumRange"] = 1,
                    ["effect"] = new JsonObject { ["kind"] = "heal", ["adjustedPower"] = 15, ["fullRecovery"] = false } }); break;
        }
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal((kind, code), (failed.Failure.Kind, failed.Failure.Code));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReusableStartRejectsInvalidDeploymentBeforePublishingASecondSession(bool unknownPolicy)
    {
        var admitted = Admitted();
        var running = Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session;
        var before = running.Current;
        var encounter = before.Battle.Definition;
        var deployments = encounter.Deployments.Select(d => d.Control == BattleControl.Automatic
            ? d with { AiStrategy = unknownPolicy ? (BattleAiStrategy)99 : null } : d);
        var invalid = new BattleDefinition(encounter.Encounter, encounter.Map, encounter.Width, encounter.Height,
            encounter.Terrain, deployments, encounter.Spells.Values, encounter.Rewards);
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(new ScenarioDefinition("invalid", [invalid]), admitted.Start));
        Assert.Equal((SessionFailureKind.UnsupportedCapability, "control-ai"), (failed.Failure.Kind, failed.Failure.Code));
        Assert.Same(before, running.Current);
    }
}
