using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record PublicSyntheticBattleRequestId
{
    public PublicSyntheticBattleRequestId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record PublicSyntheticBattleDefinition
{
    public PublicSyntheticBattleDefinition(
        PublicSyntheticBattleRequestId request,
        TacticalBattleRules rules,
        MapId sourceMap,
        MapPosition sourcePosition,
        MapSetupId sourceSetup,
        EventTargetId sourceZoneTarget,
        MapId returnMap,
        MapPosition returnPosition,
        MapSetupId returnSetup,
        SemanticFacing returnFacing,
        PresentationCueId requestCue,
        PresentationCueId admittedCue,
        PresentationCueId moveCue,
        PresentationCueId attackCue,
        PresentationCueId completedCue,
        PresentationCueId returnedCue)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
        SourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));
        SourcePosition = sourcePosition ?? throw new ArgumentNullException(nameof(sourcePosition));
        SourceSetup = sourceSetup ?? throw new ArgumentNullException(nameof(sourceSetup));
        SourceZoneTarget = sourceZoneTarget ??
            throw new ArgumentNullException(nameof(sourceZoneTarget));
        ReturnMap = returnMap ?? throw new ArgumentNullException(nameof(returnMap));
        ReturnPosition = returnPosition ?? throw new ArgumentNullException(nameof(returnPosition));
        ReturnSetup = returnSetup ?? throw new ArgumentNullException(nameof(returnSetup));
        if (!Enum.IsDefined(returnFacing))
        {
            throw new ArgumentOutOfRangeException(nameof(returnFacing));
        }

        PresentationCueId[] cues =
        [
            requestCue ?? throw new ArgumentNullException(nameof(requestCue)),
            admittedCue ?? throw new ArgumentNullException(nameof(admittedCue)),
            moveCue ?? throw new ArgumentNullException(nameof(moveCue)),
            attackCue ?? throw new ArgumentNullException(nameof(attackCue)),
            completedCue ?? throw new ArgumentNullException(nameof(completedCue)),
            returnedCue ?? throw new ArgumentNullException(nameof(returnedCue)),
        ];
        if (cues.Distinct().Count() != cues.Length)
        {
            throw new ArgumentException(
                "A public-synthetic battle requires six distinct presentation cues.",
                nameof(requestCue));
        }

        ReturnFacing = returnFacing;
        RequestCue = cues[0];
        AdmittedCue = cues[1];
        MoveCue = cues[2];
        AttackCue = cues[3];
        CompletedCue = cues[4];
        ReturnedCue = cues[5];
    }

    public PublicSyntheticBattleRequestId Request { get; }

    public TacticalBattleRules Rules { get; }

    public MapId SourceMap { get; }

    public MapPosition SourcePosition { get; }

    public MapSetupId SourceSetup { get; }

    public EventTargetId SourceZoneTarget { get; }

    public MapId ReturnMap { get; }

    public MapPosition ReturnPosition { get; }

    public MapSetupId ReturnSetup { get; }

    public SemanticFacing ReturnFacing { get; }

    public PresentationCueId RequestCue { get; }

    public PresentationCueId AdmittedCue { get; }

    public PresentationCueId MoveCue { get; }

    public PresentationCueId AttackCue { get; }

    public PresentationCueId CompletedCue { get; }

    public PresentationCueId ReturnedCue { get; }

    public IReadOnlyList<PresentationCueId> Cues =>
        [RequestCue, AdmittedCue, MoveCue, AttackCue, CompletedCue, ReturnedCue];
}

public sealed class PublicSyntheticBattleCatalog
{
    private readonly ReadOnlyCollection<PublicSyntheticBattleDefinition> _definitions;
    private readonly Dictionary<EventTargetId, PublicSyntheticBattleDefinition> _byTarget;
    private readonly Dictionary<PublicSyntheticBattleRequestId, PublicSyntheticBattleDefinition>
        _byRequest;
    private readonly Dictionary<TacticalBattleId, PublicSyntheticBattleDefinition> _byBattle;

    public PublicSyntheticBattleCatalog(
        IEnumerable<PublicSyntheticBattleDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        List<PublicSyntheticBattleDefinition> copied = [];
        _byTarget = [];
        _byRequest = [];
        _byBattle = [];
        foreach (PublicSyntheticBattleDefinition definition in definitions)
        {
            PublicSyntheticBattleDefinition admitted = definition ?? throw new ArgumentException(
                "Public-synthetic battle definitions cannot contain null values.",
                nameof(definitions));
            if (!_byTarget.TryAdd(admitted.SourceZoneTarget, admitted) ||
                !_byRequest.TryAdd(admitted.Request, admitted) ||
                !_byBattle.TryAdd(admitted.Rules.Battle, admitted))
            {
                throw new ArgumentException(
                    "Public-synthetic battle target, request, and battle IDs must be unique.",
                    nameof(definitions));
            }

            copied.Add(admitted);
        }

        _definitions = copied.AsReadOnly();
    }

    public IReadOnlyList<PublicSyntheticBattleDefinition> Definitions => _definitions;

    public PublicSyntheticBattleDefinition? FindByTarget(EventTargetId target)
    {
        ArgumentNullException.ThrowIfNull(target);
        _byTarget.TryGetValue(target, out PublicSyntheticBattleDefinition? definition);
        return definition;
    }

    public PublicSyntheticBattleDefinition? FindByRequest(PublicSyntheticBattleRequestId request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _byRequest.TryGetValue(request, out PublicSyntheticBattleDefinition? definition);
        return definition;
    }

    public PublicSyntheticBattleDefinition? FindByBattle(TacticalBattleId battle)
    {
        ArgumentNullException.ThrowIfNull(battle);
        _byBattle.TryGetValue(battle, out PublicSyntheticBattleDefinition? definition);
        return definition;
    }
}
