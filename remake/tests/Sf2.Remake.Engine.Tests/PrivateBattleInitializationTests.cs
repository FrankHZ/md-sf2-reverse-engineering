using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PrivateBattleInitializationTests
{
    private static NewBattleStartPolicy Policy => new("controlled-varied-party", "project-authored-comparison", "No natural entry claim",
        0, true, true, true, false, false, 0);
    private static (ScenarioDefinition Scenario, BattleStartInput Start) Setup(BattleMover mover = BattleMover.Regular,
        byte allyId = 7, byte enemyAttack = 11, byte enemyAgility = 1, byte regionId = 6, byte primary = 6,
        byte secondary = 15, BattleRegionProgram program = BattleRegionProgram.None, byte spawn = 0)
    {
        var player = new BattleActorDefinition(new("different-player"), BattleClassRule.UnpromotedPriest, 3, 30, 12, 19, 7, 40, false, 6,
            [new("heal", 1), new("egress", 1)], mover: mover, sourceLoadout: new([213, 127, 127, 127], [0, 10, 63, 63]));
        var enemy = new BattleActorDefinition(new("different-enemy"), BattleClassRule.Ordinary, 2, 16, 4, enemyAttack, 8, enemyAgility, false, 4,
            [], mover: BattleMover.Hovering, sourceLoadout: new([127, 127, 127, 127], [63, 63, 63, 63]));
        var terrain = Enumerable.Repeat(new BattleTerrain(TerrainSurface.Open, TerrainProtection.None), 2304).ToArray();
        terrain[2 * 48 + 3] = new(TerrainSurface.Rough, TerrainProtection.Heavy);
        var region = new BattleActivationRegion(regionId, [new(2, 2), new(4, 2), new(4, 4), new(2, 4)]);
        var battle = new BattleDefinition("varied-entry", new("unrelated-map"), 12, 12, terrain,
            [new(player, BattleFaction.Ally, allyId, BattleControl.Player, null, new(2, 2), new(null, 0, 15, 15, 0)),
             new(enemy, BattleFaction.Enemy, 130, BattleControl.Automatic, BattleAiStrategy.SourceOrders, new(8, 8), new(0x3000, spawn, primary, secondary, 0x60, 6, 255, 255))],
            [new(new("heal", 1), 3, 15, 0, 1)], initialization: new([region], program));
        var start = new BattleStartInput("varied-entry", [new(player.Actor, 8, 2, null, null, null, 0, null), new(enemy.Actor, 3, 1, null, null, null, 0, null)],
            0x00005678, 0xBEEF4321, null, Policy);
        return (new("varied-scenario", [battle]), start);
    }

    [Theory]
    [InlineData(BattleMover.Regular, 3, 13)]
    [InlineData(BattleMover.Healer, 3, 13)]
    [InlineData(BattleMover.Centaur, 5, 13)]
    [InlineData(BattleMover.Hovering, 2, 13)]
    public void VariedDefinitionHealsOnceActivatesAnEdgeAndUsesTheActualMover(BattleMover mover, int cost, int attack)
    {
        var input = Setup(mover); var before = JsonSerializer.Serialize(input);
        var first = Assert.IsType<SessionStarted>(GameSession.Start(input.Scenario, input.Start));
        var second = Assert.IsType<SessionStarted>(GameSession.Start(input.Scenario, input.Start));
        var state = first.Session.Current.Battle; var player = state.GetActor(new("different-player")); var enemy = state.GetActor(new("different-enemy"));
        Assert.Equal((ushort)30, player.Hp); Assert.Equal((byte)12, player.Mp); Assert.Equal((byte)19, player.Attack);
        Assert.Equal((ushort)16, enemy.Hp); Assert.Equal((byte)4, enemy.Mp); Assert.Equal(attack, (int)enemy.Attack); Assert.Equal((byte)11, enemy.Definition.Attack);
        Assert.Equal((ushort)0x3061, enemy.ActivationWord); Assert.Equal((ushort)64, state.Regions!.Tested);
        Assert.True(state.Regions.Flags[6]); Assert.Single(state.Regions.Flags, flag => flag);
        Assert.Null(player.Exp); Assert.Null(state.Gold); Assert.Equal((ushort)0, player.ActivationWord);
        Assert.Equal(0xBEEF4321u, state.ThinkingSeed); Assert.Equal(0x5678u, state.MainSeed & 65535);
        var preview = FinishMovement(first.Session, Accept(first.Session, new Move(ExplorationDirection.East)));
        Assert.Equal(cost, preview.Snapshot.Selection!.Preview.Cost); Assert.Same(state, preview.Snapshot.Battle);
        Accept(first.Session, new Confirm()); var cancelled = FinishMovement(first.Session, Accept(first.Session, new Cancel()));
        Assert.Equal(new MapPosition(2, 2), cancelled.Snapshot.Selection!.Preview.Destination); Assert.Same(state, cancelled.Snapshot.Battle);
        Assert.Equal(before, JsonSerializer.Serialize(input)); Assert.NotSame(state, second.Session.Current.Battle);
    }

    [Fact]
    public void SecondaryActivationAndAnExistingWordPreserveTheOtherSourceBits()
    {
        var input = Setup(primary: 15, secondary: 6); var battle = input.Scenario.Encounters[input.Start.Encounter];
        var initial = BattleTurnFlow.Start(battle, input.Start); var activated = BattleTurnFlow.GenerateRound(initial);
        Assert.Equal((ushort)0x3060, initial.GetActor(new("different-enemy")).ActivationWord);
        Assert.Equal((ushort)0x3063, activated.GetActor(new("different-enemy")).ActivationWord);
        Assert.All(initial.Regions!.Flags, flag => Assert.False(flag)); Assert.Equal((ushort)0, initial.Regions.Tested);
        var preserved = initial.With(actors: initial.Actors.Select(actor => actor.IsAlly ? actor.With(activationWord: 0x8000) : actor));
        var admitted = BattleControlRules.EnterPlayer(preserved, new("different-player"));
        Assert.Same(preserved, admitted); Assert.Equal((ushort)0x8000, admitted.GetActor(new("different-player")).ActivationWord);
    }

    [Theory]
    [InlineData("program", "region-program")]
    [InlineData("spawn", "spawn-mode")]
    [InlineData("region", "missing-activation-region")]
    [InlineData("difficulty", "difficulty")]
    [InlineData("word", "player-control-missingactivationword")]
    public void UnsupportedInitialPrerequisitesPublishNoSessionOrSeedChanges(string shape, string expected)
    {
        var input = Setup(program: shape == "program" ? BattleRegionProgram.Required : BattleRegionProgram.None,
            spawn: shape == "spawn" ? (byte)1 : (byte)0, primary: shape == "region" ? (byte)3 : (byte)6);
        var start = input.Start;
        if (shape is "difficulty" or "word") start = new(start.Encounter, start.Actors, start.MainSeed, start.ThinkingSeed, start.Gold,
            shape == "difficulty" ? Policy with { Difficulty = 1 } : Policy with { MissingCandidateAllyWord = null });
        var before = JsonSerializer.Serialize(start);
        Assert.Equal(expected, Assert.IsType<SessionStartFailed>(GameSession.Start(input.Scenario, start)).Failure.Code);
        Assert.Equal(before, JsonSerializer.Serialize(start));
    }

    [Fact]
    public void KnownUnimplementedSpellAndUnknownAccountingFailOnlyWhenReached()
    {
        var input = Setup(); var session = Assert.IsType<SessionStarted>(GameSession.Start(input.Scenario, input.Start)).Session;
        Accept(session, new Confirm()); var before = session.Current;
        Assert.Equal("spell-effect", Send(session, new SelectSpell(new("egress", 1))).Failure!.Code); Assert.Same(before, session.Current);
        Accept(session, new SelectSpell(new("heal", 1))); Accept(session, new SelectTarget(new("different-player")));
        before = session.Current;
        Assert.Equal("unspecified-exp", Send(session, new Confirm()).Failure!.Code); Assert.Same(before, session.Current);
        FinishMovement(session, Accept(session, new Cancel())); Accept(session, new Confirm());
        Assert.Equal("physical-definition", Send(session, new ChooseAction(SessionAction.PhysicalAttack)).Failure!.Code);
    }

    [Theory]
    [InlineData(7, 8)] [InlineData(11, 13)] [InlineData(204, 255)]
    public void EnemyDifficultyZeroAddsTheTruncatedQuarterWithinItsAdmittedDomain(byte source, byte expected) =>
        Assert.Equal(expected, BattleInitializationRules.EnemyAttack(source, 0));
}
