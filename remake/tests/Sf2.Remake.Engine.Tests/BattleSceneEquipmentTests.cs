using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleSceneEquipmentTests
{
    [Fact]
    public void SceneUsesTheCurrentlyEquippedItemAndKeepsMissingBindingsUnavailable()
    {
        var actor = Start().Current.Battle.GetActor(new("medic-a"));
        var kind = actor.Definition.ClassRule;
        var oldWeapon = new BattleSceneActorVisual(1, 0, 85, ["actor"], ["old-weapon"], new Dictionary<string, BattleSceneAnimation>());
        var purchased = oldWeapon with { Item = 86, WeaponFrames = ["purchased-weapon"] };
        var content = new BattleSceneDefinition("encounter", "background", "ground",
            new Dictionary<string, ExplorationRaster>(), new Dictionary<BattleClassRule, BattleSceneActorVisual> { [kind] = oldWeapon },
            new Dictionary<ActorRef, BattleSceneActorVisual>(), new Dictionary<int, string>(), [],
            Equipment: new Dictionary<(BattleClassRule, int), BattleSceneActorVisual> { [(kind, 85)] = oldWeapon, [(kind, 86)] = purchased });
        BattleActorState Equipped(ushort item) => actor.With(sourceLoadout: new([item, 127, 127, 127], [63, 63, 63, 63]));
        Assert.True(content.TryAllyVisual(Equipped(213), out var first));
        Assert.Same(oldWeapon, first);
        Assert.True(content.TryAllyVisual(Equipped(214), out var changed));
        Assert.Same(purchased, changed);
        Assert.False(content.TryAllyVisual(Equipped(215), out _));
        Assert.True(content.TryAllyVisual(Equipped(86), out var unarmed));
        Assert.Same(oldWeapon, unarmed);
    }
}
