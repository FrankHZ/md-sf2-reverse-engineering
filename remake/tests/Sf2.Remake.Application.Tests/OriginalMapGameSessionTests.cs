using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class OriginalMapGameSessionTests
{
    [Fact]
    public void ExactAcceptedImportStartsASeparatedPrivateMap3Session()
    {
        AcceptedSource source = new(Accepted());

        PrivateOriginalMapGameSessionStarted started =
            Assert.IsType<PrivateOriginalMapGameSessionStarted>(
                GameSession.StartPrivateOriginalMap(source, Request()));

        Assert.Equal(1, source.AdmitCalls);
        Assert.Equal(ContentProfile.PrivateLocal, started.Session.PrivateOriginalMapSnapshot.Profile);
        Assert.Equal(GameFlowStage.Exploration, started.Session.PrivateOriginalMapSnapshot.FlowStage);
        Assert.Equal(new MapId("map3"), started.Session.PrivateOriginalMapSnapshot.Map);
        Assert.Equal(new MapPosition(56, 3),
            started.Session.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.Equal(0, started.Session.PrivateOriginalMapSnapshot.SimulationStep);
        Assert.Null(started.Session.PrivateOriginalMapSnapshot.LastTraversal);
        Assert.Equal(OriginalMapRuntimeAdmission.AcceptedContentDigest,
            started.Receipt.ContentDigest);
        Assert.Throws<InvalidOperationException>(() => _ = started.Session.Snapshot);
    }

    [Fact]
    public void GameSessionOwnsMovedCollisionAndActiveAreaOutcomes()
    {
        ushort[] words = EmptyWords();
        words[Index(56, 4)] = OriginalMapTraversal.CollisionMask;
        GameSession session = Start(Definition(words));

        PrivateOriginalMapMoveApplied moved = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Equal(OriginalMapTraversalOutcome.Moved, moved.Traversal.Outcome);
        Assert.Equal(new MapPosition(57, 3), moved.Snapshot.PlayerPosition);
        Assert.Equal(1, moved.Snapshot.SimulationStep);

        PrivateOriginalMapMoveApplied outside = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Equal(OriginalMapTraversalOutcome.Moved, outside.Traversal.Outcome);
        Assert.Equal(new MapPosition(58, 3), outside.Snapshot.PlayerPosition);

        PrivateOriginalMapMoveApplied blockedArea = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Equal(
            OriginalMapTraversalOutcome.BlockedOutsideActiveArea,
            blockedArea.Traversal.Outcome);
        Assert.Equal(new MapPosition(58, 3), blockedArea.Snapshot.PlayerPosition);
        Assert.Equal(3, blockedArea.Snapshot.SimulationStep);

        GameSession collisionSession = Start(Definition(words));
        WorkingMapLayout collisionLayout =
            collisionSession.PrivateOriginalMapSnapshot.WorkingLayout;
        PrivateOriginalMapMoveApplied blockedCollision =
            collisionSession.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.South));
        Assert.Equal(
            OriginalMapTraversalOutcome.BlockedByCollision,
            blockedCollision.Traversal.Outcome);
        Assert.Equal(new MapPosition(56, 3), blockedCollision.Snapshot.PlayerPosition);
        Assert.Same(collisionLayout, blockedCollision.Snapshot.WorkingLayout);
    }

    [Fact]
    public void LayoutBoundaryBlocksWithoutChangingTheAuthoritativePosition()
    {
        GameSession session = Start(Definition(
            EmptyWords(),
            new OriginalMapTraversalArea(0, 0, 63, 63)));
        for (int index = 0; index < 3; index++)
        {
            Assert.Equal(
                OriginalMapTraversalOutcome.Moved,
                session.ApplyPrivateOriginalMap(
                    new MoveExplorationCommand(ExplorationDirection.North)).Traversal.Outcome);
        }

        PrivateOriginalMapMoveApplied blocked = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.North));

        Assert.Equal(OriginalMapTraversalOutcome.BlockedByBoundary, blocked.Traversal.Outcome);
        Assert.Equal(new MapPosition(56, 0), blocked.Snapshot.PlayerPosition);
        Assert.Equal(4, blocked.Snapshot.SimulationStep);
    }

    [Fact]
    public void DirectionalStairMovementUsesTheAcceptedOriginalPolicy()
    {
        ushort[] words = EmptyWords();
        words[Index(56, 3)] = OriginalMapTraversal.RightStairMask;
        words[Index(57, 2)] = OriginalMapTraversal.RightStairMask;
        GameSession session = Start(Definition(words));

        PrivateOriginalMapMoveApplied moved = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));

        Assert.Equal(OriginalMapTraversalOutcome.Moved, moved.Traversal.Outcome);
        Assert.Equal(new MapPosition(57, 2), moved.Snapshot.PlayerPosition);
        Assert.Equal(OriginalMapTraversal.RightStairMask, moved.Traversal.SourceWord);
        Assert.Equal(OriginalMapTraversal.RightStairMask, moved.Traversal.DestinationWord);
    }

    [Fact]
    public void RestartReconstructsTheControlledStartWithoutPersistence()
    {
        AcceptedSource source = new(Accepted());
        PrivateOriginalMapGameSessionStarted first =
            Assert.IsType<PrivateOriginalMapGameSessionStarted>(
                GameSession.StartPrivateOriginalMap(source, Request()));
        Assert.IsType<PrivateOriginalMapMoveApplied>(
            first.Session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.East)));

        PrivateOriginalMapGameSessionStarted restarted =
            Assert.IsType<PrivateOriginalMapGameSessionStarted>(
                GameSession.StartPrivateOriginalMap(source, Request()));

        Assert.Equal(2, source.AdmitCalls);
        Assert.Equal(new MapPosition(56, 3),
            restarted.Session.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.Equal(0, restarted.Session.PrivateOriginalMapSnapshot.SimulationStep);
        Assert.Null(restarted.Session.PrivateOriginalMapSnapshot.LastTraversal);
    }

    [Fact]
    public void RequestTrustRootRejectsBeforeCallingTheSource()
    {
        AcceptedSource source = new(Accepted());

        PrivateOriginalMapGameSessionStartRejected wrongPackage =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    source,
                    new OriginalMapImportRequest(
                        "other-package",
                        ContentProfile.PrivateLocal,
                        OriginalMapRuntimeAdmission.AcceptedContentDigest)));
        Assert.Equal(
            OriginalMapImportFailureCode.PackageIdentityMismatch,
            wrongPackage.Diagnostic.Code);

        PrivateOriginalMapGameSessionStartRejected wrongProfile =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    source,
                    new OriginalMapImportRequest(
                        OriginalMapRuntimeAdmission.PackageId,
                        ContentProfile.PublicSynthetic,
                        OriginalMapRuntimeAdmission.AcceptedContentDigest)));
        Assert.Equal(OriginalMapImportFailureCode.ProfileMismatch, wrongProfile.Diagnostic.Code);

        PrivateOriginalMapGameSessionStartRejected wrongDigest =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    source,
                    new OriginalMapImportRequest(
                        OriginalMapRuntimeAdmission.PackageId,
                        ContentProfile.PrivateLocal,
                        new string('0', 64))));
        Assert.Equal(
            OriginalMapImportFailureCode.ContentDigestMismatch,
            wrongDigest.Diagnostic.Code);
        Assert.Equal(0, source.AdmitCalls);
    }

    [Fact]
    public void AcceptedSourceReceiptMustRetainExactDigestAndCapabilities()
    {
        OriginalMapImportDefinition definition = Definition(EmptyWords());
        PrivateOriginalMapGameSessionStartRejected wrongDigest =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    new AcceptedSource(Accepted(
                        definition,
                        Receipt(contentDigest: new string('1', 64)))),
                    Request()));
        Assert.Equal(
            OriginalMapImportFailureCode.ContentDigestMismatch,
            wrongDigest.Diagnostic.Code);

        PrivateOriginalMapGameSessionStartRejected missingCapability =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    new AcceptedSource(Accepted(
                        definition,
                        Receipt(capabilities:
                            [OriginalMapRuntimeAdmission.ImportCapability]))),
                    Request()));
        Assert.Equal(
            OriginalMapImportFailureCode.MissingReference,
            missingCapability.Diagnostic.Code);
    }

    [Fact]
    public void AcceptedSourceReceiptMustRetainExactCanonicalProvenanceAndEvidenceOwners()
    {
        OriginalMapImportDefinition definition = Definition(EmptyWords());
        AssertRejectedReceipt(
            definition,
            Receipt(romSha256: new string('A', 64)),
            OriginalMapImportFailureCode.ProvenanceMismatch);
        AssertRejectedReceipt(
            definition,
            Receipt(upstreamRepository: "https://example.invalid/incompatible.git"),
            OriginalMapImportFailureCode.ProvenanceMismatch);
        AssertRejectedReceipt(
            definition,
            Receipt(upstreamCommit: new string('b', 40)),
            OriginalMapImportFailureCode.ProvenanceMismatch);
        AssertRejectedReceipt(
            definition,
            Receipt(evidenceOwnerIds:
                OriginalMapRuntimeAdmission.RequiredEvidenceOwners.Skip(1)),
            OriginalMapImportFailureCode.ProvenanceMismatch);
        AssertRejectedReceipt(
            definition,
            Receipt(evidenceOwnerIds:
                OriginalMapRuntimeAdmission.RequiredEvidenceOwners.Append(
                    "incompatible-extra-owner")),
            OriginalMapImportFailureCode.ProvenanceMismatch);

        PrivateOriginalMapGameSessionStarted reordered =
            Assert.IsType<PrivateOriginalMapGameSessionStarted>(
                GameSession.StartPrivateOriginalMap(
                    new AcceptedSource(Accepted(
                        definition,
                        Receipt(evidenceOwnerIds:
                            OriginalMapRuntimeAdmission.RequiredEvidenceOwners.Reverse()))),
                    Request()));
        Assert.Equal(new MapId(OriginalMapRuntimeAdmission.MapId),
            reordered.Session.PrivateOriginalMapSnapshot.Map);
    }

    [Fact]
    public void AcceptedSourceReceiptMustRetainExactMapProjectionDigests()
    {
        OriginalMapImportDefinition definition = Definition(EmptyWords());
        AssertRejectedReceipt(
            definition,
            Receipt(decodedLayoutDigest: new string('2', 64)),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            definition,
            Receipt(collisionProjectionDigest: new string('3', 64)),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void TypedSourceRejectionPassesThroughWithoutCreatingASession()
    {
        OriginalMapImportDiagnostic diagnostic = new(
            OriginalMapImportFailureCode.PackageUnavailable,
            "package",
            "The ignored package is unavailable.");

        PrivateOriginalMapGameSessionStartRejected rejected =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(new RejectedSource(diagnostic), Request()));

        Assert.Same(diagnostic, rejected.Diagnostic);
    }

    [Fact]
    public void PrivateOriginalSessionRejectsThePublicSyntheticApplySurface()
    {
        GameSession session = Start(Definition(EmptyWords()));

        Assert.Throws<InvalidOperationException>(() =>
            session.Apply(new MoveExplorationCommand(ExplorationDirection.East)));
    }

    private static GameSession Start(OriginalMapImportDefinition definition) =>
        Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(
                new AcceptedSource(Accepted(definition)),
                Request())).Session;

    private static OriginalMapImportAccepted Accepted(
        OriginalMapImportDefinition? definition = null,
        OriginalMapImportReceipt? receipt = null) =>
        new(definition ?? Definition(EmptyWords()), receipt ?? Receipt());

    private static OriginalMapImportDefinition Definition(
        ushort[] words,
        OriginalMapTraversalArea? activeArea = null)
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        return new OriginalMapImportDefinition(
            map,
            new WorkingMapLayout(words),
            new OriginalMapTraversal(
                [activeArea ?? new OriginalMapTraversalArea(55, 2, 58, 4)]),
            new OriginalMapControlledAdmission(
                map,
                new MapPosition(
                    OriginalMapRuntimeAdmission.StartX,
                    OriginalMapRuntimeAdmission.StartY),
                OriginalMapRuntimeAdmission.OpaqueStartFacing,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SelectedInitIdentity,
                noProgramRequest: true),
            ["natural-route-and-effects-unknown"]);
    }

    private static OriginalMapImportReceipt Receipt(
        string? contentDigest = null,
        IEnumerable<string>? capabilities = null,
        string? romSha256 = null,
        string? upstreamRepository = null,
        string? upstreamCommit = null,
        string? decodedLayoutDigest = null,
        string? collisionProjectionDigest = null,
        IEnumerable<string>? evidenceOwnerIds = null) =>
        new(
            OriginalMapRuntimeAdmission.PackageId,
            OriginalMapRuntimeAdmission.SchemaVersion,
            contentDigest ?? OriginalMapRuntimeAdmission.AcceptedContentDigest,
            decodedLayoutDigest ?? OriginalMapRuntimeAdmission.AcceptedDecodedLayoutDigest,
            collisionProjectionDigest ??
                OriginalMapRuntimeAdmission.AcceptedCollisionProjectionDigest,
            ContentProfile.PrivateLocal,
            new OriginalMapImportProvenance(
                OriginalMapRuntimeAdmission.PackageId,
                romSha256 ?? OriginalMapRuntimeAdmission.AcceptedRomSha256,
                upstreamRepository ?? OriginalMapRuntimeAdmission.AcceptedUpstreamRepository,
                upstreamCommit ?? OriginalMapRuntimeAdmission.AcceptedUpstreamCommit),
            evidenceOwnerIds ?? OriginalMapRuntimeAdmission.RequiredEvidenceOwners,
            capabilities ?? OriginalMapRuntimeAdmission.RequiredCapabilities);

    private static void AssertRejectedReceipt(
        OriginalMapImportDefinition definition,
        OriginalMapImportReceipt receipt,
        OriginalMapImportFailureCode expectedCode)
    {
        PrivateOriginalMapGameSessionStartRejected rejected =
            Assert.IsType<PrivateOriginalMapGameSessionStartRejected>(
                GameSession.StartPrivateOriginalMap(
                    new AcceptedSource(Accepted(definition, receipt)),
                    Request()));
        Assert.Equal(expectedCode, rejected.Diagnostic.Code);
    }

    private static OriginalMapImportRequest Request() =>
        new(
            OriginalMapRuntimeAdmission.PackageId,
            ContentProfile.PrivateLocal,
            OriginalMapRuntimeAdmission.AcceptedContentDigest);

    private static ushort[] EmptyWords() => new ushort[WorkingMapLayout.WordCount];

    private static int Index(int x, int y) =>
        (y * WorkingMapLayout.ColumnCount) + x;

    private sealed class AcceptedSource(OriginalMapImportAccepted accepted) : IOriginalMapImportSource
    {
        public int AdmitCalls { get; private set; }

        public OriginalMapImportResult Admit(OriginalMapImportRequest request)
        {
            AdmitCalls++;
            return accepted;
        }
    }

    private sealed class RejectedSource(OriginalMapImportDiagnostic diagnostic) :
        IOriginalMapImportSource
    {
        public OriginalMapImportResult Admit(OriginalMapImportRequest request) =>
            new OriginalMapImportRejected(diagnostic);
    }
}
