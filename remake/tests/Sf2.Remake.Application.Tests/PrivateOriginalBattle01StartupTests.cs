using System.Reflection;
using System.Text.Json;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01StartupTests
{
    [Fact]
    public void PreparationAndRepeatedPreparationRetainExactPendingAndEverySessionIdentity()
    {
        var session = PendingSession();
        var pending = session.PrivateOriginalBattle01Admission!;
        var snapshot = session.PrivateOriginalMapSnapshot;
        var animation = session.PrivateOriginalMapPlayerLocomotion;
        var bridge = session.PrivateOriginalMapBattleBridge;
        Assert.NotNull(bridge);
        var definition = Definition();
        var source = new Source(new OriginalBattle01StartupImported(definition));
        var party = OriginalBattle01ControlledPartyPreset.PlayerReadyComparison;
        for (int index = 0; index < 2; index++)
        {
            var prepared = Assert.IsType<PrivateOriginalBattle01StartupPrepared>(
                session.PreparePrivateOriginalBattle01Startup(pending, source, party));
            Assert.Same(pending, prepared.Pending);
            Assert.Same(definition, prepared.Inputs);
            Assert.Same(party, prepared.Party);
            Assert.Same(pending, session.PrivateOriginalBattle01Admission);
            Assert.Same(snapshot, session.PrivateOriginalMapSnapshot);
            Assert.Same(animation, session.PrivateOriginalMapPlayerLocomotion);
            Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
        }
        Assert.Equal(2, source.Calls);
        Assert.Equal(new MapId("map40"), snapshot.Map);
        Assert.Equal(new MapPosition(14, 13), snapshot.PlayerPosition);
        Assert.Throws<InvalidOperationException>(() => session.ApplyPrivateOriginalMap(new(ExplorationDirection.North)));
    }

    [Theory]
    [InlineData("missing-pending", "pending")]
    [InlineData("other-session", "pending")]
    [InlineData("no-current-pending", "pending")]
    [InlineData("missing-source", "source")]
    [InlineData("missing-party", "party")]
    [InlineData("party-id", "party.id")]
    [InlineData("rng", "party.randomSeed")]
    [InlineData("difficulty", "party.difficulty")]
    [InlineData("stats", "party.allies")]
    public void InvalidPreparationRequestsRejectBeforeReadingAndWithoutMutation(string drift, string field)
    {
        var session = PendingSession(createPending: drift != "no-current-pending");
        var pending = drift == "missing-pending" ? null : drift is "other-session" or "no-current-pending"
            ? PendingSession().PrivateOriginalBattle01Admission : session.PrivateOriginalBattle01Admission;
        var currentPending = session.PrivateOriginalBattle01Admission;
        var snapshot = session.PrivateOriginalMapSnapshot;
        var animation = session.PrivateOriginalMapPlayerLocomotion;
        var bridge = session.PrivateOriginalMapBattleBridge;
        var acceptedParty = OriginalBattle01ControlledPartyPreset.PlayerReadyComparison;
        var allies = acceptedParty.Allies.ToArray();
        if (drift == "stats")
        {
            var ally = allies[0];
            allies[0] = new(ally.Id, ally.ClassId, ally.Level, ally.HpMax, ally.HpCurrent, ally.MpMax, ally.MpCurrent,
                10, ally.EffectiveDefense, ally.EffectiveAgility, ally.EffectiveMove, ally.StatusEffects, ally.Items, ally.Spells);
        }
        var party = new OriginalBattle01ControlledPartyPreset(drift == "party-id" ? "unnamed" : acceptedParty.Id,
            drift == "rng" ? 1u : acceptedParty.RandomSeed, (byte)(drift == "difficulty" ? 1 : 0), allies);
        var source = new Source(new OriginalBattle01StartupImported(Definition()));
        var result = Assert.IsType<PrivateOriginalBattle01StartupRejected>(session.PreparePrivateOriginalBattle01Startup(
            pending, drift == "missing-source" ? null : source, drift == "missing-party" ? null : party));
        Assert.Equal(field, result.Diagnostic.Field);
        Assert.Equal(0, source.Calls);
        Assert.Same(snapshot, session.PrivateOriginalMapSnapshot);
        Assert.Same(animation, session.PrivateOriginalMapPlayerLocomotion);
        Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
        Assert.Same(currentPending, session.PrivateOriginalBattle01Admission);
    }

    [Theory]
    [InlineData("provenance", "provenance")]
    [InlineData("deployment", "entities")]
    [InlineData("terrain", "terrain")]
    [InlineData("null", "source")]
    [InlineData("rejected", "private-input")]
    public void CustomSourcePortsCannotBypassTheirOwnInputContract(string drift, string field)
    {
        var session = PendingSession();
        var before = session.PrivateOriginalMapSnapshot;
        var animation = session.PrivateOriginalMapPlayerLocomotion;
        var bridge = session.PrivateOriginalMapBattleBridge;
        var pending = session.PrivateOriginalBattle01Admission;
        var definition = Definition(provenance: drift == "provenance" ? Provenance() with { Commit = new string('0', 40) } : null,
            computeDeployment: drift == "deployment", computeTerrain: drift == "terrain");
        OriginalBattle01StartupImportResult imported = drift == "null" ? new OriginalBattle01StartupImported(null!) :
            drift == "rejected" ? new OriginalBattle01StartupImportRejected(new("private-input", "Unavailable")) : new OriginalBattle01StartupImported(definition);
        var source = new Source(imported);
        var rejected = Assert.IsType<PrivateOriginalBattle01StartupRejected>(session.PreparePrivateOriginalBattle01Startup(
            pending, source, OriginalBattle01ControlledPartyPreset.PlayerReadyComparison));
        Assert.Equal(field, rejected.Diagnostic.Field);
        Assert.Equal(1, source.Calls);
        Assert.Same(before, session.PrivateOriginalMapSnapshot);
        Assert.Same(animation, session.PrivateOriginalMapPlayerLocomotion);
        Assert.Same(bridge, session.PrivateOriginalMapBattleBridge);
        Assert.Same(pending, session.PrivateOriginalBattle01Admission);
    }

    [Theory]
    [InlineData("count", "entities")]
    [InlineData("duplicate", "entities")]
    [InlineData("overlap", "entities")]
    [InlineData("bounds", "entities")]
    [InlineData("region-reference", "entities")]
    [InlineData("spawn", "entities")]
    [InlineData("region-id", "regions")]
    [InlineData("region-bounds", "regions")]
    [InlineData("terrain-stride", "terrain")]
    [InlineData("occupancy", "terrain")]
    public void TypedInputsRejectInvalidStructureBeforeAnyDigestCheck(string drift, string parameter)
    {
        var entities = Entities(); var regions = Regions(); byte[] terrain = new byte[2304];
        switch (drift)
        {
            case "count": entities = entities[..8]; break;
            case "duplicate": entities[8] = entities[8] with { Ordinal = 7 }; break;
            case "overlap": entities[8] = entities[8] with { Position = entities[7].Position }; break;
            case "bounds": entities[8] = entities[8] with { Position = new(16, 0) }; break;
            case "region-reference": entities[8] = entities[8] with { PrimaryRegion = 3 }; break;
            case "spawn": entities[8] = entities[8] with { Spawn = (OriginalBattle01Spawn)1 }; break;
            case "region-id": regions[2] = new(1, 0, regions[2].Vertices, 0, 0); break;
            case "region-bounds": regions[2] = new(2, 0, [new(0, 0), new(1, 0), new(1, 20), new(0, 1)], 0, 0); break;
            case "terrain-stride": terrain = new byte[16 * 20]; break;
            case "occupancy": terrain[0] = 0x81; break;
        }
        var error = Assert.Throws<ArgumentException>(() => new OriginalBattle01StartupDefinition(Provenance(), entities, regions, terrain));
        Assert.Equal(parameter, error.ParamName);
    }

    [Fact]
    public void PreparedInputsOwnTheirCollectionsAndKeepTerrainStrideSeparateFromTheBattleArea()
    {
        var entities = Entities(); var regions = Regions(); byte[] terrain = new byte[2304];
        terrain[49] = 2; terrain[17] = 3;
        var definition = new OriginalBattle01StartupDefinition(Provenance(), entities, regions, terrain);
        entities[0] = entities[0] with { Filler = 200 }; terrain[49] = 0;
        Assert.Equal(0, definition.Entities[0].Filler);
        Assert.Equal(2, definition.TerrainAt(1, 1));
        Assert.Equal(3, definition.TerrainAt(17, 0));
        Assert.Equal(2304, definition.Terrain.Count);
        Assert.Equal((16, 20), (definition.AreaWidth, definition.AreaHeight));
        Assert.Throws<ArgumentOutOfRangeException>(() => definition.TerrainAt(48, 0));
        Assert.Throws<NotSupportedException>(() => ((IList<byte>)definition.Terrain)[0] = 255);
        Assert.Throws<NotSupportedException>(() => ((IList<OriginalBattle01Placement>)definition.Entities).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<MapPosition>)definition.Regions[0].Vertices).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<ushort>)definition.EnemyBaseline.Items)[0] = 0);
        var itemWords = new ushort[] { 199, 0, 127, 127 }; var spells = new byte[] { 10, 63, 63, 63 };
        var ally = new OriginalBattle01ControlledAlly(0, 0, 1, 12, 12, 8, 8, 9, 4, 4, 6, 0, itemWords, spells);
        itemWords[0] = 0; spells[0] = 63;
        Assert.Equal(199, ally.Items[0]); Assert.Equal(10, ally.Spells[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<ushort>)ally.Items).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<byte>)ally.Spells).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<OriginalBattle01ControlledAlly>)OriginalBattle01ControlledPartyPreset.PlayerReadyComparison.Allies).Clear());
    }

    [Fact]
    public void ControlledPartyAndSeedBindTheAcceptedObservationWithoutActivationOrTurnScores()
    {
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../tests/fixtures/h3/map3-battle01-player-ready-v1.json"));
        using var fixture = JsonDocument.Parse(File.ReadAllText(path));
        var record = fixture.RootElement.GetProperty("expectedObservation").GetProperty("records")[0];
        var preset = OriginalBattle01ControlledPartyPreset.PlayerReadyComparison;
        Assert.Null(preset.GetAdmissionDiagnostic());
        Assert.Equal(record.GetProperty("deterministicState").GetProperty("seeded").GetProperty("randomSeed").GetUInt32(), preset.RandomSeed);
        Assert.Equal(fixture.RootElement.GetProperty("cases")[0].GetProperty("injectedDifficultyMenuReturn").GetByte(), preset.Difficulty);
        var observed = record.GetProperty("scenario").GetProperty("combatants");
        foreach (var ally in preset.Allies)
        {
            var row = observed.EnumerateArray().Single(value => value.GetProperty("id").GetByte() == ally.Id);
            Assert.Equal(row.GetProperty("class").GetByte(), ally.ClassId);
            Assert.Equal(row.GetProperty("level").GetByte(), ally.Level);
            Assert.Equal(row.GetProperty("hpMax").GetUInt16(), ally.HpMax); Assert.Equal(row.GetProperty("hpCurrent").GetUInt16(), ally.HpCurrent);
            Assert.Equal(row.GetProperty("mpMax").GetByte(), ally.MpMax); Assert.Equal(row.GetProperty("mpCurrent").GetByte(), ally.MpCurrent);
            Assert.Equal(row.GetProperty("attack").GetByte(), ally.EffectiveAttack); Assert.Equal(row.GetProperty("defense").GetByte(), ally.EffectiveDefense);
            Assert.Equal(row.GetProperty("agility").GetByte(), ally.EffectiveAgility); Assert.Equal(row.GetProperty("move").GetByte(), ally.EffectiveMove);
            Assert.Equal(row.GetProperty("statusEffects").GetUInt16(), ally.StatusEffects);
            Assert.Equal(row.GetProperty("items").EnumerateArray().Select(value => value.GetUInt16()), ally.Items);
            Assert.Equal(row.GetProperty("spells").EnumerateArray().Select(value => value.GetByte()), ally.Spells);
        }
        Assert.DoesNotContain(typeof(OriginalBattle01ControlledAlly).GetProperties(), property => property.Name.Contains("Activation", StringComparison.Ordinal));
        var gizmo = Definition().EnemyBaseline;
        var enemy = observed.EnumerateArray().Single(value => value.GetProperty("id").GetInt32() == 128);
        Assert.Equal(enemy.GetProperty("level").GetByte(), gizmo.Level);
        Assert.Equal(enemy.GetProperty("hpMax").GetUInt16(), gizmo.HpMax);
        Assert.Equal(enemy.GetProperty("mpMax").GetByte(), gizmo.MpMax);
        // enemy-promotions.md/placement owns source ATT 7; this post-initialization observation is 8.
        // Preparation must not replace the spawn baseline with a difficulty-adjusted observation.
        Assert.Equal(7, gizmo.BaseAttack);
        Assert.Equal(8, enemy.GetProperty("attack").GetByte());
        Assert.Equal(enemy.GetProperty("defense").GetByte(), gizmo.BaseDefense);
        Assert.Equal(enemy.GetProperty("agility").GetByte(), gizmo.BaseAgility);
        Assert.Equal(enemy.GetProperty("move").GetByte(), gizmo.BaseMove);
    }

    private sealed class Source(OriginalBattle01StartupImportResult result) : IOriginalBattle01StartupSource
    {
        public int Calls { get; private set; }
        public OriginalBattle01StartupImportResult Admit() { Calls++; return result; }
    }
    private static OriginalBattle01StartupProvenance Provenance() => new(OriginalBattle01StartupDefinition.Repository,
        OriginalBattle01StartupDefinition.UpstreamCommit, OriginalBattle01StartupDefinition.PlacementExportDigest, OriginalBattle01StartupDefinition.SceneExportDigest);
    private static OriginalBattle01Placement[] Entities() => Enumerable.Range(0, 9).Select(index => new OriginalBattle01Placement(
        (byte)index, index < 3 ? OriginalBattle01EntityKind.Ally : OriginalBattle01EntityKind.Enemy,
        (byte)(index < 3 ? index : 39), new(index, 1), OriginalBattle01AiCommandSet.Healer1, 127, 255, 0, 255, 0, 0, OriginalBattle01Spawn.Starting)).ToArray();
    private static OriginalBattle01AiRegion[] Regions() => Enumerable.Range(0, 3).Select(index => new OriginalBattle01AiRegion(
        (byte)index, 0, [new(0, 0), new(1, 0), new(1, 1), new(0, 1)], 0, 0)).ToArray();
    internal static OriginalBattle01StartupDefinition Definition(OriginalBattle01StartupProvenance? provenance = null,
        bool computeDeployment = false, bool computeTerrain = false, IEnumerable<byte>? terrain = null) => new(provenance ?? Provenance(), Entities(), Regions(), terrain ?? new byte[2304],
            computeDeployment ? null : OriginalBattle01StartupDefinition.AcceptedPlacementDigest,
            computeTerrain ? null : OriginalBattle01StartupDefinition.AcceptedTerrainDigest);

    internal static GameSession PendingSession(bool createPending = true)
    {
        // Reuse the accepted test-owned completed-route seed without widening production ownership.
        var method = typeof(OriginalMapGameSessionTests).GetMethod("Battle01AdmissionSession", BindingFlags.Static | BindingFlags.NonPublic)!;
        var session = (GameSession)method.Invoke(null, [new MapPosition(14, 13), null, true, null])!;
        var battle = (PublicSyntheticBattleDefinition)typeof(PrivateOriginalMapBattleBridgeTests)
            .GetMethod("Battle", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, null)!;
        var controlled = session.PrivateOriginalMapSnapshot.Definition.ControlledAdmission;
        var bridge = PrivateOriginalMapBattleBridgeSnapshot.Ready(new(controlled.Map, controlled.Position, battle));
        typeof(GameSession).GetField("_privateOriginalMapBattleBridge", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, bridge);
        if (createPending) session.ApplyPrivateOriginalMap(new(ExplorationDirection.North));
        return session;
    }
}
