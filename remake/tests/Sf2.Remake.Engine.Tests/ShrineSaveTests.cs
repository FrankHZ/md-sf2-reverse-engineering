using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class ShrineSaveTests
{
    [Theory]
    [InlineData("map-6", false, true)]
    [InlineData("map-9", true, true)]
    [InlineData("map-13", true, true)]
    [InlineData("map-9", false, false)]
    [InlineData("map-50", true, false)]
    public void EastShrineContinuePreservesThePostDesertEgressRegister(string map, bool desertCleared, bool valid)
    {
        var (basis, prior) = OpeningRestoreTests.Fixture();
        var data = prior with { Recipe = "east-shrine", EgressMap = map, Flags = desertCleared ? [514] : [] };
        if (valid) basis.Persistence!.ValidateRegisters(data);
        else Assert.Equal("save-egress-map",
            Assert.Throws<BattleRuleException>(() => basis.Persistence!.ValidateRegisters(data)).Code);
    }

    [Theory]
    [InlineData(14, 3, true, true, true)]
    [InlineData(27, 11, true, true, true)]
    [InlineData(42, 23, true, true, false)]
    [InlineData(32, 11, true, true, false)]
    [InlineData(14, 3, true, false, false)]
    [InlineData(14, 3, false, true, false)]
    public void HallSaveUsesTheDepartedOldMansLandingOnlyOnTheShrineSide(
        int x, int y, bool victory, bool departure, bool admitted)
    {
        var (basis, _) = OpeningRestoreTests.Fixture();
        var source = basis.Exploration!.Maps[new("map-57")];
        var hall = new ExplorationMapDefinition(new("map-50"), source.Layout,
            new([new(0, 0, 31, 23), new(33, 0, 54, 24)]), [], []);
        var definition = new ScenarioDefinition(basis.Package, basis.Encounters.Values,
            exploration: new([hall], []));
        var player = new FieldReturnAnchor(hall.Map, new(x, y), 3);
        var flags = new List<int>();
        if (victory) flags.Add(515);
        if (departure) flags.Add(732);
        var policy = basis.Persistence!;
        if (admitted)
        {
            var location = policy.Select(definition, player, flags, null);
            Assert.Equal("ancient-hall", location.Recipe);
            Assert.Equal(new FieldReturnAnchor(hall.Map, new(27, 11), 1), location.Anchor);
        }
        else Assert.Equal("save-location-unavailable",
            Assert.Throws<BattleRuleException>(() => policy.Select(definition, player, flags, null)).Code);
    }
}
