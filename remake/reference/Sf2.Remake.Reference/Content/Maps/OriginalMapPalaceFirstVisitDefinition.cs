using System.Collections.ObjectModel;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public enum OriginalMapPalaceFirstVisitPreset
{
    ControlledClear605And507,
}

/// <summary>A fixed, explicitly selected result projection; it does not resolve natural caller flags.</summary>
public sealed record OriginalMapPalaceFirstVisitDefinition
{
    public OriginalMapPalaceFirstVisitDefinition(string initBodySha256, string scriptProjectionSha256)
    {
        OriginalMapImportRequest.ValidateSha256(initBodySha256, nameof(initBodySha256));
        OriginalMapImportRequest.ValidateSha256(scriptProjectionSha256, nameof(scriptProjectionSha256));
        InitBodySha256 = initBodySha256;
        ScriptProjectionSha256 = scriptProjectionSha256;
    }

    public string InitBodySha256 { get; }
    public string ScriptProjectionSha256 { get; }
    public MapId Map { get; } = new(OriginalMapRuntimeAdmission.Map20Id);
    public string InitIdentity => OriginalMapRuntimeAdmission.Map20SelectedInitIdentity;
    public string ProgramIdentity => "cs_53996";
    public string SharedTailIdentity => "cs_53B60";
    public uint PackedEntryPredicate => 0x22803780;
    public OriginalMapPalaceFirstVisitPreset Preset => OriginalMapPalaceFirstVisitPreset.ControlledClear605And507;
    public MapPosition Entry { get; } = new(23, 37);
    public MapPosition PlayerEndpoint { get; } = new(23, 39);
    public byte PlayerOpaqueFacing => 3;
    public int TextCursorId => 2176;
    public int CompletionFlag => 605;
    public int SourceOperationCount => 113;
    public int SharedTailOperationCount => 2;
    public OriginalMapEntityRecordIdentity HiddenEntity130 { get; } =
        new(OriginalMapRuntimeAdmission.Map20EntityListResourceId, 3);
    public OriginalMapEntityRecordIdentity MovedEntity131 { get; } =
        new(OriginalMapRuntimeAdmission.Map20EntityListResourceId, 4);
    public MapPosition Entity131Source { get; } = new(18, 40);
    public MapPosition Entity131Endpoint { get; } = new(20, 39);
    public ReadOnlyCollection<int> Entity131MoveOperationIndices { get; } = Array.AsReadOnly(new[] { 16, 69, 77 });
}
