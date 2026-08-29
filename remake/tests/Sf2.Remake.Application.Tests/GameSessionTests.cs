using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class GameSessionTests
{
    [Fact]
    public void StartAndMoveProduceDeterministicExplorationSnapshot()
    {
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(
            GameSession.Start(CreateAcceptedSource(), Request()));

        GameSessionCommandApplied applied = Assert.IsType<GameSessionCommandApplied>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));

        Assert.Equal(ExplorationMovementOutcome.Moved, applied.Outcome);
        Assert.Equal(new MapPosition(2, 1), applied.Snapshot.Exploration.PlayerPosition);
        Assert.Equal(1, applied.Snapshot.SimulationStep);
        Assert.Equal(GameFlowStage.Exploration, applied.Snapshot.FlowStage);
        Assert.Equal((byte)3, applied.Snapshot.AdmissionFacts.OpaqueStartFacing);
        Assert.True(applied.Snapshot.AdmissionFacts.NoProgramRequest);
    }

    [Fact]
    public void BlockedMoveAdvancesLogicalStepWithoutChangingPosition()
    {
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(
            GameSession.Start(CreateAcceptedSource(), Request()));

        GameSessionCommandApplied applied = Assert.IsType<GameSessionCommandApplied>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.North)));

        Assert.Equal(ExplorationMovementOutcome.BlockedByTerrain, applied.Outcome);
        Assert.Equal(new MapPosition(1, 1), applied.Snapshot.Exploration.PlayerPosition);
        Assert.Equal(1, applied.Snapshot.SimulationStep);
    }

    [Fact]
    public void ContextCommandSelectsSetupAreaAndZoneAtCurrentSnapshotPosition()
    {
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(
            GameSession.Start(CreateAcceptedSource(), Request()));
        GameSessionCommandApplied moved = Assert.IsType<GameSessionCommandApplied>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));

        GameSessionContextSelected selected = Assert.IsType<GameSessionContextSelected>(
            started.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));

        Assert.Equal(moved.Snapshot.Exploration.PlayerPosition, selected.Selection.Position);
        Assert.Equal("synthetic-setup", selected.Selection.SelectedSetup.Value);
        Assert.Equal(AreaDescriptionSelectionKind.Text, selected.Selection.AreaDescription.Kind);
        Assert.Equal(424, selected.Selection.AreaDescription.InvestigationTextIndex);
        Assert.Equal(1002, selected.Selection.AreaDescription.DescriptionTextIndex);
        Assert.Equal("east-zone", selected.Selection.ZoneEvent.Target.Value);
        Assert.Equal(2, selected.Snapshot.SimulationStep);
        Assert.Same(selected.Selection, selected.Snapshot.ContextSelection);
    }

    [Fact]
    public void ContextCommandUsesDefaultZoneWithoutExecutingItsOpaqueTarget()
    {
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(
            GameSession.Start(CreateAcceptedSource(), Request()));

        GameSessionContextSelected selected = Assert.IsType<GameSessionContextSelected>(
            started.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));

        Assert.Equal(AreaDescriptionSelectionKind.NoMatch, selected.Selection.AreaDescription.Kind);
        Assert.Equal("no-zone", selected.Selection.ZoneEvent.Target.Value);
        Assert.Equal(1, selected.Snapshot.SimulationStep);
    }

    [Fact]
    public void MovementClearsAStaleContextSelection()
    {
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(
            GameSession.Start(CreateAcceptedSource(), Request()));
        GameSessionContextSelected selected = Assert.IsType<GameSessionContextSelected>(
            started.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));

        GameSessionCommandApplied moved = Assert.IsType<GameSessionCommandApplied>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));

        Assert.NotNull(selected.Snapshot.ContextSelection);
        Assert.Null(moved.Snapshot.ContextSelection);
    }

    [Fact]
    public void EventRequestRequiresASelectedAndAdmittedZoneTarget()
    {
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(
            GameSession.Start(CreateAcceptedSource(), Request()));
        GameSessionSnapshot initial = started.Session.Snapshot;

        GameSessionCommandRejected missingSelection = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(new RequestSelectedZoneEventCommand()));
        Assert.Equal(
            GameSessionCommandFailureCode.ContextSelectionRequired,
            missingSelection.Diagnostic.Code);
        Assert.Same(initial, started.Session.Snapshot);

        GameSessionContextSelected defaultSelection = Assert.IsType<GameSessionContextSelected>(
            started.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionCommandRejected defaultTarget = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(new RequestSelectedZoneEventCommand()));
        Assert.Equal(
            GameSessionCommandFailureCode.EventRequestNotAdmitted,
            defaultTarget.Diagnostic.Code);
        Assert.Same(defaultSelection.Snapshot, started.Session.Snapshot);
    }

    [Fact]
    public void EventRequestEmitsPendingCueAndRequiresExactAcknowledgement()
    {
        GameSessionStarted started = StartAtEastContext();

        GameSessionEventRequested requested = Assert.IsType<GameSessionEventRequested>(
            started.Session.Apply(new RequestSelectedZoneEventCommand()));

        Assert.Equal("east-zone-request", requested.Request.Request.Value);
        Assert.Equal("east-zone", requested.Request.Target.Value);
        Assert.Equal(new MapPosition(2, 1), requested.Request.Position);
        Assert.Equal(MapEventRequestStatus.Pending, requested.Request.Status);
        Assert.Equal(3, requested.Request.RequestedAtStep);
        Assert.Equal(1, requested.Request.CueSequence);
        Assert.Null(requested.Request.AcknowledgedAtStep);
        Assert.Equal("east-zone-selected", requested.Cue.Cue.Value);
        Assert.Equal(1, requested.Cue.Sequence);
        Assert.True(requested.Cue.RequiresAcknowledgement);
        Assert.Equal(1, requested.Snapshot.LastCueSequence);
        Assert.Same(requested.Request, requested.Snapshot.EventRequest);

        GameSessionCommandRejected blockedMove = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.West)));
        Assert.Equal(
            GameSessionCommandFailureCode.PendingAcknowledgement,
            blockedMove.Diagnostic.Code);
        Assert.Same(requested.Snapshot, started.Session.Snapshot);

        GameSessionCommandRejected wrongAcknowledgement =
            Assert.IsType<GameSessionCommandRejected>(
                started.Session.Apply(
                    new AcknowledgeMapEventRequestCommand(
                        new MapEventRequestId("wrong-request"),
                        requested.Cue.Sequence)));
        Assert.Equal(
            GameSessionCommandFailureCode.AcknowledgementMismatch,
            wrongAcknowledgement.Diagnostic.Code);
        Assert.Same(requested.Snapshot, started.Session.Snapshot);

        GameSessionCommandRejected wrongSequence = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(
                new AcknowledgeMapEventRequestCommand(
                    requested.Request.Request,
                    requested.Cue.Sequence + 1)));
        Assert.Equal(
            GameSessionCommandFailureCode.AcknowledgementMismatch,
            wrongSequence.Diagnostic.Code);
        Assert.Same(requested.Snapshot, started.Session.Snapshot);

        GameSessionEventRequestAcknowledged acknowledged =
            Assert.IsType<GameSessionEventRequestAcknowledged>(
                started.Session.Apply(
                    new AcknowledgeMapEventRequestCommand(
                        requested.Request.Request,
                        requested.Cue.Sequence)));
        Assert.Equal(MapEventRequestStatus.Acknowledged, acknowledged.Request.Status);
        Assert.Equal(4, acknowledged.Request.AcknowledgedAtStep);
        Assert.Equal(4, acknowledged.Snapshot.SimulationStep);
        Assert.Equal(1, acknowledged.Snapshot.LastCueSequence);

        GameSessionCommandRejected duplicateAcknowledgement =
            Assert.IsType<GameSessionCommandRejected>(
                started.Session.Apply(
                    new AcknowledgeMapEventRequestCommand(
                        requested.Request.Request,
                        requested.Cue.Sequence)));
        Assert.Equal(
            GameSessionCommandFailureCode.NoPendingAcknowledgement,
            duplicateAcknowledgement.Diagnostic.Code);
        Assert.Same(acknowledged.Snapshot, started.Session.Snapshot);
    }

    [Fact]
    public void AcknowledgedContextCanIssueAnotherMonotonicCue()
    {
        GameSessionStarted started = StartAtEastContext();
        GameSessionEventRequested first = Assert.IsType<GameSessionEventRequested>(
            started.Session.Apply(new RequestSelectedZoneEventCommand()));
        Assert.IsType<GameSessionEventRequestAcknowledged>(
            started.Session.Apply(
                new AcknowledgeMapEventRequestCommand(
                    first.Request.Request,
                    first.Cue.Sequence)));

        GameSessionEventRequested second = Assert.IsType<GameSessionEventRequested>(
            started.Session.Apply(new RequestSelectedZoneEventCommand()));

        Assert.Equal(2, second.Cue.Sequence);
        Assert.Equal(2, second.Snapshot.LastCueSequence);
        Assert.Equal(MapEventRequestStatus.Pending, second.Request.Status);
    }

    [Fact]
    public void EventRequestCatalogAndContextRejectDuplicateOrDanglingIdentities()
    {
        MapEventRequestDefinition first = RequestDefinition(
            "request-1",
            "east-zone",
            "cue-1");

        Assert.Throws<ArgumentException>(() => new MapEventRequestCatalog(
            [first, RequestDefinition("request-1", "other-zone", "cue-2")]));
        Assert.Throws<ArgumentException>(() => new MapEventRequestCatalog(
            [first, RequestDefinition("request-2", "east-zone", "cue-2")]));
        Assert.Throws<ArgumentException>(() => new MapEventRequestCatalog(
            [first, RequestDefinition("request-2", "other-zone", "cue-1")]));

        Assert.Throws<ArgumentException>(() => CreateMapContext(
            new MapId("map3"),
            requestTarget: "missing-zone"));
        Assert.Throws<ArgumentException>(() => CreateMapContext(
            new MapId("map3"),
            requestTarget: "no-zone"));
    }

    [Fact]
    public void RestartCreatesIndependentSessionFromImmutableDefinition()
    {
        IMapScenarioSource source = CreateAcceptedSource();
        GameSessionStarted first = Assert.IsType<GameSessionStarted>(
            GameSession.Start(source, Request()));
        GameSessionStarted restarted = Assert.IsType<GameSessionStarted>(
            GameSession.Start(source, Request()));

        first.Session.Apply(new MoveExplorationCommand(ExplorationDirection.East));
        first.Session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary));
        first.Session.Apply(new RequestSelectedZoneEventCommand());

        Assert.Equal(new MapPosition(2, 1), first.Session.Snapshot.Exploration.PlayerPosition);
        Assert.Equal(MapEventRequestStatus.Pending, first.Session.Snapshot.EventRequest?.Status);
        Assert.Equal(new MapPosition(1, 1), restarted.Session.Snapshot.Exploration.PlayerPosition);
        Assert.Equal(0, restarted.Session.Snapshot.SimulationStep);
        Assert.Equal(0, restarted.Session.Snapshot.LastCueSequence);
        Assert.Null(restarted.Session.Snapshot.EventRequest);
    }

    [Fact]
    public void UnknownCommandIsRejectedWithoutMutatingSnapshot()
    {
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(
            GameSession.Start(CreateAcceptedSource(), Request()));
        GameSessionSnapshot before = started.Session.Snapshot;

        GameSessionCommandRejected rejected = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(new UnknownCommand()));

        Assert.Equal(GameSessionCommandFailureCode.UnsupportedCommand, rejected.Diagnostic.Code);
        Assert.Same(before, rejected.Snapshot);
        Assert.Same(before, started.Session.Snapshot);
    }

    [Fact]
    public void SourceRejectionRemainsTypedAtApplicationBoundary()
    {
        ScenarioAdmissionDiagnostic diagnostic = new(
            ScenarioAdmissionFailureCode.InvalidMap,
            "map",
            "invalid synthetic map");

        GameSessionStartRejected rejected = Assert.IsType<GameSessionStartRejected>(
            GameSession.Start(new RejectedSource(diagnostic), Request()));

        Assert.Same(diagnostic, rejected.Diagnostic);
    }

    private static MapScenarioRequest Request() =>
        new("synthetic-package", ContentProfile.PublicSynthetic);

    private static IMapScenarioSource CreateAcceptedSource()
    {
        MapId map = new("map3");
        MapPosition start = new(1, 1);
        ExplorationMovementState exploration = new(
            map,
            new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]),
            new SyntheticWalkabilityGrid(
                3,
                3,
                [
                    false, false, false,
                    true, true, true,
                    true, true, true,
                ]),
            start);
        ScenarioAdmissionFacts facts = new(
            map,
            map,
            start,
            opaqueStartFacing: 3,
            "synthetic-setup",
            "synthetic-init",
            noProgramRequest: true,
            explorationReady: true);
        MapScenarioDefinition definition = new(
            "synthetic-scenario",
            "Synthetic scenario",
            exploration,
            facts,
            CreateMapContext(map));
        ScenarioAdmissionReceipt receipt = new(
            "synthetic-package",
            schemaVersion: 1,
            "digest",
            ContentProfile.PublicSynthetic,
            exactControlledAdmission: false,
            ["evidence-owner"],
            ["capability"]);
        return new AcceptedSource(definition, receipt);
    }

    private static GameSessionStarted StartAtEastContext()
    {
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(
            GameSession.Start(CreateAcceptedSource(), Request()));
        Assert.IsType<GameSessionCommandApplied>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));
        Assert.IsType<GameSessionContextSelected>(
            started.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        return started;
    }

    private static MapScenarioContextDefinition CreateMapContext(
        MapId map,
        string requestTarget = "east-zone")
    {
        MapSetupCatalog setupCatalog = new(
            [
                new MapSetupCatalogEntry(
                    map,
                    new MapSetupRoute(
                        new MapSetupId("synthetic-setup"),
                        [
                            new MapSetupFlagVariant(
                                new FlagId("alternate-enabled"),
                                new MapSetupId("alternate-setup")),
                        ])),
            ]);
        MapAreaDescriptionSource descriptions = MapAreaDescriptionSource.Table(
            descriptionTextBase: 1000,
            [
                new MapAreaDescriptionEntry(
                    x: 2,
                    y: 1,
                    AreaDescriptionCondition.Always,
                    AreaDescriptionPayload.Text(
                        investigationOffset: 1,
                        descriptionOffset: 2)),
            ]);
        MapSetupEventTable<ZoneEventRecord> zoneEvents = new(
            [
                ZoneEventRecord.Specific(
                    EventFieldMatch.Exact(2),
                    EventFieldMatch.Exact(1),
                    new EventTargetId("east-zone")),
                ZoneEventRecord.Default(new EventTargetId("no-zone")),
            ]);
        MapEventRequestCatalog eventRequests = new(
            [RequestDefinition("east-zone-request", requestTarget, "east-zone-selected")]);
        return new MapScenarioContextDefinition(
            setupCatalog,
            new MapSetupId("void-setup"),
            setFlags: [],
            descriptions,
            zoneEvents,
            eventRequests);
    }

    private static MapEventRequestDefinition RequestDefinition(
        string request,
        string target,
        string cue) =>
        new(
            new MapEventRequestId(request),
            new EventTargetId(target),
            new PresentationCueId(cue));

    private sealed record UnknownCommand : IGameSessionCommand;

    private sealed class AcceptedSource(
        MapScenarioDefinition definition,
        ScenarioAdmissionReceipt receipt) : IMapScenarioSource
    {
        public MapScenarioAdmissionResult Admit(MapScenarioRequest request) =>
            new MapScenarioAccepted(definition, receipt);
    }

    private sealed class RejectedSource(ScenarioAdmissionDiagnostic diagnostic) : IMapScenarioSource
    {
        public MapScenarioAdmissionResult Admit(MapScenarioRequest request) =>
            new MapScenarioRejected(diagnostic);
    }
}
