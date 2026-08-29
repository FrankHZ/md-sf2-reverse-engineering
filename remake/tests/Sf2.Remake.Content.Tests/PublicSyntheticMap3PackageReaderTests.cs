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
            [PublicSyntheticMap3PackageReader.Capability],
            accepted.Receipt.Capabilities);
        Assert.Equal(64, accepted.Receipt.ContentDigest.Length);
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
