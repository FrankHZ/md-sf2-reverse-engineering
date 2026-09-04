using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum PrivateOriginalMapZone601LifecyclePhase
{
    Ready,
    AmbientWalkingHandoff,
    AstralZoneRepositioned,
}

public sealed record PrivateOriginalMapZone601State
{
    private PrivateOriginalMapZone601State(
        PrivateOriginalMapZone601LifecyclePhase phase,
        OriginalMapEntityRecordIdentity actorSourceRecord,
        int logicalActorId,
        MapPosition actorPosition,
        byte actorOpaqueFacing,
        string actorBehaviorIdentity,
        bool flag601Set,
        MapPosition? ambientCenter,
        int? ambientRange)
    {
        if (!Enum.IsDefined(phase))
        {
            throw new ArgumentOutOfRangeException(nameof(phase));
        }

        ActorSourceRecord = actorSourceRecord ??
            throw new ArgumentNullException(nameof(actorSourceRecord));
        ArgumentOutOfRangeException.ThrowIfNegative(logicalActorId);
        ActorPosition = actorPosition ?? throw new ArgumentNullException(nameof(actorPosition));
        if (actorOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(actorOpaqueFacing));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(actorBehaviorIdentity);
        if (phase == PrivateOriginalMapZone601LifecyclePhase.Ready &&
            (flag601Set || ambientCenter is not null || ambientRange is not null) ||
            phase == PrivateOriginalMapZone601LifecyclePhase.AmbientWalkingHandoff &&
            (!flag601Set || ambientCenter is null || ambientRange is null || ambientRange < 0) ||
            phase == PrivateOriginalMapZone601LifecyclePhase.AstralZoneRepositioned &&
            (!flag601Set || ambientCenter is null || ambientRange is null || ambientRange < 0))
        {
            throw new ArgumentException(
                "The Zone 601 state must retain an exact ready, ambient-handoff, or Astral-zone shape.");
        }

        Phase = phase;
        LogicalActorId = logicalActorId;
        ActorOpaqueFacing = actorOpaqueFacing;
        ActorBehaviorIdentity = actorBehaviorIdentity;
        Flag601Set = flag601Set;
        AmbientCenter = ambientCenter;
        AmbientRange = ambientRange;
    }

    public PrivateOriginalMapZone601LifecyclePhase Phase { get; }

    public OriginalMapEntityRecordIdentity ActorSourceRecord { get; }

    public int LogicalActorId { get; }

    public MapPosition ActorPosition { get; }

    public byte ActorOpaqueFacing { get; }

    public string ActorBehaviorIdentity { get; }

    public bool Flag601Set { get; }

    public MapPosition? AmbientCenter { get; }

    public int? AmbientRange { get; }

    internal static PrivateOriginalMapZone601State Ready(
        OriginalMapZone601Definition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new(
            PrivateOriginalMapZone601LifecyclePhase.Ready,
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            definition.ActorInitialPosition,
            definition.ActorInitialOpaqueFacing,
            definition.ActorInitialBehaviorIdentity,
            flag601Set: false,
            ambientCenter: null,
            ambientRange: null);
    }

    internal static PrivateOriginalMapZone601State Complete(
        OriginalMapZone601Definition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new(
            PrivateOriginalMapZone601LifecyclePhase.AmbientWalkingHandoff,
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            definition.ActorBlockingEndPosition,
            definition.ActorBlockingEndOpaqueFacing,
            definition.AmbientBehaviorIdentity,
            flag601Set: true,
            definition.AmbientCenter,
            definition.AmbientRange);
    }

    internal static PrivateOriginalMapZone601State AstralZoneRepositioned(
        OriginalMapZone601Definition definition,
        OriginalMapAstralZoneDefinition astralZone)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(astralZone);
        if (astralZone.Zone601ActorSourceRecord != definition.ActorSourceRecord ||
            astralZone.Zone601LogicalActorId != definition.LogicalActorId)
        {
            throw new ArgumentException(
                "The Astral-zone handoff must bind the admitted Zone 601 actor.",
                nameof(astralZone));
        }

        return new(
            PrivateOriginalMapZone601LifecyclePhase.AstralZoneRepositioned,
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            astralZone.Zone601ActorDestination,
            astralZone.Zone601ActorOpaqueFacing,
            definition.AmbientBehaviorIdentity,
            flag601Set: true,
            definition.AmbientCenter,
            definition.AmbientRange);
    }

    internal bool Matches(
        OriginalMapZone601Definition definition,
        OriginalMapAstralZoneDefinition? astralZone = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        PrivateOriginalMapZone601State expected = Phase switch
        {
            PrivateOriginalMapZone601LifecyclePhase.Ready => Ready(definition),
            PrivateOriginalMapZone601LifecyclePhase.AmbientWalkingHandoff =>
                Complete(definition),
            PrivateOriginalMapZone601LifecyclePhase.AstralZoneRepositioned =>
                AstralZoneRepositioned(
                    definition,
                    astralZone ?? throw new InvalidOperationException(
                        "Astral-zone Zone 601 state requires its admitted definition.")),
            _ => throw new InvalidOperationException("Unknown Zone 601 lifecycle phase."),
        };
        return this == expected;
    }
}

public sealed record PrivateOriginalMapZone601Receipt
{
    private readonly ReadOnlyCollection<int> _textIds;
    private readonly ReadOnlyCollection<OriginalMapZone601BlockingStage> _blockingStages;

    internal PrivateOriginalMapZone601Receipt(
        OriginalMapZone601Definition definition,
        MapPosition playerSource,
        MapPosition candidateTarget,
        long simulationStep)
    {
        ArgumentNullException.ThrowIfNull(definition);
        PlayerSource = playerSource ?? throw new ArgumentNullException(nameof(playerSource));
        CandidateTarget = candidateTarget ??
            throw new ArgumentNullException(nameof(candidateTarget));
        if (candidateTarget != definition.Trigger || playerSource == candidateTarget)
        {
            throw new ArgumentException(
                "A Zone 601 receipt must bind the admitted candidate target.",
                nameof(candidateTarget));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        EventIdentity = definition.Identity;
        GateFlag = definition.GateFlag;
        BlockingSequenceIdentity = definition.BlockingSequenceIdentity;
        LogicalActorId = definition.LogicalActorId;
        ActorSourceRecord = definition.ActorSourceRecord;
        ActorInitialPosition = definition.ActorInitialPosition;
        ActorBlockingEndPosition = definition.ActorBlockingEndPosition;
        ActorBlockingEndOpaqueFacing = definition.ActorBlockingEndOpaqueFacing;
        OpaqueFaceWaitOperand = definition.OpaqueFaceWaitOperand;
        _textIds = Array.AsReadOnly(definition.TextIds.ToArray());
        AmbientBehaviorIdentity = definition.AmbientBehaviorIdentity;
        AmbientCenter = definition.AmbientCenter;
        AmbientRange = definition.AmbientRange;
        _blockingStages = Array.AsReadOnly(definition.BlockingStages.ToArray());
        SimulationStep = simulationStep;
    }

    public OriginalMapZoneEventIdentity EventIdentity { get; }

    public MapPosition PlayerSource { get; }

    public MapPosition CandidateTarget { get; }

    public int GateFlag { get; }

    public string BlockingSequenceIdentity { get; }

    public int LogicalActorId { get; }

    public OriginalMapEntityRecordIdentity ActorSourceRecord { get; }

    public MapPosition ActorInitialPosition { get; }

    public MapPosition ActorBlockingEndPosition { get; }

    public byte ActorBlockingEndOpaqueFacing { get; }

    public int OpaqueFaceWaitOperand { get; }

    public IReadOnlyList<int> TextIds => _textIds;

    public string AmbientBehaviorIdentity { get; }

    public MapPosition AmbientCenter { get; }

    public int AmbientRange { get; }

    public IReadOnlyList<OriginalMapZone601BlockingStage> BlockingStages => _blockingStages;

    public long SimulationStep { get; }

    internal bool Matches(OriginalMapZone601Definition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return EventIdentity == definition.Identity &&
            CandidateTarget == definition.Trigger &&
            GateFlag == definition.GateFlag &&
            string.Equals(
                BlockingSequenceIdentity,
                definition.BlockingSequenceIdentity,
                StringComparison.Ordinal) &&
            LogicalActorId == definition.LogicalActorId &&
            ActorSourceRecord == definition.ActorSourceRecord &&
            ActorInitialPosition == definition.ActorInitialPosition &&
            ActorBlockingEndPosition == definition.ActorBlockingEndPosition &&
            ActorBlockingEndOpaqueFacing == definition.ActorBlockingEndOpaqueFacing &&
            OpaqueFaceWaitOperand == definition.OpaqueFaceWaitOperand &&
            TextIds.SequenceEqual(definition.TextIds) &&
            string.Equals(
                AmbientBehaviorIdentity,
                definition.AmbientBehaviorIdentity,
                StringComparison.Ordinal) &&
            AmbientCenter == definition.AmbientCenter &&
            AmbientRange == definition.AmbientRange &&
            BlockingStages.SequenceEqual(definition.BlockingStages);
    }
}

public sealed partial class GameSession
{
    private bool TryApplyPrivateOriginalMapZone601(
        PrivateOriginalMapSessionSnapshot current,
        MoveExplorationCommand command,
        out PrivateOriginalMapMoveApplied? applied)
    {
        applied = null;
        OriginalMapZone601Definition? definition = current.Definition.Zone601;
        if (definition is null || current.Zone601?.Flag601Set != false)
        {
            return false;
        }

        MapPosition? candidate = current.Definition.Traversal.ResolveCandidateTarget(
            current.WorkingLayout,
            current.PlayerPosition,
            command.Direction);
        if (candidate != definition.Trigger)
        {
            return false;
        }

        OriginalMapTraversalResult traversal = current.Definition.Traversal.TryMove(
            current.WorkingLayout,
            current.PlayerPosition,
            command.Direction);
        if (traversal.Outcome != OriginalMapTraversalOutcome.Moved ||
            traversal.Position != definition.Trigger)
        {
            throw new InvalidOperationException(
                "The admitted Zone 601 candidate did not produce its exact traversal result.");
        }

        long nextStep = checked(current.SimulationStep + 1);
        PrivateOriginalMapZone601State state =
            PrivateOriginalMapZone601State.Complete(definition);
        PrivateOriginalMapZone601Receipt receipt = new(
            definition,
            current.PlayerPosition,
            candidate,
            nextStep);
        PrivateOriginalMapSessionSnapshot next = new(
            current.Definition,
            current.Receipt,
            current.WorkingLayout,
            nextStep,
            traversal.Position,
            traversal,
            current.ControlledStepCopyApplied,
            lastLayoutMutation: null,
            lastSameMapWarp: null,
            roofOnLoadLifecycle: current.RoofOnLoadLifecycle,
            lastRoofOnLoad: null,
            current.BowieDoorStepCopyApplied,
            lastNaturalStepCopy: null,
            current.SchoolDoorStepCopyApplied,
            state,
            receipt,
            current.Sarah,
            lastSarah: null,
            current.Entity142,
            pendingEntity142: null,
            lastEntity142Request: null,
            lastEntity142Acknowledgement: null,
            lastAstralZone: null,
            current.MessengerAcceptance,
            lastMessengerAcceptance: null,
            current.CastleGate,
            lastCastleGate: null);
        _privateOriginalMapSnapshot = next;
        applied = new PrivateOriginalMapMoveApplied(next, traversal, receipt);
        return true;
    }
}
