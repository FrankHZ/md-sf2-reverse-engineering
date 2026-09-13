using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PhysicalBattleTests
{
    [Theory]
    [InlineData("stone-court", false, "swordsman", "lookout", "raider", "scavenger", 100, 23, 48)]
    [InlineData("stone-court", true, "swordsman", "lookout", "raider", "scavenger", 100, 23, 48)]
    [InlineData("river-post", false, "lancer", "runner", "intruder", "outlaw", 200, 48, 98)]
    [InlineData("river-post", true, "lancer", "runner", "intruder", "outlaw", 200, 48, 98)]
    public void BothKillOrdersUseRealCommandsAndReturnToTheNextLivingActor(string package, bool reverse,
        string player, string nextPlayer, string left, string right, int gold, int firstExp, int finalExp)
    {
        var session = Start(package);
        var attacker = new ActorRef(player);
        string first = reverse ? right : left, second = reverse ? left : right;
        Assert.Equal(attacker, session.Current.Selection!.Actor);
        Assert.Equal(0x3EB11234u, session.Current.Battle.MainSeed);
        var firstResult = Attack(session, first);
        var rolls = firstResult.Observations.Where(o => o.Kind.StartsWith("rng-", StringComparison.Ordinal)).ToArray();
        Assert.Equal(new ushort?[] { 32, package == "stone-court" ? (ushort)16 : (ushort)32, 4, 4, 16, 16 },
            rolls.Select(o => o.RandomRange));
        Assert.Equal(new ushort?[] { 5, package == "stone-court" ? (ushort)6 : (ushort)12, 0, 2, 9, 0 },
            rolls.Select(o => o.RandomValue));
        Assert.Equal(0x3EB11234L, rolls[0].Before);
        for (int i = 1; i < rolls.Length; i++) Assert.Equal(rolls[i - 1].After, rolls[i].Before);
        Assert.Equal(0x0A7F1234u, session.Current.Battle.MainSeed);
        Assert.Equal(firstExp, session.Current.Battle.GetActor(attacker).Exp);
        Assert.Equal((uint)(gold + (reverse ? 20 : 19)), session.Current.Battle.Gold);
        Assert.Equal(new ActorRef(nextPlayer), session.Current.Selection!.Actor);
        Assert.Contains(firstResult.Observations, o => o.Kind == "dead-entry-skipped" && o.Actor == new ActorRef(first));
        Assert.Contains(firstResult.Observations, o => o.Kind == "ai-stay");
        Assert.Null(session.Current.Battle.GetActor(new(first)).Position);
        Stay(session);
        Assert.Equal(2, session.Current.Battle.Round);
        Assert.Equal(attacker, session.Current.Selection!.Actor);
        Assert.Equal(0xBDCB1234u, session.Current.Battle.MainSeed);
        Attack(session, second);
        Assert.Equal(0xC0491234u, session.Current.Battle.MainSeed);
        Assert.Equal(finalExp, session.Current.Battle.GetActor(attacker).Exp);
        Assert.Equal((uint)(gold + 39), session.Current.Battle.Gold);
        Assert.Equal(2, session.Current.Battle.GetActor(attacker).Kills);
        Assert.Equal(new ActorRef(nextPlayer), session.Current.Selection!.Actor);
        Assert.Equal(0xBEEF0042u, session.Current.Battle.ThinkingSeed);
        // Both defeated cells are now legal destinations; occupancy reads persistent cleanup.
        Stay(session);
        var origin = session.Current.Battle.GetActor(attacker).Position!;
        Accept(session, new Move(ExplorationDirection.West));
        Assert.Equal(new MapPosition(origin.X - 1, origin.Y), session.Current.Selection!.Preview.Destination);
        Assert.Equal(origin, session.Current.Battle.GetActor(attacker).Position);
        Accept(session, new Cancel());
        Accept(session, new Move(ExplorationDirection.East));
        Accept(session, new Confirm());
        Accept(session, new ChooseAction(SessionAction.Stay));
        Accept(session, new Confirm());
        Assert.Equal(new MapPosition(origin.X + 1, origin.Y), session.Current.Battle.GetActor(attacker).Position);
    }

    [Theory]
    [InlineData(0, false, false, 499, 0x62B51234u, 8, 1)]
    [InlineData(14, false, true, 499, 0xA3BB1234u, 8, 1)]
    [InlineData(62, true, false, 500, 0x1A821234u, 5, 1)]
    public void NaturalRollsPreserveDodgeCriticalSpreadAndMinimumExperience(int seed, bool dodge, bool critical,
        int hp, uint afterSeed, int draws, int exp)
    {
        var session = Start("stone-court", d => ConfigureNonlethal(d, seed));
        var result = Attack(session, "raider");
        Assert.Equal(hp, session.Current.Battle.GetActor(new("raider")).Hp);
        Assert.Equal(afterSeed, session.Current.Battle.MainSeed);
        Assert.Equal(exp, session.Current.Battle.GetActor(new("swordsman")).Exp);
        Assert.Equal(draws, result.Observations.Count(o => o.Kind.StartsWith("rng-", StringComparison.Ordinal)));
        Assert.Equal(dodge, result.Observations.Any(o => o.Kind == "dodge"));
        Assert.Equal(critical, result.Observations.Any(o => o.Kind == "critical"));
        Assert.Equal(100u, session.Current.Battle.Gold);
    }

    [Theory]
    [InlineData(2, "physical-double")]
    [InlineData(52, "physical-counter")]
    public void ReachedFollowupsRejectTheWholeProvisionalAction(int seed, string code)
    {
        var session = Start("stone-court", d => ConfigureNonlethal(d, seed));
        Select(session, "raider");
        var before = session.Current;
        var result = Send(session, new Confirm());
        Assert.Equal(code, result.Failure!.Code);
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure.Kind);
        Assert.Same(before, session.Current);
        Assert.Empty(result.Observations);
        // Retry is deterministic; neither failed temporary hit nor RNG becomes persistent.
        Assert.Equal(code, Send(session, new Confirm()).Failure!.Code);
        Assert.Same(before, session.Current);
    }

    [Theory]
    [InlineData('1', 474)]
    [InlineData('2', 470)]
    [InlineData('3', 477)]
    public void LandReductionPrecedesCriticalBonusAndTheSharedDownwardSpread(char tile, int hp)
    {
        var session = Start("stone-court", d =>
        {
            ConfigureNonlethal(d, 14);
            d["actors"]![0]!["attack"] = 30;
            var row = d["terrains"]![0]!["rows"]![3]!.GetValue<string>().ToCharArray();
            row[2] = tile;
            d["terrains"]![0]!["rows"]![3] = new string(row);
        });
        var result = Attack(session, "raider");
        Assert.Equal(hp, session.Current.Battle.GetActor(new("raider")).Hp);
        Assert.Contains(result.Observations, o => o.Kind == "critical");
        Assert.Equal(0xA3BB1234u, session.Current.Battle.MainSeed);
    }

    [Theory]
    [InlineData("level", "level-up")]
    [InlineData("leader", "leader-defeat-program")]
    [InlineData("last-enemy", "battle-outcome-program")]
    public void SettlementBoundariesLeaveMovementHpResourcesRngAndSelectionUntouched(string boundary, string code)
    {
        var session = Start("stone-court", d =>
        {
            if (boundary == "level") d["actors"]![0]!["exp"] = 99;
            if (boundary == "leader") d["actors"]![2]!["physical"]!["leader"] = true;
            if (boundary == "last-enemy")
                foreach (int index in new[] { 3, 4 }) d["actors"]![index]!["hp"] = 0;
            d["encounters"]![0]!["placements"]![2]!["x"] = 2;
            d["encounters"]![0]!["placements"]![2]!["y"] = 2;
        });
        Accept(session, new Move(ExplorationDirection.North));
        Select(session, "raider");
        var before = session.Current;
        var result = Send(session, new Confirm());
        Assert.Equal(code, result.Failure!.Code);
        Assert.Same(before, result.Snapshot);
        Assert.Same(before, session.Current);
        Assert.Equal(new MapPosition(3, 3), before.Battle.GetActor(new("swordsman")).Position);
        Assert.Equal(new MapPosition(3, 2), before.Selection!.Preview.Destination);
        Assert.Empty(result.Observations);
    }

    [Theory]
    [InlineData(1, false, 1, 50)]
    [InlineData(4, false, 1, 40)]
    [InlineData(5, false, 1, 30)]
    [InlineData(6, false, 1, 20)]
    [InlineData(7, false, 1, 10)]
    [InlineData(8, false, 1, 0)]
    [InlineData(1, true, 18, 40)]
    public void EffectiveLevelControlsDamageAndKillExperience(int level, bool promoted, int target, int expected)
    {
        Assert.Equal(expected, BattleRewards.KillExperience(level, promoted, target));
        Assert.Equal(expected / 2, BattleRewards.DamageExperience(10, 20, expected));
    }

    [Fact]
    public void GoldAndKillCountsSaturateAfterAnActualDeath()
    {
        var session = Start("stone-court", d =>
        {
            d["encounters"]![0]!["rewards"]!["initialGold"] = 9999990;
            d["actors"]![0]!["physical"]!["kills"] = 9999;
        });
        Attack(session, "raider");
        Assert.Equal(9999999u, session.Current.Battle.Gold);
        Assert.Equal(9999, session.Current.Battle.GetActor(new("swordsman")).Kills);
        Assert.Equal(9999999u, BattleRewards.Gold(uint.MaxValue, 30));
    }

    [Theory]
    [InlineData("special", "taros", "physical-special-rule")]
    [InlineData("movementType", "hovering", "physical-movement-type")]
    public void ContentDoesNotEraseOriginalSpecialRules(string field, string value, string code)
    {
        var document = Document("stone-court");
        document["actors"]![2]!["physical"]![field] = value;
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal(code, failed.Failure.Code);
        Assert.Equal(SessionFailureKind.UnsupportedCapability, failed.Failure.Kind);
    }

    [Fact]
    public void ADeadLeaderCannotEnterAsAnOrdinaryContinuingEncounter()
    {
        var document = Document("stone-court");
        document["actors"]![2]!["hp"] = 0;
        document["actors"]![2]!["physical"]!["leader"] = true;
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal("leader-defeat-program", failed.Failure.Code);
        Assert.Equal(SessionFailureKind.UnsupportedCapability, failed.Failure.Kind);
    }

    private static void ConfigureNonlethal(JsonNode d, int seed)
    {
        d["start"]!["mainSeed"] = ((uint)seed << 16) | 0x1234u;
        d["actors"]![0]!["attack"] = 5;
        d["actors"]![2]!["hp"] = 500;
        d["actors"]![2]!["maxHp"] = 500;
    }
    private static void Select(GameSession session, string target)
    {
        Accept(session, new Confirm());
        Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(new(target)));
    }
    private static SessionResult Attack(GameSession session, string target)
    {
        Select(session, target);
        return Accept(session, new Confirm());
    }
}
