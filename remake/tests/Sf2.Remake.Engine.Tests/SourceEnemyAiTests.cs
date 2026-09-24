using System.Text.Json;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class SourceEnemyAiTests
{
    private static readonly ActorRef Enemy = new("varied-source-enemy");
    private static EngineBattleState State(BattleMover mover = BattleMover.Hovering, bool active = false,
        byte memory = 0, uint thinking = 0x12340042, byte commandset = 6, MapPosition? allyPosition = null,
        bool blockSouth = true, bool peer = true, byte primaryRegion = 6)
    {
        var terrain = Enumerable.Repeat(new BattleTerrain(TerrainSurface.Open, TerrainProtection.None), 2304).ToArray();
        if (blockSouth) terrain[6 * 48 + 5] = new(TerrainSurface.Barrier, TerrainProtection.None);
        terrain[5 * 48 + 4] = new(TerrainSurface.Rough, TerrainProtection.Heavy);
        var empty = new BattleSourceLoadout([127, 127, 127, 127], [63, 63, 63, 63]);
        var enemyDefinition = new BattleActorDefinition(Enemy, BattleClassRule.Ordinary, 2, 16, 0, 11, 8, 5, false, 2, [], mover: mover, sourceLoadout: empty);
        var allyDefinition = new BattleActorDefinition(new("other-party"), BattleClassRule.UnpromotedKnight, 1, 20, 0, 12, 9, 8, false, 7, [], mover: BattleMover.Centaur, sourceLoadout: empty);
        var enemy = new BattleDeploymentDefinition(enemyDefinition, BattleFaction.Enemy, 190, BattleControl.Automatic, BattleAiStrategy.SourceOrders,
            new(5, 5), new(0x2000, 0, primaryRegion, 15, (byte)(commandset << 4), commandset, 255, 255));
        var ally = new BattleDeploymentDefinition(allyDefinition, BattleFaction.Ally, 22, BattleControl.Player, null, allyPosition ?? new(16, 16));
        List<BattleDeploymentDefinition> deployments = [enemy, ally];
        List<BattleActorState> actors = [new(enemy, 16, 0, null, new(5, 5), null, null, new("remembered-target"),
            activationWord: (ushort)(0x2000 | commandset << 4 | (active ? 1 : 0)), aiMemory: memory),
            new(ally, 20, 0, null, ally.Position, null, null, activationWord: 0)];
        if (peer)
        {
            var definition = new BattleActorDefinition(new("source-peer"), BattleClassRule.Ordinary, 2, 16, 0, 11, 8, 5, false, 2, [], mover: mover, sourceLoadout: empty);
            var deployment = enemy with { Definition = definition, Position = new(6, 5), ProcessingOrder = 191 };
            deployments.Add(deployment); actors.Add(new(deployment, 16, 0, null, deployment.Position, null, null, activationWord: 0x2060));
        }
        var battle = new BattleDefinition("another-encounter", new("other-map"), 20, 20, terrain, deployments, []);
        var flags = new bool[16]; flags[6] = active;
        return new(battle, actors, 0xCAFE5678, thinking, 17, [], 0, null, regions: new(flags, 7));
    }

    [Fact]
    public void InactiveMovementUsesItsSourceAnchorMemoryAndIndependentThinkingChannel()
    {
        var before = State(); string frozen = JsonSerializer.Serialize(before);
        var decision = SourceEnemyAi.Resolve(before, Enemy); var actor = decision.Battle.GetActor(Enemy);
        Assert.Equal(before.GetActor(Enemy).Position, actor.Position);
        Assert.Equal(new MapPosition(4, 5), decision.Destination);
        Assert.Equal(new[] { new MapPosition(5, 5), new MapPosition(4, 5) }, decision.Path);
        Assert.Equal(new MapPosition(5, 5), actor.Deployment.Position);
        Assert.Equal((byte)0x14, actor.AiMemory); Assert.Equal(0x39340042u, decision.Battle.ThinkingSeed);
        Assert.Equal(0xCAFE5678u, decision.Battle.MainSeed); Assert.Equal((ushort)0, decision.Battle.Regions!.Tested);
        Assert.Equal(before.Regions!.Flags, decision.Battle.Regions.Flags); Assert.Equal(new ActorRef("remembered-target"), actor.LastTarget);
        Assert.Equal(new ushort?[] { 8, 2, 1 }, decision.Effects.Where(e => e.Kind == "thinking-rng").Select(e => e.RandomRange));
        Assert.Equal(new ushort?[] { 7, 0, 0 }, decision.Effects.Where(e => e.Kind == "thinking-rng").Select(e => e.RandomValue));
        Assert.Equal(frozen, JsonSerializer.Serialize(before)); Assert.Null(actor.Exp); Assert.Null(decision.Battle.Gold);
    }

    [Theory]
    [InlineData(BattleMover.Hovering, 4, 0x14)]
    [InlineData(BattleMover.Centaur, 5, 0)]
    public void ContentMoverChangesReachabilityWithoutChangingTheStandbyPolicy(BattleMover mover, int x, byte memory)
    {
        var decision = SourceEnemyAi.Resolve(State(mover), Enemy);
        Assert.Equal(new MapPosition(x, 5), decision.Destination); Assert.Equal(memory, decision.Battle.GetActor(Enemy).AiMemory);
    }

    [Fact]
    public void ImmediateIdleRetainsMemoryAndStillPerformsItsRealThinkingDraw()
    {
        var before = State(memory: 0x24, thinking: 0x03340099);
        var after = SourceEnemyAi.Resolve(before, Enemy);
        Assert.Equal(before.GetActor(Enemy).Position, after.Destination); Assert.Equal((byte)0x24, after.Battle.GetActor(Enemy).AiMemory);
        Assert.Equal(0x02340099u, after.Battle.ThinkingSeed);
        Assert.Single(after.Effects, effect => effect.Kind == "thinking-rng");
    }

    [Fact]
    public void ActivatedSetSevenPreservesFailedCommandOrderAndPursuesWithoutReseeding()
    {
        var before = State(active: true, commandset: 7, memory: 0x24, blockSouth: false, peer: false, allyPosition: new(5, 15));
        var after = SourceEnemyAi.Resolve(before, Enemy);
        Assert.Equal(new MapPosition(5, 7), after.Destination);
        Assert.Equal(new[] { "ai-command-move-order1", "ai-command-attack1", "ai-command-heal1", "ai-command-support", "ai-command-move1" },
            after.Effects.Where(effect => effect.Kind.StartsWith("ai-command-", StringComparison.Ordinal)).Select(effect => effect.Kind));
        Assert.Equal(new long?[] { -1, -1, -1, -1, 0 }, after.Effects.Where(effect => effect.Kind.StartsWith("ai-command-", StringComparison.Ordinal)).Select(effect => effect.After));
        Assert.Equal(before.ThinkingSeed, after.Battle.ThinkingSeed); Assert.Equal(before.MainSeed, after.Battle.MainSeed);
        Assert.Equal((byte)0x24, after.Battle.GetActor(Enemy).AiMemory); Assert.Equal(before.GetActor(Enemy).LastTarget, after.Battle.GetActor(Enemy).LastTarget);
        Assert.Equal((ushort)0x2071, after.Battle.GetActor(Enemy).ActivationWord); Assert.True(after.Battle.Regions!.Flags[6]);
    }

    [Theory]
    [InlineData("attack", "physical-definition")]
    [InlineData("memory", "standby-memory")]
    [InlineData("order", "source-move-order")]
    [InlineData("unknown", "source-occupancy-word")]
    public void ReachedUnsupportedInputsCannotPublishPartialMemoryRegionsOrRng(string shape, string code)
    {
        var before = State(active: shape == "attack", memory: shape == "memory" ? (byte)2 : (byte)0,
            blockSouth: false, peer: false, allyPosition: shape == "attack" ? new(5, 8) : shape == "unknown" ? new(5, 7) : null);
        if (shape == "order") before = before.With(actors: before.Actors.Select(actor => actor.Actor == Enemy ?
            new BattleActorState(actor.Deployment with { Initialization = actor.Deployment.Initialization! with { PrimaryOrder = 22 } },
                actor.Hp, actor.Mp, actor.Exp, actor.Position, actor.Kills, actor.Defeats, activationWord: actor.ActivationWord) : actor));
        if (shape == "unknown") before = before.With(actors: before.Actors.Select(actor => actor.IsAlly ?
            new BattleActorState(actor.Deployment, actor.Hp, actor.Mp, actor.Exp, actor.Position, actor.Kills, actor.Defeats) : actor));
        string frozen = JsonSerializer.Serialize(before);
        Assert.Equal(code, Assert.Throws<BattleRuleException>(() => SourceEnemyAi.Resolve(before, Enemy)).Code);
        Assert.Equal(frozen, JsonSerializer.Serialize(before));
    }

    [Fact]
    public void NoTriggerRegionsActivatesTheActualActorAndUsesItsOwnCommandset()
    {
        var before = State(commandset: 7, primaryRegion: 15, blockSouth: false, peer: false, allyPosition: new(5, 15));
        var after = SourceEnemyAi.Resolve(before, Enemy);
        Assert.Equal((ushort)0x2071, after.Battle.GetActor(Enemy).ActivationWord);
        Assert.Contains(after.Effects, effect => effect.Kind == "activation-word");
        Assert.DoesNotContain(after.Effects, effect => effect.Kind == "source-standby");
        Assert.Equal(before.ThinkingSeed, after.Battle.ThinkingSeed);
    }
}
