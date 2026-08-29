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
        Assert.Equal("east-zone-variant-effect", requested.Request.ExpectedEffect.Value);
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
                        requested.Cue.Sequence,
                        requested.Request.ExpectedEffect)));
        Assert.Equal(
            GameSessionCommandFailureCode.AcknowledgementMismatch,
            wrongAcknowledgement.Diagnostic.Code);
        Assert.Same(requested.Snapshot, started.Session.Snapshot);

        GameSessionCommandRejected wrongSequence = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(
                new AcknowledgeMapEventRequestCommand(
                    requested.Request.Request,
                    requested.Cue.Sequence + 1,
                    requested.Request.ExpectedEffect)));
        Assert.Equal(
            GameSessionCommandFailureCode.AcknowledgementMismatch,
            wrongSequence.Diagnostic.Code);
        Assert.Same(requested.Snapshot, started.Session.Snapshot);

        GameSessionCommandRejected wrongEffect = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(
                new AcknowledgeMapEventRequestCommand(
                    requested.Request.Request,
                    requested.Cue.Sequence,
                    new MapEventEffectId("wrong-effect"))));
        Assert.Equal(
            GameSessionCommandFailureCode.AcknowledgementMismatch,
            wrongEffect.Diagnostic.Code);
        Assert.Same(requested.Snapshot, started.Session.Snapshot);

        GameSessionEventEffectApplied acknowledged =
            Assert.IsType<GameSessionEventEffectApplied>(
                started.Session.Apply(
                    new AcknowledgeMapEventRequestCommand(
                        requested.Request.Request,
                        requested.Cue.Sequence,
                        requested.Request.ExpectedEffect)));
        Assert.Equal(MapEventRequestStatus.Acknowledged, acknowledged.Request.Status);
        Assert.Equal(4, acknowledged.Request.AcknowledgedAtStep);
        Assert.Equal(4, acknowledged.Snapshot.SimulationStep);
        Assert.Equal(2, acknowledged.Snapshot.LastCueSequence);
        Assert.Null(acknowledged.Snapshot.ContextSelection);
        Assert.Equal("east-zone-variant-effect", acknowledged.Effect.Effect.Value);
        Assert.Equal("east-zone-request", acknowledged.Effect.Request.Value);
        Assert.Equal("alternate-enabled", acknowledged.Effect.Flag.Value);
        Assert.Equal(1, acknowledged.Effect.RequestCueSequence);
        Assert.Equal(4, acknowledged.Effect.AppliedAtStep);
        Assert.Equal(2, acknowledged.Effect.CueSequence);
        Assert.Equal("variant-applied", acknowledged.Cue.Cue.Value);
        Assert.Equal(2, acknowledged.Cue.Sequence);
        Assert.False(acknowledged.Cue.RequiresAcknowledgement);
        Assert.True(acknowledged.Snapshot.SyntheticFlags.IsSet(acknowledged.Effect.Flag));
        Assert.Same(acknowledged.Effect, acknowledged.Snapshot.LastEventEffect);

        GameSessionCommandRejected duplicateAcknowledgement =
            Assert.IsType<GameSessionCommandRejected>(
                started.Session.Apply(
                    new AcknowledgeMapEventRequestCommand(
                        requested.Request.Request,
                        requested.Cue.Sequence,
                        requested.Request.ExpectedEffect)));
        Assert.Equal(
            GameSessionCommandFailureCode.NoPendingAcknowledgement,
            duplicateAcknowledgement.Diagnostic.Code);
        Assert.Same(acknowledged.Snapshot, started.Session.Snapshot);
    }

    [Fact]
    public void AppliedEffectRequiresFreshContextAndSelectsVariantExactlyOnce()
    {
        GameSessionStarted started = StartAtEastContext();
        GameSessionEventRequested requested = Assert.IsType<GameSessionEventRequested>(
            started.Session.Apply(new RequestSelectedZoneEventCommand()));
        GameSessionEventEffectApplied applied = Assert.IsType<GameSessionEventEffectApplied>(
            started.Session.Apply(
                new AcknowledgeMapEventRequestCommand(
                    requested.Request.Request,
                    requested.Cue.Sequence,
                    requested.Request.ExpectedEffect)));

        GameSessionCommandRejected staleContext = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(new RequestSelectedZoneEventCommand()));
        Assert.Equal(
            GameSessionCommandFailureCode.ContextSelectionRequired,
            staleContext.Diagnostic.Code);
        Assert.Same(applied.Snapshot, started.Session.Snapshot);

        GameSessionContextSelected reselected = Assert.IsType<GameSessionContextSelected>(
            started.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        Assert.Equal("alternate-setup", reselected.Selection.SelectedSetup.Value);

        GameSessionCommandRejected duplicateEffect = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(new RequestSelectedZoneEventCommand()));
        Assert.Equal(
            GameSessionCommandFailureCode.EventEffectAlreadyApplied,
            duplicateEffect.Diagnostic.Code);
        Assert.Same(reselected.Snapshot, started.Session.Snapshot);
        Assert.Equal(2, started.Session.Snapshot.LastCueSequence);
    }

    [Fact]
    public void LocalTransitionRequiresFreshSelectedAndAdmittedSourceContext()
    {
        GameSessionStarted started = Assert.IsType<GameSessionStarted>(
            GameSession.Start(CreateAcceptedSource(), Request()));
        GameSessionSnapshot initial = started.Session.Snapshot;

        GameSessionCommandRejected missingSelection = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(new RequestSelectedLocalTransitionCommand()));
        Assert.Equal(
            GameSessionCommandFailureCode.ContextSelectionRequired,
            missingSelection.Diagnostic.Code);
        Assert.Same(initial, started.Session.Snapshot);

        Assert.IsType<GameSessionCommandApplied>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));
        GameSessionContextSelected eventContext = Assert.IsType<GameSessionContextSelected>(
            started.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionCommandRejected wrongTarget = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(new RequestSelectedLocalTransitionCommand()));
        Assert.Equal(
            GameSessionCommandFailureCode.LocalTransitionNotAdmitted,
            wrongTarget.Diagnostic.Code);
        Assert.Same(eventContext.Snapshot, started.Session.Snapshot);

        Assert.IsType<GameSessionCommandApplied>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.West)));
        Assert.IsType<GameSessionCommandApplied>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.South)));
        GameSessionContextSelected transitionContext = Assert.IsType<GameSessionContextSelected>(
            started.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        Assert.Equal("local-transition-zone", transitionContext.Selection.ZoneEvent.Target.Value);
        GameSessionCommandRejected wrongSetup = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(new RequestSelectedLocalTransitionCommand()));
        Assert.Equal(
            GameSessionCommandFailureCode.LocalTransitionNotAdmitted,
            wrongSetup.Diagnostic.Code);
        Assert.Same(transitionContext.Snapshot, started.Session.Snapshot);
        Assert.IsType<GameSessionCommandApplied>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));

        GameSessionCommandRejected staleContext = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(new RequestSelectedLocalTransitionCommand()));
        Assert.Equal(
            GameSessionCommandFailureCode.ContextSelectionRequired,
            staleContext.Diagnostic.Code);
        Assert.Same(started.Session.Snapshot, staleContext.Snapshot);
    }

    [Fact]
    public void LocalTransitionAcknowledgementRelocatesAtomicallyAndClearsStaleLifecycleState()
    {
        GameSessionStarted started = StartAtEastContext();
        GameSessionEventRequested eventRequest = Assert.IsType<GameSessionEventRequested>(
            started.Session.Apply(new RequestSelectedZoneEventCommand()));
        Assert.IsType<GameSessionEventEffectApplied>(
            started.Session.Apply(
                new AcknowledgeMapEventRequestCommand(
                    eventRequest.Request.Request,
                    eventRequest.Cue.Sequence,
                    eventRequest.Request.ExpectedEffect)));
        Assert.IsType<GameSessionCommandApplied>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.West)));
        Assert.IsType<GameSessionCommandApplied>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.South)));
        GameSessionContextSelected selected = Assert.IsType<GameSessionContextSelected>(
            started.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));

        GameSessionLocalTransitionRequested requested =
            Assert.IsType<GameSessionLocalTransitionRequested>(
                started.Session.Apply(new RequestSelectedLocalTransitionCommand()));

        Assert.Equal("local-transition-request", requested.Transition.Request.Value);
        Assert.Equal("local-transition", requested.Transition.Transition.Value);
        Assert.Equal(selected.Selection.ZoneEvent.Target, requested.Transition.Target);
        Assert.Equal(new MapPosition(1, 2), requested.Transition.SourcePosition);
        Assert.Equal("alternate-setup", requested.Transition.SourceSetup.Value);
        Assert.Equal(new MapPosition(2, 2), requested.Transition.DestinationPosition);
        Assert.Equal("synthetic-arrival-east", requested.Transition.DestinationOrientation.Value);
        Assert.Equal(MapLocalTransitionStatus.Pending, requested.Transition.Status);
        Assert.Equal(8, requested.Transition.RequestedAtStep);
        Assert.Equal(3, requested.Transition.CueSequence);
        Assert.Equal("local-transition-ready", requested.Cue.Cue.Value);
        Assert.True(requested.Cue.RequiresAcknowledgement);
        Assert.Same(requested.Transition, requested.Snapshot.LocalTransition);

        GameSessionCommandRejected blockedMove = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));
        Assert.Equal(
            GameSessionCommandFailureCode.PendingAcknowledgement,
            blockedMove.Diagnostic.Code);
        Assert.Same(requested.Snapshot, started.Session.Snapshot);

        GameSessionCommandRejected wrongRequest = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(
                new AcknowledgeMapLocalTransitionCommand(
                    new MapLocalTransitionRequestId("wrong-request"),
                    requested.Cue.Sequence,
                    requested.Transition.Transition)));
        Assert.Equal(
            GameSessionCommandFailureCode.AcknowledgementMismatch,
            wrongRequest.Diagnostic.Code);
        Assert.Same(requested.Snapshot, started.Session.Snapshot);

        GameSessionCommandRejected wrongSequence = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(
                new AcknowledgeMapLocalTransitionCommand(
                    requested.Transition.Request,
                    requested.Cue.Sequence + 1,
                    requested.Transition.Transition)));
        Assert.Equal(
            GameSessionCommandFailureCode.AcknowledgementMismatch,
            wrongSequence.Diagnostic.Code);
        Assert.Same(requested.Snapshot, started.Session.Snapshot);

        GameSessionCommandRejected wrongTransition = Assert.IsType<GameSessionCommandRejected>(
            started.Session.Apply(
                new AcknowledgeMapLocalTransitionCommand(
                    requested.Transition.Request,
                    requested.Cue.Sequence,
                    new MapLocalTransitionId("wrong-transition"))));
        Assert.Equal(
            GameSessionCommandFailureCode.AcknowledgementMismatch,
            wrongTransition.Diagnostic.Code);
        Assert.Same(requested.Snapshot, started.Session.Snapshot);

        GameSessionLocalTransitionApplied applied =
            Assert.IsType<GameSessionLocalTransitionApplied>(
                started.Session.Apply(
                    new AcknowledgeMapLocalTransitionCommand(
                        requested.Transition.Request,
                        requested.Cue.Sequence,
                        requested.Transition.Transition)));

        Assert.Equal(MapLocalTransitionStatus.Acknowledged, applied.Transition.Status);
        Assert.Equal(9, applied.Transition.AcknowledgedAtStep);
        Assert.Equal(9, applied.Snapshot.SimulationStep);
        Assert.Equal(3, applied.Snapshot.LastCueSequence);
        Assert.Equal(new MapPosition(2, 2), applied.Snapshot.Exploration.PlayerPosition);
        Assert.Equal("map3", applied.Snapshot.Exploration.Map.Value);
        Assert.Null(applied.Snapshot.ContextSelection);
        Assert.Null(applied.Snapshot.EventRequest);
        Assert.Null(applied.Snapshot.LastEventEffect);
        Assert.True(applied.Snapshot.SyntheticFlags.IsSet(new FlagId("alternate-enabled")));
        Assert.Same(applied.Transition, applied.Snapshot.LocalTransition);

        GameSessionCommandRejected duplicateAcknowledgement =
            Assert.IsType<GameSessionCommandRejected>(
                started.Session.Apply(
                    new AcknowledgeMapLocalTransitionCommand(
                        requested.Transition.Request,
                        requested.Cue.Sequence,
                        requested.Transition.Transition)));
        Assert.Equal(
            GameSessionCommandFailureCode.NoPendingAcknowledgement,
            duplicateAcknowledgement.Diagnostic.Code);
        Assert.Same(applied.Snapshot, started.Session.Snapshot);
    }

    [Fact]
    public void LocalTransitionStateIsIsolatedAcrossRestart()
    {
        IMapScenarioSource source = CreateAcceptedSource();
        GameSessionStarted first = Assert.IsType<GameSessionStarted>(
            GameSession.Start(source, Request()));
        GameSessionStarted restarted = Assert.IsType<GameSessionStarted>(
            GameSession.Start(source, Request()));
        Assert.IsType<GameSessionCommandApplied>(
            first.Session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));
        Assert.IsType<GameSessionContextSelected>(
            first.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionEventRequested eventRequest = Assert.IsType<GameSessionEventRequested>(
            first.Session.Apply(new RequestSelectedZoneEventCommand()));
        Assert.IsType<GameSessionEventEffectApplied>(
            first.Session.Apply(
                new AcknowledgeMapEventRequestCommand(
                    eventRequest.Request.Request,
                    eventRequest.Cue.Sequence,
                    eventRequest.Request.ExpectedEffect)));
        Assert.IsType<GameSessionCommandApplied>(
            first.Session.Apply(new MoveExplorationCommand(ExplorationDirection.West)));
        Assert.IsType<GameSessionCommandApplied>(
            first.Session.Apply(new MoveExplorationCommand(ExplorationDirection.South)));
        Assert.IsType<GameSessionContextSelected>(
            first.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        GameSessionLocalTransitionRequested requested =
            Assert.IsType<GameSessionLocalTransitionRequested>(
                first.Session.Apply(new RequestSelectedLocalTransitionCommand()));
        Assert.IsType<GameSessionLocalTransitionApplied>(
            first.Session.Apply(
                new AcknowledgeMapLocalTransitionCommand(
                    requested.Transition.Request,
                    requested.Cue.Sequence,
                    requested.Transition.Transition)));

        Assert.Equal(new MapPosition(2, 2), first.Session.Snapshot.Exploration.PlayerPosition);
        Assert.Equal(MapLocalTransitionStatus.Acknowledged, first.Session.Snapshot.LocalTransition?.Status);
        Assert.Equal(new MapPosition(1, 1), restarted.Session.Snapshot.Exploration.PlayerPosition);
        Assert.Null(restarted.Session.Snapshot.LocalTransition);
        Assert.Equal(0, restarted.Session.Snapshot.SimulationStep);
        Assert.Equal(0, restarted.Session.Snapshot.LastCueSequence);
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

        MapEventEffectDefinition effect = EffectDefinition(
            "effect-1",
            "east-zone-request",
            "alternate-enabled",
            "effect-cue-1");
        Assert.Throws<ArgumentException>(() => new MapEventEffectCatalog(
            [effect, EffectDefinition(
                "effect-1", "request-2", "flag-2", "effect-cue-2")]));
        Assert.Throws<ArgumentException>(() => new MapEventEffectCatalog(
            [effect, EffectDefinition(
                "effect-2", "east-zone-request", "flag-2", "effect-cue-2")]));
        Assert.Throws<ArgumentException>(() => new MapEventEffectCatalog(
            [effect, EffectDefinition(
                "effect-2", "request-2", "alternate-enabled", "effect-cue-2")]));
        Assert.Throws<ArgumentException>(() => new MapEventEffectCatalog(
            [effect, EffectDefinition(
                "effect-2", "request-2", "flag-2", "effect-cue-1")]));

        Assert.Throws<ArgumentException>(() => CreateMapContext(
            new MapId("map3"),
            effectRequest: "missing-request"));
        Assert.Throws<ArgumentException>(() => CreateMapContext(
            new MapId("map3"),
            effectFlag: "missing-variant-flag"));
        Assert.Throws<ArgumentException>(() => CreateMapContext(
            new MapId("map3"),
            initialSetFlags: [new FlagId("alternate-enabled")]));
        Assert.Throws<ArgumentException>(() => CreateMapContext(
            new MapId("map3"),
            effectCue: "east-zone-selected"));
        Assert.Throws<ArgumentException>(() => CreateMapContext(
            new MapId("map3"),
            includeEffect: false));
    }

    [Fact]
    public void LocalTransitionCatalogAndScenarioRejectDuplicateOrDanglingDefinitions()
    {
        MapId map = new("map3");
        MapLocalTransitionDefinition first = TransitionDefinition();
        Assert.Throws<ArgumentException>(() => new MapLocalTransitionCatalog(
            [first, TransitionDefinition(
                request: "local-transition-request",
                transition: "other-transition",
                target: "other-target",
                cue: "other-cue")]));
        Assert.Throws<ArgumentException>(() => new MapLocalTransitionCatalog(
            [first, TransitionDefinition(
                request: "other-request",
                transition: "local-transition",
                target: "other-target",
                cue: "other-cue")]));
        Assert.Throws<ArgumentException>(() => new MapLocalTransitionCatalog(
            [first, TransitionDefinition(
                request: "other-request",
                transition: "other-transition",
                target: "local-transition-zone",
                cue: "other-cue")]));
        Assert.Throws<ArgumentException>(() => new MapLocalTransitionCatalog(
            [first, TransitionDefinition(
                request: "other-request",
                transition: "other-transition",
                target: "other-target",
                cue: "local-transition-ready")]));

        Assert.Throws<ArgumentException>(() => CreateMapContext(
            map,
            localTransitions: new MapLocalTransitionCatalog(
                [TransitionDefinition(target: "no-zone")])));
        Assert.Throws<ArgumentException>(() => CreateMapContext(
            map,
            localTransitions: new MapLocalTransitionCatalog(
                [TransitionDefinition(target: "east-zone", source: new MapPosition(2, 1))])));
        Assert.Throws<ArgumentException>(() => CreateMapContext(
            map,
            localTransitions: new MapLocalTransitionCatalog(
                [TransitionDefinition(source: new MapPosition(2, 2))])));
        Assert.Throws<ArgumentException>(() => CreateMapContext(
            map,
            localTransitions: new MapLocalTransitionCatalog(
                [TransitionDefinition(sourceMap: new MapId("missing-map"))])));
        Assert.Throws<ArgumentException>(() => CreateMapContext(
            map,
            localTransitions: new MapLocalTransitionCatalog(
                [TransitionDefinition(destinationMap: new MapId("missing-map"))])));
        Assert.Throws<ArgumentException>(() => CreateMapContext(
            map,
            localTransitions: new MapLocalTransitionCatalog(
                [TransitionDefinition(sourceSetup: "missing-setup")])));
        Assert.Throws<ArgumentException>(() => CreateMapContext(
            map,
            localTransitions: new MapLocalTransitionCatalog(
                [TransitionDefinition(cue: "east-zone-selected")])));
        Assert.Throws<ArgumentOutOfRangeException>(() => new MapPosition(64, 0));
        Assert.Throws<ArgumentException>(() => CreateAcceptedSource(
            new MapLocalTransitionCatalog(
                [TransitionDefinition(destination: new MapPosition(1, 2))])));
        Assert.Throws<ArgumentException>(() => CreateAcceptedSource(
            new MapLocalTransitionCatalog(
                [TransitionDefinition(destination: new MapPosition(0, 0))])));
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
        GameSessionEventRequested requested = Assert.IsType<GameSessionEventRequested>(
            first.Session.Apply(new RequestSelectedZoneEventCommand()));
        Assert.IsType<GameSessionEventEffectApplied>(
            first.Session.Apply(
                new AcknowledgeMapEventRequestCommand(
                    requested.Request.Request,
                    requested.Cue.Sequence,
                    requested.Request.ExpectedEffect)));

        Assert.Equal(new MapPosition(2, 1), first.Session.Snapshot.Exploration.PlayerPosition);
        Assert.Equal(MapEventRequestStatus.Acknowledged, first.Session.Snapshot.EventRequest?.Status);
        Assert.True(first.Session.Snapshot.SyntheticFlags.IsSet(
            new FlagId("alternate-enabled")));
        Assert.NotNull(first.Session.Snapshot.LastEventEffect);
        Assert.Equal(new MapPosition(1, 1), restarted.Session.Snapshot.Exploration.PlayerPosition);
        Assert.Equal(0, restarted.Session.Snapshot.SimulationStep);
        Assert.Equal(0, restarted.Session.Snapshot.LastCueSequence);
        Assert.Null(restarted.Session.Snapshot.EventRequest);
        Assert.Empty(restarted.Session.Snapshot.SyntheticFlags.SetFlags);
        Assert.Null(restarted.Session.Snapshot.LastEventEffect);
        Assert.Null(restarted.Session.Snapshot.LocalTransition);

        GameSessionContextSelected restartedSelection = Assert.IsType<GameSessionContextSelected>(
            restarted.Session.Apply(
                new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)));
        Assert.Equal("synthetic-setup", restartedSelection.Selection.SelectedSetup.Value);
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

    private static IMapScenarioSource CreateAcceptedSource(
        MapLocalTransitionCatalog? localTransitions = null)
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
            CreateMapContext(map, localTransitions: localTransitions));
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
        string requestTarget = "east-zone",
        string effectRequest = "east-zone-request",
        string effectFlag = "alternate-enabled",
        string effectCue = "variant-applied",
        IEnumerable<FlagId>? initialSetFlags = null,
        bool includeEffect = true,
        MapLocalTransitionCatalog? localTransitions = null)
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
                ZoneEventRecord.Specific(
                    EventFieldMatch.Exact(1),
                    EventFieldMatch.Exact(2),
                    new EventTargetId("local-transition-zone")),
                ZoneEventRecord.Default(new EventTargetId("no-zone")),
            ]);
        MapEventRequestCatalog eventRequests = new(
            [RequestDefinition("east-zone-request", requestTarget, "east-zone-selected")]);
        MapEventEffectCatalog eventEffects = new(
            includeEffect
                ? [EffectDefinition(
                    "east-zone-variant-effect",
                    effectRequest,
                    effectFlag,
                    effectCue)]
                : []);
        return new MapScenarioContextDefinition(
            setupCatalog,
            new MapSetupId("void-setup"),
            initialSetFlags ?? [],
            descriptions,
            zoneEvents,
            eventRequests,
            eventEffects,
            localTransitions ?? new MapLocalTransitionCatalog([TransitionDefinition()]));
    }

    private static MapEventRequestDefinition RequestDefinition(
        string request,
        string target,
        string cue) =>
        new(
            new MapEventRequestId(request),
            new EventTargetId(target),
            new PresentationCueId(cue));

    private static MapEventEffectDefinition EffectDefinition(
        string effect,
        string request,
        string flag,
        string cue) =>
        new(
            new MapEventEffectId(effect),
            new MapEventRequestId(request),
            new FlagId(flag),
            new PresentationCueId(cue));

    private static MapLocalTransitionDefinition TransitionDefinition(
        string request = "local-transition-request",
        string transition = "local-transition",
        string target = "local-transition-zone",
        MapId? sourceMap = null,
        MapPosition? source = null,
        string sourceSetup = "alternate-setup",
        MapId? destinationMap = null,
        MapPosition? destination = null,
        string orientation = "synthetic-arrival-east",
        string cue = "local-transition-ready") =>
        new(
            new MapLocalTransitionRequestId(request),
            new MapLocalTransitionId(transition),
            new EventTargetId(target),
            sourceMap ?? new MapId("map3"),
            source ?? new MapPosition(1, 2),
            new MapSetupId(sourceSetup),
            destinationMap ?? new MapId("map3"),
            destination ?? new MapPosition(2, 2),
            new OpaqueMapOrientationId(orientation),
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
