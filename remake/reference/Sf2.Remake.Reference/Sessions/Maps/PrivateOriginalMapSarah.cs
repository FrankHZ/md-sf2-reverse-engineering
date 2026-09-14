using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum PrivateOriginalMapSarahLifecyclePhase
{
    Ready,
    RouteCleared,
    AstralZoneRepositioned,
    MessengerFollowerReady,
}

public sealed record PrivateOriginalMapSarahState
{
    private PrivateOriginalMapSarahState(
        PrivateOriginalMapSarahLifecyclePhase phase,
        OriginalMapEntityRecordIdentity actorSourceRecord,
        int logicalActorId,
        MapPosition actorPosition,
        byte actorOpaqueFacing,
        bool temporaryRouteFlag256Set,
        bool astralZoneFlag260Set)
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

        if ((phase == PrivateOriginalMapSarahLifecyclePhase.Ready &&
                (temporaryRouteFlag256Set || astralZoneFlag260Set)) ||
            (phase == PrivateOriginalMapSarahLifecyclePhase.RouteCleared &&
                (!temporaryRouteFlag256Set || astralZoneFlag260Set)) ||
            ((phase == PrivateOriginalMapSarahLifecyclePhase.AstralZoneRepositioned ||
                    phase == PrivateOriginalMapSarahLifecyclePhase.MessengerFollowerReady) &&
                (!temporaryRouteFlag256Set || !astralZoneFlag260Set)))
        {
            throw new ArgumentException(
                "Sarah state must retain the exact ready, route-cleared, or Astral-zone shape.");
        }

        Phase = phase;
        LogicalActorId = logicalActorId;
        ActorOpaqueFacing = actorOpaqueFacing;
        TemporaryRouteFlag256Set = temporaryRouteFlag256Set;
        AstralZoneFlag260Set = astralZoneFlag260Set;
    }

    public PrivateOriginalMapSarahLifecyclePhase Phase { get; }

    public OriginalMapEntityRecordIdentity ActorSourceRecord { get; }

    public int LogicalActorId { get; }

    public MapPosition ActorPosition { get; }

    public byte ActorOpaqueFacing { get; }

    public bool TemporaryRouteFlag256Set { get; }

    public bool AstralZoneFlag260Set { get; }

    public bool IsMessengerFollowerReady =>
        Phase == PrivateOriginalMapSarahLifecyclePhase.MessengerFollowerReady;

    public bool OccupiesRouteTile => !IsMessengerFollowerReady;

    internal static PrivateOriginalMapSarahState Ready(OriginalMapSarahDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new(
            PrivateOriginalMapSarahLifecyclePhase.Ready,
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            definition.ActorInitialPosition,
            definition.ActorInitialOpaqueFacing,
            temporaryRouteFlag256Set: false,
            astralZoneFlag260Set: false);
    }

    internal static PrivateOriginalMapSarahState RouteCleared(
        OriginalMapSarahDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new(
            PrivateOriginalMapSarahLifecyclePhase.RouteCleared,
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            definition.FirstInteractionWaypoint,
            definition.RestoredOpaqueFacing,
            temporaryRouteFlag256Set: true,
            astralZoneFlag260Set: false);
    }

    internal static PrivateOriginalMapSarahState AstralZoneRepositioned(
        OriginalMapSarahDefinition definition,
        OriginalMapAstralZoneDefinition astralZone)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(astralZone);
        if (astralZone.SarahSourceRecord != definition.ActorSourceRecord ||
            astralZone.SarahLogicalActorId != definition.LogicalActorId)
        {
            throw new ArgumentException(
                "The Astral-zone handoff must bind the admitted Sarah actor.",
                nameof(astralZone));
        }

        return new(
            PrivateOriginalMapSarahLifecyclePhase.AstralZoneRepositioned,
            definition.ActorSourceRecord,
            definition.LogicalActorId,
            astralZone.SarahDestination,
            astralZone.SarahOpaqueFacing,
            temporaryRouteFlag256Set: true,
            astralZoneFlag260Set: true);
    }

    internal static PrivateOriginalMapSarahState MessengerFollowerReady(
        OriginalMapSarahDefinition definition,
        OriginalMapAstralZoneDefinition astralZone,
        OriginalMapMessengerAcceptanceDefinition messenger)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(astralZone);
        ArgumentNullException.ThrowIfNull(messenger);
        if (messenger.SarahSourceRecord != definition.ActorSourceRecord ||
            messenger.SarahCharacterId != definition.LogicalActorId ||
            !messenger.Followers.Any(link =>
                link.FollowerId == definition.LogicalActorId))
        {
            throw new ArgumentException(
                "Messenger acceptance must bind Sarah's admitted follower state.",
                nameof(messenger));
        }

        PrivateOriginalMapSarahState astral = AstralZoneRepositioned(definition, astralZone);
        return new(
            PrivateOriginalMapSarahLifecyclePhase.MessengerFollowerReady,
            astral.ActorSourceRecord,
            astral.LogicalActorId,
            astral.ActorPosition,
            astral.ActorOpaqueFacing,
            temporaryRouteFlag256Set: true,
            astralZoneFlag260Set: true);
    }

    internal bool Matches(
        OriginalMapSarahDefinition definition,
        OriginalMapAstralZoneDefinition? astralZone = null,
        OriginalMapMessengerAcceptanceDefinition? messenger = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return this == (Phase switch
        {
            PrivateOriginalMapSarahLifecyclePhase.Ready => Ready(definition),
            PrivateOriginalMapSarahLifecyclePhase.RouteCleared => RouteCleared(definition),
            PrivateOriginalMapSarahLifecyclePhase.AstralZoneRepositioned =>
                AstralZoneRepositioned(
                    definition,
                    astralZone ?? throw new InvalidOperationException(
                        "Astral-zone Sarah state requires its admitted definition.")),
            PrivateOriginalMapSarahLifecyclePhase.MessengerFollowerReady =>
                MessengerFollowerReady(
                    definition,
                    astralZone ?? throw new InvalidOperationException(
                        "Messenger follower state requires its admitted Astral definition."),
                    messenger ?? throw new InvalidOperationException(
                        "Messenger follower state requires its admitted definition.")),
            _ => throw new InvalidOperationException("Unknown Sarah lifecycle phase."),
        });
    }
}

public sealed record InteractPrivateOriginalMapSarahCommand(long ExpectedSimulationStep)
{
    public long ExpectedSimulationStep { get; } =
        ExpectedSimulationStep >= 0
            ? ExpectedSimulationStep
            : throw new ArgumentOutOfRangeException(nameof(ExpectedSimulationStep));
}

public sealed record PrivateOriginalMapSarahReceipt
{
    private readonly ReadOnlyCollection<int> _textIds;
    private readonly ReadOnlyCollection<OriginalMapSarahInteractionStage> _stages;

    internal PrivateOriginalMapSarahReceipt(
        OriginalMapSarahDefinition definition,
        MapPosition playerPosition,
        byte playerOpaqueFacing,
        PrivateOriginalMapSarahState before,
        PrivateOriginalMapSarahState after,
        bool repeated,
        long simulationStep)
    {
        ArgumentNullException.ThrowIfNull(definition);
        PlayerPosition = playerPosition ?? throw new ArgumentNullException(nameof(playerPosition));
        Before = before ?? throw new ArgumentNullException(nameof(before));
        After = after ?? throw new ArgumentNullException(nameof(after));
        if (playerOpaqueFacing > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(playerOpaqueFacing));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(simulationStep, 1);
        EventIdentity = definition.Identity;
        PlayerOpaqueFacing = playerOpaqueFacing;
        Repeated = repeated;
        LaterBranchFlag603 = definition.LaterBranchFlag603;
        LaterBranchFlag602 = definition.LaterBranchFlag602;
        TemporaryRouteFlag256 = definition.TemporaryRouteFlag256;
        BlockingSequenceIdentity = definition.BlockingSequenceIdentity;
        _textIds = Array.AsReadOnly((repeated
            ? definition.RepeatInteractionTextIds
            : definition.FirstInteractionTextIds).ToArray());
        _stages = Array.AsReadOnly((repeated
            ? definition.RepeatInteractionStages
            : definition.FirstInteractionStages).ToArray());
        SimulationStep = simulationStep;
    }

    public OriginalMapSarahEventIdentity EventIdentity { get; }

    public MapPosition PlayerPosition { get; }

    public byte PlayerOpaqueFacing { get; }

    public PrivateOriginalMapSarahState Before { get; }

    public PrivateOriginalMapSarahState After { get; }

    public bool Repeated { get; }

    public int LaterBranchFlag603 { get; }

    public bool LaterBranchFlag603Set => false;

    public int LaterBranchFlag602 { get; }

    public bool LaterBranchFlag602Set => false;

    public int TemporaryRouteFlag256 { get; }

    public string BlockingSequenceIdentity { get; }

    public IReadOnlyList<int> TextIds => _textIds;

    public IReadOnlyList<OriginalMapSarahInteractionStage> Stages => _stages;

    public long SimulationStep { get; }

    internal bool Matches(OriginalMapSarahDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return EventIdentity == definition.Identity &&
            Before.Matches(definition) &&
            After.Matches(definition) &&
            LaterBranchFlag603 == definition.LaterBranchFlag603 &&
            LaterBranchFlag602 == definition.LaterBranchFlag602 &&
            TemporaryRouteFlag256 == definition.TemporaryRouteFlag256 &&
            string.Equals(
                BlockingSequenceIdentity,
                definition.BlockingSequenceIdentity,
                StringComparison.Ordinal) &&
            TextIds.SequenceEqual(Repeated
                ? definition.RepeatInteractionTextIds
                : definition.FirstInteractionTextIds) &&
            Stages.SequenceEqual(Repeated
                ? definition.RepeatInteractionStages
                : definition.FirstInteractionStages) &&
            Repeated == (Before.Phase == PrivateOriginalMapSarahLifecyclePhase.RouteCleared) &&
            (Repeated ? ReferenceEquals(Before, After) :
                After == PrivateOriginalMapSarahState.RouteCleared(definition));
    }
}

public enum PrivateOriginalMapSarahInteractionFailureCode
{
    StaleSimulationStep,
    LocomotionBusy,
    BattleBridgeBusy,
    InteractionTargetMismatch,
    UnsupportedLaterBranchState,
}

public sealed record PrivateOriginalMapSarahInteractionDiagnostic(
    PrivateOriginalMapSarahInteractionFailureCode Code,
    string Message)
{
    public string Message { get; } = !string.IsNullOrWhiteSpace(Message)
        ? Message
        : throw new ArgumentException("A Sarah interaction diagnostic requires a message.", nameof(Message));
}

public abstract record PrivateOriginalMapSarahInteractionResult;

public sealed record PrivateOriginalMapSarahInteractionApplied :
    PrivateOriginalMapSarahInteractionResult
{
    internal PrivateOriginalMapSarahInteractionApplied(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapSarahReceipt receipt)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Receipt = receipt ?? throw new ArgumentNullException(nameof(receipt));
        if (!ReferenceEquals(snapshot.LastSarah, receipt))
        {
            throw new ArgumentException(
                "A Sarah interaction result must expose the snapshot's exact receipt.",
                nameof(receipt));
        }
    }

    public PrivateOriginalMapSessionSnapshot Snapshot { get; }

    public PrivateOriginalMapSarahReceipt Receipt { get; }
}

public sealed record PrivateOriginalMapSarahInteractionRejected :
    PrivateOriginalMapSarahInteractionResult
{
    internal PrivateOriginalMapSarahInteractionRejected(
        PrivateOriginalMapSessionSnapshot snapshot,
        PrivateOriginalMapSarahInteractionDiagnostic diagnostic)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Diagnostic = diagnostic ?? throw new ArgumentNullException(nameof(diagnostic));
    }

    public PrivateOriginalMapSessionSnapshot Snapshot { get; }

    public PrivateOriginalMapSarahInteractionDiagnostic Diagnostic { get; }
}
