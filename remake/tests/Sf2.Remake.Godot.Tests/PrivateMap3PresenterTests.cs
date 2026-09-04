using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class PrivateMap3PresenterTests
{
    [Fact]
    public void AvailablePrivateLocalPlanPreservesTheExactDiagnosticPresentation()
    {
        PrivateMap3PresentationPlan plan =
            PrivateMap3PresentationPlan.PrivateLocalAvailable();

        Assert.Equal(Map3Root.PrivateBannerText, plan.BannerText);
        Assert.Equal(
            "Project-authored traversal diagnostics from accepted Domain policy. " +
                "Original presentation remains unavailable.",
            plan.ExplanationText);
        Assert.Equal(
            "Admitting PrivateLocal canonical Map 3...",
            plan.InitialStatus);
        Assert.True(plan.IncludeTraversalViewport);
        Assert.True(plan.ShowTraversalViewport);
        Assert.False(plan.IncludeBaseVisualViewport);
        Assert.Equal(450, plan.StatusY);
    }

    [Fact]
    public void BaseVisualPlanKeepsDiagnosticsForSmokeButShowsOnlyTheProjectAuthoredView()
    {
        PrivateMap3PresentationPlan plan =
            PrivateMap3PresentationPlan.PrivateLocalWithBaseVisual();

        Assert.Equal(Map3Root.PrivateBannerText, plan.BannerText);
        Assert.Equal(
            "Project-authored base composition from admitted private Map 3 data. " +
                "Not full original fidelity.",
            plan.ExplanationText);
        Assert.True(plan.IncludeTraversalViewport);
        Assert.False(plan.ShowTraversalViewport);
        Assert.True(plan.IncludeBaseVisualViewport);
        Assert.Equal(PrivateMap3WorldTreatment.ExactNearest, plan.WorldTreatment);
        Assert.False(plan.StaticOverlayDiagnostic);
        Assert.False(plan.CurrentAreaOverlay);
        Assert.Equal(310, plan.StatusY);
    }

    [Fact]
    public void CurrentAreaOverlayPlanKeepsPlayableMarkersAndLabelsModernComposition()
    {
        PrivateMap3PresentationPlan plan =
            PrivateMap3PresentationPlan.PrivateLocalWithBaseVisual(
                currentAreaOverlay: true);

        Assert.True(plan.IncludeBaseVisualViewport);
        Assert.False(plan.StaticOverlayDiagnostic);
        Assert.True(plan.CurrentAreaOverlay);
        Assert.Equal(
            "Project-authored current-area second-layer composition from admitted private Map 3 data. " +
                "Not original layer priority, timing, or full fidelity.",
            plan.ExplanationText);
    }

    [Fact]
    public void StaticAndCurrentAreaOverlayPlansAreMutuallyExclusive()
    {
        Assert.Throws<ArgumentException>(() =>
            PrivateMap3PresentationPlan.PrivateLocalWithBaseVisual(
                staticOverlayDiagnostic: true,
                currentAreaOverlay: true));
    }

    [Fact]
    public void EdgeScale2xPlanRetainsTheExplicitTreatmentWithoutMovingPresentationRules()
    {
        PrivateMap3PresentationPlan plan =
            PrivateMap3PresentationPlan.PrivateLocalWithBaseVisual(
                PrivateMap3WorldTreatment.EdgeScale2x);

        Assert.True(plan.IncludeBaseVisualViewport);
        Assert.Equal(PrivateMap3WorldTreatment.EdgeScale2x, plan.WorldTreatment);
        Assert.Equal(
            "Project-authored base composition from admitted private Map 3 data. " +
                "Not full original fidelity.",
            plan.ExplanationText);
    }

    [Fact]
    public void PrivateStatusWrapsInsideTheLeftColumnBeforeTheBattlePanel()
    {
        float rightEdge = PrivateMap3Presenter.StatusX +
            PrivateMap3Presenter.StatusSize.X;

        Assert.Equal(24, PrivateMap3Presenter.StatusX);
        Assert.Equal(new global::Godot.Vector2(552, 96), PrivateMap3Presenter.StatusSize);
        Assert.True(
            rightEdge < PublicSyntheticBattlePresenter.PanelBounds.Position.X);
        Assert.Equal(
            24,
            PublicSyntheticBattlePresenter.PanelBounds.Position.X - rightEdge);
        Assert.Equal(
            global::Godot.TextServer.AutowrapMode.WordSmart,
            PrivateMap3Presenter.StatusAutowrapMode);
    }

    [Fact]
    public void UnavailablePlansPreserveTheirExactDisclosureWithoutCreatingAViewport()
    {
        PrivateMap3PresentationPlan privatePlan =
            PrivateMap3PresentationPlan.PrivateLocalUnavailable(
                "Project-authored unavailable diagnostic.");
        PrivateMap3PresentationPlan profilePlan =
            PrivateMap3PresentationPlan.ProfileUnavailable(
            "Project-authored unavailable diagnostic.");

        Assert.Equal(Map3Root.PrivateBannerText, privatePlan.BannerText);
        Assert.Equal("PROFILE UNAVAILABLE — NO FALLBACK", profilePlan.BannerText);
        Assert.Equal(
            "Project-authored traversal diagnostics from accepted Domain policy. " +
                "Original presentation remains unavailable.",
            privatePlan.ExplanationText);
        foreach (PrivateMap3PresentationPlan plan in new[] { privatePlan, profilePlan })
        {
            Assert.Equal(
                "Unavailable: Project-authored unavailable diagnostic.",
                plan.InitialStatus);
            Assert.False(plan.IncludeTraversalViewport);
            Assert.False(plan.ShowTraversalViewport);
            Assert.False(plan.IncludeBaseVisualViewport);
            Assert.False(plan.StaticOverlayDiagnostic);
            Assert.False(plan.CurrentAreaOverlay);
            Assert.Equal(105, plan.StatusY);
        }
    }

    [Fact]
    public void BaseAtlasConsumerIsAnExplicitBoundedPresenterSurface()
    {
        System.Reflection.MethodInfo? bind = typeof(PrivateMap3Presenter).GetMethod(
            "TryBindBaseAtlas",
            System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);

        Assert.NotNull(bind);
        Assert.Equal(typeof(bool), bind.ReturnType);
        Assert.Equal(
            new[]
            {
                typeof(PrivateLocalPresentationRasterMount),
                typeof(PrivateOriginalMapSessionSnapshot),
                typeof(PrivateLocalPresentationAssetMountDiagnostic).MakeByRefType(),
            },
            bind.GetParameters().Select(parameter => parameter.ParameterType));
        System.Reflection.MethodInfo? playerBind = typeof(PrivateMap3Presenter).GetMethod(
            "TryBindPlayerReference",
            System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(playerBind);
        Assert.Equal(typeof(bool), playerBind.ReturnType);
        Assert.Equal(
            new[]
            {
                typeof(PrivateLocalPresentationRasterMount),
                typeof(PrivateLocalPresentationAssetMountDiagnostic).MakeByRefType(),
            },
            playerBind.GetParameters().Select(parameter => parameter.ParameterType));
        System.Reflection.MethodInfo? entityBind = typeof(PrivateMap3Presenter).GetMethod(
            "TryBindEntity142Diagnostic",
            System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(entityBind);
        Assert.Equal(typeof(bool), entityBind.ReturnType);
        Assert.Equal(
            new[]
            {
                typeof(PrivateLocalPresentationRasterMount),
                typeof(PrivateOriginalMapSessionSnapshot),
                typeof(PrivateLocalPresentationAssetMountDiagnostic).MakeByRefType(),
            },
            entityBind.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            "private-local-map3-entity142-half0-diagnostic-idle-consumer-v1",
            PrivateMap3Entity142DiagnosticProjection.Capability);
        Assert.Equal(
            "project-authored-half0-diagnostic-idle-v1",
            PrivateMap3Entity142DiagnosticProjection.Policy);
        Assert.Equal(
            "SF2_MAP3_PRIVATE_LOCAL_BASE_ATLAS_SMOKE ",
            Map3Root.PrivateBaseAtlasSmokeMarker);
        Assert.Equal(
            "private-local-map3-base-atlas-diagnostic-consumer-v1",
            Map3Root.PrivateBaseAtlasCapability);
        Assert.Equal(
            "SF2_MAP3_PRIVATE_LOCAL_WORLD_TREATMENT_SMOKE ",
            Map3Root.PrivateWorldTreatmentSmokeMarker);
        Assert.Equal(
            "private-local-map3-edge-scale2x-world-treatment-v1",
            Map3Root.PrivateWorldTreatmentCapability);
        Assert.DoesNotContain(
            typeof(Map3Root).GetFields(
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic),
            field => field.Name.Contains("PlayerReferenceSmoke", StringComparison.Ordinal));
        Assert.DoesNotContain(
            typeof(Map3Root).GetFields(
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic),
            field => field.Name.Contains("Entity142", StringComparison.Ordinal));
        Assert.DoesNotContain(
            typeof(PrivateMap3Presenter).GetProperties(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic),
            property => property.Name.Contains("Path", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Contains("Root", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Contains("Pixel", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SnapshotFormattingAndTypedViewportProjectionRemainExact()
    {
        PrivateOriginalMapSessionSnapshot snapshot = Snapshot();

        string status = PrivateMap3PresentationPlan.FormatStatus(snapshot, "Moved");
        PrivateOriginalMapTraversalViewProjection projection =
            PrivateOriginalMapTraversalViewProjection.Create(snapshot);

        Assert.Equal(
            "Map map3  Tile (56, 3)  Area 2  Step 7  Moved  |  " +
                "WASD semantic movement  |  Sarah Ready; actor 1 at " +
                "(1, 1), facing 3; temporary route flag clear; F semantic interaction " +
                "request  |  Entity142 logical 142/slot 17 at (54, 17), facing 1; " +
                "flags261/602 clear; no pending request; F request / G acknowledge",
            status);
        Assert.Equal(new MapId(OriginalMapRuntimeAdmission.MapId), projection.Map);
        Assert.Equal(50, projection.OriginX);
        Assert.Equal(0, projection.OriginY);
        Assert.Equal(6, projection.PlayerColumn);
        Assert.Equal(3, projection.PlayerRow);
    }

    [Fact]
    public void CompletedZone601ProjectsOnlyTypedPersistentStateAndKeepsRandomPathUnknown()
    {
        PrivateOriginalMapSessionSnapshot ready = Snapshot();
        OriginalMapZone601Definition definition = ready.Definition.Zone601!;
        PrivateOriginalMapZone601State completed =
            (PrivateOriginalMapZone601State)typeof(PrivateOriginalMapZone601State)
                .GetMethod(
                    "Complete",
                    System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.NonPublic)!
                .Invoke(null, [definition])!;
        PrivateOriginalMapSessionSnapshot snapshot = new(
            ready.Definition,
            ready.Receipt,
            ready.WorkingLayout,
            ready.SimulationStep,
            ready.PlayerPosition,
            ready.LastTraversal,
            ready.ControlledStepCopyApplied,
            ready.LastLayoutMutation,
            zone601: completed);

        string status = PrivateMap3PresentationPlan.FormatStatus(snapshot, "Moved");

        Assert.Equal(
            "Map map3  Tile (56, 3)  Area 2  Step 7  Moved  |  " +
                "WASD semantic movement  |  Zone601 complete; actor 128 at " +
                "(5, 4), facing 2; ambient center (5, 6) range 1; " +
                "random choices Unknown  |  Sarah Ready; actor 1 at " +
                "(1, 1), facing 3; temporary route flag clear; F semantic interaction " +
                "request  |  Entity142 logical 142/slot 17 at (54, 17), facing 1; " +
                "flags261/602 clear; no pending request; F request / G acknowledge",
            status);
        Assert.DoesNotContain("510", status, StringComparison.Ordinal);
        Assert.DoesNotContain("dialogue", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MessengerCompletionPresentationUsesOnlyTypedReleasedRouteState()
    {
        PrivateOriginalMapSessionSnapshot ready = Snapshot();
        Assert.Equal(
            "F semantic interaction request",
            PrivateMap3PresentationPlan.SarahAction(ready.Sarah!));
        Assert.Equal(
            "no pending request; F request / G acknowledge",
            PrivateMap3PresentationPlan.Entity142Action(
                ready.Entity142!,
                "no pending request"));

        PrivateOriginalMapSarahState follower = MessengerFollowerSarah(ready.Sarah!);
        PrivateOriginalMapEntity142State released = ReleasedEntity142(ready.Entity142!);
        Assert.Equal(
            "follower ready; route occupancy released",
            PrivateMap3PresentationPlan.SarahAction(follower));
        Assert.Equal(
            "route occupancy released; immutable sprite diagnostic only",
            PrivateMap3PresentationPlan.Entity142Action(released, "ignored pending label"));
    }

    [Fact]
    public void CastleGatePresentationUsesOnlyTypedPersistentState()
    {
        OriginalMapCastleGateDefinition definition = CastleGateDefinition();
        PrivateOriginalMapCastleGateState ready = InvokeCastleGateState("Ready", definition);
        PrivateOriginalMapCastleGateState opened = InvokeCastleGateState("Completed", definition);

        Assert.Equal(
            "Castle gate closed; bounded event ready only after messenger acceptance",
            PrivateMap3PresentationPlan.CastleGateStatus(ready));
        string status = PrivateMap3PresentationPlan.CastleGateStatus(opened);
        Assert.Equal(
            "Castle gate open; flag604 set; bounded opening admission complete; " +
                "source dialogue/facing/restoration, timing, and presentation Unknown",
            status);
        Assert.DoesNotContain("text", status, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("asset", status, StringComparison.OrdinalIgnoreCase);
    }

    private static PrivateOriginalMapSessionSnapshot Snapshot()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        WorkingMapLayout layout = new(words);
        OriginalMapAreaCatalog areaCatalog = new(
        [
            Area("project-authored-private-presenter-areas", 1, new(0, 0, 10, 10)),
            Area("project-authored-private-presenter-areas", 2, new(0, 0, 63, 63)),
        ]);
        OriginalMapImportDefinition definition = new(
            map,
            layout,
            new OriginalMapBlockCatalog(
            [
                new OriginalMapBlockDefinition(
                    new OriginalMapBlockRecordIdentity(
                        "project-authored-private-presenter-blocks",
                        zeroBasedBlockIndex: 0),
                    new ushort[OriginalMapBlockDefinition.OpaqueWordCount]),
            ]),
            areaCatalog,
            ProjectAuthoredEntityPopulation(map),
            new OriginalMapVisualResourceSelection(
                map,
                paletteIndex: 0,
                [0, 37, 43, 53, 66]),
            new OriginalMapControlledAdmission(
                map,
                new MapPosition(56, 3),
                OriginalMapRuntimeAdmission.OpaqueStartFacing,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SelectedInitIdentity,
                noProgramRequest: true),
            controlledStepCopy: null,
            sameMapWarps: null,
            unsupportedCapabilities: ["natural-route-and-presentation-unknown"],
            zone601: Zone601(map),
            sarah: Sarah(map),
            entity142: Entity142(map));
        OriginalMapImportReceipt receipt = new(
            OriginalMapRuntimeAdmission.PackageId,
            OriginalMapRuntimeAdmission.SchemaVersion,
            OriginalMapRuntimeAdmission.AcceptedContentDigest,
            OriginalMapRuntimeAdmission.AcceptedDecodedLayoutDigest,
            OriginalMapRuntimeAdmission.AcceptedCollisionProjectionDigest,
            ContentProfile.PrivateLocal,
            new OriginalMapImportProvenance(
                OriginalMapRuntimeAdmission.PackageId,
                OriginalMapRuntimeAdmission.AcceptedRomSha256,
                OriginalMapRuntimeAdmission.AcceptedUpstreamRepository,
                OriginalMapRuntimeAdmission.AcceptedUpstreamCommit),
            OriginalMapRuntimeAdmission.RequiredEvidenceOwners,
            OriginalMapRuntimeAdmission.RequiredCapabilities);
        OriginalMapTraversalResult lastTraversal = new(
            new MapPosition(55, 3),
            new MapPosition(56, 3),
            ExplorationDirection.East,
            OriginalMapTraversalOutcome.Moved,
            sourceWord: 0,
            destinationWord: 0);
        return new PrivateOriginalMapSessionSnapshot(
            definition,
            receipt,
            layout,
            simulationStep: 7,
            new MapPosition(56, 3),
            lastTraversal,
            controlledStepCopyApplied: false,
            lastLayoutMutation: null);
    }

    private static OriginalMapCastleGateDefinition CastleGateDefinition()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        return new OriginalMapCastleGateDefinition(
            new OriginalMapZoneEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.CastleGateZoneEventResourceId,
                OriginalMapRuntimeAdmission.CastleGateZoneEventRecordOrdinal,
                OriginalMapRuntimeAdmission.CastleGateZoneEventTargetIdentity),
            new MapPosition(31, 6),
            ExplorationDirection.North,
            new MapPosition(31, 5),
            OriginalMapRuntimeAdmission.CastleGateProgramIdentity,
            OriginalMapRuntimeAdmission.CastleGateControlShapeSha256,
            OriginalMapRuntimeAdmission.CastleGateTextCursorId,
            OriginalMapRuntimeAdmission.CastleGateCompletionFlag604,
            OriginalMapRuntimeAdmission.CastleGateSourceProgramOperationCount,
            OriginalMapRuntimeAdmission.CastleGateProjectionSourceOperationIndices,
            OriginalMapRuntimeAdmission.CastleGateGuardMoves,
            OriginalMapRuntimeAdmission.CastleGateStages);
    }

    private static PrivateOriginalMapCastleGateState InvokeCastleGateState(
        string method,
        OriginalMapCastleGateDefinition definition) =>
        (PrivateOriginalMapCastleGateState)typeof(PrivateOriginalMapCastleGateState)
            .GetMethod(
                method,
                System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.NonPublic)!
            .Invoke(null, [definition])!;

    private static OriginalMapEntityPopulation ProjectAuthoredEntityPopulation(MapId map) =>
        new(
            map,
            new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
            Enumerable.Range(1, OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal)
                .Select(ordinal => ordinal switch
                {
                    1 => EntityRecord(ordinal, rawX: 1, rawY: 1, opaqueFacing: 3),
                    3 => EntityRecord(
                        ordinal,
                        rawX: 5,
                        rawY: 6,
                        mapSprite: 195,
                        opaqueTail: [0, 4, 97, 2]),
                    OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal =>
                        EntityRecord(
                            ordinal,
                            rawX: OriginalMapRuntimeAdmission.Entity142ActorX,
                            rawY: OriginalMapRuntimeAdmission.Entity142ActorY,
                            opaqueFacing:
                                OriginalMapRuntimeAdmission.Entity142ActorOpaqueFacing,
                            mapSprite: OriginalMapRuntimeAdmission.Entity142ActorMapSprite,
                            opaqueTail: [0, 4, 96, 206]),
                    _ => EntityRecord(ordinal, rawX: ordinal, rawY: 30),
                }));

    private static OriginalMapEntityDefinition EntityRecord(
        int ordinal,
        int rawX,
        int rawY,
        byte opaqueFacing = 0,
        byte mapSprite = 0,
        IEnumerable<byte>? opaqueTail = null) =>
        new(
            new OriginalMapEntityRecordIdentity(
                "project-authored-private-presenter-entities",
                ordinal),
            checked((byte)rawX),
            checked((byte)rawY),
            opaqueFacing,
            mapSprite,
            opaqueTail ?? [0, 0, 0, 0]);

    private static OriginalMapZone601Definition Zone601(MapId map) =>
        new(
            new OriginalMapZoneEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                "project-authored-private-presenter-zone-events",
                1,
                "project-authored-zone601-target"),
            new MapPosition(4, 4),
            601,
            "project-authored-zone601-sequence",
            new OriginalMapEntityRecordIdentity(
                "project-authored-private-presenter-entities",
                3),
            128,
            new MapPosition(5, 6),
            0,
            "project-authored-init-slow",
            new MapPosition(5, 4),
            2,
            20,
            [510, 511, 483],
            "project-authored-ambient-walking",
            new MapPosition(5, 6),
            1,
            OriginalMapRuntimeAdmission.Zone601BlockingStages);

    private static OriginalMapSarahDefinition Sarah(MapId map) =>
        new(
            new OriginalMapSarahEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                "project-authored-private-presenter-entity-events",
                1,
                "project-authored-sarah-target",
                opaqueEventFacing: 3),
            new OriginalMapEntityRecordIdentity(
                "project-authored-private-presenter-entities",
                1),
            logicalActorId: 1,
            new MapPosition(1, 1),
            actorInitialOpaqueFacing: 3,
            new MapPosition(1, 2),
            playerInteractionOpaqueFacing: 1,
            laterBranchFlag603: 603,
            laterBranchFlag602: 602,
            temporaryRouteFlag256: 256,
            "project-authored-sarah-sequence",
            new MapPosition(2, 1),
            restoredOpaqueFacing: 3,
            [512, 480, 481],
            [480, 481],
            OriginalMapRuntimeAdmission.SarahFirstStages,
            OriginalMapRuntimeAdmission.SarahRepeatStages);

    private static OriginalMapEntity142Definition Entity142(MapId map) =>
        new(
            new OriginalMapEntity142EventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.Entity142EventResourceId,
                OriginalMapRuntimeAdmission.Entity142EventRecordOrdinal,
                OriginalMapRuntimeAdmission.Entity142EventTargetIdentity,
                OriginalMapRuntimeAdmission.Entity142EventOpaqueFacing),
            new OriginalMapEntityRecordIdentity(
                "project-authored-private-presenter-entities",
                OriginalMapRuntimeAdmission.Entity142ActorSourceRecordOrdinal),
            OriginalMapRuntimeAdmission.Entity142LogicalActorId,
            OriginalMapRuntimeAdmission.Entity142PhysicalActorSlot,
            new MapPosition(
                OriginalMapRuntimeAdmission.Entity142ActorX,
                OriginalMapRuntimeAdmission.Entity142ActorY),
            OriginalMapRuntimeAdmission.Entity142ActorOpaqueFacing,
            new MapPosition(
                OriginalMapRuntimeAdmission.Entity142PlayerInteractionX,
                OriginalMapRuntimeAdmission.Entity142PlayerInteractionY),
            OriginalMapRuntimeAdmission.Entity142PlayerInteractionOpaqueFacing,
            OriginalMapRuntimeAdmission.Entity142FirstInteractionFlag261,
            OriginalMapRuntimeAdmission.Entity142CompletionFlag602,
            OriginalMapRuntimeAdmission.Entity142FirstTextIds,
            OriginalMapRuntimeAdmission.Entity142RepeatTextIds,
            OriginalMapRuntimeAdmission.Entity142FirstStages,
            OriginalMapRuntimeAdmission.Entity142RepeatStages);

    private static OriginalMapAreaDefinition Area(
        string resourceId,
        int ordinal,
        OriginalMapTraversalArea area) =>
        new(
            new OriginalMapAreaRecordIdentity(resourceId, ordinal),
            area,
            new OriginalMapAreaWordPair(0, 0),
            new OriginalMapAreaWordPair(0, 0),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaBytePair(0, 0),
            new OriginalMapAreaBytePair(0, 0),
            mainLayerType: 0,
            defaultMusic: 0);

    internal static PrivateOriginalMapSarahState MessengerFollowerSarah(
        PrivateOriginalMapSarahState ready)
    {
        System.Reflection.ConstructorInfo constructor =
            typeof(PrivateOriginalMapSarahState).GetConstructors(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 7);
        return (PrivateOriginalMapSarahState)constructor.Invoke(
        [
            PrivateOriginalMapSarahLifecyclePhase.MessengerFollowerReady,
            ready.ActorSourceRecord,
            ready.LogicalActorId,
            new MapPosition(41, 10),
            (byte)1,
            true,
            true,
        ]);
    }

    private static PrivateOriginalMapEntity142State ReleasedEntity142(
        PrivateOriginalMapEntity142State ready)
    {
        System.Reflection.ConstructorInfo constructor =
            typeof(PrivateOriginalMapEntity142State).GetConstructors(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)
            .Single(candidate => candidate.GetParameters().Length == 9);
        return (PrivateOriginalMapEntity142State)constructor.Invoke(
        [
            ready.ActorSourceRecord,
            ready.LogicalActorId,
            ready.PhysicalActorSlot,
            ready.ActorPosition,
            ready.ActorOpaqueFacing,
            true,
            true,
            1L,
            true,
        ]);
    }
}
