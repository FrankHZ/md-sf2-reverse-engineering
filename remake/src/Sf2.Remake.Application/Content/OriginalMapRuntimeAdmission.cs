using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public static class OriginalMapRuntimeAdmission
{
    public const string PackageId = "sf2-canonical-map-import-v1";
    public const int SchemaVersion = 1;
    public const string AcceptedContentDigest =
        "DDDA4FA05455DDBA9CDAF85497CEE0C1C89C6E625721A8FEAD301044C892E508";
    public const string AcceptedRomSha256 =
        "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9";
    public const string AcceptedUpstreamRepository =
        "https://github.com/ShiningForceCentral/SF2DISASM.git";
    public const string AcceptedUpstreamCommit =
        "c834c652b6862bc5679fd7f69a38a7093206efc6";
    public const string AcceptedDecodedLayoutDigest =
        "6BC4D0BF350242EA908A5ED00FFFDF68F6428E7A5189B23AE189CD24BC220446";
    public const string AcceptedCollisionProjectionDigest =
        "A9A7BACA8952DCC50CA90CD0985512C7F2393184FEA45F4E85E397422EAC9433";
    public const string AcceptedBlocksetResourceId = "Map03s0_Blocks";
    public const int AcceptedBlockCount = 447;
    public const string AcceptedBlocksetProjectionDigest =
        "90DC1053A67860A9C6C7F3EE16F3E77544D93FAB223961D38D719D62BE027159";
    public const int AcceptedVisualReferenceByteCount = 6;
    public const int AcceptedTilesetSlotCount = 5;
    public const string AcceptedVisualReferenceProjectionDigest =
        "3082476EF0180C435C721C6DFD56E8CD58B5A16FB92914895AE88CE155596235";
    public const string AcceptedAreaResourceId = "Map03s2_Areas";
    public const int AcceptedAreaRecordCount = 3;
    public const int ControlledStartAreaRecordOrdinal = 2;
    public const string AcceptedAreaProjectionDigest =
        "A9C712C1E02FB4A03CA60E68FF3AEFE6CC71A9E07A986E0CEB46C9CD9C81A2A6";
    public const string AcceptedAreaSourceProjectionDigest =
        "B60D96CC0359E390A8C26FDA9CE3313023ACB4774902CD99E12CB798041EB225";
    public const string AcceptedEntityListResourceId = "ms_map3_Entities";
    public const int AcceptedEntityRecordCount = 19;
    public const int AcceptedFixedEntityRecordCount = 16;
    public const int AcceptedWalkingEntityRecordCount = 3;
    public const int AcceptedSequencedEntityRecordCount = 0;
    public const string AcceptedEntityProjectionDigest =
        "344A1BB9BBFD26D1A4AF3913A54BB42284D2095D8D5F4E3022BFD303AA6D739D";

    public const string MapId = "map3";
    public const int StartX = 56;
    public const int StartY = 3;
    public const byte OpaqueStartFacing = 3;
    public const string SelectedSetupId = "ms_map3";
    public const string SelectedInitIdentity = "ms_map3_InitFunction";

    public const string ControlledStepCopyResourceId = "Map03s4_StepEvents";
    public const int ControlledStepCopyRecordOrdinal = 6;
    public const int ControlledStepCopyTriggerX = 41;
    public const int ControlledStepCopyTriggerY = 13;
    public const int ControlledStepCopySourceX = 62;
    public const int ControlledStepCopySourceY = 0;
    public const int ControlledStepCopyDestinationX = 41;
    public const int ControlledStepCopyDestinationY = 13;
    public const int ControlledStepCopyWidth = 1;
    public const int ControlledStepCopyHeight = 1;

    public const int StepCopySourceRecordCount = 6;
    public const int BowieDoorStepCopyRecordOrdinal = 1;
    public const int BowieDoorStepCopyApproachX = 4;
    public const int BowieDoorStepCopyApproachY = 7;
    public const ExplorationDirection BowieDoorStepCopyDirection = ExplorationDirection.South;
    public const int BowieDoorStepCopyTriggerX = 4;
    public const int BowieDoorStepCopyTriggerY = 8;
    public const int BowieDoorStepCopySourceX = 62;
    public const int BowieDoorStepCopySourceY = 0;
    public const int BowieDoorStepCopyDestinationX = 4;
    public const int BowieDoorStepCopyDestinationY = 8;
    public const int BowieDoorStepCopyWidth = 1;
    public const int BowieDoorStepCopyHeight = 1;

    public const int SchoolDoorStepCopyApproachX = 41;
    public const int SchoolDoorStepCopyApproachY = 14;
    public const ExplorationDirection SchoolDoorStepCopyDirection = ExplorationDirection.North;

    public const string SameMapWarpResourceId = "Map03s6_WarpEvents";
    public const int SameMapWarpSourceRecordCount = 9;
    public const int SchoolWarpRecordOrdinal = 6;
    public const int SchoolWarpTriggerX = 46;
    public const int SchoolWarpTriggerY = 7;
    public const int SchoolWarpDestinationX = 59;
    public const int SchoolWarpDestinationY = 12;
    public const byte SchoolWarpOpaqueFacing = 2;
    public const int HouseWarpRecordOrdinal = 9;
    public const int HouseWarpTriggerX = 54;
    public const int HouseWarpTriggerY = 3;
    public const int HouseWarpDestinationX = 3;
    public const int HouseWarpDestinationY = 3;
    public const byte HouseWarpOpaqueFacing = 0;

    public const string Zone601ResourceId = "ms_map3_ZoneEvents";
    public const int Zone601SourceRecordCount = 10;
    public const int Zone601RecordOrdinal = 7;
    public const int Zone601TriggerX = 4;
    public const int Zone601TriggerY = 4;
    public const string Zone601TargetIdentity = "Map3_ZoneEvent6";
    public const string Zone601BlockingSequenceIdentity = "cs_5145C";
    public const int Zone601GateFlag = 601;
    public const int Zone601ActorSourceRecordOrdinal = 3;
    public const int Zone601LogicalActorId = 128;
    public const int Zone601ActorInitialX = 5;
    public const int Zone601ActorInitialY = 6;
    public const byte Zone601ActorInitialOpaqueFacing = 0;
    public const string Zone601ActorInitialBehaviorIdentity = "eas_InitSlow";
    public const uint Zone601ActorInitialActionValue = 0x00046102;
    public const int Zone601ActorBlockingEndX = 5;
    public const int Zone601ActorBlockingEndY = 4;
    public const byte Zone601ActorBlockingEndOpaqueFacing = 2;
    public const int Zone601OpaqueFaceWaitOperand = 20;
    public const string Zone601AmbientBehaviorIdentity = "eas_Walking";
    public const int Zone601AmbientCenterX = 5;
    public const int Zone601AmbientCenterY = 6;
    public const int Zone601AmbientRange = 1;

    public const string SarahEntityEventResourceId = "ms_map3_EntityEvents";
    public const int SarahEntityEventSourceRecordCount = 17;
    public const int SarahEntityEventRecordOrdinal = 1;
    public const string SarahEntityEventTargetIdentity = "Map3_EntityEvent0";
    public const byte SarahEntityEventOpaqueFacing = 3;
    public const int SarahActorSourceRecordOrdinal = 1;
    public const int SarahLogicalActorId = 1;
    public const int SarahActorInitialX = 42;
    public const int SarahActorInitialY = 8;
    public const byte SarahActorInitialOpaqueFacing = 3;
    public const uint SarahActorInitialActionValue = 0x000460CE;
    public const int SarahPlayerInteractionX = 42;
    public const int SarahPlayerInteractionY = 9;
    public const byte SarahPlayerInteractionOpaqueFacing = 1;
    public const int SarahLaterBranchFlag603 = 603;
    public const int SarahLaterBranchFlag602 = 602;
    public const int SarahTemporaryRouteFlag256 = 256;
    public const string SarahBlockingSequenceIdentity = "cs_513D6";
    public const int SarahFirstWaypointX = 41;
    public const int SarahFirstWaypointY = 7;
    public const byte SarahRestoredOpaqueFacing = 3;

    public const int Entity142EventHandlerAddress = 331536;
    public const string Entity142EventResourceId = "ms_map3_EntityEvents";
    public const int Entity142EventSourceRecordCount = 17;
    public const int Entity142EventRecordOrdinal = 16;
    public const int Entity142EventRecordAddress = 331596;
    public const int Entity142EventRelativeOffset = 308;
    public const int Entity142EventResolvedTargetAddress = 331844;
    public const string Entity142EventTargetIdentity = "Map3_EntityEvent15";
    public const byte Entity142EventOpaqueFacing = 3;
    public const int Entity142ActorSourceRecordOrdinal = 17;
    public const int Entity142LogicalActorId = 142;
    public const int Entity142PhysicalActorSlot = 17;
    public const int Entity142ActorSourceAddress = 330672;
    public const int Entity142ActorX = 54;
    public const int Entity142ActorY = 17;
    public const byte Entity142ActorOpaqueFacing = 1;
    public const byte Entity142ActorMapSprite = 209;
    public const uint Entity142ActorActionValue = 0x000460CE;
    public const int Entity142PlayerInteractionX = 55;
    public const int Entity142PlayerInteractionY = 17;
    public const byte Entity142PlayerInteractionOpaqueFacing = 2;
    public const int Entity142FirstInteractionFlag261 = 261;
    public const int Entity142CompletionFlag602 = 602;

    public const string RoofOnLoadResourceId = "Map03s5_RoofEvents";
    public const int RoofOnLoadSourceRecordCount = 10;
    public const int HouseRoofOnLoadRecordOrdinal = 1;
    public const int HouseRoofSourceTriggerX = 4;
    public const int HouseRoofSourceTriggerY = 8;
    public const int HouseRoofClearDestinationX = 2;
    public const int HouseRoofClearDestinationY = 32;
    public const int HouseRoofClearWidth = 7;
    public const int HouseRoofClearHeight = 8;
    public const int HouseRoofDestinationAreaOrdinal = 1;

    public const string ImportCapability = "private-canonical-map3-layout-import-v1";
    public const string TraversalCapability = "original-map3-traversal-policy-v1";
    public const string ControlledAdmissionCapability =
        "controlled-map3-import-definition-v1";
    public const string ControlledStepCopyCapability =
        "private-local-map3-controlled-step-copy-diagnostic-v1";
    public const string CurrentAreaDiagnosticCapability =
        "private-local-map3-current-area-diagnostic-v1";
    public const string AreaSourceRecordAdmissionCapability =
        "private-local-map3-source-area-record-admission-v1";
    public const string SelectedSetupEntityPopulationCapability =
        "private-local-map3-selected-setup-entity-population-v1";
    public const string BlocksetSourceAdmissionCapability =
        "private-local-map3-source-blockset-admission-v1";
    public const string VisualReferenceAdmissionCapability =
        "private-local-map3-source-visual-reference-admission-v1";
    public const string SameMapWarpAdmissionCapability =
        "private-local-map3-same-map-warp-admission-v1";
    public const string RoofOnLoadClearCapability =
        "private-local-map3-roof-on-load-clear-v1";
    public const string BowieDoorStepCopyCapability =
        "private-local-map3-bowie-door-step-copy-v1";
    public const string SchoolDoorStepCopyCapability =
        "private-local-map3-school-door-step-copy-v1";
    public const string Zone601InterceptionCapability =
        "private-local-map3-zone601-interception-v1";
    public const string SarahRouteCapability =
        "private-local-map3-sarah-route-v1";
    public const string Entity142AcknowledgementCapability =
        "private-local-map3-entity142-acknowledgement-v1";

    private static readonly ReadOnlyCollection<int> ReadOnlyZone601TextIds =
        Array.AsReadOnly(new[] { 510, 511, 483 });

    private static readonly ReadOnlyCollection<OriginalMapZone601BlockingStage>
        ReadOnlyZone601BlockingStages = Array.AsReadOnly(
        [
            OriginalMapZone601BlockingStage.ActorInitAndWait,
            OriginalMapZone601BlockingStage.ActorMoveUpTwoAndWait,
            OriginalMapZone601BlockingStage.ActorFaceLeftAndWait,
            OriginalMapZone601BlockingStage.PresentText510,
            OriginalMapZone601BlockingStage.PresentText511,
            OriginalMapZone601BlockingStage.PresentText483,
            OriginalMapZone601BlockingStage.ActorReinitAndWait,
            OriginalMapZone601BlockingStage.AmbientWalkingHandoff,
            OriginalMapZone601BlockingStage.SetFlag601,
        ]);

    private static readonly ReadOnlyCollection<int> ReadOnlySarahFirstTextIds =
        Array.AsReadOnly(new[] { 512, 480, 481 });

    private static readonly ReadOnlyCollection<int> ReadOnlySarahRepeatTextIds =
        Array.AsReadOnly(new[] { 480, 481 });

    private static readonly ReadOnlyCollection<OriginalMapSarahInteractionStage>
        ReadOnlySarahFirstStages = Array.AsReadOnly(
        [
            OriginalMapSarahInteractionStage.ReadFlag603Clear,
            OriginalMapSarahInteractionStage.ReadFlag602Clear,
            OriginalMapSarahInteractionStage.ReadTemporaryFlag256Clear,
            OriginalMapSarahInteractionStage.PresentText512,
            OriginalMapSarahInteractionStage.PresentText480,
            OriginalMapSarahInteractionStage.PresentText481,
            OriginalMapSarahInteractionStage.ReadTemporaryFlag256ClearAgain,
            OriginalMapSarahInteractionStage.MoveLeftOneAndWait,
            OriginalMapSarahInteractionStage.MoveUpOneAndWait,
            OriginalMapSarahInteractionStage.SetTemporaryFlag256,
            OriginalMapSarahInteractionStage.RestoreFacingDown,
        ]);

    private static readonly ReadOnlyCollection<OriginalMapSarahInteractionStage>
        ReadOnlySarahRepeatStages = Array.AsReadOnly(
        [
            OriginalMapSarahInteractionStage.ReadFlag603Clear,
            OriginalMapSarahInteractionStage.ReadFlag602Clear,
            OriginalMapSarahInteractionStage.ReadTemporaryFlag256Set,
            OriginalMapSarahInteractionStage.PresentText480,
            OriginalMapSarahInteractionStage.PresentText481,
            OriginalMapSarahInteractionStage.ReadTemporaryFlag256SetAgain,
            OriginalMapSarahInteractionStage.RestoreFacingDown,
        ]);

    private static readonly ReadOnlyCollection<int> ReadOnlyEntity142FirstTextIds =
        Array.AsReadOnly(new[] { 500, 501 });

    private static readonly ReadOnlyCollection<int> ReadOnlyEntity142RepeatTextIds =
        Array.AsReadOnly(new[] { 501 });

    private static readonly ReadOnlyCollection<OriginalMapEntity142InteractionStage>
        ReadOnlyEntity142FirstStages = Array.AsReadOnly(
        [
            OriginalMapEntity142InteractionStage.ReadFlag261Clear,
            OriginalMapEntity142InteractionStage.PresentText500,
            OriginalMapEntity142InteractionStage.SetFlag261,
            OriginalMapEntity142InteractionStage.PresentText501,
            OriginalMapEntity142InteractionStage.SetFlag602,
        ]);

    private static readonly ReadOnlyCollection<OriginalMapEntity142InteractionStage>
        ReadOnlyEntity142RepeatStages = Array.AsReadOnly(
        [
            OriginalMapEntity142InteractionStage.ReadFlag261Set,
            OriginalMapEntity142InteractionStage.PresentText501,
            OriginalMapEntity142InteractionStage.SetFlag602,
        ]);

    private static readonly ReadOnlyCollection<string> ReadOnlyRequiredCapabilities =
        Array.AsReadOnly(
            new[]
            {
                ImportCapability,
                TraversalCapability,
                ControlledAdmissionCapability,
                ControlledStepCopyCapability,
                CurrentAreaDiagnosticCapability,
                AreaSourceRecordAdmissionCapability,
                SelectedSetupEntityPopulationCapability,
                BlocksetSourceAdmissionCapability,
                VisualReferenceAdmissionCapability,
                SameMapWarpAdmissionCapability,
                RoofOnLoadClearCapability,
                BowieDoorStepCopyCapability,
                SchoolDoorStepCopyCapability,
                Zone601InterceptionCapability,
                SarahRouteCapability,
                Entity142AcknowledgementCapability,
            });

    private static readonly ReadOnlyCollection<string> ReadOnlyRequiredEvidenceOwners =
        Array.AsReadOnly(
            new[]
            {
                "sf2-map-content-static-v1",
                "sf2-map-layout-decode-v1",
                "sf2-canonical-map-import-v1",
                "sf2-map-tileset-decode-v1",
                "sf2-map-palette-static-v1",
                "sf2-map-setup-static-v1",
                "sf2-map-entities-static-v1",
                "sf2-map3-castle-battle-unlock-static-v1",
                "sf2-map3-admitted-start-runtime-v1",
                "sf2-map3-battle01-natural-route-runtime-v1",
            });

    public static IReadOnlyList<string> RequiredCapabilities => ReadOnlyRequiredCapabilities;

    public static IReadOnlyList<string> RequiredEvidenceOwners =>
        ReadOnlyRequiredEvidenceOwners;

    public static IReadOnlyList<int> Zone601TextIds => ReadOnlyZone601TextIds;

    public static IReadOnlyList<OriginalMapZone601BlockingStage> Zone601BlockingStages =>
        ReadOnlyZone601BlockingStages;

    public static IReadOnlyList<int> SarahFirstTextIds => ReadOnlySarahFirstTextIds;

    public static IReadOnlyList<int> SarahRepeatTextIds => ReadOnlySarahRepeatTextIds;

    public static IReadOnlyList<OriginalMapSarahInteractionStage> SarahFirstStages =>
        ReadOnlySarahFirstStages;

    public static IReadOnlyList<OriginalMapSarahInteractionStage> SarahRepeatStages =>
        ReadOnlySarahRepeatStages;

    public static IReadOnlyList<int> Entity142FirstTextIds =>
        ReadOnlyEntity142FirstTextIds;

    public static IReadOnlyList<int> Entity142RepeatTextIds =>
        ReadOnlyEntity142RepeatTextIds;

    public static IReadOnlyList<OriginalMapEntity142InteractionStage> Entity142FirstStages =>
        ReadOnlyEntity142FirstStages;

    public static IReadOnlyList<OriginalMapEntity142InteractionStage> Entity142RepeatStages =>
        ReadOnlyEntity142RepeatStages;

    internal static bool HasExactRequiredCapabilities(IEnumerable<string> capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        HashSet<string> actual = new(capabilities, StringComparer.Ordinal);
        return actual.Count == ReadOnlyRequiredCapabilities.Count &&
            actual.SetEquals(ReadOnlyRequiredCapabilities);
    }

    internal static bool HasExactRequiredEvidenceOwners(IEnumerable<string> evidenceOwners)
    {
        ArgumentNullException.ThrowIfNull(evidenceOwners);
        HashSet<string> actual = new(evidenceOwners, StringComparer.Ordinal);
        return actual.Count == ReadOnlyRequiredEvidenceOwners.Count &&
            actual.SetEquals(ReadOnlyRequiredEvidenceOwners);
    }

    public static bool HasExactAcceptedBlocksetProjection(OriginalMapBlockCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        return string.Equals(
                catalog.ResourceId,
                AcceptedBlocksetResourceId,
                StringComparison.Ordinal) &&
            catalog.Records.Count == AcceptedBlockCount &&
            string.Equals(
                catalog.ProjectionDigest,
                AcceptedBlocksetProjectionDigest,
                StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasExactAcceptedVisualResourceSelection(
        OriginalMapVisualResourceSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return string.Equals(selection.Map.Value, MapId, StringComparison.Ordinal) &&
            selection.TilesetSlots.Count == AcceptedTilesetSlotCount &&
            OriginalMapVisualResourceSelection.ProjectionByteCount ==
                AcceptedVisualReferenceByteCount &&
            string.Equals(
                selection.ProjectionDigest,
                AcceptedVisualReferenceProjectionDigest,
                StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasExactAcceptedEntityPopulation(
        OriginalMapEntityPopulation population)
    {
        ArgumentNullException.ThrowIfNull(population);
        return string.Equals(population.Map.Value, MapId, StringComparison.Ordinal) &&
            string.Equals(
                population.SelectedSetup.Value,
                SelectedSetupId,
                StringComparison.Ordinal) &&
            string.Equals(
                population.ResourceId,
                AcceptedEntityListResourceId,
                StringComparison.Ordinal) &&
            population.Records.Count == AcceptedEntityRecordCount &&
            population.Records.Count(record =>
                record.Kind == OriginalMapEntityRecordKind.Fixed) ==
                AcceptedFixedEntityRecordCount &&
            population.Records.Count(record =>
                record.Kind == OriginalMapEntityRecordKind.Walking) ==
                AcceptedWalkingEntityRecordCount &&
            population.Records.Count(record =>
                record.Kind == OriginalMapEntityRecordKind.Sequenced) ==
                AcceptedSequencedEntityRecordCount &&
            string.Equals(
                population.ProjectionDigest,
                AcceptedEntityProjectionDigest,
                StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsExactControlledStepCopy(OriginalMapStepCopyDefinition? definition)
    {
        if (definition is null)
        {
            return false;
        }

        OriginalMapStepCopyIdentity identity = definition.Identity;
        WorkingMapBlockCopy copy = definition.Copy;
        return identity.Profile == ContentProfile.PrivateLocal &&
            string.Equals(identity.Map.Value, MapId, StringComparison.Ordinal) &&
            string.Equals(
                identity.SourceResourceId,
                ControlledStepCopyResourceId,
                StringComparison.Ordinal) &&
            identity.OneBasedRecordOrdinal == ControlledStepCopyRecordOrdinal &&
            definition.Trigger == new MapPosition(
                ControlledStepCopyTriggerX,
                ControlledStepCopyTriggerY) &&
            copy.SourceX == ControlledStepCopySourceX &&
            copy.SourceY == ControlledStepCopySourceY &&
            copy.DestinationX == ControlledStepCopyDestinationX &&
            copy.DestinationY == ControlledStepCopyDestinationY &&
            copy.Width == ControlledStepCopyWidth &&
            copy.Height == ControlledStepCopyHeight;
    }

    public static bool HasExactAcceptedBowieDoorStepCopy(
        OriginalMapStepCopyDefinition? definition)
    {
        if (definition is null)
        {
            return false;
        }

        OriginalMapStepCopyIdentity identity = definition.Identity;
        WorkingMapBlockCopy copy = definition.Copy;
        return identity.Profile == ContentProfile.PrivateLocal &&
            string.Equals(identity.Map.Value, MapId, StringComparison.Ordinal) &&
            string.Equals(
                identity.SourceResourceId,
                ControlledStepCopyResourceId,
                StringComparison.Ordinal) &&
            identity.OneBasedRecordOrdinal == BowieDoorStepCopyRecordOrdinal &&
            definition.Trigger == new MapPosition(
                BowieDoorStepCopyTriggerX,
                BowieDoorStepCopyTriggerY) &&
            copy.SourceX == BowieDoorStepCopySourceX &&
            copy.SourceY == BowieDoorStepCopySourceY &&
            copy.DestinationX == BowieDoorStepCopyDestinationX &&
            copy.DestinationY == BowieDoorStepCopyDestinationY &&
            copy.Width == BowieDoorStepCopyWidth &&
            copy.Height == BowieDoorStepCopyHeight;
    }

    public static bool HasExactAcceptedSchoolDoorStepCopy(
        OriginalMapStepCopyDefinition? definition) =>
        IsExactControlledStepCopy(definition);

    public static bool HasExactAcceptedZone601(
        OriginalMapZone601Definition? definition,
        OriginalMapEntityPopulation population,
        OriginalMapTraversal traversal,
        WorkingMapLayout workingLayout)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(traversal);
        ArgumentNullException.ThrowIfNull(workingLayout);
        if (definition is null ||
            definition.Identity.Profile != ContentProfile.PrivateLocal ||
            definition.Identity.Map != new MapId(MapId) ||
            definition.Identity.Setup != new MapSetupId(SelectedSetupId) ||
            !string.Equals(
                definition.Identity.ResourceId,
                Zone601ResourceId,
                StringComparison.Ordinal) ||
            definition.Identity.OneBasedRecordOrdinal != Zone601RecordOrdinal ||
            !string.Equals(
                definition.Identity.TargetIdentity,
                Zone601TargetIdentity,
                StringComparison.Ordinal) ||
            definition.Trigger != new MapPosition(Zone601TriggerX, Zone601TriggerY) ||
            definition.GateFlag != Zone601GateFlag ||
            !string.Equals(
                definition.BlockingSequenceIdentity,
                Zone601BlockingSequenceIdentity,
                StringComparison.Ordinal) ||
            definition.ActorSourceRecord != new OriginalMapEntityRecordIdentity(
                AcceptedEntityListResourceId,
                Zone601ActorSourceRecordOrdinal) ||
            definition.LogicalActorId != Zone601LogicalActorId ||
            definition.ActorInitialPosition != new MapPosition(
                Zone601ActorInitialX,
                Zone601ActorInitialY) ||
            definition.ActorInitialOpaqueFacing != Zone601ActorInitialOpaqueFacing ||
            !string.Equals(
                definition.ActorInitialBehaviorIdentity,
                Zone601ActorInitialBehaviorIdentity,
                StringComparison.Ordinal) ||
            definition.ActorBlockingEndPosition != new MapPosition(
                Zone601ActorBlockingEndX,
                Zone601ActorBlockingEndY) ||
            definition.ActorBlockingEndOpaqueFacing != Zone601ActorBlockingEndOpaqueFacing ||
            definition.OpaqueFaceWaitOperand != Zone601OpaqueFaceWaitOperand ||
            !definition.TextIds.SequenceEqual(ReadOnlyZone601TextIds) ||
            !string.Equals(
                definition.AmbientBehaviorIdentity,
                Zone601AmbientBehaviorIdentity,
                StringComparison.Ordinal) ||
            definition.AmbientCenter != new MapPosition(
                Zone601AmbientCenterX,
                Zone601AmbientCenterY) ||
            definition.AmbientRange != Zone601AmbientRange ||
            !definition.BlockingStages.SequenceEqual(ReadOnlyZone601BlockingStages) ||
            population.Map != definition.Identity.Map ||
            population.SelectedSetup != definition.Identity.Setup ||
            population.ResourceId != definition.ActorSourceRecord.ResourceId ||
            population.Records.Count < definition.ActorSourceRecord.OneBasedRecordOrdinal)
        {
            return false;
        }

        OriginalMapEntityDefinition actor =
            population.Records[definition.ActorSourceRecord.OneBasedRecordOrdinal - 1];
        Span<byte> expectedAction = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(
            expectedAction,
            Zone601ActorInitialActionValue);
        return actor.Identity == definition.ActorSourceRecord &&
            actor.Position == definition.ActorInitialPosition &&
            actor.RawX == Zone601ActorInitialX &&
            actor.RawY == Zone601ActorInitialY &&
            actor.OpaqueFacing == definition.ActorInitialOpaqueFacing &&
            actor.Kind == OriginalMapEntityRecordKind.Fixed &&
            actor.OpaqueTail.SequenceEqual(expectedAction.ToArray()) &&
            traversal.ResolveCandidateTarget(
                workingLayout,
                new MapPosition(HouseWarpDestinationX, HouseWarpDestinationY),
                ExplorationDirection.East) == definition.Trigger &&
            !OriginalMapTraversal.IsBlocked(workingLayout, definition.Trigger);
    }

    public static bool HasExactAcceptedSarah(
        OriginalMapSarahDefinition? definition,
        OriginalMapEntityPopulation population,
        OriginalMapTraversal traversal,
        WorkingMapLayout workingLayout)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(traversal);
        ArgumentNullException.ThrowIfNull(workingLayout);
        if (definition is null ||
            definition.Identity != new OriginalMapSarahEventIdentity(
                ContentProfile.PrivateLocal,
                new MapId(MapId),
                new MapSetupId(SelectedSetupId),
                SarahEntityEventResourceId,
                SarahEntityEventRecordOrdinal,
                SarahEntityEventTargetIdentity,
                SarahEntityEventOpaqueFacing) ||
            definition.ActorSourceRecord != new OriginalMapEntityRecordIdentity(
                AcceptedEntityListResourceId,
                SarahActorSourceRecordOrdinal) ||
            definition.LogicalActorId != SarahLogicalActorId ||
            definition.ActorInitialPosition != new MapPosition(
                SarahActorInitialX,
                SarahActorInitialY) ||
            definition.ActorInitialOpaqueFacing != SarahActorInitialOpaqueFacing ||
            definition.PlayerInteractionPosition != new MapPosition(
                SarahPlayerInteractionX,
                SarahPlayerInteractionY) ||
            definition.PlayerInteractionOpaqueFacing != SarahPlayerInteractionOpaqueFacing ||
            definition.LaterBranchFlag603 != SarahLaterBranchFlag603 ||
            definition.LaterBranchFlag602 != SarahLaterBranchFlag602 ||
            definition.TemporaryRouteFlag256 != SarahTemporaryRouteFlag256 ||
            !string.Equals(
                definition.BlockingSequenceIdentity,
                SarahBlockingSequenceIdentity,
                StringComparison.Ordinal) ||
            definition.FirstInteractionWaypoint != new MapPosition(
                SarahFirstWaypointX,
                SarahFirstWaypointY) ||
            definition.RestoredOpaqueFacing != SarahRestoredOpaqueFacing ||
            !definition.FirstInteractionTextIds.SequenceEqual(ReadOnlySarahFirstTextIds) ||
            !definition.RepeatInteractionTextIds.SequenceEqual(ReadOnlySarahRepeatTextIds) ||
            !definition.FirstInteractionStages.SequenceEqual(ReadOnlySarahFirstStages) ||
            !definition.RepeatInteractionStages.SequenceEqual(ReadOnlySarahRepeatStages) ||
            population.Map != definition.Identity.Map ||
            population.SelectedSetup != definition.Identity.Setup ||
            population.ResourceId != definition.ActorSourceRecord.ResourceId ||
            population.Records.Count < definition.ActorSourceRecord.OneBasedRecordOrdinal)
        {
            return false;
        }

        OriginalMapEntityDefinition actor =
            population.Records[definition.ActorSourceRecord.OneBasedRecordOrdinal - 1];
        Span<byte> expectedAction = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(expectedAction, SarahActorInitialActionValue);
        return actor.Identity == definition.ActorSourceRecord &&
            actor.Position == definition.ActorInitialPosition &&
            actor.RawX == SarahActorInitialX &&
            actor.RawY == SarahActorInitialY &&
            actor.OpaqueFacing == definition.ActorInitialOpaqueFacing &&
            actor.Kind == OriginalMapEntityRecordKind.Fixed &&
            actor.OpaqueTail.SequenceEqual(expectedAction.ToArray()) &&
            traversal.IsWithinActiveArea(definition.ActorInitialPosition) &&
            traversal.IsWithinActiveArea(definition.PlayerInteractionPosition) &&
            traversal.IsWithinActiveArea(definition.FirstInteractionWaypoint) &&
            !OriginalMapTraversal.IsBlocked(workingLayout, definition.ActorInitialPosition) &&
            !OriginalMapTraversal.IsBlocked(workingLayout, definition.PlayerInteractionPosition) &&
            !OriginalMapTraversal.IsBlocked(workingLayout, definition.FirstInteractionWaypoint);
    }

    public static bool HasExactAcceptedEntity142(
        OriginalMapEntity142Definition? definition,
        OriginalMapEntityPopulation population,
        OriginalMapTraversal traversal,
        WorkingMapLayout workingLayout)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(traversal);
        ArgumentNullException.ThrowIfNull(workingLayout);
        if (definition is null ||
            definition.Identity != new OriginalMapEntity142EventIdentity(
                ContentProfile.PrivateLocal,
                new MapId(MapId),
                new MapSetupId(SelectedSetupId),
                Entity142EventResourceId,
                Entity142EventRecordOrdinal,
                Entity142EventTargetIdentity,
                Entity142EventOpaqueFacing) ||
            definition.ActorSourceRecord != new OriginalMapEntityRecordIdentity(
                AcceptedEntityListResourceId,
                Entity142ActorSourceRecordOrdinal) ||
            definition.LogicalActorId != Entity142LogicalActorId ||
            definition.PhysicalActorSlot != Entity142PhysicalActorSlot ||
            definition.ActorPosition != new MapPosition(Entity142ActorX, Entity142ActorY) ||
            definition.ActorOpaqueFacing != Entity142ActorOpaqueFacing ||
            definition.PlayerInteractionPosition != new MapPosition(
                Entity142PlayerInteractionX,
                Entity142PlayerInteractionY) ||
            definition.PlayerInteractionOpaqueFacing != Entity142PlayerInteractionOpaqueFacing ||
            definition.FirstInteractionFlag261 != Entity142FirstInteractionFlag261 ||
            definition.CompletionFlag602 != Entity142CompletionFlag602 ||
            !definition.FirstInteractionTextIds.SequenceEqual(ReadOnlyEntity142FirstTextIds) ||
            !definition.RepeatInteractionTextIds.SequenceEqual(ReadOnlyEntity142RepeatTextIds) ||
            !definition.FirstInteractionStages.SequenceEqual(ReadOnlyEntity142FirstStages) ||
            !definition.RepeatInteractionStages.SequenceEqual(ReadOnlyEntity142RepeatStages) ||
            population.Map != definition.Identity.Map ||
            population.SelectedSetup != definition.Identity.Setup ||
            population.ResourceId != definition.ActorSourceRecord.ResourceId ||
            population.Records.Count < definition.ActorSourceRecord.OneBasedRecordOrdinal)
        {
            return false;
        }

        OriginalMapEntityDefinition actor =
            population.Records[definition.ActorSourceRecord.OneBasedRecordOrdinal - 1];
        Span<byte> expectedAction = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(expectedAction, Entity142ActorActionValue);
        return actor.Identity == definition.ActorSourceRecord &&
            actor.RawX == Entity142ActorX &&
            actor.RawY == Entity142ActorY &&
            actor.Position == definition.ActorPosition &&
            actor.OpaqueFacing == definition.ActorOpaqueFacing &&
            actor.MapSprite == Entity142ActorMapSprite &&
            actor.Kind == OriginalMapEntityRecordKind.Fixed &&
            actor.OpaqueTail.SequenceEqual(expectedAction.ToArray()) &&
            traversal.IsWithinActiveArea(definition.ActorPosition) &&
            traversal.IsWithinActiveArea(definition.PlayerInteractionPosition) &&
            !OriginalMapTraversal.IsBlocked(workingLayout, definition.ActorPosition) &&
            !OriginalMapTraversal.IsBlocked(workingLayout, definition.PlayerInteractionPosition);
    }

    public static bool HasExactAcceptedSameMapWarps(OriginalMapSameMapWarpCatalog? catalog)
    {
        if (catalog is null ||
            catalog.Map != new MapId(MapId) ||
            !string.Equals(catalog.ResourceId, SameMapWarpResourceId, StringComparison.Ordinal) ||
            catalog.Records.Count != 2)
        {
            return false;
        }

        return IsExactSameMapWarp(
                catalog.Records[0],
                SchoolWarpRecordOrdinal,
                SchoolWarpTriggerX,
                SchoolWarpTriggerY,
                SchoolWarpDestinationX,
                SchoolWarpDestinationY,
                SchoolWarpOpaqueFacing) &&
            IsExactSameMapWarp(
                catalog.Records[1],
                HouseWarpRecordOrdinal,
                HouseWarpTriggerX,
                HouseWarpTriggerY,
                HouseWarpDestinationX,
                HouseWarpDestinationY,
                HouseWarpOpaqueFacing);
    }

    public static bool HasExactAcceptedRoofOnLoadClear(
        OriginalMapRoofOnLoadDefinition? definition)
    {
        if (definition is null)
        {
            return false;
        }

        OriginalMapRoofOnLoadIdentity identity = definition.Identity;
        return identity.Profile == ContentProfile.PrivateLocal &&
            identity.Map == new MapId(MapId) &&
            string.Equals(identity.ResourceId, RoofOnLoadResourceId, StringComparison.Ordinal) &&
            identity.OneBasedRecordOrdinal == HouseRoofOnLoadRecordOrdinal &&
            definition.SourceTrigger == new MapPosition(
                HouseRoofSourceTriggerX,
                HouseRoofSourceTriggerY) &&
            definition.ClearDestination == new MapPosition(
                HouseRoofClearDestinationX,
                HouseRoofClearDestinationY) &&
            definition.Width == HouseRoofClearWidth &&
            definition.Height == HouseRoofClearHeight &&
            definition.AppliedAfterWarp == new OriginalMapSameMapWarpIdentity(
                ContentProfile.PrivateLocal,
                new MapId(MapId),
                SameMapWarpResourceId,
                HouseWarpRecordOrdinal) &&
            definition.DestinationArea == new OriginalMapAreaRecordIdentity(
                AcceptedAreaResourceId,
                HouseRoofDestinationAreaOrdinal);
    }

    private static bool IsExactSameMapWarp(
        OriginalMapSameMapWarpDefinition definition,
        int oneBasedRecordOrdinal,
        int triggerX,
        int triggerY,
        int destinationX,
        int destinationY,
        byte opaqueFacing) =>
        definition.Identity.Profile == ContentProfile.PrivateLocal &&
        definition.Identity.Map == new MapId(MapId) &&
        string.Equals(
            definition.Identity.ResourceId,
            SameMapWarpResourceId,
            StringComparison.Ordinal) &&
        definition.Identity.OneBasedRecordOrdinal == oneBasedRecordOrdinal &&
        definition.Trigger == new MapPosition(triggerX, triggerY) &&
        definition.Destination == new MapPosition(destinationX, destinationY) &&
        definition.OpaqueFacing == opaqueFacing;

    public static bool HasExactAcceptedAreaProjection(OriginalMapTraversal traversal)
    {
        ArgumentNullException.ThrowIfNull(traversal);
        if (traversal.ActiveAreas.Count != AcceptedAreaRecordCount)
        {
            return false;
        }

        Span<byte> projection = stackalloc byte[1 + (AcceptedAreaRecordCount * 4)];
        projection[0] = AcceptedAreaRecordCount;
        for (int index = 0; index < traversal.ActiveAreas.Count; index++)
        {
            OriginalMapTraversalArea area = traversal.ActiveAreas[index];
            int offset = 1 + (index * 4);
            projection[offset] = checked((byte)area.MinimumX);
            projection[offset + 1] = checked((byte)area.MinimumY);
            projection[offset + 2] = checked((byte)area.MaximumX);
            projection[offset + 3] = checked((byte)area.MaximumY);
        }

        return string.Equals(
            Convert.ToHexString(SHA256.HashData(projection)),
            AcceptedAreaProjectionDigest,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasExactAcceptedAreaSourceProjection(OriginalMapAreaCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        if (!string.Equals(
                catalog.ResourceId,
                AcceptedAreaResourceId,
                StringComparison.Ordinal) ||
            catalog.Records.Count != AcceptedAreaRecordCount)
        {
            return false;
        }

        Span<byte> projection = stackalloc byte[1 + (AcceptedAreaRecordCount * 30)];
        projection[0] = AcceptedAreaRecordCount;
        int offset = 1;
        foreach (OriginalMapAreaDefinition record in catalog.Records)
        {
            WriteWord(projection, ref offset, checked((ushort)record.MainLayerBounds.MinimumX));
            WriteWord(projection, ref offset, checked((ushort)record.MainLayerBounds.MinimumY));
            WriteWord(projection, ref offset, checked((ushort)record.MainLayerBounds.MaximumX));
            WriteWord(projection, ref offset, checked((ushort)record.MainLayerBounds.MaximumY));
            WritePair(projection, ref offset, record.SecondLayerForegroundStart);
            WritePair(projection, ref offset, record.SecondLayerBackgroundStart);
            WritePair(projection, ref offset, record.MainLayerParallax);
            WritePair(projection, ref offset, record.SecondLayerParallax);
            projection[offset++] = record.MainLayerAutoscroll.X;
            projection[offset++] = record.MainLayerAutoscroll.Y;
            projection[offset++] = record.SecondLayerAutoscroll.X;
            projection[offset++] = record.SecondLayerAutoscroll.Y;
            projection[offset++] = record.MainLayerType;
            projection[offset++] = record.DefaultMusic;
        }

        return string.Equals(
            Convert.ToHexString(SHA256.HashData(projection)),
            AcceptedAreaSourceProjectionDigest,
            StringComparison.OrdinalIgnoreCase);
    }

    private static void WritePair(
        Span<byte> destination,
        ref int offset,
        OriginalMapAreaWordPair pair)
    {
        WriteWord(destination, ref offset, pair.X);
        WriteWord(destination, ref offset, pair.Y);
    }

    private static void WriteWord(Span<byte> destination, ref int offset, ushort value)
    {
        BinaryPrimitives.WriteUInt16BigEndian(destination[offset..], value);
        offset += sizeof(ushort);
    }
}
