using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleSceneAllyAppearanceTests
{
    [Fact]
    public void CurrentActorClassAndWeaponSelectAppearanceWithoutFixingTheRecruitLoadout()
    {
        var defaults = Visual("default"); var red = Visual("red"); var spear = Visual("red-spear");
        ActorRef actor = new("traveller");
        var scene = new BattleSceneDefinition("test", "", "", new Dictionary<string, ExplorationRaster>(),
            new Dictionary<BattleClassRule, BattleSceneActorVisual>
                { [BattleClassRule.UnpromotedKnight] = defaults, [BattleClassRule.UnpromotedMage] = defaults },
            new Dictionary<ActorRef, BattleSceneActorVisual>(), new Dictionary<int, string>(), [],
            AllyAppearances: new Dictionary<(ActorRef, BattleClassRule, int), BattleSceneActorVisual>
            {
                [(actor, BattleClassRule.UnpromotedKnight, -1)] = red,
                [(actor, BattleClassRule.UnpromotedKnight, 5)] = spear,
            });
        Assert.True(scene.TryAllyVisual(Actor(actor, BattleClassRule.UnpromotedKnight, 127), out var unarmed));
        Assert.Same(red, unarmed);
        Assert.True(scene.TryAllyVisual(Actor(actor, BattleClassRule.UnpromotedKnight, 128 | 5), out var armed));
        Assert.Same(spear, armed);
        Assert.False(scene.TryAllyVisual(Actor(actor, BattleClassRule.UnpromotedKnight, 128 | 6), out _));
        Assert.True(scene.TryAllyVisual(Actor(actor, BattleClassRule.UnpromotedMage, 127), out var changedClass));
        Assert.Same(defaults, changedClass);
        Assert.True(scene.TryAllyVisual(Actor(new("other"), BattleClassRule.UnpromotedKnight, 127), out var other));
        Assert.Same(defaults, other);
        Assert.True((scene with { AllyAppearances = null }).TryAllyVisual(Actor(actor, BattleClassRule.UnpromotedKnight, 127), out var legacy));
        Assert.Same(defaults, legacy);
    }

    private static BattleSceneActorVisual Visual(string resource) => new(2, 1, null, [resource], [], new Dictionary<string, BattleSceneAnimation>());
    private static BattleActorState Actor(ActorRef id, BattleClassRule kind, ushort item)
    {
        var definition = new BattleActorDefinition(id, kind, 1, 20, 5, 8, 4, 6, false, 6, [],
            sourceLoadout: new([item, 127, 127, 127], []));
        var deployment = new BattleDeploymentDefinition(definition, BattleFaction.Ally, 0, BattleControl.Player, null, new MapPosition(1, 1));
        return new(deployment, 20, 5, 0, new(1, 1), 0, 0, sourceLoadout: definition.SourceLoadout);
    }
}
