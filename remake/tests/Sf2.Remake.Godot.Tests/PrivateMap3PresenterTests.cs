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
                "WASD semantic movement",
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
                "random choices Unknown",
            status);
        Assert.DoesNotContain("510", status, StringComparison.Ordinal);
        Assert.DoesNotContain("dialogue", status, StringComparison.OrdinalIgnoreCase);
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
            zone601: Zone601(map));
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

    private static OriginalMapEntityPopulation ProjectAuthoredEntityPopulation(MapId map) =>
        new(
            map,
            new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
            [
                new OriginalMapEntityDefinition(
                    new OriginalMapEntityRecordIdentity(
                        "project-authored-private-presenter-entities",
                        1),
                    rawX: 1,
                    rawY: 1,
                    opaqueFacing: 0,
                    mapSprite: 0,
                    [0, 0, 0, 0]),
                new OriginalMapEntityDefinition(
                    new OriginalMapEntityRecordIdentity(
                        "project-authored-private-presenter-entities",
                        2),
                    rawX: 2,
                    rawY: 2,
                    opaqueFacing: 0,
                    mapSprite: 0,
                    [0, 0, 0, 0]),
                new OriginalMapEntityDefinition(
                    new OriginalMapEntityRecordIdentity(
                        "project-authored-private-presenter-entities",
                        3),
                    rawX: 5,
                    rawY: 6,
                    opaqueFacing: 0,
                    mapSprite: 195,
                    [0, 4, 97, 2]),
            ]);

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
}
