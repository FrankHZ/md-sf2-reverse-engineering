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

    public const string Map19Id = "map19";
    public const string Map19BlocksetResourceId = "Map19s0_Blocks";
    public const int Map19BlockCount = 368;
    public const string Map19BlocksetProjectionDigest =
        "A956EAE66F0CEDA97E1F7ED2D09BA7030E79A8BC6A2CDA2F7A6044CB3000A805";
    public const string Map19DecodedLayoutDigest =
        "1AB1665A1BC051B14AEEDA51E51AF4BDC06D6AAB4120727EF38F5E6504274E4B";
    public const string Map19CollisionProjectionDigest =
        "1CF8302EB72974524DC4829147D24DF76E41C9F12FCC7A3199D351E3F5517D0A";
    public const string Map19AreaResourceId = "Map19s2_Areas";
    public const int Map19AreaRecordCount = 1;
    public const string Map19AreaProjectionDigest =
        "0C350E385F98C5CE7AEDAB22FF8EE5DDE008651A94040E5E6DE9B27D1049612B";
    public const string Map19AreaSourceProjectionDigest =
        "C2B27F6A2AD1A88D2EE2F0B2A2A579D8CBA9355DBEAC65029B40D1B7517C120C";
    public const string Map19SelectedSetupId = "ms_map19";
    public const string Map19SelectedInitIdentity = "ms_map19_InitFunction";
    public const string Map19EntityListResourceId = "ms_map19_Entities";
    public const int Map19EntityRecordCount = 13;
    public const int Map19FixedEntityRecordCount = 9;
    public const int Map19WalkingEntityRecordCount = 4;
    public const string Map19EntityProjectionDigest =
        "8471F30D66C6C68873D135F72E0C6A3FB909F886638F15A6F4F18EDBD27DDCDC";

    public const string Map20Id = "map20";
    public const string Map20BlocksetResourceId = "Map20s0_Blocks";
    public const int Map20BlockCount = 376;
    public const string Map20BlocksetProjectionDigest =
        "B4AF73B3A810A2431D1D88EFBBFA0C62029C13F689CF74C845B6893C31608F69";
    public const string Map20DecodedLayoutDigest =
        "FBB9851422BEE93242A644E00367F6AE08BE2D85B1A1828F79B3BDA2304FA749";
    public const string Map20CollisionProjectionDigest =
        "F7E37D09A609C9ABDBAC7A4F2DB069CBED2233B81DA3BF9B7335313E2EF32254";
    public const string Map20AreaResourceId = "Map20s2_Areas";
    public const int Map20AreaRecordCount = 3;
    public const string Map20AreaProjectionDigest =
        "56E9F7D7C6E7DB6F018612AFD47519C53CD4B69E62C75D6208A5D1C6DA7595C0";
    public const string Map20AreaSourceProjectionDigest =
        "1B458DB3012CC30FC7184E2CF2AD85F44D4EF0807A7CABE56E37358D1E37D29A";
    public const string Map20SelectedSetupId = "ms_map20";
    public const string Map20SelectedInitIdentity = "ms_map20_InitFunction";
    public const string Map20EntityListResourceId = "ms_map20_Entities";
    public const int Map20EntityRecordCount = 8;
    public const int Map20FixedEntityRecordCount = 7;
    public const int Map20WalkingEntityRecordCount = 1;
    public const string Map20EntityProjectionDigest =
        "7350F0612C704C1DBEE16E9F48399CB21BE3CC1B653261A8910DBF6661BCB02D";

    public const string RoyalMap20TransitionCapability = "private-map19-royal-map20-transition-v1";
    public const string RoyalMap20WarpResourceId = "Map19s6_WarpEvents";
    public const int RoyalMap20WarpRecordOrdinal = 2;
    public const byte RoyalMap20WarpSourceTriggerX = 23;
    public const byte RoyalMap20WarpSourceTriggerY = 3;
    public const int RoyalMap20WarpApproachX = 22;
    public const int RoyalMap20WarpApproachY = 4;
    public const ExplorationDirection RoyalMap20WarpDirection = ExplorationDirection.East;
    public const int RoyalMap20WarpTriggerX = 23;
    public const int RoyalMap20WarpTriggerY = 3;
    public const int RoyalMap20WarpDestinationX = 23;
    public const int RoyalMap20WarpDestinationY = 37;
    public const byte RoyalMap20WarpDestinationOpaqueFacing = 3;

    public const string PalaceFirstVisitCapability = "private-map20-controlled-first-visit-result-v1";
    public const string PalaceInitBodySha256 =
        "68809DD783F749F19112E33D7B70AC663C0101D5B720779437B7449706B6776C";
    // Imported operation-token projection, including the shared tail label. This is distinct
    // from the source-whitespace control-effect digest retained by the accepted H2 owner.
    public const string PalaceScriptProjectionSha256 =
        "0CF57192969A95347789B3857439F824230E8D58AB6CAF4B574B872C0B1A29B3";
    public const string PalaceSourceControlEffectSha256 =
        "CBFCC3AC371E2FBB9BA844B7A3C3DE0B6EA92549B5192A16EF3385E7A678A556";

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

    public const int NorthMap19WarpRecordOrdinal = 1;
    public const byte NorthMap19WarpSourceTriggerX = byte.MaxValue;
    public const byte NorthMap19WarpSourceTriggerY = 1;
    public const int NorthMap19WarpApproachX = 28;
    public const int NorthMap19WarpApproachY = 2;
    public const ExplorationDirection NorthMap19WarpDirection = ExplorationDirection.North;
    public const int NorthMap19WarpTriggerX = 28;
    public const int NorthMap19WarpTriggerY = 1;
    public const int NorthMap19WarpDestinationX = 26;
    public const int NorthMap19WarpDestinationY = 30;
    public const byte NorthMap19WarpDestinationOpaqueFacing = 1;

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

    public const int AstralZoneEventHandlerAddress = 331084;
    public const string AstralZoneEventResourceId = "ms_map3_ZoneEvents";
    public const int AstralZoneEventSourceRecordCount = 10;
    public const int AstralZoneEventRecordOrdinal = 8;
    public const int AstralZoneEventRecordAddress = 331112;
    public const int AstralZoneEventRelativeOffset = 282;
    public const int AstralZoneEventResolvedTargetAddress = 331366;
    public const string AstralZoneEventTargetIdentity = "Map3_ZoneEvent7";
    public const int AstralZoneTriggerX = 58;
    public const int AstralZoneTriggerY = 13;
    public const string AstralZonePositionProgramIdentity = "cs_5148C";
    public const int AstralZonePositionProgramAddress = 332940;
    public const int AstralZoneMessengerCompletionFlag603 = 603;
    public const int AstralZoneRequiredEntity142Flag602 = 602;
    public const int AstralZoneCompletionFlag260 = 260;
    public const int AstralZoneSarahDestinationX = 41;
    public const int AstralZoneSarahDestinationY = 10;
    public const byte AstralZoneSarahOpaqueFacing = 1;
    public const int AstralZoneActor128DestinationX = 6;
    public const int AstralZoneActor128DestinationY = 4;
    public const byte AstralZoneActor128OpaqueFacing = 1;

    public const int MessengerZoneEventHandlerAddress = 331084;
    public const string MessengerZoneEventResourceId = "ms_map3_ZoneEvents";
    public const int MessengerZoneEventSourceRecordCount = 10;
    public const int MessengerZoneEventRecordOrdinal = 9;
    public const int MessengerZoneEventRecordAddress = 331116;
    public const int MessengerZoneEventRelativeOffset = 390;
    public const int MessengerZoneEventResolvedTargetAddress = 331474;
    public const string MessengerZoneEventTargetIdentity = "Map3_ZoneEvent8";
    public const int MessengerApproachX = 42;
    public const int MessengerApproachY = 10;
    public const ExplorationDirection MessengerEntryDirection = ExplorationDirection.East;
    public const int MessengerTriggerX = 43;
    public const int MessengerTriggerY = 10;
    public const string MessengerProgramIdentity = "cs_5149A";
    public const int MessengerProgramAddress = 332954;
    public const int MessengerProgramOperationCount = 112;
    public const string MessengerAcceptedBranchProgramIdentity = "cs_51614";
    public const int MessengerAcceptedBranchProgramAddress = 333332;
    public const int MessengerAcceptedBranchOperationCount = 11;
    public const int MessengerProgramEndAddress = 333392;
    public const string MessengerControlShapeSha256 =
        "B542F358F80A1537D767AB7CBFFD91E886248B9805ED3111503C32FF29177586";
    public const int MessengerPromptReturn = 0;
    public const int MessengerPromptFlag89 = 89;
    public const int MessengerJoinSelector = 128;
    public const int MessengerFlag600 = 600;
    public const int MessengerFlag66 = 66;
    public const int MessengerCompletionFlag603 = 603;
    public const int MessengerSarahCharacterId = 1;
    public const int MessengerChesterCharacterId = 2;
    public const int MessengerBowieCharacterId = 0;
    public const int MessengerFollowerDistance = 2;
    public const int MessengerActor143SourceRecordOrdinal = 18;
    public const int MessengerActor143LogicalId = 143;
    public const int MessengerActor143SourceAddress = 330680;
    public const int MessengerActor143InitialX = 63;
    public const int MessengerActor143InitialY = 63;
    public const byte MessengerActor143InitialOpaqueFacing = 1;
    public const byte MessengerActor143MapSprite = 206;
    public const uint MessengerActor143ActionValue = 0x000460CE;
    public const int MessengerGuard138SourceRecordOrdinal = 13;
    public const int MessengerGuard138SourceAddress = 330640;
    public const int MessengerGuard138LogicalId = 138;
    public const int MessengerGuard138X = 27;
    public const int MessengerGuard138Y = 3;
    public const byte MessengerGuard138OpaqueFacing = 3;
    public const int MessengerGuard139SourceRecordOrdinal = 14;
    public const int MessengerGuard139SourceAddress = 330648;
    public const int MessengerGuard139LogicalId = 139;
    public const int MessengerGuard139X = 31;
    public const int MessengerGuard139Y = 3;
    public const byte MessengerGuard139OpaqueFacing = 3;
    public const byte MessengerGuardMapSprite = 206;
    public const uint MessengerGuardActionValue = 0x000460CE;
    public const byte MessengerEndpointOpaqueFacing = 3;
    public const string MessengerTerminalIdentity = "WaitForEvent";

    public const int CastleGateZoneEventHandlerAddress = 331084;
    public const string CastleGateZoneEventResourceId = "ms_map3_ZoneEvents";
    public const int CastleGateZoneEventSourceRecordCount = 10;
    public const int CastleGateZoneEventRecordOrdinal = 6;
    public const int CastleGateZoneEventRecordAddress = 331104;
    public const int CastleGateZoneEventRelativeOffset = 172;
    public const int CastleGateZoneEventResolvedTargetAddress = 331256;
    public const string CastleGateZoneEventTargetIdentity = "Map3_ZoneEvent4";
    public const int CastleGateApproachX = 31;
    public const int CastleGateApproachY = 6;
    public const ExplorationDirection CastleGateEntryDirection = ExplorationDirection.North;
    public const int CastleGateTriggerX = 31;
    public const int CastleGateTriggerY = 5;
    public const string CastleGateProgramIdentity = "cs_51652";
    public const int CastleGateProgramAddress = 333394;
    public const int CastleGateProgramOperationCount = 7;
    public const int CastleGateSourceProgramOperationCount = 26;
    public const string CastleGateControlShapeSha256 =
        "09BB739724F4F4D21C4C44D1466E962418948EBDEDFD7DE36DA4F31C2220CA26";
    public const int CastleGateTextCursorId = 537;
    public const int CastleGateCompletionFlag604 = 604;
    public const int CastleGateGuard138DestinationX = 28;
    public const int CastleGateGuard138DestinationY = 3;
    public const ExplorationDirection CastleGateGuard138Direction = ExplorationDirection.East;
    public const int CastleGateGuard139DestinationX = 30;
    public const int CastleGateGuard139DestinationY = 3;
    public const ExplorationDirection CastleGateGuard139Direction = ExplorationDirection.West;

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
    public const string AstralZoneHandoffCapability =
        "private-local-map3-astral-zone-handoff-v1";
    public const string MessengerAcceptanceCapability =
        "private-local-map3-messenger-acceptance-v1";
    public const string CastleGateOpeningCapability =
        "private-local-map3-castle-gate-opening-v1";
    public const string NorthMap19TransitionCapability =
        "private-local-map3-north-map19-transition-v1";

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

    private static readonly ReadOnlyCollection<int> ReadOnlyAstralZoneTextIds =
        Array.AsReadOnly(new[] { 514, 515, 516 });

    private static readonly ReadOnlyCollection<OriginalMapAstralZoneStage>
        ReadOnlyAstralZoneStages = Array.AsReadOnly(
        [
            OriginalMapAstralZoneStage.ReadMessengerFlag603Clear,
            OriginalMapAstralZoneStage.ReadEntity142Flag602Set,
            OriginalMapAstralZoneStage.ReadCompletionFlag260Clear,
            OriginalMapAstralZoneStage.PresentText514,
            OriginalMapAstralZoneStage.PresentText515,
            OriginalMapAstralZoneStage.PresentText516,
            OriginalMapAstralZoneStage.RunPositionProgram,
            OriginalMapAstralZoneStage.SetCompletionFlag260,
        ]);

    private static readonly ReadOnlyCollection<int> ReadOnlyMessengerTextIds =
        Array.AsReadOnly(new[]
        {
            517, 518, 519, 520, 521, 522, 523, 524, 525,
            526, 527, 528, 529, 530, 531, 535, 536, 447,
        });

    private static readonly ReadOnlyCollection<int?> ReadOnlyMessengerSpeakerOperands =
        Array.AsReadOnly<int?>(
        [
            142, 142, 142, 143, 143, 142, 143, 142, 142,
            2, 2, 49153, 2, 49153, 49153, 1, 2, null,
        ]);

    private static readonly ReadOnlyCollection<int> ReadOnlyMessengerJoinedCharacterIds =
        Array.AsReadOnly(new[] { MessengerSarahCharacterId, MessengerChesterCharacterId });

    private static readonly ReadOnlyCollection<OriginalMapMessengerFollowerLink>
        ReadOnlyMessengerFollowers = Array.AsReadOnly(
        [
            new OriginalMapMessengerFollowerLink(
                MessengerSarahCharacterId,
                MessengerBowieCharacterId,
                MessengerFollowerDistance),
            new OriginalMapMessengerFollowerLink(
                MessengerChesterCharacterId,
                MessengerSarahCharacterId,
                MessengerFollowerDistance),
        ]);

    private static readonly ReadOnlyCollection<OriginalMapMessengerGuardState>
        ReadOnlyMessengerGuards = Array.AsReadOnly(
        [
            new OriginalMapMessengerGuardState(
                MessengerGuard138LogicalId,
                new OriginalMapEntityRecordIdentity(
                    AcceptedEntityListResourceId,
                    MessengerGuard138SourceRecordOrdinal),
                new MapPosition(MessengerGuard138X, MessengerGuard138Y),
                MessengerGuard138OpaqueFacing),
            new OriginalMapMessengerGuardState(
                MessengerGuard139LogicalId,
                new OriginalMapEntityRecordIdentity(
                    AcceptedEntityListResourceId,
                    MessengerGuard139SourceRecordOrdinal),
                new MapPosition(MessengerGuard139X, MessengerGuard139Y),
                MessengerGuard139OpaqueFacing),
        ]);

    private static readonly ReadOnlyCollection<OriginalMapMessengerAcceptanceStage>
        ReadOnlyMessengerStages = Array.AsReadOnly(
        [
            OriginalMapMessengerAcceptanceStage.EnterMessengerProgram,
            OriginalMapMessengerAcceptanceStage.PresentPrePromptTextSequence,
            OriginalMapMessengerAcceptanceStage.AcceptDefaultPrompt,
            OriginalMapMessengerAcceptanceStage.ObservePromptFlag89Set,
            OriginalMapMessengerAcceptanceStage.PresentAcceptedBranchTextSequence,
            OriginalMapMessengerAcceptanceStage.SetFlag600,
            OriginalMapMessengerAcceptanceStage.SetFlag66,
            OriginalMapMessengerAcceptanceStage.JoinSarah,
            OriginalMapMessengerAcceptanceStage.JoinChester,
            OriginalMapMessengerAcceptanceStage.LinkSarahToBowie,
            OriginalMapMessengerAcceptanceStage.LinkChesterToSarah,
            OriginalMapMessengerAcceptanceStage.PositionGuard138,
            OriginalMapMessengerAcceptanceStage.PositionGuard139,
            OriginalMapMessengerAcceptanceStage.ReturnMessengerProgram,
            OriginalMapMessengerAcceptanceStage.SetCompletionFlag603,
            OriginalMapMessengerAcceptanceStage.ReachStableWaitForEvent,
        ]);

    private static readonly ReadOnlyCollection<OriginalMapCastleGateGuardMove>
        ReadOnlyCastleGateGuardMoves = Array.AsReadOnly(
        [
            new OriginalMapCastleGateGuardMove(
                MessengerGuard138LogicalId,
                new OriginalMapEntityRecordIdentity(
                    AcceptedEntityListResourceId,
                    MessengerGuard138SourceRecordOrdinal),
                new MapPosition(MessengerGuard138X, MessengerGuard138Y),
                CastleGateGuard138Direction,
                new MapPosition(
                    CastleGateGuard138DestinationX,
                    CastleGateGuard138DestinationY)),
            new OriginalMapCastleGateGuardMove(
                MessengerGuard139LogicalId,
                new OriginalMapEntityRecordIdentity(
                    AcceptedEntityListResourceId,
                    MessengerGuard139SourceRecordOrdinal),
                new MapPosition(MessengerGuard139X, MessengerGuard139Y),
                CastleGateGuard139Direction,
                new MapPosition(
                    CastleGateGuard139DestinationX,
                    CastleGateGuard139DestinationY)),
        ]);

    private static readonly ReadOnlyCollection<OriginalMapCastleGateStage>
        ReadOnlyCastleGateStages = Array.AsReadOnly(
        [
            OriginalMapCastleGateStage.SetTextCursor537,
            OriginalMapCastleGateStage.BeginGuard138Actions,
            OriginalMapCastleGateStage.MoveGuard138RightOne,
            OriginalMapCastleGateStage.EndGuard138Actions,
            OriginalMapCastleGateStage.BeginGuard139ActionsAndWait,
            OriginalMapCastleGateStage.MoveGuard139LeftOne,
            OriginalMapCastleGateStage.EndCastleGateProgram,
        ]);

    private static readonly ReadOnlyCollection<int>
        ReadOnlyCastleGateProjectionSourceOperationIndices = Array.AsReadOnly(
        [
            0,
            1,
            2,
            3,
            4,
            5,
            25,
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
                AstralZoneHandoffCapability,
                MessengerAcceptanceCapability,
                CastleGateOpeningCapability,
                NorthMap19TransitionCapability,
                RoyalMap20TransitionCapability,
                PalaceFirstVisitCapability,
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
                "sf2-map3-messenger-acceptance-runtime-v1",
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

    public static IReadOnlyList<int> AstralZoneTextIds => ReadOnlyAstralZoneTextIds;

    public static IReadOnlyList<OriginalMapAstralZoneStage> AstralZoneStages =>
        ReadOnlyAstralZoneStages;

    public static IReadOnlyList<int> MessengerTextIds => ReadOnlyMessengerTextIds;

    public static IReadOnlyList<int?> MessengerSpeakerOperands =>
        ReadOnlyMessengerSpeakerOperands;

    public static IReadOnlyList<int> MessengerJoinedCharacterIds =>
        ReadOnlyMessengerJoinedCharacterIds;

    public static IReadOnlyList<OriginalMapMessengerFollowerLink> MessengerFollowers =>
        ReadOnlyMessengerFollowers;

    public static IReadOnlyList<OriginalMapMessengerGuardState> MessengerGuards =>
        ReadOnlyMessengerGuards;

    public static IReadOnlyList<OriginalMapMessengerAcceptanceStage> MessengerStages =>
        ReadOnlyMessengerStages;

    public static IReadOnlyList<OriginalMapCastleGateGuardMove> CastleGateGuardMoves =>
        ReadOnlyCastleGateGuardMoves;

    public static IReadOnlyList<OriginalMapCastleGateStage> CastleGateStages =>
        ReadOnlyCastleGateStages;

    public static IReadOnlyList<int> CastleGateProjectionSourceOperationIndices =>
        ReadOnlyCastleGateProjectionSourceOperationIndices;

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

    public static bool HasExactAcceptedRuntimeCatalog(
        OriginalMapExplorationRuntimeCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        if (catalog.Records.Count != 3)
        {
            return false;
        }

        try
        {
            OriginalMapExplorationRuntimeDefinition map3 = catalog.Resolve(new MapId(MapId));
            OriginalMapExplorationRuntimeDefinition map19 = catalog.Resolve(new MapId(Map19Id));
            return map3.SelectedSetup == new MapSetupId(SelectedSetupId) &&
                string.Equals(
                    map3.SelectedInitIdentity,
                    SelectedInitIdentity,
                    StringComparison.Ordinal) &&
                string.Equals(
                    map3.DecodedLayoutDigest,
                    AcceptedDecodedLayoutDigest,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    map3.CollisionProjectionDigest,
                    AcceptedCollisionProjectionDigest,
                    StringComparison.OrdinalIgnoreCase) &&
                HasExactAcceptedBlocksetProjection(map3.BlockCatalog) &&
                HasExactAcceptedAreaProjection(map3.Traversal) &&
                HasExactAcceptedAreaSourceProjection(map3.AreaCatalog) &&
                HasExactAcceptedEntityPopulation(map3.EntityPopulation) &&
                HasExactAcceptedMap19Runtime(map19) &&
                HasExactAcceptedMap20Runtime(catalog.Resolve(new MapId(Map20Id)));
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    public static bool HasExactAcceptedRoyalMap20Transition(
        OriginalMapCrossMapTransitionDefinition? transition)
    {
        return transition is not null &&
            transition.Identity.Profile == ContentProfile.PrivateLocal &&
            transition.Identity.SourceMap == new MapId(Map19Id) &&
            string.Equals(
                transition.Identity.SourceResourceId,
                RoyalMap20WarpResourceId,
                StringComparison.Ordinal) &&
            transition.Identity.OneBasedRecordOrdinal == RoyalMap20WarpRecordOrdinal &&
            transition.SourceTriggerX == RoyalMap20WarpSourceTriggerX &&
            transition.SourceTriggerY == RoyalMap20WarpSourceTriggerY &&
            transition.AdmittedApproach == new MapPosition(
                RoyalMap20WarpApproachX,
                RoyalMap20WarpApproachY) &&
            transition.AdmittedDirection == RoyalMap20WarpDirection &&
            transition.AdmittedTrigger == new MapPosition(
                RoyalMap20WarpTriggerX,
                RoyalMap20WarpTriggerY) &&
            transition.DestinationMap == new MapId(Map20Id) &&
            transition.Destination == new MapPosition(
                RoyalMap20WarpDestinationX,
                RoyalMap20WarpDestinationY) &&
            transition.DestinationOpaqueFacing == RoyalMap20WarpDestinationOpaqueFacing;
    }

    public static bool HasExactAcceptedPalaceFirstVisit(
        OriginalMapPalaceFirstVisitDefinition? definition,
        OriginalMapExplorationRuntimeCatalog catalog)
    {
        if (definition is null ||
            !string.Equals(definition.InitBodySha256, PalaceInitBodySha256, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(definition.ScriptProjectionSha256, PalaceScriptProjectionSha256, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        OriginalMapExplorationRuntimeDefinition runtime = catalog.Resolve(definition.Map);
        return runtime.Traversal.IsWithinActiveArea(definition.PlayerEndpoint) &&
            !OriginalMapTraversal.IsBlocked(runtime.WorkingLayout, definition.PlayerEndpoint) &&
            runtime.EntityPopulation.Records[2].Identity == definition.HiddenEntity130 &&
            runtime.EntityPopulation.Records[2].Position == new MapPosition(19, 39) &&
            runtime.EntityPopulation.Records[3].Identity == definition.MovedEntity131 &&
            runtime.EntityPopulation.Records[3].Position == definition.Entity131Source;
    }

    public static bool HasExactAcceptedNorthMap19Transition(
        OriginalMapCrossMapTransitionDefinition? transition)
    {
        return transition is not null &&
            transition.Identity.Profile == ContentProfile.PrivateLocal &&
            transition.Identity.SourceMap == new MapId(MapId) &&
            string.Equals(
                transition.Identity.SourceResourceId,
                SameMapWarpResourceId,
                StringComparison.Ordinal) &&
            transition.Identity.OneBasedRecordOrdinal == NorthMap19WarpRecordOrdinal &&
            transition.SourceTriggerX == NorthMap19WarpSourceTriggerX &&
            transition.SourceTriggerY == NorthMap19WarpSourceTriggerY &&
            transition.AdmittedApproach == new MapPosition(
                NorthMap19WarpApproachX,
                NorthMap19WarpApproachY) &&
            transition.AdmittedDirection == NorthMap19WarpDirection &&
            transition.AdmittedTrigger == new MapPosition(
                NorthMap19WarpTriggerX,
                NorthMap19WarpTriggerY) &&
            transition.DestinationMap == new MapId(Map19Id) &&
            transition.Destination == new MapPosition(
                NorthMap19WarpDestinationX,
                NorthMap19WarpDestinationY) &&
            transition.DestinationOpaqueFacing == NorthMap19WarpDestinationOpaqueFacing;
    }

    private static bool HasExactAcceptedMap19Runtime(
        OriginalMapExplorationRuntimeDefinition runtime)
    {
        return runtime.Map == new MapId(Map19Id) &&
            runtime.SelectedSetup == new MapSetupId(Map19SelectedSetupId) &&
            string.Equals(
                runtime.SelectedInitIdentity,
                Map19SelectedInitIdentity,
                StringComparison.Ordinal) &&
            string.Equals(
                runtime.DecodedLayoutDigest,
                Map19DecodedLayoutDigest,
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                runtime.CollisionProjectionDigest,
                Map19CollisionProjectionDigest,
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                runtime.BlockCatalog.ResourceId,
                Map19BlocksetResourceId,
                StringComparison.Ordinal) &&
            runtime.BlockCatalog.Records.Count == Map19BlockCount &&
            string.Equals(
                runtime.BlockCatalog.ProjectionDigest,
                Map19BlocksetProjectionDigest,
                StringComparison.OrdinalIgnoreCase) &&
            HasExactMap19AreaProjection(runtime.AreaCatalog) &&
            runtime.EntityPopulation.Map == new MapId(Map19Id) &&
            runtime.EntityPopulation.SelectedSetup == new MapSetupId(Map19SelectedSetupId) &&
            string.Equals(
                runtime.EntityPopulation.ResourceId,
                Map19EntityListResourceId,
                StringComparison.Ordinal) &&
            runtime.EntityPopulation.Records.Count == Map19EntityRecordCount &&
            runtime.EntityPopulation.Records.Count(record =>
                record.Kind == OriginalMapEntityRecordKind.Fixed) ==
                Map19FixedEntityRecordCount &&
            runtime.EntityPopulation.Records.Count(record =>
                record.Kind == OriginalMapEntityRecordKind.Walking) ==
                Map19WalkingEntityRecordCount &&
            string.Equals(
                runtime.EntityPopulation.ProjectionDigest,
                Map19EntityProjectionDigest,
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasExactAcceptedMap20Runtime(
        OriginalMapExplorationRuntimeDefinition runtime)
    {
        return runtime.Map == new MapId(Map20Id) &&
            runtime.SelectedSetup == new MapSetupId(Map20SelectedSetupId) &&
            string.Equals(
                runtime.SelectedInitIdentity,
                Map20SelectedInitIdentity,
                StringComparison.Ordinal) &&
            string.Equals(
                runtime.DecodedLayoutDigest,
                Map20DecodedLayoutDigest,
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                runtime.CollisionProjectionDigest,
                Map20CollisionProjectionDigest,
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                runtime.BlockCatalog.ResourceId,
                Map20BlocksetResourceId,
                StringComparison.Ordinal) &&
            runtime.BlockCatalog.Records.Count == Map20BlockCount &&
            string.Equals(
                runtime.BlockCatalog.ProjectionDigest,
                Map20BlocksetProjectionDigest,
                StringComparison.OrdinalIgnoreCase) &&
            HasExactMap20AreaProjection(runtime.AreaCatalog) &&
            runtime.EntityPopulation.Map == new MapId(Map20Id) &&
            runtime.EntityPopulation.SelectedSetup == new MapSetupId(Map20SelectedSetupId) &&
            string.Equals(
                runtime.EntityPopulation.ResourceId,
                Map20EntityListResourceId,
                StringComparison.Ordinal) &&
            runtime.EntityPopulation.Records.Count == Map20EntityRecordCount &&
            runtime.EntityPopulation.Records.Count(record =>
                record.Kind == OriginalMapEntityRecordKind.Fixed) ==
                Map20FixedEntityRecordCount &&
            runtime.EntityPopulation.Records.Count(record =>
                record.Kind == OriginalMapEntityRecordKind.Walking) ==
                Map20WalkingEntityRecordCount &&
            string.Equals(
                runtime.EntityPopulation.ProjectionDigest,
                Map20EntityProjectionDigest,
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasExactMap19AreaProjection(OriginalMapAreaCatalog catalog) =>
        HasExactCastleAreaProjection(catalog, Map19AreaResourceId, Map19AreaRecordCount,
            Map19AreaProjectionDigest, Map19AreaSourceProjectionDigest);

    private static bool HasExactMap20AreaProjection(OriginalMapAreaCatalog catalog) =>
        HasExactCastleAreaProjection(catalog, Map20AreaResourceId, Map20AreaRecordCount,
            Map20AreaProjectionDigest, Map20AreaSourceProjectionDigest);

    private static bool HasExactCastleAreaProjection(
        OriginalMapAreaCatalog catalog, string resourceId, int recordCount,
        string boundsDigest, string sourceDigest)
    {
        if (!string.Equals(
                catalog.ResourceId,
                resourceId,
                StringComparison.Ordinal) ||
            catalog.Records.Count != recordCount)
        {
            return false;
        }

        byte[] bounds = new byte[1 + recordCount * 4];
        byte[] source = new byte[1 + recordCount * 30];
        bounds[0] = checked((byte)recordCount);
        source[0] = checked((byte)recordCount);
        int boundsOffset = 1;
        int offset = 1;
        foreach (OriginalMapAreaDefinition area in catalog.Records)
        {
            bounds[boundsOffset++] = checked((byte)area.MainLayerBounds.MinimumX);
            bounds[boundsOffset++] = checked((byte)area.MainLayerBounds.MinimumY);
            bounds[boundsOffset++] = checked((byte)area.MainLayerBounds.MaximumX);
            bounds[boundsOffset++] = checked((byte)area.MainLayerBounds.MaximumY);
            WriteWord(source, ref offset, checked((ushort)area.MainLayerBounds.MinimumX));
            WriteWord(source, ref offset, checked((ushort)area.MainLayerBounds.MinimumY));
            WriteWord(source, ref offset, checked((ushort)area.MainLayerBounds.MaximumX));
            WriteWord(source, ref offset, checked((ushort)area.MainLayerBounds.MaximumY));
            WritePair(source, ref offset, area.SecondLayerForegroundStart);
            WritePair(source, ref offset, area.SecondLayerBackgroundStart);
            WritePair(source, ref offset, area.MainLayerParallax);
            WritePair(source, ref offset, area.SecondLayerParallax);
            source[offset++] = area.MainLayerAutoscroll.X;
            source[offset++] = area.MainLayerAutoscroll.Y;
            source[offset++] = area.SecondLayerAutoscroll.X;
            source[offset++] = area.SecondLayerAutoscroll.Y;
            source[offset++] = area.MainLayerType;
            source[offset++] = area.DefaultMusic;
        }
        return string.Equals(
                Convert.ToHexString(SHA256.HashData(bounds)),
                boundsDigest,
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                Convert.ToHexString(SHA256.HashData(source)),
                sourceDigest,
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

    public static bool HasExactAcceptedAstralZone(
        OriginalMapAstralZoneDefinition? definition,
        OriginalMapSarahDefinition? sarah,
        OriginalMapZone601Definition? zone601,
        OriginalMapTraversal traversal,
        WorkingMapLayout workingLayout)
    {
        ArgumentNullException.ThrowIfNull(traversal);
        ArgumentNullException.ThrowIfNull(workingLayout);
        if (definition is null || sarah is null || zone601 is null ||
            definition.Identity != new OriginalMapAstralZoneEventIdentity(
                ContentProfile.PrivateLocal,
                new MapId(MapId),
                new MapSetupId(SelectedSetupId),
                AstralZoneEventResourceId,
                AstralZoneEventRecordOrdinal,
                AstralZoneEventTargetIdentity) ||
            definition.Trigger != new MapPosition(AstralZoneTriggerX, AstralZoneTriggerY) ||
            !string.Equals(
                definition.PositionProgramIdentity,
                AstralZonePositionProgramIdentity,
                StringComparison.Ordinal) ||
            definition.MessengerCompletionFlag603 != AstralZoneMessengerCompletionFlag603 ||
            definition.RequiredEntity142Flag602 != AstralZoneRequiredEntity142Flag602 ||
            definition.CompletionFlag260 != AstralZoneCompletionFlag260 ||
            definition.SarahSourceRecord != sarah.ActorSourceRecord ||
            definition.SarahLogicalActorId != sarah.LogicalActorId ||
            definition.SarahDestination != new MapPosition(
                AstralZoneSarahDestinationX,
                AstralZoneSarahDestinationY) ||
            definition.SarahOpaqueFacing != AstralZoneSarahOpaqueFacing ||
            definition.Zone601ActorSourceRecord != zone601.ActorSourceRecord ||
            definition.Zone601LogicalActorId != zone601.LogicalActorId ||
            definition.Zone601ActorDestination != new MapPosition(
                AstralZoneActor128DestinationX,
                AstralZoneActor128DestinationY) ||
            definition.Zone601ActorOpaqueFacing != AstralZoneActor128OpaqueFacing ||
            !definition.TextIds.SequenceEqual(ReadOnlyAstralZoneTextIds) ||
            !definition.Stages.SequenceEqual(ReadOnlyAstralZoneStages) ||
            !traversal.IsWithinActiveArea(definition.Trigger) ||
            !traversal.IsWithinActiveArea(definition.SarahDestination) ||
            !traversal.IsWithinActiveArea(definition.Zone601ActorDestination) ||
            OriginalMapTraversal.IsBlocked(workingLayout, definition.Trigger))
        {
            return false;
        }

        return true;
    }

    public static bool HasExactAcceptedMessengerAcceptance(
        OriginalMapMessengerAcceptanceDefinition? definition,
        OriginalMapEntityPopulation population,
        OriginalMapSarahDefinition? sarah,
        OriginalMapEntity142Definition? entity142,
        OriginalMapAstralZoneDefinition? astralZone,
        OriginalMapTraversal traversal,
        WorkingMapLayout workingLayout)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(traversal);
        ArgumentNullException.ThrowIfNull(workingLayout);
        if (definition is null || sarah is null || entity142 is null || astralZone is null ||
            definition.Identity != new OriginalMapMessengerZoneEventIdentity(
                ContentProfile.PrivateLocal,
                new MapId(MapId),
                new MapSetupId(SelectedSetupId),
                MessengerZoneEventResourceId,
                MessengerZoneEventRecordOrdinal,
                MessengerZoneEventTargetIdentity) ||
            definition.Approach != new MapPosition(MessengerApproachX, MessengerApproachY) ||
            definition.EntryDirection != MessengerEntryDirection ||
            definition.Trigger != new MapPosition(MessengerTriggerX, MessengerTriggerY) ||
            !string.Equals(definition.MessengerProgramIdentity, MessengerProgramIdentity,
                StringComparison.Ordinal) ||
            !string.Equals(
                definition.AcceptedBranchProgramIdentity,
                MessengerAcceptedBranchProgramIdentity,
                StringComparison.Ordinal) ||
            !string.Equals(definition.ControlShapeSha256, MessengerControlShapeSha256,
                StringComparison.OrdinalIgnoreCase) ||
            definition.PromptReturn != MessengerPromptReturn ||
            definition.PromptFlag89 != MessengerPromptFlag89 ||
            definition.JoinSelector != MessengerJoinSelector ||
            definition.Flag600 != MessengerFlag600 || definition.Flag66 != MessengerFlag66 ||
            definition.CompletionFlag603 != MessengerCompletionFlag603 ||
            definition.SarahSourceRecord != sarah.ActorSourceRecord ||
            definition.SarahCharacterId != MessengerSarahCharacterId ||
            definition.Entity142SourceRecord != entity142.ActorSourceRecord ||
            definition.Entity142LogicalActorId != entity142.LogicalActorId ||
            definition.MessengerActorSourceRecord != new OriginalMapEntityRecordIdentity(
                AcceptedEntityListResourceId,
                MessengerActor143SourceRecordOrdinal) ||
            definition.MessengerLogicalActorId != MessengerActor143LogicalId ||
            definition.MessengerActorInitialPosition != new MapPosition(
                MessengerActor143InitialX,
                MessengerActor143InitialY) ||
            definition.MessengerActorInitialOpaqueFacing !=
                MessengerActor143InitialOpaqueFacing ||
            !definition.TextIds.SequenceEqual(ReadOnlyMessengerTextIds) ||
            !definition.SpeakerOperands.SequenceEqual(ReadOnlyMessengerSpeakerOperands) ||
            !definition.JoinedCharacterIds.SequenceEqual(ReadOnlyMessengerJoinedCharacterIds) ||
            !definition.Followers.SequenceEqual(ReadOnlyMessengerFollowers) ||
            !definition.Guards.SequenceEqual(ReadOnlyMessengerGuards) ||
            definition.Endpoint != new MapPosition(MessengerTriggerX, MessengerTriggerY) ||
            definition.EndpointOpaqueFacing != MessengerEndpointOpaqueFacing ||
            !string.Equals(definition.TerminalIdentity, MessengerTerminalIdentity,
                StringComparison.Ordinal) ||
            !definition.Stages.SequenceEqual(ReadOnlyMessengerStages) ||
            astralZone.CompletionFlag260 != AstralZoneCompletionFlag260 ||
            astralZone.RequiredEntity142Flag602 != entity142.CompletionFlag602 ||
            astralZone.MessengerCompletionFlag603 != definition.CompletionFlag603 ||
            population.Map != definition.Identity.Map ||
            population.SelectedSetup != definition.Identity.Setup ||
            population.ResourceId != AcceptedEntityListResourceId ||
            population.Records.Count != AcceptedEntityRecordCount ||
            !traversal.IsWithinActiveArea(definition.Approach) ||
            !traversal.IsWithinActiveArea(definition.Trigger) ||
            OriginalMapTraversal.IsBlocked(workingLayout, definition.Approach) ||
            OriginalMapTraversal.IsBlocked(workingLayout, definition.Trigger))
        {
            return false;
        }

        OriginalMapEntityDefinition messenger =
            population.Records[MessengerActor143SourceRecordOrdinal - 1];
        OriginalMapEntityDefinition guard138 =
            population.Records[MessengerGuard138SourceRecordOrdinal - 1];
        OriginalMapEntityDefinition guard139 =
            population.Records[MessengerGuard139SourceRecordOrdinal - 1];
        return MatchesFixedEntity(
                messenger,
                MessengerActor143SourceRecordOrdinal,
                MessengerActor143InitialX,
                MessengerActor143InitialY,
                MessengerActor143InitialOpaqueFacing,
                MessengerActor143MapSprite,
                MessengerActor143ActionValue) &&
            MatchesFixedEntity(
                guard138,
                MessengerGuard138SourceRecordOrdinal,
                MessengerGuard138X,
                MessengerGuard138Y,
                MessengerGuard138OpaqueFacing,
                MessengerGuardMapSprite,
                MessengerGuardActionValue) &&
            MatchesFixedEntity(
                guard139,
                MessengerGuard139SourceRecordOrdinal,
                MessengerGuard139X,
                MessengerGuard139Y,
                MessengerGuard139OpaqueFacing,
                MessengerGuardMapSprite,
                MessengerGuardActionValue);
    }

    public static bool HasExactAcceptedCastleGate(
        OriginalMapCastleGateDefinition? definition,
        OriginalMapMessengerAcceptanceDefinition? messengerAcceptance,
        OriginalMapTraversal traversal,
        WorkingMapLayout workingLayout)
    {
        ArgumentNullException.ThrowIfNull(traversal);
        ArgumentNullException.ThrowIfNull(workingLayout);
        if (definition is null || messengerAcceptance is null ||
            definition.Identity != new OriginalMapZoneEventIdentity(
                ContentProfile.PrivateLocal,
                new MapId(MapId),
                new MapSetupId(SelectedSetupId),
                CastleGateZoneEventResourceId,
                CastleGateZoneEventRecordOrdinal,
                CastleGateZoneEventTargetIdentity) ||
            definition.Approach != new MapPosition(CastleGateApproachX, CastleGateApproachY) ||
            definition.EntryDirection != CastleGateEntryDirection ||
            definition.Trigger != new MapPosition(CastleGateTriggerX, CastleGateTriggerY) ||
            !string.Equals(
                definition.ProgramIdentity,
                CastleGateProgramIdentity,
                StringComparison.Ordinal) ||
            !string.Equals(
                definition.ControlShapeSha256,
                CastleGateControlShapeSha256,
                StringComparison.OrdinalIgnoreCase) ||
            definition.TextCursorId != CastleGateTextCursorId ||
            definition.CompletionFlag != CastleGateCompletionFlag604 ||
            definition.SourceOperationCount != CastleGateSourceProgramOperationCount ||
            !definition.ProjectionSourceOperationIndices.SequenceEqual(
                ReadOnlyCastleGateProjectionSourceOperationIndices) ||
            !definition.GuardMoves.SequenceEqual(ReadOnlyCastleGateGuardMoves) ||
            !definition.Stages.SequenceEqual(ReadOnlyCastleGateStages) ||
            !traversal.IsWithinActiveArea(definition.Approach) ||
            !traversal.IsWithinActiveArea(definition.Trigger) ||
            OriginalMapTraversal.IsBlocked(workingLayout, definition.Approach) ||
            OriginalMapTraversal.IsBlocked(workingLayout, definition.Trigger) ||
            traversal.ResolveCandidateTarget(
                workingLayout,
                definition.Approach,
                definition.EntryDirection) != definition.Trigger)
        {
            return false;
        }

        return messengerAcceptance.Guards.Count == definition.GuardMoves.Count &&
            definition.GuardMoves.All(move =>
                messengerAcceptance.Guards.SingleOrDefault(guard =>
                    guard.LogicalActorId == move.LogicalActorId &&
                    guard.SourceRecord == move.SourceRecord &&
                    guard.Position == move.Source) is not null);
    }

    private static bool MatchesFixedEntity(
        OriginalMapEntityDefinition entity,
        int oneBasedRecordOrdinal,
        int x,
        int y,
        byte opaqueFacing,
        byte mapSprite,
        uint actionValue)
    {
        Span<byte> expectedAction = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32BigEndian(expectedAction, actionValue);
        return entity.Identity == new OriginalMapEntityRecordIdentity(
                AcceptedEntityListResourceId,
                oneBasedRecordOrdinal) &&
            entity.RawX == x && entity.RawY == y &&
            entity.Position == new MapPosition(x, y) &&
            entity.OpaqueFacing == opaqueFacing &&
            entity.MapSprite == mapSprite &&
            entity.Kind == OriginalMapEntityRecordKind.Fixed &&
            entity.OpaqueTail.SequenceEqual(expectedAction.ToArray());
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
