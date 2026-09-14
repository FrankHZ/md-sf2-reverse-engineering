using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class EntityMotionTests
{
    [Theory]
    [InlineData(96, 480, 768, 4)]
    [InlineData(200, 584, 768, 2)]
    public void MovementInstallsVelocityThenIntegratesAndSnapsWithoutOvershoot(int speed, int firstX, int finalX, int ticks)
    {
        var initial = EntityMotionState.At(new(1, 1), 1, (ushort)speed);
        var moving = EntityMotion.Start(initial, 768, 384, [])!;
        Assert.Equal(384, moving.X);
        Assert.Equal(384, moving.XTravel);
        var first = EntityMotion.Tick(moving, 0x2000);
        Assert.Equal(firstX, first.X);
        Assert.Equal(0, first.Facing);
        var last = first;
        for (int tick = 1; tick < ticks; tick++) last = EntityMotion.Tick(last, 0x2000);
        Assert.Equal(finalX, last.X);
        Assert.Equal(0, last.XTravel);
        Assert.Equal(2, last.Layer);
        Assert.Equal(384, initial.X);
    }

    [Fact]
    public void ObstructionUsesOtherDestinationsAndRetainsTheOriginalMotion()
    {
        var initial = EntityMotionState.At(new(1, 1), 0, 96) with { FlagsA = 0x20 };
        var blocking = EntityMotionState.At(new(4, 1), 0, 96) with { XDestination = 768, YDestination = 384 };
        Assert.Null(EntityMotion.Start(initial, 768, 384, [blocking]));
        Assert.NotNull(EntityMotion.Start(initial, 768, 768, [blocking]));
        Assert.Equal(384, initial.XDestination);
    }

    [Fact]
    public void DecelerationCannotSetVelocityToZeroAndImmersedClearsOnOrdinaryTerrain()
    {
        var initial = EntityMotionState.At(new(1, 1), 0, 16) with
        {
            XDestination = 480, XTravel = 384, XVelocity = 16,
            XAcceleration = 16, FlagsA = 4, FlagsB = 0x60, AnimationCounter = 255,
        };
        var decelerating = EntityMotion.Tick(initial with { X = 400 }, 0);
        Assert.Equal(16, decelerating.XVelocity);
        Assert.Equal(255, decelerating.AnimationCounter);
        var final = EntityMotion.Tick(decelerating with { X = 472 }, 0);
        Assert.Equal(480, final.X);
        Assert.Equal(0, final.FlagsB & 0x20);
    }

    [Fact]
    public void CoreMovementAndDestinationAdmissionMatchTheThirteenOwnedH3Cases()
    {
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "fixtures", "entity-motion.json")));
        foreach (var row in document.RootElement.GetProperty("cases").EnumerateArray())
        {
            var source = row.GetProperty("entity");
            int N(string key) => source.GetProperty(key).GetInt32();
            var state = new EntityMotionState((short)N("x"), (short)N("y"), (short)N("xDest"), (short)N("yDest"),
                (short)N("xVelocity"), (short)N("yVelocity"), (ushort)N("xTravel"), (ushort)N("yTravel"),
                (ushort)N("xSpeed"), (ushort)N("ySpeed"), (byte)N("xAccel"), (byte)N("yAccel"),
                (byte)N("flagsA"), (byte)N("flagsB"), (byte)N("facing"), (byte)N("layer"), (byte)N("animCounter"), (byte)N("waitTimer"));
            var script = row.GetProperty("script");
            string kind = script.GetProperty("kind").GetString()!;
            var tile = row.GetProperty("arrivalTileWord");
            ushort? word = tile.ValueKind == System.Text.Json.JsonValueKind.Null ? null : tile.GetUInt16();
            int tick = 0;
            foreach (var expected in row.GetProperty("expected").GetProperty("states").EnumerateArray())
            {
                state = EntityMotion.Tick(state, word);
                if (tick == 0 && kind is "relative" or "absolute")
                {
                    int x = script.GetProperty("x").GetInt32() * 384 + (kind == "relative" ? state.X : 0);
                    int y = script.GetProperty("y").GetInt32() * 384 + (kind == "relative" ? state.Y : 0);
                    var blocker = row.GetProperty("blocker");
                    EntityMotionState[] others = blocker.ValueKind == System.Text.Json.JsonValueKind.Null ? [] :
                        [state with { XDestination = blocker.GetProperty("xDest").GetInt16(), YDestination = blocker.GetProperty("yDest").GetInt16() }];
                    state = EntityMotion.Start(state, (short)x, (short)y, others) ?? state;
                }
                // The H3 record also owns script wait/PC fields; this comparison claims only
                // the extracted core fields and destination admission, not the full EAS engine.
                int[] actual = [++tick, state.X, state.Y, state.XVelocity, state.YVelocity, state.XTravel, state.YTravel,
                    state.XDestination, state.YDestination, state.Facing, state.Layer, state.FlagsB, state.AnimationCounter];
                Assert.True(actual.SequenceEqual(expected.EnumerateArray().Take(actual.Length).Select(value => value.GetInt32())),
                    $"{row.GetProperty("id").GetString()} core tick {tick}");
            }
        }
    }
}
