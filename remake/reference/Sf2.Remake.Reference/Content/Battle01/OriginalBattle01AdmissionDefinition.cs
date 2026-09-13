using System.Security.Cryptography;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalBattle01ControlledPreset(
    string Id, bool CompletedFlag501, bool SuspendedFlag88, bool IntroFlag451)
{
    public static OriginalBattle01ControlledPreset NewBattle { get; } =
        new("private-local-battle01-controlled-new-battle-v1", false, false, false);
}

// A battle destination has no exploration setup, init or entity-list identity.
public sealed class OriginalBattle01AdmissionDefinition
{
    public const string Capability = "private-local-battle01-pending-admission-v1";
    public const string MapIdValue = "map57";
    public const int BlockCount = 120;
    public const string LayoutDigest = "3F1F9421ECB9979F99206896659F468DB27680FF83C7E6760C488002446F6E97";
    public const string BlockDigest = "731C681FE78A532460103E2D2CCBA5AF1DD6A7BAD2321CF1C74D4CB9D8065061";
    public const string AreaBoundsDigest = "A07AC25D7A4F211E90A2F3FE5D9A46E53A9D4E7317D94A26FB346047427973F3";
    public const string AreaSourceDigest = "6B155830DA1CDE19A0889F8F43543BF0FB6B6F2E64FD35680F1D5E125BE9E716";

    public OriginalBattle01AdmissionDefinition(
        WorkingMapLayout workingLayout, OriginalMapBlockCatalog blockCatalog,
        OriginalMapAreaCatalog areaCatalog, OriginalMapVisualResourceSelection visualResourceSelection,
        OriginalMapCrossMapTransitionIdentity warpIdentity, byte triggerX, byte triggerY,
        MapPosition destination, byte destinationOpaqueFacing, OriginalBattle01ControlledPreset preset,
        string? setupRouteReference = null, string? animationTableReference = null)
        : this(workingLayout, blockCatalog, areaCatalog, visualResourceSelection, warpIdentity,
            triggerX, triggerY, destination, destinationOpaqueFacing, preset,
            setupRouteReference, animationTableReference, layoutDigestOverride: null) { }

    internal OriginalBattle01AdmissionDefinition(
        WorkingMapLayout workingLayout, OriginalMapBlockCatalog blockCatalog,
        OriginalMapAreaCatalog areaCatalog, OriginalMapVisualResourceSelection visualResourceSelection,
        OriginalMapCrossMapTransitionIdentity warpIdentity, byte triggerX, byte triggerY,
        MapPosition destination, byte destinationOpaqueFacing, OriginalBattle01ControlledPreset preset,
        string? setupRouteReference, string? animationTableReference, string? layoutDigestOverride)
    {
        WorkingLayout = workingLayout ?? throw new ArgumentNullException(nameof(workingLayout));
        BlockCatalog = blockCatalog ?? throw new ArgumentNullException(nameof(blockCatalog));
        AreaCatalog = areaCatalog ?? throw new ArgumentNullException(nameof(areaCatalog));
        VisualResourceSelection = visualResourceSelection ?? throw new ArgumentNullException(nameof(visualResourceSelection));
        WarpIdentity = warpIdentity ?? throw new ArgumentNullException(nameof(warpIdentity));
        Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        Preset = preset ?? throw new ArgumentNullException(nameof(preset));
        ArgumentException.ThrowIfNullOrWhiteSpace(preset.Id);
        TriggerX = triggerX;
        TriggerY = triggerY;
        DestinationOpaqueFacing = destinationOpaqueFacing;
        SetupRouteReference = setupRouteReference;
        AnimationTableReference = animationTableReference;
        blockCatalog.ValidateLayoutReferences(workingLayout, nameof(workingLayout));
        byte[] words = workingLayout.Words.SelectMany(word => new[] { (byte)(word >> 8), (byte)(word & 0xFF) }).ToArray();
        DecodedLayoutDigest = layoutDigestOverride ?? Convert.ToHexString(SHA256.HashData(words));
        OriginalMapImportRequest.ValidateSha256(DecodedLayoutDigest, nameof(layoutDigestOverride));
    }

    public WorkingMapLayout WorkingLayout { get; }
    public OriginalMapBlockCatalog BlockCatalog { get; }
    public OriginalMapAreaCatalog AreaCatalog { get; }
    public OriginalMapVisualResourceSelection VisualResourceSelection { get; }
    public MapId DestinationMap => VisualResourceSelection.Map;
    public OriginalMapCrossMapTransitionIdentity WarpIdentity { get; }
    public byte TriggerX { get; }
    public byte TriggerY { get; }
    public MapPosition Destination { get; }
    public byte DestinationOpaqueFacing { get; }
    public OriginalBattle01ControlledPreset Preset { get; }
    public string? SetupRouteReference { get; }
    public string? AnimationTableReference { get; }
    public string DecodedLayoutDigest { get; }
    // Fixed CheckBattle row 1 facts, separate from inclusive map-area bounds.
    public int BattleIndex => 1;
    public int BattleAreaX => 0;
    public int BattleAreaY => 0;
    public int BattleAreaWidth => 16;
    public int BattleAreaHeight => 20;
    public int UnlockedFlag => 401;
    public int CompletedFlag => 501;
    public byte BattleTriggerX => 255;
    public byte BattleTriggerY => 255;
}
