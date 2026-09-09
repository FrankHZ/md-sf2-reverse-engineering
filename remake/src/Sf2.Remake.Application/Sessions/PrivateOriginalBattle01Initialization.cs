using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed class PrivateOriginalBattle01SessionSnapshot
{
    internal PrivateOriginalBattle01SessionSnapshot(PrivateOriginalBattle01StartupPrepared preparation,
        Battle01InitializedState battle, PrivateOriginalMapPlayerLocomotionSnapshot sourceLocomotion,
        PrivateOriginalMapBattleBridgeSnapshot? sourceBridge)
    {
        Preparation = preparation; Battle = battle; SourceLocomotion = sourceLocomotion; SourceBridge = sourceBridge;
    }
    public GameFlowStage FlowStage => GameFlowStage.Battle;
    public MapId Map => Battle.Map;
    public Battle01InitializedState Battle { get; }
    // Frozen provenance only; none of these values is current exploration or a reusable admission.
    public PrivateOriginalBattle01StartupPrepared Preparation { get; }
    public PrivateOriginalMapSessionSnapshot SourceSnapshot => Preparation.Pending.SourceSnapshot;
    public PrivateOriginalMapPlayerLocomotionSnapshot SourceLocomotion { get; }
    public PrivateOriginalMapBattleBridgeSnapshot? SourceBridge { get; }
}

public abstract record PrivateOriginalBattle01InitializationResult;
public sealed record PrivateOriginalBattle01Initialized(PrivateOriginalBattle01SessionSnapshot Snapshot)
    : PrivateOriginalBattle01InitializationResult;
public sealed record PrivateOriginalBattle01InitializationRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : PrivateOriginalBattle01InitializationResult;

public sealed partial class GameSession
{
    public PrivateOriginalBattle01SessionSnapshot? PrivateOriginalBattle01 { get; private set; }

    public GameFlowStage PrivateOriginalFlowStage => _privateOriginalMapSnapshot is null
        ? throw new InvalidOperationException("This session does not own the private original profile.")
        : PrivateOriginalBattle01 is null ? GameFlowStage.Exploration : GameFlowStage.Battle;
    public MapId PrivateOriginalCurrentMap => PrivateOriginalBattle01?.Map ?? PrivateOriginalMapSnapshot.Map;

    public PrivateOriginalBattle01InitializationResult InitializePrivateOriginalBattle01(
        PrivateOriginalBattle01StartupPrepared? prepared)
    {
        if (PrivateOriginalBattle01 is not null)
            return InitializationRejected("battle", "Battle 01 has already initialized; its admission is consumed.");
        if (prepared is null)
            return InitializationRejected("prepared", "A prepared startup request is required.");
        if (PrivateOriginalBattle01Admission is null ||
            !ReferenceEquals(prepared.Pending, PrivateOriginalBattle01Admission) ||
            !ReferenceEquals(prepared.Pending.SourceSnapshot, _privateOriginalMapSnapshot))
            return InitializationRejected("pending", "Initialization requires this session's exact current, unconsumed pending admission.");
        if (!OriginalMapRuntimeAdmission.HasExactAcceptedBattle01Admission(prepared.Pending.Definition) ||
            prepared.Pending.SourceSnapshot.MiddleTowerGuard?.ProgramStoryFlag1Set != true ||
            PrivateOriginalMapPlayerLocomotion.IsMoving || IsPrivateOriginalMapBattleBridgeBusy)
            return InitializationRejected("pending.state", "The idle controlled new-battle route and F401 unlock are required.");
        if (prepared.Party.GetAdmissionDiagnostic() is { } partyFailure)
            return new PrivateOriginalBattle01InitializationRejected(partyFailure);
        if (prepared.Inputs.GetAdmissionDiagnostic() is { } inputFailure)
            return new PrivateOriginalBattle01InitializationRejected(inputFailure);

        Battle01InitializedState battle;
        try { battle = ProjectBattle01Initialization(prepared.Inputs, prepared.Party); }
        catch (ArgumentException error)
        {
            return InitializationRejected("initialization." + (error.ParamName ?? "state"),
                "The prepared inputs do not form a valid controlled initialized battle.");
        }
        var next = new PrivateOriginalBattle01SessionSnapshot(prepared, battle,
            PrivateOriginalMapPlayerLocomotion, PrivateOriginalMapBattleBridge);
        var applied = new PrivateOriginalBattle01Initialized(next);
        // All validation, allocation and projection precede this non-throwing commit.
        // The old backing snapshot remains solely to identify the private profile and retain provenance.
        PrivateOriginalBattle01Admission = null;
        PrivateOriginalBattle01 = next;
        return applied;
    }

    private static Battle01InitializedState ProjectBattle01Initialization(
        OriginalBattle01StartupDefinition inputs, OriginalBattle01ControlledPartyPreset party)
    {
        var enemy = inputs.EnemyBaseline;
        return Battle01Initialization.Initialize(
            inputs.Entities.Select(row => new Battle01Deployment(row.Ordinal, row.CombatantIndex, row.Identity,
                row.Position, (byte)row.AiCommandSet, row.ItemWord, row.PrimaryOrder, row.PrimaryRegion,
                row.SecondaryOrder, row.SecondaryRegion, row.Filler, (byte)row.Spawn)),
            inputs.Regions.Select(region => new Battle01Region(region.Id, region.Unknown, region.Vertices,
                region.TrailingByte0, region.TrailingByte1)), inputs.Terrain,
            party.Allies.Select(ally => new Battle01AllyInput(ally.Id, ally.ClassId,
                new(ally.Level, ally.HpMax, ally.HpCurrent, ally.MpMax, ally.MpCurrent, ally.EffectiveAttack,
                    ally.EffectiveDefense, ally.EffectiveAgility, ally.EffectiveMove, ally.StatusEffects, ally.Items, ally.Spells))),
            new(enemy.EnemyDefinitionId, enemy.SourceUnknownByte, enemy.SpellPowerMode,
                new(enemy.Level, enemy.HpMax, enemy.HpMax, enemy.MpMax, enemy.MpMax, enemy.BaseAttack,
                    enemy.BaseDefense, enemy.BaseAgility, enemy.BaseMove, enemy.InitialStatus, enemy.Items, enemy.Spells),
                enemy.BaseResistance, enemy.BaseProwess, enemy.MovementType, enemy.BaseAiBitfield),
            party.RandomSeed, party.Difficulty, party.RandomSeedCopy);
    }

    private static PrivateOriginalBattle01InitializationRejected InitializationRejected(string field, string message) =>
        new(new(field, message));
}
