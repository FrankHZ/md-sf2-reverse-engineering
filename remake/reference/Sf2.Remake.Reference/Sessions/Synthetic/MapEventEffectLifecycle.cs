using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Sessions;

public sealed class PublicSyntheticFlagStateSnapshot
{
    private readonly ReadOnlyCollection<FlagId> _setFlags;
    private readonly HashSet<FlagId> _setFlagLookup;

    public PublicSyntheticFlagStateSnapshot(IEnumerable<FlagId> setFlags)
    {
        ArgumentNullException.ThrowIfNull(setFlags);

        List<FlagId> copiedFlags = [];
        _setFlagLookup = [];
        foreach (FlagId flag in setFlags)
        {
            FlagId admitted = flag ?? throw new ArgumentException(
                "Synthetic flag state cannot contain null values.",
                nameof(setFlags));
            if (!_setFlagLookup.Add(admitted))
            {
                throw new ArgumentException(
                    $"Duplicate synthetic flag '{admitted}'.",
                    nameof(setFlags));
            }

            copiedFlags.Add(admitted);
        }

        _setFlags = copiedFlags.AsReadOnly();
    }

    public IReadOnlyList<FlagId> SetFlags => _setFlags;

    public bool IsSet(FlagId flag)
    {
        ArgumentNullException.ThrowIfNull(flag);
        return _setFlagLookup.Contains(flag);
    }

    internal PublicSyntheticFlagStateSnapshot SetOnce(FlagId flag)
    {
        ArgumentNullException.ThrowIfNull(flag);
        if (_setFlagLookup.Contains(flag))
        {
            throw new InvalidOperationException(
                $"Synthetic flag '{flag}' has already been applied.");
        }

        return new PublicSyntheticFlagStateSnapshot([.. _setFlags, flag]);
    }
}

public sealed record MapEventEffectSnapshot
{
    internal MapEventEffectSnapshot(
        MapEventEffectDefinition definition,
        long requestCueSequence,
        long appliedAtStep,
        long cueSequence)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentOutOfRangeException.ThrowIfLessThan(requestCueSequence, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(appliedAtStep, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(cueSequence, 1);
        if (cueSequence <= requestCueSequence)
        {
            throw new ArgumentException(
                "An event-effect cue must follow its request cue.",
                nameof(cueSequence));
        }

        Effect = definition.Effect;
        Request = definition.Request;
        Flag = definition.Flag;
        RequestCueSequence = requestCueSequence;
        AppliedAtStep = appliedAtStep;
        CueSequence = cueSequence;
    }

    public MapEventEffectId Effect { get; }

    public MapEventRequestId Request { get; }

    public FlagId Flag { get; }

    public long RequestCueSequence { get; }

    public long AppliedAtStep { get; }

    public long CueSequence { get; }
}

public sealed record MapEventEffectCue
{
    public MapEventEffectCue(
        PresentationCueId cue,
        MapEventEffectId effect,
        FlagId flag,
        long sequence)
    {
        Cue = cue ?? throw new ArgumentNullException(nameof(cue));
        Effect = effect ?? throw new ArgumentNullException(nameof(effect));
        Flag = flag ?? throw new ArgumentNullException(nameof(flag));
        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        Sequence = sequence;
    }

    public PresentationCueId Cue { get; }

    public MapEventEffectId Effect { get; }

    public FlagId Flag { get; }

    public long Sequence { get; }

    public bool RequiresAcknowledgement => false;
}

public sealed record GameSessionEventEffectApplied(
    GameSessionSnapshot Snapshot,
    MapEventRequestSnapshot Request,
    MapEventEffectSnapshot Effect,
    MapEventEffectCue Cue) : GameSessionCommandResult
{
    public GameSessionSnapshot Snapshot { get; } =
        Snapshot ?? throw new ArgumentNullException(nameof(Snapshot));

    public MapEventRequestSnapshot Request { get; } =
        Request ?? throw new ArgumentNullException(nameof(Request));

    public MapEventEffectSnapshot Effect { get; } =
        Effect ?? throw new ArgumentNullException(nameof(Effect));

    public MapEventEffectCue Cue { get; } =
        Cue ?? throw new ArgumentNullException(nameof(Cue));
}
