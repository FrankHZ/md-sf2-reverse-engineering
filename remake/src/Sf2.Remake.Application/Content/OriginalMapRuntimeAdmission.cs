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
    public const string AcceptedAreaResourceId = "Map03s2_Areas";
    public const int AcceptedAreaRecordCount = 3;
    public const int ControlledStartAreaRecordOrdinal = 2;
    public const string AcceptedAreaProjectionDigest =
        "A9C712C1E02FB4A03CA60E68FF3AEFE6CC71A9E07A986E0CEB46C9CD9C81A2A6";

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

    public const string ImportCapability = "private-canonical-map3-layout-import-v1";
    public const string TraversalCapability = "original-map3-traversal-policy-v1";
    public const string ControlledAdmissionCapability =
        "controlled-map3-import-definition-v1";
    public const string ControlledStepCopyCapability =
        "private-local-map3-controlled-step-copy-diagnostic-v1";
    public const string CurrentAreaDiagnosticCapability =
        "private-local-map3-current-area-diagnostic-v1";

    private static readonly ReadOnlyCollection<string> ReadOnlyRequiredCapabilities =
        Array.AsReadOnly(
            new[]
            {
                ImportCapability,
                TraversalCapability,
                ControlledAdmissionCapability,
                ControlledStepCopyCapability,
                CurrentAreaDiagnosticCapability,
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
                "sf2-map3-castle-battle-unlock-static-v1",
                "sf2-map3-admitted-start-runtime-v1",
            });

    public static IReadOnlyList<string> RequiredCapabilities => ReadOnlyRequiredCapabilities;

    public static IReadOnlyList<string> RequiredEvidenceOwners =>
        ReadOnlyRequiredEvidenceOwners;

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
}
