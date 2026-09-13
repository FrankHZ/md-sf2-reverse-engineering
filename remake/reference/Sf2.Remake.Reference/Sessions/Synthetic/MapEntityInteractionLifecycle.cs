using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public enum MapEntityInteractionStatus
{
    Pending,
    Acknowledged,
}

public sealed record TurnExplorationCommand : IGameSessionCommand
{
    public TurnExplorationCommand(SemanticFacing facing)
    {
        if (!Enum.IsDefined(facing))
        {
            throw new ArgumentOutOfRangeException(nameof(facing));
        }

        Facing = facing;
    }

    public SemanticFacing Facing { get; }
}

public sealed record RequestEntityInteractionCommand : IGameSessionCommand;

public sealed record AcknowledgeEntityInteractionCommand : IGameSessionCommand
{
    public AcknowledgeEntityInteractionCommand(
        MapEntityInteractionRequestId request,
        long cueSequence,
        MapEntityId entity,
        MapEntityInteractionTargetId target)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        CueSequence = cueSequence;
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public MapEntityInteractionRequestId Request { get; }

    public long CueSequence { get; }

    public MapEntityId Entity { get; }

    public MapEntityInteractionTargetId Target { get; }
}

public sealed record MapEntityInteractionSnapshot
{
    private MapEntityInteractionSnapshot(
        MapEntityInteractionRequestId request,
        MapEntityId entity,
        MapEntityInteractionTargetId target,
        MapId map,
        MapPosition entityPosition,
        MapPosition playerPosition,
        SemanticFacing facing,
        PresentationCueId cue,
        MapEntityInteractionStatus status,
        long requestedAtStep,
        long cueSequence,
        long? acknowledgedAtStep)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Map = map ?? throw new ArgumentNullException(nameof(map));
        EntityPosition = entityPosition ?? throw new ArgumentNullException(nameof(entityPosition));
        PlayerPosition = playerPosition ?? throw new ArgumentNullException(nameof(playerPosition));
        if (!Enum.IsDefined(facing))
        {
            throw new ArgumentOutOfRangeException(nameof(facing));
        }

        Facing = facing;
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(requestedAtStep, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        if (status == MapEntityInteractionStatus.Pending && acknowledgedAtStep is not null)
        {
            throw new ArgumentException(
                "A pending entity interaction cannot have an acknowledgement step.",
                nameof(acknowledgedAtStep));
        }

        if (status == MapEntityInteractionStatus.Acknowledged &&
            (acknowledgedAtStep is null || acknowledgedAtStep <= requestedAtStep))
        {
            throw new ArgumentException(
                "An acknowledged entity interaction requires a later acknowledgement step.",
                nameof(acknowledgedAtStep));
        }

        Status = status;
        RequestedAtStep = requestedAtStep;
        CueSequence = cueSequence;
        AcknowledgedAtStep = acknowledgedAtStep;
    }

    public MapEntityInteractionRequestId Request { get; }

    public MapEntityId Entity { get; }

    public MapEntityInteractionTargetId Target { get; }

    public MapId Map { get; }

    public MapPosition EntityPosition { get; }

    public MapPosition PlayerPosition { get; }

    public SemanticFacing Facing { get; }

    public PresentationCueId Cue { get; }

    public MapEntityInteractionStatus Status { get; }

    public long RequestedAtStep { get; }

    public long CueSequence { get; }

    public long? AcknowledgedAtStep { get; }

    internal static MapEntityInteractionSnapshot Pending(
        MapEntityDefinition entity,
        MapEntityInteractionDefinition interaction,
        MapPosition playerPosition,
        SemanticFacing facing,
        long requestedAtStep,
        long cueSequence) =>
        new(
            interaction.Request,
            entity.Entity,
            interaction.Target,
            entity.Map,
            entity.Position,
            playerPosition,
            facing,
            interaction.Cue,
            MapEntityInteractionStatus.Pending,
            requestedAtStep,
            cueSequence,
            acknowledgedAtStep: null);

    internal MapEntityInteractionSnapshot Acknowledge(long acknowledgedAtStep) =>
        new(
            Request,
            Entity,
            Target,
            Map,
            EntityPosition,
            PlayerPosition,
            Facing,
            Cue,
            MapEntityInteractionStatus.Acknowledged,
            RequestedAtStep,
            CueSequence,
            acknowledgedAtStep);
}

public sealed record MapEntityInteractionCue
{
    public MapEntityInteractionCue(
        PresentationCueId cue,
        MapEntityInteractionRequestId request,
        MapEntityId entity,
        MapEntityInteractionTargetId target,
        MapPosition entityPosition,
        long sequence)
    {
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        EntityPosition = entityPosition ?? throw new ArgumentNullException(nameof(entityPosition));
        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        Sequence = sequence;
    }

    public PresentationCueId Cue { get; }

    public MapEntityInteractionRequestId Request { get; }

    public MapEntityId Entity { get; }

    public MapEntityInteractionTargetId Target { get; }

    public MapPosition EntityPosition { get; }

    public long Sequence { get; }

    public bool RequiresAcknowledgement => true;
}

public sealed record GameSessionFacingChanged(
    GameSessionSnapshot Snapshot,
    SemanticFacing Facing) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));
}

public sealed record GameSessionEntityInteractionRequested(
    GameSessionSnapshot Snapshot,
    MapEntityInteractionSnapshot Interaction,
    MapEntityInteractionCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapEntityInteractionSnapshot Interaction { get; } =
        Interaction ?? throw new ArgumentNullException(nameof(Interaction));

    public MapEntityInteractionCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}

public sealed record GameSessionEntityInteractionAcknowledged(
    GameSessionSnapshot Snapshot,
    MapEntityInteractionSnapshot Interaction,
    MapDialogueSnapshot Dialogue,
    MapDialogueCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapEntityInteractionSnapshot Interaction { get; } =
        Interaction ?? throw new ArgumentNullException(nameof(Interaction));

    public MapDialogueSnapshot Dialogue { get; } =
        Dialogue ?? throw new ArgumentNullException(nameof(Dialogue));

    public MapDialogueCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}
