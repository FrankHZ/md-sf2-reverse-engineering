using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Items;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Content;

public sealed class PublicSyntheticMap3PackageReader : IMapScenarioSource
{
    public const string PackageId = "public-synthetic-map3-smoke-v1";
    public const string Capability = "map3-synthetic-exploration-smoke";
    public const string ContextCapability = "public-synthetic-map3-context-selection-v1";
    public const string EventRequestCapability =
        "public-synthetic-map3-event-request-lifecycle-v1";
    public const string StateEffectCapability =
        "public-synthetic-map3-state-effect-v1";
    public const string LocalTransitionCapability =
        "public-synthetic-map3-local-transition-v1";
    public const string EntityInteractionCapability =
        "public-synthetic-map3-entity-interaction-v1";
    public const string DialogueCapability =
        "public-synthetic-map3-placeholder-dialogue-v1";
    public const string FieldSearchCapability =
        "public-synthetic-map3-field-search-v1";
    public const string ItemAcquisitionCapability =
        "public-synthetic-map3-placeholder-item-acquisition-v1";
    public const string OutboundTransitionCapability =
        "public-synthetic-map3-outbound-cross-map-transition-v1";
    public const string TacticalBattleCapability =
        "public-synthetic-map3-tactical-battle-completion-v1";
    public const string TacticalBattleDepthCapability =
        "project-authored-tactical-battle-depth-v1";
    public const string BattleCompletionWorldStateCapability =
        "public-synthetic-map3-battle-completion-world-state-v1";
    public const string EvidenceOwner = "sf2-map3-admitted-start-runtime-v1";
    public const string ExpectedContentDigest =
        "79db2e4a1f6186164c3224c1ccc7b5e6b8f6a36f8b2750b50af347b8b71e8131";

    private const string Profile = "public-synthetic";
    private const string ProvenanceKind = "project-authored-synthetic";
    private const string ProvenanceSource = "sf2-project-public-synthetic";
    private const string ScenarioId = "map3-public-synthetic-smoke";
    private const string DisplayName = "Map 3 public-synthetic exploration smoke";
    private const string Map3 = "map3";
    private const string OutboundShell = "public-synthetic-outbound-shell";
    private const string SetupIdentity = "ms_map3";
    private const string InitIdentity = "ms_map3_InitFunction";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    private readonly Func<byte[]> _loadDocument;

    public PublicSyntheticMap3PackageReader(string contentRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRoot);
        string resolvedRoot = Path.GetFullPath(contentRoot);
        _loadDocument = () => File.ReadAllBytes(Path.Combine(resolvedRoot, PackageId + ".json"));
    }

    private PublicSyntheticMap3PackageReader(Func<byte[]> loadDocument)
    {
        _loadDocument = loadDocument;
    }

    public static PublicSyntheticMap3PackageReader FromDocumentBytes(IEnumerable<byte> documentBytes)
    {
        ArgumentNullException.ThrowIfNull(documentBytes);
        byte[] copiedBytes = [.. documentBytes];
        return new PublicSyntheticMap3PackageReader(() => [.. copiedBytes]);
    }

    public MapScenarioAdmissionResult Admit(MapScenarioRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Profile != ContentProfile.PublicSynthetic)
        {
            return Reject(
                ScenarioAdmissionFailureCode.ProfileMismatch,
                "profile",
                "This adapter admits only the public-synthetic content profile.");
        }

        if (!string.Equals(request.PackageId, PackageId, StringComparison.Ordinal))
        {
            return Reject(
                ScenarioAdmissionFailureCode.PackageIdentityMismatch,
                "packageId",
                "This adapter owns exactly the public-synthetic Map 3 smoke package.");
        }

        byte[] documentBytes;
        try
        {
            documentBytes = _loadDocument();
        }
        catch (IOException error)
        {
            return Reject(
                ScenarioAdmissionFailureCode.PackageUnavailable,
                "package",
                $"The synthetic package is unavailable: {error.GetType().Name}.");
        }
        catch (UnauthorizedAccessException error)
        {
            return Reject(
                ScenarioAdmissionFailureCode.PackageUnavailable,
                "package",
                $"The synthetic package is unavailable: {error.GetType().Name}.");
        }

        string contentDigest =
            Convert.ToHexString(SHA256.HashData(documentBytes)).ToLowerInvariant();
        if (!string.Equals(contentDigest, ExpectedContentDigest, StringComparison.Ordinal))
        {
            return Reject(
                ScenarioAdmissionFailureCode.ContentDigestMismatch,
                "contentDigest",
                "The public-synthetic package bytes do not match the tracked package identity.");
        }

        return AdmitDocument(documentBytes, contentDigest);
    }

    internal static MapScenarioAdmissionResult AdmitSemanticallyForTests(
        IEnumerable<byte> documentBytes)
    {
        ArgumentNullException.ThrowIfNull(documentBytes);
        byte[] copied = [.. documentBytes];
        string digest = Convert.ToHexString(SHA256.HashData(copied)).ToLowerInvariant();
        return AdmitDocument(copied, digest);
    }

    private static MapScenarioAdmissionResult AdmitDocument(
        byte[] documentBytes,
        string contentDigest)
    {
        try
        {
            PublicSyntheticMap3Document? document =
                JsonSerializer.Deserialize<PublicSyntheticMap3Document>(documentBytes, JsonOptions);
            if (document is null)
            {
                return Reject(
                    ScenarioAdmissionFailureCode.InvalidDocument,
                    "document",
                    "The synthetic package document cannot be null.");
            }

            ScenarioAdmissionDiagnostic? diagnostic = Validate(document);
            if (diagnostic is not null)
            {
                return new MapScenarioRejected(diagnostic);
            }

            WorkingMapLayout layout = BuildLayout(document.LayoutRecipe);
            SyntheticWalkabilityGrid walkability = BuildWalkability(document.Walkability);
            MapId currentMap = new(document.Admission.CurrentMap);
            MapScenarioContextDefinition mapContext = BuildMapContext(
                document.MapContext,
                document.OutboundShell,
                document.InitialSemanticFacing,
                currentMap,
                layout,
                walkability);
            MapPosition start = new(
                document.Admission.LogicalStartPosition.X,
                document.Admission.LogicalStartPosition.Y);
            ScenarioAdmissionFacts admissionFacts = new(
                currentMap,
                new MapId(document.Admission.EgressMap),
                start,
                checked((byte)document.Admission.OpaqueStartFacing),
                document.Admission.SetupIdentity,
                document.Admission.InitIdentity,
                document.Admission.NoProgramRequest,
                document.Admission.ExplorationReady);
            MapScenarioDefinition scenario = new(
                document.ScenarioId,
                document.DisplayName,
                currentMap,
                start,
                admissionFacts,
                mapContext);
            ScenarioAdmissionReceipt receipt = new(
                document.PackageId,
                document.SchemaVersion,
                contentDigest,
                ContentProfile.PublicSynthetic,
                document.ExactControlledAdmission,
                document.EvidenceOwnerIds,
                document.Capabilities);
            return new MapScenarioAccepted(scenario, receipt);
        }
        catch (JsonException error)
        {
            return Reject(
                ScenarioAdmissionFailureCode.InvalidDocument,
                "document",
                $"The synthetic package JSON is invalid: {error.GetType().Name}.");
        }
        catch (Exception error) when (
            error is ArgumentException or ArithmeticException or InvalidOperationException or
                NullReferenceException)
        {
            return Reject(
                ScenarioAdmissionFailureCode.InvalidMap,
                "map",
                $"The synthetic map package is invalid: {error.Message}");
        }
    }

    private static ScenarioAdmissionDiagnostic? Validate(PublicSyntheticMap3Document document)
    {
        if (document.SchemaVersion != 1)
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.UnsupportedSchema,
                "schemaVersion",
                "Only public-synthetic Map 3 schema version 1 is supported.");
        }

        if (!string.Equals(document.PackageId, PackageId, StringComparison.Ordinal))
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.PackageIdentityMismatch,
                "packageId",
                "The package document identity does not match its admitted ID.");
        }

        if (!string.Equals(document.Profile, Profile, StringComparison.Ordinal) ||
            document.ExactControlledAdmission)
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.ProfileMismatch,
                "profile",
                "The tracked package must remain public-synthetic and non-exact.");
        }

        if (!string.Equals(document.Provenance.Kind, ProvenanceKind, StringComparison.Ordinal) ||
            !document.Provenance.Redistributable ||
            !string.Equals(document.Provenance.Source, ProvenanceSource, StringComparison.Ordinal))
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidDocument,
                "provenance",
                "Public content must use the exact project-authored synthetic provenance.");
        }

        if (!document.Capabilities.SequenceEqual(
                [
                    Capability,
                    ContextCapability,
                    EventRequestCapability,
                    StateEffectCapability,
                    LocalTransitionCapability,
                    EntityInteractionCapability,
                    DialogueCapability,
                    FieldSearchCapability,
                    ItemAcquisitionCapability,
                    OutboundTransitionCapability,
                    TacticalBattleCapability,
                    TacticalBattleDepthCapability,
                    BattleCompletionWorldStateCapability,
                ],
                StringComparer.Ordinal) ||
            !document.EvidenceOwnerIds.SequenceEqual([EvidenceOwner], StringComparer.Ordinal))
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidDocument,
                "capabilities",
                "The public package capabilities and evidence labels must remain closed sets.");
        }

        if (!string.Equals(document.ScenarioId, ScenarioId, StringComparison.Ordinal) ||
            !string.Equals(document.DisplayName, DisplayName, StringComparison.Ordinal))
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidDocument,
                "scenarioId",
                "The public-synthetic scenario identity is not recognized.");
        }

        AdmissionDocument admission = document.Admission;
        if (!string.Equals(admission.CurrentMap, Map3, StringComparison.Ordinal) ||
            !string.Equals(admission.EgressMap, Map3, StringComparison.Ordinal) ||
            admission.LogicalStartPosition.X != 56 ||
            admission.LogicalStartPosition.Y != 3 ||
            admission.OpaqueStartFacing != 3 ||
            !string.Equals(admission.SetupIdentity, SetupIdentity, StringComparison.Ordinal) ||
            !string.Equals(admission.InitIdentity, InitIdentity, StringComparison.Ordinal) ||
            !admission.NoProgramRequest ||
            !admission.ExplorationReady)
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidMap,
                "admission",
                "The package does not match the accepted logical Map 3 admission projection.");
        }

        if (document.LayoutRecipe.Width != WorkingMapLayout.ColumnCount ||
            document.LayoutRecipe.Height != WorkingMapLayout.RowCount ||
            document.Walkability.Width != WorkingMapLayout.ColumnCount ||
            document.Walkability.Height != WorkingMapLayout.RowCount ||
            document.Walkability.DefaultPassable)
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidMap,
                "layout",
                "The synthetic layout and walkability dimensions or defaults are invalid.");
        }

        if (!string.Equals(document.OutboundShell.MapId, OutboundShell, StringComparison.Ordinal) ||
            document.OutboundShell.LayoutRecipe.Width != WorkingMapLayout.ColumnCount ||
            document.OutboundShell.LayoutRecipe.Height != WorkingMapLayout.RowCount ||
            document.OutboundShell.Walkability.Width != WorkingMapLayout.ColumnCount ||
            document.OutboundShell.Walkability.Height != WorkingMapLayout.RowCount ||
            document.OutboundShell.Walkability.DefaultPassable)
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidMap,
                "outboundShell",
                "The outbound shell must remain the exact bounded public-synthetic runtime.");
        }

        if (document.MapContext.FieldSearches.Length != 1)
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidDocument,
                "mapContext.fieldSearches",
                "The bounded public-synthetic package requires exactly one field-search definition.");
        }

        if (document.MapContext.ItemAcquisitions.Length != 1)
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidDocument,
                "mapContext.itemAcquisitions",
                "The bounded public-synthetic package requires exactly one item-acquisition definition.");
        }

        if (document.MapContext.OutboundTransitions.Length != 1)
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidDocument,
                "mapContext.outboundTransitions",
                "The bounded public-synthetic package requires exactly one outbound transition.");
        }

        if (document.MapContext.PublicSyntheticBattles.Length != 1)
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidDocument,
                "mapContext.publicSyntheticBattles",
                "The bounded package requires exactly one public-synthetic tactical battle.");
        }

        if (document.OutboundShell.ZoneEvents.Length != 2 ||
            !string.Equals(
                document.OutboundShell.ZoneEvents[0].Kind,
                "specific",
                StringComparison.Ordinal) ||
            document.OutboundShell.ZoneEvents[0].X != 2 ||
            document.OutboundShell.ZoneEvents[0].Y != 1 ||
            !string.Equals(
                document.OutboundShell.ZoneEvents[0].TargetId,
                "public-synthetic-outbound-shell-battle-zone",
                StringComparison.Ordinal) ||
            !string.Equals(
                document.OutboundShell.ZoneEvents[1].Kind,
                "default",
                StringComparison.Ordinal) ||
            document.OutboundShell.ZoneEvents[1].X is not null ||
            document.OutboundShell.ZoneEvents[1].Y is not null ||
            !string.Equals(
                document.OutboundShell.ZoneEvents[1].TargetId,
                "synthetic-outbound-shell-no-zone",
                StringComparison.Ordinal))
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidMap,
                "outboundShell.zoneEvents",
                "The public-synthetic battle source zone and fallback order must remain exact.");
        }

        PublicSyntheticBattleDocument battle = document.MapContext.PublicSyntheticBattles[0];
        string[] battleIds =
        [
            battle.RequestId,
            battle.BattleId,
            battle.ActorId,
            battle.EnemyId,
            battle.SourceZoneTargetId,
            battle.RequestCueId,
            battle.AdmittedCueId,
            battle.MoveCueId,
            battle.AttackCueId,
            battle.EnemyResponseCueId,
            battle.CompletedCueId,
            battle.DefeatedCueId,
            battle.RestartedCueId,
            battle.ReturnedCueId,
        ];
        if (battleIds.Any(value =>
                !value.StartsWith("public-synthetic-", StringComparison.Ordinal)) ||
            battleIds.Distinct(StringComparer.Ordinal).Count() != battleIds.Length ||
            !string.Equals(battle.BattleId, "public-synthetic-map3-tactical-battle", StringComparison.Ordinal) ||
            !string.Equals(battle.SourceMapId, OutboundShell, StringComparison.Ordinal) ||
            battle.SourcePosition.X != 2 || battle.SourcePosition.Y != 1 ||
            !string.Equals(battle.SourceSetupId, "public-synthetic-outbound-shell-setup", StringComparison.Ordinal) ||
            !string.Equals(battle.ReturnMapId, OutboundShell, StringComparison.Ordinal) ||
            battle.ReturnPosition.X != 1 || battle.ReturnPosition.Y != 1 ||
            !string.Equals(battle.ReturnSetupId, "public-synthetic-outbound-shell-completed-setup", StringComparison.Ordinal) ||
            !string.Equals(battle.ReturnFacing, "east", StringComparison.Ordinal) ||
            !string.Equals(
                battle.CompletionEffectId,
                "public-synthetic-map3-battle-completion-world-effect",
                StringComparison.Ordinal) ||
            !string.Equals(
                battle.CompletionFlagId,
                "public-synthetic-map3-battle-completed",
                StringComparison.Ordinal) ||
            battle.Grid.Width != 3 || battle.Grid.Height != 2 ||
            battle.Grid.PassableCells.Length != 6 ||
            battle.Grid.PassableCells.Any(passable => !passable) ||
            battle.ActorStart.X != 0 || battle.ActorStart.Y != 1 ||
            battle.EnemyStart.X != 2 || battle.EnemyStart.Y != 1 ||
            battle.ActorMoveRange != 1 || battle.ActorAttackRange != 2 ||
            battle.ActorMaxHitPoints != 2 || battle.ActorDamage != 1 ||
            battle.EnemyMoveRange != 1 || battle.EnemyAttackRange != 1 ||
            battle.EnemyMaxHitPoints != 3 || battle.EnemyDamage != 1)
        {
            return Diagnostic(
                ScenarioAdmissionFailureCode.InvalidMap,
                "mapContext.publicSyntheticBattles",
                "The public-synthetic tactical battle definition is not the admitted bounded micro-loop.");
        }

        return null;
    }

    private static WorkingMapLayout BuildLayout(LayoutRecipeDocument recipe)
    {
        if (recipe.DefaultWord is < 0 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(recipe.DefaultWord));
        }

        ushort[] words = new ushort[WorkingMapLayout.WordCount];
        Array.Fill(words, checked((ushort)recipe.DefaultWord));
        HashSet<int> occupied = [];
        foreach (LayoutCellDocument cell in recipe.Cells)
        {
            int index = CheckedIndex(cell.X, cell.Y, "layoutRecipe.cells");
            if (cell.Word is < 0 or > ushort.MaxValue || !occupied.Add(index))
            {
                throw new ArgumentException("Synthetic layout cells must be unique ushort values.");
            }

            words[index] = checked((ushort)cell.Word);
        }

        return new WorkingMapLayout(words);
    }

    private static SyntheticWalkabilityGrid BuildWalkability(WalkabilityDocument walkability)
    {
        bool[] cells = new bool[WorkingMapLayout.WordCount];
        HashSet<int> covered = [];
        foreach (WalkabilityRectangleDocument rectangle in walkability.PassableRectangles)
        {
            if (rectangle.Width < 1 || rectangle.Height < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(walkability.PassableRectangles));
            }

            for (int y = rectangle.Y; y < checked(rectangle.Y + rectangle.Height); y++)
            {
                for (int x = rectangle.X; x < checked(rectangle.X + rectangle.Width); x++)
                {
                    int index = CheckedIndex(x, y, "walkability.passableRectangles");
                    covered.Add(index);
                    cells[index] = true;
                }
            }
        }

        if (covered.Count == 0)
        {
            throw new ArgumentException("Synthetic walkability requires a passable region.");
        }

        foreach (PositionDocument cell in walkability.BlockedCells)
        {
            cells[CheckedIndex(cell.X, cell.Y, "walkability.blockedCells")] = false;
        }

        return new SyntheticWalkabilityGrid(
            walkability.Width,
            walkability.Height,
            cells);
    }

    private static MapScenarioContextDefinition BuildMapContext(
        MapContextDocument context,
        OutboundShellDocument outboundShell,
        string initialSemanticFacing,
        MapId currentMap,
        WorkingMapLayout layout,
        SyntheticWalkabilityGrid walkability)
    {
        MapSetupCatalog setupCatalog = new(
            context.SetupCatalog.Select(entry =>
                new MapSetupCatalogEntry(
                    new MapId(entry.MapId),
                    new MapSetupRoute(
                        new MapSetupId(entry.DefaultSetupId),
                        entry.FlagAlternatives.Select(alternative =>
                            new MapSetupFlagVariant(
                                new FlagId(alternative.FlagId),
                                new MapSetupId(alternative.SetupId)))))));
        MapAreaDescriptionSource areaDescriptions = MapAreaDescriptionSource.Table(
            context.AreaDescriptions.DescriptionTextBase,
            context.AreaDescriptions.Entries.Select(BuildAreaDescriptionEntry));
        MapSetupEventTable<ZoneEventRecord> zoneEvents = new(
            context.ZoneEvents.Select(BuildZoneEventRecord));
        WorkingMapLayout outboundLayout = BuildLayout(outboundShell.LayoutRecipe);
        SyntheticWalkabilityGrid outboundWalkability =
            BuildWalkability(outboundShell.Walkability);
        MapExplorationRuntimeCatalog mapRuntimes = new(
            [
                new MapExplorationRuntimeDefinition(
                    currentMap,
                    layout,
                    walkability,
                    areaDescriptions,
                    zoneEvents),
                new MapExplorationRuntimeDefinition(
                    new MapId(outboundShell.MapId),
                    outboundLayout,
                    outboundWalkability,
                    MapAreaDescriptionSource.Table(
                        outboundShell.AreaDescriptions.DescriptionTextBase,
                        outboundShell.AreaDescriptions.Entries.Select(
                            BuildAreaDescriptionEntry)),
                    new MapSetupEventTable<ZoneEventRecord>(
                        outboundShell.ZoneEvents.Select(BuildZoneEventRecord))),
            ]);
        MapEventRequestCatalog eventRequests = new(
            context.EventRequests.Select(entry =>
                new MapEventRequestDefinition(
                    new MapEventRequestId(entry.RequestId),
                    new EventTargetId(entry.ZoneTargetId),
                    new PresentationCueId(entry.CueId))));
        MapEventEffectCatalog eventEffects = new(
            context.EventEffects.Select(entry =>
                new MapEventEffectDefinition(
                    new MapEventEffectId(entry.EffectId),
                    new MapEventRequestId(entry.RequestId),
                    new FlagId(entry.FlagId),
                    new PresentationCueId(entry.CueId))));
        MapLocalTransitionCatalog localTransitions = new(
            context.LocalTransitions.Select(entry =>
                new MapLocalTransitionDefinition(
                    new MapLocalTransitionRequestId(entry.RequestId),
                    new MapLocalTransitionId(entry.TransitionId),
                    new EventTargetId(entry.ZoneTargetId),
                    new MapId(entry.SourceMapId),
                    new MapPosition(entry.SourcePosition.X, entry.SourcePosition.Y),
                    new MapSetupId(entry.SourceSetupId),
                    new MapId(entry.DestinationMapId),
                    new MapPosition(
                        entry.DestinationPosition.X,
                        entry.DestinationPosition.Y),
                    new OpaqueMapOrientationId(entry.DestinationOrientationId),
                    new PresentationCueId(entry.CueId))));
        MapEntityInteractionCatalog entityInteractions = new(
            context.Entities.Select(entry =>
                new MapEntityDefinition(
                    new MapEntityId(entry.EntityId),
                    new MapId(entry.MapId),
                    new MapPosition(entry.Position.X, entry.Position.Y),
                    new MapEntityInteractionTargetId(entry.InteractionTargetId))),
            context.EntityInteractions.Select(BuildEntityInteractionDefinition));
        MapDialogueCatalog dialogues = new(
            context.Dialogues.Select(BuildDialogueDefinition));
        MapFieldSearchCatalog fieldSearches = new(
            context.FieldSearches.Select(BuildFieldSearchDefinition));
        MapItemAcquisitionCatalog itemAcquisitions = new(
            context.ItemAcquisitions.Select(BuildItemAcquisitionDefinition));
        MapOutboundTransitionCatalog outboundTransitions = new(
            context.OutboundTransitions.Select(entry =>
                new MapOutboundTransitionDefinition(
                    new MapOutboundTransitionRequestId(entry.RequestId),
                    new MapOutboundTransitionId(entry.TransitionId),
                    new EventTargetId(entry.ZoneTargetId),
                    new MapId(entry.SourceMapId),
                    new MapPosition(entry.SourcePosition.X, entry.SourcePosition.Y),
                    new MapSetupId(entry.SourceSetupId),
                    new MapId(entry.DestinationMapId),
                    new MapPosition(
                        entry.DestinationPosition.X,
                        entry.DestinationPosition.Y),
                    new MapSetupId(entry.DestinationSetupId),
                    ParseSemanticFacing(entry.DestinationFacing),
                    new PresentationCueId(entry.CueId))));
        PublicSyntheticBattleCatalog publicSyntheticBattles = new(
            context.PublicSyntheticBattles.Select(entry =>
                new PublicSyntheticBattleDefinition(
                    new PublicSyntheticBattleRequestId(entry.RequestId),
                    new TacticalBattleRules(
                        new TacticalBattleId(entry.BattleId),
                        new TacticalBattleGrid(
                            entry.Grid.Width,
                            entry.Grid.Height,
                            entry.Grid.PassableCells),
                        new TacticalCombatantId(entry.ActorId),
                        new TacticalPosition(entry.ActorStart.X, entry.ActorStart.Y),
                        new TacticalCombatantId(entry.EnemyId),
                        new TacticalPosition(entry.EnemyStart.X, entry.EnemyStart.Y),
                        entry.ActorMoveRange,
                        entry.ActorAttackRange,
                        entry.ActorMaxHitPoints,
                        entry.ActorDamage,
                        entry.EnemyMoveRange,
                        entry.EnemyAttackRange,
                        entry.EnemyMaxHitPoints,
                        entry.EnemyDamage),
                    new MapId(entry.SourceMapId),
                    new MapPosition(entry.SourcePosition.X, entry.SourcePosition.Y),
                    new MapSetupId(entry.SourceSetupId),
                    new EventTargetId(entry.SourceZoneTargetId),
                    new MapId(entry.ReturnMapId),
                    new MapPosition(entry.ReturnPosition.X, entry.ReturnPosition.Y),
                    new MapSetupId(entry.ReturnSetupId),
                    ParseSemanticFacing(entry.ReturnFacing),
                    new MapEventEffectId(entry.CompletionEffectId),
                    new FlagId(entry.CompletionFlagId),
                    new PresentationCueId(entry.RequestCueId),
                    new PresentationCueId(entry.AdmittedCueId),
                    new PresentationCueId(entry.MoveCueId),
                    new PresentationCueId(entry.AttackCueId),
                    new PresentationCueId(entry.EnemyResponseCueId),
                    new PresentationCueId(entry.CompletedCueId),
                    new PresentationCueId(entry.DefeatedCueId),
                    new PresentationCueId(entry.RestartedCueId),
                    new PresentationCueId(entry.ReturnedCueId))));
        return new MapScenarioContextDefinition(
            mapRuntimes,
            setupCatalog,
            new MapSetupId(context.VoidSetupId),
            context.SetFlags.Select(flag => new FlagId(flag)),
            eventRequests,
            eventEffects,
            localTransitions,
            ParseSemanticFacing(initialSemanticFacing),
            entityInteractions,
            dialogues,
            fieldSearches,
            itemAcquisitions,
            outboundTransitions,
            publicSyntheticBattles);
    }

    private static MapEntityInteractionDefinition BuildEntityInteractionDefinition(
        EntityInteractionDocument entry)
    {
        if (!string.Equals(entry.Kind, "specific", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Synthetic entity interactions admit only explicit non-default targets.");
        }

        return new MapEntityInteractionDefinition(
            new MapEntityInteractionRequestId(entry.RequestId),
            new MapEntityInteractionTargetId(entry.TargetId),
            new PresentationCueId(entry.CueId));
    }

    private static MapDialogueDefinition BuildDialogueDefinition(DialogueDocument entry)
    {
        if (!string.Equals(entry.Kind, "specific", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Synthetic dialogues admit only explicit non-default interaction targets.");
        }

        return new MapDialogueDefinition(
            new MapDialogueId(entry.DialogueId),
            new MapEntityInteractionTargetId(entry.InteractionTargetId),
            entry.Lines.Select(line =>
                new MapDialogueLineDefinition(
                    new MapDialogueLineId(line.LineId),
                    line.Text,
                    new PresentationCueId(line.CueId))),
            new PresentationCueId(entry.CloseCueId));
    }

    private static MapFieldSearchDefinition BuildFieldSearchDefinition(
        FieldSearchDocument entry)
    {
        if (!string.Equals(entry.Kind, "specific", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Synthetic field searches admit only explicit non-default contexts.");
        }

        return new MapFieldSearchDefinition(
            new MapFieldSearchContextId(entry.ContextId),
            new MapFieldSearchRequestId(entry.RequestId),
            new MapFieldSearchResultId(entry.ResultId),
            new MapDiscoveryId(entry.DiscoveryId),
            new MapId(entry.MapId),
            new MapPosition(entry.Position.X, entry.Position.Y),
            new MapSetupId(entry.SetupId),
            new EventTargetId(entry.ZoneTargetId),
            new PresentationCueId(entry.RequestCueId),
            new PresentationCueId(entry.DiscoveryCueId));
    }

    private static MapItemAcquisitionDefinition BuildItemAcquisitionDefinition(
        ItemAcquisitionDocument entry)
    {
        if (!string.Equals(entry.Kind, "specific", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Synthetic item acquisition admits only an explicit field-search discovery mapping.");
        }

        return new MapItemAcquisitionDefinition(
            new MapDiscoveryId(entry.DiscoveryId),
            new MapItemAcquisitionRequestId(entry.RequestId),
            new MapItemAcquisitionResultId(entry.ResultId),
            new ItemId(entry.ItemId),
            new PresentationCueId(entry.RequestCueId),
            new PresentationCueId(entry.AcquiredCueId));
    }

    private static SemanticFacing ParseSemanticFacing(string value) =>
        value switch
        {
            "north" => SemanticFacing.North,
            "east" => SemanticFacing.East,
            "south" => SemanticFacing.South,
            "west" => SemanticFacing.West,
            _ => throw new ArgumentException(
                "The synthetic semantic facing is not recognized.",
                nameof(value)),
        };

    private static MapAreaDescriptionEntry BuildAreaDescriptionEntry(
        AreaDescriptionEntryDocument entry)
    {
        if (!string.Equals(entry.Condition, "always", StringComparison.Ordinal) ||
            !string.Equals(entry.Payload.Kind, "text", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The bounded public-synthetic context admits only ordinary text descriptions.");
        }

        return new MapAreaDescriptionEntry(
            CheckedByte(entry.X, "mapContext.areaDescriptions.entries.x"),
            CheckedByte(entry.Y, "mapContext.areaDescriptions.entries.y"),
            AreaDescriptionCondition.Always,
            AreaDescriptionPayload.Text(
                entry.Payload.InvestigationOffset,
                entry.Payload.DescriptionOffset));
    }

    private static ZoneEventRecord BuildZoneEventRecord(ZoneEventDocument entry)
    {
        EventTargetId target = new(entry.TargetId);
        return entry.Kind switch
        {
            "specific" when entry.X is not null && entry.Y is not null =>
                ZoneEventRecord.Specific(
                    EventFieldMatch.Exact(CheckedByte(
                        entry.X.Value,
                        "mapContext.zoneEvents.x")),
                    EventFieldMatch.Exact(CheckedByte(
                        entry.Y.Value,
                        "mapContext.zoneEvents.y")),
                    target),
            "default" when entry.X is null && entry.Y is null =>
                ZoneEventRecord.Default(target),
            _ => throw new ArgumentException(
                "Synthetic zone events require a specific X/Y pair or one coordinate-free default."),
        };
    }

    private static byte CheckedByte(int value, string field)
    {
        if (value is < byte.MinValue or > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(field);
        }

        return checked((byte)value);
    }

    private static int CheckedIndex(int x, int y, string field)
    {
        if (x < 0 || x >= WorkingMapLayout.ColumnCount ||
            y < 0 || y >= WorkingMapLayout.RowCount)
        {
            throw new ArgumentOutOfRangeException(field);
        }

        return (y * WorkingMapLayout.ColumnCount) + x;
    }

    private static MapScenarioRejected Reject(
        ScenarioAdmissionFailureCode code,
        string field,
        string message) =>
        new(Diagnostic(code, field, message));

    private static ScenarioAdmissionDiagnostic Diagnostic(
        ScenarioAdmissionFailureCode code,
        string field,
        string message) =>
        new(code, field, message);

    private sealed class PublicSyntheticMap3Document
    {
        public required int SchemaVersion { get; init; }

        public required string PackageId { get; init; }

        public required string Profile { get; init; }

        public required bool ExactControlledAdmission { get; init; }

        public required ProvenanceDocument Provenance { get; init; }

        public required string[] EvidenceOwnerIds { get; init; }

        public required string[] Capabilities { get; init; }

        public required string ScenarioId { get; init; }

        public required string DisplayName { get; init; }

        public required string InitialSemanticFacing { get; init; }

        public required AdmissionDocument Admission { get; init; }

        public required LayoutRecipeDocument LayoutRecipe { get; init; }

        public required WalkabilityDocument Walkability { get; init; }

        public required MapContextDocument MapContext { get; init; }

        public required OutboundShellDocument OutboundShell { get; init; }
    }

    private sealed class ProvenanceDocument
    {
        public required string Kind { get; init; }

        public required string Source { get; init; }

        public required bool Redistributable { get; init; }
    }

    private sealed class AdmissionDocument
    {
        public required string CurrentMap { get; init; }

        public required string EgressMap { get; init; }

        public required PositionDocument LogicalStartPosition { get; init; }

        public required int OpaqueStartFacing { get; init; }

        public required string SetupIdentity { get; init; }

        public required string InitIdentity { get; init; }

        public required bool NoProgramRequest { get; init; }

        public required bool ExplorationReady { get; init; }
    }

    private sealed class LayoutRecipeDocument
    {
        public required int Width { get; init; }

        public required int Height { get; init; }

        public required int DefaultWord { get; init; }

        public required LayoutCellDocument[] Cells { get; init; }
    }

    private sealed class LayoutCellDocument
    {
        public required int X { get; init; }

        public required int Y { get; init; }

        public required int Word { get; init; }
    }

    private sealed class WalkabilityDocument
    {
        public required int Width { get; init; }

        public required int Height { get; init; }

        public required bool DefaultPassable { get; init; }

        public required WalkabilityRectangleDocument[] PassableRectangles { get; init; }

        public required PositionDocument[] BlockedCells { get; init; }
    }

    private sealed class WalkabilityRectangleDocument
    {
        public required int X { get; init; }

        public required int Y { get; init; }

        public required int Width { get; init; }

        public required int Height { get; init; }
    }

    private sealed class MapContextDocument
    {
        public required string VoidSetupId { get; init; }

        public required string[] SetFlags { get; init; }

        public required SetupCatalogEntryDocument[] SetupCatalog { get; init; }

        public required AreaDescriptionsDocument AreaDescriptions { get; init; }

        public required ZoneEventDocument[] ZoneEvents { get; init; }

        public required EventRequestDocument[] EventRequests { get; init; }

        public required EventEffectDocument[] EventEffects { get; init; }

        public required LocalTransitionDocument[] LocalTransitions { get; init; }

        public required EntityDocument[] Entities { get; init; }

        public required EntityInteractionDocument[] EntityInteractions { get; init; }

        public required DialogueDocument[] Dialogues { get; init; }

        public required FieldSearchDocument[] FieldSearches { get; init; }

        public required ItemAcquisitionDocument[] ItemAcquisitions { get; init; }

        public required OutboundTransitionDocument[] OutboundTransitions { get; init; }

        public required PublicSyntheticBattleDocument[] PublicSyntheticBattles { get; init; }
    }

    private sealed class OutboundShellDocument
    {
        public required string MapId { get; init; }

        public required LayoutRecipeDocument LayoutRecipe { get; init; }

        public required WalkabilityDocument Walkability { get; init; }

        public required AreaDescriptionsDocument AreaDescriptions { get; init; }

        public required ZoneEventDocument[] ZoneEvents { get; init; }
    }

    private sealed class SetupCatalogEntryDocument
    {
        public required string MapId { get; init; }

        public required string DefaultSetupId { get; init; }

        public required SetupFlagAlternativeDocument[] FlagAlternatives { get; init; }
    }

    private sealed class SetupFlagAlternativeDocument
    {
        public required string FlagId { get; init; }

        public required string SetupId { get; init; }
    }

    private sealed class AreaDescriptionsDocument
    {
        public required int DescriptionTextBase { get; init; }

        public required AreaDescriptionEntryDocument[] Entries { get; init; }
    }

    private sealed class AreaDescriptionEntryDocument
    {
        public required int X { get; init; }

        public required int Y { get; init; }

        public required string Condition { get; init; }

        public required AreaDescriptionPayloadDocument Payload { get; init; }
    }

    private sealed class AreaDescriptionPayloadDocument
    {
        public required string Kind { get; init; }

        public required int InvestigationOffset { get; init; }

        public required int DescriptionOffset { get; init; }
    }

    private sealed class ZoneEventDocument
    {
        public required string Kind { get; init; }

        public int? X { get; init; }

        public int? Y { get; init; }

        public required string TargetId { get; init; }
    }

    private sealed class EventRequestDocument
    {
        public required string RequestId { get; init; }

        public required string ZoneTargetId { get; init; }

        public required string CueId { get; init; }
    }

    private sealed class EventEffectDocument
    {
        public required string EffectId { get; init; }

        public required string RequestId { get; init; }

        public required string FlagId { get; init; }

        public required string CueId { get; init; }
    }

    private sealed class LocalTransitionDocument
    {
        public required string RequestId { get; init; }

        public required string TransitionId { get; init; }

        public required string ZoneTargetId { get; init; }

        public required string SourceMapId { get; init; }

        public required PositionDocument SourcePosition { get; init; }

        public required string SourceSetupId { get; init; }

        public required string DestinationMapId { get; init; }

        public required PositionDocument DestinationPosition { get; init; }

        public required string DestinationOrientationId { get; init; }

        public required string CueId { get; init; }
    }

    private sealed class OutboundTransitionDocument
    {
        public required string RequestId { get; init; }

        public required string TransitionId { get; init; }

        public required string ZoneTargetId { get; init; }

        public required string SourceMapId { get; init; }

        public required PositionDocument SourcePosition { get; init; }

        public required string SourceSetupId { get; init; }

        public required string DestinationMapId { get; init; }

        public required PositionDocument DestinationPosition { get; init; }

        public required string DestinationSetupId { get; init; }

        public required string DestinationFacing { get; init; }

        public required string CueId { get; init; }
    }

    private sealed class PublicSyntheticBattleDocument
    {
        public required string RequestId { get; init; }

        public required string BattleId { get; init; }

        public required string SourceMapId { get; init; }

        public required PositionDocument SourcePosition { get; init; }

        public required string SourceSetupId { get; init; }

        public required string SourceZoneTargetId { get; init; }

        public required string ReturnMapId { get; init; }

        public required PositionDocument ReturnPosition { get; init; }

        public required string ReturnSetupId { get; init; }

        public required string ReturnFacing { get; init; }

        public required string CompletionEffectId { get; init; }

        public required string CompletionFlagId { get; init; }

        public required TacticalGridDocument Grid { get; init; }

        public required string ActorId { get; init; }

        public required PositionDocument ActorStart { get; init; }

        public required string EnemyId { get; init; }

        public required PositionDocument EnemyStart { get; init; }

        public required int ActorMoveRange { get; init; }

        public required int ActorAttackRange { get; init; }

        public required int ActorMaxHitPoints { get; init; }

        public required int ActorDamage { get; init; }

        public required int EnemyMoveRange { get; init; }

        public required int EnemyAttackRange { get; init; }

        public required int EnemyMaxHitPoints { get; init; }

        public required int EnemyDamage { get; init; }

        public required string RequestCueId { get; init; }

        public required string AdmittedCueId { get; init; }

        public required string MoveCueId { get; init; }

        public required string AttackCueId { get; init; }

        public required string EnemyResponseCueId { get; init; }

        public required string CompletedCueId { get; init; }

        public required string DefeatedCueId { get; init; }

        public required string RestartedCueId { get; init; }

        public required string ReturnedCueId { get; init; }
    }

    private sealed class TacticalGridDocument
    {
        public required int Width { get; init; }

        public required int Height { get; init; }

        public required bool[] PassableCells { get; init; }
    }

    private sealed class PositionDocument
    {
        public required int X { get; init; }

        public required int Y { get; init; }
    }

    private sealed class EntityDocument
    {
        public required string EntityId { get; init; }

        public required string MapId { get; init; }

        public required PositionDocument Position { get; init; }

        public required string InteractionTargetId { get; init; }
    }

    private sealed class EntityInteractionDocument
    {
        public required string Kind { get; init; }

        public required string RequestId { get; init; }

        public required string TargetId { get; init; }

        public required string CueId { get; init; }
    }

    private sealed class DialogueDocument
    {
        public required string Kind { get; init; }

        public required string DialogueId { get; init; }

        public required string InteractionTargetId { get; init; }

        public required DialogueLineDocument[] Lines { get; init; }

        public required string CloseCueId { get; init; }
    }

    private sealed class DialogueLineDocument
    {
        public required string LineId { get; init; }

        public required string Text { get; init; }

        public required string CueId { get; init; }
    }

    private sealed class FieldSearchDocument
    {
        public required string Kind { get; init; }

        public required string ContextId { get; init; }

        public required string RequestId { get; init; }

        public required string ResultId { get; init; }

        public required string DiscoveryId { get; init; }

        public required string MapId { get; init; }

        public required PositionDocument Position { get; init; }

        public required string SetupId { get; init; }

        public required string ZoneTargetId { get; init; }

        public required string RequestCueId { get; init; }

        public required string DiscoveryCueId { get; init; }
    }

    private sealed class ItemAcquisitionDocument
    {
        public required string Kind { get; init; }

        public required string DiscoveryId { get; init; }

        public required string RequestId { get; init; }

        public required string ResultId { get; init; }

        public required string ItemId { get; init; }

        public required string RequestCueId { get; init; }

        public required string AcquiredCueId { get; init; }
    }
}
