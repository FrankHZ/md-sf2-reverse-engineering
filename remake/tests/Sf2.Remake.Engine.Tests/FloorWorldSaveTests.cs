using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class FloorWorldSaveTests
{
    [Theory]
    [InlineData(26, 14, 3, false)]
    [InlineData(14, 24, 2, true)]
    [InlineData(59, 4, 1, true)]
    public void CompletedLandingRestoresPositionAndIndependentRingTail(int x, int y, byte facing, bool ring)
    {
        var (definition, data) = OpeningRestoreTests.Fixture();
        var flags = data.Flags.Concat(ring ? new[] { 779, 780 } : new[] { 779 }).ToArray();
        var slot = data with { Recipe = "floor-world", Map = "map-26", X = x, Y = y, Facing = facing,
            Flags = flags, EgressMap = "map-26" };
        var session = Assert.IsType<SessionStarted>(GameSession.Restore(definition, slot, RuleCompositions.Sf2())).Session;
        var world = session.Current.Exploration!;
        Assert.Equal(new MapPosition(x, y), world.PlayerEntity.Position);
        Assert.Equal(facing, world.PlayerEntity.Motion.Facing);
        Assert.Equal(new MapId("map-26"), session.Current.Story.EgressMap);
        Assert.DoesNotContain(999, session.Current.Story.Flags); // No replay of first-arrival init.
        Assert.True(world.TryResolveEntity(new("entity-129"), out var worker));
        Assert.Equal(ring ? 0x7000 : 5 * 384, worker.Motion.X);
        Assert.Equal(ring ? 0x7000 : 24 * 384, worker.Motion.Y);
        Assert.Equal(data.Gold, world.Party.Gold);
        var policy = definition.Persistence!;
        var anchor = new FieldReturnAnchor(new("map-26"), new(x, y), facing);
        Assert.Equal("floor-world", policy.Select(definition, anchor, flags, null).Recipe);
        Assert.Equal("save-location-unavailable", Assert.Throws<BattleRuleException>(() =>
            policy.Select(definition, anchor, flags.Where(f => f != 779).ToArray(), null)).Code);
    }
}
