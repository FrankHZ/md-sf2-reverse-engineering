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
        Assert.Equal(310, plan.StatusY);
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
            Assert.Equal(105, plan.StatusY);
        }
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
            unsupportedCapabilities: ["natural-route-and-presentation-unknown"]);
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
