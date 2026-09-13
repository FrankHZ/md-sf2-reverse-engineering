using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleFactionOrderTests
{
    [Theory]
    [InlineData("stone-court", "lookout", "raider", 100, 23)]
    [InlineData("river-post", "runner", "intruder", 200, 48)]
    public void ExplicitFactionAndSparseOrderDriveActualTargetingAndRewardsAfterJsonReordering(
        string package, string allyName, string enemyName, uint gold, byte exp)
    {
        var document = Document(package);
        var placements = document["encounters"]![0]!["placements"]!.AsArray();
        int[] orders = [255, 1000, 70000, 80000, int.MaxValue];
        foreach (var (placement, index) in placements.OrderBy(p => p!["processingOrder"]!.GetValue<int>()).Select((p, i) => (p!, i)))
            placement["processingOrder"] = orders[index];
        // An ally's name and order both contradict the former inferred side convention.
        string oldName = document["actors"]![0]!["id"]!.GetValue<string>();
        document["actors"]![0]!["id"] = "enemy-named-player";
        foreach (var placement in placements)
            if (placement!["actor"]!.GetValue<string>() == oldName) placement["actor"] = "enemy-named-player";
        document["start"]!["actors"]![0]!["actor"] = "enemy-named-player";
        document["actors"] = Reverse(document["actors"]!);
        document["start"]!["actors"] = Reverse(document["start"]!["actors"]!);
        document["encounters"]![0]!["placements"] = Reverse(placements);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document))).Session;
        var actor = new ActorRef("enemy-named-player"); var enemy = new ActorRef(enemyName);
        Assert.Equal(actor, session.Current.Selection!.Actor);
        Assert.Equal(255, session.Current.Battle.GetActor(actor).ProcessingOrder);
        Assert.Equal(BattleFaction.Ally, session.Current.Battle.GetActor(actor).Faction);
        Assert.Equal(BattleFaction.Enemy, session.Current.Battle.GetActor(enemy).Faction);
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        var before = session.Current;
        var friendly = Send(session, new SelectTarget(new(allyName)));
        Assert.Equal(SessionFailureKind.IllegalCommand, friendly.Failure!.Kind);
        Assert.Same(before.Battle, session.Current.Battle);
        Accept(session, new SelectTarget(enemy)); var result = Accept(session, new Confirm());
        Assert.Equal(0x0A7F1234u, result.Snapshot.Battle.MainSeed);
        Assert.Equal(exp, result.Snapshot.Battle.GetActor(actor).Exp);
        Assert.Equal(gold + 19, result.Snapshot.Battle.Gold);
        Assert.Equal((ushort)1, result.Snapshot.Battle.GetActor(actor).Kills);
        Assert.Equal((ushort)0, result.Snapshot.Battle.GetActor(enemy).Hp);
        Assert.Null(result.Snapshot.Battle.GetActor(enemy).Position);
        Assert.Equal(new ActorRef(allyName), result.Snapshot.Selection!.Actor);
        Assert.Equal(0xBEEF0042u, result.Snapshot.Battle.ThinkingSeed);
    }

    [Fact]
    public void LowOrderEnemyRemainsAnOpponentForHealingAndMovement()
    {
        var document = Document(); var placements = document["encounters"]![0]!["placements"]!;
        placements[0]!["processingOrder"] = 300; placements[1]!["processingOrder"] = 500;
        placements[2]!["processingOrder"] = 0;
        // Put the enemy adjacent: a rejection must be allegiance, not range.
        placements[2]!["x"] = 4; placements[2]!["y"] = 3;
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document))).Session;
        for (int i = 0; session.Current.Selection?.Actor != new ActorRef("medic-a"); i++)
        { Assert.True(i < 8); Stay(session); }
        var battle = session.Current.Battle;
        Assert.Equal(BattleFaction.Enemy, battle.GetActor(new("dummy-a")).Faction);
        Assert.Equal(0, battle.GetActor(new("dummy-a")).ProcessingOrder);
        Assert.Throws<BattleRuleException>(() => BattleMovement.Preview(battle, new("medic-a"), new(4, 3)));
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("mend", 1)));
        var before = session.Current;
        var rejected = Send(session, new SelectTarget(new("dummy-a")));
        Assert.Equal("invalid-heal-target", rejected.Failure!.Code);
        Assert.Same(before.Battle, session.Current.Battle);
    }

    [Theory]
    [InlineData("faction", "battle-faction", SessionFailureKind.UnsupportedCapability)]
    [InlineData("ally-ai", "controller-side", SessionFailureKind.UnsupportedCapability)]
    [InlineData("enemy-player", "controller-side", SessionFailureKind.UnsupportedCapability)]
    [InlineData("duplicate-order", "duplicate-processing-order", SessionFailureKind.ContentError)]
    [InlineData("negative-order", "numeric-range", SessionFailureKind.ContentError)]
    [InlineData("missing-order", "missing-field", SessionFailureKind.ContentError)]
    [InlineData("missing-faction", "missing-field", SessionFailureKind.ContentError)]
    [InlineData("ally-level", "numeric-range", SessionFailureKind.ContentError)]
    public void ContentRejectsUndefinedFactionOrderAndUnsupportedControlCombinations(string shape, string code, SessionFailureKind kind)
    {
        var document = Document(); var placements = document["encounters"]![0]!["placements"]!;
        switch (shape)
        {
            case "faction": placements[0]!["faction"] = "neutral"; break;
            case "ally-ai": document["actors"]![0]!["controller"] = "stay"; break;
            case "enemy-player": document["actors"]![2]!["controller"] = "player"; break;
            case "duplicate-order": placements[0]!["processingOrder"] = placements[1]!["processingOrder"]!.DeepClone(); break;
            case "negative-order": placements[0]!["processingOrder"] = -1; break;
            case "missing-order": placements[0]!.AsObject().Remove("processingOrder"); break;
            case "missing-faction": placements[0]!.AsObject().Remove("faction"); break;
            case "ally-level": document["actors"]![0]!["level"] = 0; break;
        }
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal((kind, code), (failed.Failure.Kind, failed.Failure.Code));
    }

    [Theory]
    [InlineData("ally", 30, 0)]
    [InlineData("enemy", 32, 2)]
    public void EncounterFactionCapacityIncludesDeadDeployments(string faction, int limit, int templateIndex)
    {
        var document = Document();
        var actors = document["actors"]!.AsArray();
        var starts = document["start"]!["actors"]!.AsArray();
        var placements = document["encounters"]![0]!["placements"]!.AsArray();
        int initial = placements.Count(p => p!["faction"]!.GetValue<string>() == faction);
        for (int i = initial; i <= limit; i++)
        {
            if (i == limit) Assert.IsType<SessionStarted>(GameSession.Start(Reader(document)));
            string id = $"reserve-{i}";
            var actor = actors[templateIndex]!.DeepClone(); actor["id"] = id; actors.Add(actor);
            var start = starts[templateIndex]!.DeepClone(); start["actor"] = id; start["hp"] = 0; starts.Add(start);
            var deployment = placements[templateIndex]!.DeepClone();
            deployment["actor"] = id; deployment["processingOrder"] = 1000 + i; placements.Add(deployment);
        }
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal((SessionFailureKind.ContentError, "faction-capacity"), (failed.Failure.Kind, failed.Failure.Code));
    }

    private static JsonArray Reverse(JsonNode input) => new(input.AsArray().Reverse().Select(n => n!.DeepClone()).ToArray());
}
