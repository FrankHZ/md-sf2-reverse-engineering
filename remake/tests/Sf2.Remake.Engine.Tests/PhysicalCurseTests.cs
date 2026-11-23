using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PhysicalCurseTests
{
    private static readonly ActorRef Attacker = new("swordsman"), Enemy = new("raider"), Next = new("lookout");

    [Fact]
    public void InactionPublishesOnlyTheConfirmedStopAndOneTurn()
    {
        var session = CursedBattle(0);
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.North)));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.West)));
        var stop = session.Current.Selection!.Preview.Destination;
        var before = session.Current.Battle;
        var action = Attack(session, Enemy);
        var scene = session.Current.BattleScene!;
        Assert.Empty(scene.Action.Reactions);
        Assert.Equal(stop, session.Current.Battle.GetActor(Attacker).Position);
        Assert.Equal(new[] { "rng-curse-inaction" }, ConstructionRolls(action).Select(row => row.Kind));
        Accept(session, new CompletePresentation(scene.Token, scene.CompletionKind));
        scene = session.Current.BattleScene!;
        Assert.Equal(BattleScenePhase.PhysicalInactionMessage, scene.Phase);
        Assert.Equal(Enemy, scene.Target);
        Assert.False(scene.HasReaction);
        Assert.Equal(before.GetActor(Enemy).Hp, session.Current.Battle.GetActor(Enemy).Hp);
        Assert.Equal(before.Gold, session.Current.Battle.Gold);
        Assert.NotNull(Send(session, new CompletePresentation(scene.Token, scene.CompletionKind)).Failure);
        var done = FinishBattleScenes(session, Accept(session, new Acknowledge(scene.Token)));
        Assert.Single(done.Observations, row => row.Kind == "curse-inaction");
        Assert.Single(done.Observations, row => row.Kind == "action-committed");
        Assert.DoesNotContain(done.Observations, row => row.Kind is "hp" or "exp");
        Assert.Equal(Next, session.Current.Selection!.Actor);
        Assert.Equal((ushort)4, session.Current.Battle.GetActor(Attacker).Status);
    }

    [Theory]
    [InlineData(21421u, 0, true)]
    [InlineData(1970u, 1, false)]
    public void AnAlreadyDispatchedMixedStatusSceneDrawsCurseThenStun(uint seed, int curseRoll, bool curseMessage)
    {
        var battle = CursedBattle(1).Current.Battle;
        battle = battle.With(mainSeed: (seed << 16) | 0x1234u, actors: battle.Actors.Select(actor =>
            actor.Actor == Attacker ? actor.With(status: 5, refreshStats: true) : actor));
        // Normal dispatch skips STUN before opening the menu. This probes only the scene initializer.
        var action = PhysicalBattleAction.PrepareSourceDefault(battle, Attacker, new(3, 3), Enemy);
        Assert.Empty(action.Reactions);
        Assert.Null(action.Reward);
        Assert.Equal(curseMessage, action.Inaction!.DueToCurse);
        Assert.Equal(new[] { "rng-curse-inaction", "rng-stun-inaction" }, action.ConstructionEffects.Select(row => row.Kind));
        Assert.Equal(new ushort?[] { (ushort)curseRoll, 0 }, action.ConstructionEffects.Select(row => row.RandomValue));
    }

    [Theory]
    [InlineData(58, 60, true)]
    [InlineData(1, 15, false)]
    public void AFirstDodgeOrLethalDefenderSkipsRecoil(int seed, int enemyHp, bool dodges)
    {
        var session = CursedBattle(seed, enemyHp: enemyHp);
        var action = Attack(session, Enemy);
        Assert.Equal(dodges ? BattleReactionKind.Dodge : BattleReactionKind.Damage, session.Current.BattleScene!.Reaction.Kind);
        Assert.DoesNotContain(ConstructionRolls(action).TakeWhile(row => row.Kind != "rng-double"),
            row => row.Kind == "rng-curse-recoil");
        if (enemyHp == 15)
        {
            Assert.DoesNotContain(ConstructionRolls(action), row => row.Kind is "rng-double" or "rng-counter");
            FinishBattleScenes(session, action);
            Assert.Equal(0, session.Current.Battle.GetActor(Enemy).Hp);
            Assert.Equal(35, session.Current.Battle.GetActor(Attacker).Hp);
            Assert.Equal(1, session.Current.Battle.GetActor(Attacker).Kills!.Value);
        }
    }

    [Fact]
    public void SurvivingRecoilPrecedesFollowupRollsAndDoesNotEarnExperience()
    {
        var session = CursedBattle(1);
        var action = Attack(session, Enemy);
        Assert.Equal(new[] { "rng-curse-inaction", "rng-dodge", "rng-critical", "rng-spread-1", "rng-spread-2",
            "rng-curse-recoil", "rng-double", "rng-counter", "rng-exp-plus", "rng-exp-minus" },
            ConstructionRolls(action).Select(row => row.Kind));
        Assert.DoesNotContain(action.Observations, row => row.Kind == "curse-recoil");
        Assert.Equal(35, session.Current.Battle.GetActor(Attacker).Hp);
        var done = FinishBattleScenes(session, action);
        var recoil = Assert.Single(done.Observations, row => row.Kind == "curse-recoil");
        Assert.Equal((Attacker, Attacker, 35L, 31L), (recoil.Actor!.Value, recoil.Target!.Value, recoil.Before!.Value, recoil.After!.Value));
        Assert.Equal(31, session.Current.Battle.GetActor(Attacker).Hp);
        Assert.Equal(32, session.Current.Battle.GetActor(Enemy).Hp);
        Assert.Equal(11, session.Current.Battle.GetActor(Attacker).Exp!.Value);
        Assert.Equal(48, done.Observations.Count(row => row.Kind is "rng-reaction-x" or "rng-reaction-y"));
        Assert.Equal(Next, session.Current.Selection!.Actor);
    }

    [Theory]
    [InlineData(1, 4, 32, 1)]
    [InlineData(4693, 4, 13, 2)]
    public void LethalRecoilEndsFirstOrSecondStrikeAndDeliversDeathOnce(int seed, int hp, int remainingEnemyHp, int strikes)
    {
        var session = CursedBattle(seed, hp);
        var action = Attack(session, Enemy);
        var rolls = ConstructionRolls(action).ToArray();
        Assert.Equal("rng-curse-recoil", rolls[^1].Kind);
        Assert.Equal(strikes == 2 ? new ushort?[] { 0 } : [], rolls.Where(row => row.Kind == "rng-counter")
            .Select(row => row.RandomValue));
        var done = FinishBattleScenes(session, action);
        Assert.Equal(strikes, done.Observations.Count(row => row.Kind is "physical-first" or "physical-second"));
        Assert.Single(done.Observations, row => row.Kind == "curse-recoil" && row.Actor == Attacker && row.After == 0);
        Assert.Single(done.Observations, row => row.Kind == "death-cleanup" && row.Actor == Attacker);
        Assert.DoesNotContain(done.Observations, row => row.Kind is "physical-counter" or "exp" or "gold");
        var fallen = session.Current.Battle.GetActor(Attacker);
        Assert.Equal(0, fallen.Hp);
        Assert.Null(fallen.Position);
        Assert.Equal(1, fallen.Defeats!.Value);
        Assert.Equal(0, fallen.Status);
        Assert.Equal(remainingEnemyHp, session.Current.Battle.GetActor(Enemy).Hp);
        Assert.Equal(Next, session.Current.Selection!.Actor);
    }

    [Fact]
    public void CursedAllyCounterCanRecoilFatallyWithoutAnOpeningCurseRoll()
    {
        var battle = CursedBattle(1, hp: 3).Current.Battle.With(mainSeed: 192u << 16 | 0x1234u);
        var (after, effects) = PhysicalBattleAction.ResolveSourceDefault(battle, Enemy, new(2, 3), Attacker);
        Assert.Equal(new[] { "physical-first", "physical-counter", "curse-recoil" },
            effects.Where(row => row.Kind.StartsWith("physical-", StringComparison.Ordinal) || row.Kind == "curse-recoil")
                .Select(row => row.Kind));
        Assert.DoesNotContain(effects, row => row.Kind is "rng-curse-inaction" or "rng-exp-plus" or "exp" or "gold");
        Assert.Equal("rng-curse-recoil", effects.Last(row => row.RandomRange is not null).Kind);
        Assert.Equal(0, after.GetActor(Attacker).Hp);
        Assert.Null(after.GetActor(Attacker).Position);
        Assert.Equal(1, after.GetActor(Attacker).Defeats!.Value);
        Assert.Equal(47, after.GetActor(Enemy).Hp);
    }

    private static GameSession CursedBattle(int seed, int hp = 35, int enemyHp = 60) => Start("stone-court", doc =>
    {
        var start = doc["start"]!;
        start["mainSeed"] = ((uint)seed << 16) | 0x1234u;
        start["actors"]![0]!["status"] = "curse";
        start["actors"]![0]!["hp"] = hp;
        start["actors"]![2]!["hp"] = enemyHp;
        doc["actors"]![2]!["maxHp"] = enemyHp;
    });

    private static SessionResult Attack(GameSession session, ActorRef target)
    {
        Accept(session, new Confirm());
        Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(target));
        return Accept(session, new Confirm());
    }
}
