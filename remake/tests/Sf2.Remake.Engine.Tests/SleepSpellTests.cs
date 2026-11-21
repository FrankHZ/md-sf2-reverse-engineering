using System.Text.Json.Nodes;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.SilenceSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class SleepSpellTests
{
    private static readonly SpellRef Sleep = new("sleep", 1);
    private static readonly BattleActionRef Magic = new(BattleActionKind.SupportSpell, Sleep);

    private static void Book(JsonNode document)
    {
        document["spells"]!.AsArray().Add(new JsonObject { ["id"] = "sleep", ["level"] = 1, ["mpCost"] = 4,
            ["minimumRange"] = 1, ["maximumRange"] = 2, ["effect"] = new JsonObject { ["kind"] = "sleep", ["radius"] = 1 } });
        document["actors"]![0]!["level"] = 25;
        Learn(document["actors"]![0]!);
        document["terrains"]![0]!["rows"]![2] = "pppppppppp";
    }

    private static void Learn(JsonNode actor) =>
        actor["spells"]!.AsArray().Add(new JsonObject { ["id"] = "sleep", ["level"] = 1 });

    private static BattleActionResolution Cast(EngineBattleState battle, ActorRef caster, ActorRef target)
    {
        var rules = RuleCompositions.Sf2();
        return BattleActionRules.Prepare(rules.Action(Magic), battle, caster, battle.GetActor(caster).Position!,
            Magic, target, rules.Progression);
    }

    [Theory]
    [InlineData(0, 2600, 4, 0x0040, false)]
    [InlineData(0, 3200, 5, 0x0040, true)]
    [InlineData(1, 3200, 5, 0x0080, false)]
    [InlineData(1, 3800, 6, 0x00C0, true)]
    [InlineData(2, 3800, 6, 0x0040, false)]
    [InlineData(2, 4500, 7, 0x0040, true)]
    [InlineData(3, 4500, 7, 0x00C0, false)]
    public void SleepRollsOnceAndOnlyAHitWritesAndRefreshes(int setting, int seed, int roll, int counter, bool hit)
    {
        var initial = Battle(document =>
        {
            Book(document);
            var enemy = document["actors"]![2]!;
            enemy["resistance"] = setting << 14;
            enemy["spells"] = new JsonArray();
        });
        var battle = initial.With(mainSeed: (uint)seed << 16, actors: initial.Actors.Select(actor => actor.Actor == Smoke
            ? actor.With(status: (ushort)(0x1000 | counter), refreshStats: true).With(status: (ushort)(0x3000 | counter))
            : actor));
        var cast = Cast(battle, Mage, Smoke);
        var reaction = Assert.Single(cast.Reactions);
        Assert.Equal(hit ? BattleReactionKind.Status : BattleReactionKind.Ineffective, reaction.Kind);
        Assert.Equal(new[] { "rng-sleep", "rng-exp-plus", "rng-exp-minus" }, cast.ConstructionEffects.Select(e => e.Kind));
        var attempt = BattleRandom.NextMain(battle.MainSeed, 8);
        Assert.Equal((ushort?)roll, cast.ConstructionEffects[0].RandomValue);
        var plus = BattleRandom.NextMain(attempt.After, 16);
        var minus = BattleRandom.NextMain(plus.After, 16);
        Assert.Equal(minus.After, cast.Prepared.MainSeed);
        Assert.Equal(Math.Max(1, (hit ? 5 : 0) + (plus.Value == 0 ? 1 : 0) - (minus.Value == 0 ? 1 : 0)),
            cast.Reward!.Amount);
        Assert.Same(battle.GetActor(Smoke), cast.Prepared.GetActor(Smoke));
        var paid = cast.ApplySpellCost(cast.Prepared);
        Assert.Equal(16, paid.GetActor(Mage).Mp);
        var target = cast.ApplyReaction(paid, reaction).GetActor(Smoke);
        Assert.Equal(hit ? 0x30C0 : 0x3000 | counter, target.Status);
        Assert.Equal(hit ? 0x3000 : 0x1000, target.AppliedStatBits);
        Assert.Equal(battle.GetActor(Smoke).Hp, target.Hp);
        Assert.Equal(battle.GetActor(Smoke).Mp, target.Mp);
    }

    [Fact]
    public void OpposingCentersAndAreaUseDeploymentOrderWithoutExtraRolls()
    {
        var battle = Battle(document =>
        {
            Book(document);
            document["terrains"]![0]!["rows"]![1] = "pppppppppp";
            document["terrains"]![0]!["rows"]![3] = "pppppppppp";
            var encounter = document["encounters"]![0]!;
            var initialization = encounter["initialization"]!;
            initialization["enemyCount"] = 6;
            foreach (var (id, slot, x, y) in new[] { ("outside", 133, 6, 3), ("unplaced", 132, 5, 1),
                ("neutral", 131, 6, 2), ("dead", 130, 4, 2), ("neighbor", 129, 5, 3) })
            {
                var actor = document["actors"]![2]!.DeepClone();
                actor["id"] = id;
                document["actors"]!.AsArray().Add(actor);
                var placement = encounter["placements"]![2]!.DeepClone();
                placement["actor"] = id; placement["processingOrder"] = slot;
                placement["x"] = x; placement["y"] = y;
                encounter["placements"]!.AsArray().Add(placement);
                var state = document["start"]!["actors"]![2]!.DeepClone();
                state["actor"] = id;
                if (id == "dead") state["hp"] = 0;
                if (id == "neutral") state["initialization"]!["activationWord"] = 9;
                document["start"]!["actors"]!.AsArray().Add(state);
                initialization["sourceCombatants"]!.AsArray().Add(new JsonObject { ["slot"] = slot, ["actor"] = id });
            }
            encounter["rewards"]!["halvedExperience"] = true;
        });
        battle = battle.With(actors: battle.Actors.Select(actor => actor.Actor.Value == "unplaced" ? actor.With(clearPosition: true) : actor));
        var cast = Cast(battle, Mage, Smoke);
        Assert.Equal(new[] { "smoke", "neighbor" }, cast.Reactions.Select(r => r.Target.Value));
        Assert.Equal(new[] { BattleReactionKind.Status, BattleReactionKind.Ineffective }, cast.Reactions.Select(r => r.Kind));
        Assert.Equal(new[] { "rng-sleep", "rng-sleep", "rng-exp-plus", "rng-exp-minus" }, cast.ConstructionEffects.Select(e => e.Kind));
        Assert.Equal(2, cast.Reward!.Amount);
        foreach (ActorRef invalid in new ActorRef[] { Priest, new("dead"), new("neutral"), new("unplaced"), new("neighbor") })
            Assert.Throws<BattleRuleException>(() => Cast(battle, Mage, invalid));
    }

    [Fact]
    public void LearnedCastCanCancelThenPayAndCauseTheRealTurnSkip()
    {
        var session = EngineTestContent.Start("silence-watch", document =>
        {
            Book(document);
            // Eleven initial queue draws advance this controlled seed to the known 0x1234 effect input.
            document["start"]!["mainSeed"] = 0xFF5F0000u;
        });
        var before = session.Current.Battle;
        EngineTestContent.Accept(session, new Confirm());
        EngineTestContent.Accept(session, new SelectSpell(Sleep));
        EngineTestContent.Accept(session, new Cancel());
        Assert.Same(before, session.Current.Battle);
        EngineTestContent.Accept(session, new Confirm());
        EngineTestContent.Accept(session, new SelectSpell(Sleep));
        EngineTestContent.Accept(session, new SelectTarget(Smoke));
        var done = EngineTestContent.FinishBattleScenes(session, EngineTestContent.Accept(session, new Confirm()));
        Assert.Equal(16, session.Current.Battle.GetActor(Mage).Mp);
        Assert.Equal((byte?)6, session.Current.Battle.GetActor(Mage).Exp);
        Assert.Equal(0xC0, session.Current.Battle.GetActor(Smoke).Status);
        Assert.Contains(done.Observations, row => row.Kind == "status" && row.Actor == Smoke && row.After == 0xC0);
        Assert.Equal(Priest, session.Current.Selection!.Actor);

        var skipped = EngineTestContent.Stay(session);
        Assert.Contains(skipped.Observations, row => row.Kind == "sleeping-action-skipped" && row.Actor == Smoke);
        Assert.Equal(354, session.Current.BattleScene!.SleepTurnText);
        EngineTestContent.FinishBattleScenes(session, skipped);
        Assert.Equal(0, session.Current.Battle.GetActor(Smoke).Status);
        Assert.True(session.Current.HasBattleControl);
    }

    [Fact]
    public void EnemyCastUsesOpposingTargetsAndSilencedAllyPaysWithoutResolution()
    {
        var battle = Battle(document => { Book(document); Learn(document["actors"]![2]!); });
        var enemy = Cast(battle, Smoke, Mage);
        Assert.Equal(new[] { Mage, Priest }, enemy.Reactions.Select(r => r.Target));
        Assert.Equal(new[] { "rng-sleep", "rng-sleep" }, enemy.ConstructionEffects.Select(e => e.Kind));
        Assert.Null(enemy.Reward);
        Assert.Equal(1, enemy.ApplySpellCost(enemy.Prepared).GetActor(Smoke).Mp);
        Assert.Throws<BattleRuleException>(() => Cast(battle, Smoke, Smoke));

        var silenced = Cast(Status(battle, Mage, 0x300), Mage, Smoke);
        Assert.NotNull(silenced.Silenced);
        Assert.Empty(silenced.Reactions);
        Assert.Empty(silenced.ConstructionEffects);
        Assert.Null(silenced.Reward);
        Assert.Equal(battle.MainSeed, silenced.Prepared.MainSeed);
        Assert.Equal(0x300, silenced.ApplySpellCost(silenced.Prepared).GetActor(Mage).Status);
        Assert.Equal(16, silenced.ApplySpellCost(silenced.Prepared).GetActor(Mage).Mp);
        var dry = battle.With(actors: battle.Actors.Select(actor => actor.Actor == Mage ? actor.With(mp: 3) : actor));
        Assert.Equal("insufficient-mp", Assert.Throws<BattleRuleException>(() => Cast(dry, Mage, Smoke)).Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x40)]
    public void SettledBattleSaveRestoresThePendingSleepConsumer(int status)
    {
        var (definition, initial) = BattleSaveFixture.Create();
        var current = BattleSaveFixture.At(Status(initial.Battle, new("enemy"), (ushort)status), initial.Story);
        var save = BattleSaveFixture.Capture(definition, current);
        var wire = OpeningSaveJson.ReadRecord(OpeningSaveJson.WriteRecord(new BattleSaveRecord(
            1, 1, DateTimeOffset.UnixEpoch, save)), 1);
        var restored = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, wire, RuleCompositions.Sf2())).Session;
        BattleSaveFixture.EqualBattle(current.Battle, restored.Current.Battle);
        var live = BattleSaveFixture.Stay(current);
        var cold = BattleSaveFixture.Stay(restored.Current);
        BattleSaveFixture.EqualBattle(live.Battle, cold.Battle);
        Assert.Equal(0, cold.Battle.GetActor(new("enemy")).Status);
    }
}
