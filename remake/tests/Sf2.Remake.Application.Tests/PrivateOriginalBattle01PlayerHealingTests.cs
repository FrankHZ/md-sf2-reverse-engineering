using System.Text.Json;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01PlayerHealingTests
{
    internal static GameSession Ready()
    {
        var session = PrivateOriginalBattle01FirstRoundTests.FiveSurvivorSession(OriginalBattle01ControlledPartyPreset.SarahHealComparison);
        Assert.IsType<PrivateOriginalBattle01FirstRoundEntered>(session.EnterPrivateOriginalBattle01NextRound(session.PrivateOriginalBattle01));
        for (int i = 0; i < 2; i++) Assert.IsType<PrivateOriginalBattle01EnemyPursuitCompleted>(session.CompletePrivateOriginalBattle01EnemyPursuit(
            session.PrivateOriginalBattle01, session.PrivateOriginalBattle01!.Battle.FirstRound!.CurrentCandidate!.Value.CombatantIndex));
        Assert.IsType<PrivateOriginalBattle01NextPlayerControlEntered>(session.EnterPrivateOriginalBattle01NextPlayerControl(session.PrivateOriginalBattle01, 1));
        return session;
    }

    internal static GameSession Selected()
    {
        var session = Ready();
        Assert.IsType<PrivateOriginalBattle01PlayerMovementApplied>(session.ConfirmPrivateOriginalBattle01PlayerMovement(session.PrivateOriginalBattle01, 1));
        Assert.IsType<PrivateOriginalBattle01PlayerHealingApplied>(session.BeginPrivateOriginalBattle01PlayerHealing(session.PrivateOriginalBattle01, 1));
        Assert.IsType<PrivateOriginalBattle01PlayerHealingApplied>(session.SelectPrivateOriginalBattle01HealingSpell(session.PrivateOriginalBattle01, 1, 0));
        return session;
    }

    [Fact]
    public void PlayerHealingPublishesOneCompleteSnapshotAndRejectsStaleOrCopiedRequests()
    {
        var session = Selected(); var before = session.PrivateOriginalBattle01!; string frozen = Json(before);
        foreach (var invalid in new PrivateOriginalBattle01SessionSnapshot?[] { null,
            new(before.Preparation, before.Battle, before.SourceLocomotion, before.SourceBridge), Selected().PrivateOriginalBattle01 })
        {
            Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01PlayerHealingRejected>(session.ConfirmPrivateOriginalBattle01PlayerHealing(invalid, 1)).Diagnostic.Field);
            Assert.Same(before, session.PrivateOriginalBattle01); Assert.Equal(frozen, Json(before));
        }
        var local = Battle01PlayerHealing.Confirm(before.Battle, 1, Battle01HealingCompletionPolicy.ControlledSarahHealOneBowie);
        var after = Assert.IsType<PrivateOriginalBattle01PlayerHealingApplied>(session.ConfirmPrivateOriginalBattle01PlayerHealing(before, 1)).Snapshot;
        Assert.Equal(Json(local), Json(after.Battle)); Assert.NotSame(before, after);
        Assert.Same(before.Preparation, after.Preparation); Assert.Same(before.SourceLocomotion, after.SourceLocomotion); Assert.Same(before.SourceBridge, after.SourceBridge);
        Assert.Equal(frozen, Json(before));
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01PlayerHealingRejected>(session.ConfirmPrivateOriginalBattle01PlayerHealing(before, 1)).Diagnostic.Field);
        Assert.IsType<PrivateOriginalBattle01PlayerHealingRejected>(session.ConfirmPrivateOriginalBattle01PlayerHealing(after, 1));
        Assert.Same(after, session.PrivateOriginalBattle01);
    }

    internal static string Json(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions { MaxDepth = 256 });
}
