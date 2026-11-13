using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;

namespace Sf2.Remake.Content.Scenarios;

// Selected private identities are separate from the shared encounter format and gameplay rules.
public sealed class PrivateBattleEncounterReader
{
    public const string Repository = "https://github.com/ShiningForceCentral/SF2DISASM.git";
    public const string UpstreamCommit = "c834c652b6862bc5679fd7f69a38a7093206efc6";
    public const string PlacementExportDigest = "32EEAE9AFB01DC38A1BAA99EE2E0B4C0B48F8A676F69771A5F5C52A2FC7E4C60";
    public const string SceneExportDigest = "DB9CCC8A40EBC1E0DB23C3EE5BDD67CAF026EF3EBD280B819FAB40E739BAD567";
    public const string CompressedTerrainDigest = "A0E6B0D4F656C7BD893923330148B3F9366CA7D839F5A0676272A2C95DAABC4A";
    private const string PlacementSourceDigest = "DBE89C24807B4577F741FC0EB6EDAE3386F1C47F8712CA2586AE945EDFEDD94B";
    private const string Placement02Digest = "4769B0E79B6516F6AE0EA1F4BE4B3C6D7FC06A18E1AE491090676F3A2C9AA524";
    private const string Scene02Digest = "76BEAFD66090CC8719BC0125EE82E356401FE070D2E161736DD7EFA2D24C39E7";
    private const string Terrain02Digest = "9577ADEF7B0222733F2D6F26C879CDE4076BBDE0906193A6E3F48FD9F1045789";
    private const string Source02Digest = "0FCEEB266BA2B6D1A1529D5B3A2539EEA65C3A1D33E01B09C036A0967FCD612C";
    private readonly string _placementPath, _scenePath, _terrainPath;

    public PrivateBattleEncounterReader(string placementPath, string scenePath, string terrainPath)
    {
        foreach (string path in new[] { placementPath, scenePath, terrainPath })
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
                throw new ArgumentException("Explicit fully qualified paths are required for all three private encounter inputs.");
        _placementPath = placementPath; _scenePath = scenePath; _terrainPath = terrainPath;
    }

    public BattleEncounterReadResult Read()
    {
        try
        {
            byte[] placement = ReadInput(_placementPath, Pins.Values.Select(pin => pin.Placement), "placement", null, 65536);
            using var selection = JsonDocument.Parse(placement);
            int id = selection.RootElement.GetProperty("battle").GetProperty("id").GetInt32();
            var pin = Pin(id);
            byte[] scene = ReadInput(_scenePath, pin.Scene, "scene");
            byte[] stored = ReadInput(_terrainPath, pin.Terrain, "terrain", pin.TerrainRange.LengthBytes);
            byte[] terrain;
            try { terrain = StackCompressedGraphicsDecoder.Decode(stored, 2304).Output; }
            catch (InvalidDataException) { throw Invalid("terrain", "The selected terrain failed bounded Stack decoding."); }
            var result = Decode(placement, scene, terrain);
            if (result is BattleEncounterReadAccepted accepted) RequireSelectedSource(accepted.Definition);
            return result;
        }
        catch (EncounterReadException error) { return new BattleEncounterReadRejected(error.Diagnostic); }
    }

    // Same parser used after real private file/decoder admission. It does not assert private file trust.
    internal static BattleEncounterReadResult Decode(byte[] placement, byte[] scene, byte[] terrain)
    {
        try { return new BattleEncounterReadAccepted(Parse(placement, scene, terrain)); }
        catch (EncounterReadException error) { return new BattleEncounterReadRejected(error.Diagnostic); }
        catch (JsonException) { return new BattleEncounterReadRejected(new("document", "The encounter JSON is invalid.")); }
    }

    private static BattleEncounterDefinition Parse(byte[] placementBytes, byte[] sceneBytes, byte[] terrain)
    {
        Require(placementBytes.Length is > 0 and <= 65536, "placement", "The document length is invalid.");
        Require(sceneBytes.Length is > 0 and <= 65536, "scene", "The document length is invalid.");
        Require(terrain.Length == 2304 && terrain.All(value => value <= 8 || value == 255), "terrain", "Retain the complete raw 48-by-48 terrain.");
        using var placementDocument = JsonDocument.Parse(placementBytes, new JsonDocumentOptions { MaxDepth = 32 });
        using var sceneDocument = JsonDocument.Parse(sceneBytes, new JsonDocumentOptions { MaxDepth = 32 });
        var placement = placementDocument.RootElement; var scene = sceneDocument.RootElement;
        Properties(placement, "placement", "schemaVersion", "provenance", "battle", "romRange", "counts", "entities", "aiRegions", "aiPoints");
        Properties(scene, "scene", "schemaVersion", "provenance", "battle", "map", "scene", "terrain");
        Number(placement, "schemaVersion", 1, 1, "placement.schemaVersion");
        Number(scene, "schemaVersion", 1, 1, "scene.schemaVersion");
        var placementSource = Source(placement.GetProperty("provenance"), false);
        var terrainSource = Source(scene.GetProperty("provenance"), true);
        Require(placementSource.Repository == terrainSource.Repository, "provenance.repository", "Source repositories must agree.");
        Require(placementSource.Commit == terrainSource.Commit, "provenance.commit", "Source commits must agree.");
        var battle = Battle(placement.GetProperty("battle"));
        Require(battle == Battle(scene.GetProperty("battle")), "battle", "Placement and scene must select the same battle.");
        var placementRange = Range(placement.GetProperty("romRange"), "romRange");
        var map = scene.GetProperty("map");
        Properties(map, "map", "id", "x", "y", "width", "height", "triggerX", "triggerY");
        var area = new EncounterArea(Byte(map, "id"), Byte(map, "x"), Byte(map, "y"),
            (byte)Number(map, "width", 1, 48, "map.width"), (byte)Number(map, "height", 1, 48, "map.height"), Byte(map, "triggerX"), Byte(map, "triggerY"));
        bool Within(EncounterPoint point) => point.X < area.Width && point.Y < area.Height;
        var selection = scene.GetProperty("scene");
        Properties(selection, "scene.selection", "customBackgroundExpression", "enemyLeaderPresent", "halfExperience");
        var sceneData = new EncounterScene(Text(selection, "customBackgroundExpression"), Boolean(selection, "enemyLeaderPresent"), Boolean(selection, "halfExperience"));
        var counts = placement.GetProperty("counts");
        Properties(counts, "counts", "allies", "enemies", "aiRegions", "aiPoints");
        int allies = Number(counts, "allies", 0, 30, "counts.allies"), enemies = Number(counts, "enemies", 0, 32, "counts.enemies");
        int regionCount = Number(counts, "aiRegions", 0, 16, "counts.aiRegions"), pointCount = Number(counts, "aiPoints", 0, 255, "counts.aiPoints");
        var entities = Array(placement, "entities", allies + enemies).EnumerateArray().Select(entity =>
        {
            Properties(entity, "entities", "id", "kind", "identityExpression", "x", "y", "aiCommandsetExpression", "itemExpression", "behavior");
            var kind = Text(entity, "kind") switch { "ally" => EncounterEntityKind.Ally, "enemy" => EncounterEntityKind.Enemy,
                _ => throw Invalid("entities.kind", "Unknown entity kind.") };
            var behavior = entity.GetProperty("behavior");
            Properties(behavior, "entities.behavior", "primaryOrderExpression", "primaryRegion", "secondaryOrderExpression", "secondaryRegion", "filler", "spawnExpression");
            return new EncounterPlacement(Byte(entity, "id"), kind, Text(entity, "identityExpression"), Position(entity),
                Text(entity, "aiCommandsetExpression"), Text(entity, "itemExpression"),
                new(Text(behavior, "primaryOrderExpression"), Byte(behavior, "primaryRegion"), Text(behavior, "secondaryOrderExpression"),
                    Byte(behavior, "secondaryRegion"), Byte(behavior, "filler"), Text(behavior, "spawnExpression")));
        }).ToArray();
        Require(entities.Count(entity => entity.Kind == EncounterEntityKind.Ally) == allies, "counts.allies", "Declared side counts must match the entities.");
        Require(entities.Count(entity => entity.Kind == EncounterEntityKind.Enemy) == enemies, "counts.enemies", "Declared side counts must match the entities.");
        Require(entities.Select(entity => entity.Id).Distinct().Count() == entities.Length, "entities", "Entity IDs must be unique.");
        // Only this literal establishes deployed geometry. Other spawn expressions and their raw
        // coordinates survive for a future resolver; importing them does not execute a spawn rule.
        var starting = entities.Where(entity => entity.Behavior.SpawnExpression == "STARTING").ToArray();
        Require(starting.All(entity => Within(entity.Position)), "entities", "STARTING placements must lie within the battle area.");
        Require(starting.Select(entity => entity.Position).Distinct().Count() == starting.Length, "entities", "STARTING placements must not overlap.");
        var regions = Array(placement, "aiRegions", regionCount).EnumerateArray().Select(region =>
        {
            Properties(region, "aiRegions", "id", "vertexCount", "unknown", "vertices", "trailingBytes");
            int count = Number(region, "vertexCount", 3, 4, "aiRegions.vertexCount");
            var vertices = Array(region, "vertices", 4).EnumerateArray().Take(count).Select(point =>
            { Properties(point, "aiRegions.vertices", "x", "y"); return Position(point); }).ToArray();
            Require(vertices.All(Within) && vertices.Distinct().Count() == count &&
                Cross(vertices[0], vertices[1], vertices[^1]) != 0 &&
                (count == 3 || Cross(vertices[2], vertices[1], vertices[3]) != 0),
                "regions", "Region vertices must be distinct, bounded and form nondegenerate triangles.");
            var trailing = Array(region, "trailingBytes", 2).EnumerateArray().Select(value => ByteValue(value, "aiRegions.trailingBytes")).ToArray();
            return new EncounterRegion((byte)Number(region, "id", 0, 15, "aiRegions.id"), Byte(region, "unknown"), vertices, trailing[0], trailing[1]);
        }).ToArray();
        Require(regions.Select(region => region.Id).Distinct().Count() == regions.Length, "regions", "Region IDs must be unique.");
        // Ally records carry unused zeroed AI operands even in spritesets with no regions.
        // Region assignment belongs to the enemy AI initialization path.
        Require(entities.Where(entity => entity.Kind == EncounterEntityKind.Enemy)
            .All(entity => ValidRegion(entity.Behavior.PrimaryRegion) && ValidRegion(entity.Behavior.SecondaryRegion)),
            "entities", "An assigned region must exist or be the absent-region value15.");
        bool ValidRegion(byte id) => id == 15 || regions.Any(region => region.Id == id);
        var points = Array(placement, "aiPoints", pointCount).EnumerateArray().Select(point =>
        { Properties(point, "aiPoints", "x", "y"); return Position(point); }).ToArray();
        Require(points.All(Within), "aiPoints", "AI points must lie within the battle area.");
        Require(placementRange.LengthBytes == 4 + entities.Length * 12 + regions.Length * 12 + points.Length * 2,
            "romRange", "The source range must match the declared record lengths.");
        var metadata = scene.GetProperty("terrain");
        Properties(metadata, "terrain", "compressedRange", "compressedSha256", "decompressedWidth", "decompressedHeight", "decompressedLengthBytes", "decompressedSha256", "valueCounts");
        var compressedRange = Range(metadata.GetProperty("compressedRange"), "terrain.compressedRange");
        string compressedHash = Digest(metadata, "compressedSha256"), terrainHash = Digest(metadata, "decompressedSha256");
        Require(compressedHash.Equals(terrainSource.SourceSha256, StringComparison.OrdinalIgnoreCase), "terrain.compressedSha256", "Compressed identity must join its source.");
        Number(metadata, "decompressedWidth", 48, 48, "terrain.decompressedWidth");
        Number(metadata, "decompressedHeight", 48, 48, "terrain.decompressedHeight");
        Number(metadata, "decompressedLengthBytes", 2304, 2304, "terrain.decompressedLengthBytes");
        Require(terrainHash.Equals(Convert.ToHexString(SHA256.HashData(terrain)), StringComparison.OrdinalIgnoreCase), "terrain.decompressedSha256", "Decoded terrain identity drifted.");
        var valueCounts = terrain.GroupBy(value => value).ToDictionary(group => group.Key, group => group.Count());
        var declaredCounts = metadata.GetProperty("valueCounts");
        Properties(declaredCounts, "terrain.valueCounts", valueCounts.Keys.Select(value => value.ToString(CultureInfo.InvariantCulture)).ToArray());
        foreach (var (value, count) in valueCounts) Number(declaredCounts, value.ToString(CultureInfo.InvariantCulture), count, count, "terrain.valueCounts");
        return new(placementSource, terrainSource, battle, placementRange, area, sceneData, entities, regions, points,
            compressedRange, compressedHash, terrainHash, terrain, valueCounts);
    }

    internal static void RequireSelectedSource(BattleEncounterDefinition definition)
    {
        foreach (var source in new[] { definition.PlacementSource, definition.TerrainSource })
        {
            Require(source.Repository == Repository, "provenance.repository", "The selected repository drifted.");
            Require(source.Commit == UpstreamCommit, "provenance.commit", "The selected commit drifted.");
        }
        int id = definition.Battle.Id;
        var pin = Pin(id);
        Require(definition.Battle == new EncounterIdentity((byte)id, pin.Code), "battle", "The selected battle identity drifted.");
        string suffix = id.ToString("D2", CultureInfo.InvariantCulture);
        Require(definition.PlacementSource.SourcePath == $"data/battles/spritesets/spriteset{suffix}.asm", "provenance.sourcePath", "The selected placement source drifted.");
        Require(definition.PlacementSource.SourceSha256 == pin.Source, "provenance.sourceSha256", "The selected placement identity drifted.");
        Require(definition.TerrainSource.SourcePath == $"data/battles/entries/battle{pin.TerrainId:D2}/terrain.bin", "provenance.terrainSourcePath", "The selected terrain source drifted.");
        Require(definition.TerrainSource.SourceSha256 == pin.Terrain, "provenance.terrainSourceSha256", "The selected terrain identity drifted.");
        Require(definition.PlacementRange == pin.PlacementRange, "romRange", "The selected placement range drifted.");
        Require(definition.CompressedRange == pin.TerrainRange, "terrain.compressedRange", "The selected terrain range drifted.");
    }

    private sealed record EncounterPin(string Code, string Placement, string Scene, string Terrain, string Source,
        int TerrainId, EncounterRange PlacementRange, EncounterRange TerrainRange);
    private static readonly Dictionary<int, EncounterPin> Pins = new()
    {
        [1] = new("INSIDE_ANCIENT_TOWER", PlacementExportDigest, SceneExportDigest, CompressedTerrainDigest, PlacementSourceDigest,
            1, new(1782498, 1782646, 148), new(1758020, 1758304, 284)),
        [2] = new("TO_YEEL", Placement02Digest, Scene02Digest, Terrain02Digest, Source02Digest,
            2, new(1782646, 1782772, 126), new(1758304, 1758636, 332)),
        [3] = new("TO_HAWEL_HOUSE", "67E6BE08EA40B2AEFDD78BAC46666AE9845AC16415365B039A9308593DBB35A2",
            "8E9F759345D0EC1596C9921E85020A6F3DCFD5E972B7D4D75B7427E7866457B3",
            "56B788E61D745755DCE3D331A179A33938BC5E1CBB116C505B0DF3DAA4F577AA",
            "0616BB9F25BB37A92D97C40C05A18DC0E71D109D7E9564428F8BB9E509C67402",
            3, new(1782772, 1783004, 232), new(1758636, 1758970, 334)),
        [4] = new("AMBUSHED_BY_GALAM_SOLDIERS", "774A4F283268C61B1731D2E544C5ABF1C9382487D3EFA9F37E92CF44FFDB44DA",
            "C0F6BE4E0EA9324355DD9207FBF2CE927A65064EAB0C58E904B3423A79686E06",
            "56B788E61D745755DCE3D331A179A33938BC5E1CBB116C505B0DF3DAA4F577AA",
            "79381A1FB344CB88BDB20A849AA13B4513F686C0E2E9C4CAAE5A6DD39D15CA46",
            3, new(1783004, 1783202, 198), new(1758636, 1758970, 334)),
        [5] = new("GALAM_CASTLE", "EB40C3C98971F7E1A615FBCF0F918F30FF3B3C1041D63F4274EACC97E953798A",
            "25E8E9A8010EE9861ECDC5FDA96EC53808FAA328131789846A6B012038D85331",
            "E79FE15A5C0CF652E250AE6ABF724B7B28CC4BE065777CC65C6658B8123E3D21",
            "0BED3ECCBC1F47AB44AD69F9530546B9F848BEBDE0DF35BA4F2765DBFBFB4992",
            5, new(1783202, 1783388, 186), new(1758970, 1759216, 246)),
        [6] = new("TO_GRANSEAL", "C4C2CB4FA1B2F570A148DD75F708E367F8BE0599E347F8356271C2723B2CD6A6",
            "78C9FA92257543DC796B6AE27A0BC913ADBB3A22E398B44C5C9AFC505769770E",
            "2B2EE4B36DC44B8BD3D3C59BBC12B32615A1C805DE3C109942C436290BF02C1D",
            "F758BB5DC6E1745E9ABAA386782BD21B7C4BDD4163AA5FAB1BB1E0EF8479E690",
            6, new(1783388, 1783584, 196), new(1759216, 1759612, 396)),
        [7] = new("VERSUS_DARK_SMOKES", "02A4222BA3D848A8890D69D131691E495FAA360D2C901A448C66C6C63ED6358E",
            "519A261C2E703EE43B63844030E5627E9E4018085FFEB0ABE89BB8EA1AFB0B93",
            "7A805B54EA8E9250CBB7BF6BB5260CA1D58B744CF13ABEB008DDC886EDD3A6FF",
            "D1F6A6B62CC7F6466D0E0FA059252E71DC99930CF8C146A37EC768E2671694F0",
            7, new(1783584, 1783842, 258), new(1759612, 1759940, 328)),
        [8] = new("NORTH_CLIFF", "F277127A26646C6F69660DEBDE6F3DF9168E0F1BCC4A6CB92E9ED7B616BACFB5",
            "0DD29102F6EDCDA858772104C4CF750B1C9A8DC2789E799AFAD64DF67686C043",
            "6A3C2A5DE3EAC5C13C084DE2E3D4FEDC564F7D79A758DD06117C052543F76C5C",
            "F05D2D8EF9BAFACED8876AC18615A7B62CC1E2E0B8FF86FA242329580E8E722F",
            8, new(1783842, 1784184, 342), new(1759940, 1760388, 448)),
        [9] = new("TO_RIBBLE", "B2AD1CD4BDAAE830F41C0110D188A3765F382FAACD1FFE42080FEE8991755E99",
            "35A9F844E60DD2C0438E6C97323F680E2AA081034284DA02F3B70D5BD45DB132",
            "70F8EFFF0311D01DABE00FE52DCFB79536F121DBB68654BE4D1B45600B018F15",
            "0DE52CA492E565230D1C6353EB140071A798CCB238ED86DEAAC11DB71373204B",
            9, new(1784184, 1784488, 304), new(1760388, 1760904, 516)),
        [10] = new("TO_THE_EAST", "3EBB4F6A96B1538E05A97363036D4BF2E1CFD44558C76C34BA9BBE2C06419943",
            "B4CB1158B7DDCB5575949D9CEC5E4DB89BEEEE4BF63EAE23B0C29BEECBF27199",
            "091D0B44499CFBFA6FF830304773A32F9AF311F12E675F3A27C9BADE16EFB583",
            "85718F0367A8661A26118D3587138B6DB138D81C9A850F5EB72CED0D32D8D650",
            10, new(1784488, 1784818, 330), new(1760904, 1761352, 448)),
        [11] = new("CAVE_OF_DARKNESS", "D81EDD8779DECAB42836118BC5D4302B06A21B5EDF657D48AB5FC3ECC95DB3E1",
            "9124FF832D3D023A644B7567B8B5448C170A50D2F02E223C1B9FB83F8FC352F9",
            "C5146F0B6C79E94BD2949EA6B2D29D132AA9954CFA21565A2A9A380B5C5DA529",
            "DA8DAF4989BDEC0F180200127F5EDC6003729DBA21BBD27382B2FC52CA0A129C",
            11, new(1784818, 1785136, 318), new(1761352, 1761668, 316)),
        [12] = new("MOUNT_VOLCANO", "7354E59D2F2EFC8FC40D6DC7DDF07F81BDBB05CE7B3AA3B944ACB8BCB6063BA2",
            "C8C517730F9AFDB7247BBF6232479EBBD7C864566D8782E9AD60CB75C865431B",
            "D585535E6119A6AF2A790440B58A35891113FFA134C6C6BBC778316215592305",
            "371DF1548CFB07087655117CB18B5CDEBDA7E59C634168F60E738ADAAF77DFF9",
            12, new(1785136, 1785478, 342), new(1761668, 1762024, 356)),
        [13] = new("POLCA_VILLAGE", "7537FD60C01EC3CC1007C5875BA83BA82037D3D7A13BEAC15402921EA172BC36",
            "B22B20CAF4F7D0F06A2D4B9747121054DB75002D55E772B7906F1B221213A7D4",
            "C984F95674E5C249CD2A2153DE0B664D1FAE73E72662C999E2023840D5636497",
            "E1D6CE7A043B984A9028297F79EC0EB490E3E8DDAC2714AAE34155BD0C880259",
            13, new(1785478, 1785856, 378), new(1762024, 1762414, 390)),
        [14] = new("SOUTHEAST_DESERT", "FC2CD6C57217AFAF957E47419B4AF75C5B055519B2D909BEF05CD25EF4A60910",
            "656A2F5C629253FEF13FDA2AC530E7586087BC9A590E7293021EEB71E1311EE9",
            "92FDEE833BAFA5D47D68559F2AC40C72630FED0DA4EDF5162DD61372D83669E6",
            "88ABF624F64B34E3C49F9D6630566420FE1A58145B1C4095DEB41FCB39E50954",
            14, new(1785856, 1786220, 364), new(1762414, 1762838, 424)),
        [15] = new("SHRINE_SOUTH_OF_RIBBLE", "D2733336CFC46958A72247E6B1CD8FFD2486104BBB4ED7293D9A75DC5E408EB9",
            "4084FD768894B168F371C8389BB3D27D1432393BC6D1FEBC9BF4EF815B14BF14",
            "84DAE925E9B547471D6A7E36D64CE1872E716C167BA08D7AEEF0A30B084798D0",
            "AF9F0E99DFDC802A30DB0E501CC8E48C39AE11F1039237C0DA2930FF242978D7",
            15, new(1786220, 1786586, 366), new(1762838, 1763228, 390)),
        [16] = new("VERSUS_KRAKEN", "7CC1580137B60923B348F68BCF1169E6373E434C909BDF357E41C6F515CD0A00",
            "702E05751A0C8BD42866C1AC60569F2BD5CE45D8EFB719B0BE8B2970886FBD4D",
            "1416B853925A9526BA0B3640A0C300A64C04F1034099441913351F91FB55855F",
            "73199BC27B3A9C10D56D6551E15D2C5888AD316C93B78F37925E7620505DEA76",
            16, new(1786586, 1786866, 280), new(1763228, 1763464, 236)),
        [17] = new("TO_TAROS_SHRINE", "38E7B10E18829CA66EF7A9C9B76324F83F00A24B28B62E6C04A9A8F9EC9B4C5A",
            "7A086FBF98D33B33EFBD9A38291E3FF19C09CAF16440B17CCC03C240D660F93E",
            "97B6C6425F02D1EADD590622E0B3284046B1E2D61137C472A46EBA3577044206",
            "1D1BC1822293184E30B3F24E780204E62260B9F8FCD6988B21718C61106E22B4",
            17, new(1786866, 1787206, 340), new(1763464, 1763814, 350)),
    };
    internal static int RegisteredCount => Pins.Count;
    private static EncounterPin Pin(int id) => Pins.TryGetValue(id, out var pin) ? pin
        : throw Invalid("battle", "The selected encounter is not admitted.");

    internal static byte[] ReadInput(string path, string digest, string field, int? expectedLength = null, int maximumLength = 65536) =>
        ReadInput(path, [digest], field, expectedLength, maximumLength);

    private static byte[] ReadInput(string path, IEnumerable<string> digests, string field, int? expectedLength, int maximumLength)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Require(stream.Length > 0 && stream.Length <= maximumLength && (expectedLength is null || stream.Length == expectedLength), field, "The selected input length is invalid.");
            byte[] bytes = new byte[(int)stream.Length]; stream.ReadExactly(bytes);
            string actual = Convert.ToHexString(SHA256.HashData(bytes));
            Require(digests.Contains(actual, StringComparer.Ordinal), field, "The fixed selected input identity drifted.");
            return bytes;
        }
        catch (IOException) { throw Invalid(field, "The required private encounter input is unavailable."); }
        catch (UnauthorizedAccessException) { throw Invalid(field, "The required private encounter input cannot be read."); }
    }

    private static EncounterSource Source(JsonElement source, bool terrain)
    {
        string path = terrain ? "terrainSourcePath" : "sourcePath", hash = terrain ? "terrainSourceSha256" : "sourceSha256";
        Properties(source, "provenance", "repository", "commit", path, hash);
        string commit = Text(source, "commit");
        Require(commit.Length == 40 && commit.All(Uri.IsHexDigit), "provenance.commit", "A complete source revision is required.");
        return new(Text(source, "repository"), commit, Text(source, path), Digest(source, hash));
    }
    private static EncounterIdentity Battle(JsonElement battle)
    { Properties(battle, "battle", "id", "code"); return new(Byte(battle, "id"), Text(battle, "code")); }
    private static EncounterRange Range(JsonElement range, string field)
    {
        Properties(range, field, "start", "endExclusive", "lengthBytes");
        int start = Number(range, "start", 0, int.MaxValue, field), end = Number(range, "endExclusive", start, int.MaxValue, field);
        int length = Number(range, "lengthBytes", 1, int.MaxValue, field);
        Require(end - start == length, field, "Range endpoints and length must agree."); return new(start, end, length);
    }
    private static string Digest(JsonElement parent, string key)
    { string value = Text(parent, key); Require(value.Length == 64 && value.All(Uri.IsHexDigit), key, "A complete SHA256 identity is required."); return value; }
    private static void Properties(JsonElement value, string field, params string[] required)
    {
        Require(value.ValueKind == JsonValueKind.Object, field, "An object is required.");
        var names = value.EnumerateObject().Select(property => property.Name).ToArray();
        Require(names.Length == required.Length && names.Distinct(StringComparer.Ordinal).Count() == names.Length &&
            names.ToHashSet(StringComparer.Ordinal).SetEquals(required), field, "The closed schema properties are invalid.");
    }
    private static JsonElement Array(JsonElement parent, string key, int count)
    { var value = parent.GetProperty(key); Require(value.ValueKind == JsonValueKind.Array && value.GetArrayLength() == count, key, "The array must match its declared count."); return value; }
    private static string Text(JsonElement parent, string key)
    { var value = parent.GetProperty(key); Require(value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString()), key, "A nonempty string is required."); return value.GetString()!; }
    private static bool Boolean(JsonElement parent, string key)
    { var value = parent.GetProperty(key); Require(value.ValueKind is JsonValueKind.True or JsonValueKind.False, "scene.selection", "A boolean is required."); return value.GetBoolean(); }
    private static int Number(JsonElement parent, string key, int minimum, int maximum, string field)
    { var value = parent.GetProperty(key); if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int number) || number < minimum || number > maximum) throw Invalid(field, "The numeric value is invalid."); return number; }
    private static byte Byte(JsonElement parent, string key) => ByteValue(parent.GetProperty(key), key);
    private static byte ByteValue(JsonElement value, string field) => value.ValueKind == JsonValueKind.Number && value.TryGetByte(out byte result)
        ? result : throw Invalid(field, "An unsigned byte is required.");
    private static EncounterPoint Position(JsonElement point) => new(Byte(point, "x"), Byte(point, "y"));
    private static int Cross(EncounterPoint a, EncounterPoint b, EncounterPoint c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
    private static void Require(bool condition, string field, string message) { if (!condition) throw Invalid(field, message); }
    private static EncounterReadException Invalid(string field, string message) => new(new(field, message));
    internal sealed class EncounterReadException(BattleEncounterDiagnostic diagnostic) : Exception
    { internal BattleEncounterDiagnostic Diagnostic { get; } = diagnostic; }
}
