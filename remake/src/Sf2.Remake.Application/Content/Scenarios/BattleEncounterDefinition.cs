using System.Collections.ObjectModel;

namespace Sf2.Remake.Application.Content.Scenarios;

public abstract record BattleEncounterReadResult;
public sealed record BattleEncounterReadAccepted(BattleEncounterDefinition Definition) : BattleEncounterReadResult;
public sealed record BattleEncounterReadRejected(BattleEncounterDiagnostic Diagnostic) : BattleEncounterReadResult;
public sealed record BattleEncounterDiagnostic(string Field, string Message);

public sealed record EncounterPoint(byte X, byte Y);
public sealed record EncounterSource(string Repository, string Commit, string SourcePath, string SourceSha256);
public sealed record EncounterRange(int Start, int EndExclusive, int LengthBytes);
public sealed record EncounterIdentity(byte Id, string Code);
public sealed record EncounterArea(byte MapId, byte X, byte Y, byte Width, byte Height, byte TriggerX, byte TriggerY);
public sealed record EncounterScene(string CustomBackgroundExpression, bool EnemyLeaderPresent, bool HalfExperience);
public enum EncounterEntityKind { Ally, Enemy }
public sealed record EncounterBehavior(string PrimaryOrderExpression, byte PrimaryRegion,
    string SecondaryOrderExpression, byte SecondaryRegion, byte Filler, string SpawnExpression);
public sealed record EncounterPlacement(byte Id, EncounterEntityKind Kind, string IdentityExpression,
    EncounterPoint Position, string AiCommandsetExpression, string ItemExpression, EncounterBehavior Behavior);

public sealed class EncounterRegion
{
    internal EncounterRegion(byte id, byte unknown, IEnumerable<EncounterPoint> vertices, byte trailingByte0, byte trailingByte1)
    { Id = id; Unknown = unknown; Vertices = Array.AsReadOnly(vertices.ToArray()); TrailingByte0 = trailingByte0; TrailingByte1 = trailingByte1; }
    public byte Id { get; }
    public byte Unknown { get; }
    public IReadOnlyList<EncounterPoint> Vertices { get; }
    public byte TrailingByte0 { get; }
    public byte TrailingByte1 { get; }
}

// Source encounter data only. No enemy baseline, party preset, initialized state or capability claim.
public sealed class BattleEncounterDefinition
{
    internal BattleEncounterDefinition(EncounterSource placementSource, EncounterSource terrainSource,
        EncounterIdentity battle, EncounterRange placementRange, EncounterArea area, EncounterScene scene,
        IEnumerable<EncounterPlacement> placements, IEnumerable<EncounterRegion> regions, IEnumerable<EncounterPoint> aiPoints,
        EncounterRange compressedRange, string compressedSha256, string terrainSha256,
        IEnumerable<byte> terrain, IDictionary<byte, int> terrainValueCounts)
    {
        PlacementSource = placementSource; TerrainSource = terrainSource; Battle = battle; PlacementRange = placementRange;
        Area = area; Scene = scene; Placements = Array.AsReadOnly(placements.ToArray());
        Regions = Array.AsReadOnly(regions.ToArray()); AiPoints = Array.AsReadOnly(aiPoints.ToArray());
        CompressedRange = compressedRange; CompressedSha256 = compressedSha256; TerrainSha256 = terrainSha256;
        Terrain = Array.AsReadOnly(terrain.ToArray());
        TerrainValueCounts = new ReadOnlyDictionary<byte, int>(new Dictionary<byte, int>(terrainValueCounts));
    }
    public int SchemaVersion => 1;
    public EncounterSource PlacementSource { get; }
    public EncounterSource TerrainSource { get; }
    public EncounterIdentity Battle { get; }
    public EncounterRange PlacementRange { get; }
    public EncounterArea Area { get; }
    public EncounterScene Scene { get; }
    public IReadOnlyList<EncounterPlacement> Placements { get; }
    public int AllyCount => Placements.Count(p => p.Kind == EncounterEntityKind.Ally);
    public int EnemyCount => Placements.Count - AllyCount;
    public IReadOnlyList<EncounterRegion> Regions { get; }
    public IReadOnlyList<EncounterPoint> AiPoints { get; }
    public EncounterRange CompressedRange { get; }
    public string CompressedSha256 { get; }
    public int TerrainWidth => 48;
    public int TerrainHeight => 48;
    public string TerrainSha256 { get; }
    public IReadOnlyList<byte> Terrain { get; }
    public IReadOnlyDictionary<byte, int> TerrainValueCounts { get; }
}
