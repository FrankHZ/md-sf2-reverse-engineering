using System.Text.Json.Nodes;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.SilenceSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class MuddleAllyTurnTests
{
    [Fact]
    public void MuddledPlayerAllyAttacksAnOpponentAndRecoversForItsSecondEntry()
    {
        var before = Battle(d =>
        {
            d["actors"]![0]!["spells"] = new JsonArray();
            d["terrains"]![0]!["rows"]![2] = "pppppppppp";
            d["encounters"]![0]!["placements"]![2]!["x"] = 4;
        });
        before = Status(before, Mage, 0x10);
        var (after, events) = Play(before, Mage, mainWord: 4500);
        Assert.Equal(Smoke, Assert.Single(events, e => e.Kind == "ai-target").Target);
        Assert.Contains(events, e => e.Kind == "physical-first" && e.Actor == Mage && e.Target == Smoke);
        Assert.Equal((ushort?)2, Assert.Single(events, e => e.Kind == "rng-dodge" && e.Actor == Mage).RandomRange);
        Assert.Contains(events, e => e.Kind == "muddle-status" && e.Actor == Mage && e.Before == 0x10 && e.After == 0);
        Assert.Equal(before.GetActor(Mage).LastTarget, after.Battle.GetActor(Mage).LastTarget);
        Assert.Equal(before.GetActor(Mage).ActivationWord, after.Battle.GetActor(Mage).ActivationWord);
        Assert.NotEqual(before.ThinkingSeed, after.Battle.ThinkingSeed);
    }

    [Fact]
    public void TemporaryMageUsesExistingSpellAndCategoryRngWithItsRealBook()
    {
        var before = Battle(d =>
        {
            d["actors"]![0]!["level"] = 18;
            d["spells"]!.AsArray().Add(new JsonObject { ["id"] = "desoul", ["level"] = 1, ["mpCost"] = 8,
                ["minimumRange"] = 1, ["maximumRange"] = 2,
                ["effect"] = new JsonObject { ["kind"] = "instant-death", ["radius"] = 0 } });
            d["actors"]![0]!["spells"]!.AsArray().Insert(0, new JsonObject { ["id"] = "desoul", ["level"] = 1 });
            d["terrains"]![0]!["rows"]![2] = "pppppppppp";
        });
        before = Status(before, Mage, 0x10);
        var (after, events) = Play(before, Mage, mainWord: 1700);
        Assert.Equal(Smoke, Assert.Single(events, e => e.Kind == "ai-spell-target").Target);
        var category = Assert.Single(events, e => e.Kind == "rng-ai-attack-category");
        Assert.Equal((ushort?)2, category.RandomValue);
        Assert.True(events.FindIndex(e => e.Kind == "thinking-rng") < events.IndexOf(category));
        Assert.Single(events, e => e.Kind == "rng-spell-critical");
        Assert.DoesNotContain(events, e => e.Kind == "rng-desoul");
        Assert.Equal(18, after.Battle.GetActor(Mage).Mp);
        Assert.InRange(after.Battle.GetActor(Smoke).Hp, 1, before.GetActor(Smoke).Hp - 1);
        Assert.Equal(before.GetActor(Mage).LastTarget, after.Battle.GetActor(Mage).LastTarget);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AHealChoiceUsesFriendlyTargetsAndSilenceStillPays(bool silenced)
    {
        var before = Battle(d => d["encounters"]![0]!["placements"]![2]!["x"] = 8);
        before = Status(before.With(actors: before.Actors.Select(a => a.Actor == Mage ? a.With(hp: 6) : a)),
            Priest, (ushort)(silenced ? 0x310 : 0x10));
        var (after, events) = Play(before, Priest, mainWord: 3000);
        Assert.Equal(Mage, Assert.Single(events, e => e.Kind == "ai-heal-target").Target);
        Assert.Equal(17, after.Battle.GetActor(Priest).Mp);
        if (silenced)
        {
            Assert.Contains(events, e => e.Kind == "silenced-cast" && e.Actor == Priest);
            Assert.Single(events, e => e.Kind == "mp" && e.Actor == Priest && e.After == 17);
            Assert.Equal(6, after.Battle.GetActor(Mage).Hp);
        }
        else Assert.True(after.Battle.GetActor(Mage).Hp > 6);
        Assert.Equal(before.ThinkingSeed, after.Battle.ThinkingSeed);
    }

    [Theory]
    [InlineData(3, 3, 50)]
    [InlineData(14, 4, 60)]
    public void PackedHealFourSelectionCompletesAtLowMpWhileManualChoiceRefuses(int mp, byte rank, int hpAfter)
    {
        var before = Battle(d =>
        {
            d["actors"]![0]!["maxHp"] = 60;
            d["actors"]![1]!["level"] = 40;
            d["start"]!["actors"]![0]!["hp"] = 20;
            d["start"]!["actors"]![1]!["mp"] = mp;
            d["encounters"]![0]!["placements"]![2]!["x"] = 8;
            foreach (var (level, cost, power) in new[] { (2, 5, 15), (3, 10, 30), (4, 20, 255) })
            {
                d["spells"]!.AsArray().Add(new JsonObject { ["id"] = "heal", ["level"] = level, ["mpCost"] = cost,
                    ["minimumRange"] = 0, ["maximumRange"] = level == 2 ? 2 : 1,
                    ["effect"] = new JsonObject { ["kind"] = "heal", ["adjustedPower"] = power, ["fullRecovery"] = false } });
                d["actors"]![1]!["spells"]!.AsArray().Add(new JsonObject { ["id"] = "heal", ["level"] = level });
            }
        });
        var rules = RuleCompositions.Sf2();
        var manual = before.With(queue: [new(Priest, 30), new(Mage, 24), new(null, 255)], cursor: 0);
        var ready = BattleAdvancer.Advance(new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(manual, null),
            new StoryState(), SessionStopReason.SimulationWait), [], rules);
        Assert.Equal(Priest, ready.Snapshot.Selection!.Actor);
        var commands = BattleCommandDispatcher.Submit(ready.Snapshot, new Confirm(), rules);
        Assert.Null(commands.Failure);
        var refused = BattleCommandDispatcher.Submit(commands.Snapshot, new SelectSpell(new("heal", rank)), rules);
        Assert.Equal("insufficient-mp", refused.Failure!.Code);
        Assert.Equal(mp, refused.Snapshot.Battle.GetActor(Priest).Mp);

        var (after, events) = Play(Status(before, Priest, 0x10), Priest, mainWord: 3000);
        Assert.Equal(Mage, Assert.Single(events, e => e.Kind == "ai-heal-target").Target);
        var payment = Assert.Single(events, e => e.Kind == "mp" && e.Actor == Priest);
        Assert.Equal(mp, payment.Before);
        Assert.Equal(0, payment.After);
        Assert.Equal(0, after.Battle.GetActor(Priest).Mp);
        Assert.Equal(hpAfter, after.Battle.GetActor(Mage).Hp);
    }

    [Fact]
    public void TemporaryAuraUsesBothEligibleAlliesAndPaysOnce()
    {
        var before = Battle(d =>
        {
            d["encounters"]![0]!["placements"]![2]!["x"] = 8;
            d["start"]!["actors"]![0]!["hp"] = 6;
            d["start"]!["actors"]![1]!["hp"] = 9;
            d["spells"]!.AsArray().Add(new JsonObject { ["id"] = "aura", ["level"] = 1, ["mpCost"] = 7,
                ["minimumRange"] = 0, ["maximumRange"] = 3,
                ["effect"] = new JsonObject { ["kind"] = "aura", ["adjustedPower"] = 15, ["fullRecovery"] = false, ["radius"] = 1 } });
            d["actors"]![1]!["spells"] = new JsonArray(new JsonObject { ["id"] = "aura", ["level"] = 1 });
        });
        var (after, events) = Play(Status(before, Priest, 0x10), Priest, mainWord: 3000);
        Assert.Equal(13, Assert.Single(events, e => e.Kind == "mp" && e.Actor == Priest).After);
        Assert.Equal(21, after.Battle.GetActor(Mage).Hp);
        Assert.Equal(24, after.Battle.GetActor(Priest).Hp);
        Assert.Equal(before.GetActor(Smoke).Hp, after.Battle.GetActor(Smoke).Hp);
    }

    [Fact]
    public void NoReachableAttackOrHealUsesTheNormalMoveWithoutDecisionRng()
    {
        var before = Battle(d =>
        {
            d["actors"]![0]!["spells"] = new JsonArray();
            d["terrains"]![0]!["rows"]![2] = "pppppppppp";
            d["encounters"]![0]!["placements"]![2]!["x"] = 8;
        });
        var (after, events) = Play(Status(before, Mage, 0x10), Mage, mainWord: 3000);
        Assert.Equal(Smoke, Assert.Single(events, e => e.Kind == "ai-move-target").Target);
        Assert.True(after.Battle.GetActor(Mage).Position!.X > before.GetActor(Mage).Position!.X);
        Assert.DoesNotContain(events, e => e.Kind is "thinking-rng" or "rng-ai-attack-category");
        Assert.Equal(before.ThinkingSeed, after.Battle.ThinkingSeed);
    }

    private static (SessionSnapshot Snapshot, List<SessionObservation> Observations) Play(
        EngineBattleState battle, ActorRef actor, uint mainWord)
    {
        var before = battle.With(mainSeed: mainWord << 16, queue: [new(actor, 30), new(actor, 24), new(null, 255)], cursor: 0);
        var rules = RuleCompositions.Sf2();
        var start = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveBattle(before, null), new StoryState(), SessionStopReason.SimulationWait);
        var result = BattleAdvancer.Advance(start, [], rules);
        List<SessionObservation> observations = [];
        for (int i = 0; ; i++)
        {
            Assert.True(i < 1000, "temporary entry settles");
            Assert.Null(result.Failure);
            observations.AddRange(result.Observations);
            var current = result.Snapshot;
            Assert.Equal(BattleControl.Player, current.Battle.GetActor(actor).Control);
            if (current.Selection is not null)
            {
                Assert.Equal(actor, current.Selection.Actor);
                Assert.Equal(BattleSelectionStage.Movement, current.Selection.Stage);
                Assert.Equal(1, current.Battle.Cursor);
                Assert.Equal(actor, Assert.Single(observations, e => e.Kind == "player-control").Actor);
                return (current, observations);
            }
            result = current.BattleMovement is { } move
                ? BattleMovementContinuation.Submit(current, new CompletePresentation(move.Token, move.CompletionKind), rules)
                : current.BattleScene is { } scene
                    ? BattleSceneContinuation.Submit(current, scene.Healing is { LogicalComplete: false, AtTimedInput: false }
                        ? new AdvanceSimulation(scene.Token) : scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
                            new CompletePresentation(scene.Token, scene.CompletionKind), rules)
                    : BattleAdvancer.Advance(current, [], rules);
        }
    }
}
