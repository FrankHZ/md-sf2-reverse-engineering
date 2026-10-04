using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class SourceEnemyAiTests
{
    [Theory]
    [InlineData("{W1}", 0x00340099u)]
    [InlineData("{W2}", 0x00340099u)]
    [InlineData("{W2}", 0x00ABCDEFu)]
    public void FieldTextCopyReplacesInitialAiByteBeforeEntryAndAiKeepsItsUpdatedByte(string textToken, uint preserved)
    {
        var template = ExplorationTextWaitTests.StartFieldText(textToken, configure: document =>
        {
            document["start"]!.AsObject().Remove("program");
            document["start"]!["map"] = "yard-map";
            document["start"]!["flags"] = new JsonArray(13);
            document["world"]!["maps"]![1]!["battle"]!["before"]!["program"] = "invitation";
            document["world"]!["programs"]![0]!["instructions"]![1]!["speaker"] = null;
            document["world"]!["programs"]![0]!["instructions"]![1]!["speakerFlags"] = 255;
        });
        var source = State(peer: false).Definition;
        var deployments = source.Deployments.Select(row => row.Faction == BattleFaction.Ally
            ? row with { Initialization = new(null, 0, 15, 15, 0) } : row).ToArray();
        var encounter = new BattleDefinition("yard-watch", new("yard-map"), source.Width, source.Height,
            source.Terrain, deployments, [], initialization: new([
                new(6, [new(0, 0), new(1, 0), new(1, 1), new(0, 1)])], BattleRegionProgram.None));
        var definition = new ScenarioDefinition("field-ai-copy", [encounter], exploration: template.Definition.Exploration);
        foreach (uint oldByte in new[] { 0x03000000u, 0x12000000u })
        {
            uint thinking = oldByte | preserved;
            var policy = new NewBattleStartPolicy("controlled engine behavior", nameof(SourceEnemyAiTests),
                "before-battle text to first automatic AI", 0, false, true, true, false, false, 0);
            var party = new BattleStartInput("yard-watch", deployments.Select(row =>
                new BattleActorStartInput(row.Actor, row.Definition.MaxHp, row.Definition.MaxMp, 0, 0, 0, 0, null)),
                0xCAFE5678, thinking, 0, policy);
            var world = template.Current.Exploration!;
            var start = new ExplorationStartInput(world.Map, world.Player, world.PlayerEntity.Position, 0, 24,
                [13], party, display: template.Current.Story.Display, textSettings: template.Current.Story.TextSettings);
            var opened = Assert.IsType<SessionStarted>(GameSession.Start(definition, start));
            Assert.Null(opened.Result.Failure);
            var session = opened.Session;
            for (int i = 0; !session.Current.CanWaitForText; i++)
            {
                Assert.True(i < 30);
                Assert.NotNull(session.Current.Story.Wait);
                if (session.Current.Story.Wait is FieldTextWait { LogicalDone: true, Revealed: false } text)
                    Accept(session, new CompleteTextReveal(text.Token));
                else Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
            }
            Assert.Null(session.Current.Story.RandomSeedCopy);
            Assert.Equal((byte)(thinking >> 24), session.Current.CurrentRandomSeedCopy);
            var accepted = Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
            Assert.Contains(accepted.Observations, row => row.Kind == (textToken == "{W2}" ? "rng-text-w2" : "rng-text-w1"));
            for (int i = 0; !session.Current.HasBattleControl; i++)
            {
                Assert.True(i < 30);
                Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
            }
            var entered = session.Current;
            Assert.Equal((byte)0x4E, entered.Story.RandomSeedCopy);
            Assert.Equal((byte)0x4E, entered.CurrentRandomSeedCopy);
            Assert.Equal(0x4E000000u | preserved, entered.Battle.ThinkingSeed);
            var action = Stay(session);
            Assert.Equal(entered.Battle.MainSeed, action.Snapshot.Battle.MainSeed);
            var result = FinishMovement(session, action);
            Assert.Contains(result.Observations, row => row.Kind == "source-standby");
            Assert.Equal(new MapPosition(4, 6), session.Current.Battle.GetActor(Enemy).Position);
            Assert.Equal((byte)0x13, session.Current.Battle.GetActor(Enemy).AiMemory);
            Assert.Equal(preserved, session.Current.Battle.ThinkingSeed);
            Assert.Equal((byte)0, session.Current.CurrentRandomSeedCopy);
            Assert.Equal((byte)0x4E, session.Current.Story.RandomSeedCopy); // Historical text write, not current AI state.
            var draws = result.Observations.Where(row => row.Kind == "thinking-rng").ToArray();
            Assert.Equal(new ushort?[] { 8, 2, 2 }, draws.Select(row => row.RandomRange));
            Assert.Equal(new ushort?[] { 0, 1, 0 }, draws.Select(row => row.RandomValue));
            Assert.Equal(0x4E000000u | preserved, draws[0].Before);
            Assert.Equal(preserved, draws[^1].After);
        }
    }

    private static readonly ActorRef Enemy = new("varied-source-enemy");
    internal static EngineBattleState State(BattleMover mover = BattleMover.Hovering, bool active = false,
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

    [Theory]
    [InlineData(0x12340042u, 0x39340042u)]
    [InlineData(0x80ABCDEFu, 0x39ABCDEFu)]
    [InlineData(0xFF000000u, 0x39000000u)]
    public void InactiveMovementUsesItsSourceAnchorMemoryAndIndependentThinkingChannel(uint thinking, uint after)
    {
        var before = State(thinking: thinking); string frozen = JsonSerializer.Serialize(before);
        var decision = SourceEnemyAi.Resolve(before, Enemy); var actor = decision.Battle.GetActor(Enemy);
        Assert.Equal(before.GetActor(Enemy).Position, actor.Position);
        Assert.Equal(new MapPosition(4, 5), decision.Destination);
        Assert.Equal(new[] { new MapPosition(5, 5), new MapPosition(4, 5) }, decision.Path);
        Assert.Equal(new MapPosition(5, 5), actor.Deployment.Position);
        Assert.Equal((byte)0x14, actor.AiMemory); Assert.Equal(after, decision.Battle.ThinkingSeed);
        Assert.Equal(0xCAFE5678u, decision.Battle.MainSeed); Assert.Equal((ushort)0, decision.Battle.Regions!.Tested);
        Assert.Equal(before.Regions!.Flags, decision.Battle.Regions.Flags); Assert.Equal(new ActorRef("remembered-target"), actor.LastTarget);
        Assert.Equal(new ushort?[] { 8, 2, 1 }, decision.Effects.Where(e => e.Kind == "thinking-rng").Select(e => e.RandomRange));
        Assert.Equal(new ushort?[] { 7, 0, 0 }, decision.Effects.Where(e => e.Kind == "thinking-rng").Select(e => e.RandomValue));
        Assert.Equal(frozen, JsonSerializer.Serialize(before)); Assert.Null(actor.Exp); Assert.Null(decision.Battle.Gold);
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, 0, decision.Battle, null, SessionStopReason.PlayerInput);
        Assert.Null(snapshot.Story.RandomSeedCopy);
        Assert.Equal((byte)0x39, snapshot.CurrentRandomSeedCopy); // No text write is needed to observe real AI state.
    }

    [Theory]
    [InlineData(BattleMover.Hovering, 4, 0x14)]
    [InlineData(BattleMover.Centaur, 5, 0)]
    public void ContentMoverChangesReachabilityWithoutChangingTheStandbyPolicy(BattleMover mover, int x, byte memory)
    {
        var decision = SourceEnemyAi.Resolve(State(mover), Enemy);
        Assert.Equal(new MapPosition(x, 5), decision.Destination); Assert.Equal(memory, decision.Battle.GetActor(Enemy).AiMemory);
    }

    [Theory]
    [InlineData(0x03340099u, 0x02340099u)]
    [InlineData(0x03FEDCBAu, 0x02FEDCBAu)]
    public void ImmediateIdleRetainsMemoryAndStillPerformsItsRealThinkingDraw(uint thinking, uint expected)
    {
        var before = State(memory: 0x24, thinking: thinking);
        var after = SourceEnemyAi.Resolve(before, Enemy);
        Assert.Equal(before.GetActor(Enemy).Position, after.Destination); Assert.Equal((byte)0x24, after.Battle.GetActor(Enemy).AiMemory);
        Assert.Equal(expected, after.Battle.ThinkingSeed);
        var draw = Assert.Single(after.Effects, effect => effect.Kind == "thinking-rng");
        Assert.Equal((ushort)8, draw.RandomRange); Assert.Equal((ushort)2, draw.RandomValue);
        Assert.Equal(before.MainSeed, after.Battle.MainSeed);
        Assert.Equal(before.GetActor(Enemy).LastTarget, after.Battle.GetActor(Enemy).LastTarget);
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
