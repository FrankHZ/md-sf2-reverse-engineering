using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01TurnCompletionTests
{
    private static Battle01StayCompletionPolicy Policy => Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats;

    [Fact]
    public void StayRetainsProvisionalPositionAndStatsChecksBothFactionsThenAdvancesOneByteEntry()
    {
        var ready = Battle01FirstControl.Enter(Battle01FirstControlTests.Round(), 1).State!;
        var origin = ready.Roster[1].Position; var destination = new MapPosition(origin.X, origin.Y - 1);
        var action = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 1, destination), 1);
        string before = JsonSerializer.Serialize(action);
        var completed = Battle01TurnCompletion.CommitStay(action, 1, Policy);
        Assert.Equal(Battle01Phase.PlayerTurnCompleted, completed.Phase); Assert.Null(completed.FirstControl);
        Assert.Equal(1, completed.TurnCompletion!.CompletedActorIndex); Assert.Same(Policy, completed.TurnCompletion.Policy);
        Assert.Equal(new Battle01FactionCounts(3, 6), completed.TurnCompletion.BeforeAfterTurn);
        Assert.Equal(completed.TurnCompletion.BeforeAfterTurn, completed.TurnCompletion.AfterAfterTurn);
        Assert.Equal(destination, completed.Roster[1].Position); Assert.Equal(origin, completed.Roster[1].Deployment.Position);
        Assert.Equal(-1, completed.OccupantAt(origin)); Assert.Equal(1, completed.OccupantAt(destination));
        Assert.Same(action.Roster, completed.Roster); Assert.Same(action.Occupancy, completed.Occupancy);
        Assert.Same(ready.Roster[1].Stats, completed.Roster[1].Stats);
        Assert.Same(action.FirstRound!.Slots, completed.FirstRound!.Slots); Assert.Equal(64, completed.FirstRound.Slots.Count);
        Assert.Equal(0, action.FirstRound.CurrentTurnOffset); Assert.Equal(2, completed.FirstRound.CurrentTurnOffset);
        Assert.Equal(action.FirstRound.Slots[1], completed.FirstRound.CurrentCandidate);
        Assert.Equal(action.FirstRound.FirstCandidate, completed.FirstRound.FirstCandidate);
        Assert.NotEqual(action.FirstRound.Slots[2], completed.FirstRound.CurrentCandidate);
        Assert.Equal(action.RandomSeedImage, completed.RandomSeedImage); Assert.Equal(0xA4991234u, completed.RandomSeedImage);
        Assert.Same(action.RegionFlags90Through105, completed.RegionFlags90Through105);
        Assert.Same(action.AiMemory, completed.AiMemory); Assert.Same(action.AiLastTargets, completed.AiLastTargets);
        Assert.Equal(before, JsonSerializer.Serialize(action));
        Assert.Equal(JsonSerializer.Serialize(completed), JsonSerializer.Serialize(Battle01TurnCompletion.CommitStay(action, 1, Policy)));
        foreach (Action request in new Action[] {
            () => Battle01TurnCompletion.CommitStay(completed, 1, Policy),
            () => Battle01PlayerMovement.Cancel(completed, 1),
            () => Battle01PlayerMovement.Confirm(completed, 1),
            () => Battle01PlayerMovement.SelectDestination(completed, 1, origin),
            () => Battle01FirstControl.Enter(completed, completed.FirstRound.CurrentCandidate!.Value.CombatantIndex) })
            Assert.Equal("phase", Assert.Throws<ArgumentException>(request).ParamName);
        Assert.Equal("current", Assert.Throws<ArgumentException>(() => Battle01FirstRound.Enter(completed)).ParamName);
    }

    [Fact]
    public void StayingAtOriginIsValidAndSentinelAtTheNextByteEntryIsNeverSkipped()
    {
        var round = Battle01FirstControlTests.Round(); var slots = round.FirstRound!.Slots.ToArray();
        slots[1] = new(255, 255); slots[2] = new(128, 90);
        round = new(round, round.Roster.ToArray(), round.RegionFlags90Through105.ToArray(), round.NewlyTestedRegionMask,
            round.RandomSeedImage, new(slots, [], []));
        var ready = Battle01FirstControl.Enter(round, 1).State!;
        var action = Battle01PlayerMovement.Confirm(ready, 1);
        var completed = Battle01TurnCompletion.CommitStay(action, 1, Policy);
        Assert.Equal(ready.Roster.Select(unit => unit.Position), completed.Roster.Select(unit => unit.Position));
        Assert.Equal(ready.Occupancy, completed.Occupancy); Assert.Null(completed.FirstRound!.CurrentCandidate);
        Assert.Equal(2, completed.FirstRound.CurrentTurnOffset); Assert.Equal(slots, completed.FirstRound.Slots);
        Assert.Equal(round.FirstRound!.FirstCandidate, completed.FirstRound.FirstCandidate);
    }

    [Theory]
    [InlineData(2, 127, "status")]
    [InlineData(0, 0xE1, "equipment")]
    [InlineData(0, 0xE4, "equipment")]
    [InlineData(0, 0xFC, "equipment")]
    [InlineData(0, 127, "stats")]
    public void UnconsumedStatusAndEquippedRecoveryRejectWithoutNormalizingAnything(ushort status, ushort item, string field)
    {
        var action = ActionChoice(); var source = action.Roster[1].Stats;
        var stats = new Battle01Stats(source.Level, source.HpMax, source.HpCurrent, source.MpMax, source.MpCurrent,
            source.Attack, source.Defense, source.Agility, source.Move, status, [item, 127, 127, 127], source.Spells);
        var roster = action.Roster.ToArray(); var actor = roster[1];
        roster[1] = new(actor.Deployment, stats, actor.ClassId, actor.EnemySource, actor.AiBitfield, actor.Position);
        var altered = new Battle01InitializedState(action, roster, action.Occupancy, action.FirstControl!);
        string before = JsonSerializer.Serialize(altered);
        Assert.Equal(field, Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(altered, 1, Policy)).ParamName);
        Assert.Equal(before, JsonSerializer.Serialize(altered)); Assert.Null(altered.TurnCompletion);
    }

    [Fact]
    public void MissingPolicyWrongActorWrongPhaseAndInconsistentPlacementCannotCommit()
    {
        var action = ActionChoice();
        Assert.Equal("policy", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(action, 1, null)).ParamName);
        Assert.Equal("actor", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(action, 2, Policy)).ParamName);
        var cancelled = Battle01PlayerMovement.Cancel(Battle01PlayerMovement.SelectDestination(
            Battle01FirstControl.Enter(Battle01FirstControlTests.Round(), 1).State!, 1, new(9, 17)), 1);
        Assert.Equal("phase", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(cancelled, 1, Policy)).ParamName);
        var inconsistent = new Battle01InitializedState(action, action.Roster.ToArray(),
            Array.AsReadOnly(Enumerable.Repeat(-1, 2304).ToArray()), action.FirstControl!);
        Assert.Equal("occupancy", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(inconsistent, 1, Policy)).ParamName);
        Assert.Equal(0, action.FirstRound!.CurrentTurnOffset); Assert.Null(action.TurnCompletion);
    }

    [Fact]
    public void DefeatedWrapperPrecedesPolicyAndIncompleteRosterCannotNormalize()
    {
        var action = ActionChoice();
        var missing = new Battle01InitializedState(action, action.Roster.Where(unit => unit.Index != 128).ToArray(),
            action.Occupancy, action.FirstControl!);
        Assert.Equal("defeated", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(missing, 1, null)).ParamName);
        var incomplete = new Battle01InitializedState(action, action.Roster.Where(unit => unit.Index != 2).ToArray(),
            action.Occupancy, action.FirstControl!);
        Assert.Equal("roster", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(incomplete, 1, Policy)).ParamName);
        var unplaced = action.Roster.Select(unit => unit.Index == 2 ? unit.WithPosition(new(47, 47)) : unit).ToArray();
        var outside = new Battle01InitializedState(action, unplaced, action.Occupancy, action.FirstControl!);
        Assert.Equal("position", Assert.Throws<ArgumentException>(() => Battle01TurnCompletion.CommitStay(outside, 1, Policy)).ParamName);
        Assert.Same(action.FirstRound, outside.FirstRound); Assert.Null(outside.TurnCompletion);
    }

    private static Battle01InitializedState ActionChoice() =>
        Battle01PlayerMovement.Confirm(Battle01FirstControl.Enter(Battle01FirstControlTests.Round(), 1).State!, 1);
}
