using System.Text.Json;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Application.Tests.PrivateOriginalBattle01FirstControlTests;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01PlayerPhysicalAttackTests
{
    [Fact]
    public void ManualLifecycleReplacesOnlyTheExactSnapshotAndPreservesPreparation()
    {
        var session = ReadySession(); var ready = session.PrivateOriginalBattle01!;
        var provisional = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(ready, 0)).Snapshot;
        var selected = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.BeginPrivateOriginalBattle01PlayerAttack(provisional, 0)).Snapshot;
        string frozen = JsonSerializer.Serialize(selected);
        var completed = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.ConfirmPrivateOriginalBattle01PlayerAttack(selected, 0)).Snapshot;
        Assert.Same(completed, session.PrivateOriginalBattle01); Assert.NotSame(selected, completed);
        Assert.Equal((9, 2, (byte?)15), ((int)completed.Battle.Roster[0].Stats.HpCurrent, (int)completed.Battle.Roster[7].Stats.HpCurrent, completed.Battle.Roster[0].Stats.CurrentExp));
        Assert.Same(ready.Preparation, completed.Preparation); Assert.Equal((byte?)0, completed.Preparation.Party.Allies[0].CurrentExp);
        Assert.Same(ready.SourceLocomotion, completed.SourceLocomotion); Assert.Same(ready.SourceBridge, completed.SourceBridge);
        Assert.Same(ready.SourceSnapshot, completed.SourceSnapshot); Assert.Equal(frozen, JsonSerializer.Serialize(selected));
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01PlayerAttackRejected>(session.ConfirmPrivateOriginalBattle01PlayerAttack(selected, 0)).Diagnostic.Field);
        Assert.Equal("phase", Assert.IsType<PrivateOriginalBattle01PlayerAttackRejected>(session.ConfirmPrivateOriginalBattle01PlayerAttack(completed, 0)).Diagnostic.Field);
        Assert.Same(completed, session.PrivateOriginalBattle01);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("equivalent")]
    [InlineData("foreign")]
    [InlineData("wrongActor")]
    [InlineData("wrongPhase")]
    [InlineData("oldPreset")]
    [InlineData("occupancy")]
    [InlineData("terrain")]
    [InlineData("selection")]
    public void RejectedRequestsKeepAllCurrentSessionChannels(string mutation)
    {
        var session = ReadySession(); var current = session.PrivateOriginalBattle01!;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 0)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.BeginPrivateOriginalBattle01PlayerAttack(current, 0)).Snapshot;
        var request = current;
        if (mutation == "null") request = null;
        if (mutation == "equivalent") request = new(current.Preparation, current.Battle, current.SourceLocomotion, current.SourceBridge);
        if (mutation == "foreign") request = ReadySession().PrivateOriginalBattle01;
        if (mutation == "oldPreset")
        {
            var preparation = new PrivateOriginalBattle01StartupPrepared(current.Preparation.Pending, current.Preparation.Inputs,
                OriginalBattle01ControlledPartyPreset.PlayerReadyComparison);
            request = current = new(preparation, current.Battle, current.SourceLocomotion, current.SourceBridge);
            Set(session, current);
        }
        if (mutation is "occupancy" or "terrain" or "selection")
        {
            // Public collections are immutable; reflection authors drift to exercise confirmation admission.
            var b = current.Battle; var occupancy = b.Occupancy.ToArray(); var terrain = b.Terrain.ToArray();
            if (mutation == "occupancy") occupancy[11 + 14 * 48] = -1;
            if (mutation == "terrain") terrain[11 + 14 * 48] = 255;
            var initial = Internal<Battle01InitializedState>(b.Roster.ToArray(), b.Regions.ToArray(), terrain, occupancy, b.RandomSeedImage, b.RandomSeedCopy);
            var withAi = Internal<Battle01InitializedState>(initial, b.Roster.ToArray(), occupancy, b.AiMemory.ToArray(), b.RandomSeedCopy!.Value, b.RandomSeedImage, b.AiLastTargets.ToArray());
            var withRound = Internal<Battle01InitializedState>(withAi, b.Roster.ToArray(), b.RegionFlags90Through105.ToArray(), b.NewlyTestedRegionMask, b.RandomSeedImage, b.FirstRound);
            var history = Internal<Battle01InitializedState>(withRound, b.FirstRound, b.TurnCompletion);
            var control = b.FirstControl!;
            if (mutation == "selection")
            {
                var movement = Internal<Battle01PlayerMovementSelection>(control.Movement.Range, control.Movement.Preview,
                    Battle01PlayerMovementStage.TargetSelection, Internal<Battle01PlayerAttackSelection>(new[] { 131 }, 0));
                control = (Battle01FirstControlState)typeof(Battle01FirstControlState).GetMethod("WithMovement", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(control, [movement])!;
            }
            var battle = Internal<Battle01InitializedState>(history, b.Roster.ToArray(), Array.AsReadOnly(occupancy), control);
            request = current = new(current.Preparation, battle, current.SourceLocomotion, current.SourceBridge); Set(session, current);
        }
        string frozen = JsonSerializer.Serialize(current);
        var result = mutation == "wrongPhase" ? session.BeginPrivateOriginalBattle01PlayerAttack(current, 0) :
            session.ConfirmPrivateOriginalBattle01PlayerAttack(request, mutation == "wrongActor" ? 1 : 0);
        Assert.IsType<PrivateOriginalBattle01PlayerAttackRejected>(result);
        Assert.Same(current, session.PrivateOriginalBattle01); Assert.Equal(frozen, JsonSerializer.Serialize(current));
    }

    [Fact]
    public void NonoriginTargetCancelThenMovementCancelRetainsHpExpAndRandomState()
    {
        var session = ReadySession(); var ready = session.PrivateOriginalBattle01!;
        var current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(ready, 0, new(12, 14))).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 0)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.BeginPrivateOriginalBattle01PlayerAttack(current, 0)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.CyclePrivateOriginalBattle01PlayerAttackTarget(current, 0, -1)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.CancelPrivateOriginalBattle01PlayerAttackTarget(current, 0)).Snapshot;
        Assert.Equal(new MapPosition(12, 14), current.Battle.Roster[0].Position); Assert.Equal(Battle01Phase.PlayerActionChoice, current.Battle.Phase);
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.CancelPrivateOriginalBattle01PlayerMovement(current, 0)).Snapshot;
        Assert.Equal(new MapPosition(11, 15), current.Battle.Roster[0].Position); Assert.Same(ready.Battle.TurnCompletion, current.Battle.TurnCompletion);
        Assert.Equal(ready.Battle.RandomSeedImage, current.Battle.RandomSeedImage); Assert.Equal(ready.Battle.RandomSeedCopy, current.Battle.RandomSeedCopy);
        Assert.Same(ready.Battle.Roster[0].Stats, current.Battle.Roster[0].Stats);
    }

    [Fact]
    public void FirstDefeatPublishesOneSnapshotAndNextPlayerCanMoveAndCancel()
    {
        var session = FirstDefeatSession(); var selected = session.PrivateOriginalBattle01!;
        string frozen = JsonSerializer.Serialize(selected);
        var after = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerAttack(selected, 0)).Snapshot;
        Assert.Same(after, session.PrivateOriginalBattle01); Assert.NotSame(selected, after);
        Assert.Equal(((uint?)60, (ushort?)1, (byte?)39), (after.Battle.CurrentGold,
            after.Battle.Roster[0].Stats.CurrentKills, after.Battle.Roster[0].Stats.CurrentExp));
        Assert.Null(after.Battle.Roster[7].Position); Assert.Equal(0, after.Battle.Roster[7].Stats.HpCurrent);
        Assert.Equal(-1, after.Battle.OccupantAt(new(11, 14)));
        Assert.Same(selected.Preparation, after.Preparation); Assert.Equal((uint?)0, after.Preparation.Party.CurrentGold);
        Assert.Equal((ushort?)0, after.Preparation.Party.Allies[0].CurrentKills);
        Assert.Same(selected.SourceSnapshot, after.SourceSnapshot); Assert.Same(selected.SourceLocomotion, after.SourceLocomotion);
        Assert.Same(selected.SourceBridge, after.SourceBridge); Assert.Equal(frozen, JsonSerializer.Serialize(selected));
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01PlayerAttackRejected>(
            session.ConfirmPrivateOriginalBattle01PlayerAttack(selected, 0)).Diagnostic.Field);
        Assert.Equal("phase", Assert.IsType<PrivateOriginalBattle01PlayerAttackRejected>(
            session.ConfirmPrivateOriginalBattle01PlayerAttack(after, 0)).Diagnostic.Field);
        Assert.Same(after, session.PrivateOriginalBattle01);
        var current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(after, 1)).Snapshot;
        Assert.Equal((1, 10), (current.Battle.FirstControl!.ActorIndex, current.Battle.FirstControl.Movement.Range.Budget));
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.SelectPrivateOriginalBattle01PlayerDestination(current, 1, new(10, 17))).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 1)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.CancelPrivateOriginalBattle01PlayerMovement(current, 1)).Snapshot;
        Assert.Equal(new MapPosition(9, 17), current.Battle.Roster[1].Position);
        Assert.Same(after.Battle.TurnCompletion, current.Battle.TurnCompletion);
        Assert.Equal((uint?)60, current.Battle.CurrentGold); Assert.Equal((ushort?)1, current.Battle.Roster[0].Stats.CurrentKills);
    }

    [Theory]
    [InlineData("foreign")]
    [InlineData("equivalent")]
    [InlineData("goldInput")]
    [InlineData("killsInput")]
    [InlineData("latePreparation")]
    public void FirstDefeatRejectionRetainsExactSessionIncludingAllAccountingChannels(string mutation)
    {
        var session = FirstDefeatSession(); var current = session.PrivateOriginalBattle01!;
        var request = current;
        if (mutation == "foreign") request = FirstDefeatSession().PrivateOriginalBattle01!;
        if (mutation == "equivalent") request = new(current.Preparation, current.Battle, current.SourceLocomotion, current.SourceBridge);
        if (mutation is "goldInput" or "killsInput")
        {
            var b = current.Battle; var roster = b.Roster.ToArray();
            if (mutation == "killsInput")
            {
                var actor = roster[0]; var s = actor.Stats;
                var stats = new Battle01Stats(s.Level, s.HpMax, s.HpCurrent, s.MpMax, s.MpCurrent, s.Attack, s.Defense, s.Agility,
                    s.Move, s.Status, s.Items, s.Spells, s.CurrentExp);
                roster[0] = Internal<Battle01Combatant>(actor.Deployment, stats, actor.ClassId, actor.EnemySource, actor.AiBitfield, actor.Position);
            }
            var battle = Internal<Battle01InitializedState>(b, roster, b.RandomSeedImage, mutation == "goldInput" ? null : b.CurrentGold);
            request = current = new(current.Preparation, battle, current.SourceLocomotion, current.SourceBridge); Set(session, current);
        }
        if (mutation == "latePreparation")
        {
            // Nonlethal confirmation can finish locally, then the named preparation rejects a forged known/unknown accounting origin.
            session = ReadySession(firstDefeat: true); current = session.PrivateOriginalBattle01!;
            current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
                session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 0)).Snapshot;
            current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(
                session.BeginPrivateOriginalBattle01PlayerAttack(current, 0)).Snapshot;
            var prepared = new PrivateOriginalBattle01StartupPrepared(current.Preparation.Pending, current.Preparation.Inputs,
                OriginalBattle01ControlledPartyPreset.PlayerAttackComparison);
            request = current = new(prepared, current.Battle, current.SourceLocomotion, current.SourceBridge); Set(session, current);
        }
        string frozen = JsonSerializer.Serialize(current);
        var rejected = Assert.IsType<PrivateOriginalBattle01PlayerAttackRejected>(
            session.ConfirmPrivateOriginalBattle01PlayerAttack(request, 0));
        if (mutation == "latePreparation") Assert.Equal("accounting.input", rejected.Diagnostic.Field);
        Assert.Same(current, session.PrivateOriginalBattle01); Assert.Equal(frozen, JsonSerializer.Serialize(current));
    }

    private static GameSession FirstDefeatSession()
    {
        var session = ReadySession(firstDefeat: true); var current = session.PrivateOriginalBattle01!;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 0)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(
            session.BeginPrivateOriginalBattle01PlayerAttack(current, 0)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerAttack(current, 0)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPursuit(current, 131)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(
            session.CompletePrivateOriginalBattle01EnemyStandby(current, 133)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(current)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(current, 2)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 2)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(current, 2)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPursuit(current, 131)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(current, 132)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01EnemyStandbyCompleted>(
            session.CompletePrivateOriginalBattle01EnemyStandby(current, 133)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(
            session.EnterPrivateOriginalBattle01NextPlayerControl(current, 0)).Snapshot;
        current = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(
            session.ConfirmPrivateOriginalBattle01PlayerMovement(current, 0)).Snapshot;
        Assert.IsType<PrivateOriginalBattle01PlayerAttackApplied>(session.BeginPrivateOriginalBattle01PlayerAttack(current, 0));
        return session;
    }

    internal static GameSession ReadySession(bool firstDefeat = false)
    {
        // Existing authored route supplies the pre-strike state; declare EXP0 before either physical receipt.
        var session = PrivateOriginalBattle01EnemyPhysicalAttackTests.AttackSession(); var before = session.PrivateOriginalBattle01!;
        var roster = before.Battle.Roster.ToArray(); var actor = roster[0]; var s = actor.Stats;
        var stats = new Battle01Stats(s.Level, s.HpMax, s.HpCurrent, s.MpMax, s.MpCurrent, s.Attack, s.Defense, s.Agility,
            s.Move, s.Status, s.Items, s.Spells, 0);
        roster[0] = Internal<Battle01Combatant>(actor.Deployment, stats, actor.ClassId, actor.EnemySource, actor.AiBitfield, actor.Position);
        var preset = firstDefeat ? OriginalBattle01ControlledPartyPreset.FirstDefeatComparison : OriginalBattle01ControlledPartyPreset.PlayerAttackComparison;
        if (firstDefeat)
            for (int index = 0; index < 3; index++)
            {
                var ally = preset.Allies[index]; var unit = roster[index];
                var supplied = new Battle01Stats(ally.Level, ally.HpMax, ally.HpCurrent, ally.MpMax, ally.MpCurrent,
                    ally.EffectiveAttack, ally.EffectiveDefense, ally.EffectiveAgility, ally.EffectiveMove, ally.StatusEffects,
                    ally.Items, ally.Spells, ally.CurrentExp, ally.CurrentKills);
                roster[index] = Internal<Battle01Combatant>(unit.Deployment, supplied, unit.ClassId, unit.EnemySource, unit.AiBitfield, unit.Position);
            }
        var battle = Internal<Battle01InitializedState>(before.Battle, roster, before.Battle.RandomSeedImage, preset.CurrentGold);
        var prepared = new PrivateOriginalBattle01StartupPrepared(before.Preparation.Pending, before.Preparation.Inputs,
            preset);
        before = new(prepared, battle, before.SourceLocomotion, before.SourceBridge); Set(session, before);
        var after = Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(before, 132)).Snapshot;
        Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(after, 0));
        return session;
    }
    private static void Set(GameSession session, PrivateOriginalBattle01SessionSnapshot snapshot) =>
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session, snapshot);
}
