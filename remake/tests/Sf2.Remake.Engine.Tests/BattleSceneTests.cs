using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleSceneTests
{
    [Fact]
    public void ExperienceTextPrecedesGrowthRngAndAllLevelMessagesRetainControl()
    {
        var admitted = Admitted("stone-court");
        var definition = admitted.Definition.Encounters.Values.Single();
        var actor = definition.Deployments[0].Actor;
        var growth = new BattleGrowthDefinition(0, 0,
            Enumerable.Repeat(new StatGrowth(1, 31, Enumerable.Repeat(new StatGrowthFraction(128, 16), 30).ToArray()), 5).ToArray(),
            [], new Dictionary<byte, SpellRef>());
        var encounter = new BattleDefinition(definition.Encounter, definition.Map, definition.Width, definition.Height, definition.Terrain,
            definition.Deployments.Select(row => row.Actor == actor ? row with { Definition = row.Definition.WithGrowth(growth) } : row),
            definition.Spells.Values, definition.Rewards);
        var input = admitted.Start;
        var session = Assert.IsType<SessionStarted>(GameSession.Start(new ScenarioDefinition("scene-growth", [encounter]),
            new BattleStartInput(input.Encounter, input.Actors.Select(row => row.Actor == actor ? row with { Exp = 99 } : row),
                input.MainSeed, input.ThinkingSeed, input.Gold))).Session;
        byte level = session.Current.Battle.GetActor(actor).Level;
        ushort hp = session.Current.Battle.GetActor(actor).Hp;
        SelectAttack(session, new("raider")); Accept(session, new Confirm());
        while (session.Current.BattleScene!.Phase != BattleScenePhase.RewardMessage) Step(session);
        Assert.Equal(122, session.Current.Battle.GetActor(actor).Exp!.Value);
        Assert.Equal(level, session.Current.Battle.GetActor(actor).Level);
        uint beforeGrowth = session.Current.Battle.MainSeed;
        var grown = Step(session);
        Assert.Equal(BattleScenePhase.GrowthMessage, session.Current.BattleScene!.Phase);
        Assert.Equal(BattleGrowthNoticeKind.Level, session.Current.BattleScene.GrowthNotice!.Kind);
        Assert.Equal(level + 1, session.Current.Battle.GetActor(actor).Level);
        Assert.Equal(22, session.Current.Battle.GetActor(actor).Exp!.Value);
        Assert.Equal(hp, session.Current.Battle.GetActor(actor).Hp);
        Assert.Equal(10, grown.Observations.Count(row => row.Kind.StartsWith("rng-growth-", StringComparison.Ordinal)));
        Assert.NotEqual(beforeGrowth, session.Current.Battle.MainSeed);
        while (session.Current.BattleScene?.Phase == BattleScenePhase.GrowthMessage)
        {
            var before = session.Current;
            Assert.NotNull(Send(session, new Confirm()).Failure);
            Assert.Same(before, session.Current);
            Assert.False(session.Current.HasBattleControl);
            Step(session);
        }
        while (session.Current.BattleScene is not null) Step(session);
        Assert.Equal(new ActorRef("lookout"), session.Current.Selection!.Actor);
    }

    [Theory]
    [InlineData(2, "physical-first,physical-second", 500, 456)]
    [InlineData(55, "physical-first,physical-counter", 493, 478)]
    [InlineData(73, "physical-first,physical-second,physical-counter", 493, 453)]
    public void FollowupsExecuteInSourceOrderWithoutConsumingTheCounterActorsTurn(int seed, string expected, int hp, int targetHp)
    {
        var session = Start("stone-court", document =>
        {
            document["start"]!["mainSeed"] = ((uint)seed << 16) | 0x1234u;
            foreach (int index in new[] { 0, 2 })
            {
                document["start"]!["actors"]![index]!["hp"] = 500;
                document["actors"]![index]!["maxHp"] = 500;
            }
            document["actors"]![2]!["attack"] = 18;
        });
        SelectAttack(session, new("raider")); Accept(session, new Confirm());
        int cursor = session.Current.Battle.Cursor;
        var order = new List<string>();
        var phases = new List<BattleScenePhase>();
        int hpWrites = 0, draws = 0;
        uint carriedSeed = session.Current.Battle.MainSeed;
        while (session.Current.BattleScene is { } scene)
        {
            phases.Add(scene.Phase);
            Assert.Equal(cursor, session.Current.Battle.Cursor);
            if (scene.Phase == BattleScenePhase.ActionMessage) order.Add(scene.ActionKind);
            if (scene.Phase == BattleScenePhase.Reaction)
            {
                Assert.Equal(12, scene.Motion.Count);
                Assert.Equal(0, session.Current.Battle.GetActor(new("swordsman")).Exp!.Value);
            }
            var result = Step(session);
            foreach (var roll in result.Observations.Where(row => row.Kind.StartsWith("rng-reaction-", StringComparison.Ordinal)))
            {
                Assert.Equal((long)carriedSeed, roll.Before);
                carriedSeed = checked((uint)roll.After!.Value);
            }
            Assert.Equal(carriedSeed, result.Snapshot.Battle.MainSeed);
            hpWrites += result.Observations.Count(row => row.Kind == "hp");
            draws += result.Observations.Count(row => row.Kind.StartsWith("rng-reaction-", StringComparison.Ordinal));
        }
        Assert.Equal(expected.Split(','), order);
        Assert.Equal(order.Count, hpWrites);
        Assert.Equal(order.Count * 24, draws);
        Assert.Single(phases, phase => phase == BattleScenePhase.Reward);
        Assert.Equal(hp, session.Current.Battle.GetActor(new("swordsman")).Hp);
        Assert.Equal(targetHp, session.Current.Battle.GetActor(new("raider")).Hp);
        Assert.Equal(new ActorRef("lookout"), session.Current.Selection!.Actor);
        Assert.Equal(0xBEEF0042u, session.Current.Battle.ThinkingSeed);
    }

    [Theory]
    [InlineData("raider", 119u)]
    [InlineData("scavenger", 120u)]
    public void ConstructionReactionRewardAndReleaseHaveDistinctPersistentBoundaries(string target, uint gold)
    {
        var session = Start("stone-court");
        var actor = session.Current.Selection!.Actor;
        var victim = new ActorRef(target);
        int cursor = session.Current.Battle.Cursor;
        SelectAttack(session, victim);
        var prepared = Accept(session, new Confirm());
        Assert.Equal(BattleScenePhase.Initialize, prepared.Snapshot.BattleScene!.Phase);
        Assert.Equal(gold, session.Current.Battle.Gold);
        Assert.Equal(15, session.Current.Battle.GetActor(victim).Hp);
        Assert.Equal(0, session.Current.Battle.GetActor(actor).Exp!.Value);
        Assert.False(session.Current.HasBattleControl);
        Assert.DoesNotContain(prepared.Observations, row => row.Kind is "hp" or "action-committed" or "battle-outcome");

        Step(session); // initialized, ready to show action message
        Step(session); // acknowledged message, animation must run
        Assert.Equal(15, session.Current.Battle.GetActor(victim).Hp);
        var reaction = Step(session);
        Assert.Equal(BattleScenePhase.Reaction, session.Current.BattleScene!.Phase);
        Assert.Equal(0, session.Current.Battle.GetActor(victim).Hp);
        Assert.Equal(12, session.Current.BattleScene.Motion.Count);
        Assert.Equal(24, reaction.Observations.Count(row => row.Kind.StartsWith("rng-reaction-", StringComparison.Ordinal)));
        Assert.All(reaction.Observations.Where(row => row.RandomRange is not null), row => Assert.Equal((ushort)7, row.RandomRange));
        Assert.Equal(0, session.Current.Battle.GetActor(actor).Exp!.Value);
        Assert.Equal(0, session.Current.Battle.GetActor(actor).Kills!.Value);
        Assert.Equal(cursor, session.Current.Battle.Cursor);

        Step(session); // reaction consumed, damage message remains
        Step(session); // damage message acknowledged; death message remains
        Assert.Equal(BattleScenePhase.DeathMessage, session.Current.BattleScene!.Phase);
        Assert.Equal(0, session.Current.Battle.GetActor(actor).Exp!.Value);
        Step(session); // giveExp runs after death message
        Assert.Equal(BattleScenePhase.Reward, session.Current.BattleScene!.Phase);
        Assert.Equal(23, session.Current.Battle.GetActor(actor).Exp!.Value);
        Assert.Equal(0, session.Current.Battle.GetActor(actor).Kills!.Value);
        Assert.Equal(cursor, session.Current.Battle.Cursor);
        Step(session); // reward consumer finished
        Step(session); // reward message acknowledged; gold text remains
        Assert.Equal(BattleScenePhase.GoldMessage, session.Current.BattleScene!.Phase);
        Step(session); // gold message acknowledged, scene end still blocks
        Assert.Equal(BattleScenePhase.End, session.Current.BattleScene!.Phase);
        Assert.False(session.Current.HasBattleControl);
        var complete = Step(session);
        Assert.Null(session.Current.BattleScene);
        Assert.Equal(new ActorRef("lookout"), session.Current.Selection!.Actor);
        Assert.Equal(1, session.Current.Battle.GetActor(actor).Kills!.Value);
        Assert.Contains(complete.Observations, row => row.Kind == "scene-ended");
        Assert.Contains(complete.Observations, row => row.Kind == "action-committed");
    }

    [Fact]
    public void WrongDuplicateAndStaleCompletionsCannotSkipEffectsOrReleaseInput()
    {
        var session = Start("stone-court");
        SelectAttack(session, new("raider"));
        Accept(session, new Confirm());
        var initial = session.Current;
        foreach (SessionCommand command in new SessionCommand[]
        {
            new Confirm(), new Cancel(), new Move(ExplorationDirection.East), new AdvanceSimulation(),
            new Acknowledge(initial.BattleScene!.Token),
            new CompletePresentation(new WaitToken(initial.BattleScene.Token.Value + 1), PresentationCueKind.BattleLoad),
            new CompletePresentation(initial.BattleScene.Token, PresentationCueKind.Gesture),
        })
        {
            Assert.NotNull(Send(session, command).Failure);
            Assert.Same(initial, session.Current);
        }
        var completion = new CompletePresentation(initial.BattleScene!.Token, initial.BattleScene.CompletionKind);
        var envelope = new CommandEnvelope(initial.SessionId, initial.Revision, null, completion);
        Assert.Null(session.Submit(envelope).Failure);
        var message = session.Current;
        Assert.NotNull(session.Submit(envelope).Failure);
        Assert.NotNull(Send(session, completion).Failure);
        Assert.Same(message, session.Current);
        Assert.Equal(15, session.Current.Battle.GetActor(new("raider")).Hp);
    }

    [Fact]
    public void UnsupportedRewardFailsBeforeConstructionPublishesGoldOrDamage()
    {
        var session = Start("stone-court", document => document["start"]!["actors"]![0]!["exp"] = 99);
        SelectAttack(session, new("raider"));
        var before = session.Current;
        Assert.Equal("level-up", Send(session, new Confirm()).Failure!.Code);
        Assert.Same(before, session.Current);
        Assert.Equal(100u, before.Battle.Gold);
        Assert.Equal(15, before.Battle.GetActor(new("raider")).Hp);
    }

    private static void SelectAttack(GameSession session, ActorRef target)
    {
        Accept(session, new Confirm());
        Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(target));
    }

    private static SessionResult Step(GameSession session)
    {
        var scene = session.Current.BattleScene!;
        return Accept(session, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) :
            new CompletePresentation(scene.Token, scene.CompletionKind));
    }
}
