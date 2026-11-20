using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.SilenceSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class SupportSpellActionTests
{
    private static void Book(JsonNode d)
    {
        foreach (var (id, rank, mp, min, max, radius) in new[] { ("boost", 1, 2, 0, 1, 1), ("slow", 1, 3, 1, 1, 1), ("slow", 2, 6, 1, 2, 2) })
        {
            d["spells"]!.AsArray().Add(new JsonObject { ["id"] = id, ["level"] = rank, ["mpCost"] = mp,
                ["minimumRange"] = min, ["maximumRange"] = max, ["effect"] = new JsonObject { ["kind"] = id, ["radius"] = radius } });
            d["actors"]![0]!["spells"]!.AsArray().Add(new JsonObject { ["id"] = id, ["level"] = rank });
        }
        d["terrains"]![0]!["rows"]![2] = "pppppp#ppp";
        d["actors"]![0]!["defense"] = 40;
        d["actors"]![1]!["defense"] = 40;
        d["actors"]![1]!["agility"] = 24;
        d["encounters"]![0]!["rewards"]!["halvedExperience"] = true;
    }

    private static BattleActionResolution Cast(EngineBattleState battle, string name, byte lv, ActorRef center, MapPosition? destination = null) =>
        BattleActionRules.Prepare(new Sf2SupportSpellAction(), battle, Mage, destination ?? battle.GetActor(Mage).Position!,
            new(BattleActionKind.SupportSpell, new(name, lv)), center, new Sf2BattleProgressionRule());

    [Fact]
    public void FreshBoostConstructsWordsThenPaysWithOldSelfWordAndRefreshesInTargetOrder()
    {
        var battle = Battle(Book);
        var action = Cast(battle, "boost", 1, Priest);
        Assert.Equal(new[] { Mage, Priest }, action.Reactions.Select(r => r.Target));
        Assert.All(action.Prepared.Actors.Where(a => a.IsAlly), a =>
        { Assert.Equal(0x3000, a.Status); Assert.Equal(40, a.Defense); });
        Assert.DoesNotContain(action.ConstructionEffects, e => e.RandomRange == 8);
        Assert.InRange(action.Reward!.Amount, 9, 11);
        var paid = action.ApplySpellCost(action.Prepared);
        Assert.Equal(18, paid.GetActor(Mage).Mp);
        Assert.Equal(0, paid.GetActor(Mage).Status);
        var first = action.ApplyReaction(paid, action.Reactions[0]);
        Assert.Equal(55, first.GetActor(Mage).Defense);
        Assert.Equal(40, first.GetActor(Priest).Defense);
        var second = action.ApplyReaction(first, action.Reactions[1]);
        Assert.Equal(55, second.GetActor(Priest).Defense);
        Assert.Equal(33, second.GetActor(Priest).Agility);
    }

    [Fact]
    public void FailedBoostRecastRestoresSelfButLeavesTheOtherTargetsDurationAhead()
    {
        var initial = Battle(Book);
        var battle = initial.With(actors: initial.Actors.Select(a => a.IsAlly ? a.With(status: 0x1000, refreshStats: true) : a));
        var cast = Cast(battle, "boost", 1, Mage);
        Assert.Equal(2, cast.ConstructionEffects.Count(e => e.Kind == "rng-boost-recast"));
        Assert.All(cast.Reactions, r => Assert.Equal(BattleReactionKind.Ineffective, r.Kind));
        var done = cast.Reactions.Aggregate(cast.ApplySpellCost(cast.Prepared), cast.ApplyReaction);
        Assert.Equal(0x1000, done.GetActor(Mage).Status);
        Assert.Equal(0x3000, done.GetActor(Priest).Status);
        Assert.Equal(45, done.GetActor(Priest).Defense);
        Assert.Equal(27, done.GetActor(Priest).Agility);
        Assert.Equal(1, cast.Reward!.Amount);
        Assert.Equal(18, done.GetActor(Mage).Mp);
    }

    [Fact]
    public void OrdinaryBoostChoiceCanCancelThenShowsEachAllysAgiAndDefenseBeforeReturningControl()
    {
        var session = EngineTestContent.Start("silence-watch", Book);
        var initial = session.Current.Battle;
        SpellRef spell = new("boost", 1);
        EngineTestContent.Accept(session, new Confirm());
        EngineTestContent.Accept(session, new SelectSpell(spell));
        EngineTestContent.Accept(session, new Cancel());
        Assert.Same(initial, session.Current.Battle);
        EngineTestContent.Accept(session, new Confirm());
        EngineTestContent.Accept(session, new SelectSpell(spell));
        EngineTestContent.Accept(session, new SelectTarget(Priest));
        EngineTestContent.Accept(session, new Confirm());

        List<(ActorRef?, BattleScenePhase, int)> messages = [];
        while (session.Current.BattleScene is { } scene)
        {
            if (scene.Phase is BattleScenePhase.ResultMessage or BattleScenePhase.SupportDefenseMessage)
            {
                Assert.Null(scene.DisplayedEnemy);
                Assert.Equal(scene.Target, scene.DisplayedAlly);
                messages.Add((scene.DisplayedAlly, scene.Phase, scene.SupportStatAmount));
            }
            EngineTestContent.Accept(session, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                new CompletePresentation(scene.Token, scene.CompletionKind));
        }
        Assert.Equal([(Mage, BattleScenePhase.ResultMessage, 37), (Mage, BattleScenePhase.SupportDefenseMessage, 15),
            (Priest, BattleScenePhase.ResultMessage, 9), (Priest, BattleScenePhase.SupportDefenseMessage, 15)], messages);
        Assert.True(session.Current.HasBattleControl);
        Assert.Equal(0x2000, session.Current.Battle.GetActor(Mage).Status);
        Assert.Equal(0x3000, session.Current.Battle.GetActor(Priest).Status);
        Assert.Equal(18, session.Current.Battle.GetActor(Mage).Mp);
        Assert.InRange((int)session.Current.Battle.GetActor(Mage).Exp!, 9, 11);
    }

    [Theory]
    [InlineData(0, 1, true)]
    [InlineData(1, 1, true)]
    [InlineData(2, 2, true)]
    [InlineData(3, 2, false)]
    public void SlowUsesStatusResistanceAndOnlyReachedRecastConsumesSecondRoll(int resistance, byte rank, bool pass)
    {
        var original = Battle(d => { Book(d); d["actors"]![2]!["resistance"] = resistance << 14; }).With(mainSeed: 0x12340000);
        var destination = new MapPosition(4, 2);
        var fresh = Cast(original, "slow", rank, Smoke, destination);
        Assert.Equal(1, fresh.ConstructionEffects.Count(e => e.RandomRange == 8));
        var hit = Assert.Single(fresh.Reactions);
        Assert.Equal(pass ? BattleReactionKind.Status : BattleReactionKind.Ineffective, hit.Kind);
        Assert.Equal(pass ? 0x0C00 : 0, fresh.Prepared.GetActor(Smoke).Status);
        Assert.Equal(rank == 1 ? 17 : 14, fresh.ApplySpellCost(fresh.Prepared).GetActor(Mage).Mp);
        var active = original.With(actors: original.Actors.Select(a => a.Actor == Smoke ? a.With(status: 0x1400, refreshStats: true) : a));
        var recast = Cast(active, "slow", rank, Smoke, destination);
        Assert.Equal(pass ? 2 : 1, recast.ConstructionEffects.Count(e => e.RandomRange == 8));
        Assert.Equal(BattleReactionKind.Ineffective, Assert.Single(recast.Reactions).Kind);
        Assert.Equal(pass ? 0x1C00 : 0x1400, recast.Prepared.GetActor(Smoke).Status);
        Assert.Equal(active.GetActor(Smoke).Defense, recast.Prepared.GetActor(Smoke).Defense);
        Assert.Equal(active.GetActor(Smoke).Agility, recast.Prepared.GetActor(Smoke).Agility);
        Assert.InRange(fresh.Reward!.Amount, 1, 3);
    }

    [Fact]
    public void SlowRollsFollowDeploymentProcessingOrderWhenAuthoredSourceSlotsDiffer()
    {
        var initial = Battle(doc =>
        {
            Book(doc);
            var encounter = doc["encounters"]![0]!;
            var init = encounter["initialization"]!;
            var smoke = doc["actors"]![2]!;
            smoke["resistance"] = 1 << 14;
            var second = smoke.DeepClone();
            second["id"] = "second";
            doc["actors"]!.AsArray().Add(second);
            var smokePlacement = encounter["placements"]![2]!;
            smokePlacement["processingOrder"] = 129;
            var placement = smokePlacement.DeepClone();
            placement["actor"] = "second"; placement["processingOrder"] = 128;
            placement["x"] = 4; placement["y"] = 1;
            encounter["placements"]!.AsArray().Add(placement);
            var state = doc["start"]!["actors"]![2]!.DeepClone();
            state["actor"] = "second";
            doc["start"]!["actors"]!.AsArray().Add(state);
            init["enemyCount"] = 2;
            init["sourceCombatants"]!.AsArray().Add(new JsonObject { ["slot"] = 129, ["actor"] = "second" });
        });
        var cast = Cast(initial.With(mainSeed: 0x12340000), "slow", 2, Smoke);
        Assert.Equal(new[] { ("second", 7), (Smoke.Value, 0) },
            cast.ConstructionEffects.Where(e => e.Kind == "rng-slow").Select(e => (e.Target!.Value.Value, (int)e.RandomValue!.Value)));
        Assert.Equal(0, cast.Prepared.GetActor(Smoke).Status);
        Assert.Equal(0x0C00, cast.Prepared.GetActor(new("second")).Status);
    }

    [Fact]
    public void SlowAreaConsumesOnlyReachedRollsInSourceSlotOrder()
    {
        var original = Battle(d =>
        {
            Book(d);
            var encounter = d["encounters"]![0]!;
            var init = encounter["initialization"]!;
            var extra = new[] { ("active", 4, 1), ("immune", 6, 1), ("neutral", 5, 4), ("dead", 4, 3), ("outside", 8, 2) };
            init["enemyCount"] = 1 + extra.Length;
            foreach (var (id, x, y) in extra.Reverse())
            {
                int slot = 129 + Array.FindIndex(extra, e => e.Item1 == id);
                var actor = d["actors"]![2]!.DeepClone();
                actor["id"] = id;
                if (id == "immune") actor["resistance"] = 3 << 14;
                d["actors"]!.AsArray().Add(actor);
                var placement = encounter["placements"]![2]!.DeepClone();
                placement["actor"] = id; placement["processingOrder"] = slot;
                placement["x"] = x; placement["y"] = y;
                encounter["placements"]!.AsArray().Add(placement);
                var input = d["start"]!["actors"]![2]!.DeepClone();
                input["actor"] = id;
                if (id == "dead") input["hp"] = 0;
                if (id == "neutral") input["initialization"]!["activationWord"] = 9;
                d["start"]!["actors"]!.AsArray().Add(input);
                init["sourceCombatants"]!.AsArray().Add(new JsonObject { ["slot"] = slot, ["actor"] = id });
            }
        });
        var battle = original.With(actors: original.Actors.Select(a => a.Actor.Value == "active" ?
            a.With(status: 0x0400, refreshStats: true) : a));
        var cast = Cast(battle, "slow", 2, Smoke);
        Assert.Equal(new[] { "smoke", "active", "immune" }, cast.Reactions.Select(r => r.Target.Value));
        Assert.Equal(new[] { BattleReactionKind.Status, BattleReactionKind.Ineffective, BattleReactionKind.Ineffective },
            cast.Reactions.Select(r => r.Kind));
        Assert.Equal(new[] { "rng-slow", "rng-slow", "rng-slow-recast", "rng-slow", "rng-exp-plus", "rng-exp-minus" },
            cast.ConstructionEffects.Select(e => e.Kind));
        uint seed = battle.MainSeed;
        foreach (ushort range in new ushort[] { 8, 8, 8, 8, 16, 16 }) seed = BattleRandom.NextMain(seed, range).After;
        Assert.Equal(seed, cast.Prepared.MainSeed);
        Assert.Equal((ushort?)0x0400, cast.Prepared.GetActor(new("active")).LastRefreshBits);
        Assert.Equal(0, cast.Prepared.GetActor(new("immune")).Status);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(1, 5, false)]
    [InlineData(1, 6, true)]
    [InlineData(2, 6, false)]
    [InlineData(2, 7, true)]
    [InlineData(3, 7, false)]
    public void SlowTestsTheExactResistanceThreshold(int resistance, int roll, bool success)
    {
        var battle = Battle(d => { Book(d); d["actors"]![2]!["resistance"] = resistance << 14; });
        uint seed = (uint)Enumerable.Range(0, 65536).First(i => BattleRandom.NextMain((uint)i << 16, 8).Value == roll) << 16;
        var cast = Cast(battle.With(mainSeed: seed), "slow", 2, Smoke);
        Assert.Equal(success ? BattleReactionKind.Status : BattleReactionKind.Ineffective, Assert.Single(cast.Reactions).Kind);
        Assert.Equal(roll, (int)cast.ConstructionEffects[0].RandomValue!);
    }

    [Fact]
    public void MovedCasterSuppliesBothCenterAndAreaPositionAndPaidSilenceSuppressesAllTargets()
    {
        var battle = Battle(Book);
        Assert.Equal(new[] { Mage }, Cast(battle, "boost", 1, Mage, new(4, 2)).Reactions.Select(r => r.Target));
        Assert.Throws<BattleRuleException>(() => Cast(battle, "boost", 1, Priest, new(4, 2)));
        Assert.Throws<BattleRuleException>(() => Cast(battle, "slow", 1, Smoke));
        Assert.Single(Cast(battle, "slow", 2, Smoke).Reactions);
        var blocked = Cast(Status(battle, Mage, 0x0300), "boost", 1, Priest);
        Assert.NotNull(blocked.Silenced);
        Assert.Empty(blocked.Reactions);
        Assert.Equal(battle.MainSeed, blocked.Prepared.MainSeed);
        Assert.Equal(18, blocked.ApplySpellCost(blocked.Prepared).GetActor(Mage).Mp);
        Assert.Null(blocked.Reward);
        var low = battle.With(actors: battle.Actors.Select(a => a.Actor == Mage ? a.With(mp: 1) : a));
        Assert.Equal("insufficient-mp", Assert.Throws<BattleRuleException>(() => Cast(low, "boost", 1, Priest)).Code);
    }
}
