using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class Map3Root : Node2D
{
    public const string BannerText = "PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY";
    public const string SmokeMarker = "SF2_MAP3_SMOKE ";

    private GameSession? _session;
    private ScenarioAdmissionReceipt? _admissionReceipt;
    private Map3Presenter? _presenter;
    private Map3InputAdapter? _inputAdapter;

    public override void _Ready()
    {
        long readyStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        long selectionStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        Map3RuntimeProfileSelection selection =
            Map3RuntimeProfileSelection.Parse(OS.GetCmdlineUserArgs());
        TracePrivateStage(
            selection.PrivateSmokeRequested,
            "profile-selection",
            selectionStarted);
        _runtimeProfile = selection.RequestedProfile;
        _inputAdapter = Map3InputAdapter.CreateGodot(CreatePublicSyntheticInputActions());
        _inputAdapter.EnsureActionsRegistered();
        BuildSelectedPresentation(selection);
        StartScenario(selection);
        TracePrivateStage(selection.PrivateSmokeRequested, "godot-ready", readyStarted);
    }

    public override void _Process(double delta)
    {
        _ = delta;
        if (_session is null)
        {
            return;
        }

        if (_runtimeProfile == Map3RuntimeProfile.PrivateLocal)
        {
            ProcessPrivateInput();
            return;
        }

        _inputAdapter?.PollPublicSynthetic();
    }

    private Map3InputActions CreatePublicSyntheticInputActions() =>
        new(
            ApplyMove,
            ApplyContextSelection,
            ApplyEventRequest,
            ApplyEventRequestAcknowledgement,
            ApplyLocalTransitionRequest,
            ApplyLocalTransitionAcknowledgement,
            ApplyTurn,
            ApplyEntityInteractionRequest,
            ApplyEntityInteractionAcknowledgement,
            ApplyDialogueAdvance,
            ApplyFieldSearchRequest,
            ApplyFieldSearchAcknowledgement,
            ApplyItemAcquisitionRequest,
            ApplyItemAcquisitionAcknowledgement,
            ApplyOutboundTransitionRequest,
            ApplyOutboundTransitionAcknowledgement);

    private void StartScenario(Map3RuntimeProfileSelection selection)
    {
        if (!selection.IsAvailable)
        {
            FailProfileStartup(selection);
            return;
        }

        if (selection.RequestedProfile == Map3RuntimeProfile.PrivateLocal)
        {
            StartPrivateScenario(
                selection.CanonicalImportPath!,
                selection.PrivateSmokeRequested);
            return;
        }

        byte[] packageBytes = Godot.FileAccess.GetFileAsBytes(
            "res://content/public-synthetic-map3-smoke-v1.json");
        PublicSyntheticMap3PackageReader source =
            PublicSyntheticMap3PackageReader.FromDocumentBytes(packageBytes);
        GameSessionStartResult result = GameSession.Start(
            source,
            new MapScenarioRequest(
                PublicSyntheticMap3PackageReader.PackageId,
                ContentProfile.PublicSynthetic));
        if (result is not GameSessionStarted started)
        {
            GameSessionStartRejected rejected = (GameSessionStartRejected)result;
            FailStartup(rejected.Diagnostic.Message);
            return;
        }

        _session = started.Session;
        _admissionReceipt = started.Receipt;
        ProjectSnapshot("Ready");
        if (OS.GetCmdlineUserArgs().Contains("--map3-smoke", StringComparer.Ordinal))
        {
            Callable.From(RunHeadlessSmoke).CallDeferred();
        }
    }

    private void ApplyMove(ExplorationDirection direction)
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new MoveExplorationCommand(direction));
        if (result is GameSessionCommandApplied applied)
        {
            ProjectSnapshot(applied.Outcome.ToString());
            return;
        }

        GameSessionCommandRejected rejected = (GameSessionCommandRejected)result;
        ProjectRejection(rejected);
    }

    private void ApplyContextSelection()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary));
        if (result is GameSessionContextSelected selected)
        {
            ProjectSnapshot("Context selected");
            return;
        }

        GameSessionCommandRejected rejected = (GameSessionCommandRejected)result;
        ProjectRejection(rejected);
    }

    private void ApplyEventRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new RequestSelectedZoneEventCommand());
        if (result is GameSessionEventRequested)
        {
            ProjectSnapshot("Event request pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyEventRequestAcknowledgement()
    {
        if (_session?.Snapshot.EventRequest is not MapEventRequestSnapshot request)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeMapEventRequestCommand(
                request.Request,
                request.CueSequence,
                request.ExpectedEffect));
        if (result is GameSessionEventEffectApplied)
        {
            ProjectSnapshot("Synthetic effect applied; re-select context");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyLocalTransitionRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new RequestSelectedLocalTransitionCommand());
        if (result is GameSessionLocalTransitionRequested)
        {
            ProjectSnapshot("Local transition pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyLocalTransitionAcknowledgement()
    {
        if (_session?.Snapshot.LocalTransition is not MapLocalTransitionSnapshot transition)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeMapLocalTransitionCommand(
                transition.Request,
                transition.CueSequence,
                transition.Transition));
        if (result is GameSessionLocalTransitionApplied)
        {
            ProjectSnapshot("Synthetic local transition applied");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyTurn(SemanticFacing facing)
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new TurnExplorationCommand(facing));
        if (result is GameSessionFacingChanged)
        {
            ProjectSnapshot($"Facing {facing}");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyEntityInteractionRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new RequestEntityInteractionCommand());
        if (result is GameSessionEntityInteractionRequested)
        {
            ProjectSnapshot("Placeholder entity interaction pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyEntityInteractionAcknowledgement()
    {
        if (_session?.Snapshot.EntityInteraction is not MapEntityInteractionSnapshot interaction)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeEntityInteractionCommand(
                interaction.Request,
                interaction.CueSequence,
                interaction.Entity,
                interaction.Target));
        if (result is GameSessionEntityInteractionAcknowledged acknowledged)
        {
            ProjectSnapshot(
                $"Placeholder interaction acknowledged; dialogue {acknowledged.Dialogue.Dialogue} opened");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyDialogueAdvance()
    {
        if (_session?.Snapshot.Dialogue is not MapDialogueSnapshot
            {
                Status: MapDialogueStatus.Open,
                CurrentLine: not null,
            } dialogue)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AdvanceDialogueCommand(
                dialogue.Dialogue,
                dialogue.CueSequence,
                dialogue.CurrentLine.Line));
        switch (result)
        {
            case GameSessionDialogueAdvanced advanced:
                ProjectSnapshot(
                    $"Placeholder dialogue advanced to line {advanced.Dialogue.CurrentLineIndex + 1}");
                return;
            case GameSessionDialogueClosed:
                ProjectSnapshot("Placeholder dialogue closed");
                return;
            default:
                ProjectRejection((GameSessionCommandRejected)result);
                return;
        }
    }

    private void ApplyFieldSearchRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(new RequestFieldSearchCommand());
        if (result is GameSessionFieldSearchRequested requested)
        {
            ProjectSnapshot($"Synthetic field search {requested.Search.Context} pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyFieldSearchAcknowledgement()
    {
        if (_session?.Snapshot.FieldSearch is not MapFieldSearchSnapshot
            {
                Status: MapFieldSearchStatus.Pending,
            } search)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeFieldSearchCommand(
                search.Request,
                search.RequestCueSequence,
                search.Result));
        if (result is GameSessionFieldSearchDiscovered discovered)
        {
            ProjectSnapshot($"Placeholder discovery {discovered.Receipt.Discovery} admitted");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyItemAcquisitionRequest()
    {
        if (_session?.Snapshot.FieldSearch is not MapFieldSearchSnapshot
            {
                Status: MapFieldSearchStatus.Discovered,
            } search)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new RequestMapItemAcquisitionCommand(search.Discovery));
        if (result is GameSessionItemAcquisitionRequested requested)
        {
            ProjectSnapshot($"Placeholder item {requested.Acquisition.Item} pending acquisition");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyItemAcquisitionAcknowledgement()
    {
        if (_session?.Snapshot.ItemAcquisition is not MapItemAcquisitionSnapshot
            {
                Status: MapItemAcquisitionStatus.Pending,
            } acquisition)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeMapItemAcquisitionCommand(
                acquisition.Request,
                acquisition.RequestCueSequence,
                acquisition.Result,
                acquisition.Item));
        if (result is GameSessionItemAcquired acquired)
        {
            ProjectSnapshot($"Placeholder item {acquired.Receipt.Item} acquired once");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyOutboundTransitionRequest()
    {
        if (_session is null)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new RequestSelectedOutboundTransitionCommand());
        if (result is GameSessionOutboundTransitionRequested)
        {
            ProjectSnapshot("Outbound transition pending");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ApplyOutboundTransitionAcknowledgement()
    {
        if (_session?.Snapshot.OutboundTransition is not MapOutboundTransitionSnapshot transition)
        {
            return;
        }

        GameSessionCommandResult result = _session.Apply(
            new AcknowledgeMapOutboundTransitionCommand(
                transition.Request,
                transition.CueSequence,
                transition.Transition));
        if (result is GameSessionOutboundTransitionApplied)
        {
            ProjectSnapshot("Public-synthetic outbound transition applied");
            return;
        }

        ProjectRejection((GameSessionCommandRejected)result);
    }

    private void ProjectRejection(GameSessionCommandRejected rejected)
    {
        _presenter?.ProjectStatus(rejected.Diagnostic.Message);
    }

    private void ProjectSnapshot(string outcome)
    {
        if (_session is null)
        {
            return;
        }

        _presenter?.Project(_session.Snapshot, outcome);
    }

    private void RunHeadlessSmoke()
    {
        if (_session is null || _admissionReceipt is null)
        {
            FailStartup("Session was not admitted.");
            return;
        }

        GameSessionSnapshot before = _session.Snapshot;
        GameSessionCommandApplied? applied = _session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)) as GameSessionCommandApplied;
        if (applied is null || applied.Outcome != ExplorationMovementOutcome.Moved)
        {
            FailStartup("The bounded synthetic movement command did not move.");
            return;
        }

        GameSessionContextSelected? selected = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (selected is null ||
            selected.Selection.Position != applied.Snapshot.Exploration.PlayerPosition ||
            selected.Selection.SelectedSetup.Value != applied.Snapshot.AdmissionFacts.SetupIdentity ||
            selected.Selection.AreaDescription.Kind != AreaDescriptionSelectionKind.Text ||
            selected.Selection.ZoneEvent.Target.Value != "synthetic-map3-east-zone")
        {
            FailStartup("The bounded synthetic setup/area/event selection did not match.");
            return;
        }

        GameSessionEventRequested? requested = _session.Apply(
            new RequestSelectedZoneEventCommand()) as GameSessionEventRequested;
        if (requested is null ||
            requested.Request.Status != MapEventRequestStatus.Pending ||
            requested.Request.Target != selected.Selection.ZoneEvent.Target ||
            requested.Cue.Cue.Value != "synthetic-map3-east-zone-selected" ||
            !requested.Cue.RequiresAcknowledgement)
        {
            FailStartup("The bounded synthetic event request was not admitted.");
            return;
        }

        GameSessionEventEffectApplied? acknowledged = _session.Apply(
            new AcknowledgeMapEventRequestCommand(
                requested.Request.Request,
                requested.Cue.Sequence,
                requested.Request.ExpectedEffect)) as GameSessionEventEffectApplied;
        if (acknowledged is null ||
            acknowledged.Request.Status != MapEventRequestStatus.Acknowledged ||
            acknowledged.Request.CueSequence != requested.Cue.Sequence ||
            acknowledged.Effect.Effect.Value !=
                "synthetic-map3-east-zone-variant-effect" ||
            acknowledged.Effect.Flag.Value != "synthetic-map3-variant-enabled" ||
            acknowledged.Cue.Cue.Value != "synthetic-map3-variant-applied" ||
            acknowledged.Cue.RequiresAcknowledgement ||
            acknowledged.Snapshot.ContextSelection is not null ||
            !acknowledged.Snapshot.SyntheticFlags.IsSet(acknowledged.Effect.Flag))
        {
            FailStartup("The bounded synthetic state effect was not applied atomically.");
            return;
        }

        GameSessionContextSelected? reselected = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (reselected is null ||
            reselected.Selection.SelectedSetup.Value != "synthetic-map3-variant" ||
            reselected.Selection.Position != acknowledged.Snapshot.Exploration.PlayerPosition)
        {
            FailStartup("The synthetic setup variant was not visible after context re-selection.");
            return;
        }

        GameSessionCommandApplied? transitionMove = _session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)) as GameSessionCommandApplied;
        if (transitionMove is null ||
            transitionMove.Outcome != ExplorationMovementOutcome.Moved ||
            transitionMove.Snapshot.Exploration.PlayerPosition != new MapPosition(58, 3))
        {
            FailStartup("The bounded synthetic transition source was not reached.");
            return;
        }

        GameSessionContextSelected? transitionSelection = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (transitionSelection is null ||
            transitionSelection.Selection.ZoneEvent.Target.Value !=
                "synthetic-map3-local-transition-zone")
        {
            FailStartup("The bounded synthetic local-transition context did not match.");
            return;
        }

        GameSessionLocalTransitionRequested? transitionRequest = _session.Apply(
            new RequestSelectedLocalTransitionCommand()) as
            GameSessionLocalTransitionRequested;
        if (transitionRequest is null ||
            transitionRequest.Transition.Status != MapLocalTransitionStatus.Pending ||
            transitionRequest.Transition.SourcePosition != new MapPosition(58, 3) ||
            transitionRequest.Transition.DestinationPosition != new MapPosition(55, 4) ||
            transitionRequest.Cue.Cue.Value != "synthetic-map3-local-transition-ready" ||
            !transitionRequest.Cue.RequiresAcknowledgement)
        {
            FailStartup("The bounded synthetic local transition was not admitted.");
            return;
        }

        GameSessionLocalTransitionApplied? transitionApplied = _session.Apply(
            new AcknowledgeMapLocalTransitionCommand(
                transitionRequest.Transition.Request,
                transitionRequest.Cue.Sequence,
                transitionRequest.Transition.Transition)) as
            GameSessionLocalTransitionApplied;
        if (transitionApplied is null ||
            transitionApplied.Transition.Status != MapLocalTransitionStatus.Acknowledged ||
            transitionApplied.Snapshot.Exploration.Map.Value != "map3" ||
            transitionApplied.Snapshot.Exploration.PlayerPosition != new MapPosition(55, 4) ||
            transitionApplied.Transition.DestinationOrientation.Value !=
                "synthetic-arrival-south" ||
            transitionApplied.Snapshot.ContextSelection is not null ||
            transitionApplied.Snapshot.EventRequest is not null ||
            transitionApplied.Snapshot.LastEventEffect is not null)
        {
            FailStartup("The bounded synthetic local transition was not applied atomically.");
            return;
        }

        GameSessionFacingChanged? turned = _session.Apply(
            new TurnExplorationCommand(SemanticFacing.North)) as GameSessionFacingChanged;
        if (turned is null ||
            turned.Facing != SemanticFacing.North ||
            turned.Snapshot.Exploration.PlayerPosition != new MapPosition(55, 4) ||
            turned.Snapshot.EntityInteraction is not null)
        {
            FailStartup("The bounded synthetic facing command was not applied.");
            return;
        }

        GameSessionEntityInteractionRequested? entityRequested = _session.Apply(
            new RequestEntityInteractionCommand()) as GameSessionEntityInteractionRequested;
        if (entityRequested is null ||
            entityRequested.Interaction.Status != MapEntityInteractionStatus.Pending ||
            entityRequested.Interaction.Entity.Value !=
                "synthetic-map3-placeholder-guide" ||
            entityRequested.Interaction.Target.Value !=
                "synthetic-map3-placeholder-guide-target" ||
            entityRequested.Interaction.PlayerPosition != new MapPosition(55, 4) ||
            entityRequested.Interaction.EntityPosition != new MapPosition(55, 3) ||
            entityRequested.Interaction.Facing != SemanticFacing.North ||
            entityRequested.Cue.Cue.Value != "synthetic-map3-placeholder-guide-cue" ||
            !entityRequested.Cue.RequiresAcknowledgement)
        {
            FailStartup("The bounded placeholder entity interaction was not admitted.");
            return;
        }

        GameSessionEntityInteractionAcknowledged? entityAcknowledged = _session.Apply(
            new AcknowledgeEntityInteractionCommand(
                entityRequested.Interaction.Request,
                entityRequested.Cue.Sequence,
                entityRequested.Interaction.Entity,
                entityRequested.Interaction.Target)) as
            GameSessionEntityInteractionAcknowledged;
        if (entityAcknowledged is null ||
            entityAcknowledged.Interaction.Status !=
                MapEntityInteractionStatus.Acknowledged ||
            entityAcknowledged.Interaction.CueSequence != entityRequested.Cue.Sequence ||
            entityAcknowledged.Snapshot.Exploration.PlayerPosition != new MapPosition(55, 4) ||
            entityAcknowledged.Dialogue.Status != MapDialogueStatus.Open ||
            entityAcknowledged.Dialogue.CurrentLine?.Line.Value !=
                "synthetic-map3-placeholder-guide-line-1" ||
            entityAcknowledged.Cue.Text !=
                "Hello from a project-authored placeholder.")
        {
            FailStartup("The bounded placeholder dialogue did not open from the acknowledged interaction.");
            return;
        }

        GameSessionDialogueAdvanced? dialogueAdvanced = _session.Apply(
            new AdvanceDialogueCommand(
                entityAcknowledged.Dialogue.Dialogue,
                entityAcknowledged.Dialogue.CueSequence,
                entityAcknowledged.Dialogue.CurrentLine!.Line)) as
            GameSessionDialogueAdvanced;
        if (dialogueAdvanced is null ||
            dialogueAdvanced.Dialogue.Status != MapDialogueStatus.Open ||
            dialogueAdvanced.Dialogue.CurrentLine?.Line.Value !=
                "synthetic-map3-placeholder-guide-line-2" ||
            dialogueAdvanced.Cue.Text !=
                "This is synthetic text, not original game dialogue.")
        {
            FailStartup("The bounded placeholder dialogue did not advance to its second line.");
            return;
        }

        GameSessionDialogueClosed? dialogueClosed = _session.Apply(
            new AdvanceDialogueCommand(
                dialogueAdvanced.Dialogue.Dialogue,
                dialogueAdvanced.Dialogue.CueSequence,
                dialogueAdvanced.Dialogue.CurrentLine!.Line)) as
            GameSessionDialogueClosed;
        if (dialogueClosed is null ||
            dialogueClosed.Dialogue.Status != MapDialogueStatus.Closed ||
            dialogueClosed.Dialogue.CurrentLine is not null ||
            dialogueClosed.Cue.Cue.Value !=
                "synthetic-map3-placeholder-guide-dialogue-closed")
        {
            FailStartup("The bounded placeholder dialogue did not close atomically.");
            return;
        }

        GameSessionContextSelected? searchContext = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (searchContext is null ||
            searchContext.Selection.Position != new MapPosition(55, 4) ||
            searchContext.Selection.SelectedSetup.Value != "synthetic-map3-variant" ||
            searchContext.Selection.ZoneEvent.Target.Value != "synthetic-no-zone")
        {
            FailStartup("The bounded synthetic field-search context did not match.");
            return;
        }

        GameSessionFieldSearchRequested? searchRequested = _session.Apply(
            new RequestFieldSearchCommand()) as GameSessionFieldSearchRequested;
        if (searchRequested is null ||
            searchRequested.Search.Status != MapFieldSearchStatus.Pending ||
            searchRequested.Search.Context.Value !=
                "synthetic-map3-arrival-search-context" ||
            searchRequested.Search.Request.Value !=
                "synthetic-map3-field-search-request" ||
            searchRequested.Search.Result.Value !=
                "synthetic-map3-field-search-result" ||
            searchRequested.Search.Discovery.Value !=
                "synthetic-map3-placeholder-discovery" ||
            searchRequested.Cue.Cue.Value != "synthetic-map3-field-search-pending" ||
            !searchRequested.Cue.RequiresAcknowledgement)
        {
            FailStartup("The bounded synthetic field search was not admitted.");
            return;
        }

        GameSessionFieldSearchDiscovered? searchDiscovered = _session.Apply(
            new AcknowledgeFieldSearchCommand(
                searchRequested.Search.Request,
                searchRequested.Cue.Sequence,
                searchRequested.Search.Result)) as GameSessionFieldSearchDiscovered;
        if (searchDiscovered is null ||
            searchDiscovered.Search.Status != MapFieldSearchStatus.Discovered ||
            searchDiscovered.Receipt.Context != searchRequested.Search.Context ||
            searchDiscovered.Receipt.Result != searchRequested.Search.Result ||
            searchDiscovered.Receipt.Discovery != searchRequested.Search.Discovery ||
            searchDiscovered.Cue.Cue.Value !=
                "synthetic-map3-placeholder-discovered" ||
            searchDiscovered.Cue.RequiresAcknowledgement ||
            !searchDiscovered.Snapshot.Discoveries.IsDiscovered(
                searchDiscovered.Receipt.Discovery))
        {
            FailStartup("The bounded placeholder discovery was not applied atomically.");
            return;
        }

        GameSessionSnapshot discoveredSnapshot = searchDiscovered.Snapshot;
        GameSessionCommandRejected? repeatedSearch = _session.Apply(
            new RequestFieldSearchCommand()) as GameSessionCommandRejected;
        if (repeatedSearch?.Diagnostic.Code !=
                GameSessionCommandFailureCode.FieldSearchAlreadyDiscovered ||
            !ReferenceEquals(discoveredSnapshot, _session.Snapshot))
        {
            FailStartup("The bounded placeholder discovery was not once-only.");
            return;
        }

        GameSessionItemAcquisitionRequested? itemRequested = _session.Apply(
            new RequestMapItemAcquisitionCommand(searchDiscovered.Receipt.Discovery)) as
            GameSessionItemAcquisitionRequested;
        if (itemRequested is null ||
            itemRequested.Acquisition.Status != MapItemAcquisitionStatus.Pending ||
            itemRequested.Acquisition.Discovery != searchDiscovered.Receipt.Discovery ||
            itemRequested.Acquisition.Request.Value !=
                "synthetic-map3-placeholder-item-acquisition-request" ||
            itemRequested.Acquisition.Result.Value !=
                "synthetic-map3-placeholder-item-acquisition-result" ||
            itemRequested.Acquisition.Item.Value != "synthetic-map3-placeholder-item" ||
            itemRequested.Cue.Cue.Value !=
                "synthetic-map3-placeholder-item-acquisition-pending" ||
            !itemRequested.Cue.RequiresAcknowledgement ||
            itemRequested.Snapshot.Inventory.Items.Count != 0)
        {
            FailStartup("The bounded placeholder item acquisition was not admitted.");
            return;
        }

        GameSessionItemAcquired? itemAcquired = _session.Apply(
            new AcknowledgeMapItemAcquisitionCommand(
                itemRequested.Acquisition.Request,
                itemRequested.Cue.Sequence,
                itemRequested.Acquisition.Result,
                itemRequested.Acquisition.Item)) as GameSessionItemAcquired;
        if (itemAcquired is null ||
            itemAcquired.Acquisition.Status != MapItemAcquisitionStatus.Acquired ||
            itemAcquired.Receipt.Discovery != itemRequested.Acquisition.Discovery ||
            itemAcquired.Receipt.Request != itemRequested.Acquisition.Request ||
            itemAcquired.Receipt.Result != itemRequested.Acquisition.Result ||
            itemAcquired.Receipt.Item != itemRequested.Acquisition.Item ||
            itemAcquired.Cue.Cue.Value != "synthetic-map3-placeholder-item-acquired" ||
            itemAcquired.Cue.RequiresAcknowledgement ||
            itemAcquired.Snapshot.Inventory.Items.Count != 1 ||
            itemAcquired.Snapshot.Inventory.Items.Single() != itemAcquired.Receipt.Item)
        {
            FailStartup("The bounded placeholder item was not acquired atomically.");
            return;
        }

        GameSessionSnapshot acquiredSnapshot = itemAcquired.Snapshot;
        GameSessionCommandRejected? repeatedAcquisition = _session.Apply(
            new RequestMapItemAcquisitionCommand(itemAcquired.Receipt.Discovery)) as
            GameSessionCommandRejected;
        GameSessionCommandRejected? duplicateAcquisitionAcknowledgement = _session.Apply(
            new AcknowledgeMapItemAcquisitionCommand(
                itemRequested.Acquisition.Request,
                itemRequested.Cue.Sequence,
                itemRequested.Acquisition.Result,
                itemRequested.Acquisition.Item)) as GameSessionCommandRejected;
        if (repeatedAcquisition?.Diagnostic.Code !=
                GameSessionCommandFailureCode.ItemAlreadyAcquired ||
            duplicateAcquisitionAcknowledgement?.Diagnostic.Code !=
                GameSessionCommandFailureCode.NoPendingAcknowledgement ||
            !ReferenceEquals(acquiredSnapshot, _session.Snapshot))
        {
            FailStartup("The bounded placeholder item acquisition was not once-only.");
            return;
        }

        GameSessionCommandApplied? outboundMove = _session.Apply(
            new MoveExplorationCommand(ExplorationDirection.West)) as GameSessionCommandApplied;
        GameSessionContextSelected? outboundContext = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (outboundMove?.Outcome != ExplorationMovementOutcome.Moved ||
            outboundMove.Snapshot.Exploration.PlayerPosition != new MapPosition(54, 4) ||
            outboundContext?.Selection.Map.Value != "map3" ||
            outboundContext.Selection.ZoneEvent.Target.Value !=
                "synthetic-map3-outbound-transition-zone")
        {
            FailStartup("The bounded public-synthetic outbound source context did not match.");
            return;
        }

        GameSessionOutboundTransitionRequested? outboundRequested = _session.Apply(
            new RequestSelectedOutboundTransitionCommand()) as
            GameSessionOutboundTransitionRequested;
        if (outboundRequested is null ||
            outboundRequested.Transition.Status != MapOutboundTransitionStatus.Pending ||
            outboundRequested.Transition.SourceMap.Value != "map3" ||
            outboundRequested.Transition.DestinationMap.Value !=
                "public-synthetic-outbound-shell" ||
            outboundRequested.Cue.Cue.Value != "synthetic-map3-outbound-transition-ready" ||
            !outboundRequested.Cue.RequiresAcknowledgement)
        {
            FailStartup("The bounded public-synthetic outbound transition was not admitted.");
            return;
        }

        GameSessionOutboundTransitionApplied? outboundApplied = _session.Apply(
            new AcknowledgeMapOutboundTransitionCommand(
                outboundRequested.Transition.Request,
                outboundRequested.Cue.Sequence,
                outboundRequested.Transition.Transition)) as
            GameSessionOutboundTransitionApplied;
        if (outboundApplied is null ||
            outboundApplied.Transition.Status != MapOutboundTransitionStatus.Acknowledged ||
            outboundApplied.Snapshot.Exploration.Map.Value !=
                "public-synthetic-outbound-shell" ||
            outboundApplied.Snapshot.Exploration.PlayerPosition != new MapPosition(1, 1) ||
            outboundApplied.Snapshot.Facing != SemanticFacing.East ||
            outboundApplied.Snapshot.ContextSelection is not null ||
            outboundApplied.Snapshot.Entities.Count != 0 ||
            !outboundApplied.Snapshot.SyntheticFlags.IsSet(
                new FlagId("synthetic-map3-variant-enabled")) ||
            !outboundApplied.Snapshot.Discoveries.IsDiscovered(
                itemAcquired.Receipt.Discovery) ||
            !outboundApplied.Snapshot.Inventory.Contains(itemAcquired.Receipt.Item))
        {
            FailStartup("The bounded public-synthetic runtime swap was not atomic.");
            return;
        }

        GameSessionContextSelected? outboundShellContext = _session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (outboundShellContext?.Selection.Map.Value !=
                "public-synthetic-outbound-shell" ||
            outboundShellContext.Selection.SelectedSetup.Value !=
                "public-synthetic-outbound-shell-setup" ||
            outboundShellContext.Selection.Position != new MapPosition(1, 1) ||
            outboundShellContext.Selection.ZoneEvent.Target.Value !=
                "synthetic-outbound-shell-no-zone")
        {
            FailStartup("The public-synthetic outbound shell context did not match.");
            return;
        }

        ProjectSnapshot("Public-synthetic outbound shell admitted");
        object receipt = new
        {
            status = "Pass",
            profile = "public-synthetic",
            scenarioId = applied.Snapshot.ScenarioId,
            exactControlledAdmission = _admissionReceipt.ExactControlledAdmission,
            capability = _admissionReceipt.Capabilities.Single(
                capability => capability == PublicSyntheticMap3PackageReader.Capability),
            evidenceOwner = _admissionReceipt.EvidenceOwnerIds.Single(),
            mapId = applied.Snapshot.AdmissionFacts.CurrentMap.Value,
            opaqueStartFacing = applied.Snapshot.AdmissionFacts.OpaqueStartFacing,
            before = new
            {
                x = before.Exploration.PlayerPosition.X,
                y = before.Exploration.PlayerPosition.Y,
            },
            after = new
            {
                x = applied.Snapshot.Exploration.PlayerPosition.X,
                y = applied.Snapshot.Exploration.PlayerPosition.Y,
            },
            outcome = applied.Outcome.ToString(),
            simulationStep = applied.Snapshot.SimulationStep,
            banner = BannerText,
        };
        GD.Print(SmokeMarker + JsonSerializer.Serialize(receipt));
        GetTree().Quit(0);
    }

    private void FailStartup(string message)
    {
        GD.PushError(message);
        _presenter?.ProjectStatus(message);

        if (OS.GetCmdlineUserArgs().Contains("--map3-smoke", StringComparer.Ordinal))
        {
            GD.Print(SmokeMarker + JsonSerializer.Serialize(new { status = "Fail", message }));
            GetTree().Quit(1);
        }
    }

    private void BuildPresentation()
    {
        _presenter = Map3Presenter.Attach(this);
    }
}
