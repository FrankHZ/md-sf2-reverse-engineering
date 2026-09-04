using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class OriginalMapImportDefinitionTests
{
    private const string Digest =
        "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    [Fact]
    public void MessengerAcceptanceDefinitionOwnsClosedAlignedForceAndStageBindings()
    {
        OriginalMapMessengerAcceptanceDefinition accepted =
            AcceptedOriginalMapMessenger.Create(new MapId("map3"));
        List<int> textIds = [.. accepted.TextIds];
        List<int?> speakers = [.. accepted.SpeakerOperands];
        List<int> joined = [.. accepted.JoinedCharacterIds];
        List<OriginalMapMessengerFollowerLink> followers = [.. accepted.Followers];
        List<OriginalMapMessengerGuardState> guards = [.. accepted.Guards];
        List<OriginalMapMessengerAcceptanceStage> stages = [.. accepted.Stages];
        OriginalMapMessengerAcceptanceDefinition definition = new(
            accepted.Identity,
            accepted.Approach,
            accepted.EntryDirection,
            accepted.Trigger,
            accepted.MessengerProgramIdentity,
            accepted.AcceptedBranchProgramIdentity,
            accepted.ControlShapeSha256,
            accepted.PromptReturn,
            accepted.PromptFlag89,
            accepted.JoinSelector,
            accepted.Flag600,
            accepted.Flag66,
            accepted.CompletionFlag603,
            accepted.SarahSourceRecord,
            accepted.SarahCharacterId,
            accepted.Entity142SourceRecord,
            accepted.Entity142LogicalActorId,
            accepted.MessengerActorSourceRecord,
            accepted.MessengerLogicalActorId,
            accepted.MessengerActorInitialPosition,
            accepted.MessengerActorInitialOpaqueFacing,
            textIds,
            speakers,
            joined,
            followers,
            guards,
            accepted.Endpoint,
            accepted.EndpointOpaqueFacing,
            accepted.TerminalIdentity,
            stages);
        textIds.Clear();
        speakers.Clear();
        joined.Clear();
        followers.Clear();
        guards.Clear();
        stages.Clear();

        Assert.Equal(18, definition.TextIds.Count);
        Assert.Equal(18, definition.SpeakerOperands.Count);
        Assert.Equal(new[] { 1, 2 }, definition.JoinedCharacterIds);
        Assert.Equal(2, definition.Followers.Count);
        Assert.Equal(2, definition.Guards.Count);
        Assert.Equal(16, definition.Stages.Count);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<int>)definition.TextIds).Add(999));
        Assert.Throws<ArgumentException>(() => new OriginalMapMessengerAcceptanceDefinition(
            accepted.Identity,
            accepted.Approach,
            accepted.EntryDirection,
            accepted.Trigger,
            accepted.MessengerProgramIdentity,
            accepted.AcceptedBranchProgramIdentity,
            accepted.ControlShapeSha256,
            accepted.PromptReturn,
            accepted.PromptFlag89,
            accepted.JoinSelector,
            accepted.Flag600,
            accepted.Flag66,
            accepted.CompletionFlag603,
            accepted.SarahSourceRecord,
            accepted.SarahCharacterId,
            accepted.Entity142SourceRecord,
            accepted.Entity142LogicalActorId,
            accepted.MessengerActorSourceRecord,
            accepted.MessengerLogicalActorId,
            accepted.MessengerActorInitialPosition,
            accepted.MessengerActorInitialOpaqueFacing,
            accepted.TextIds,
            accepted.SpeakerOperands.Skip(1),
            accepted.JoinedCharacterIds,
            accepted.Followers,
            accepted.Guards,
            accepted.Endpoint,
            accepted.EndpointOpaqueFacing,
            accepted.TerminalIdentity,
            accepted.Stages));
    }

    [Fact]
    public void AstralZoneDefinitionOwnsClosedActorFlagTextAndStageBindings()
    {
        MapId map = new("map3");
        List<int> textIds = [514, 515, 516];
        List<OriginalMapAstralZoneStage> stages =
        [
            OriginalMapAstralZoneStage.ReadMessengerFlag603Clear,
            OriginalMapAstralZoneStage.ReadEntity142Flag602Set,
            OriginalMapAstralZoneStage.ReadCompletionFlag260Clear,
            OriginalMapAstralZoneStage.PresentText514,
            OriginalMapAstralZoneStage.PresentText515,
            OriginalMapAstralZoneStage.PresentText516,
            OriginalMapAstralZoneStage.RunPositionProgram,
            OriginalMapAstralZoneStage.SetCompletionFlag260,
        ];
        OriginalMapAstralZoneDefinition definition = new(
            new OriginalMapAstralZoneEventIdentity(
                ContentProfile.PrivateLocal,
                map,
                new MapSetupId("ms_map3"),
                "project-authored-zone-events",
                8,
                "project-authored-zone-event-7"),
            new MapPosition(58, 13),
            "project-authored-position-program",
            603,
            602,
            260,
            new OriginalMapEntityRecordIdentity("project-authored-entities", 1),
            1,
            new MapPosition(41, 10),
            1,
            new OriginalMapEntityRecordIdentity("project-authored-entities", 3),
            128,
            new MapPosition(6, 4),
            1,
            textIds,
            stages);
        textIds.Clear();
        stages.Clear();

        Assert.Equal(new[] { 514, 515, 516 }, definition.TextIds);
        Assert.Equal(8, definition.Stages.Count);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<int>)definition.TextIds).Add(517));
        Assert.Throws<ArgumentException>(() => new OriginalMapAstralZoneDefinition(
            definition.Identity,
            definition.Trigger,
            definition.PositionProgramIdentity,
            definition.MessengerCompletionFlag603,
            definition.RequiredEntity142Flag602,
            definition.RequiredEntity142Flag602,
            definition.SarahSourceRecord,
            definition.SarahLogicalActorId,
            definition.SarahDestination,
            definition.SarahOpaqueFacing,
            definition.Zone601ActorSourceRecord,
            definition.Zone601LogicalActorId,
            definition.Zone601ActorDestination,
            definition.Zone601ActorOpaqueFacing,
            definition.TextIds,
            definition.Stages));
    }

    [Fact]
    public void DefinitionOwnsAControlledPrivateImportWithoutRuntimeWiring()
    {
        MapId map = new("map3");
        OriginalMapAreaCatalog areaCatalog = AreaCatalog(
            new OriginalMapTraversalArea(0, 0, 63, 63));
        WorkingMapLayout layout = new(new ushort[WorkingMapLayout.WordCount]);
        OriginalMapEntityPopulation entityPopulation = Population(map);
        OriginalMapVisualResourceSelection visualResourceSelection = VisualSelection(map);
        List<string> unsupported = ["natural-route", "presentation"];
        OriginalMapControlledAdmission admission = new(
            map,
            new MapPosition(56, 3),
            opaqueFacing: 3,
            new MapSetupId("ms_map3"),
            "ms_map3_InitFunction",
            noProgramRequest: true);

        OriginalMapImportDefinition definition = new(
            map,
            layout,
            BlockCatalog(),
            areaCatalog,
            entityPopulation,
            visualResourceSelection,
            admission,
            unsupported);
        unsupported.Clear();

        Assert.Same(layout, definition.WorkingLayout);
        Assert.Same(areaCatalog, definition.AreaCatalog);
        Assert.Same(entityPopulation, definition.EntityPopulation);
        Assert.Same(visualResourceSelection, definition.VisualResourceSelection);
        Assert.Same(areaCatalog.Traversal, definition.Traversal);
        Assert.Equal(new MapPosition(56, 3), definition.ControlledAdmission.Position);
        Assert.Equal((byte)3, definition.ControlledAdmission.OpaqueFacing);
        Assert.Equal("ms_map3", definition.ControlledAdmission.SelectedSetup.Value);
        Assert.Equal("ms_map3_InitFunction", definition.ControlledAdmission.SelectedInitIdentity);
        Assert.True(definition.ControlledAdmission.NoProgramRequest);
        Assert.Equal(new[] { "natural-route", "presentation" }, definition.UnsupportedCapabilities);
    }

    [Fact]
    public void DefinitionRejectsMapMismatchBlockedStartAndOpenCapabilityBoundary()
    {
        MapId map = new("map3");
        OriginalMapAreaCatalog areaCatalog = AreaCatalog(
            new OriginalMapTraversalArea(0, 0, 63, 63));
        OriginalMapControlledAdmission admission = Admission(map);
        ushort[] blockedWords = new ushort[WorkingMapLayout.WordCount];
        blockedWords[(3 * WorkingMapLayout.ColumnCount) + 56] =
            OriginalMapTraversal.CollisionMask;

        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                new MapId("other"),
                new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]),
                BlockCatalog(),
                areaCatalog,
                Population(map),
                VisualSelection(map),
                admission,
                ["unknown"]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                map,
                new WorkingMapLayout(blockedWords),
                BlockCatalog(),
                areaCatalog,
                Population(map),
                VisualSelection(map),
                admission,
                ["unknown"]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                map,
                new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]),
                BlockCatalog(),
                areaCatalog,
                Population(map),
                VisualSelection(map),
                admission,
                []));

        ushort[] missingBlockWords = new ushort[WorkingMapLayout.WordCount];
        missingBlockWords[0] = 3;
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportDefinition(
                map,
                new WorkingMapLayout(missingBlockWords),
                BlockCatalog(),
                areaCatalog,
                Population(map),
                VisualSelection(map),
                admission,
                ["unknown"]));
    }

    [Fact]
    public void SameMapWarpCatalogOwnsExactPrivateRecordsAndSelectsByCandidateTarget()
    {
        MapId map = new("map3");
        OriginalMapSameMapWarpDefinition first = Warp(
            map,
            "project-authored-warps",
            ordinal: 2,
            trigger: new MapPosition(10, 10),
            destination: new MapPosition(11, 10),
            opaqueFacing: 0);
        OriginalMapSameMapWarpDefinition second = Warp(
            map,
            "project-authored-warps",
            ordinal: 7,
            trigger: new MapPosition(20, 20),
            destination: new MapPosition(21, 20),
            opaqueFacing: 2);
        List<OriginalMapSameMapWarpDefinition> records = [first, second];

        OriginalMapSameMapWarpCatalog catalog = new(records);
        records.Clear();

        Assert.Equal(map, catalog.Map);
        Assert.Equal("project-authored-warps", catalog.ResourceId);
        Assert.Equal([first, second], catalog.Records);
        Assert.Same(first, catalog.Select(map, first.Trigger));
        Assert.Null(catalog.Select(map, new MapPosition(9, 10)));
        Assert.Null(catalog.Select(new MapId("other"), first.Trigger));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<OriginalMapSameMapWarpDefinition>)catalog.Records).Add(first));
    }

    [Fact]
    public void SameMapWarpCatalogAndImportFailClosedOnIdentityAndGeometryDrift()
    {
        MapId map = new("map3");
        OriginalMapSameMapWarpDefinition first = Warp(
            map,
            "project-authored-warps",
            ordinal: 2,
            trigger: new MapPosition(10, 10),
            destination: new MapPosition(11, 10),
            opaqueFacing: 0);

        Assert.Throws<ArgumentException>(() => new OriginalMapSameMapWarpCatalog([]));
        Assert.Throws<ArgumentException>(() => new OriginalMapSameMapWarpCatalog([null!]));
        Assert.Throws<ArgumentException>(() => new OriginalMapSameMapWarpCatalog(
        [
            first,
            Warp(map, "other-warps", 3, new MapPosition(20, 20),
                new MapPosition(21, 20), 2),
        ]));
        Assert.Throws<ArgumentException>(() => new OriginalMapSameMapWarpCatalog(
        [
            first,
            Warp(map, "project-authored-warps", 2, new MapPosition(20, 20),
                new MapPosition(21, 20), 2),
        ]));
        Assert.Throws<ArgumentException>(() => new OriginalMapSameMapWarpCatalog(
        [
            first,
            Warp(map, "project-authored-warps", 3, first.Trigger,
                new MapPosition(21, 20), 2),
        ]));
        Assert.Throws<ArgumentException>(() => new OriginalMapSameMapWarpIdentity(
            ContentProfile.PublicSynthetic,
            map,
            "project-authored-warps",
            1));

        OriginalMapAreaCatalog areaCatalog = AreaCatalog(
            new OriginalMapTraversalArea(0, 0, 30, 30));
        ushort[] blocked = new ushort[WorkingMapLayout.WordCount];
        blocked[(10 * WorkingMapLayout.ColumnCount) + 11] =
            OriginalMapTraversal.CollisionMask;
        Assert.Throws<ArgumentException>(() => new OriginalMapImportDefinition(
            map,
            new WorkingMapLayout(blocked),
            BlockCatalog(),
            areaCatalog,
            Population(map),
            VisualSelection(map),
            Admission(map),
            controlledStepCopy: null,
            new OriginalMapSameMapWarpCatalog([first]),
            ["unknown"]));
        Assert.Throws<ArgumentException>(() => new OriginalMapImportDefinition(
            map,
            new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]),
            BlockCatalog(),
            areaCatalog,
            Population(map),
            VisualSelection(map),
            Admission(map),
            controlledStepCopy: null,
            new OriginalMapSameMapWarpCatalog(
            [
                Warp(map, "project-authored-warps", 2,
                    new MapPosition(31, 10), new MapPosition(11, 10), 0),
            ]),
            ["unknown"]));
    }

    [Fact]
    public void RoofOnLoadClearBindsOnePrivateWarpAndItsDestinationArea()
    {
        MapId map = new("map3");
        OriginalMapAreaCatalog areas = AreaCatalog(
            new OriginalMapTraversalArea(0, 0, 63, 63));
        OriginalMapSameMapWarpDefinition warp = Warp(
            map,
            "project-authored-warps",
            ordinal: 2,
            trigger: new MapPosition(10, 10),
            destination: new MapPosition(11, 10),
            opaqueFacing: 0);
        OriginalMapSameMapWarpCatalog warps = new([warp]);
        OriginalMapRoofOnLoadDefinition roof = new(
            new OriginalMapRoofOnLoadIdentity(
                ContentProfile.PrivateLocal,
                map,
                "project-authored-roofs",
                oneBasedRecordOrdinal: 1),
            sourceTrigger: new MapPosition(4, 8),
            clearDestination: new MapPosition(2, 20),
            width: 7,
            height: 8,
            warp.Identity,
            areas.Records[0].Identity);

        OriginalMapImportDefinition definition = new(
            map,
            new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]),
            BlockCatalog(),
            areas,
            Population(map),
            VisualSelection(map),
            Admission(map),
            controlledStepCopy: null,
            warps,
            ["unknown"],
            roof);

        Assert.Same(roof, definition.RoofOnLoadClear);
        Assert.Throws<ArgumentException>(() => new OriginalMapRoofOnLoadIdentity(
            ContentProfile.PublicSynthetic,
            map,
            "project-authored-roofs",
            1));
        Assert.Throws<ArgumentException>(() => new OriginalMapRoofOnLoadDefinition(
            roof.Identity,
            roof.SourceTrigger,
            roof.ClearDestination,
            roof.Width,
            roof.Height,
            new OriginalMapSameMapWarpIdentity(
                ContentProfile.PrivateLocal,
                new MapId("other"),
                warp.Identity.ResourceId,
                warp.Identity.OneBasedRecordOrdinal),
            roof.DestinationArea));
        Assert.Throws<ArgumentException>(() => new OriginalMapImportDefinition(
            map,
            definition.WorkingLayout,
            definition.BlockCatalog,
            definition.AreaCatalog,
            definition.EntityPopulation,
            definition.VisualResourceSelection,
            definition.ControlledAdmission,
            controlledStepCopy: null,
            sameMapWarps: null,
            ["unknown"],
            roof));
    }

    [Fact]
    public void EntityPopulationOwnsOrderedOpaqueRecordsWithoutInventingBehavior()
    {
        MapId map = new("map3");
        byte[] tail = [0xFF, 7, 8, 3];
        OriginalMapEntityDefinition walking = new(
            new OriginalMapEntityRecordIdentity("project-authored-entities", 1),
            rawX: 0xC7,
            rawY: 0x88,
            opaqueFacing: 3,
            mapSprite: 42,
            tail);
        List<OriginalMapEntityDefinition> records =
        [
            walking,
            new(
                new OriginalMapEntityRecordIdentity("project-authored-entities", 2),
                rawX: 7,
                rawY: 8,
                opaqueFacing: 1,
                mapSprite: 43,
                [0, 0, 0, 1]),
            new(
                new OriginalMapEntityRecordIdentity("project-authored-entities", 3),
                rawX: 7,
                rawY: 8,
                opaqueFacing: 0,
                mapSprite: 44,
                [0xFE, 0, 0, 2]),
        ];

        OriginalMapEntityPopulation population = new(
            map,
            new MapSetupId("ms_map3"),
            records);
        tail[0] = 0;
        records.Clear();

        Assert.Equal("project-authored-entities", population.ResourceId);
        Assert.Equal(3, population.Records.Count);
        Assert.NotSame(walking, population.Records[0]);
        Assert.Equal(new MapPosition(7, 8), population.Records[0].Position);
        Assert.Equal(OriginalMapEntityRecordKind.Walking, population.Records[0].Kind);
        Assert.Equal(new byte[] { 0xFF, 7, 8, 3 }, population.Records[0].OpaqueTail);
        Assert.Equal(OriginalMapEntityRecordKind.Fixed, population.Records[1].Kind);
        Assert.Equal(OriginalMapEntityRecordKind.Sequenced, population.Records[2].Kind);
        Assert.Equal(population.Records[1].Position, population.Records[2].Position);
        Assert.Throws<NotSupportedException>(
            () => ((IList<byte>)population.Records[0].OpaqueTail).Add(0));
    }

    [Fact]
    public void EntityPopulationRejectsMalformedTailAndNonContiguousOrMixedIdentity()
    {
        MapId map = new("map3");
        Assert.Throws<ArgumentException>(() => Entity("entities", 1, [0, 0, 0]));
        Assert.Throws<ArgumentException>(() => Entity("entities", 1, [0, 0, 0, 0, 0]));
        Assert.Throws<ArgumentException>(() => new OriginalMapEntityPopulation(
            map,
            new MapSetupId("ms_map3"),
            []));
        Assert.Throws<ArgumentException>(() => new OriginalMapEntityPopulation(
            map,
            new MapSetupId("ms_map3"),
            [Entity("entities", 2, [0, 0, 0, 0])]));
        Assert.Throws<ArgumentException>(() => new OriginalMapEntityPopulation(
            map,
            new MapSetupId("ms_map3"),
            [
                Entity("entities", 1, [0, 0, 0, 0]),
                Entity("other-entities", 2, [0, 0, 0, 0]),
            ]));
    }

    [Fact]
    public void VisualResourceSelectionOwnsFiveOrderedSlotsAndComputesTheAcceptedProjection()
    {
        byte[] slots = [0, 37, 43, 53, 66];
        OriginalMapVisualResourceSelection selection = new(
            new MapId("map3"),
            paletteIndex: 0,
            slots);
        slots[1] = byte.MaxValue;

        Assert.Equal((byte)0, selection.PaletteIndex);
        Assert.Equal(new byte[] { 0, 37, 43, 53, 66 }, selection.TilesetSlots);
        Assert.Equal(
            OriginalMapRuntimeAdmission.AcceptedVisualReferenceProjectionDigest,
            selection.ProjectionDigest);
        Assert.True(OriginalMapRuntimeAdmission.HasExactAcceptedVisualResourceSelection(selection));
        Assert.Throws<NotSupportedException>(
            () => ((IList<byte>)selection.TilesetSlots).Add(1));
        Assert.Throws<ArgumentException>(() => new OriginalMapVisualResourceSelection(
            new MapId("map3"),
            paletteIndex: 0,
            [0, 37, 43, 53]));
        Assert.Throws<ArgumentException>(() => new OriginalMapVisualResourceSelection(
            new MapId("map3"),
            paletteIndex: 0,
            [0, 37, 43, 53, 66, 67]));
    }

    [Fact]
    public void BlockCatalogDerivesOneResourceAndDefensivelyOwnsOrderedOpaqueWords()
    {
        ushort[] words = Enumerable.Range(0, OriginalMapBlockDefinition.OpaqueWordCount)
            .Select(value => checked((ushort)value))
            .ToArray();
        OriginalMapBlockDefinition original = new(
            new OriginalMapBlockRecordIdentity("project-authored-blocks", 0),
            words);
        List<OriginalMapBlockDefinition> records =
        [
            original,
            new(
                new OriginalMapBlockRecordIdentity("project-authored-blocks", 1),
                new ushort[OriginalMapBlockDefinition.OpaqueWordCount]),
        ];

        OriginalMapBlockCatalog catalog = new(records);
        words[0] = ushort.MaxValue;
        records.Clear();

        Assert.Equal("project-authored-blocks", catalog.ResourceId);
        Assert.Equal(2, catalog.Records.Count);
        Assert.NotSame(original, catalog.Records[0]);
        Assert.Equal((ushort)0, catalog.Records[0].OpaqueWords[0]);
        Assert.Equal(0, catalog.Resolve(0).Identity.ZeroBasedBlockIndex);

        ushort[] layoutWords = new ushort[WorkingMapLayout.WordCount];
        layoutWords[(3 * WorkingMapLayout.ColumnCount) + 2] = 1;
        Assert.Equal(
            1,
            catalog.Resolve(
                new WorkingMapLayout(layoutWords),
                new MapPosition(2, 3)).Identity.ZeroBasedBlockIndex);
    }

    [Fact]
    public void BlockCatalogRejectsEmptyNullMixedNonContiguousAndMalformedRecords()
    {
        Assert.Throws<ArgumentException>(() => new OriginalMapBlockCatalog([]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapBlockCatalog([null!]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapBlockDefinition(
                new OriginalMapBlockRecordIdentity("blocks", 0),
                new ushort[OriginalMapBlockDefinition.OpaqueWordCount - 1]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapBlockCatalog(
            [
                Block("blocks", 0),
                Block("other-blocks", 1),
            ]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapBlockCatalog(
            [
                Block("blocks", 0),
                Block("blocks", 0),
            ]));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapBlockCatalog(
            [
                Block("blocks", 0),
                Block("blocks", 2),
            ]));
    }

    [Fact]
    public void AreaCatalogOwnsOneOrderedSourceAndDerivesTheOnlyTraversal()
    {
        List<OriginalMapAreaDefinition> records =
        [
            AreaDefinition(1, new OriginalMapTraversalArea(0, 0, 31, 31)),
            AreaDefinition(2, new OriginalMapTraversalArea(32, 0, 63, 31)),
        ];
        OriginalMapAreaCatalog catalog = new(records);
        records.Clear();

        Assert.Equal("project-authored-area-table", catalog.ResourceId);
        Assert.Equal(2, catalog.Records.Count);
        Assert.Equal(
            catalog.Records[1],
            catalog.Resolve(new OriginalMapTraversalAreaSelection(
                2,
                catalog.Traversal.ActiveAreas[1])));
        Assert.Throws<ArgumentException>(() => new OriginalMapAreaCatalog(
        [
            AreaDefinition(2, new OriginalMapTraversalArea(0, 0, 31, 31)),
        ]));
        Assert.Throws<ArgumentException>(() => new OriginalMapAreaCatalog(
        [
            AreaDefinition(1, new OriginalMapTraversalArea(0, 0, 31, 31)),
            AreaDefinition(
                2,
                new OriginalMapTraversalArea(32, 0, 63, 31),
                "other-area-table"),
        ]));
        Assert.Throws<ArgumentException>(() => catalog.Resolve(
            new OriginalMapTraversalAreaSelection(
                2,
                new OriginalMapTraversalArea(32, 1, 63, 31))));
    }

    [Fact]
    public void RequestAndReceiptKeepDigestProfileAndClosedSetsExact()
    {
        OriginalMapImportRequest request = new(
            "sf2-canonical-map-import-v1",
            ContentProfile.PrivateLocal,
            Digest.ToLowerInvariant());
        List<string> owners = ["owner-a"];
        List<string> capabilities = ["capability-a"];
        OriginalMapImportReceipt receipt = new(
            request.PackageId,
            schemaVersion: 1,
            Digest,
            Digest,
            Digest,
            ContentProfile.PrivateLocal,
            new OriginalMapImportProvenance(
                "sf2-canonical-map-import-v1",
                Digest,
                "https://example.invalid/repository.git",
                "0123456789abcdef0123456789abcdef01234567"),
            owners,
            capabilities);
        owners.Clear();
        capabilities.Clear();

        Assert.Equal(Digest, request.ExpectedContentDigest);
        Assert.Equal(ContentProfile.PrivateLocal, receipt.Profile);
        Assert.Equal(Digest, receipt.ContentDigest);
        Assert.Equal(Digest, receipt.DecodedLayoutDigest);
        Assert.Equal(Digest, receipt.CollisionProjectionDigest);
        Assert.Equal(new[] { "owner-a" }, receipt.EvidenceOwnerIds);
        Assert.Equal(new[] { "capability-a" }, receipt.Capabilities);
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportRequest(
                "package",
                ContentProfile.PrivateLocal,
                "not-a-digest"));
        Assert.Throws<ArgumentException>(
            () => new OriginalMapImportReceipt(
                "package",
                1,
                Digest,
                Digest,
                Digest,
                ContentProfile.PublicSynthetic,
                receipt.Provenance,
                ["owner"],
                ["capability"]));
    }

    [Fact]
    public void ResultAndDiagnosticRemainTypedAndPathFree()
    {
        OriginalMapImportDiagnostic diagnostic = new(
            OriginalMapImportFailureCode.ContentDigestMismatch,
            "contentDigest",
            "mismatch");
        OriginalMapImportRejected rejected = new(diagnostic);

        Assert.Same(diagnostic, rejected.Diagnostic);
        Assert.DoesNotContain(
            typeof(OriginalMapImportReceipt).GetProperties(),
            property => property.Name.Contains("Path", StringComparison.Ordinal));
        Assert.DoesNotContain(
            typeof(OriginalMapImportDefinition).GetProperties(),
            property => property.Name.Contains("Path", StringComparison.Ordinal));
    }

    private static OriginalMapControlledAdmission Admission(MapId map) =>
        new(
            map,
            new MapPosition(56, 3),
            opaqueFacing: 3,
            new MapSetupId("ms_map3"),
            "ms_map3_InitFunction",
            noProgramRequest: true);

    private static OriginalMapSameMapWarpDefinition Warp(
        MapId map,
        string resourceId,
        int ordinal,
        MapPosition trigger,
        MapPosition destination,
        byte opaqueFacing) =>
        new(
            new OriginalMapSameMapWarpIdentity(
                ContentProfile.PrivateLocal,
                map,
                resourceId,
                ordinal),
            trigger,
            destination,
            opaqueFacing);

    private static OriginalMapVisualResourceSelection VisualSelection(MapId map) =>
        new(map, paletteIndex: 0, [0, 37, 43, 53, 66]);

    private static OriginalMapEntityPopulation Population(MapId map) =>
        new(
            map,
            new MapSetupId("ms_map3"),
            [Entity("project-authored-entities", 1, [0, 0, 0, 0])]);

    private static OriginalMapEntityDefinition Entity(
        string resourceId,
        int oneBasedOrdinal,
        IEnumerable<byte> opaqueTail) =>
        new(
            new OriginalMapEntityRecordIdentity(resourceId, oneBasedOrdinal),
            rawX: 1,
            rawY: 2,
            opaqueFacing: 3,
            mapSprite: 4,
            opaqueTail);

    private static OriginalMapBlockCatalog BlockCatalog(int count = 3) =>
        new(Enumerable.Range(0, count).Select(index => Block(
            "project-authored-blocks",
            index)));

    private static OriginalMapBlockDefinition Block(
        string resourceId,
        int zeroBasedIndex) =>
        new(
            new OriginalMapBlockRecordIdentity(resourceId, zeroBasedIndex),
            new ushort[OriginalMapBlockDefinition.OpaqueWordCount]);

    private static OriginalMapAreaCatalog AreaCatalog(
        params OriginalMapTraversalArea[] activeAreas) =>
        new(activeAreas.Select((area, index) => AreaDefinition(index + 1, area)));

    private static OriginalMapAreaDefinition AreaDefinition(
        int oneBasedRecordOrdinal,
        OriginalMapTraversalArea area,
        string resourceId = "project-authored-area-table") =>
        new(
            new OriginalMapAreaRecordIdentity(resourceId, oneBasedRecordOrdinal),
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
