using System.Text.Json;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01DefeatReturnTests
{
    private sealed class Source : IOriginalBattle01StartupSource
    {
        public int Calls { get; private set; }
        public OriginalBattle01StartupImportResult Admit()
        {
            Calls++;
            return new OriginalBattle01StartupImported(PrivateOriginalBattle01StartupTests.Definition());
        }
    }

    [Theory]
    [InlineData("id")] [InlineData("egress")] [InlineData("flag64")] [InlineData("flag640")]
    public void InvalidReturnInputsRejectBeforeSourceAdmission(string field)
    {
        var session = PrivateOriginalBattle01StartupTests.PendingSession(); var source = new Source();
        var pending = session.PrivateOriginalBattle01Admission;
        var selected = OriginalBattle01ControlledReturnInputs.GransealFirstAttemptComparison;
        var invalid = field switch {
            "id" => selected with { Id = "unknown" }, "egress" => selected with { EgressMap = 4 },
            "flag64" => selected with { Flag64 = true }, _ => selected with { Flag640 = true } };
        Assert.Equal("return.inputs", Assert.IsType<PrivateOriginalBattle01StartupRejected>(
            session.PreparePrivateOriginalBattle01Startup(pending, source,
                OriginalBattle01ControlledPartyPreset.LeaderDefeatComparison, invalid)).Diagnostic.Field);
        Assert.Equal(0, source.Calls); Assert.Same(pending, session.PrivateOriginalBattle01Admission);
        Assert.Null(session.PrivateOriginalBattle01);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void InitializationBindsOnlyTheExplicitPreparation(bool selected)
    {
        var session = PrivateOriginalBattle01StartupTests.PendingSession(); var source = new Source();
        var input = selected ? OriginalBattle01ControlledReturnInputs.GransealFirstAttemptComparison : null;
        var prepared = Assert.IsType<PrivateOriginalBattle01StartupPrepared>(session.PreparePrivateOriginalBattle01Startup(
            session.PrivateOriginalBattle01Admission, source, OriginalBattle01ControlledPartyPreset.LeaderDefeatComparison, input));
        var clone = prepared with { };
        if (selected) Assert.NotSame(prepared.ReturnAdmission, clone.ReturnAdmission);
        var current = Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(prepared)).Snapshot;
        Assert.Same(prepared, current.Preparation); Assert.Same(input, prepared.ReturnInputs);
        Assert.Same(prepared.ReturnAdmission, current.Battle.ReturnAdmission); Assert.True(current.Battle.BattleEntryFlag399);
        Assert.Equal(selected, current.Battle.ReturnAdmission is not null); Assert.False(current.CanRequestDefeatReturn);
        Assert.IsType<PrivateOriginalBattle01DefeatReturnRejected>(session.RequestPrivateOriginalBattle01DefeatReturn(current));
        Assert.Same(current, session.PrivateOriginalBattle01); Assert.Equal(1, source.Calls);
    }

    [Fact]
    public void ReplacingLegacyPreparationAfterRecoveryCannotGrantReturnPermission()
    {
        var session = PrivateOriginalBattle01EnemyPhysicalAttackTests.FirstAllyDefeatSession(leader: true);
        var terminal = Assert.IsType<PrivateOriginalBattle01EnemyPhysicalAttackCompleted>(
            session.CompletePrivateOriginalBattle01EnemyPhysicalAttack(session.PrivateOriginalBattle01, 129)).Snapshot;
        var recovered = Assert.IsType<PrivateOriginalBattle01DefeatRecovered>(session.RecoverPrivateOriginalBattle01Defeat(terminal)).Snapshot;
        Assert.False(recovered.CanRequestDefeatReturn);
        Assert.IsType<PrivateOriginalBattle01DefeatReturnRejected>(session.RequestPrivateOriginalBattle01DefeatReturn(recovered));
        var late = new PrivateOriginalBattle01StartupPrepared(recovered.Preparation.Pending, recovered.Preparation.Inputs,
            recovered.Preparation.Party, OriginalBattle01ControlledReturnInputs.GransealFirstAttemptComparison);
        var forged = new PrivateOriginalBattle01SessionSnapshot(late, recovered.Battle, recovered.SourceLocomotion, recovered.SourceBridge);
        typeof(GameSession).GetProperty(nameof(GameSession.PrivateOriginalBattle01))!.SetValue(session, forged);
        string frozen = JsonSerializer.Serialize(forged.Battle, new JsonSerializerOptions { MaxDepth = 256 });
        Assert.False(forged.CanRequestDefeatReturn);
        Assert.Equal("snapshot", Assert.IsType<PrivateOriginalBattle01DefeatReturnRejected>(
            session.RequestPrivateOriginalBattle01DefeatReturn(recovered)).Diagnostic.Field);
        Assert.Equal("return.binding", Assert.IsType<PrivateOriginalBattle01DefeatReturnRejected>(
            session.RequestPrivateOriginalBattle01DefeatReturn(forged)).Diagnostic.Field);
        Assert.Same(forged, session.PrivateOriginalBattle01); Assert.Null(forged.DefeatReturn);
        Assert.Equal(frozen, JsonSerializer.Serialize(forged.Battle, new JsonSerializerOptions { MaxDepth = 256 }));
    }
}
