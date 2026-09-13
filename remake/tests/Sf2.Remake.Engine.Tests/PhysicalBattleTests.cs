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
    [InlineData("stone-court", 2, 500, 456, 2, 0x5BDD1234u, 14, "first,second")]
    [InlineData("stone-court", 55, 493, 478, 2, 0x557E1234u, 14, "first,counter")]
    [InlineData("stone-court", 52, 500, 478, 1, 0xD22E1234u, 11, "first,counter")]
    [InlineData("stone-court", 73, 493, 453, 2, 0x3A1E1234u, 20, "first,second,counter")]
    [InlineData("stone-court", 385, 500, 447, 2, 0xCDA11234u, 17, "first,second,counter")]
    [InlineData("stone-court", 976, 493, 479, 1, 0xE1F31234u, 14, "first,counter")]
    [InlineData("river-post", 55, 491, 474, 3, 0x557E1234u, 14, "first,counter")]
    public void NaturalFollowupsResolveOrderedHitsAndOneActionAward(string package, int seed,
        int hp, int targetHp, int exp, uint finalSeed, int drawCount, string order)
    {
        var session = FollowupStart(package, seed);
        var before = session.Current;
        var actor = before.Battle.Actors[0].Actor;
        var target = before.Battle.Actors[2].Actor;
        var result = Attack(session, target.Value);
        var hits = result.Observations.Where(o => o.Kind.StartsWith("physical-", StringComparison.Ordinal)).ToArray();
        Assert.Equal(order.Split(',').Select(kind => "physical-" + kind), hits.Select(o => o.Kind));
        foreach (var hit in hits)
            Assert.Equal(hit.Kind == "physical-counter" ? (target, actor) : (actor, target), (hit.Actor!.Value, hit.Target!.Value));
        var rolls = result.Observations.Where(o => o.RandomRange is not null).ToArray();
        Assert.Equal(drawCount, rolls.Length);
        Assert.Equal((long)before.Battle.MainSeed, rolls[0].Before);
        for (int i = 1; i < rolls.Length; i++) Assert.Equal(rolls[i - 1].After, rolls[i].Before);
        Assert.Equal((long)finalSeed, rolls[^1].After);
        Assert.Equal(finalSeed, session.Current.Battle.MainSeed);
        Assert.Equal(before.Battle.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        Assert.Equal(hp, session.Current.Battle.GetActor(actor).Hp);
        Assert.Equal(targetHp, session.Current.Battle.GetActor(target).Hp);
        Assert.Equal(exp, session.Current.Battle.GetActor(actor).Exp);
        Assert.Equal(before.Battle.Gold, session.Current.Battle.Gold);
        Assert.Single(result.Observations, o => o.Kind == "exp");
        Assert.Single(result.Observations, o => o.Kind == "action-committed");
        Assert.Equal(before.Battle.Actors[1].Actor, session.Current.Selection!.Actor);
        // A counter is a reaction, not consumption of the counterattacker's queued turn.
        Assert.Contains(result.Observations, o => o.Kind == "ai-stay" && o.Actor == target);
        if (seed == 73) Assert.Equal(new ushort?[] { 0, 26, 6 },
            rolls.Where(o => o.Kind == "rng-counter").Select(o => o.RandomValue));
        if (seed == 385) Assert.Equal(new ushort?[] { 11, 0, 24 },
            rolls.Where(o => o.Kind == "rng-counter").Select(o => o.RandomValue));
        if (seed == 976) Assert.Equal((ushort)0, rolls.Last(o => o.Kind == "rng-double").RandomValue);
    }

    [Fact]
    public void ACarriedSecondRoundCanNaturallyCounterWithoutResettingRng()
    {
        var session = FollowupStart("stone-court", 42);
        Stay(session);
        Stay(session);
        Assert.Equal(2, session.Current.Battle.Round);
        Assert.Equal(0xEE281234u, session.Current.Battle.MainSeed);
        var result = Attack(session, "raider");
        Assert.Contains(result.Observations, o => o.Kind == "physical-counter");
        Assert.Equal(0xDAA61234u, session.Current.Battle.MainSeed);
        Assert.Equal(493, session.Current.Battle.GetActor(new("swordsman")).Hp);
        Assert.Equal(478, session.Current.Battle.GetActor(new("raider")).Hp);
        Assert.Equal(1, session.Current.Battle.GetActor(new("swordsman")).Exp);
    }

    [Fact]
    public void EachHitTruncatesDamageExperienceBeforeAddingToTheActionAccumulator()
    {
        var session = FollowupStart("stone-court", 2, d => d["actors"]![2]!["maxHp"] = 550);
        Attack(session, "raider");
        // Hits20/24 earn floor(1000/550)+floor(1200/550)=1+2. Halving gives1,
        // whereas incorrectly combining damage first would award2 on these same final rolls.
        Assert.Equal(1, session.Current.Battle.GetActor(new("swordsman")).Exp);
        Assert.Equal(456, session.Current.Battle.GetActor(new("raider")).Hp);
        Assert.Equal(0x5BDD1234u, session.Current.Battle.MainSeed);
    }

    [Fact]
    public void LethalSecondHitClearsEvenAnAlreadyRequestedCounterAndAwardsOneKill()
    {
        var session = FollowupStart("stone-court", 73, d => d["start"]!["actors"]![2]!["hp"] = 35);
        var result = Attack(session, "raider");
        Assert.Equal(new[] { "physical-first", "physical-second" },
            result.Observations.Where(o => o.Kind.StartsWith("physical-", StringComparison.Ordinal)).Select(o => o.Kind));
        Assert.Equal(new long?[] { 35, 11 }, result.Observations.Where(o => o.Kind == "hp").Select(o => o.Before));
        Assert.Equal(new long?[] { 11, 0 }, result.Observations.Where(o => o.Kind == "hp").Select(o => o.After));
        Assert.Equal(12, result.Observations.Count(o => o.RandomRange is not null));
        Assert.Equal(0xD1F61234u, session.Current.Battle.MainSeed);
        Assert.Equal(24, session.Current.Battle.GetActor(new("swordsman")).Exp);
        Assert.Equal(1, session.Current.Battle.GetActor(new("swordsman")).Kills);
        Assert.Equal(119u, session.Current.Battle.Gold);
        Assert.Null(session.Current.Battle.GetActor(new("raider")).Position);
        Assert.Contains(result.Observations, o => o.Kind == "dead-entry-skipped" && o.Actor == new ActorRef("raider"));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(9999, 9999)]
    public void LethalCounterCleansAnOrdinaryAllyAndSkipsExperienceAndAwardRandomness(int defeats, int expected)
    {
        var session = FollowupStart("stone-court", 55, d =>
        {
            d["start"]!["actors"]![0]!["hp"] = 1;
            d["start"]!["actors"]![0]!["exp"] = 99;
            d["start"]!["actors"]![0]!["defeats"] = defeats;
        });
        var result = Attack(session, "raider");
        var dead = session.Current.Battle.GetActor(new("swordsman"));
        Assert.Equal(0, dead.Hp);
        Assert.Null(dead.Position);
        Assert.Equal(expected, dead.Defeats);
        Assert.Equal(99, dead.Exp);
        Assert.Equal(478, session.Current.Battle.GetActor(new("raider")).Hp);
        Assert.Equal(0x4CCA1234u, session.Current.Battle.MainSeed);
        Assert.Equal(10, result.Observations.Count(o => o.RandomRange is not null));
        Assert.DoesNotContain(result.Observations, o => o.Kind is "exp" or "rng-exp-plus" or "rng-exp-minus" or "gold");
        Assert.Equal(100u, session.Current.Battle.Gold);
        Assert.Equal(new ActorRef("lookout"), session.Current.Selection!.Actor);
        Stay(session);
        Assert.Equal(2, session.Current.Battle.Round);
        Assert.Equal(new ActorRef("lookout"), session.Current.Selection!.Actor);
        Assert.DoesNotContain(session.Current.Battle.Queue, entry => entry.ActorSlot == 4);
    }

    [Fact]
    public void CounterUsesTheMovedOriginalActorsLandEffect()
    {
        var session = FollowupStart("stone-court", 55, d =>
        {
            d["encounters"]![0]!["placements"]![2]!["y"] = 2;
            d["terrains"]![0]!["rows"]![2] = "#2232222#";
        });
        Accept(session, new Move(ExplorationDirection.North));
        Attack(session, "raider");
        var actor = session.Current.Battle.GetActor(new("swordsman"));
        Assert.Equal(new MapPosition(3, 2), actor.Position);
        Assert.Equal(495, actor.Hp); // floor(14*205/256)=11, counter halves to5, spread range1.
        Assert.Equal(0x557E1234u, session.Current.Battle.MainSeed);
    }

    [Theory]
    [InlineData("level", 73, "level-up")]
    [InlineData("leader", 55, "leader-defeat-program")]
    [InlineData("last-ally", 331, "battle-outcome-program")]
    public void UnsupportedFollowupSettlementRollsBackEveryHitAndProvisionalMovement(string boundary, int seed, string code)
    {
        var session = FollowupStart("stone-court", seed, d =>
        {
            d["encounters"]![0]!["placements"]![2]!["y"] = 2;
            if (boundary == "level") d["start"]!["actors"]![0]!["exp"] = 99;
            else d["start"]!["actors"]![0]!["hp"] = 1;
            if (boundary == "leader") d["actors"]![0]!["physical"]!["leader"] = true;
            if (boundary == "last-ally") d["start"]!["actors"]![1]!["hp"] = 0;
        });
        Accept(session, new Move(ExplorationDirection.North));
        Select(session, "raider");
        var before = session.Current;
        var result = Send(session, new Confirm());
        Assert.Equal(code, result.Failure!.Code);
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure.Kind);
        Assert.Same(before, result.Snapshot);
        Assert.Same(before, session.Current);
        Assert.Empty(result.Observations);
        Assert.Equal(new MapPosition(3, 3), before.Battle.GetActor(new("swordsman")).Position);
        Assert.Equal(new MapPosition(3, 2), before.Selection!.Preview.Destination);
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
            if (boundary == "level") d["start"]!["actors"]![0]!["exp"] = 99;
            if (boundary == "leader") d["actors"]![2]!["physical"]!["leader"] = true;
            if (boundary == "last-enemy")
                foreach (int index in new[] { 3, 4 }) d["start"]!["actors"]![index]!["hp"] = 0;
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
            d["start"]!["gold"] = 9999990;
            d["start"]!["actors"]![0]!["kills"] = 9999;
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
        document["start"]!["actors"]![2]!["hp"] = 0;
        document["actors"]![2]!["physical"]!["leader"] = true;
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal("leader-defeat-program", failed.Failure.Code);
        Assert.Equal(SessionFailureKind.UnsupportedCapability, failed.Failure.Kind);
    }

    private static GameSession FollowupStart(string package, int seed, Action<JsonNode>? change = null) =>
        Start(package, d =>
        {
            d["start"]!["mainSeed"] = ((uint)seed << 16) | 0x1234u;
            foreach (int index in new[] { 0, 2 })
            {
                d["start"]!["actors"]![index]!["hp"] = 500;
                d["actors"]![index]!["maxHp"] = 500;
            }
            d["actors"]![2]!["attack"] = package == "stone-court" ? 18 : 26;
            change?.Invoke(d);
        });

    private static void ConfigureNonlethal(JsonNode d, int seed)
    {
        d["start"]!["mainSeed"] = ((uint)seed << 16) | 0x1234u;
        d["actors"]![0]!["attack"] = 5;
        d["start"]!["actors"]![2]!["hp"] = 500;
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
