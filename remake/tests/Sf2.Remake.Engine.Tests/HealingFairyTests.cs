using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Application.Runtime.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class HealingFairyTests
{
    private static readonly ActorRef Actor = new("healer");

    [Fact]
    public void TimedInputTestsAcknowledgementBeforeItsNextFairyOpportunity()
    {
        var (cursor, seed) = Cursor();
        cursor = cursor.Enter(BattleScenePhase.ResultMessage);
        List<BattleEffect> effects = [];
        while (!cursor.AtTimedInput) cursor = cursor.Advance(seed, Actor, 1, effects, out seed);
        Assert.Equal(65, cursor.Remaining); // bsc10 speed2: bit6 then inclusive DBF.
        var fairy = cursor.Fairy;
        var accepted = cursor.Advance(seed, Actor, 1, [], out uint acknowledgedSeed, acknowledge: true);
        Assert.True(accepted.LogicalComplete);
        Assert.True(accepted.Delivered);
        Assert.Same(fairy, accepted.Fairy);
        Assert.Equal(seed, acknowledgedSeed);
        var waited = cursor.Advance(seed, Actor, 1, [], out _);
        Assert.Equal(64, waited.Remaining);
        Assert.NotEqual(fairy, waited.Fairy);
    }

    [Fact]
    public void DeliveryBeforeOrAfterMandatoryIdleDoesNotChangeTheLogicalWork()
    {
        var (cursor, seed) = Cursor();
        cursor = (cursor with { TargetIdleTicks = 13 }).Enter(BattleScenePhase.MakeIdle);
        var early = cursor with { Delivered = true };
        var late = cursor;
        uint earlySeed = seed, lateSeed = seed;
        for (int opportunity = 0; opportunity < 13; opportunity++)
        {
            early = early.Advance(earlySeed, Actor, 1, [], out earlySeed);
            late = late.Advance(lateSeed, Actor, 1, [], out lateSeed);
        }
        Assert.True(early.LogicalComplete); Assert.True(late.LogicalComplete);
        Assert.Equal(earlySeed, lateSeed);
        Assert.Equal(early.Fairy!.Fairies, late.Fairy!.Fairies);
        Assert.Same(early, early.Advance(earlySeed, Actor, 1, [], out uint afterDelivery));
        Assert.Equal(earlySeed, afterDelivery);
    }

    [Fact]
    public void SceneStopResumesClearedCleanupThenRestorationBeforeCompletion()
    {
        var (cursor, seed) = Cursor();
        cursor = cursor.Enter(BattleScenePhase.SpellStop).Normalize(seed, Actor, 1, [], out seed);
        Assert.Equal(2, cursor.Fairy!.Control);
        cursor = cursor.Advance(seed, Actor, 1, [], out seed); // waiting phase retires without another draw.
        Assert.Equal("ReinitializeSceneAfterSpell:wait", cursor.Caller);
        Assert.Equal(0, cursor.Fairy!.Control);
        Assert.True(cursor.Fairy.CleanupPending);
        Assert.False(cursor.LogicalComplete);
        cursor = cursor.Advance(seed, Actor, 1, [], out seed);
        Assert.Equal("bsc0D:restore-wait", cursor.Caller);
        Assert.False(cursor.Fairy!.CleanupPending);
        cursor = cursor.Advance(seed, Actor, 1, [], out _);
        Assert.True(cursor.LogicalComplete);
        Assert.False(cursor.Delivered);
    }

    private static (HealingSceneCursor Cursor, uint Seed) Cursor()
    {
        var battle = EngineTestContent.Start().Current.Battle;
        var actor = new ActorRef("medic-a");
        var action = PlayerHealing.Prepare(battle, actor, battle.GetActor(actor).Position!, new("mend", 1), actor);
        var setup = HealingFairy.Begin(1, action.Prepared.MainSeed, actor);
        return (HealingSceneCursor.Create(action, null) with { Fairy = setup.State }, setup.Seed);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    public void SetupAllocatesInstancesAndDrawsEachInstancesOperandsInOrder(int level, int instances)
    {
        var setup = HealingFairy.Begin(level, 0x1234ABCD, Actor);
        Assert.Equal(instances, setup.State.ActiveCount);
        Assert.Equal(25 - 2 * instances, setup.State.Dust.Count);
        Assert.Equal(Enumerable.Repeat(new ushort[] { 32, 30, 12 }, instances).SelectMany(x => x),
            setup.Effects.Select(effect => effect.RandomRange!.Value));
        Assert.Equal(65535, setup.State.Lifetime);
        Assert.Equal(1, setup.State.Control);
        uint seed = 0x1234ABCD;
        foreach (var effect in setup.Effects)
        {
            Assert.Equal((long)seed, effect.Before);
            seed = checked((uint)effect.After!.Value);
        }
        Assert.Equal(seed, setup.Seed);
        Assert.All(setup.State.Fairies, fairy => {
            Assert.Equal((ushort)1, fairy.Age); Assert.Equal((ushort)7, fairy.Phase);
            Assert.InRange(fairy.Y, (ushort)128, (ushort)159);
            Assert.InRange(fairy.XFraction, (short)1, (short)30);
            Assert.InRange(fairy.DustClock, (ushort)1, (ushort)12);
        });
    }

    [Fact]
    public void WaitingAdvancesWithoutDrawingUntilItsOwnDelayExpires()
    {
        var state = State(new(1, 7, 0, 0, 2, 0, 0, 0, 4, 384, 140));
        var waiting = HealingFairy.Advance(state, 0x1234ABCD, Actor);
        Assert.Empty(waiting.Effects);
        Assert.Equal(0x1234ABCDu, waiting.Seed);
        Assert.Equal(65534, waiting.State.Lifetime);
        Assert.Equal(1, waiting.State.Fairies[0].XFraction);
        var reentry = HealingFairy.Advance(waiting.State, waiting.Seed, Actor);
        Assert.Equal((ushort)16, Assert.Single(reentry.Effects).RandomRange);
        Assert.Equal((ushort)0, reentry.State.Fairies[0].Phase);
        Assert.Equal((ushort)2, reentry.State.Fairies[0].Age);
        Assert.Equal((ushort)240, reentry.State.Fairies[0].Speed);
    }

    [Fact]
    public void BoundaryAndDustConditionsCanBothDrawInOneUpdate()
    {
        var state = State(new(10, 2, 0, 0, 0, 0, 0, 0, 1, 95, 140));
        var update = HealingFairy.Advance(state, 0x1234ABCD, Actor);
        Assert.Equal(new ushort[] { 28, 32, 12 }, update.Effects.Select(effect => effect.RandomRange!.Value));
        Assert.Equal((ushort)3, update.State.Fairies[0].Phase);
        Assert.True(update.State.Fairies[0].Mirrored);
        Assert.Equal((ushort)96, update.State.Fairies[0].X);
        var dust = Assert.Single(update.State.Dust, particle => particle.Age != 0);
        Assert.Equal((ushort)108, dust.X);
        Assert.Equal(update.State.Fairies[0].Y + 12, dust.Y);
        Assert.Equal((ushort)0, update.State.PendingDustY);
    }

    [Fact]
    public void StopPreservesFlyingInstancesThenClearsBeforeCleanupWait()
    {
        var state = State(new(10, 2, 0, 0, 0, 0, 0, 0, 20, 100, 140));
        var stopped = HealingFairy.Advance(HealingFairy.RequestStop(state), 0x1234ABCD, Actor);
        Assert.Equal((ushort)0, stopped.State.Lifetime);
        Assert.Equal(1, stopped.State.ActiveCount);
        Assert.False(stopped.State.CleanupPending);
        var exitState = stopped.State with { Fairies = [stopped.State.Fairies[0] with { Phase = 3 }] };
        var cleanup = HealingFairy.Advance(exitState, stopped.Seed, Actor);
        Assert.Empty(cleanup.Effects);
        Assert.Equal(stopped.Seed, cleanup.Seed);
        Assert.Equal(0, cleanup.State.ActiveCount);
        Assert.Equal(0, cleanup.State.Control);
        Assert.True(cleanup.State.CleanupPending);
        Assert.Same(cleanup.State, HealingFairy.Advance(cleanup.State, cleanup.Seed, Actor).State);
        Assert.False(HealingFairy.FinishCleanup(cleanup.State).CleanupPending);
        Assert.Equal((ushort)20, state.Fairies[0].DustClock);
    }

    [Fact]
    public void WordLifetimeAndSignedMotionRetainSourceArithmetic()
    {
        var state = State(new(10, 0, 4090, 240, -250, -250, 3, 0, 20, 200, 140)) with { Lifetime = 1 };
        var update = HealingFairy.Advance(state, 0x1234ABCD, Actor);
        var fairy = update.State.Fairies[0];
        Assert.Equal((ushort)0, update.State.Lifetime);
        Assert.Equal((ushort)0, fairy.Angle);
        Assert.Equal((ushort)260, fairy.Speed);
        Assert.Equal((short)10, fairy.XFraction);
        Assert.Equal((short)-250, fairy.YFraction);
        Assert.Equal((ushort)200, fairy.X);
        Assert.Equal((ushort)140, fairy.Y);
        Assert.Equal((byte)0, fairy.WingClock);
        Assert.Equal((byte)1, fairy.WingFrame);
        Assert.Empty(update.Effects);
    }

    [Theory]
    [InlineData(0, 256, 0)]
    [InlineData(64, 0, -256)]
    [InlineData(128, -256, 0)]
    [InlineData(192, 0, 256)]
    public void IntegerDirectionHasTheSourceAxisOrientation(int angle, int x, int y) =>
        Assert.Equal((x, y), HealingFairy.Direction(angle));

    private static HealingFairyState State(HealingFairyInstance fairy) =>
        new(65535, 1, [fairy], Enumerable.Repeat(new HealingDust(0, 0, 0, 0, 0), 23).ToArray());
}
