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
        Assert.Equal(
            OriginalMapRuntimeAdmission.ControlledStartAreaRecordOrdinal,
            started.Session.PrivateOriginalMapSnapshot.CurrentArea.OneBasedRecordOrdinal);
        Assert.Equal(
            OriginalMapRuntimeAdmission.ControlledStartAreaRecordOrdinal,
            started.Session.PrivateOriginalMapSnapshot.CurrentAreaDefinition
                .Identity.OneBasedRecordOrdinal);
        Assert.Equal(0, started.Session.PrivateOriginalMapSnapshot.SimulationStep);
        Assert.Null(started.Session.PrivateOriginalMapSnapshot.LastTraversal);
        Assert.Null(started.Session.PrivateOriginalMapSnapshot.LastLayoutMutation);
        Assert.False(started.Session.PrivateOriginalMapSnapshot.ControlledStepCopyApplied);
        Assert.Same(
            started.Session.PrivateOriginalMapSnapshot.Definition.WorkingLayout,
            started.Session.PrivateOriginalMapSnapshot.WorkingLayout);
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

        PrivateOriginalMapMoveApplied moved = null!;
        for (int expectedX = 57; expectedX <= 61; expectedX++)
        {
            moved = session.ApplyPrivateOriginalMap(
                new MoveExplorationCommand(ExplorationDirection.East));
            Assert.Equal(OriginalMapTraversalOutcome.Moved, moved.Traversal.Outcome);
            Assert.Equal(new MapPosition(expectedX, 3), moved.Snapshot.PlayerPosition);
            Assert.Equal(2, moved.Snapshot.CurrentArea.OneBasedRecordOrdinal);
        }

        PrivateOriginalMapMoveApplied blockedArea = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.East));
        Assert.Equal(
            OriginalMapTraversalOutcome.BlockedOutsideActiveArea,
            blockedArea.Traversal.Outcome);
        Assert.Equal(new MapPosition(61, 3), blockedArea.Snapshot.PlayerPosition);
        Assert.Equal(6, blockedArea.Snapshot.SimulationStep);

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
        GameSession session = Start(Definition(EmptyWords()));
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
    public void CurrentAreaRecomputesFromPositionAndSurvivesBlockedMutationAndRestartBoundaries()
    {
        ushort[] words = EmptyWords();
        words[Index(49, 3)] = OriginalMapTraversal.CollisionMask;
        OriginalMapImportAccepted accepted = Accepted(Definition(words));
        AcceptedSource source = new(accepted);
        GameSession session = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;

        Assert.Equal(2, session.PrivateOriginalMapSnapshot.CurrentArea.OneBasedRecordOrdinal);
        Assert.Same(
            session.PrivateOriginalMapSnapshot.Definition.AreaCatalog.Records[1],
            session.PrivateOriginalMapSnapshot.CurrentAreaDefinition);
        for (int index = 0; index < 6; index++)
        {
            Assert.Equal(
                OriginalMapTraversalOutcome.Moved,
                session.ApplyPrivateOriginalMap(
                    new MoveExplorationCommand(ExplorationDirection.West)).Traversal.Outcome);
        }

        PrivateOriginalMapSessionSnapshot crossed = session.PrivateOriginalMapSnapshot;
        Assert.Equal(new MapPosition(50, 3), crossed.PlayerPosition);
        Assert.Equal(1, crossed.CurrentArea.OneBasedRecordOrdinal);
        Assert.Same(crossed.Definition.AreaCatalog.Records[0], crossed.CurrentAreaDefinition);

        PrivateOriginalMapMoveApplied blocked = session.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.West));
        Assert.Equal(OriginalMapTraversalOutcome.BlockedByCollision, blocked.Traversal.Outcome);
        Assert.Equal(1, blocked.Snapshot.CurrentArea.OneBasedRecordOrdinal);
        Assert.Same(crossed.CurrentAreaDefinition, blocked.Snapshot.CurrentAreaDefinition);

        PrivateOriginalMapLayoutMutationApplied mutated =
            Assert.IsType<PrivateOriginalMapLayoutMutationApplied>(
                session.ApplyPrivateOriginalMapLayoutMutation(MutationCommand(blocked.Snapshot)));
        Assert.Equal(1, mutated.Snapshot.CurrentArea.OneBasedRecordOrdinal);
        Assert.Same(crossed.CurrentAreaDefinition, mutated.Snapshot.CurrentAreaDefinition);
        Assert.Equal(blocked.Snapshot.PlayerPosition, mutated.Snapshot.PlayerPosition);

        GameSession restarted = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        Assert.Equal(new MapPosition(56, 3), restarted.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.Equal(2, restarted.PrivateOriginalMapSnapshot.CurrentArea.OneBasedRecordOrdinal);
        Assert.Same(
            restarted.PrivateOriginalMapSnapshot.Definition.AreaCatalog.Records[1],
            restarted.PrivateOriginalMapSnapshot.CurrentAreaDefinition);
    }

    [Fact]
    public void ExactControlledStepCopyMutatesOnlyTheAuthoritativeSessionLayout()
    {
        ushort[] words = EmptyWords();
        words[Index(41, 13)] = OriginalMapTraversal.CollisionMask;
        OriginalMapImportDefinition definition = Definition(words);
        GameSession session = Start(definition);
        PrivateOriginalMapSessionSnapshot before = session.PrivateOriginalMapSnapshot;

        PrivateOriginalMapLayoutMutationApplied applied =
            Assert.IsType<PrivateOriginalMapLayoutMutationApplied>(
                session.ApplyPrivateOriginalMapLayoutMutation(
                    MutationCommand(before)));

        Assert.Same(applied.Snapshot, session.PrivateOriginalMapSnapshot);
        Assert.NotSame(definition.WorkingLayout, applied.Snapshot.WorkingLayout);
        Assert.True(OriginalMapTraversal.IsBlocked(
            definition.WorkingLayout,
            new MapPosition(41, 13)));
        Assert.False(OriginalMapTraversal.IsBlocked(
            applied.Snapshot.WorkingLayout,
            new MapPosition(41, 13)));
        Assert.Equal(1, applied.Snapshot.SimulationStep);
        Assert.True(applied.Snapshot.ControlledStepCopyApplied);
        Assert.Null(applied.Snapshot.LastTraversal);
        Assert.Same(applied.Receipt, applied.Snapshot.LastLayoutMutation);
        Assert.Equal(
            PrivateOriginalMapCollisionCategory.BlockedByAcceptedCollisionClass,
            applied.Receipt.BeforeCollision);
        Assert.Equal(
            PrivateOriginalMapCollisionCategory.ActiveNonBlocked,
            applied.Receipt.AfterCollision);
        Assert.Equal((62, 0, 41, 13, 1, 1), Geometry(applied.Receipt.Copy));
        Assert.DoesNotContain(
            applied.Receipt.GetType().GetProperties(),
            property => property.Name.Contains("Word", StringComparison.Ordinal) ||
                property.Name.Contains("Path", StringComparison.Ordinal) ||
                property.Name.Contains("Payload", StringComparison.Ordinal));
    }

    [Fact]
    public void WrongStaleAndDuplicateMutationCommandsAreZeroMutation()
    {
        ushort[] words = EmptyWords();
        words[Index(41, 13)] = OriginalMapTraversal.CollisionMask;
        GameSession session = Start(Definition(words));
        PrivateOriginalMapSessionSnapshot initial = session.PrivateOriginalMapSnapshot;
        OriginalMapStepCopyIdentity exact = initial.Definition.ControlledStepCopy!.Identity;

        foreach (OriginalMapStepCopyIdentity wrong in new[]
        {
            new OriginalMapStepCopyIdentity(
                ContentProfile.PublicSynthetic,
                exact.Map,
                exact.SourceResourceId,
                exact.OneBasedRecordOrdinal),
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                new MapId("other-map"),
                exact.SourceResourceId,
                exact.OneBasedRecordOrdinal),
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                exact.Map,
                "OtherStepEvents",
                exact.OneBasedRecordOrdinal),
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                exact.Map,
                exact.SourceResourceId,
                exact.OneBasedRecordOrdinal - 1),
        })
        {
            AssertRejectedMutation(
                session,
                new ApplyPrivateOriginalMapLayoutMutationCommand(wrong, 0),
                initial,
                PrivateOriginalMapLayoutMutationFailureCode.ReferenceMismatch);
        }

        AssertRejectedMutation(
            session,
            new ApplyPrivateOriginalMapLayoutMutationCommand(exact, 1),
            initial,
            PrivateOriginalMapLayoutMutationFailureCode.StaleSimulationStep);

        PrivateOriginalMapLayoutMutationApplied applied =
            Assert.IsType<PrivateOriginalMapLayoutMutationApplied>(
                session.ApplyPrivateOriginalMapLayoutMutation(
                    new ApplyPrivateOriginalMapLayoutMutationCommand(exact, 0)));
        AssertRejectedMutation(
            session,
            new ApplyPrivateOriginalMapLayoutMutationCommand(exact, 1),
            applied.Snapshot,
            PrivateOriginalMapLayoutMutationFailureCode.AlreadyApplied);
    }

    [Fact]
    public void MovementReadsMutatedLayoutAndRestartRestoresTheAdmittedLayout()
    {
        ushort[] words = EmptyWords();
        words[Index(41, 13)] = OriginalMapTraversal.CollisionMask;
        OriginalMapImportAccepted accepted = Accepted(Definition(words));
        AcceptedSource source = new(accepted);
        GameSession first = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;

        for (int index = 0; index < 15; index++)
        {
            Assert.Equal(
                OriginalMapTraversalOutcome.Moved,
                first.ApplyPrivateOriginalMap(
                    new MoveExplorationCommand(ExplorationDirection.West)).Traversal.Outcome);
        }

        for (int index = 0; index < 9; index++)
        {
            Assert.Equal(
                OriginalMapTraversalOutcome.Moved,
                first.ApplyPrivateOriginalMap(
                    new MoveExplorationCommand(ExplorationDirection.South)).Traversal.Outcome);
        }

        Assert.Equal(new MapPosition(41, 12), first.PrivateOriginalMapSnapshot.PlayerPosition);
        Assert.IsType<PrivateOriginalMapLayoutMutationApplied>(
            first.ApplyPrivateOriginalMapLayoutMutation(
                MutationCommand(first.PrivateOriginalMapSnapshot)));
        PrivateOriginalMapMoveApplied entered = first.ApplyPrivateOriginalMap(
            new MoveExplorationCommand(ExplorationDirection.South));
        Assert.Equal(OriginalMapTraversalOutcome.Moved, entered.Traversal.Outcome);
        Assert.Equal(new MapPosition(41, 13), entered.Snapshot.PlayerPosition);
        Assert.True(entered.Snapshot.ControlledStepCopyApplied);
        Assert.Null(entered.Snapshot.LastLayoutMutation);

        GameSession restarted = Assert.IsType<PrivateOriginalMapGameSessionStarted>(
            GameSession.StartPrivateOriginalMap(source, Request())).Session;
        Assert.True(OriginalMapTraversal.IsBlocked(
            restarted.PrivateOriginalMapSnapshot.WorkingLayout,
            new MapPosition(41, 13)));
        Assert.False(restarted.PrivateOriginalMapSnapshot.ControlledStepCopyApplied);
        Assert.Equal(0, restarted.PrivateOriginalMapSnapshot.SimulationStep);
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
    public void AcceptedSourceDefinitionMustRetainTheExactControlledStepCopy()
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        WorkingMapLayout layout = new(EmptyWords());
        OriginalMapAreaCatalog areaCatalog = AcceptedAreaCatalog();
        OriginalMapControlledAdmission controlled = new(
            map,
            new MapPosition(
                OriginalMapRuntimeAdmission.StartX,
                OriginalMapRuntimeAdmission.StartY),
            OriginalMapRuntimeAdmission.OpaqueStartFacing,
            new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
            OriginalMapRuntimeAdmission.SelectedInitIdentity,
            noProgramRequest: true);
        OriginalMapImportDefinition missing = new(
            map,
            layout,
            areaCatalog,
            controlled,
            ["natural-route-and-effects-unknown"]);
        OriginalMapImportDefinition wrongIdentity = new(
            map,
            layout,
            areaCatalog,
            controlled,
            new OriginalMapStepCopyDefinition(
                new OriginalMapStepCopyIdentity(
                    ContentProfile.PrivateLocal,
                    map,
                    "OtherStepEvents",
                    OriginalMapRuntimeAdmission.ControlledStepCopyRecordOrdinal),
                new MapPosition(41, 13),
                new WorkingMapBlockCopy(62, 0, 41, 13, 1, 1)),
            ["natural-route-and-effects-unknown"]);

        AssertRejectedReceipt(
            missing,
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            wrongIdentity,
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
    }

    [Fact]
    public void AcceptedSourceDefinitionMustRetainExactOrderedAreaProjection()
    {
        OriginalMapTraversalArea[] changedBounds = AcceptedAreas();
        changedBounds[0] = new OriginalMapTraversalArea(0, 0, 49, 31);
        OriginalMapTraversalArea[] reordered = AcceptedAreas();
        (reordered[0], reordered[1]) = (reordered[1], reordered[0]);
        OriginalMapAreaDefinition[] changedSourceRecords = AcceptedAreaDefinitions();
        changedSourceRecords[1] = AcceptedAreaDefinition(
            oneBasedRecordOrdinal: 2,
            area: changedSourceRecords[1].MainLayerBounds,
            defaultMusic: 9);

        AssertRejectedReceipt(
            Definition(EmptyWords(), AcceptedAreaCatalog(changedBounds)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(EmptyWords(), AcceptedAreaCatalog(reordered)),
            Receipt(),
            OriginalMapImportFailureCode.InvalidMapProjection);
        AssertRejectedReceipt(
            Definition(EmptyWords(), new OriginalMapAreaCatalog(changedSourceRecords)),
            Receipt(),
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
        OriginalMapAreaCatalog? areaCatalog = null)
    {
        MapId map = new(OriginalMapRuntimeAdmission.MapId);
        ushort[] admittedWords = [.. words];
        admittedWords[Index(
            OriginalMapRuntimeAdmission.ControlledStepCopyDestinationX,
            OriginalMapRuntimeAdmission.ControlledStepCopyDestinationY)] |=
            OriginalMapTraversal.CollisionMask;
        admittedWords[Index(
            OriginalMapRuntimeAdmission.ControlledStepCopySourceX,
            OriginalMapRuntimeAdmission.ControlledStepCopySourceY)] &=
            unchecked((ushort)~OriginalMapTraversal.CollisionMask);
        return new OriginalMapImportDefinition(
            map,
            new WorkingMapLayout(admittedWords),
            areaCatalog ?? AcceptedAreaCatalog(),
            new OriginalMapControlledAdmission(
                map,
                new MapPosition(
                    OriginalMapRuntimeAdmission.StartX,
                    OriginalMapRuntimeAdmission.StartY),
                OriginalMapRuntimeAdmission.OpaqueStartFacing,
                new MapSetupId(OriginalMapRuntimeAdmission.SelectedSetupId),
                OriginalMapRuntimeAdmission.SelectedInitIdentity,
                noProgramRequest: true),
            ControlledStepCopy(map),
            ["natural-route-and-effects-unknown"]);
    }

    private static OriginalMapTraversalArea[] AcceptedAreas() =>
    [
        new OriginalMapTraversalArea(0, 0, 50, 31),
        new OriginalMapTraversalArea(51, 0, 61, 9),
        new OriginalMapTraversalArea(51, 10, 61, 19),
    ];

    private static OriginalMapAreaCatalog AcceptedAreaCatalog(
        IEnumerable<OriginalMapTraversalArea>? activeAreas = null) =>
        new((activeAreas ?? AcceptedAreas()).Select(
            (area, index) => AcceptedAreaDefinition(index + 1, area)));

    private static OriginalMapAreaDefinition[] AcceptedAreaDefinitions() =>
        AcceptedAreas()
            .Select((area, index) => AcceptedAreaDefinition(index + 1, area))
            .ToArray();

    private static OriginalMapAreaDefinition AcceptedAreaDefinition(
        int oneBasedRecordOrdinal,
        OriginalMapTraversalArea area,
        byte defaultMusic = 8) =>
        new(
            new OriginalMapAreaRecordIdentity(
                OriginalMapRuntimeAdmission.AcceptedAreaResourceId,
                oneBasedRecordOrdinal),
            area,
            new OriginalMapAreaWordPair(0, oneBasedRecordOrdinal == 1 ? (ushort)32 : (ushort)0),
            new OriginalMapAreaWordPair(0, 0),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaWordPair(256, 256),
            new OriginalMapAreaBytePair(0, 0),
            new OriginalMapAreaBytePair(0, 0),
            mainLayerType: 0,
            defaultMusic);

    private static OriginalMapStepCopyDefinition ControlledStepCopy(MapId map) =>
        new(
            new OriginalMapStepCopyIdentity(
                ContentProfile.PrivateLocal,
                map,
                OriginalMapRuntimeAdmission.ControlledStepCopyResourceId,
                OriginalMapRuntimeAdmission.ControlledStepCopyRecordOrdinal),
            new MapPosition(
                OriginalMapRuntimeAdmission.ControlledStepCopyTriggerX,
                OriginalMapRuntimeAdmission.ControlledStepCopyTriggerY),
            new WorkingMapBlockCopy(
                OriginalMapRuntimeAdmission.ControlledStepCopySourceX,
                OriginalMapRuntimeAdmission.ControlledStepCopySourceY,
                OriginalMapRuntimeAdmission.ControlledStepCopyDestinationX,
                OriginalMapRuntimeAdmission.ControlledStepCopyDestinationY,
                OriginalMapRuntimeAdmission.ControlledStepCopyWidth,
                OriginalMapRuntimeAdmission.ControlledStepCopyHeight));

    private static ApplyPrivateOriginalMapLayoutMutationCommand MutationCommand(
        PrivateOriginalMapSessionSnapshot snapshot) =>
        new(
            snapshot.Definition.ControlledStepCopy!.Identity,
            snapshot.SimulationStep);

    private static void AssertRejectedMutation(
        GameSession session,
        ApplyPrivateOriginalMapLayoutMutationCommand command,
        PrivateOriginalMapSessionSnapshot expectedSnapshot,
        PrivateOriginalMapLayoutMutationFailureCode expectedCode)
    {
        PrivateOriginalMapLayoutMutationRejected rejected =
            Assert.IsType<PrivateOriginalMapLayoutMutationRejected>(
                session.ApplyPrivateOriginalMapLayoutMutation(command));
        Assert.Same(expectedSnapshot, rejected.Snapshot);
        Assert.Same(expectedSnapshot, session.PrivateOriginalMapSnapshot);
        Assert.Equal(expectedCode, rejected.Diagnostic.Code);
    }

    private static (int, int, int, int, int, int) Geometry(WorkingMapBlockCopy copy) =>
        (copy.SourceX, copy.SourceY, copy.DestinationX, copy.DestinationY,
            copy.Width, copy.Height);

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
