using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class SupportStatusCoreTests
{
    private static readonly ActorRef Hero = new("ally-0"), Enemy = new("enemy");

    [Theory]
    [InlineData(0x1000, 45, 27)]
    [InlineData(0x2000, 50, 30)]
    public void ColdContinuePreservesRecastUntilNextActualRefresh(ushort applied, int defense, int agility)
    {
        var (definition, entry) = BattleSaveFixture.Create();
        var hero = entry.Battle.GetActor(Hero);
        var progress = hero.Progress! with { Defense = 40, Agility = 24 };
        var pending = hero.With(progress: progress, status: applied, refreshStats: true)
            .With(status: 0x3000).With(facing: 1);
        var battle = entry.Battle.With(actors: entry.Battle.Actors.Select(a => a.Actor == Hero ? pending : a));
        var live = BattleSaveFixture.At(battle, entry.Story);
        var data = BattleSaveFixture.Capture(definition, live);
        var bytes = OpeningSaveJson.WriteRecord(new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, data));
        var record = OpeningSaveJson.ReadRecord(bytes, 1);
        Assert.Equal(5, record.FormatVersion);
        var cold = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, record, RuleCompositions.Sf2())).Session.Current;
        Assert.Equal(battle.Queue, cold.Battle.Queue);
        Assert.Equal(battle.Cursor, cold.Battle.Cursor);
        Assert.Equal(battle.MainSeed, cold.Battle.MainSeed);

        foreach (var start in new[] { live, cold })
        {
            var actor = start.Battle.GetActor(Hero);
            Assert.Equal(applied, actor.LastRefreshBits);
            Assert.Equal(0x3000, actor.Status);
            Assert.Equal(defense, actor.Defense);
            Assert.Equal(agility, actor.Agility);
            BattleTurnFlow.GenerateQueue(start.Battle, out var next);
            Assert.Equal(agility, next.Candidates.Single(a => a.Actor == Hero).Agility);
            var following = BattleSaveFixture.Stay(start);
            var ticked = following.Battle.GetActor(Hero);
            Assert.Equal(0x2000, ticked.Status);
            Assert.Null(ticked.LastRefreshBits);
            Assert.Equal(50, ticked.Defense);
            Assert.Equal(30, ticked.Agility);
        }
        BattleSaveFixture.EqualBattle(BattleSaveFixture.Stay(live).Battle, BattleSaveFixture.Stay(cold).Battle);
    }

    [Fact]
    public void ColdPhysicalHitAndAiEstimateUseOldDefenseBeforeTargetReaction()
    {
        var (definition, entry) = BattleSaveFixture.Create();
        var foe = entry.Battle.GetActor(Enemy).With(position: new(5, 5), status: 0x1000, refreshStats: true).With(status: 0x3000);
        var battle = entry.Battle.With(actors: entry.Battle.Actors.Select(a => a.Actor == Enemy ? foe : a));
        var live = BattleSaveFixture.At(battle, entry.Story);
        var data = BattleSaveFixture.Capture(definition, live);
        var bytes = OpeningSaveJson.WriteRecord(new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, data));
        var record = OpeningSaveJson.ReadRecord(bytes, 1);
        var cold = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, record, RuleCompositions.Sf2())).Session.Current;
        int Hit(EngineBattleState current)
        {
            var rule = new Sf2PhysicalAction();
            Assert.Equal(10, rule.EstimateDamage(current, Hero, Enemy));
            var attack = BattleActionRules.Prepare(rule, current, Hero, current.GetActor(Hero).Position!,
                new(BattleActionKind.Physical), Enemy, new Sf2BattleProgressionRule());
            var hit = attack.Reactions.First();
            Assert.Equal(BattleReactionKind.Damage, hit.Kind);
            Assert.Equal(4, attack.Prepared.GetActor(Enemy).Defense);
            var after = attack.ApplyReaction(attack.Prepared, hit);
            Assert.Equal(hit.HpAfter, after.GetActor(Enemy).Hp);
            Assert.Equal(5, after.GetActor(Enemy).Defense);
            Assert.Equal(current.Queue, after.Queue);
            Assert.Equal(9, rule.EstimateDamage(after, Hero, Enemy));
            var refreshed = current.With(actors: current.Actors.Select(a => a.Actor == Enemy ? a.With(refreshStats: true) : a));
            var later = BattleActionRules.Prepare(rule, refreshed, Hero, refreshed.GetActor(Hero).Position!,
                new(BattleActionKind.Physical), Enemy, new Sf2BattleProgressionRule());
            Assert.True(hit.Critical);
            Assert.Equal(15, hit.Amount);                   // 10 + floor(10 / 2)
            Assert.Equal(13, later.Reactions.First().Amount); // 9 + floor(9 / 2), same rolls
            return hit.Amount;
        }
        Assert.Equal(Hit(live.Battle), Hit(cold.Battle));
    }

    [Fact]
    public void OwnerExpiresSlowThenBoostBeforeOneRefresh()
    {
        var (_, entry) = BattleSaveFixture.Create();
        var hero = entry.Battle.GetActor(Hero);
        var both = hero.With(progress: hero.Progress! with { Defense = 195, Agility = 40 },
            status: 0x1400, refreshStats: true);
        // Two player entries keep the next automatic action outside this observation.
        var battle = entry.Battle.With(queue: [new(Hero, 25), new(Hero, 20), new(null, 255)], cursor: 0,
            actors: entry.Battle.Actors.Select(a => a.Actor == Hero ? both :
            a.Actor == Enemy ? a.With(status: 0x2000, refreshStats: true) : a));
        var changes = new List<SessionObservation>();
        var slow = BattleAfterTurnContinuation.Begin(BattleSaveFixture.At(battle, entry.Story), Hero, changes);
        Assert.Equal(BattleScenePhase.SlowExpiredMessage, slow.BattleScene!.Phase);
        Assert.Equal(0x1400, slow.Battle.GetActor(Hero).Status);
        Assert.Equal(176, slow.Battle.GetActor(Hero).Defense); // 195 + 24 capped at 200, then -24
        var rules = RuleCompositions.Sf2();
        var boost = BattleAfterTurnContinuation.Complete(slow,
            new CompletePresentation(slow.BattleScene.Token, slow.BattleScene.CompletionKind), rules).Snapshot;
        Assert.Equal(BattleScenePhase.BoostExpiredMessage, boost.BattleScene!.Phase);
        Assert.Equal(0x1000, boost.Battle.GetActor(Hero).Status);
        Assert.Equal(176, boost.Battle.GetActor(Hero).Defense);
        var done = BattleAfterTurnContinuation.Complete(boost,
            new CompletePresentation(boost.BattleScene.Token, boost.BattleScene.CompletionKind), rules).Snapshot;
        Assert.Equal(0, done.Battle.GetActor(Hero).Status);
        Assert.Equal(195, done.Battle.GetActor(Hero).Defense);
        Assert.Equal(0x2000, done.Battle.GetActor(Enemy).Status);
        Assert.Equal(battle.MainSeed, done.Battle.MainSeed);
    }

    [Theory]
    [InlineData(0)]    // Source-orders standby
    [InlineData(0x40)] // Skipped sleeping entry
    [InlineData(1)]    // Skipped stunned entry
    public void AutomaticEntryTicksOnlyAtItsOwnTurn(ushort otherStatus)
    {
        var (_, entry) = BattleSaveFixture.Create();
        var battle = entry.Battle.With(actors: entry.Battle.Actors.Select(a => a.Actor == Enemy
            ? a.With(status: (ushort)(0x0C00 | otherStatus), refreshStats: true) : a));
        var next = BattleSaveFixture.Stay(BattleSaveFixture.At(battle, entry.Story));
        Assert.Equal(3, next.Battle.Cursor);
        Assert.Equal(0x0800, next.Battle.GetActor(Enemy).Status & 0x0C00);
        Assert.Null(next.Battle.GetActor(Enemy).LastRefreshBits);
        Assert.Equal(0, next.Battle.GetActor(Hero).Status);
    }

    [Fact]
    public void EquipmentPreviewKeepsRecastAndChangingWeaponRefreshesIt()
    {
        var actor = EngineTestContent.Start("battle-equipment").Current.Battle.GetActor(new("champion"))
            .With(status: 0x1000, refreshStats: true).With(status: 0x3000);
        var option = BattleEquipment.Choices(actor).Single(o => o.Slot == 1);
        Assert.Equal((ushort)0x1000, actor.LastRefreshBits);
        var equipped = BattleEquipment.Equip(actor, option.Slot);
        Assert.Null(equipped.LastRefreshBits);
        Assert.Equal(0x3000, equipped.Status);
        Assert.Equal(option.Attack, equipped.Attack);
        Assert.Equal(actor.BaseDefense + actor.BaseDefense * 3 / 8, equipped.Defense);
        Assert.Null(BattleEquipment.RemoveItem(actor, option.Slot).LastRefreshBits);
    }

    [Fact]
    public void ZeroHpReactionRetainsDurationUntilDeadListCleanup()
    {
        var (_, entry) = BattleSaveFixture.Create();
        var before = entry.Battle.GetActor(Enemy).With(status: 0x2800, refreshStats: true);
        var fallen = before.With(hp: 0, refreshStats: true);
        var battle = entry.Battle.With(actors: entry.Battle.Actors.Select(a => a.Actor == Enemy ? fallen : a));
        var cleaned = BattleDeathBatch.Empty.Append(before, fallen).Clean(battle, Hero, new Sf2BattleProgressionRule());
        Assert.Equal(0x2800, fallen.Status);
        Assert.NotNull(fallen.Position);
        var dead = cleaned.Battle.GetActor(Enemy);
        Assert.Equal(0, dead.Status);
        Assert.Null(dead.Position);
        Assert.Null(dead.LastRefreshBits);
        Assert.Equal(before.BaseDefense, dead.Defense);
    }

    [Fact]
    public void ScriptRemovedSlotPreservesItsUnrefreshedWordThroughContinue()
    {
        var (definition, entry) = BattleSaveFixture.Create();
        ActorRef slot = new("hidden");
        var removed = entry.Battle.GetActor(slot)
            .With(initialized: true, attack: 5, hp: 10, position: new(9, 9), status: 0x1000, refreshStats: true)
            .With(status: 0x3000).With(hp: 0, clearPosition: true);
        var battle = entry.Battle.With(actors: entry.Battle.Actors.Select(a => a.Actor == slot ? removed : a));
        var data = BattleSaveFixture.Capture(definition, BattleSaveFixture.At(battle, entry.Story));
        var bytes = OpeningSaveJson.WriteRecord(new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, data));
        var record = OpeningSaveJson.ReadRecord(bytes, 1);
        var cold = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, record, RuleCompositions.Sf2()));
        var restored = cold.Session.Current.Battle.GetActor(slot);
        Assert.Null(restored.Position);
        Assert.Equal(0, restored.Hp);
        Assert.Equal(0x3000, restored.Status);
        Assert.Equal((ushort)0x1000, restored.LastRefreshBits);
    }

    [Theory]
    [InlineData(0x3000, 0)]      // Construction still pending
    [InlineData(0x3000, 0x3000)] // Redundant
    [InlineData(0x1000, 0x2000)] // Not a recast to three
    [InlineData(0x3000, 1)]      // Non-counter bits
    public void BattleSaveRejectsImpossibleRefreshOperands(ushort status, ushort operand)
    {
        var (definition, entry) = BattleSaveFixture.Create();
        var data = BattleSaveFixture.Capture(definition, entry);
        var actors = data.Battle.Actors.Select(a => a.Actor == Hero.Value
            ? a with { Status = status, LastRefreshCounters = operand } : a).ToArray();
        var invalid = data with { Battle = data.Battle with { Actors = actors } };
        Assert.IsType<SessionStartFailed>(GameSession.RestoreBattle(definition, invalid, RuleCompositions.Sf2()));
        var error = Assert.Throws<SaveFormatException>(() =>
            OpeningSaveJson.WriteRecord(new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, invalid)));
        Assert.Equal("save-battle-stat-counters", error.Code);
    }

    [Theory]
    [InlineData(false, 0x1000, 2)]
    [InlineData(true, 0x0400, 4)]
    public void RetainedFieldMemberCannotCarryBattleCountersIntoSaveOrContinue(bool classes, ushort counters, int version)
    {
        var (definition, entry) = BattleSaveFixture.Create();
        var rules = RuleCompositions.Sf2();
        var data = BattleSaveFixture.Capture(definition, entry) with { HasClassTerms = classes };
        var record = new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, data);
        var wire = OpeningSaveJson.ReadRecord(OpeningSaveJson.WriteRecord(record), 1);
        Assert.Equal(version, wire.FormatVersion);
        Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, wire, rules));

        var player = data.Story.Player;
        var retained = player.RetainedAllies.Select(a => a with { Status = (ushort)(a.Status | counters) }).ToArray();
        var invalid = data with { Story = data.Story with { Player = player with { RetainedAllies = retained } } };
        var result = GameSession.RestoreBattle(definition, invalid, rules);
        Assert.Equal("save-battle-retained-status", Assert.IsType<SessionStartFailed>(result).Failure.Code);
        var invalidRecord = new BattleSaveRecord(1, 1, DateTimeOffset.UnixEpoch, invalid);
        Assert.Equal(version, invalidRecord.FormatVersion);
        Assert.Equal("save-battle-retained-status", Assert.Throws<SaveFormatException>(() =>
            OpeningSaveJson.WriteRecord(invalidRecord)).Code);

        var story = entry.Story.Copy(null, fieldPlayer: entry.Story.FieldPlayer! with { RetainedAllies = retained });
        var live = BattleSaveFixture.At(entry.Battle, story);
        var captured = BattleSave.Capture(definition, live, rules, live.SessionId, live.Revision);
        Assert.Null(captured.Data);
        Assert.Equal("save-battle-retained-status", captured.Error);
    }

    [Fact]
    public void DirectHighDefenseFeedsInspectionEstimateAndStrikeUntilStayRefreshesIt()
    {
        var session = EngineTestContent.Start("battle-equipment", doc =>
        {
            doc["actors"]![0]!["defense"] = 210;
            doc["actors"]![1]!["attack"] = 220;
        });
        var target = session.Current.Selection!.Actor;
        ActorRef attacker = new("guardian");
        var physical = new Sf2PhysicalAction();
        int Hit(int defense)
        {
            var battle = session.Current.Battle.With(mainSeed: 0x1234ABCD);
            Assert.False(battle.GetActor(target).ExtraRoundAction);
            Assert.Equal(defense, session.QueryBattleInspection(unit: target).Unit!.Defense);
            Assert.Equal(220 - defense, physical.EstimateDamage(battle, attacker, target));
            var action = BattleActionRules.Prepare(physical, battle, attacker, battle.GetActor(attacker).Position!,
                new(BattleActionKind.Physical), target, new Sf2BattleProgressionRule());
            var hit = action.Reactions.First();
            Assert.Equal(BattleReactionKind.Damage, hit.Kind);
            Assert.True(hit.Critical);
            Assert.Equal(hit.Amount, battle.GetActor(target).Hp - action.ApplyReaction(action.Prepared, hit).GetActor(target).Hp);
            return hit.Amount;
        }
        Assert.Equal(15, Hit(210));
        EngineTestContent.Stay(session);
        Assert.Equal(210, session.Current.Battle.GetActor(target).BaseDefense);
        Assert.InRange(Hit(200), 24, 30);
    }

    [Fact]
    public void DirectHighAgilitySurvivesQueueInspectionUntilItsFirstRealRefresh()
    {
        var session = EngineTestContent.Start(change: doc =>
        {
            doc["actors"]![0]!["agility"] = 82;
            doc["actors"]![0]!["extraRoundAction"] = true;
        });
        var actor = session.Current.Selection!.Actor;
        var initial = session.Current.Battle;
        Assert.Equal(82, initial.GetActor(actor).Agility); // direct current byte 210
        BattleTurnFlow.GenerateQueue(initial, out var preview);
        Assert.Equal(82, preview.Candidates.Single(c => c.Actor == actor).Agility);
        var next = EngineTestContent.Stay(session).Snapshot.Battle;
        Assert.Equal(initial.Queue, next.Queue);
        Assert.Equal(72, next.GetActor(actor).Agility); // zero SLOW still clamps 210 to 200
        Assert.True(next.GetActor(actor).ExtraRoundAction);
        BattleTurnFlow.GenerateQueue(next, out var generated);
        Assert.Equal(72, generated.Candidates.Single(c => c.Actor == actor).Agility);
        Assert.Equal(2, generated.Slots.Count(s => s.Actor == actor));
        var expiring = next.GetActor(actor).With(status: 0x0400, refreshStats: true);
        Assert.Equal(56, expiring.Agility); // (210 - floor(210 / 8)) & 127
        var held = next.With(queue: [new(actor, 25), new(actor, 20)], cursor: 0,
            actors: next.Actors.Select(a => a.Actor == actor ? expiring : a));
        var wait = BattleAfterTurnContinuation.Begin(BattleSaveFixture.At(held, session.Current.Story), actor, []);
        var scene = wait.BattleScene!;
        var expired = BattleAfterTurnContinuation.Complete(wait,
            new CompletePresentation(scene.Token, scene.CompletionKind), RuleCompositions.Sf2()).Snapshot;
        Assert.Equal(72, expired.Battle.GetActor(actor).Agility);
        Assert.True(expired.Battle.GetActor(actor).ExtraRoundAction);
    }

    [Fact]
    public void PackedAgilityChangesNextRoundEntryCountOnly()
    {
        var (_, entry) = BattleSaveFixture.Create();
        var foe = entry.Battle.GetActor(Enemy);
        var definition = new BattleActorDefinition(Enemy, foe.Definition.ClassRule, 1, 25, 0, 6, 41, 20, true, 3, []);
        var slowed = new BattleActorState(foe.Deployment with { Definition = definition }, 19, 0, null, foe.Position,
            null, null, status: 0x0c00);
        var battle = entry.Battle.With(actors: entry.Battle.Actors.Select(a => a.Actor == Enemy ? slowed : a));
        Assert.Equal(2, battle.Queue.Count(a => a.Actor == Enemy));
        Assert.Equal(26, slowed.Defense);
        Assert.Equal(93, slowed.Agility);
        Assert.False(slowed.ExtraRoundAction);
        var next = BattleTurnFlow.GenerateQueue(battle, out _);
        Assert.Single(next.Queue, a => a.Actor == Enemy);
    }
}
