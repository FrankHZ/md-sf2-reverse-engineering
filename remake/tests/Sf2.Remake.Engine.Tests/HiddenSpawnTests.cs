using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class HiddenSpawnTests
{
    internal static readonly ActorRef Leader = new("leader"), Returner = new("returner"), Hidden = new("hidden"), Sentry = new("sentry");
    internal static EngineBattleState Initial()
    {
        var input = Admitted("hidden-spawns");
        return BattleTurnFlow.Start(input.Definition.Encounters[input.Start.Encounter], input.Start);
    }
    internal static EngineBattleState Held(EngineBattleState b, ushort tested = 1)
    { var flags = new bool[16]; flags[0] = true; return b.With(regions: new(flags, tested)); }

    [Fact]
    public void DormantMaximumIsRuntimeStateAndFreshBattleUsesOnlyModeBits()
    {
        var input = Admitted("hidden-spawns"); var definition = input.Definition.Encounters[input.Start.Encounter];
        var start = new BattleStartInput(input.Start.Encounter, input.Start.Actors, 0, 0, 0,
            new("authored-fresh", "project-tests", "not natural entry", 0, true, true, true, false, false, 0));
        var b = BattleTurnFlow.Start(definition, start);
        foreach (var id in new[] { Hidden, Returner })
        { var actor = b.GetActor(id); Assert.False(actor.Initialized); Assert.Equal(0, actor.MaxHp); Assert.Equal(0, actor.Hp); Assert.Null(actor.Position); Assert.Equal(1, actor.Definition.MaxHp); }
        Assert.Equal((ushort)512, b.GetActor(Hidden).ActivationWord);
        Assert.Equal((ushort)768, b.GetActor(Returner).ActivationWord);
        Assert.Equal(0, b.Regions!.Tested); Assert.DoesNotContain(true, b.Regions.Flags);
    }

    [Fact]
    public void ResetListIsSlotOrderedAndSuccessRestoresStatsButRetainsExternalAiMemoryAndActivation()
    {
        var b = Held(Initial());
        var dirty = b.GetActor(Returner).With(mp: 0, attack: 99, status: 7, activationWord: 0xA3B2,
            aiMemory: 9, lastTarget: Leader, primaryOrder: 9, secondaryOrder: 10,
            sourceLoadout: new([0, 0, 0, 0], [1, 1, 1, 1]));
        b = b.With(actors: b.Actors.Reverse().Select(a => a.Actor == Returner ? dirty : a), deathCredit: new(null));
        var batch = BattleSpawnRules.Populate(b);
        Assert.Equal(new[] { Returner, Hidden }, batch.Appearances);
        var reset = batch.Battle.GetActor(Returner);
        Assert.True(reset.Initialized); Assert.Equal(reset.Definition.MaxHp, reset.Hp); Assert.Equal(reset.Definition.MaxMp, reset.Mp);
        Assert.Equal(reset.Deployment.Position, reset.Position); Assert.Equal(0, reset.Status);
        Assert.Equal(BattleInitializationRules.EnemyAttack(reset.Definition.Attack, 0), reset.Attack);
        Assert.Same(reset.Definition.SourceLoadout, reset.SourceLoadout);
        Assert.Equal((byte)255, reset.PrimaryOrder); Assert.Equal((byte)255, reset.SecondaryOrder);
        Assert.Equal((ushort)0xA3B3, reset.ActivationWord); Assert.Equal((byte)9, reset.AiMemory); Assert.Equal(Leader, reset.LastTarget);
        Assert.Same(b.DeathCredit, batch.Battle.DeathCredit); Assert.Same(b.ItemCustody, batch.Battle.ItemCustody);
        Assert.Equal(b.MainSeed, batch.Battle.MainSeed); Assert.Same(b.Regions, batch.Battle.Regions);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void FalseRegionsNeverSpawnButHeldRegionsDoNotNeedALivingEnemy(bool held)
    {
        var b = Initial(); b = b.With(actors: b.Actors.Select(a => a.Actor == Sentry ? a.With(hp: 0, clearPosition: true) :
            a.Actor == Leader ? a.With(position: new(2, 3)) : a));
        if (held) b = Held(b);
        var activated = BattleActivationRules.BeforeRound(b);
        Assert.Equal(held, activated.Regions!.Flags[0]);
        Assert.Equal(held ? 2 : 0, BattleSpawnRules.Populate(activated).Appearances.Count);
    }

    [Theory]
    [InlineData(0, 15, true, false, 1)]
    [InlineData(15, 0, true, false, 3)]
    [InlineData(0, 1, true, true, 1)]
    [InlineData(0, 1, false, true, 3)]
    [InlineData(15, 15, true, true, 0)]
    public void PrimaryWinsAndNoneSentinelNeverActivates(byte primary, byte secondary, bool first, bool second, ushort expected)
    {
        var flags = new bool[16]; flags[0] = first; flags[1] = second; flags[15] = true;
        Assert.Equal((ushort)(0xA300 | expected), BattleActivationRules.ActivateAssignedRegions(0xA300, primary, secondary, flags));
    }

    [Fact]
    public void CheckedWordSurvivesRoundsAndOrdinarySourceAiClearsIt()
    {
        var b = BattleActivationRules.BeforeRound(Initial()); Assert.False(b.Regions!.Flags[0]); Assert.Equal(1, b.Regions.Tested);
        b = b.With(actors: b.Actors.Select(a => a.Actor == Leader ? a.With(position: new(2, 3)) : a));
        Assert.False(BattleActivationRules.BeforeRound(b).Regions!.Flags[0]);
        var ai = SourceEnemyAi.Resolve(b, Sentry);
        Assert.Equal(0, ai.Battle.Regions!.Tested);
        Assert.True(BattleActivationRules.BeforeRound(ai.Battle).Regions!.Flags[0]);
        var movedOut = Held(b).With(actors: b.Actors.Select(a => a.Actor == Leader ? a.With(position: new(0, 7)) : a), regions: Held(b, 0).Regions);
        Assert.True(BattleActivationRules.BeforeRound(movedOut).Regions!.Flags[0]);
    }

    [Theory]
    [InlineData("living")] [InlineData("dead")] [InlineData("self")]
    public void AllRetainedCoordinatesRefuseResetThenAbsentPlacementAllowsRetry(string occupant)
    {
        var b = Held(Initial()); var spot = b.GetActor(Returner).Deployment.Position;
        b = b.With(actors: b.Actors.Select(a => a.Actor == (occupant == "self" ? Returner : Sentry)
            ? a.With(position: spot, hp: occupant == "living" ? (ushort)60 : (ushort)0, aiMemory: 8, lastTarget: Leader) : a));
        var refused = BattleSpawnRules.Populate(b); var enemy = refused.Battle.GetActor(Returner);
        Assert.DoesNotContain(Returner, refused.Appearances); Assert.False(enemy.Initialized); Assert.Equal(0, enemy.Hp); Assert.Null(enemy.Position);
        Assert.Equal((ushort)769, enemy.ActivationWord); Assert.True(refused.Battle.Regions!.Flags[0]);
        if (occupant == "self") { Assert.Equal(8, enemy.AiMemory); Assert.Equal(Leader, enemy.LastTarget); }
        var freed = refused.Battle.With(actors: refused.Battle.Actors.Select(a => a.Actor == Sentry ? a.With(clearPosition: true) : a));
        Assert.Contains(Returner, BattleSpawnRules.Populate(freed).Appearances);
    }

    [Fact]
    public void ModeTwoOrdinaryDeathStaysLoadedWhileModeThreeReturnsAtNextScan()
    {
        var b = BattleSpawnRules.Populate(Held(Initial())).Battle;
        b = b.With(actors: b.Actors.Select(a => a.Actor == Hidden || a.Actor == Returner ? a.With(hp: 0, clearPosition: true) : a));
        Assert.True(b.GetActor(Hidden).Initialized); Assert.Equal(1, b.GetActor(Hidden).MaxHp);
        var next = BattleSpawnRules.Populate(b);
        Assert.Equal(new[] { Returner }, next.Appearances); Assert.Equal(0, next.Battle.GetActor(Hidden).Hp);
    }

    [Fact]
    public void DeclaredBoundRefusalAndSourceSlotChangesAreIndependentOfIdsAndProcessingOrder()
    {
        var doc = Document("hidden-spawns");
        doc["encounters"]![0]!["initialization"]!["enemyCount"] = 1;
        var accepted = Assert.IsType<ScenarioReadAccepted>(Reader(doc).Read());
        var b = BattleTurnFlow.Start(accepted.Definition.Encounters[accepted.Start.Encounter], accepted.Start);
        var batch = BattleSpawnRules.Populate(Held(b)); Assert.Equal(new[] { Returner }, batch.Appearances);
        Assert.False(batch.Battle.GetActor(Hidden).Initialized);
        doc = Document("hidden-spawns");
        doc["encounters"]![0]!["initialization"]!["sourceCombatants"]![1]!["slot"] = 129;
        doc["encounters"]![0]!["initialization"]!["sourceCombatants"]![2]!["slot"] = 128;
        doc["actors"]![1]!["id"] = "changed"; doc["start"]!["actors"]![1]!["actor"] = "changed";
        doc["encounters"]![0]!["placements"]![1]!["actor"] = "changed";
        doc["encounters"]![0]!["placements"]![1]!["processingOrder"] = 15;
        doc["encounters"]![0]!["placements"]![1]!["x"] = 5;
        doc["encounters"]![0]!["initialization"]!["sourceCombatants"]![1]!["actor"] = "changed";
        accepted = Assert.IsType<ScenarioReadAccepted>(Reader(doc).Read());
        b = BattleTurnFlow.Start(accepted.Definition.Encounters[accepted.Start.Encounter], accepted.Start);
        batch = BattleSpawnRules.Populate(Held(b));
        Assert.Equal(new[] { Hidden, new ActorRef("changed") }, batch.Appearances);
        Assert.Equal(new MapPosition(5, 2), batch.Battle.GetActor(new("changed")).Position);
    }

    [Fact]
    public void RegionGeometryIgnoresSignedHighAllyHpButEnemyCallerRequiresOnlyNonzeroHp()
    {
        var b = Initial();
        b = b.With(actors: b.Actors.Select(a => a.Actor == Leader ? a.With(hp: 0x8000, position: new(2, 3)) : a));
        Assert.False(BattleActivationRules.BeforeRound(b).Regions!.Flags[0]);
        b = b.With(actors: b.Actors.Select(a => a.Actor == Leader ? a.With(hp: 1) : a.Actor == Sentry ? a.With(hp: 0x8000) : a));
        Assert.True(BattleActivationRules.BeforeRound(b).Regions!.Flags[0]);
    }

    [Fact]
    public void EarlierSlotResetBlocksLaterSharedStartAndLoadedDeadModeTwoDoesNotReset()
    {
        var d = Document("hidden-spawns");
        d["encounters"]![0]!["placements"]![2]!["x"] = 2;
        d["encounters"]![0]!["placements"]![2]!["y"] = 2;
        var input = Assert.IsType<ScenarioReadAccepted>(Reader(d).Read());
        var b = Held(BattleTurnFlow.Start(input.Definition.Encounters[input.Start.Encounter], input.Start));
        var result = BattleSpawnRules.Populate(b);
        Assert.Equal(new[] { Returner }, result.Appearances);
        Assert.Contains(result.Effects, e => e.Kind == "spawn-refused" && e.Actor == Hidden);
        d = Document("hidden-spawns"); d["start"]!["actors"]![2]!["initialization"]!["initialized"] = true;
        input = Assert.IsType<ScenarioReadAccepted>(Reader(d).Read());
        b = Held(BattleTurnFlow.Start(input.Definition.Encounters[input.Start.Encounter], input.Start));
        result = BattleSpawnRules.Populate(b);
        Assert.DoesNotContain(Hidden, result.Appearances);
        Assert.Equal((ushort)513, result.Battle.GetActor(Hidden).ActivationWord);
        Assert.Equal(1, result.Battle.GetActor(Hidden).MaxHp);
    }

    [Fact]
    public void SourceIntegerProjectionKeepsTruncationNegativeAndDegenerateBranches()
    {
        Assert.Equal(1, SourceRegionGeometry.Segment(new(0, 0), new(4, 2), new(1, 1)));
        Assert.Equal(2, SourceRegionGeometry.Segment(new(4, 2), new(0, 0), new(1, 1)));
        Assert.Equal(1, SourceRegionGeometry.Segment(new(0, 0), new(2, 4), new(1, 1)));
        Assert.Equal(2, SourceRegionGeometry.Segment(new(2, 0), new(4, 2), new(0, 0)));
        Assert.Equal(3, SourceRegionGeometry.Segment(new(2, 2), new(2, 2), new(1, 1)));
        Assert.Equal(2, SourceRegionGeometry.Segment(new(1, 1), new(3, 1), new(4, 1)));
        Assert.True(SourceRegionGeometry.Includes([new(0, 0), new(4, 2), new(4, 4)], new(1, 1)));
        Assert.True(SourceRegionGeometry.Includes([new(1, 1), new(4, 1), new(4, 3), new(1, 3)], new(1, 3)));
        Assert.False(SourceRegionGeometry.Includes([new(1, 1), new(4, 1), new(4, 3), new(1, 3)], new(5, 3)));
        Assert.False(SourceRegionGeometry.Includes([new(2, 2), new(2, 2), new(2, 2)], new(2, 2)));
    }

    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void ContentCannotMakeLivingAlliesOrStartingEnemiesDormant(bool allStarting, bool enemy)
    {
        var d = Document("hidden-spawns");
        if (allStarting)
            for (int i = 1; i <= 2; i++)
            {
                d["encounters"]![0]!["placements"]![i]!["initialization"]!["spawnMode"] = 0;
                d["start"]!["actors"]![i]!["initialization"]!["initialized"] = true;
                d["start"]!["actors"]![i]!["initialization"]!["activationWord"] = 0;
            }
        d["start"]!["actors"]![enemy ? 3 : 0]!["initialization"]!["initialized"] = false;
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(d)));
        Assert.Equal("dormant-start-state", failed.Failure.Code);
    }

    [Theory]
    [InlineData("missing")] [InlineData("mode")]
    public void ControlledInitializationCannotOmitStateOrContradictMode(string shape)
    {
        var d = Document("hidden-spawns");
        if (shape == "missing") d["start"]!["actors"]![0]!.AsObject().Remove("initialization");
        else d["start"]!["actors"]![3]!["initialization"]!["activationWord"] = 768;
        Assert.Equal("controlled-initialization", Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(d))).Failure.Code);
    }

    [Theory]
    [InlineData("mode")] [InlineData("program")] [InlineData("dormant")] [InlineData("slots")]
    public void UnsupportedOrContradictoryContentRejectsBeforeSession(string shape)
    {
        var d = Document("hidden-spawns"); var encounter = d["encounters"]![0]!;
        if (shape == "mode") encounter["placements"]![1]!["initialization"]!["spawnMode"] = 1;
        if (shape == "program") encounter["initialization"]!["regionProgram"] = "jaro";
        if (shape == "dormant") d["start"]!["actors"]![1]!["hp"] = 1;
        if (shape == "slots") encounter["initialization"]!["sourceCombatants"]![1]!["slot"] = 7;
        Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(d)));
    }
}
