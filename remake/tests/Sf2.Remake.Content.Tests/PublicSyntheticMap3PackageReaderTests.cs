using System.Security.Cryptography;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Content;
using Sf2.Remake.Domain.Items;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Content.Tests;

public sealed class PublicSyntheticMap3PackageReaderTests
{
    private static readonly string TrackedContentRoot =
        Path.Combine(AppContext.BaseDirectory, "content");

    [Fact]
    public void TrackedPackageAdmitsExactLogicalProjectionAndSyntheticCapability()
    {
        PublicSyntheticMap3PackageReader reader = new(TrackedContentRoot);

        MapScenarioAccepted accepted = AssertAccepted(reader.Admit(Request()));

        Assert.Equal("map3", accepted.Scenario.AdmissionFacts.CurrentMap.Value);
        Assert.Equal("map3", accepted.Scenario.AdmissionFacts.EgressMap.Value);
        Assert.Equal(new MapPosition(56, 3), accepted.Scenario.StartState.PlayerPosition);
        Assert.Equal((byte)3, accepted.Scenario.AdmissionFacts.OpaqueStartFacing);
        Assert.Equal("ms_map3", accepted.Scenario.AdmissionFacts.SetupIdentity);
        Assert.Equal("ms_map3_InitFunction", accepted.Scenario.AdmissionFacts.InitIdentity);
        Assert.True(accepted.Scenario.AdmissionFacts.NoProgramRequest);
        Assert.True(accepted.Scenario.AdmissionFacts.ExplorationReady);
        Assert.False(accepted.Receipt.ExactControlledAdmission);
        Assert.Equal(ContentProfile.PublicSynthetic, accepted.Receipt.Profile);
        Assert.Equal(
            [PublicSyntheticMap3PackageReader.EvidenceOwner],
            accepted.Receipt.EvidenceOwnerIds);
        Assert.Equal(
            [
                PublicSyntheticMap3PackageReader.Capability,
                PublicSyntheticMap3PackageReader.ContextCapability,
                PublicSyntheticMap3PackageReader.EventRequestCapability,
                PublicSyntheticMap3PackageReader.StateEffectCapability,
                PublicSyntheticMap3PackageReader.LocalTransitionCapability,
                PublicSyntheticMap3PackageReader.EntityInteractionCapability,
                PublicSyntheticMap3PackageReader.DialogueCapability,
                PublicSyntheticMap3PackageReader.FieldSearchCapability,
                PublicSyntheticMap3PackageReader.ItemAcquisitionCapability,
                PublicSyntheticMap3PackageReader.OutboundTransitionCapability,
            ],
            accepted.Receipt.Capabilities);
        string trackedDigest = Convert.ToHexString(
            SHA256.HashData(File.ReadAllBytes(PackagePath()))).ToLowerInvariant();
        Assert.Equal(PublicSyntheticMap3PackageReader.ExpectedContentDigest, trackedDigest);
        Assert.Equal(trackedDigest, accepted.Receipt.ContentDigest);
        Assert.Equal(2, accepted.Scenario.MapContext.MapRuntimes.Definitions.Count);
        MapExplorationRuntimeDefinition runtime =
            accepted.Scenario.MapContext.MapRuntimes.GetRequired(new MapId("map3"));
        Assert.Equal(accepted.Scenario.StartState.Map, runtime.Map);
        Assert.Same(runtime.Layout, accepted.Scenario.StartState.Layout);
        Assert.Same(runtime.Walkability, accepted.Scenario.StartState.Walkability);
        MapExplorationRuntimeDefinition outboundRuntime =
            accepted.Scenario.MapContext.MapRuntimes.GetRequired(
                new MapId("public-synthetic-outbound-shell"));
        Assert.NotSame(runtime.Layout, outboundRuntime.Layout);
        Assert.NotSame(runtime.Walkability, outboundRuntime.Walkability);
        Assert.True(outboundRuntime.Walkability.IsPassable(new MapPosition(1, 1)));
        Assert.False(outboundRuntime.Walkability.IsPassable(new MapPosition(0, 0)));
    }

    [Fact]
    public void TrackedPackageBuildsTypedSyntheticSetupAreaAndZoneSelectors()
    {
        PublicSyntheticMap3PackageReader reader = new(TrackedContentRoot);
        MapScenarioAccepted accepted = AssertAccepted(reader.Admit(Request()));

        MapScenarioContextDefinition context = accepted.Scenario.MapContext;
        MapExplorationRuntimeDefinition runtime =
            context.MapRuntimes.GetRequired(accepted.Scenario.StartState.Map);
        MapSetupId setup = context.SetupCatalog.Select(
            accepted.Scenario.StartState.Map,
            context.VoidSetup,
            context.IsInitiallySet);
        AreaDescriptionSelection area = MapAreaDescriptionSelector.Select(
            runtime.AreaDescriptions,
            new MapAreaDescriptionQuery(57, 3, AreaDescriptionAdmission.Ordinary));
        ZoneEventSelection zone = MapSetupEventSelector.Select(
            runtime.ZoneEvents,
            new ZoneEventQuery(57, 3));
        ZoneEventSelection fallbackZone = MapSetupEventSelector.Select(
            runtime.ZoneEvents,
            new ZoneEventQuery(56, 3));
        MapEventRequestDefinition? request = context.EventRequests.FindByTarget(zone.Target);
        MapEventEffectDefinition? effect = context.EventEffects.FindByRequest(request!.Request);
        ZoneEventSelection transitionZone = MapSetupEventSelector.Select(
            runtime.ZoneEvents,
            new ZoneEventQuery(58, 3));
        MapLocalTransitionDefinition? transition =
            context.LocalTransitions.FindByTarget(transitionZone.Target);
        MapEntityDefinition entity = Assert.Single(context.EntityInteractions.Entities);
        MapEntityInteractionDefinition interaction = Assert.Single(
            context.EntityInteractions.Interactions);
        MapDialogueDefinition dialogue = Assert.Single(context.Dialogues.Definitions);
        MapFieldSearchDefinition search = Assert.Single(context.FieldSearches.Definitions);
        MapItemAcquisitionDefinition acquisition = Assert.Single(
            context.ItemAcquisitions.Definitions);
        ZoneEventSelection outboundZone = MapSetupEventSelector.Select(
            runtime.ZoneEvents,
            new ZoneEventQuery(54, 4));
        MapOutboundTransitionDefinition outboundTransition = Assert.Single(
            context.OutboundTransitions.Definitions);
        MapExplorationRuntimeDefinition outboundRuntime = context.MapRuntimes.GetRequired(
            outboundTransition.DestinationMap);

        Assert.Equal("ms_map3", setup.Value);
        Assert.Equal(AreaDescriptionSelectionKind.Text, area.Kind);
        Assert.Equal(MapAreaDescriptionSelector.InvestigationTextIndexBase, area.InvestigationTextIndex);
        Assert.Equal(1000, area.DescriptionTextIndex);
        Assert.Equal("synthetic-map3-east-zone", zone.Target.Value);
        Assert.Equal("synthetic-no-zone", fallbackZone.Target.Value);
        Assert.NotNull(request);
        Assert.Equal("synthetic-map3-east-zone-request", request.Request.Value);
        Assert.Equal("synthetic-map3-east-zone-selected", request.Cue.Value);
        Assert.NotNull(effect);
        Assert.Equal("synthetic-map3-east-zone-variant-effect", effect.Effect.Value);
        Assert.Equal("synthetic-map3-variant-enabled", effect.Flag.Value);
        Assert.Equal("synthetic-map3-variant-applied", effect.Cue.Value);
        Assert.False(context.IsInitiallySet(effect.Flag));
        Assert.Equal("synthetic-map3-local-transition-zone", transitionZone.Target.Value);
        Assert.NotNull(transition);
        Assert.Equal("synthetic-map3-local-transition-request", transition.Request.Value);
        Assert.Equal("synthetic-map3-local-transition", transition.Transition.Value);
        Assert.Equal("map3", transition.SourceMap.Value);
        Assert.Equal(new MapPosition(58, 3), transition.SourcePosition);
        Assert.Equal("synthetic-map3-variant", transition.SourceSetup.Value);
        Assert.Equal("map3", transition.DestinationMap.Value);
        Assert.Equal(new MapPosition(55, 4), transition.DestinationPosition);
        Assert.Equal("synthetic-arrival-south", transition.DestinationOrientation.Value);
        Assert.Equal("synthetic-map3-local-transition-ready", transition.Cue.Value);
        Assert.Null(context.EventRequests.FindByTarget(transition.ZoneTarget));
        Assert.Equal(SemanticFacing.East, context.InitialFacing);
        Assert.Equal("synthetic-map3-placeholder-guide", entity.Entity.Value);
        Assert.Equal("map3", entity.Map.Value);
        Assert.Equal(new MapPosition(55, 3), entity.Position);
        Assert.Equal(
            "synthetic-map3-placeholder-guide-target",
            entity.InteractionTarget.Value);
        Assert.False(accepted.Scenario.StartState.Walkability.IsPassable(entity.Position));
        Assert.Equal("synthetic-map3-placeholder-guide-request", interaction.Request.Value);
        Assert.Equal(entity.InteractionTarget, interaction.Target);
        Assert.Equal("synthetic-map3-placeholder-guide-cue", interaction.Cue.Value);
        Assert.Same(entity, context.EntityInteractions.FindEntityAt(entity.Map, entity.Position));
        Assert.Same(interaction, context.EntityInteractions.FindByTarget(entity.InteractionTarget));
        Assert.Equal("synthetic-map3-placeholder-guide-dialogue", dialogue.Dialogue.Value);
        Assert.Equal(entity.InteractionTarget, dialogue.InteractionTarget);
        Assert.Collection(
            dialogue.Lines,
            line =>
            {
                Assert.Equal("synthetic-map3-placeholder-guide-line-1", line.Line.Value);
                Assert.Equal("Hello from a project-authored placeholder.", line.Text);
                Assert.Equal(
                    "synthetic-map3-placeholder-guide-line-1-presented",
                    line.Cue.Value);
            },
            line =>
            {
                Assert.Equal("synthetic-map3-placeholder-guide-line-2", line.Line.Value);
                Assert.Equal(
                    "This is synthetic text, not original game dialogue.",
                    line.Text);
                Assert.Equal(
                    "synthetic-map3-placeholder-guide-line-2-presented",
                    line.Cue.Value);
            });
        Assert.Equal(
            "synthetic-map3-placeholder-guide-dialogue-closed",
            dialogue.CloseCue.Value);
        Assert.Same(dialogue, context.Dialogues.FindByTarget(entity.InteractionTarget));
        Assert.Equal("synthetic-map3-arrival-search-context", search.Context.Value);
        Assert.Equal("synthetic-map3-field-search-request", search.Request.Value);
        Assert.Equal("synthetic-map3-field-search-result", search.Result.Value);
        Assert.Equal("synthetic-map3-placeholder-discovery", search.Discovery.Value);
        Assert.Equal("map3", search.Map.Value);
        Assert.Equal(new MapPosition(55, 4), search.Position);
        Assert.Equal("synthetic-map3-variant", search.Setup.Value);
        Assert.Equal("synthetic-no-zone", search.ZoneTarget.Value);
        Assert.Equal("synthetic-map3-field-search-pending", search.RequestCue.Value);
        Assert.Equal("synthetic-map3-placeholder-discovered", search.DiscoveryCue.Value);
        Assert.Same(
            search,
            context.FieldSearches.FindForSelection(
                search.Map,
                search.Position,
                search.Setup,
                search.ZoneTarget));
        Assert.Equal(search.Discovery, acquisition.Discovery);
        Assert.Equal(
            "synthetic-map3-placeholder-item-acquisition-request",
            acquisition.Request.Value);
        Assert.Equal(
            "synthetic-map3-placeholder-item-acquisition-result",
            acquisition.Result.Value);
        Assert.Equal(new ItemId("synthetic-map3-placeholder-item"), acquisition.Item);
        Assert.Equal(
            "synthetic-map3-placeholder-item-acquisition-pending",
            acquisition.RequestCue.Value);
        Assert.Equal(
            "synthetic-map3-placeholder-item-acquired",
            acquisition.AcquiredCue.Value);
        Assert.Same(
            acquisition,
            context.ItemAcquisitions.FindByDiscovery(search.Discovery));
        Assert.Equal("synthetic-map3-outbound-transition-zone", outboundZone.Target.Value);
        Assert.Equal(outboundZone.Target, outboundTransition.ZoneTarget);
        Assert.Equal("synthetic-map3-outbound-transition-request", outboundTransition.Request.Value);
        Assert.Equal("synthetic-map3-outbound-transition", outboundTransition.Transition.Value);
        Assert.Equal(new MapId("map3"), outboundTransition.SourceMap);
        Assert.Equal(new MapPosition(54, 4), outboundTransition.SourcePosition);
        Assert.Equal(new MapSetupId("synthetic-map3-variant"), outboundTransition.SourceSetup);
        Assert.Equal(
            new MapId("public-synthetic-outbound-shell"),
            outboundTransition.DestinationMap);
        Assert.Equal(new MapPosition(1, 1), outboundTransition.DestinationPosition);
        Assert.Equal(
            new MapSetupId("public-synthetic-outbound-shell-setup"),
            outboundTransition.DestinationSetup);
        Assert.Equal(SemanticFacing.East, outboundTransition.DestinationFacing);
        Assert.Equal(
            "synthetic-map3-outbound-transition-ready",
            outboundTransition.Cue.Value);
        Assert.Same(
            outboundTransition,
            context.OutboundTransitions.FindByTarget(outboundZone.Target));
        Assert.True(outboundRuntime.Walkability.IsPassable(
            outboundTransition.DestinationPosition));
        MapSetupId outboundSetup = context.SetupCatalog.Select(
            outboundTransition.DestinationMap,
            context.VoidSetup,
            context.IsInitiallySet);
        Assert.Equal(outboundTransition.DestinationSetup, outboundSetup);
        ZoneEventSelection outboundFallback = MapSetupEventSelector.Select(
            outboundRuntime.ZoneEvents,
            new ZoneEventQuery(1, 1));
        Assert.Equal("synthetic-outbound-shell-no-zone", outboundFallback.Target.Value);
    }

    [Theory]
    [InlineData("public-synthetic-map3-outbound-cross-map-transition-v1", "changed-outbound-capability")]
    [InlineData("public-synthetic-outbound-shell", "changed-outbound-shell")]
    [InlineData("synthetic-map3-outbound-transition-request", "changed-outbound-request")]
    [InlineData("synthetic-map3-outbound-transition", "changed-outbound-transition")]
    [InlineData("synthetic-map3-outbound-transition-zone", "changed-outbound-zone")]
    [InlineData("public-synthetic-outbound-shell-setup", "changed-outbound-setup")]
    [InlineData("synthetic-map3-outbound-transition-ready", "changed-outbound-cue")]
    public void OutboundIdentityOrCrossReferenceByteMutationFailsDigestAdmission(
        string oldValue,
        string newValue)
    {
        AssertDigestMismatch(original =>
            original.Replace(oldValue, newValue, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("\"destinationFacing\": \"east\"", "\"destinationFacing\": \"west\"")]
    [InlineData("\"x\": 54", "\"x\": 53")]
    [InlineData("\"width\": 2, \"height\": 2", "\"width\": 3, \"height\": 2")]
    public void OutboundPoseFacingOrRuntimeByteMutationFailsDigestAdmission(
        string oldValue,
        string newValue)
    {
        AssertDigestMismatch(original =>
            original.Replace(oldValue, newValue, StringComparison.Ordinal));
    }

    [Fact]
    public void TrackedPackageSupportsOneLegalAndOneSyntheticBlockedMove()
    {
        PublicSyntheticMap3PackageReader reader = new(TrackedContentRoot);
        MapScenarioAccepted accepted = AssertAccepted(reader.Admit(Request()));

        ExplorationMovementResult legal = ExplorationMovementReducer.TryMove(
            accepted.Scenario.StartState,
            new ExplorationMovementCommand(ExplorationDirection.East));
        ExplorationMovementResult blocked = ExplorationMovementReducer.TryMove(
            accepted.Scenario.StartState,
            new ExplorationMovementCommand(ExplorationDirection.North));

        Assert.Equal(ExplorationMovementOutcome.Moved, legal.Outcome);
        Assert.Equal(new MapPosition(57, 3), legal.State.PlayerPosition);
        Assert.Equal(ExplorationMovementOutcome.BlockedByTerrain, blocked.Outcome);
        Assert.Same(accepted.Scenario.StartState, blocked.State);
    }

    [Fact]
    public void UnknownPackageAndPrivateProfileFailClosed()
    {
        PublicSyntheticMap3PackageReader reader = new(TrackedContentRoot);

        MapScenarioRejected unknown = Assert.IsType<MapScenarioRejected>(
            reader.Admit(new MapScenarioRequest("unknown", ContentProfile.PublicSynthetic)));
        MapScenarioRejected privateProfile = Assert.IsType<MapScenarioRejected>(
            reader.Admit(new MapScenarioRequest(
                PublicSyntheticMap3PackageReader.PackageId,
                ContentProfile.PrivateLocal)));

        Assert.Equal(ScenarioAdmissionFailureCode.PackageIdentityMismatch, unknown.Diagnostic.Code);
        Assert.Equal(ScenarioAdmissionFailureCode.ProfileMismatch, privateProfile.Diagnostic.Code);
    }

    [Fact]
    public void MissingPackageFailsClosed()
    {
        string contentRoot = CreateTemporaryContentRoot("{}");
        File.Delete(Path.Combine(
            contentRoot,
            PublicSyntheticMap3PackageReader.PackageId + ".json"));
        try
        {
            PublicSyntheticMap3PackageReader reader = new(contentRoot);

            MapScenarioRejected rejected = Assert.IsType<MapScenarioRejected>(
                reader.Admit(Request()));

            Assert.Equal(ScenarioAdmissionFailureCode.PackageUnavailable, rejected.Diagnostic.Code);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Theory]
    [InlineData("\"sourcePath\": \"C:\\\\private\\\\content.payload\",", null)]
    [InlineData("\"exactControlledAdmission\": false", "\"exactControlledAdmission\": true")]
    [InlineData("\"x\": 54", "\"x\": 64")]
    public void UnknownPrivateExactOrOutOfBoundsContentFailsClosed(
        string oldValue,
        string? newValue = null)
    {
        string original = File.ReadAllText(PackagePath(), System.Text.Encoding.UTF8);
        string modified = newValue is null
            ? original.Replace("{\n", "{\n  " + oldValue + "\n", StringComparison.Ordinal)
            : original.Replace(oldValue, newValue, StringComparison.Ordinal);
        string contentRoot = CreateTemporaryContentRoot(modified);
        try
        {
            PublicSyntheticMap3PackageReader reader = new(contentRoot);

            Assert.IsType<MapScenarioRejected>(reader.Admit(Request()));
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public void OtherwiseValidLayoutMutationUnderSamePackageIdFailsDigestAdmission()
    {
        AssertDigestMismatch(
            original => original.Replace(
                "\"x\": 54, \"y\": 1, \"word\": 1",
                "\"x\": 54, \"y\": 1, \"word\": 2",
                StringComparison.Ordinal));
    }

    [Fact]
    public void OtherwiseValidWalkabilityMutationUnderSamePackageIdFailsDigestAdmission()
    {
        AssertDigestMismatch(
            original => original.Replace(
                "\"x\": 56, \"y\": 2",
                "\"x\": 55, \"y\": 2",
                StringComparison.Ordinal));
    }

    [Fact]
    public void OtherwiseValidEventRequestCrossReferenceMutationFailsDigestAdmission()
    {
        AssertDigestMismatch(
            original => original.Replace(
                "\"zoneTargetId\": \"synthetic-map3-east-zone\"",
                "\"zoneTargetId\": \"synthetic-no-zone\"",
            StringComparison.Ordinal));
    }

    [Fact]
    public void OtherwiseValidEventEffectCrossReferenceMutationFailsDigestAdmission()
    {
        AssertDigestMismatch(
            original => original.Replace(
                "\"flagId\": \"synthetic-map3-variant-enabled\"",
                "\"flagId\": \"synthetic-map3-missing-variant\"",
            StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(
        "\"zoneTargetId\": \"synthetic-map3-local-transition-zone\"",
        "\"zoneTargetId\": \"synthetic-no-zone\"")]
    [InlineData(
        "\"sourceMapId\": \"map3\"",
        "\"sourceMapId\": \"missing-map\"")]
    [InlineData(
        "\"x\": 58,\n          \"y\": 3",
        "\"x\": 57,\n          \"y\": 3")]
    [InlineData(
        "\"destinationMapId\": \"map3\"",
        "\"destinationMapId\": \"missing-map\"")]
    [InlineData(
        "\"x\": 55,\n          \"y\": 4",
        "\"x\": 56,\n          \"y\": 2")]
    public void LocalTransitionIdentityOrCrossReferenceByteMutationFailsDigestAdmission(
        string oldValue,
        string newValue)
    {
        AssertDigestMismatch(
            original => original.Replace(oldValue, newValue, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(
        "\"initialSemanticFacing\": \"east\"",
        "\"initialSemanticFacing\": \"default\"")]
    [InlineData(
        "\"entityId\": \"synthetic-map3-placeholder-guide\"",
        "\"entityId\": \"synthetic-map3-placeholder-guide-copy\"")]
    [InlineData(
        "\"x\": 55,\n          \"y\": 3",
        "\"x\": 55,\n          \"y\": 4")]
    [InlineData(
        "\"interactionTargetId\": \"synthetic-map3-placeholder-guide-target\"",
        "\"interactionTargetId\": \"missing-target\"")]
    [InlineData(
        "\"kind\": \"specific\"",
        "\"kind\": \"default\"")]
    [InlineData(
        "\"targetId\": \"synthetic-map3-placeholder-guide-target\"",
        "\"targetId\": \"missing-target\"")]
    public void EntityIdentityPoseOrCrossReferenceByteMutationFailsDigestAdmission(
        string oldValue,
        string newValue)
    {
        AssertDigestMismatch(
            original => original.Replace(oldValue, newValue, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(
        "\"kind\": \"specific\",\n        \"dialogueId\"",
        "\"kind\": \"default\",\n        \"dialogueId\"")]
    [InlineData(
        "\"dialogueId\": \"synthetic-map3-placeholder-guide-dialogue\"",
        "\"dialogueId\": \"default\"")]
    [InlineData(
        "\"interactionTargetId\": \"synthetic-map3-placeholder-guide-target\"",
        "\"interactionTargetId\": \"missing-target\"")]
    [InlineData(
        "\"lineId\": \"synthetic-map3-placeholder-guide-line-2\"",
        "\"lineId\": \"synthetic-map3-placeholder-guide-line-1\"")]
    [InlineData(
        "\"text\": \"Hello from a project-authored placeholder.\"",
        "\"text\": \"\"")]
    [InlineData(
        "\"cueId\": \"synthetic-map3-placeholder-guide-line-2-presented\"",
        "\"cueId\": \"synthetic-map3-placeholder-guide-line-1-presented\"")]
    [InlineData(
        "\"closeCueId\": \"synthetic-map3-placeholder-guide-dialogue-closed\"",
        "\"closeCueId\": \"synthetic-map3-placeholder-guide-cue\"")]
    public void DialogueIdentityTextCueOrCrossReferenceByteMutationFailsDigestAdmission(
        string oldValue,
        string newValue)
    {
        AssertDigestMismatch(
            original => original.Replace(oldValue, newValue, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(
        "\"kind\": \"specific\",\n        \"contextId\"",
        "\"kind\": \"default\",\n        \"contextId\"")]
    [InlineData(
        "\"contextId\": \"synthetic-map3-arrival-search-context\"",
        "\"contextId\": \"default\"")]
    [InlineData(
        "\"requestId\": \"synthetic-map3-field-search-request\"",
        "\"requestId\": \"default\"")]
    [InlineData(
        "\"resultId\": \"synthetic-map3-field-search-result\"",
        "\"resultId\": \"default\"")]
    [InlineData(
        "\"discoveryId\": \"synthetic-map3-placeholder-discovery\"",
        "\"discoveryId\": \"default\"")]
    [InlineData(
        "\"mapId\": \"map3\",\n        \"position\"",
        "\"mapId\": \"missing-map\",\n        \"position\"")]
    [InlineData(
        "\"x\": 55,\n          \"y\": 4",
        "\"x\": 56,\n          \"y\": 2")]
    [InlineData(
        "\"setupId\": \"synthetic-map3-variant\"",
        "\"setupId\": \"missing-setup\"")]
    [InlineData(
        "\"zoneTargetId\": \"synthetic-no-zone\"",
        "\"zoneTargetId\": \"missing-zone\"")]
    [InlineData(
        "\"discoveryCueId\": \"synthetic-map3-placeholder-discovered\"",
        "\"discoveryCueId\": \"synthetic-map3-field-search-pending\"")]
    public void FieldSearchIdentityLocationOrCrossReferenceByteMutationFailsDigestAdmission(
        string oldValue,
        string newValue)
    {
        AssertDigestMismatch(
            original => original.Replace(oldValue, newValue, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(
        "\"kind\": \"specific\",\n        \"discoveryId\"",
        "\"kind\": \"default\",\n        \"discoveryId\"")]
    [InlineData(
        "\"discoveryId\": \"synthetic-map3-placeholder-discovery\",\n        \"requestId\": \"synthetic-map3-placeholder-item-acquisition-request\"",
        "\"discoveryId\": \"missing-discovery\",\n        \"requestId\": \"synthetic-map3-placeholder-item-acquisition-request\"")]
    [InlineData(
        "\"requestId\": \"synthetic-map3-placeholder-item-acquisition-request\"",
        "\"requestId\": \"default\"")]
    [InlineData(
        "\"resultId\": \"synthetic-map3-placeholder-item-acquisition-result\"",
        "\"resultId\": \"default\"")]
    [InlineData(
        "\"itemId\": \"synthetic-map3-placeholder-item\"",
        "\"itemId\": \"default\"")]
    [InlineData(
        "\"requestCueId\": \"synthetic-map3-placeholder-item-acquisition-pending\"",
        "\"requestCueId\": \"synthetic-map3-placeholder-discovered\"")]
    [InlineData(
        "\"acquiredCueId\": \"synthetic-map3-placeholder-item-acquired\"",
        "\"acquiredCueId\": \"synthetic-map3-placeholder-item-acquisition-pending\"")]
    [InlineData(
        "\"acquiredCueId\": \"synthetic-map3-placeholder-item-acquired\"",
        "\"acquiredCueId\": \"synthetic-map3-placeholder-item-acquired\",\n        \"unknownField\": true")]
    public void ItemAcquisitionIdentityCueOrCrossReferenceByteMutationFailsDigestAdmission(
        string oldValue,
        string newValue)
    {
        AssertDigestMismatch(
            original => original.Replace(oldValue, newValue, StringComparison.Ordinal));
    }

    [Fact]
    public void WhitespaceOnlyByteMutationUnderSamePackageIdFailsDigestAdmission()
    {
        AssertDigestMismatch(original => original + "\n");
    }

    private static MapScenarioRequest Request() =>
        new(
            PublicSyntheticMap3PackageReader.PackageId,
            ContentProfile.PublicSynthetic);

    private static MapScenarioAccepted AssertAccepted(MapScenarioAdmissionResult result)
    {
        if (result is MapScenarioRejected rejected)
        {
            Assert.Fail(
                $"Admission failed: {rejected.Diagnostic.Code} " +
                $"{rejected.Diagnostic.Field} {rejected.Diagnostic.Message}");
        }

        return Assert.IsType<MapScenarioAccepted>(result);
    }

    private static void AssertDigestMismatch(Func<string, string> mutate)
    {
        string original = File.ReadAllText(PackagePath(), System.Text.Encoding.UTF8);
        string modified = mutate(original);
        Assert.NotEqual(original, modified);
        string contentRoot = CreateTemporaryContentRoot(modified);
        try
        {
            PublicSyntheticMap3PackageReader reader = new(contentRoot);

            MapScenarioRejected rejected = Assert.IsType<MapScenarioRejected>(
                reader.Admit(Request()));

            Assert.Equal(
                ScenarioAdmissionFailureCode.ContentDigestMismatch,
                rejected.Diagnostic.Code);
            Assert.Equal("contentDigest", rejected.Diagnostic.Field);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    private static string PackagePath() =>
        Path.Combine(
            TrackedContentRoot,
            PublicSyntheticMap3PackageReader.PackageId + ".json");

    private static string CreateTemporaryContentRoot(string document)
    {
        string root = Path.Combine(
            Path.GetTempPath(),
            "sf2-content-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(
            Path.Combine(root, PublicSyntheticMap3PackageReader.PackageId + ".json"),
            document,
            System.Text.Encoding.UTF8);
        return root;
    }
}
