using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PrivateSourceAiTests
{
    [PrivateInputFact("SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD", "SF2_PRIVATE_CONTROLLED_START")]
    public void ActualPrivateCommandsRunInactiveStandbyThenActivationAndSetSevenToNextPlayer()
    {
        var session = Assert.IsType<SessionStarted>(GameSession.Start(PrivateBattleScenarioTests.Selected())).Session;
        var definition = session.Definition; var initial = session.Current.Battle;
        Assert.All(initial.Actors.Where(a => !a.IsAlly), actor =>
        { Assert.Equal((byte)255, actor.Deployment.Initialization!.PrimaryOrder); Assert.Equal((byte)255, actor.Deployment.Initialization.SecondaryOrder); });
        FinishMovement(session, Stay(session)); var relay = FinishMovement(session, Stay(session));
        Assert.Equal(new ActorRef("ally-0"), session.Current.Selection!.Actor);
        Assert.Equal(0x01340000u, session.Current.Battle.ThinkingSeed); Assert.Equal(0xA4991234u, session.Current.Battle.MainSeed);
        Assert.Equal(new MapPosition[] { new(6, 3), new(10, 4), new(6, 5), new(8, 4), new(9, 6), new(6, 6) }, session.Current.Battle.Actors.Skip(3).Select(actor => actor.Position));
        Assert.Equal(new byte[] { 0x14, 0x34, 0x24, 0x24, 0x24, 0x24 }, session.Current.Battle.Actors.Skip(3).Select(actor => actor.AiMemory));
        Assert.Equal(new[] { "enemy-0", "enemy-3", "enemy-5", "enemy-1", "enemy-2", "enemy-4" },
            relay.Observations.Where(row => row.Kind == "source-standby").Select(row => row.Actor!.Value.Value));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.North))); FinishMovement(session, Stay(session)); // Real first Bowie move to (8,17).
        for (int steps = 0; session.Current.Battle.Round < 3; steps++)
        {
            Assert.True(steps < 6);
            if (session.Current.Selection!.Actor == new ActorRef("ally-0"))
            {
                for (int i = 0; i < 3; i++) FinishMovement(session, Accept(session, new Move(ExplorationDirection.East)));
                for (int i = 0; i < 2; i++) FinishMovement(session, Accept(session, new Move(ExplorationDirection.North)));
            }
            FinishMovement(session, Stay(session));
        }
        var activated = session.Current.Battle;
        Assert.Equal(new MapPosition(11, 15), activated.GetActor(new("ally-0")).Position);
        Assert.Equal(new[] { false, true, false }, activated.Regions!.Flags.Take(3));
        Assert.Equal(new ushort?[] { 0x2060, 0x2060, 0x2060, 0x2061, 0x2071, 0x2070 }, activated.Actors.Skip(3).Select(actor => actor.ActivationWord));
        Assert.Equal(0x9BD71234u, activated.MainSeed); Assert.Equal(new ActorRef("ally-2"), session.Current.Selection!.Actor);
        FinishMovement(session, Stay(session)); FinishMovement(session, Stay(session)); var setSeven = FinishMovement(session, Stay(session)); // Chester, Bowie, Sarah; source enemies run in between.
        Assert.Contains(setSeven.Observations, observation => observation.Actor == new ActorRef("enemy-4") && observation.Kind == "ai-command-move-order1" && observation.After == -1);
        Assert.Equal(new MapPosition(11, 6), session.Current.Battle.GetActor(new("enemy-4")).Position);
        Assert.Equal(4, session.Current.Battle.Round); Assert.Equal(new ActorRef("ally-1"), session.Current.Selection!.Actor);
        Assert.Equal(0x51DC1234u, session.Current.Battle.MainSeed); Assert.Equal(0x02340000u, session.Current.Battle.ThinkingSeed);
        Assert.Equal(new byte[] { 0x14, 0x24, 0x14, 0x24, 0x34, 0x34 }, session.Current.Battle.Actors.Skip(3).Select(actor => actor.AiMemory));
        SessionResult? action = null;
        for (int turns = 0; turns < 12; turns++)
        {
            action = FinishBattleScenes(session, Stay(session));
            if (action.Observations.Any(row => row.Kind == "physical-first")) break;
        }
        // Same actual command trajectory as the retained ActualRoundSixAttack reference comparison.
        Assert.Null(action!.Failure); Assert.Equal(SessionStopReason.PlayerInput, action.StopReason);
        Assert.Equal(6, session.Current.Battle.Round); Assert.Equal(6, session.Current.Battle.Cursor);
        Assert.Equal(new ActorRef("ally-0"), session.Current.Selection!.Actor);
        Assert.Equal(0xAF881234u, ConstructionSeed(action)); Assert.Equal(0x01340000u, session.Current.Battle.ThinkingSeed);
        Assert.Equal(new MapPosition(11, 14), session.Current.Battle.GetActor(new("enemy-4")).Position);
        Assert.Equal((ushort)9, session.Current.Battle.GetActor(new("ally-0")).Hp);
        Assert.Equal(new ActorRef("ally-0"), session.Current.Battle.GetActor(new("enemy-4")).LastTarget);
        Assert.Equal(new ushort?[] { 12, 30, 0, 0, 11, 21 }, ConstructionRolls(action).Select(row => row.RandomValue));
        Assert.Same(definition, session.Definition); Assert.Null(session.Current.Battle.Gold);
        foreach (var actor in session.Current.Battle.Actors)
        {
            var before = initial.GetActor(actor.Actor);
            Assert.Same(before.Deployment, actor.Deployment); Assert.Equal(before.Attack, actor.Attack);
            Assert.Equal(before.Mp, actor.Mp); Assert.Null(actor.Exp); Assert.Null(actor.Kills); Assert.Null(actor.Defeats);
            Assert.Equal(before.Definition.SourceLoadout!.Items, actor.Definition.SourceLoadout!.Items);
            Assert.Equal(before.Definition.SourceLoadout.Spells, actor.Definition.SourceLoadout.Spells);
        }
        // Unknown accounting becomes a reached failure on the actual next player attack, atomically.
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(new("enemy-4")));
        var beforeAttack = session.Current;
        Assert.Equal("unspecified-exp", Send(session, new Confirm()).Failure!.Code);
        Assert.Same(beforeAttack, session.Current);
    }
}
