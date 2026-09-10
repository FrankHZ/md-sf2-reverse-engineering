using System.Reflection;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01InitializationTests
{
    [Fact]
    public void ChesterDefeatsSupplementInitializesOnlyHisExplicitCounterAndPreservesUnknownKills()
    {
        var old = OriginalBattle01ControlledPartyPreset.ChesterPlayerAttackComparison;
        var preset = OriginalBattle01ControlledPartyPreset.ChesterDefeatComparison;
        Assert.Null(preset.GetAdmissionDiagnostic());
        Assert.Same(old.Allies[0], preset.Allies[0]); Assert.Same(old.Allies[1], preset.Allies[1]);
        Assert.Null(old.Allies[2].CurrentDefeats); Assert.Equal((ushort?)0, preset.Allies[2].CurrentDefeats);
        Assert.Equal(old.Allies[2].CurrentExp, preset.Allies[2].CurrentExp);
        Assert.Null(preset.Allies[2].CurrentKills); Assert.Equal(old.CurrentGold, preset.CurrentGold);
        var session = PrivateOriginalBattle01StartupTests.PendingSession(); var source = Prepare(session);
        var prepared = new PrivateOriginalBattle01StartupPrepared(source.Pending, source.Inputs, preset);
        var current = Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(prepared)).Snapshot;
        Assert.Same(prepared, current.Preparation);
        Assert.Equal(new ushort?[] { null, null, 0 }, current.Battle.Roster.Take(3).Select(u => u.Stats.CurrentDefeats));
        Assert.Equal(new ushort?[] { 0, null, null }, current.Battle.Roster.Take(3).Select(u => u.Stats.CurrentKills));
        Assert.Equal(new byte?[] { 0, null, 0 }, current.Battle.Roster.Take(3).Select(u => u.Stats.CurrentExp));
        Assert.All(current.Battle.Roster.Skip(3), u => Assert.Null(u.Stats.CurrentDefeats));
        foreach (var prior in new[] { OriginalBattle01ControlledPartyPreset.PlayerReadyComparison,
            OriginalBattle01ControlledPartyPreset.PlayerAttackComparison,
            OriginalBattle01ControlledPartyPreset.FirstDefeatComparison, old })
            Assert.All(prior.Allies, a => Assert.Null(a.CurrentDefeats));
    }

    [Fact]
    public void ChesterSupplementSuppliesOnlyHisExpAtInitializationAndRetainsTheThreeOlderPresets()
    {
        var old = OriginalBattle01ControlledPartyPreset.FirstDefeatComparison;
        var preset = OriginalBattle01ControlledPartyPreset.ChesterPlayerAttackComparison;
        Assert.Null(preset.GetAdmissionDiagnostic());
        Assert.Same(old.Allies[0], preset.Allies[0]); Assert.Same(old.Allies[1], preset.Allies[1]);
        Assert.Equal(old.CurrentGold, preset.CurrentGold); Assert.Null(preset.Allies[2].CurrentKills);
        foreach (var prior in new[] { OriginalBattle01ControlledPartyPreset.PlayerReadyComparison,
            OriginalBattle01ControlledPartyPreset.PlayerAttackComparison, old })
        {
            Assert.Null(prior.GetAdmissionDiagnostic()); Assert.Null(prior.Allies[2].CurrentExp);
            Assert.Null(prior.Allies[2].CurrentKills);
        }
        var session = PrivateOriginalBattle01StartupTests.PendingSession(); var source = Prepare(session);
        var prepared = new PrivateOriginalBattle01StartupPrepared(source.Pending, source.Inputs, preset);
        var initialized = Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(prepared)).Snapshot;
        Assert.Equal(new byte?[] { 0, null, 0 }, initialized.Battle.Roster.Take(3).Select(u => u.Stats.CurrentExp));
        Assert.Equal(new ushort?[] { 0, null, null }, initialized.Battle.Roster.Take(3).Select(u => u.Stats.CurrentKills));
        Assert.Equal((uint?)0, initialized.Battle.CurrentGold);
        foreach (var (id, allies) in new[] { (preset.Id, old.Allies), (old.Id, preset.Allies) })
            Assert.Equal("party.allies", new OriginalBattle01ControlledPartyPreset(id, preset.RandomSeed,
                preset.Difficulty, allies, preset.RandomSeedCopy, preset.CurrentGold).GetAdmissionDiagnostic()!.Field);
    }

    [Fact]
    public void FirstDefeatSupplementDeclaresGoldAndBowiesKillsWithoutChangingEarlierPresets()
    {
        foreach (var old in new[] { OriginalBattle01ControlledPartyPreset.PlayerReadyComparison,
            OriginalBattle01ControlledPartyPreset.PlayerAttackComparison })
        {
            Assert.Null(old.CurrentGold); Assert.All(old.Allies, ally => Assert.Null(ally.CurrentKills));
        }
        var session = PrivateOriginalBattle01StartupTests.PendingSession(); var source = Prepare(session);
        var preset = OriginalBattle01ControlledPartyPreset.FirstDefeatComparison;
        Assert.Null(preset.GetAdmissionDiagnostic());
        var prepared = new PrivateOriginalBattle01StartupPrepared(source.Pending, source.Inputs, preset);
        var initialized = Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(prepared)).Snapshot;
        Assert.Same(prepared, initialized.Preparation);
        Assert.Equal(((uint?)0, (ushort?)0, (byte?)0), (initialized.Battle.CurrentGold,
            initialized.Battle.Roster[0].Stats.CurrentKills, initialized.Battle.Roster[0].Stats.CurrentExp));
        Assert.All(initialized.Battle.Roster.Skip(1), unit => Assert.Null(unit.Stats.CurrentKills));
        Assert.Equal("party.goldInput", new OriginalBattle01ControlledPartyPreset(preset.Id, preset.RandomSeed, preset.Difficulty,
            preset.Allies, preset.RandomSeedCopy).GetAdmissionDiagnostic()!.Field);
        Assert.Equal("party.allies", new OriginalBattle01ControlledPartyPreset(preset.Id, preset.RandomSeed, preset.Difficulty,
            OriginalBattle01ControlledPartyPreset.PlayerAttackComparison.Allies, preset.RandomSeedCopy, 0).GetAdmissionDiagnostic()!.Field);
        Assert.Equal("party.goldInput", new OriginalBattle01ControlledPartyPreset(OriginalBattle01ControlledPartyPreset.PlayerAttackComparisonId,
            preset.RandomSeed, preset.Difficulty, OriginalBattle01ControlledPartyPreset.PlayerAttackComparison.Allies,
            preset.RandomSeedCopy, 0).GetAdmissionDiagnostic()!.Field);
    }

    [Fact]
    public void NamedPlayerAttackPresetSuppliesOnlyBowiesExplicitExpZero()
    {
        var session = PrivateOriginalBattle01StartupTests.PendingSession(); var old = Prepare(session);
        Assert.All(old.Party.Allies, ally => Assert.Null(ally.CurrentExp));
        var preset = OriginalBattle01ControlledPartyPreset.PlayerAttackComparison;
        Assert.Null(preset.GetAdmissionDiagnostic());
        var prepared = new PrivateOriginalBattle01StartupPrepared(old.Pending, old.Inputs, preset);
        var initialized = Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(prepared)).Snapshot;
        Assert.Equal(preset.Id, initialized.Preparation.Party.Id);
        Assert.Equal((byte?)0, initialized.Battle.Roster[0].Stats.CurrentExp);
        Assert.All(initialized.Battle.Roster.Skip(1), unit => Assert.Null(unit.Stats.CurrentExp));
        Assert.Equal("party.allies", new OriginalBattle01ControlledPartyPreset(preset.Id, preset.RandomSeed, preset.Difficulty,
            old.Party.Allies, preset.RandomSeedCopy).GetAdmissionDiagnostic()!.Field);
        Assert.Equal("party.allies", new OriginalBattle01ControlledPartyPreset(old.Party.Id, preset.RandomSeed, preset.Difficulty,
            preset.Allies, preset.RandomSeedCopy).GetAdmissionDiagnostic()!.Field);
    }

    [Fact]
    public void InitializationConsumesExactPendingAndMakesMap57TheOnlyCurrentMap()
    {
        var session = PrivateOriginalBattle01StartupTests.PendingSession();
        var source = session.PrivateOriginalMapSnapshot; var animation = session.PrivateOriginalMapPlayerLocomotion;
        var bridge = session.PrivateOriginalMapBattleBridge; var pending = session.PrivateOriginalBattle01Admission;
        var prepared = Prepare(session);
        var applied = Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(prepared));
        var snapshot = applied.Snapshot; var battle = snapshot.Battle;
        Assert.Same(snapshot, session.PrivateOriginalBattle01); Assert.Null(session.PrivateOriginalBattle01Admission);
        Assert.Equal(GameFlowStage.Battle, session.PrivateOriginalFlowStage);
        Assert.Equal(GameFlowStage.Battle, snapshot.FlowStage); Assert.Equal(new MapId("map57"), session.PrivateOriginalCurrentMap);
        Assert.Same(prepared, snapshot.Preparation); Assert.Same(pending, snapshot.Preparation.Pending);
        Assert.Same(source, snapshot.SourceSnapshot); Assert.Equal(new MapId("map40"), source.Map);
        Assert.Same(source.Receipt, snapshot.SourceSnapshot.Receipt);
        Assert.Same(source.CastleGate, snapshot.SourceSnapshot.CastleGate);
        Assert.True(snapshot.SourceSnapshot.PalaceFirstVisit!.CompletionFlag605Set);
        Assert.True(snapshot.SourceSnapshot.AstralAcceptance!.HandlerFlag607Set);
        Assert.True(snapshot.SourceSnapshot.AstralAcceptance.ProgramFlag608Set);
        Assert.True(snapshot.SourceSnapshot.MiddleTowerGuard!.ProgramFlag401Set);
        Assert.Same(animation, snapshot.SourceLocomotion); Assert.Same(bridge, snapshot.SourceBridge);
        Assert.Equal(pending!.SourceSimulationStep, source.SimulationStep);
        Assert.Equal(Battle01Phase.BeforeFirstRound, battle.Phase); Assert.Equal(9, battle.Roster.Count);
        Assert.Equal((ushort?)0x1234, battle.RandomSeedCopy); Assert.Equal(prepared.Party.RandomSeedCopy, battle.RandomSeedCopy);
        Assert.Equal(new[] { 0, 1, 2, 128, 129, 130, 131, 132, 133 }, battle.Roster.Select(unit => unit.Index));
        Assert.All(battle.Roster, unit => Assert.Equal(unit.Index, battle.OccupantAt(Assert.IsType<MapPosition>(unit.Position))));
        for (int index = 0; index < 3; index++)
        {
            var ally = prepared.Party.Allies[index]; var actual = battle.Roster[index];
            Assert.Equal(ally.ClassId, actual.ClassId); Assert.Equal(ally.EffectiveAttack, actual.Stats.Attack);
            Assert.Equal(ally.Items, actual.Stats.Items); Assert.Equal(ally.Spells, actual.Stats.Spells);
        }
        Assert.Equal(7, prepared.Inputs.EnemyBaseline.BaseAttack);
        Assert.All(battle.Roster.Skip(3), enemy => Assert.Equal(8, enemy.Stats.Attack));
        Assert.Equal(prepared.Inputs.Terrain, battle.Terrain); Assert.NotSame(prepared.Inputs.Terrain, battle.Terrain);
    }

    [Fact]
    public void DuplicateAndNewPreparationRejectWithoutRepeatingDifficultyOrMutatingBattle()
    {
        var session = PrivateOriginalBattle01StartupTests.PendingSession(); var prepared = Prepare(session);
        var state = Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(prepared)).Snapshot;
        Assert.Equal("battle", Assert.IsType<PrivateOriginalBattle01InitializationRejected>(
            session.InitializePrivateOriginalBattle01(prepared)).Diagnostic.Field);
        Assert.Equal("battle", Assert.IsType<PrivateOriginalBattle01StartupRejected>(
            session.PreparePrivateOriginalBattle01Startup(prepared.Pending, new Source(prepared.Inputs), prepared.Party)).Diagnostic.Field);
        Assert.Same(state, session.PrivateOriginalBattle01); Assert.Null(session.PrivateOriginalBattle01Admission);
        Assert.All(state.Battle.Roster.Skip(3), enemy => Assert.Equal(8, enemy.Stats.Attack));
        Assert.Equal(0x1234u, state.Battle.RandomSeedImage);
    }

    [Theory]
    [InlineData("missing-prepared", "prepared")]
    [InlineData("missing-pending", "pending")]
    [InlineData("foreign", "pending")]
    [InlineData("stale-snapshot", "pending")]
    [InlineData("party", "party.randomSeed")]
    [InlineData("input", "terrain")]
    public void RejectedRequestsPreserveExplorationAndPending(string scenario, string field)
    {
        var session = PrivateOriginalBattle01StartupTests.PendingSession(scenario != "missing-pending");
        var pending = session.PrivateOriginalBattle01Admission;
        var source = session.PrivateOriginalMapSnapshot;
        var animation = session.PrivateOriginalMapPlayerLocomotion; var bridge = session.PrivateOriginalMapBattleBridge;
        var requestSession = pending is null || scenario == "foreign"
            ? PrivateOriginalBattle01StartupTests.PendingSession() : session;
        PrivateOriginalBattle01StartupPrepared? prepared = Prepare(requestSession);
        switch (scenario)
        {
            case "missing-prepared": prepared = null; break;
            case "stale-snapshot":
                source = PrivateOriginalBattle01StartupTests.PendingSession().PrivateOriginalMapSnapshot;
                typeof(GameSession).GetField("_privateOriginalMapSnapshot", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, source);
                break;
            case "party":
                prepared = new(prepared.Pending, prepared.Inputs, new(prepared.Party.Id, 1, 0, prepared.Party.Allies));
                break;
            case "input":
                prepared = new(prepared.Pending, PrivateOriginalBattle01StartupTests.Definition(computeTerrain: true), prepared.Party);
                break;
        }
        var rejected = Assert.IsType<PrivateOriginalBattle01InitializationRejected>(session.InitializePrivateOriginalBattle01(prepared));
        Assert.Equal(field, rejected.Diagnostic.Field); Assert.Null(session.PrivateOriginalBattle01);
        Assert.Same(source, session.PrivateOriginalMapSnapshot); Assert.Same(pending, session.PrivateOriginalBattle01Admission);
        Assert.Same(animation, session.PrivateOriginalMapPlayerLocomotion); Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
        Assert.Equal(GameFlowStage.Exploration, session.PrivateOriginalFlowStage);
    }

    [Fact]
    public void LateTerrainOccupancyValidationFailureDoesNotCommitAnySessionChange()
    {
        var session = PrivateOriginalBattle01StartupTests.PendingSession();
        var pending = session.PrivateOriginalBattle01Admission; var source = session.PrivateOriginalMapSnapshot;
        var animation = session.PrivateOriginalMapPlayerLocomotion; var bridge = session.PrivateOriginalMapBattleBridge;
        // Authored definition uses the existing internal digest seam. Application trust checks pass,
        // then Domain rejects an occupied obstructed cell during final projection, before commit.
        byte[] terrain = new byte[2304]; terrain[48] = 255;
        var definition = PrivateOriginalBattle01StartupTests.Definition(terrain: terrain);
        var prepared = Prepare(session, definition);
        var rejected = Assert.IsType<PrivateOriginalBattle01InitializationRejected>(session.InitializePrivateOriginalBattle01(prepared));
        Assert.Equal("initialization.terrain", rejected.Diagnostic.Field);
        Assert.Null(session.PrivateOriginalBattle01); Assert.Same(pending, session.PrivateOriginalBattle01Admission);
        Assert.Same(source, session.PrivateOriginalMapSnapshot); Assert.Same(animation, session.PrivateOriginalMapPlayerLocomotion);
        Assert.Same(bridge, session.PrivateOriginalMapBattleBridge); Assert.Equal(255, definition.Terrain[48]);
        Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(Prepare(session)));
    }

    [Fact]
    public void OldMovementAnimationInteractionAndBridgeEntryPointsCannotOperateFrozenMap40()
    {
        var session = PrivateOriginalBattle01StartupTests.PendingSession();
        var snapshot = Assert.IsType<PrivateOriginalBattle01Initialized>(session.InitializePrivateOriginalBattle01(Prepare(session))).Snapshot;
        var command = new MoveExplorationCommand(ExplorationDirection.North);
        Assert.Throws<InvalidOperationException>(() => session.PrivateOriginalMapSnapshot);
        Assert.Throws<InvalidOperationException>(() => session.ApplyPrivateOriginalMap(command));
        Assert.Throws<InvalidOperationException>(() => session.BeginPrivateOriginalMapPlayerLocomotion(command));
        Assert.Throws<InvalidOperationException>(() => session.AdvancePrivateOriginalMapPlayerLocomotion());
        Assert.Throws<InvalidOperationException>(() => session.RequestPrivateOriginalMapInteraction(snapshot.SourceSnapshot.SimulationStep));
        Assert.Throws<InvalidOperationException>(() => session.ApplyPrivateOriginalMapBattleBridge(command));
        Assert.Throws<InvalidOperationException>(() => session.Apply(command));
        Assert.Same(snapshot, session.PrivateOriginalBattle01);
        Assert.Same(snapshot.SourceBridge, session.PrivateOriginalMapBattleBridge);
    }

    [Fact]
    public void FreshSessionStartsAtControlledMap3WithoutPendingOrBattle()
    {
        var consumed = PrivateOriginalBattle01StartupTests.PendingSession(); var prepared = Prepare(consumed);
        consumed.InitializePrivateOriginalBattle01(prepared);
        var initial = prepared.Pending.SourceSnapshot;
        var fresh = Assert.IsType<PrivateOriginalMapGameSessionStarted>(GameSession.StartPrivateOriginalMap(
            new MapSource(initial.Definition, initial.Receipt),
            new(initial.Receipt.PackageId, ContentProfile.PrivateLocal, initial.Receipt.ContentDigest))).Session;
        Assert.Equal(new MapId("map3"), fresh.PrivateOriginalCurrentMap); Assert.Equal(GameFlowStage.Exploration, fresh.PrivateOriginalFlowStage);
        Assert.Null(fresh.PrivateOriginalBattle01); Assert.Null(fresh.PrivateOriginalBattle01Admission);
        Assert.Equal(0, fresh.PrivateOriginalMapSnapshot.SimulationStep);
    }

    private sealed class Source(OriginalBattle01StartupDefinition definition) : IOriginalBattle01StartupSource
    {
        public OriginalBattle01StartupImportResult Admit() => new OriginalBattle01StartupImported(definition);
    }
    internal sealed class MapSource(OriginalMapImportDefinition definition, OriginalMapImportReceipt receipt) : IOriginalMapImportSource
    {
        public OriginalMapImportResult Admit(OriginalMapImportRequest request) => new OriginalMapImportAccepted(definition, receipt);
    }
    internal static PrivateOriginalBattle01StartupPrepared Prepare(GameSession session, OriginalBattle01StartupDefinition? definition = null) =>
        Assert.IsType<PrivateOriginalBattle01StartupPrepared>(session.PreparePrivateOriginalBattle01Startup(
            session.PrivateOriginalBattle01Admission, new Source(definition ?? PrivateOriginalBattle01StartupTests.Definition()),
            OriginalBattle01ControlledPartyPreset.PlayerReadyComparison));
}
