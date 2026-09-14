using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PhysicalRuleConfigurationTests
{
    [Theory]
    [InlineData(false, "one-in-32", "half", 32, 2, 383, 5)]
    [InlineData(false, "one-in-16", "quarter", 16, 4, 403, 4)]
    [InlineData(true, "one-in-32", "half", 32, 2, 383, 0)]
    [InlineData(true, "one-in-16", "quarter", 16, 4, 403, 0)]
    public void SemanticCriticalParametersDriveActualPlayerAndEnemyDamageAndRewards(bool enemyActs,
        string chance, string bonus, int chanceDenominator, int bonusDenominator, int hp, int exp)
    {
        var document = Document("stone-court");
        document["start"]!["mainSeed"] = 0x9B121234u;
        foreach (int index in new[] { 0, 2 })
        {
            document["actors"]![index]!["maxHp"] = 500; document["start"]!["actors"]![index]!["hp"] = 500;
            document["actors"]![index]!["defense"] = 4;
        }
        int attackerIndex = enemyActs ? 2 : 0;
        document["actors"]![attackerIndex]!["attack"] = 82;
        document["actors"]![attackerIndex]!["physical"]!["critical"] = Critical(chance, bonus);
        if (enemyActs)
        {
            document["encounters"]![0]!["placements"]![2]!["aiStrategy"] = "attack-then-approach";
            document["actors"]![2]!["move"] = 1;
        }
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document))).Session;
        var attacker = new ActorRef(enemyActs ? "raider" : "swordsman");
        var target = new ActorRef(enemyActs ? "swordsman" : "raider");
        var rule = session.Current.Battle.GetActor(attacker).Definition.Physical!.Critical;
        Assert.Equal(chanceDenominator, rule.ChanceDenominator);
        Assert.Equal(bonusDenominator, rule.DamageBonusDenominator);
        Assert.Equal(0x03071234u, session.Current.Battle.MainSeed);
        var result = enemyActs ? Stay(session) : Attack(session);
        Assert.Equal(target, Assert.Single(result.Observations, o => o.Kind == "critical").Target);
        Assert.Equal(attacker, Assert.Single(result.Observations, o => o.Kind == "physical-first").Actor);
        Assert.DoesNotContain(result.Observations, o => o.Kind is "physical-second" or "physical-counter");
        // Accepted combat arithmetic: base78 ->117 with half bonus or97 with quarter bonus.
        // Word LCG from randomness.md: after17 draws is1, then20/267 give zero spread;
        // 3478/45221 fail the double/counter draws. No product output supplies these constants.
        Assert.Equal(hp, result.Snapshot.Battle.GetActor(target).Hp);
        var rolls = result.Observations.Where(o => o.Kind.StartsWith("rng-", StringComparison.Ordinal)).ToArray();
        Assert.Equal(new ushort?[] { 32, (ushort)chanceDenominator, (ushort)(bonusDenominator == 2 ? 15 : 13),
            (ushort)(bonusDenominator == 2 ? 15 : 13), 32, 32 }, rolls.Take(6).Select(o => o.RandomRange));
        Assert.Equal(new ushort?[] { 4, 0, 0, 0, 1, 22 }, rolls.Take(6).Select(o => o.RandomValue));
        Assert.Equal(enemyActs ? 6 : 8, rolls.Length);
        Assert.Equal(enemyActs ? 0xB0A51234u : 0x9D4F1234u, result.Snapshot.Battle.MainSeed);
        Assert.Equal(exp, result.Snapshot.Battle.GetActor(new("swordsman")).Exp!.Value);
        Assert.Equal(100u, result.Snapshot.Battle.Gold);
        Assert.Equal(new ActorRef("lookout"), result.Snapshot.Selection!.Actor);
        if (enemyActs) Assert.Equal(0x02EF0042u, result.Snapshot.Battle.ThinkingSeed);
    }

    [Theory]
    [InlineData("one-in-32", "half", 32)]
    [InlineData("one-in-16", "quarter", 16)]
    public void ReversedCounterReadsItsOwnRuleWhilePreservingTheAcceptedAction(string chance, string bonus, int range)
    {
        var document = Document("stone-court"); document["start"]!["mainSeed"] = 0x00371234u;
        foreach (int index in new[] { 0, 2 })
        { document["actors"]![index]!["maxHp"] = 500; document["start"]!["actors"]![index]!["hp"] = 500; }
        document["actors"]![2]!["attack"] = 18;
        document["actors"]![2]!["physical"]!["critical"] = Critical(chance, bonus);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document))).Session;
        var result = Attack(session);
        var criticalRolls = result.Observations.Where(o => o.Kind == "rng-critical").ToArray();
        Assert.Equal(new ushort?[] { 16, (ushort)range }, criticalRolls.Select(o => o.RandomRange));
        Assert.Equal(new ActorRef("raider"), criticalRolls[1].Actor);
        Assert.Equal(new ActorRef("swordsman"), criticalRolls[1].Target);
        Assert.Equal(new[] { "physical-first", "physical-counter" },
            result.Observations.Where(o => o.Kind.StartsWith("physical-", StringComparison.Ordinal)).Select(o => o.Kind));
        // Existing seed55 first/counter observation: damage22/7, one2EXP award,14draws.
        Assert.Equal(493, result.Snapshot.Battle.GetActor(new("swordsman")).Hp);
        Assert.Equal(478, result.Snapshot.Battle.GetActor(new("raider")).Hp);
        Assert.Equal(2, result.Snapshot.Battle.GetActor(new("swordsman")).Exp!.Value);
        Assert.Equal(14, result.Observations.Count(o => o.Kind.StartsWith("rng-", StringComparison.Ordinal)));
        Assert.Equal(0x557E1234u, result.Snapshot.Battle.MainSeed);
    }

    [Theory]
    [InlineData("missing-critical", "missing-field", SessionFailureKind.ContentError)]
    [InlineData("raw-prowess", "unknown-or-duplicate-field", SessionFailureKind.ContentError)]
    [InlineData("missing-chance", "missing-field", SessionFailureKind.ContentError)]
    [InlineData("numeric-chance", "string-required", SessionFailureKind.ContentError)]
    [InlineData("null-critical", "object-required", SessionFailureKind.ContentError)]
    [InlineData("unsupported-chance", "physical-critical-rule", SessionFailureKind.UnsupportedCapability)]
    [InlineData("unsupported-bonus", "physical-critical-rule", SessionFailureKind.UnsupportedCapability)]
    [InlineData("unsupported-pair", "physical-critical-rule", SessionFailureKind.UnsupportedCapability)]
    public void ContentRejectsMissingMalformedAndUnsupportedCriticalConfiguration(string shape, string code, SessionFailureKind kind)
    {
        var document = Document("stone-court"); var physical = document["actors"]![0]!["physical"]!.AsObject();
        switch (shape)
        {
            case "missing-critical": physical.Remove("critical"); break;
            case "raw-prowess": physical.Remove("critical"); physical["prowess"] = 0; break;
            case "missing-chance": physical["critical"]!.AsObject().Remove("chance"); break;
            case "numeric-chance": physical["critical"]!["chance"] = 32; break;
            case "null-critical": physical["critical"] = null; break;
            case "unsupported-chance": physical["critical"]!["chance"] = "one-in-8"; break;
            case "unsupported-bonus": physical["critical"]!["damageBonus"] = "double"; break;
            case "unsupported-pair": physical["critical"] = Critical("one-in-32", "quarter"); break;
        }
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal((kind, code), (failed.Failure.Kind, failed.Failure.Code));
    }

    private static JsonObject Critical(string chance, string bonus) => new() { ["chance"] = chance, ["damageBonus"] = bonus };
    private static SessionResult Attack(GameSession session)
    {
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(new("raider"))); return Accept(session, new Confirm());
    }
}
