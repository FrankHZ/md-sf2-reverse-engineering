using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal static class PublicSyntheticMap3SmokeDriver
{
    internal static void Run(
        SceneTree sceneTree,
        GameSession session,
        ScenarioAdmissionReceipt admissionReceipt,
        Map3Presenter presenter)
    {
        GameSessionSnapshot before = session.Snapshot;
        GameSessionCommandApplied? applied = session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)) as GameSessionCommandApplied;
        if (applied is null || applied.Outcome != ExplorationMovementOutcome.Moved)
        {
            Fail(sceneTree, presenter, "The bounded synthetic movement command did not move.");
            return;
        }

        GameSessionContextSelected? selected = session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (selected is null ||
            selected.Selection.Position != applied.Snapshot.Exploration.PlayerPosition ||
            selected.Selection.SelectedSetup.Value != applied.Snapshot.AdmissionFacts.SetupIdentity ||
            selected.Selection.AreaDescription.Kind != AreaDescriptionSelectionKind.Text ||
            selected.Selection.ZoneEvent.Target.Value != "synthetic-map3-east-zone")
        {
            Fail(sceneTree, presenter, "The bounded synthetic setup/area/event selection did not match.");
            return;
        }

        GameSessionEventRequested? requested = session.Apply(
            new RequestSelectedZoneEventCommand()) as GameSessionEventRequested;
        if (requested is null ||
            requested.Request.Status != MapEventRequestStatus.Pending ||
            requested.Request.Target != selected.Selection.ZoneEvent.Target ||
            requested.Cue.Cue.Value != "synthetic-map3-east-zone-selected" ||
            !requested.Cue.RequiresAcknowledgement)
        {
            Fail(sceneTree, presenter, "The bounded synthetic event request was not admitted.");
            return;
        }

        GameSessionEventEffectApplied? acknowledged = session.Apply(
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
            Fail(sceneTree, presenter, "The bounded synthetic state effect was not applied atomically.");
            return;
        }

        GameSessionContextSelected? reselected = session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (reselected is null ||
            reselected.Selection.SelectedSetup.Value != "synthetic-map3-variant" ||
            reselected.Selection.Position != acknowledged.Snapshot.Exploration.PlayerPosition)
        {
            Fail(sceneTree, presenter, "The synthetic setup variant was not visible after context re-selection.");
            return;
        }

        GameSessionCommandApplied? transitionMove = session.Apply(
            new MoveExplorationCommand(ExplorationDirection.East)) as GameSessionCommandApplied;
        if (transitionMove is null ||
            transitionMove.Outcome != ExplorationMovementOutcome.Moved ||
            transitionMove.Snapshot.Exploration.PlayerPosition != new MapPosition(58, 3))
        {
            Fail(sceneTree, presenter, "The bounded synthetic transition source was not reached.");
            return;
        }

        GameSessionContextSelected? transitionSelection = session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (transitionSelection is null ||
            transitionSelection.Selection.ZoneEvent.Target.Value !=
                "synthetic-map3-local-transition-zone")
        {
            Fail(sceneTree, presenter, "The bounded synthetic local-transition context did not match.");
            return;
        }

        GameSessionLocalTransitionRequested? transitionRequest = session.Apply(
            new RequestSelectedLocalTransitionCommand()) as
            GameSessionLocalTransitionRequested;
        if (transitionRequest is null ||
            transitionRequest.Transition.Status != MapLocalTransitionStatus.Pending ||
            transitionRequest.Transition.SourcePosition != new MapPosition(58, 3) ||
            transitionRequest.Transition.DestinationPosition != new MapPosition(55, 4) ||
            transitionRequest.Cue.Cue.Value != "synthetic-map3-local-transition-ready" ||
            !transitionRequest.Cue.RequiresAcknowledgement)
        {
            Fail(sceneTree, presenter, "The bounded synthetic local transition was not admitted.");
            return;
        }

        GameSessionLocalTransitionApplied? transitionApplied = session.Apply(
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
            Fail(sceneTree, presenter, "The bounded synthetic local transition was not applied atomically.");
            return;
        }

        GameSessionFacingChanged? turned = session.Apply(
            new TurnExplorationCommand(SemanticFacing.North)) as GameSessionFacingChanged;
        if (turned is null ||
            turned.Facing != SemanticFacing.North ||
            turned.Snapshot.Exploration.PlayerPosition != new MapPosition(55, 4) ||
            turned.Snapshot.EntityInteraction is not null)
        {
            Fail(sceneTree, presenter, "The bounded synthetic facing command was not applied.");
            return;
        }

        GameSessionEntityInteractionRequested? entityRequested = session.Apply(
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
            Fail(sceneTree, presenter, "The bounded placeholder entity interaction was not admitted.");
            return;
        }

        GameSessionEntityInteractionAcknowledged? entityAcknowledged = session.Apply(
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
            Fail(
                sceneTree,
                presenter,
                "The bounded placeholder dialogue did not open from the acknowledged interaction.");
            return;
        }

        GameSessionDialogueAdvanced? dialogueAdvanced = session.Apply(
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
            Fail(
                sceneTree,
                presenter,
                "The bounded placeholder dialogue did not advance to its second line.");
            return;
        }

        GameSessionDialogueClosed? dialogueClosed = session.Apply(
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
            Fail(sceneTree, presenter, "The bounded placeholder dialogue did not close atomically.");
            return;
        }

        GameSessionContextSelected? searchContext = session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (searchContext is null ||
            searchContext.Selection.Position != new MapPosition(55, 4) ||
            searchContext.Selection.SelectedSetup.Value != "synthetic-map3-variant" ||
            searchContext.Selection.ZoneEvent.Target.Value != "synthetic-no-zone")
        {
            Fail(sceneTree, presenter, "The bounded synthetic field-search context did not match.");
            return;
        }

        GameSessionFieldSearchRequested? searchRequested = session.Apply(
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
            Fail(sceneTree, presenter, "The bounded synthetic field search was not admitted.");
            return;
        }

        GameSessionFieldSearchDiscovered? searchDiscovered = session.Apply(
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
            Fail(sceneTree, presenter, "The bounded placeholder discovery was not applied atomically.");
            return;
        }

        GameSessionSnapshot discoveredSnapshot = searchDiscovered.Snapshot;
        GameSessionCommandRejected? repeatedSearch = session.Apply(
            new RequestFieldSearchCommand()) as GameSessionCommandRejected;
        if (repeatedSearch?.Diagnostic.Code !=
                GameSessionCommandFailureCode.FieldSearchAlreadyDiscovered ||
            !ReferenceEquals(discoveredSnapshot, session.Snapshot))
        {
            Fail(sceneTree, presenter, "The bounded placeholder discovery was not once-only.");
            return;
        }

        GameSessionItemAcquisitionRequested? itemRequested = session.Apply(
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
            Fail(sceneTree, presenter, "The bounded placeholder item acquisition was not admitted.");
            return;
        }

        GameSessionItemAcquired? itemAcquired = session.Apply(
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
            Fail(sceneTree, presenter, "The bounded placeholder item was not acquired atomically.");
            return;
        }

        GameSessionSnapshot acquiredSnapshot = itemAcquired.Snapshot;
        GameSessionCommandRejected? repeatedAcquisition = session.Apply(
            new RequestMapItemAcquisitionCommand(itemAcquired.Receipt.Discovery)) as
            GameSessionCommandRejected;
        GameSessionCommandRejected? duplicateAcquisitionAcknowledgement = session.Apply(
            new AcknowledgeMapItemAcquisitionCommand(
                itemRequested.Acquisition.Request,
                itemRequested.Cue.Sequence,
                itemRequested.Acquisition.Result,
                itemRequested.Acquisition.Item)) as GameSessionCommandRejected;
        if (repeatedAcquisition?.Diagnostic.Code !=
                GameSessionCommandFailureCode.ItemAlreadyAcquired ||
            duplicateAcquisitionAcknowledgement?.Diagnostic.Code !=
                GameSessionCommandFailureCode.NoPendingAcknowledgement ||
            !ReferenceEquals(acquiredSnapshot, session.Snapshot))
        {
            Fail(sceneTree, presenter, "The bounded placeholder item acquisition was not once-only.");
            return;
        }

        GameSessionCommandApplied? outboundMove = session.Apply(
            new MoveExplorationCommand(ExplorationDirection.West)) as GameSessionCommandApplied;
        GameSessionContextSelected? outboundContext = session.Apply(
            new SelectExplorationContextCommand(AreaDescriptionAdmission.Ordinary)) as
            GameSessionContextSelected;
        if (outboundMove?.Outcome != ExplorationMovementOutcome.Moved ||
            outboundMove.Snapshot.Exploration.PlayerPosition != new MapPosition(54, 4) ||
            outboundContext?.Selection.Map.Value != "map3" ||
            outboundContext.Selection.ZoneEvent.Target.Value !=
                "synthetic-map3-outbound-transition-zone")
        {
            Fail(sceneTree, presenter, "The bounded public-synthetic outbound source context did not match.");
            return;
        }

        GameSessionOutboundTransitionRequested? outboundRequested = session.Apply(
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
            Fail(sceneTree, presenter, "The bounded public-synthetic outbound transition was not admitted.");
            return;
        }

        GameSessionOutboundTransitionApplied? outboundApplied = session.Apply(
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
            Fail(sceneTree, presenter, "The bounded public-synthetic runtime swap was not atomic.");
            return;
        }

        GameSessionContextSelected? outboundShellContext = session.Apply(
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
            Fail(sceneTree, presenter, "The public-synthetic outbound shell context did not match.");
            return;
        }

        presenter.Project(session.Snapshot, "Public-synthetic outbound shell admitted");
        object smokeReceipt = new
        {
            status = "Pass",
            profile = "public-synthetic",
            scenarioId = applied.Snapshot.ScenarioId,
            exactControlledAdmission = admissionReceipt.ExactControlledAdmission,
            capability = admissionReceipt.Capabilities.Single(
                capability => capability == PublicSyntheticMap3PackageReader.Capability),
            evidenceOwner = admissionReceipt.EvidenceOwnerIds.Single(),
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
            banner = Map3Root.BannerText,
        };
        GD.Print(Map3Root.SmokeMarker + JsonSerializer.Serialize(smokeReceipt));
        sceneTree.Quit(0);
    }

    private static void Fail(
        SceneTree sceneTree,
        Map3Presenter presenter,
        string message)
    {
        GD.PushError(message);
        presenter.ProjectStatus(message);
        GD.Print(Map3Root.SmokeMarker + JsonSerializer.Serialize(new { status = "Fail", message }));
        sceneTree.Quit(1);
    }
}
