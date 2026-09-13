using System.Security.Cryptography;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Content;

public sealed record OriginalBattle01StartupDiagnostic(string Field, string Message);
public abstract record OriginalBattle01StartupImportResult;
public sealed record OriginalBattle01StartupImported(OriginalBattle01StartupDefinition Definition)
    : OriginalBattle01StartupImportResult;
public sealed record OriginalBattle01StartupImportRejected(OriginalBattle01StartupDiagnostic Diagnostic)
    : OriginalBattle01StartupImportResult;
public interface IOriginalBattle01StartupSource
{
    OriginalBattle01StartupImportResult Admit();
}

public enum OriginalBattle01EntityKind { Ally, Enemy }
public enum OriginalBattle01AiCommandSet : byte { Healer1 = 0, Attacker1 = 6, Attacker2 = 7 }
public enum OriginalBattle01Spawn : byte { Starting = 0 }

public sealed record OriginalBattle01Placement(byte Ordinal, OriginalBattle01EntityKind Kind,
    byte Identity, MapPosition Position, OriginalBattle01AiCommandSet AiCommandSet, ushort ItemWord,
    byte PrimaryOrder, byte PrimaryRegion, byte SecondaryOrder, byte SecondaryRegion, byte Filler,
    OriginalBattle01Spawn Spawn)
{
    public int CombatantIndex => Kind == OriginalBattle01EntityKind.Ally ? Identity : 128 + Ordinal - 3;
}

public sealed class OriginalBattle01AiRegion
{
    public OriginalBattle01AiRegion(byte id, byte unknown, IEnumerable<MapPosition> vertices,
        byte trailingByte0, byte trailingByte1)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        var copy = vertices.ToArray();
        if (copy.Length != 4 || copy.Any(point => point is null))
            throw new ArgumentException("The selected regions require four ordered vertices.", nameof(vertices));
        Id = id; Unknown = unknown; Vertices = Array.AsReadOnly(copy);
        TrailingByte0 = trailingByte0; TrailingByte1 = trailingByte1;
    }
    public byte Id { get; }
    public byte Unknown { get; }
    public IReadOnlyList<MapPosition> Vertices { get; }
    public byte TrailingByte0 { get; }
    public byte TrailingByte1 { get; }
}

public sealed record OriginalBattle01StartupProvenance(string Repository, string Commit,
    string PlacementExportDigest, string SceneExportDigest);

// Source-spawn baseline from pinned enemydefs.asm GIZMO, owned by enemy-promotions.md.
// Difficulty adjustment, activation and derived combatant initialization have not run.
public sealed class OriginalBattle01GizmoBaseline
{
    internal OriginalBattle01GizmoBaseline() { }
    public byte EnemyDefinitionId => 39;
    public byte SourceUnknownByte => 39;
    public byte SpellPowerMode => 0;
    public byte Level => 0;
    public ushort HpMax => 5;
    public byte MpMax => 0;
    public byte BaseAttack => 7;
    public byte BaseDefense => 5;
    public byte BaseAgility => 5;
    public byte BaseMove => 5;
    public ushort BaseResistance => 0x40E3;
    public byte BaseProwess => 0;
    public ushort InitialStatus => 0;
    public byte MovementType => 6;
    public ushort BaseAiBitfield => 0x2000;
    public IReadOnlyList<ushort> Items { get; } = Array.AsReadOnly<ushort>([127, 127, 127, 127]);
    public IReadOnlyList<byte> Spells { get; } = Array.AsReadOnly<byte>([63, 63, 63, 63]);
}

public sealed class OriginalBattle01StartupDefinition
{
    public const string Capability = "private-local-battle01-startup-inputs-v1";
    public const string Repository = "https://github.com/ShiningForceCentral/SF2DISASM.git";
    public const string UpstreamCommit = "c834c652b6862bc5679fd7f69a38a7093206efc6";
    public const string PlacementExportDigest = "32EEAE9AFB01DC38A1BAA99EE2E0B4C0B48F8A676F69771A5F5C52A2FC7E4C60";
    public const string SceneExportDigest = "DB9CCC8A40EBC1E0DB23C3EE5BDD67CAF026EF3EBD280B819FAB40E739BAD567";
    public const string AcceptedPlacementDigest = "2CDF8BC074C11451308121825DAE04DB530422FC945322934A23E6AB20EF10CE";
    public const string AcceptedTerrainDigest = "ECA7CDDAC612489FFD835A44D32D1C82E955A686F2D1C6DBFA5ED2959453C835";
    public const int TerrainStride = 48;
    public const int TerrainLength = 48 * 48;

    public OriginalBattle01StartupDefinition(OriginalBattle01StartupProvenance provenance,
        IEnumerable<OriginalBattle01Placement> entities, IEnumerable<OriginalBattle01AiRegion> regions,
        IEnumerable<byte> terrain)
        : this(provenance, entities, regions, terrain, null, null) { }

    // Same internal projection seam as the accepted map definitions: public tests use authored payloads.
    internal OriginalBattle01StartupDefinition(OriginalBattle01StartupProvenance provenance,
        IEnumerable<OriginalBattle01Placement> entities, IEnumerable<OriginalBattle01AiRegion> regions,
        IEnumerable<byte> terrain, string? placementDigestOverride, string? terrainDigestOverride)
    {
        Provenance = provenance ?? throw new ArgumentNullException(nameof(provenance));
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(regions);
        ArgumentNullException.ThrowIfNull(terrain);
        var entityCopy = entities.ToArray();
        var regionCopy = regions.ToArray();
        var terrainCopy = terrain.ToArray();
        if (entityCopy.Length != 9 || entityCopy.Any(entity => entity is null) ||
            !entityCopy.Select(entity => entity.Ordinal).SequenceEqual(Enumerable.Range(0, 9).Select(index => (byte)index)))
            throw new ArgumentException("Nine uniquely ordered placement records are required.", nameof(entities));
        for (int index = 0; index < entityCopy.Length; index++)
        {
            var entity = entityCopy[index];
            bool ally = index < 3;
            if (entity.Kind != (ally ? OriginalBattle01EntityKind.Ally : OriginalBattle01EntityKind.Enemy) ||
                entity.Identity != (ally ? index : 39) || entity.Position is null || !WithinBattleArea(entity.Position))
                throw new ArgumentException("Placement identity or battle-area bounds drifted.", nameof(entities));
            if (!Enum.IsDefined(entity.AiCommandSet) || entity.PrimaryOrder != 255 || entity.SecondaryOrder != 255 ||
                entity.PrimaryRegion > 2 || (entity.SecondaryRegion > 2 && entity.SecondaryRegion != 15) ||
                entity.Spawn != OriginalBattle01Spawn.Starting)
                throw new ArgumentException("Placement AI references, orders or STARTING setting are invalid.", nameof(entities));
        }
        if (entityCopy.Select(entity => entity.Position).Distinct().Count() != 9)
            throw new ArgumentException("Starting placements must not overlap.", nameof(entities));
        if (regionCopy.Length != 3 || regionCopy.Any(region => region is null) ||
            !regionCopy.Select(region => region.Id).SequenceEqual(new byte[] { 0, 1, 2 }) ||
            regionCopy.Any(region => region.Vertices.Any(point => !WithinBattleArea(point)) || region.Vertices.Distinct().Count() != 4))
            throw new ArgumentException("Three uniquely ordered, bounded quadrilateral regions are required.", nameof(regions));
        if (terrainCopy.Length != TerrainLength || terrainCopy.Any(value => value > 8 && value != 255))
            throw new ArgumentException("Terrain requires 2304 raw bytes of type 0-8 or 255, without occupancy bits.", nameof(terrain));
        Entities = Array.AsReadOnly(entityCopy); Regions = Array.AsReadOnly(regionCopy);
        Terrain = Array.AsReadOnly(terrainCopy);
        PlacementDigest = placementDigestOverride ?? ComputePlacementDigest();
        TerrainDigest = terrainDigestOverride ?? Convert.ToHexString(SHA256.HashData(terrainCopy));
    }

    public OriginalBattle01StartupProvenance Provenance { get; }
    public int BattleIndex => 1;
    public MapId Map { get; } = new("map57");
    public int AreaX => 0;
    public int AreaY => 0;
    public int AreaWidth => 16;
    public int AreaHeight => 20;
    public byte TriggerX => 255;
    public byte TriggerY => 255;
    public byte CustomBackground => 9;
    public bool EnemyLeaderPresent => false;
    public bool HalfExperience => true;
    public IReadOnlyList<OriginalBattle01Placement> Entities { get; }
    public IReadOnlyList<OriginalBattle01AiRegion> Regions { get; }
    public IReadOnlyList<MapPosition> AiPoints { get; } = Array.Empty<MapPosition>();
    public IReadOnlyList<byte> Terrain { get; }
    public string PlacementDigest { get; }
    public string TerrainDigest { get; }
    public OriginalBattle01GizmoBaseline EnemyBaseline { get; } = new();
    public byte TerrainAt(int x, int y)
    {
        if (x < 0 || x >= TerrainStride || y < 0 || y >= TerrainStride)
            throw new ArgumentOutOfRangeException(nameof(x), "Terrain coordinates must address the 48-by-48 source array.");
        return Terrain[y * TerrainStride + x];
    }

    public OriginalBattle01StartupDiagnostic? GetAdmissionDiagnostic()
    {
        if (Provenance != new OriginalBattle01StartupProvenance(Repository, UpstreamCommit, PlacementExportDigest, SceneExportDigest))
            return new("provenance", "The selected startup input provenance drifted.");
        if (PlacementDigest != AcceptedPlacementDigest)
            return new("entities", "The complete selected deployment and region projection drifted.");
        if (TerrainDigest != AcceptedTerrainDigest)
            return new("terrain", "The complete 48-by-48 terrain projection drifted.");
        return null;
    }

    private static bool WithinBattleArea(MapPosition position) =>
        position.X >= 0 && position.X < 16 && position.Y >= 0 && position.Y < 20;

    private string ComputePlacementDigest()
    {
        // Accepted 148-byte source layout: counts, nine 12-byte records, then ordered regions.
        List<byte> bytes = [3, 6, 3, 0];
        foreach (var entity in Entities)
            bytes.AddRange([entity.Identity, (byte)entity.Position.X, (byte)entity.Position.Y, (byte)entity.AiCommandSet,
                (byte)(entity.ItemWord >> 8), (byte)(entity.ItemWord & 255), entity.PrimaryOrder, entity.PrimaryRegion,
                entity.SecondaryOrder, entity.SecondaryRegion, entity.Filler, (byte)entity.Spawn]);
        foreach (var region in Regions)
        {
            bytes.AddRange([4, region.Unknown]);
            foreach (var point in region.Vertices) bytes.AddRange([(byte)point.X, (byte)point.Y]);
            bytes.AddRange([region.TrailingByte0, region.TrailingByte1]);
        }
        return Convert.ToHexString(SHA256.HashData(bytes.ToArray()));
    }
}
