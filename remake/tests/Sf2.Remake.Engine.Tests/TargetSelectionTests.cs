using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class TargetSelectionTests
{
    [Theory]
    [InlineData(0, 5, 1, 19)]
    [InlineData(2, 5, 2, 15)]
    [InlineData(3, 5, 1, 13)]
    [InlineData(9, 5, 2, 1)]
    [InlineData(127, 5, 1, 1)]
    [InlineData(128, 5, 1, 19)]
    [InlineData(255, 5, 1, 1)]
    [InlineData(8, 0, 0, 16)]
    [InlineData(8, 1, 0, 1)]
    public void ScriptThreeRetainsByteDoublingBorrowAndLethality(int movement, int hp, int roll, int priority) =>
        Assert.Equal(priority, PhysicalTargetRules.ScriptThree((byte)movement, hp, (byte)roll));

    [Fact]
    public void SignedMaximumAndRawCohortAreSeparateFromCappedReturnPriority()
    {
        var signed = PhysicalTargetRules.Select([new(0, 128, null), new(0, 127, null), new(0, 255, null)], PhysicalPriorityTable.Regular)!.Value;
        Assert.Equal(new PhysicalTargetSelection(1, 127, 15), signed);
        var raw = PhysicalTargetRules.Select([new(0, 16, 0), new(0, 19, 4)], PhysicalPriorityTable.Regular)!.Value;
        Assert.Equal(new PhysicalTargetSelection(1, 19, 15), raw);
        Assert.Null(PhysicalTargetRules.Select([new(0, 128, null), new(0, 255, null)], PhysicalPriorityTable.Regular));
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public void CriticalCohortUsesAttackersActualTableBeforeMovement(bool flying, int expected)
    {
        // Source table: regular KNTE22 < WARR25, flying WARR8 < KNTE15.
        var selection = PhysicalTargetRules.Select([new(10, 15, 1), new(0, 15, 2)],
            flying ? PhysicalPriorityTable.Flying : PhysicalPriorityTable.Regular)!.Value;
        Assert.Equal(expected, selection.Index);
        Assert.Equal(15, selection.RawPriority);
    }

    [Fact]
    public void NoncriticalAndEqualClassCohortsKeepReverseCollectionAndSignedMovement()
    {
        Assert.Equal(1, Select([new(8, 14, null), new(10, 14, null)]));
        Assert.Equal(0, Select([new(8, 14, null), new(8, 14, null)]));
        Assert.Equal(0, Select([new(8, 19, 4), new(8, 19, 4)]));
        Assert.Equal(1, Select([new(128, 14, null), new(127, 14, null)]));
        Assert.Equal(1, Select([new(255, 14, null), new(0, 14, null)]));
        Assert.Null(PhysicalTargetRules.Select([new(255, 14, null), new(128, 14, null)], PhysicalPriorityTable.Regular));
        static int Select(PhysicalTargetPriority[] values) => PhysicalTargetRules.Select(values, PhysicalPriorityTable.Regular)!.Value.Index;
    }

    [Theory]
    [InlineData("stone-court", false)]
    [InlineData("stone-court", true)]
    [InlineData("river-post", false)]
    [InlineData("river-post", true)]
    public void CompetingAdjacentTargetsUseClassDefinitionsAndCommonCounterSettlement(string package, bool swapClass)
    {
        var doc = Configure(package, swapClass);
        string primary = doc["actors"]![0]!["id"]!.GetValue<string>();
        string secondary = doc["actors"]![1]!["id"]!.GetValue<string>();
        string enemy = doc["actors"]![2]!["id"]!.GetValue<string>();
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc))).Session;
        var result = Stay(session);
        var chosen = new ActorRef(swapClass ? primary : secondary);
        var target = Assert.Single(result.Observations, o => o.Kind == "ai-target");
        Assert.Equal(chosen, target.Target);
        Assert.Equal(19, target.Before); Assert.Equal(15, target.After);
        var thinking = result.Observations.Where(o => o.Kind == "thinking-rng").ToArray();
        Assert.Equal(new ActorRef?[] { new(secondary), new(primary) }, thinking.Select(o => o.Target));
        Assert.Equal(new ushort?[] { 1, 2 }, thinking.Select(o => o.RandomValue));
        Assert.Equal(0x00EF0042L, thinking[0].Before); Assert.Equal(0x01EF0042L, thinking[0].After);
        Assert.Equal(thinking[0].After, thinking[1].Before);
        Assert.Equal(0x02EF0042u, result.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(0x557E1234u, result.Snapshot.Battle.MainSeed);
        Assert.Equal(478, result.Snapshot.Battle.GetActor(chosen).Hp);
        Assert.Equal(1, result.Snapshot.Battle.GetActor(chosen).Exp);
        Assert.Equal(493, result.Snapshot.Battle.GetActor(new(enemy)).Hp);
        Assert.Equal(chosen, result.Snapshot.Battle.GetActor(new(enemy)).LastTarget);
        Assert.Equal(new ActorRef(secondary), result.Snapshot.Selection!.Actor);
        Assert.Equal(2, result.Observations.Count(o => o.Kind == "action-committed"));
    }

    [Fact]
    public void ProcessingOrderWinsEqualCostSameClassTieDespiteConfigurationArrayOrder()
    {
        var doc = Configure("stone-court");
        doc["actors"]![0]!["classRule"] = "unpromoted-swordsman";
        doc["encounters"]![0]!["placements"]![0]!["processingOrder"] = 20;
        doc["encounters"]![0]!["placements"]![1]!["processingOrder"] = 3;
        var placements = doc["encounters"]![0]!["placements"]!.AsArray();
        var reversed = new JsonArray(placements.Reverse().Select(p => p!.DeepClone()).ToArray());
        doc["encounters"]![0]!["placements"] = reversed;
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc))).Session;
        var result = Stay(session);
        Assert.Equal(new ActorRef("lookout"), Assert.Single(result.Observations, o => o.Kind == "ai-target").Target);
        Assert.Equal(new ActorRef?[] { new("swordsman"), new("lookout") },
            result.Observations.Where(o => o.Kind == "thinking-rng").Select(o => o.Target));
    }

    [Fact]
    public void NoncriticalCohortChoosesLargestMovementRatherThanNearestAndNeedsNoClass()
    {
        var doc = Configure("stone-court");
        doc["actors"]![0]!["classRule"] = "ordinary"; doc["actors"]![1]!["classRule"] = "ordinary";
        doc["actors"]![2]!["move"] = 8;
        var placements = doc["encounters"]![0]!["placements"]!;
        placements[0]!["x"] = 1; placements[0]!["y"] = 1;
        placements[1]!["x"] = 1; placements[1]!["y"] = 4;
        placements[2]!["x"] = 7; placements[2]!["y"] = 3;
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc))).Session;
        var result = Stay(session);
        var candidates = result.Observations.Where(o => o.Kind == "ai-candidate").ToArray();
        Assert.Equal(new long?[] { 12, 14 }, candidates.Select(o => o.Before));
        Assert.Equal(new long?[] { 1, 1 }, candidates.Select(o => o.After));
        Assert.Equal(new ActorRef("swordsman"), Assert.Single(result.Observations, o => o.Kind == "ai-target").Target);
        Assert.Contains(result.Observations, o => o.Kind == "movement" && o.Actor == new ActorRef("raider"));
    }

    [Fact]
    public void UniqueHighestCandidateDoesNotRequireAnUnusedClassTable()
    {
        var doc = Configure("stone-court"); doc["start"]!["thinkingSeed"] = 0xBEEF0042u;
        doc["actors"]![0]!["classRule"] = "ordinary"; doc["actors"]![1]!["classRule"] = "ordinary";
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc))).Session;
        var result = Stay(session);
        Assert.Equal(new ActorRef("lookout"), Assert.Single(result.Observations, o => o.Kind == "ai-target").Target);
        Assert.Equal(new long?[] { 19, 1 }, result.Observations.Where(o => o.Kind == "ai-candidate").Select(o => o.After));
    }

    [Fact]
    public void NaturalContinuedThinkingChangesTargetWithoutResettingEitherStream()
    {
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(Configure("stone-court")))).Session;
        var first = Stay(session);
        Assert.Equal(new ActorRef("lookout"), Assert.Single(first.Observations, o => o.Kind == "ai-target").Target);
        var round = Stay(session);
        Assert.Equal(2, round.Snapshot.Battle.Round);
        Assert.Equal(0x97231234u, round.Snapshot.Battle.MainSeed);
        var second = Stay(session);
        Assert.Equal(new ActorRef("swordsman"), Assert.Single(second.Observations, o => o.Kind == "ai-target").Target);
        Assert.Equal(new ushort?[] { 0, 1 }, second.Observations.Where(o => o.Kind == "thinking-rng").Select(o => o.RandomValue));
        Assert.Equal(0x02EF0042L, second.Observations.First(o => o.Kind == "thinking-rng").Before);
        Assert.Equal(0x01EF0042u, second.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(0xE0E11234u, second.Snapshot.Battle.MainSeed);
        Assert.Equal(474, second.Snapshot.Battle.GetActor(new("swordsman")).Hp);
        Assert.Equal(478, second.Snapshot.Battle.GetActor(new("lookout")).Hp);
        Assert.Equal(1, second.Snapshot.Battle.GetActor(new("lookout")).Exp);
        Assert.Equal(0, second.Snapshot.Battle.GetActor(new("swordsman")).Exp);
    }

    [Theory]
    [InlineData("class", "ai-target-class")]
    [InlineData("physical", "physical-definition")]
    [InlineData("level", "level-up")]
    public void MissingRequiredCandidateDataOrLateSettlementRejectsWholeAction(string shape, string code)
    {
        var doc = Configure("stone-court");
        if (shape == "class") doc["actors"]![0]!["classRule"] = "ordinary";
        if (shape == "physical") doc["actors"]![0]!.AsObject().Remove("physical");
        if (shape == "level") { doc["start"]!["actors"]![1]!["exp"] = 99; doc["start"]!["actors"]![2]!["hp"] = 1; }
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(doc))).Session;
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.Stay));
        var before = session.Current;
        var result = Send(session, new Confirm());
        Assert.Equal(code, result.Failure!.Code); Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Equal(before.Battle.Cursor + 1, result.Snapshot.Battle.Cursor);
        Assert.Equal(before.Battle.MainSeed, result.Snapshot.Battle.MainSeed);
        Assert.Equal(before.Battle.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(before.Battle.Gold, result.Snapshot.Battle.Gold);
        foreach (var actor in before.Battle.Actors)
        {
            var after = result.Snapshot.Battle.GetActor(actor.Actor);
            Assert.Equal((actor.Hp, actor.Exp, actor.Kills, actor.Defeats, actor.Position, actor.LastTarget),
                (after.Hp, after.Exp, after.Kills, after.Defeats, after.Position, after.LastTarget));
        }
        Assert.Equal("action-committed", Assert.Single(result.Observations).Kind);
    }

    [Theory]
    [InlineData("unpromoted-swordsman")]
    [InlineData("unpromoted-warrior")]
    public void OneClassDefinitionCannotContradictItsPromotionRule(string classRule)
    {
        var doc = Configure("stone-court");
        doc["actors"]![0]!["classRule"] = classRule; doc["actors"]![0]!["physical"]!["promoted"] = true;
        Assert.Equal("class-promotion", Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(doc))).Failure.Code);
    }

    private static JsonNode Configure(string package, bool swapClass = false)
    {
        var doc = Document(package);
        doc["start"]!["mainSeed"] = (55u << 16) | 0x1234u;
        doc["start"]!["thinkingSeed"] = 0x00EF0042u;
        foreach (int index in new[] { 0, 1, 2 })
        {
            doc["start"]!["actors"]![index]!["hp"] = 500; doc["actors"]![index]!["maxHp"] = 500;
            doc["actors"]![index]!["defense"] = 4;
            doc["actors"]![index]!["attack"] = index == 2 ? 30 : 18;
            doc["actors"]![index]!["physical"]!["critical"] = new JsonObject {
                ["chance"] = index == 2 ? "one-in-16" : "one-in-32",
                ["damageBonus"] = index == 2 ? "quarter" : "half" };
        }
        doc["actors"]![0]!["classRule"] = swapClass ? "unpromoted-swordsman" : "unpromoted-warrior";
        doc["actors"]![1]!["classRule"] = swapClass ? "unpromoted-warrior" : "unpromoted-swordsman";
        doc["actors"]![2]!["controller"] = "commandset06-script3"; doc["actors"]![2]!["move"] = 1;
        var placements = doc["encounters"]![0]!["placements"]!;
        placements[1]!["x"] = placements[2]!["x"]!.GetValue<int>() - 1;
        placements[1]!["y"] = placements[2]!["y"]!.GetValue<int>();
        return doc;
    }
}
