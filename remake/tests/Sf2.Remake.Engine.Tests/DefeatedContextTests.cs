using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class DefeatedContextTests
{
    [Theory]
    [InlineData("reset", false)]
    [InlineData("reset", true)]
    [InlineData("hp", true)]
    [InlineData("mp", true)]
    [InlineData("join", true)]
    [InlineData("shop", true)]
    [InlineData("church", true)]
    [InlineData("transfer", true)]
    [InlineData("warp", true)]
    [InlineData("return", true)]
    [InlineData("random-battle", true)]
    [InlineData("egress", true)]
    [InlineData("leave", true)]
    [InlineData("convert", true)]
    [InlineData("load-entities", true)]
    [InlineData("load-map", true)]
    public void ReachedResourceOrLifecycleWorkRefusesBeforePublicationAndKeepsPrefix(string operation, bool nested)
    {
        StoryInstruction rejected = operation switch
        {
            "reset" => new ResetPartyBattleStats(),
            "hp" => new RestoreMemberResource(9, true),
            "mp" => new RestoreMemberResource(7, false),
            "join" => new JoinPartyMember(9),
            "shop" => new OpenShop(new([1], [new(1, "herb", 10)])),
            "church" => new OpenChurch(new(1, 1, 10, 1, 1)),
            "transfer" => new TransferToMap(new("castle"), new(2, 3), 1, MapLoadMode.Rebuild),
            "warp" => new QueueMapWarp(new("castle"), new(2, 3), 1),
            "return" => new ReturnBattleMap(),
            "random-battle" => new SourceRandomBattle(1, new("unsafe", 0)),
            "egress" => new SetEgressMap(new("elsewhere")),
            "leave" => new LeaveOutcomeBattleParty(9),
            "convert" => new ConvertBattleActor(128, 9),
            "load-entities" => new LoadSetupEntities(new(2, 3), 1),
            "load-map" => new LoadSceneMap(new("castle"), new(2, 3)),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
        var (definition, before) = BattleEndPositionTests.DefeatedContext(programs:
        [new("defeated", [new WriteFlag(777, true), new SetEntityPosition(new("entity-7"), new(20, 21), 1),
            nested ? new CallProgram(new("branch", 0)) : rejected, new WaitProgramTicks(1), new EndProgram()]),
         new("branch", [new BranchFlag(777, true, new("unsafe", 0)), new EndProgram()]),
         new("unsafe", [rejected, new WaitProgramTicks(1), new EndProgram()])], change: ChangedResources);
        var result = Begin(definition, before);
        if (operation == "reset")
        {
            Assert.Equal((byte)2, result.Snapshot.Exploration!.Party.Actors.Single(a => a.Actor == new ActorRef("medic-a")).Mp);
            Assert.Equal((ushort)0, result.Snapshot.Exploration.Party.Actors.Single(a => a.Actor == new ActorRef("guard-a")).Hp);
        }
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Equal("defeated-program-instruction", result.Failure?.Code);
        Assert.Equal("program." + rejected.GetType().Name, result.Failure?.Field);
        Assert.Contains(777, result.Snapshot.Story.Flags);
        Assert.Equal(new MapPosition(20, 21), result.Snapshot.Exploration!.PlayerEntity.Position);
        Assert.Equal(new ProgramLocation(nested ? "unsafe" : "defeated", nested ? 0 : 2), result.Snapshot.Story.Cursor);
        Assert.Null(result.Snapshot.Story.Wait);
        Assert.Null(result.Snapshot.Story.ScriptWarp);
        Assert.Null(result.Snapshot.Story.EgressMap);
        Assert.Same(before.Battle, result.Snapshot.Story.DefeatedScene!.Battle);
        var party = result.Snapshot.Exploration.Party;
        Assert.Equal(before.Battle.Gold, party.Gold);
        Assert.Same(before.Battle.ItemCustody, party.ItemCustody);
        foreach (var actor in before.Battle.Actors.Where(a => a.IsAlly))
        {
            var actual = Assert.Single(party.Actors, a => a.Actor == actor.Actor);
            Assert.Equal(actor.Hp, actual.Hp);
            Assert.Equal(actor.Mp, actual.Mp);
            Assert.Equal(actor.SourceLoadout, actual.SourceLoadout);
        }
        Assert.DoesNotContain(result.Observations, e => e.Detail == rejected.GetType().Name || e.Kind == "field-death-started");
    }

    [Theory]
    [InlineData("population", "defeated-population")]
    [InlineData("leader", "defeated-leader")]
    [InlineData("visual", "battle-visual-alias")]
    [InlineData("sprite", "defeated-field-sprite")]
    public void MissingStagingPrerequisiteReturnsTypedFailureWithoutStarting(string missing, string code)
    {
        var (definition, before) = BattleEndPositionTests.DefeatedContext(population: missing != "population",
            enemySprites: missing != "sprite", change: battle => missing switch
            {
                "leader" => battle.With(actors: battle.Actors.Select(a => a.Actor == battle.Definition.Outcome!.Leader ? a.With(clearPosition: true) : a)),
                "visual" => battle.With(visuals: new(new Dictionary<ActorRef, int>(), [])),
                _ => battle,
            });
        var result = Begin(definition, before);
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Equal(code, result.Failure?.Code);
        Assert.Same(before.Active, result.Snapshot.Active);
        Assert.Empty(result.Observations);
    }

    [Fact]
    public void UntakenResourceBranchDoesNotPreventDialoguePoseAndWait()
    {
        var (definition, before) = BattleEndPositionTests.DefeatedContext(programs:
        [new("defeated", [new BranchFlag(777, true, new("unsafe", 0)), new CallProgram(new("speech", 0)),
            new SetEntityFacing(new("entity-9"), 1), new WaitProgramTicks(1), new EndProgram()]),
         new("speech", [new SetTextCursor(0), new ShowText(TextDisplayMode.Single, null), new ReturnProgram()]),
         new("unsafe", [new ResetPartyBattleStats(), new EndProgram()])]);
        // The finite authored text path uses the same staged source actors.
        var world = new ExplorationDefinition(definition.Exploration!.Maps.Values,
            definition.Programs.Values, new Dictionary<int, string> { [0] = "Hold here." });
        definition = new("changed-dialogue", [before.Battle.Definition], exploration: world, battleScenes: definition.BattleScenes);
        var result = Begin(definition, before);
        Assert.Null(result.Failure);
        var wait = Assert.IsType<DialogueWait>(result.Snapshot.Story.Wait);
        result = ExplorationDispatcher.Submit(definition, result.Snapshot, new Acknowledge(wait.Token));
        Assert.Null(result.Failure);
        Assert.IsType<TickWait>(result.Snapshot.Story.Wait);
        Assert.Equal(1, result.Snapshot.Exploration!.Entities[new("entity-9")].Motion.Facing);
        Assert.Same(before.Battle, result.Snapshot.Story.DefeatedScene!.Battle);
    }

    internal static EngineBattleState ChangedResources(EngineBattleState battle) => battle.With(gold: 317,
        itemCustody: new([11], new Dictionary<byte, byte> { [2] = 3 }),
        actors: battle.Actors.Select(a => a.Actor == new ActorRef("medic-a") ? a.With(hp: 13, mp: 2) :
            a.Actor == new ActorRef("guard-a") ? a.With(hp: 0) : a));

    private static SessionResult Begin(ScenarioDefinition definition, SessionSnapshot before) =>
        BattleDefeatedProgram.Begin(definition, new(before, [], before.StopReason), RuleCompositions.Sf2());
}
