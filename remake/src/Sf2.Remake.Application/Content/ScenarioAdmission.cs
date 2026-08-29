using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public enum ContentProfile
{
    PublicSynthetic,
    PrivateLocal,
}

public sealed record MapScenarioRequest
{
    public MapScenarioRequest(string packageId, ContentProfile profile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile));
        }

        PackageId = packageId;
        Profile = profile;
    }

    public string PackageId { get; }

    public ContentProfile Profile { get; }
}

public sealed record ScenarioAdmissionFacts
{
    public ScenarioAdmissionFacts(
        MapId currentMap,
        MapId egressMap,
        MapPosition logicalStartPosition,
        byte opaqueStartFacing,
        string setupIdentity,
        string initIdentity,
        bool noProgramRequest,
        bool explorationReady)
    {
        CurrentMap = currentMap ?? throw new ArgumentNullException(nameof(currentMap));
        EgressMap = egressMap ?? throw new ArgumentNullException(nameof(egressMap));
        LogicalStartPosition = logicalStartPosition ??
            throw new ArgumentNullException(nameof(logicalStartPosition));
        ArgumentException.ThrowIfNullOrWhiteSpace(setupIdentity);
        ArgumentException.ThrowIfNullOrWhiteSpace(initIdentity);

        OpaqueStartFacing = opaqueStartFacing;
        SetupIdentity = setupIdentity;
        InitIdentity = initIdentity;
        NoProgramRequest = noProgramRequest;
        ExplorationReady = explorationReady;
    }

    public MapId CurrentMap { get; }

    public MapId EgressMap { get; }

    public MapPosition LogicalStartPosition { get; }

    public byte OpaqueStartFacing { get; }

    public string SetupIdentity { get; }

    public string InitIdentity { get; }

    public bool NoProgramRequest { get; }

    public bool ExplorationReady { get; }
}

public sealed record MapScenarioDefinition
{
    public MapScenarioDefinition(
        string scenarioId,
        string displayName,
        ExplorationMovementState startState,
        ScenarioAdmissionFacts admissionFacts,
        MapScenarioContextDefinition mapContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        StartState = startState ?? throw new ArgumentNullException(nameof(startState));
        AdmissionFacts = admissionFacts ?? throw new ArgumentNullException(nameof(admissionFacts));
        MapContext = mapContext ?? throw new ArgumentNullException(nameof(mapContext));
        if (startState.Map != admissionFacts.CurrentMap ||
            startState.PlayerPosition != admissionFacts.LogicalStartPosition)
        {
            throw new ArgumentException(
                "The exploration start must equal the admitted logical map position.",
                nameof(startState));
        }

        foreach (MapLocalTransitionDefinition transition in mapContext.LocalTransitions.Definitions)
        {
            if (transition.SourceMap != startState.Map ||
                transition.DestinationMap != startState.Map)
            {
                throw new ArgumentException(
                    $"Local transition '{transition.Transition}' must remain on the admitted scenario map.",
                    nameof(mapContext));
            }

            if (transition.SourcePosition == transition.DestinationPosition ||
                !startState.Walkability.IsPassable(transition.SourcePosition) ||
                !startState.Walkability.IsPassable(transition.DestinationPosition))
            {
                throw new ArgumentException(
                    $"Local transition '{transition.Transition}' requires distinct passable source and destination cells.",
                    nameof(mapContext));
            }
        }

        HashSet<MapPosition> transitionPositions = mapContext.LocalTransitions.Definitions
            .SelectMany(transition =>
                new[] { transition.SourcePosition, transition.DestinationPosition })
            .ToHashSet();
        foreach (MapEntityDefinition entity in mapContext.EntityInteractions.Entities)
        {
            if (entity.Map != startState.Map ||
                !startState.Walkability.Contains(entity.Position) ||
                startState.Walkability.IsPassable(entity.Position) ||
                entity.Position == startState.PlayerPosition ||
                transitionPositions.Contains(entity.Position))
            {
                throw new ArgumentException(
                    $"Entity '{entity.Entity}' requires one in-bounds synthetic solid cell that does not overlap admission or transition positions.",
                    nameof(mapContext));
            }
        }

        ScenarioId = scenarioId;
        DisplayName = displayName;
    }

    public string ScenarioId { get; }

    public string DisplayName { get; }

    public ExplorationMovementState StartState { get; }

    public ScenarioAdmissionFacts AdmissionFacts { get; }

    public MapScenarioContextDefinition MapContext { get; }
}

public sealed class ScenarioAdmissionReceipt
{
    private readonly ReadOnlyCollection<string> _capabilities;
    private readonly ReadOnlyCollection<string> _evidenceOwnerIds;

    public ScenarioAdmissionReceipt(
        string packageId,
        int schemaVersion,
        string contentDigest,
        ContentProfile profile,
        bool exactControlledAdmission,
        IEnumerable<string> evidenceOwnerIds,
        IEnumerable<string> capabilities)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentOutOfRangeException.ThrowIfLessThan(schemaVersion, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentDigest);
        if (!Enum.IsDefined(profile))
        {
            throw new ArgumentOutOfRangeException(nameof(profile));
        }

        ArgumentNullException.ThrowIfNull(evidenceOwnerIds);
        ArgumentNullException.ThrowIfNull(capabilities);
        List<string> copiedOwners = [];
        foreach (string owner in evidenceOwnerIds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(owner);
            copiedOwners.Add(owner);
        }

        List<string> copiedCapabilities = [];
        foreach (string capability in capabilities)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(capability);
            copiedCapabilities.Add(capability);
        }

        PackageId = packageId;
        SchemaVersion = schemaVersion;
        ContentDigest = contentDigest;
        Profile = profile;
        ExactControlledAdmission = exactControlledAdmission;
        _evidenceOwnerIds = copiedOwners.AsReadOnly();
        _capabilities = copiedCapabilities.AsReadOnly();
    }

    public string PackageId { get; }

    public int SchemaVersion { get; }

    public string ContentDigest { get; }

    public ContentProfile Profile { get; }

    public bool ExactControlledAdmission { get; }

    public IReadOnlyList<string> EvidenceOwnerIds => _evidenceOwnerIds;

    public IReadOnlyList<string> Capabilities => _capabilities;
}

public enum ScenarioAdmissionFailureCode
{
    PackageUnavailable,
    InvalidDocument,
    PackageIdentityMismatch,
    UnsupportedSchema,
    ProfileMismatch,
    ContentDigestMismatch,
    InvalidMap,
}

public sealed record ScenarioAdmissionDiagnostic
{
    public ScenarioAdmissionDiagnostic(
        ScenarioAdmissionFailureCode code,
        string field,
        string message)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Field = field;
        Message = message;
    }

    public ScenarioAdmissionFailureCode Code { get; }

    public string Field { get; }

    public string Message { get; }
}

public abstract record MapScenarioAdmissionResult;

public sealed record MapScenarioAccepted(
    MapScenarioDefinition Scenario,
    ScenarioAdmissionReceipt Receipt) : MapScenarioAdmissionResult
{
    public MapScenarioDefinition Scenario { get; } =
        Scenario ?? throw new ArgumentNullException(nameof(Scenario));

    public ScenarioAdmissionReceipt Receipt { get; } =
        Receipt ?? throw new ArgumentNullException(nameof(Receipt));
}

public sealed record MapScenarioRejected(
    ScenarioAdmissionDiagnostic Diagnostic) : MapScenarioAdmissionResult
{
    public ScenarioAdmissionDiagnostic Diagnostic { get; } =
        Diagnostic ?? throw new ArgumentNullException(nameof(Diagnostic));
}

public interface IMapScenarioSource
{
    MapScenarioAdmissionResult Admit(MapScenarioRequest request);
}
