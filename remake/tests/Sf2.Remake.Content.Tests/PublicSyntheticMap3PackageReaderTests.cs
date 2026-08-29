using System.Security.Cryptography;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Content;
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
            ],
            accepted.Receipt.Capabilities);
        string trackedDigest = Convert.ToHexString(
            SHA256.HashData(File.ReadAllBytes(PackagePath()))).ToLowerInvariant();
        Assert.Equal(PublicSyntheticMap3PackageReader.ExpectedContentDigest, trackedDigest);
        Assert.Equal(trackedDigest, accepted.Receipt.ContentDigest);
    }

    [Fact]
    public void TrackedPackageBuildsTypedSyntheticSetupAreaAndZoneSelectors()
    {
        PublicSyntheticMap3PackageReader reader = new(TrackedContentRoot);
        MapScenarioAccepted accepted = AssertAccepted(reader.Admit(Request()));

        MapScenarioContextDefinition context = accepted.Scenario.MapContext;
        MapSetupId setup = context.SetupCatalog.Select(
            accepted.Scenario.StartState.Map,
            context.VoidSetup,
            context.IsInitiallySet);
        AreaDescriptionSelection area = MapAreaDescriptionSelector.Select(
            context.AreaDescriptions,
            new MapAreaDescriptionQuery(57, 3, AreaDescriptionAdmission.Ordinary));
        ZoneEventSelection zone = MapSetupEventSelector.Select(
            context.ZoneEvents,
            new ZoneEventQuery(57, 3));
        ZoneEventSelection fallbackZone = MapSetupEventSelector.Select(
            context.ZoneEvents,
            new ZoneEventQuery(56, 3));
        MapEventRequestDefinition? request = context.EventRequests.FindByTarget(zone.Target);
        MapEventEffectDefinition? effect = context.EventEffects.FindByRequest(request!.Request);
        ZoneEventSelection transitionZone = MapSetupEventSelector.Select(
            context.ZoneEvents,
            new ZoneEventQuery(58, 3));
        MapLocalTransitionDefinition? transition =
            context.LocalTransitions.FindByTarget(transitionZone.Target);

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
