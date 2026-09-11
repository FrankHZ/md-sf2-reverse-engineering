using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01DefeatReturnTests
{
    [Theory]
    [InlineData(4, false, false)] [InlineData(3, true, false)] [InlineData(3, false, true)]
    public void ReturnAdmissionRejectsEveryUnsupportedBranch(byte egress, bool flag64, bool flag640) =>
        Assert.Throws<ArgumentException>(() => new Battle01DefeatReturnAdmission(egress, flag64, flag640));

    [Fact]
    public void BattleEntryEffectAndEarlyBindingSurviveFirstRoundCopies()
    {
        var shape = Battle01FirstRoundTests.Initial();
        var admission = new Battle01DefeatReturnAdmission(3, false, false);
        var initial = Battle01Initialization.Initialize(shape.Roster.Select(u => u.Deployment), shape.Regions,
            shape.Terrain, shape.Roster.Take(3).Select(u => new Battle01AllyInput((byte)u.Index,
                (byte)(u.Index == 0 ? 0 : u.Index == 1 ? 4 : 1),
                Battle01FirstControlTests.Stats(u.Stats, 0, (byte)(u.Index == 0 ? 6 : u.Index == 1 ? 5 : 7)))),
            Battle01InitializationTests.Enemy(), 0x1234, 0, returnAdmission: admission);
        var current = Battle01FirstRound.Enter(initial);
        for (int turn = 0; turn < 2; turn++)
        {
            Assert.True(current.BattleEntryFlag399); Assert.Same(admission, current.ReturnAdmission);
            if (turn == 0)
            {
                int actor = current.FirstRound!.CurrentCandidate!.Value.CombatantIndex;
                var controlled = Battle01FirstControl.Enter(current, actor).State!;
                current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(controlled, actor), actor,
                    Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
            }
            else current = Battle01FirstRoundTests.CompleteOriginTurn(current);
        }
        Assert.True(initial.BattleEntryFlag399); Assert.True(current.BattleEntryFlag399);
        Assert.Same(admission, current.ReturnAdmission);
        Assert.Null(shape.ReturnAdmission); Assert.True(shape.BattleEntryFlag399);
        Assert.Throws<ArgumentException>(() => Battle01DefeatReturn.Select(current, admission));
    }

    [Fact]
    public void AValidTupleCannotBeAttachedAfterLegacyRecovery()
    {
        var current = Battle01DefeatRecovery.Complete(Battle01EnemyPhysicalAttackTests.LeaderDefeatCompleted());
        var json = new JsonSerializerOptions { MaxDepth = 256 };
        string frozen = JsonSerializer.Serialize(current, json);
        Assert.Null(current.ReturnAdmission); Assert.True(current.BattleEntryFlag399);
        Assert.Throws<ArgumentException>(() => Battle01DefeatReturn.Select(current, null));
        Assert.Equal("return.binding", Assert.Throws<ArgumentException>(() =>
            Battle01DefeatReturn.Select(current, new(3, false, false))).ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(current, json));
    }

    [Theory]
    [InlineData("hp")] [InlineData("gold")] [InlineData("history")] [InlineData("occupancy")]
    public void ReturnAuthenticatesTheEntireRecoveryBeforeSelectingADestination(string change)
    {
        var current = Battle01DefeatRecovery.Complete(Battle01EnemyPhysicalAttackTests.LeaderDefeatCompleted());
        var roster = current.Roster.ToArray();
        if (change == "hp") roster[0] = roster[0].WithStats(roster[0].Stats.WithCurrentHp(0));
        var forged = new Battle01InitializedState(current, roster, current.RandomSeedImage,
            change == "gold" ? 30u : current.CurrentGold);
        if (change == "history") forged = new(forged, current.FirstRound!, current.TurnCompletion!.Previous!);
        if (change == "occupancy") forged = new(forged, roster, Enumerable.Repeat(-1, 2304).ToArray(), null!);
        string frozen = JsonSerializer.Serialize(forged, new JsonSerializerOptions { MaxDepth = 256 });
        var failure = Assert.Throws<ArgumentException>(() => Battle01DefeatReturn.Select(forged, new(3, false, false)));
        Assert.NotEqual("return.binding", failure.ParamName);
        Assert.Equal(frozen, JsonSerializer.Serialize(forged, new JsonSerializerOptions { MaxDepth = 256 }));
    }
}
