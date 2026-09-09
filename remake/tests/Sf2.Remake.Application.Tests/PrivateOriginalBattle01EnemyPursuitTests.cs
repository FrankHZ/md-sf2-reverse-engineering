using System.Text.Json;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Application.Tests.PrivateOriginalBattle01FirstControlTests;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01EnemyPursuitTests
{
    [Fact]
    public void PursuitCommitsOncePreservesProvenanceAndContinuesThroughInactiveActorsToPlayerControl()
    {
        var session = FirstPursuitSession(); var before = session.PrivateOriginalBattle01!;
        string frozen = JsonSerializer.Serialize(before.Battle);
        var after = Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(session.CompletePrivateOriginalBattle01EnemyPursuit(before, 131)).Snapshot;
        Assert.Same(after, session.PrivateOriginalBattle01); Assert.NotSame(before, after);
        Assert.Same(before.Preparation, after.Preparation); Assert.Same(before.SourceSnapshot, after.SourceSnapshot);
        Assert.Same(before.SourceLocomotion, after.SourceLocomotion); Assert.Same(before.SourceBridge, after.SourceBridge);
        Assert.Same(before.Battle.TurnCompletion, after.Battle.TurnCompletion!.Previous);
        Assert.Null(after.Battle.TurnCompletion.EnemyStandby); Assert.Equal(new MapPosition(9, 4), after.Battle.TurnCompletion.EnemyPursuit!.Destination);
        Assert.Equal(before.Battle.RandomSeedImage, after.Battle.RandomSeedImage); Assert.Equal(before.Battle.RandomSeedCopy, after.Battle.RandomSeedCopy);
        Assert.Equal(before.Battle.AiMemory, after.Battle.AiMemory); Assert.Equal(frozen, JsonSerializer.Serialize(before.Battle));
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01EnemyPursuitRejected>(session.CompletePrivateOriginalBattle01EnemyPursuit(before, 131)).Diagnostic.Field);
        Assert.Equal("actor", Assert.IsType<PrivateOriginalBattle01EnemyPursuitRejected>(session.CompletePrivateOriginalBattle01EnemyPursuit(after, 131)).Diagnostic.Field);
        Assert.Same(after, session.PrivateOriginalBattle01);
        for (int i = 0; i < 2; i++) PrivateOriginalBattle01FirstRoundTests.CompleteOriginTurn(session);
        var current = session.PrivateOriginalBattle01!;
        Assert.Equal((byte)0, current.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex);
        var ready = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(current, 0)).Snapshot;
        Assert.Equal(0, ready.Battle.FirstControl!.ActorIndex); Assert.Same(current.Battle.TurnCompletion, ready.Battle.TurnCompletion);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("equivalent")]
    public void OnlyTheExactCurrentSnapshotCanRequestPursuit(string mutation)
    {
        var session = FirstPursuitSession(); var current = session.PrivateOriginalBattle01!;
        var request = mutation switch
        {
            "missing" => null,
            "foreign" => FirstPursuitSession().PrivateOriginalBattle01,
            _ => new PrivateOriginalBattle01SessionSnapshot(current.Preparation, current.Battle, current.SourceLocomotion, current.SourceBridge)
        };
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01EnemyPursuitRejected>(session.CompletePrivateOriginalBattle01EnemyPursuit(request, 131)).Diagnostic.Field);
        Assert.Same(current, session.PrivateOriginalBattle01);
    }

    [Fact]
    public void MissingBattleWrongActorAndUnsupportedCurrentInputsDoNotInstallAReplacement()
    {
        var pending = PrivateOriginalBattle01StartupTests.PendingSession(); var admission = pending.PrivateOriginalBattle01Admission;
        Assert.Equal("battle", Assert.IsType<PrivateOriginalBattle01EnemyPursuitRejected>(pending.CompletePrivateOriginalBattle01EnemyPursuit(null, 131)).Diagnostic.Field);
        Assert.Same(admission, pending.PrivateOriginalBattle01Admission);
        var session = FirstPursuitSession(); var original = session.PrivateOriginalBattle01!;
        Assert.Equal("actor", Assert.IsType<PrivateOriginalBattle01EnemyPursuitRejected>(session.CompletePrivateOriginalBattle01EnemyPursuit(original, 132)).Diagnostic.Field);
        var terrain = original.Battle.Terrain.ToArray(); terrain[0] = 16;
        var input = PrivateOriginalBattle01FirstRoundTests.CopyCurrent(session, terrain: terrain);
        string frozen = JsonSerializer.Serialize(input.Battle);
        Assert.Equal("terrain", Assert.IsType<PrivateOriginalBattle01EnemyPursuitRejected>(session.CompletePrivateOriginalBattle01EnemyPursuit(input, 131)).Diagnostic.Field);
        Assert.Same(input, session.PrivateOriginalBattle01); Assert.Equal(frozen, JsonSerializer.Serialize(input.Battle));
    }

    [Fact]
    public void AttackEligibilityIsATypedRejectionAndPreservesFirstEnemyTestedSeven()
    {
        var session = RoundThreeSession(); var original = session.PrivateOriginalBattle01!; var slots = original.Battle.FirstRound!.Slots.ToArray();
        (slots[0], slots[8]) = (slots[8], slots[0]); var order = Internal<Battle01FirstRoundOrder>(slots, Array.Empty<int>(), Array.Empty<int>(), 3);
        var roster = original.Battle.Roster.ToArray(); var enemy = roster[7];
        roster[7] = Internal<Battle01Combatant>(enemy.Deployment, enemy.Stats, enemy.ClassId, enemy.EnemySource, enemy.AiBitfield, new MapPosition(11, 10));
        var occupancy = Enumerable.Repeat(-1, 2304).ToArray(); foreach (var unit in roster) occupancy[unit.Position.Y * 48 + unit.Position.X] = unit.Index;
        var input = PrivateOriginalBattle01FirstRoundTests.CopyCurrent(session, roster: roster, occupancy: occupancy, order: order);
        string frozen = JsonSerializer.Serialize(input.Battle);
        var boundary = Assert.IsType<PrivateOriginalBattle01AttackSelectionRequired>(session.CompletePrivateOriginalBattle01EnemyPursuit(input, 132));
        Assert.Equal(132, boundary.ActorIndex); Assert.Equal("attack.targets", boundary.Diagnostic.Field);
        Assert.Equal(new[] { new Battle01AttackCandidate(0, new(11, 14), 8) }, boundary.Targets);
        Assert.Same(input, session.PrivateOriginalBattle01); Assert.Equal(7, input.Battle.NewlyTestedRegionMask);
        Assert.Equal(0, input.Battle.FirstRound!.CurrentTurnOffset); Assert.Equal(frozen, JsonSerializer.Serialize(input.Battle));
    }

    internal static GameSession FirstPursuitSession()
    {
        var session = RoundThreeSession(); for (int i = 0; i < 3; i++) PrivateOriginalBattle01FirstRoundTests.CompleteOriginTurn(session); return session;
    }

    internal static GameSession RoundThreeSession()
    {
        var session = PrivateOriginalBattle01FirstRoundTests.CompletedFirstRound();
        // Authored region admission, deliberately separate from the required selected-input Content chain.
        var regions = session.PrivateOriginalBattle01!.Battle.Regions.ToArray();
        regions[1] = new(1, 0, [new(10, 14), new(12, 14), new(12, 16), new(10, 16)], 0, 0);
        var initial = PrivateOriginalBattle01FirstRoundTests.CopyCurrent(session, regions: regions);
        Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(initial));
        while (session.PrivateOriginalBattle01!.Battle.FirstRound!.CurrentCandidate is { } candidate)
        {
            if (candidate.CombatantIndex != 0) { PrivateOriginalBattle01FirstRoundTests.CompleteOriginTurn(session); continue; }
            var ready = Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(session.PrivateOriginalBattle01, 0)).Snapshot;
            var selected = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.SelectPrivateOriginalBattle01PlayerDestination(ready, 0, new(11, 15))).Snapshot;
            var moved = Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(selected, 0)).Snapshot;
            Assert.IsType<PrivateOriginalBattle01StayCommitted>(session.CommitPrivateOriginalBattle01Stay(moved, 0));
        }
        Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(session.PrivateOriginalBattle01));
        return session;
    }
}
