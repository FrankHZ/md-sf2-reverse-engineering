using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01InitializationTests
{
    [Fact]
    public void ExpIsExplicitPreservedByHealingAndCopiesAndForbiddenOnEnemySource()
    {
        var party = Allies(); var s = party[0].EffectiveStats;
        Assert.Null(s.CurrentExp);
        party[0] = party[0] with { EffectiveStats = s.WithCurrentHp(9).WithCurrentExp(0) };
        var battle = Battle01Initialization.Initialize(Deployment(), Regions(), new byte[2304], party, Enemy(), 0x1234, 0);
        Assert.Equal(12, battle.Roster[0].Stats.HpCurrent); Assert.Equal((byte?)0, battle.Roster[0].Stats.CurrentExp);
        Assert.Equal((byte?)15, battle.Roster[0].Stats.WithCurrentExp(15).WithCurrentHp(9).CurrentExp);
        Assert.Throws<ArgumentException>(() => Battle01Initialization.Initialize(Deployment(), Regions(), new byte[2304], party,
            Enemy() with { SourceStats = Enemy().SourceStats.WithCurrentExp(0) }, 0x1234, 0));
        Assert.Throws<ArgumentException>(() => Battle01Initialization.Initialize(Deployment(), Regions(), new byte[2304], party,
            Enemy() with { SourceStats = Enemy().SourceStats.WithCurrentHp(4) }, 0x1234, 0));
    }

    [Fact]
    public void InitializesNineCombatantsWithSeparateTerrainOccupancyAndClearedPreRoundState()
    {
        var rows = Deployment(); var terrain = new byte[2304]; terrain[17] = 3; terrain[49] = 2;
        var state = Initialize(rows, terrain);
        Assert.Equal(new MapId("map57"), state.Map); Assert.Equal(1, state.BattleIndex);
        Assert.Equal((0, 0, 16, 20, 48), (state.AreaX, state.AreaY, state.AreaWidth, state.AreaHeight, state.TerrainStride));
        Assert.Equal(new[] { 0, 1, 2, 128, 129, 130, 131, 132, 133 }, state.Roster.Select(unit => unit.Index));
        Assert.Equal(rows, state.Roster.Select(unit => unit.Deployment));
        Assert.Equal(9, state.Occupancy.Count(index => index >= 0));
        Assert.Equal(2304, state.Terrain.Count); Assert.Equal(2304, state.Occupancy.Count);
        Assert.Equal(2, state.TerrainAt(new(1, 1))); Assert.Equal(3, state.TerrainAt(new(17, 0)));
        Assert.Equal(1, state.OccupantAt(new(1, 1))); Assert.Equal(-1, state.OccupantAt(new(17, 0)));
        Assert.All(state.Roster, unit => Assert.Equal(unit.Index, state.OccupantAt(unit.RequirePosition())));
        Assert.Equal(3, state.Regions.Count);
        Assert.Equal(16, state.RegionFlags90Through105.Count); Assert.All(state.RegionFlags90Through105, flag => Assert.False(flag));
        Assert.Equal(48, state.AiLastTargets.Count); Assert.All(state.AiLastTargets, value => Assert.Equal(255, value));
        Assert.Equal(48, state.AiMemory.Count); Assert.All(state.AiMemory, value => Assert.Equal(0, value));
        Assert.Equal(0, state.ElapsedSeconds); Assert.Equal(0x1234u, state.RandomSeedImage);
        Assert.Equal(0, state.GeneratorWord); Assert.Null(state.FirstRound);
        Assert.True(state.UnlockFlag401); Assert.True(state.IntroFlag451);
        Assert.False(state.CompletedFlag501); Assert.False(state.SuspendedFlag88);
        Assert.Equal(Battle01Phase.BeforeFirstRound, state.Phase);
        Assert.Equal(9, state.CustomBackground); Assert.True(state.HalfExperience); Assert.False(state.EnemyLeaderPresent);
    }

    [Fact]
    public void HealingRetainsEffectiveAllyEquipmentStatsAndEnemySourceSevenIsAdjustedOnce()
    {
        var allies = Allies();
        allies[0] = allies[0] with { EffectiveStats = new(1, 12, 3, 8, 2, 9, 4, 4, 6, 0xFFFF,
            [199, 0, 127, 127], [10, 63, 63, 63]) };
        var baseline = Enemy(); var rows = Deployment();
        rows[3] = rows[3] with { AiCommandSet = 6, SourceFiller = 0x40 };
        var battle = Battle01Initialization.Initialize(rows, Regions(), new byte[2304], allies, baseline, 0x1234, 0);
        var ally = battle.Roster[0];
        Assert.Equal((12, 8, 7, 9), ((int)ally.Stats.HpCurrent, ally.Stats.MpCurrent, ally.Stats.Status, ally.Stats.Attack));
        Assert.Equal(199, ally.Stats.Items[0]); Assert.Equal(3, allies[0].EffectiveStats.HpCurrent);
        Assert.Equal(0xFFFF, allies[0].EffectiveStats.Status); Assert.Null(ally.EnemySource);
        foreach (var enemy in battle.Roster.Skip(3))
        {
            Assert.Equal(7, enemy.EnemySource!.SourceStats.Attack);
            Assert.Equal((byte?)8, enemy.AdjustedBaseAttack); Assert.Equal(8, enemy.Stats.Attack);
            Assert.Equal((5, 5, 5, 5, 0, 0), ((int)enemy.Stats.HpCurrent, enemy.Stats.Defense, enemy.Stats.Agility,
                enemy.Stats.Move, enemy.Stats.MpCurrent, enemy.Stats.Status));
            Assert.Equal((ushort?)0x40E3, enemy.Resistance); Assert.Equal((byte?)0, enemy.Prowess);
            Assert.Equal(39, enemy.EnemySource.SourceUnknownByte); Assert.Null(enemy.ClassId);
        }
        Assert.Equal((byte?)0x66, battle.Roster[3].MovementTypeAndAiCommandSet);
        Assert.Equal((ushort?)0x2040, battle.Roster[3].InitializationAiBitfield);
        Assert.Equal(7, baseline.SourceStats.Attack);
    }

    [Fact]
    public void DefensiveCollectionsAndRepeatInitializationKeepInputsAndOutputsIndependent()
    {
        var rows = Deployment(); var terrain = new byte[2304]; var party = Allies();
        var points = new MapPosition[] { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
        var regions = Regions(); regions[0] = new(0, 9, points, 6, 7);
        var first = Battle01Initialization.Initialize(rows, regions, terrain, party, Enemy(), 0x1234, 0);
        var second = Battle01Initialization.Initialize(rows, regions, terrain, party, Enemy(), 0x1234, 0);
        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        Assert.NotSame(first, second); Assert.NotSame(first.Roster, second.Roster);
        rows[0] = rows[0] with { Position = new(12, 12) }; terrain[48] = 255;
        points[0] = new(10, 10); regions[0] = Regions()[0]; party[0] = party[0] with { ClassId = 9 };
        Assert.Equal(new MapPosition(0, 1), first.Roster[0].Position); Assert.Equal(0, first.Terrain[48]);
        Assert.Equal(new MapPosition(0, 0), first.Regions[0].Vertices[0]);
        Assert.Equal((9, 6, 7), ((int)first.Regions[0].SourceUnknown, first.Regions[0].TrailingByte0, first.Regions[0].TrailingByte1));
        Assert.Throws<NotSupportedException>(() => ((IList<int>)first.Occupancy)[48] = -1);
        Assert.Throws<NotSupportedException>(() => ((IList<byte>)first.Terrain)[48] = 255);
        Assert.Throws<NotSupportedException>(() => ((IList<byte>)first.AiMemory)[0] = 1);
        Assert.Throws<NotSupportedException>(() => ((IList<bool>)first.RegionFlags90Through105)[0] = true);
        Assert.Throws<NotSupportedException>(() => ((IList<ushort>)first.Roster[0].Stats.Items)[0] = 0);
        Assert.Throws<NotSupportedException>(() => ((IList<Battle01Combatant>)first.Roster)[0] = first.Roster[1]);
    }

    [Theory]
    [InlineData("overlap", "deployment")]
    [InlineData("index", "deployment")]
    [InlineData("spawn", "deployment")]
    [InlineData("region", "regions")]
    [InlineData("stride", "terrain")]
    [InlineData("occupied-obstruction", "terrain")]
    public void InvalidFinalProjectionRejectsItsOwningBoundary(string drift, string field)
    {
        var rows = Deployment(); var regions = Regions(); var terrain = new byte[2304];
        switch (drift)
        {
            case "overlap": rows[1] = rows[1] with { Position = rows[0].Position }; break;
            case "index": rows[8] = rows[8] with { CombatantIndex = 134 }; break;
            case "spawn": rows[8] = rows[8] with { Spawn = 1 }; break;
            case "region": regions = regions[..2]; break;
            case "stride": terrain = new byte[16 * 20]; break;
            case "occupied-obstruction": terrain[48] = 255; break;
        }
        var error = Assert.Throws<ArgumentException>(() =>
            Battle01Initialization.Initialize(rows, regions, terrain, Allies(), Enemy(), 0x1234, 0));
        Assert.Equal(field, error.ParamName);
    }

    [Fact]
    public void OtherDifficultySeedAndEnemyBaselineRemainUnsupported()
    {
        Assert.Throws<ArgumentException>(() => Battle01Initialization.Initialize(Deployment(), Regions(), new byte[2304], Allies(), Enemy(), 0x1234, 2));
        Assert.Throws<ArgumentException>(() => Battle01Initialization.Initialize(Deployment(), Regions(), new byte[2304], Allies(), Enemy(), 1, 0));
        Assert.Throws<ArgumentException>(() => Battle01Initialization.Initialize(Deployment(), Regions(), new byte[2304], Allies(), Enemy() with { DefinitionId = 40 }, 0x1234, 0));
    }

    [Fact]
    public void IndependentSeedCopyIsExplicitAndMissingIsNeverDerivedFromTheMainImage()
    {
        var missing = Initialize(Deployment(), new byte[2304]); Assert.Null(missing.RandomSeedCopy);
        var supplied = Battle01Initialization.Initialize(Deployment(), Regions(), new byte[2304], Allies(), Enemy(), 0x1234, 0, 0x1234);
        Assert.Equal((ushort?)0x1234, supplied.RandomSeedCopy); Assert.Equal(missing.RandomSeedImage, supplied.RandomSeedImage);
        Assert.Equal("randomSeedCopy", Assert.Throws<ArgumentException>(() => Battle01Initialization.Initialize(
            Deployment(), Regions(), new byte[2304], Allies(), Enemy(), 0x1234, 0, 0x3412)).ParamName);
    }

    private static Battle01InitializedState Initialize(Battle01Deployment[] rows, byte[] terrain) =>
        Battle01Initialization.Initialize(rows, Regions(), terrain, Allies(), Enemy(), 0x1234, 0);
    private static Battle01Deployment[] Deployment() => Enumerable.Range(0, 9).Select(index =>
        new Battle01Deployment((byte)index, index < 3 ? index : 128 + index - 3,
            (byte)(index < 3 ? index : 39), new(index, 1), 0, 127, 255, 0, 255, 15, 0, 0)).ToArray();
    private static Battle01Region[] Regions() => Enumerable.Range(0, 3).Select(index =>
        new Battle01Region((byte)index, 0, [new(0, 0), new(1, 0), new(1, 1), new(0, 1)], 0, 0)).ToArray();
    private static Battle01AllyInput[] Allies() => Enumerable.Range(0, 3).Select(index =>
        new Battle01AllyInput((byte)index, (byte)(index == 1 ? 4 : index == 2 ? 1 : 0),
            new(1, 12, 12, 8, 8, 9, 4, 4, 6, 0, [199, 0, 127, 127], [10, 63, 63, 63]))).ToArray();
    internal static Battle01EnemyInput Enemy() => new(39, 39, 0,
        new(0, 5, 5, 0, 0, 7, 5, 5, 5, 0, [127, 127, 127, 127], [63, 63, 63, 63]), 0x40E3, 0, 6, 0x2000);
}
