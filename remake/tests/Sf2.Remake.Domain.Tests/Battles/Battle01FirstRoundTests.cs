using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Domain.Tests.Battles;

public sealed class Battle01FirstRoundTests
{
    [Fact]
    public void AllyDefeatRewindsTheOldRoundBeforeGeneratingOnlySixSurvivors()
    {
        var after=Battle01EnemyPhysicalAttackTests.FirstAllyDefeatCompleted();
        var old=Battle01FirstRound.GenerationRoster(after,13);
        Assert.Equal(7,old.Count(unit=>unit.Position is not null && unit.Stats.HpCurrent>0));
        Assert.Equal(new MapPosition(9,9),old[2].Position);Assert.Equal(1,old[2].Stats.HpCurrent);
        Assert.Equal((ushort?)0,old[2].Stats.CurrentDefeats);
        Assert.Null(old[6].Position);Assert.Null(old[7].Position);
        var end=Battle01EnemyPursuit.CompleteNext(after,130,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        var generated=Battle01FirstRound.EnterNext(end);
        Assert.Equal(14,generated.FirstRound!.RoundNumber);Assert.Equal(0x02A11234u,generated.RandomSeedImage);
        Assert.Equal(new[]{new Battle01TurnEntry(1,6),new(129,6),new(128,5),new(130,5),new(0,4),new(133,4)}
            .Concat(Enumerable.Repeat(new Battle01TurnEntry(255,255),58)),generated.FirstRound.Slots);
        Assert.Same(end.TurnCompletion,generated.TurnCompletion);Assert.Equal(end.AiMemory,generated.AiMemory);
        Assert.Equal(end.AiLastTargets,generated.AiLastTargets);Assert.Equal((ushort?)0x0234,generated.RandomSeedCopy);
        Battle01EnemyStandby.RequireThinkingHistory(generated);
        Assert.Throws<ArgumentException>(()=>Battle01FirstRound.RequireGenerationFromRecordedMain(
            generated,0x33171234,0x02A21234,true,14));
    }

    [Fact]
    public void SecondDefeatRetainsHistoricalCandidateSetsAndGeneratesExactlySevenSurvivors()
    {
        var before = Battle01PlayerPhysicalAttackTests.SecondDefeatCompleted();
        Assert.Equal(9, Battle01FirstRound.GenerationRoster(before, 7).Count(u => u.Stats.HpCurrent > 0));
        Assert.Equal(8, Battle01FirstRound.GenerationRoster(before, 9).Count(u => u.Stats.HpCurrent > 0));
        Assert.Equal(7, Battle01FirstRound.GenerationRoster(before, 10).Count(u => u.Stats.HpCurrent > 0));
        ushort word = before.GeneratorWord;
        foreach (var unit in before.Roster.Where(u => u.Stats.HpCurrent > 0))
            foreach (ushort range in new ushort[] { (ushort)(unit.Stats.Agility >> 3), (ushort)(unit.Stats.Agility >> 3), 3 })
                Battle01FirstRound.NextRandom(ref word, range);
        var generated = Battle01FirstRound.EnterNext(before);
        Assert.Equal((ushort)0x9F86, word); Assert.Equal(0x9F861234u, generated.RandomSeedImage);
        Assert.Equal(new[] { new Battle01TurnEntry(2, 7), new(1, 6), new(128, 5), new(129, 5), new(130, 5), new(133, 5), new(0, 4) }
            .Concat(Enumerable.Repeat(new Battle01TurnEntry(255, 255), 57)), generated.FirstRound!.Slots);
        Assert.Equal(10, generated.FirstRound.RoundNumber); Assert.Equal(7, generated.NewlyTestedRegionMask);
        Assert.Same(before.TurnCompletion, generated.TurnCompletion); Assert.Same(before.AiMemory, generated.AiMemory);
        Assert.Same(before.AiLastTargets, generated.AiLastTargets); Assert.Equal((ushort?)0x0034, generated.RandomSeedCopy);
        Assert.Null(generated.Roster[6].Position); Assert.Null(generated.Roster[7].Position);
        var slots = generated.FirstRound.Slots.ToArray(); slots[7] = new(131, 3);
        var forged = CopyCurrent(generated, order: new(slots, [], [], 10));
        Assert.ThrowsAny<ArgumentException>(() => Battle01NextPlayerControl.Enter(forged, 2));
    }

    [Fact]
    public void NextGenerationStartsFromPostAttackMainSeedWithoutReinitializingHpOrTargets()
    {
        var before = Battle01EnemyPursuit.CompleteNext(Battle01EnemyPhysicalAttackTests.CompletedBowie(), 131,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        before = Battle01EnemyStandby.CompleteNext(before, 133, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        ushort word = before.GeneratorWord;
        for (int i = 0; i < 27; i++) Battle01FirstRound.NextRandom(ref word, 3);
        var next = Battle01FirstRound.EnterNext(before);
        Assert.Equal(7, next.FirstRound!.RoundNumber);
        Assert.Equal(((uint)word << 16) | 0x1234u, next.RandomSeedImage);
        Assert.Equal(9, next.Roster[0].Stats.HpCurrent); Assert.Same(before.Roster[0].Stats, next.Roster[0].Stats);
        Assert.Same(before.AiLastTargets, next.AiLastTargets); Assert.Equal(0, next.AiLastTargets[4]);
        Assert.Same(before.TurnCompletion, next.TurnCompletion);
    }

    [Fact]
    public void NextRoundUsesCurrentMainWordAndRetainsTheCompletePriorStateAndHistory()
    {
        var before = CompletedFirstRound(); string frozen = JsonSerializer.Serialize(before);
        var next = Battle01FirstRound.EnterNext(before);
        Assert.Equal(Battle01Phase.RoundGenerated, next.Phase); Assert.Equal(2, next.FirstRound!.RoundNumber);
        Assert.Equal(0, next.FirstRound.CurrentTurnOffset); Assert.Null(next.FirstControl);
        Assert.Equal(new byte[] { 2,128,132,131,133,0,1,129,130 }, next.FirstRound.Slots.Take(9).Select(slot => slot.CombatantIndex));
        Assert.Equal(new byte[] { 7,6,6,5,5,4,4,4,4 }, next.FirstRound.Slots.Take(9).Select(slot => slot.AlteredAgility));
        Assert.Equal(64, next.FirstRound.Slots.Count); Assert.All(next.FirstRound.Slots.Skip(9), slot => Assert.Equal(new Battle01TurnEntry(255,255), slot));
        Assert.Equal(0xAA861234u, next.RandomSeedImage); Assert.Equal((ushort?)0x0134, next.RandomSeedCopy);
        Assert.Same(before.TurnCompletion, next.TurnCompletion); Assert.Equal(1, next.TurnCompletion!.RoundNumber);
        Assert.Same(before.AiMemory, next.AiMemory); Assert.Same(before.AiLastTargets, next.AiLastTargets);
        Assert.Same(before.Occupancy, next.Occupancy); Assert.Same(before.Terrain, next.Terrain);
        for (int i = 0; i < 9; i++)
        {
            Assert.Same(before.Roster[i].Stats, next.Roster[i].Stats); Assert.Same(before.Roster[i].Deployment, next.Roster[i].Deployment);
            Assert.Equal(before.Roster[i].Position, next.Roster[i].Position); Assert.Equal(before.Roster[i].AiBitfield, next.Roster[i].AiBitfield);
        }
        Assert.Equal(7, next.NewlyTestedRegionMask); Assert.All(next.RegionFlags90Through105, Assert.False);
        Assert.Empty(next.FirstRound.RegionCutsceneRows); Assert.Empty(next.FirstRound.SpawnedCombatants);
        Assert.Equal("phase", Assert.Throws<ArgumentException>(() => Battle01FirstRound.EnterNext(next)).ParamName);
        Assert.Throws<ArgumentException>(() => Battle01FirstRound.Enter(next));
        Assert.Equal(frozen, JsonSerializer.Serialize(before));
    }

    [Fact]
    public void RepeatedOriginStayRoundsRetainNonzeroMemoryAndMatchIndependentComposedResults()
    {
        var first = CompletedFirstRound(); var current = first;
        foreach (var expected in new[] {
            (Number:2, Main:0xAA861234u, Copy:(ushort)0x0034, Steps:967, Memory:new byte[] {4,4,4,0x24,0x34,4}),
            (Number:3, Main:0x9BD71234u, Copy:(ushort)0x0234, Steps:958, Memory:new byte[] {4,0x24,0x14,0x34,0x24,0x14}) })
        {
            var generated = Battle01FirstRound.EnterNext(current); var previous = current.TurnCompletion;
            current = CompleteOriginRound(generated);
            Assert.Equal(expected.Number, current.FirstRound!.RoundNumber); Assert.Equal(18, current.FirstRound.CurrentTurnOffset);
            Assert.Null(current.FirstRound.CurrentCandidate); Assert.Equal(expected.Main, current.RandomSeedImage);
            Assert.Equal((ushort?)expected.Copy, current.RandomSeedCopy); Assert.Equal(expected.Memory, current.AiMemory.Take(6));
            var receipt = current.TurnCompletion; var decisions = new List<Battle01EnemyStandbyDecision>();
            for (int i = 8; i >= 0; i--)
            {
                Assert.Equal(expected.Number, receipt!.RoundNumber); Assert.Equal(generated.FirstRound!.Slots[i].CombatantIndex, receipt.CompletedActorIndex);
                if (receipt.EnemyStandby is { } decision) decisions.Add(decision);
                receipt = receipt.Previous;
            }
            Assert.Same(previous, receipt); Assert.Equal(expected.Steps, decisions.Sum(d => d.Rolls.Sum(r => r.GeneratorSteps)));
            Assert.Equal(11, decisions.Sum(d => d.Rolls.Count)); Assert.Contains(decisions, d => d.MoveString.Count == 3);
            Assert.Contains(decisions, d => d.MoveString.SequenceEqual(new byte[] {255}) && d.MemoryAfter == d.MemoryBefore);
            Assert.Equal(first.Roster.Take(3).Select(unit => unit.Position), current.Roster.Take(3).Select(unit => unit.Position));
            Assert.All(current.AiMemory.Skip(6), value => Assert.Equal(0, value)); Assert.Equal(0, current.NewlyTestedRegionMask);
        }
        var thirdActors = current.FirstRound!.Slots.Take(9).Select(slot => slot.CombatantIndex);
        Assert.Equal(new byte[] {2,129,130,131,128,133,0,1,132}, thirdActors);
        Assert.Equal(new byte[] {8,6,6,6,5,5,4,4,4}, current.FirstRound.Slots.Take(9).Select(slot => slot.AlteredAgility));
    }

    [Fact]
    public void ReachableRegionEntryActivatesKnownPrimaryWordsWithoutResettingRetainedState()
    {
        var current = Battle01FirstRound.EnterNext(CompletedFirstRound());
        while (current.FirstRound!.CurrentCandidate!.Value.CombatantIndex != 0) current = CompleteOriginTurn(current);
        var ready = Battle01NextPlayerControl.Enter(current,0).State!;
        var selected = Battle01PlayerMovement.SelectDestination(ready,0,new(11,15));
        Assert.Equal(new MapPosition(8,17),selected.FirstControl!.Movement.Range.Origin);
        Assert.Equal(10,selected.FirstControl.Movement.GridCost);
        current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(selected,0),0,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        while (current.FirstRound!.CurrentCandidate is not null) current = CompleteOriginTurn(current);
        var before = current; string frozen = JsonSerializer.Serialize(before);
        var active = Battle01FirstRound.EnterNext(before);
        Assert.Equal(new[] { false, true, false }, active.RegionFlags90Through105.Take(3));
        Assert.Equal(new ushort?[] { 0x2060,0x2060,0x2060,0x2061,0x2071,0x2070 }, active.Roster.Skip(3).Select(unit => unit.AiBitfield));
        Assert.Equal(0x9BD71234u, active.RandomSeedImage); Assert.Equal(before.RandomSeedCopy, active.RandomSeedCopy);
        Assert.Same(before.AiMemory, active.AiMemory); Assert.Same(before.TurnCompletion, active.TurnCompletion);
        Assert.Equal(frozen, JsonSerializer.Serialize(before)); Assert.All(before.RegionFlags90Through105, Assert.False);
        Assert.Equal(0xAA861234u, before.RandomSeedImage); Assert.Equal((ushort?)0x0034, before.RandomSeedCopy);
        // A separately authored retained tested mask proves generation does not invent a new clear.
        var alreadyTested = CopyCurrent(before, tested: 7);
        var next = Battle01FirstRound.EnterNext(alreadyTested);
        Assert.All(next.RegionFlags90Through105, Assert.False); Assert.Equal(7, next.NewlyTestedRegionMask);
    }

    [Theory]
    [InlineData("receipt", "completion")]
    [InlineData("generation", "completion")]
    [InlineData("order", "turnOrder")]
    [InlineData("memory", "memory")]
    [InlineData("seed", "randomSeedCopy")]
    [InlineData("occupancy", "occupancy")]
    [InlineData("status", "status")]
    [InlineData("active", "activation.actor128")]
    public void InvalidCompletedRoundRejectsWithoutChangingTheLastState(string mutation, string field)
    {
        var source = CompletedFirstRound(); var roster = source.Roster.ToArray(); var occupancy = source.Occupancy.ToArray();
        var memory = source.AiMemory.ToArray(); var order = source.FirstRound!; var receipt = source.TurnCompletion!;
        ushort copy = source.RandomSeedCopy!.Value;
        if (mutation == "receipt") receipt = receipt with { Previous = receipt.Previous!.Previous };
        if (mutation == "generation") receipt = receipt with { RoundNumber = 2 };
        if (mutation == "order") { var slots = order.Slots.ToArray(); slots[0] = new(128,5); order = new(slots,[],[]); for (int i=0;i<9;i++) order=order.AdvanceCompletedPlayerTurn(); }
        if (mutation == "memory") memory[3] ^= 0x10;
        if (mutation == "seed") copy ^= 0x100;
        if (mutation == "occupancy") occupancy[17*48+9] = -1;
        if (mutation == "status") { var unit=roster[0]; roster[0]=new(unit.Deployment, Battle01FirstControlTests.Stats(unit.Stats,1,unit.Stats.Move),unit.ClassId,unit.EnemySource,unit.AiBitfield,unit.Position); }
        if (mutation == "active") roster[3]=roster[3].WithAiBitfield(0x2061);
        var input=CopyCurrent(source,roster:roster,occupancy:occupancy,memory:memory,copy:copy,order:order,receipt:receipt);
        string frozen=JsonSerializer.Serialize(input);
        Assert.Equal(field,Assert.Throws<ArgumentException>(()=>Battle01FirstRound.EnterNext(input)).ParamName);
        Assert.Equal(frozen,JsonSerializer.Serialize(input));
    }

    internal static Battle01InitializedState CompletedFirstRound()
    {
        var ready=Battle01NextPlayerControl.Enter(Battle01EnemyStandbyTests.AllEnemiesCompleted(),0).State!;
        var selected=Battle01PlayerMovement.SelectDestination(ready,0,new(8,17));
        return Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(selected,0),0,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
    }
    internal static Battle01InitializedState CompleteOriginRound(Battle01InitializedState current)
    {
        for (int i=0;i<9;i++) current=CompleteOriginTurn(current);
        return current;
    }
    internal static Battle01InitializedState CompleteOriginTurn(Battle01InitializedState current)
    {
        int actor=current.FirstRound!.CurrentCandidate!.Value.CombatantIndex;
        if (actor>=128) return Battle01EnemyStandby.CompleteNext(current,actor,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        var ready=Battle01NextPlayerControl.Enter(current,actor).State!;
        return Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(ready,actor),actor,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
    }
    internal static Battle01InitializedState CopyCurrent(Battle01InitializedState source, Battle01Combatant[]? roster=null,
        int[]? occupancy=null, byte[]? memory=null, ushort? copy=null, byte[]? terrain=null, ushort? tested=null,
        Battle01FirstRoundOrder? order=null, Battle01TurnCompletionReceipt? receipt=null, uint? mainImage=null,
        byte[]? lastTargets=null, uint? gold=null)
    {
        var initial=new Battle01InitializedState(roster ?? source.Roster.ToArray(),source.Regions.ToArray(),terrain ?? source.Terrain.ToArray(),
            occupancy ?? source.Occupancy.ToArray(),mainImage ?? source.RandomSeedImage,copy ?? source.RandomSeedCopy, gold ?? source.CurrentGold);
        var thinking=new Battle01InitializedState(initial,initial.Roster.ToArray(),initial.Occupancy.ToArray(),memory ?? source.AiMemory.ToArray(),
            copy ?? source.RandomSeedCopy!.Value, mainImage ?? source.RandomSeedImage, lastTargets ?? source.AiLastTargets.ToArray());
        var round=new Battle01InitializedState(thinking,thinking.Roster.ToArray(),source.RegionFlags90Through105.ToArray(),
            tested ?? source.NewlyTestedRegionMask,mainImage ?? source.RandomSeedImage,order ?? source.FirstRound!);
        return new(round,round.FirstRound!,receipt ?? source.TurnCompletion!);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0x1234)]
    public void RoundGenerationRetainsTheIndependentSeedCopyWithoutUsingIt(int? copy)
    {
        var initial = Initial(); var seeded = new Battle01InitializedState(initial.Roster.ToArray(), initial.Regions.ToArray(),
            initial.Terrain.ToArray(), initial.Occupancy.ToArray(), initial.RandomSeedImage, (ushort?)copy);
        var after = Battle01FirstRound.Enter(seeded);
        Assert.Equal((ushort?)copy, after.RandomSeedCopy); Assert.Equal(0xA4991234u, after.RandomSeedImage);
        Assert.Equal(Battle01FirstRound.Enter(initial).FirstRound!.Slots, after.FirstRound!.Slots);
    }

    [Fact]
    public void BaselineTestsThreeRegionsWithoutActivatingThemAndComputesTheAcceptedRound()
    {
        var initial = Initial(); var next = Battle01FirstRound.Enter(initial);
        using var activation = Fixture("battle01-region-activation-v1");
        var expectedActivation = activation.RootElement.GetProperty("expected").GetProperty("initial");
        Assert.Equal(expectedActivation.GetProperty("newlyTriggered").GetUInt16(), next.NewlyTestedRegionMask);
        Assert.Equal(expectedActivation.GetProperty("regionFlags").EnumerateArray().Select(flag => flag.GetBoolean()),
            next.RegionFlags90Through105.Take(3));
        Assert.All(next.RegionFlags90Through105, flag => Assert.False(flag));
        foreach (var row in expectedActivation.GetProperty("enemies").EnumerateArray())
        {
            var unit = next.Roster.Single(unit => unit.Index == row.GetProperty("combatant").GetInt32());
            Assert.Equal((ushort?)row.GetProperty("bitfield").GetUInt16(), unit.AiBitfield);
        }
        using var ready = Fixture("map3-battle01-player-ready-v1");
        var record = ready.RootElement.GetProperty("expectedObservation").GetProperty("records")[0];
        Assert.Equal(record.GetProperty("turnState").GetProperty("entries").EnumerateArray()
            .Select(row => new Battle01TurnEntry(row.GetProperty("actor").GetByte(), row.GetProperty("score").GetByte())),
            next.FirstRound!.Slots.Take(9));
        Assert.Equal(64, next.FirstRound.Slots.Count);
        Assert.All(next.FirstRound.Slots.Skip(9), slot => Assert.Equal(new Battle01TurnEntry(255, 255), slot));
        Assert.Equal(0, next.FirstRound.CurrentTurnOffset); Assert.Equal((byte)1, next.FirstRound.FirstCandidate!.Value.CombatantIndex);
        Assert.Empty(next.FirstRound.RegionCutsceneRows); Assert.Empty(next.FirstRound.SpawnedCombatants);
        Assert.Equal(record.GetProperty("deterministicState").GetProperty("ready").GetProperty("randomSeed").GetUInt32(), next.RandomSeedImage);
        Assert.Equal(0xA499, next.GeneratorWord); Assert.Equal(0x1234u, next.RandomSeedImage & 0xFFFF);
        Assert.Equal(Battle01Phase.FirstRoundGenerated, next.Phase);
        Assert.Equal(9, next.Roster.Count); // Inactive regions do not exclude placed living enemies.
        Assert.All(next.Roster, unit => Assert.Equal(unit.Index, next.OccupantAt(unit.RequirePosition())));
        Assert.Same(initial.Terrain, next.Terrain); Assert.Same(initial.Occupancy, next.Occupancy);
        Assert.Same(initial.AiMemory, next.AiMemory); Assert.Same(initial.AiLastTargets, next.AiLastTargets);
        Assert.Equal(0, next.ElapsedSeconds); Assert.True(next.UnlockFlag401); Assert.True(next.IntroFlag451);
        Assert.False(next.CompletedFlag501); Assert.False(next.SuspendedFlag88);
    }

    [Fact]
    public void ControlledEdgeActivatesPrimaryBitsAndPreservesInitialSourceFields()
    {
        var initial = Initial(12); var next = Battle01FirstRound.Enter(initial);
        using var fixture = Fixture("battle01-region-activation-v1");
        var expected = fixture.RootElement.GetProperty("expected").GetProperty("controlled");
        Assert.Equal(expected.GetProperty("newlyTriggered").GetUInt16(), next.NewlyTestedRegionMask);
        Assert.Equal(expected.GetProperty("regionFlags").EnumerateArray().Select(flag => flag.GetBoolean()),
            next.RegionFlags90Through105.Take(3));
        foreach (var row in expected.GetProperty("enemies").EnumerateArray())
        {
            var before = initial.Roster.Single(unit => unit.Index == row.GetProperty("combatant").GetInt32());
            var after = next.Roster.Single(unit => unit.Index == before.Index);
            Assert.Equal((ushort?)row.GetProperty("bitfield").GetUInt16(), after.AiBitfield);
            Assert.Equal(before.InitializationAiBitfield, after.InitializationAiBitfield);
            Assert.Equal(before.InitializationAiBitfield, before.AiBitfield);
            Assert.Same(before.Stats, after.Stats); Assert.Same(before.Deployment, after.Deployment);
        }
        Assert.Equal(0, initial.NewlyTestedRegionMask); Assert.All(initial.RegionFlags90Through105, flag => Assert.False(flag));
        Assert.Equal(0x1234u, initial.RandomSeedImage); Assert.Null(initial.FirstRound);
        Assert.Throws<NotSupportedException>(() => ((IList<Battle01TurnEntry>)next.FirstRound!.Slots)[0] = new(2, 0));
        Assert.Throws<NotSupportedException>(() => ((IList<bool>)next.RegionFlags90Through105)[0] = false);
        Assert.Throws<ArgumentException>(() => Battle01FirstRound.Enter(next));
        Assert.Equal(JsonSerializer.Serialize(next), JsonSerializer.Serialize(Battle01FirstRound.Enter(initial)));
    }

    [Fact]
    public void SecondaryRegionFixtureSetsBothLowBitsAndPrimaryRetainsPrecedence()
    {
        using var fixture = Fixture("battle01-secondary-activation-v1");
        var input = fixture.RootElement.GetProperty("setup").GetProperty("enemy");
        var expected = fixture.RootElement.GetProperty("expected");
        bool[] flags = expected.GetProperty("regionFlags").EnumerateArray().Select(flag => flag.GetBoolean())
            .Concat(Enumerable.Repeat(false, 13)).ToArray();
        ushort initialBits = input.GetProperty("initialActivationBitfield").GetUInt16();
        Assert.Equal(expected.GetProperty("enemy").GetProperty("activationBitfield").GetUInt16(),
            Battle01FirstRound.ActivateAssignedRegions(initialBits, input.GetProperty("controlledPrimaryRegion").GetByte(),
                input.GetProperty("controlledSecondaryRegion").GetByte(), flags));
        Assert.Equal((ushort)0x2061, Battle01FirstRound.ActivateAssignedRegions(initialBits, 0, 2, flags));
        Assert.Equal(initialBits, Battle01FirstRound.ActivateAssignedRegions(initialBits, 15, 15, flags));
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(15, 12, true)]
    [InlineData(8, 12, true)]
    [InlineData(8, 13, false)]
    [InlineData(8, 18, false)]
    public void QuadUsesBothTrianglesAndIncludesEdges(int x, int y, bool inside) =>
        Assert.Equal(inside, Battle01FirstRound.IsInside(Initial().Regions[2], new(x, y)));

    [Fact]
    public void WordRngMatchesExistingFixtureWhileZeroRangeStillAdvancesTheGenerator()
    {
        using var fixture = Fixture("rng-v1");
        foreach (var row in fixture.RootElement.GetProperty("cases").EnumerateArray())
        {
            ushort word = row.GetProperty("seed").GetUInt16();
            ushort result = Battle01FirstRound.NextRandom(ref word, row.GetProperty("range").GetUInt16());
            Assert.Equal(row.GetProperty("expectedSeed").GetUInt16(), word);
            Assert.Equal(row.GetProperty("expectedValue").GetUInt16(), result);
        }
        var initial = Initial();
        Assert.Equal(0x00001234u, initial.RandomSeedImage); Assert.Equal(0, initial.GeneratorWord);
        ushort first = initial.GeneratorWord;
        Assert.Equal(0, Battle01FirstRound.NextRandom(ref first, 0)); Assert.Equal(7, first);
        Assert.Equal(0, Battle01FirstRound.NextRandom(ref first, 0)); Assert.Equal(98, first);
        ushort advanced = initial.GeneratorWord;
        for (int index = 0; index < 27; index++) Battle01FirstRound.NextRandom(ref advanced, 0);
        Assert.Equal(Battle01FirstRound.Enter(initial).GeneratorWord, advanced);
    }

    [Fact]
    public void BoundaryFixturePreservesSecondEntriesSignedStableSortAndAllSentinelSlots()
    {
        using var fixture = Fixture("turn-order-boundaries-v1");
        var candidates = Initial().Roster.Select(unit => new Battle01FirstRound.TurnCandidate(
            (byte)unit.Index, (byte)unit.RequirePosition().X, unit.Stats.HpCurrent, unit.Stats.Agility)).ToArray();
        foreach (var mutation in fixture.RootElement.GetProperty("mutations").EnumerateArray())
        {
            int index = Array.FindIndex(candidates, candidate => candidate.Index == mutation.GetProperty("combatant").GetByte());
            var value = mutation.GetProperty("value");
            candidates[index] = mutation.GetProperty("field").GetString() switch
            {
                "currentAgi" => candidates[index] with { Agility = value.GetByte() },
                "currentHp" => candidates[index] with { CurrentHp = value.GetUInt16() },
                "x" => candidates[index] with { X = value.GetByte() },
                _ => throw new InvalidDataException("Unexpected accepted mutation field."),
            };
        }
        ushort word = fixture.RootElement.GetProperty("seed").GetUInt16();
        var slots = Battle01FirstRound.GenerateTurnOrder(candidates.Reverse(), ref word);
        Assert.Equal(fixture.RootElement.GetProperty("expectedEntries").EnumerateArray()
            .Select(row => new Battle01TurnEntry(row.GetProperty("combatant").GetByte(), row.GetProperty("score").GetByte())),
            slots.Where(slot => !slot.IsSentinel));
        Assert.Equal(64, slots.Length); Assert.Equal(56, slots.Count(slot => slot.IsSentinel));
        Assert.Equal(new Battle01TurnEntry(0, 255), slots[6]); // Stable tie ahead of empty -1 slots.
        Assert.All(slots.Skip(7).Take(56), slot => Assert.Equal(new Battle01TurnEntry(255, 255), slot));
        Assert.Equal(new Battle01TurnEntry(1, 135), slots[63]); // Preserve the signed-negative entry beyond sentinels.
        Assert.DoesNotContain(slots, slot => slot.CombatantIndex is 2 or 128);
    }

    [Fact]
    public void UnsupportedSpawnRejectsAfterLocalActivationWithoutChangingTheInput()
    {
        var source = Initial(12); var roster = source.Roster.ToArray(); var enemy = roster[3];
        roster[3] = new(enemy.Deployment with { Spawn = 1 }, enemy.Stats, enemy.ClassId, enemy.EnemySource);
        var invalid = new Battle01InitializedState(roster, source.Regions.ToArray(), source.Terrain.ToArray(),
            source.Occupancy.ToArray(), source.RandomSeedImage);
        var error = Assert.Throws<ArgumentException>(() => Battle01FirstRound.Enter(invalid));
        Assert.Equal("spawn", error.ParamName);
        Assert.All(invalid.RegionFlags90Through105, flag => Assert.False(flag));
        Assert.Equal(0x1234u, invalid.RandomSeedImage); Assert.Null(invalid.FirstRound);
    }

    internal static Battle01InitializedState Initial(int bowieY = 18)
    {
        byte[] primary = [2, 2, 2, 1, 1, 0]; MapPosition[] allies = [new(8, bowieY), new(9, 18), new(7, 18)];
        var rows = Enumerable.Range(0, 9).Select(index => new Battle01Deployment((byte)index,
            index < 3 ? index : 128 + index - 3, (byte)(index < 3 ? index : 39),
            index < 3 ? allies[index] : new(4 + index - 3, 5), (byte)(index >= 7 ? 7 : index >= 3 ? 6 : 0),
            127, 255, index < 3 ? (byte)0 : primary[index - 3], 255, 15,
            (byte)(index >= 7 ? 112 : index >= 3 ? 96 : 0), 0));
        Battle01Region[] regions = [
            new(0, 0, [new(0, 0), new(0, 19), new(15, 7), new(15, 0)], 0, 0),
            new(1, 0, [new(0, 0), new(0, 7), new(15, 19), new(15, 0)], 0, 0),
            new(2, 0, [new(0, 0), new(0, 12), new(15, 12), new(15, 0)], 0, 0)];
        byte[] agility = [4, 5, 7];
        var party = Enumerable.Range(0, 3).Select(index => new Battle01AllyInput((byte)index, (byte)index,
            new(1, 12, 12, 8, 8, 9, 4, agility[index], 6, 0, [127, 127, 127, 127], [63, 63, 63, 63])));
        // Authored empty terrain and bounded source facts; no private grid or observation snapshot is copied.
        return Battle01Initialization.Initialize(rows, regions, new byte[2304], party, Battle01InitializationTests.Enemy(), 0x1234, 0);
    }
    private static JsonDocument Fixture(string name) => JsonDocument.Parse(File.ReadAllText(Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../../../tests/fixtures/h3", name + ".json"))));
}
