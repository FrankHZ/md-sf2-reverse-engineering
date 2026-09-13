using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class EngineSessionTests
{
    // Nine natural RNG draws per round (three ordinary actors). These independent scalar expectations
    // are not obtained from the production RNG or reset into a running session.
    [Theory]
    [InlineData("practice-yard", "medic-a", 2, 0x0A061234u, 0x9E581234u, 100, 17, 10)]
    [InlineData("practice-yard", "medic-a", 5, 0x41571234u, 0x22D11234u, 100, 17, 10)]
    [InlineData("garden-watch", "herbalist-b", 2, 0x0A061234u, 0x9E581234u, 20, 9, 15)]
    public void NaturalHistoriesHealThroughTheSameContentAndSessionPath(string package, string healer,
        int round, uint beforeSeed, uint afterSeed, int hp, int mp, int exp)
    {
        var session = Start(package);
        var history = new List<SessionObservation>();
        var actor = new ActorRef(healer);
        for (int action = 0; session.Current.Battle.Round != round || session.Current.Selection?.Actor != actor; action++)
        {
            Assert.True(action < 20, "Player control must naturally return within the authored history.");
            history.AddRange(Stay(session).Observations);
        }
        Assert.Contains(history, o => o.Kind == "ai-stay");
        Assert.Contains(history, o => o.Kind == "round-started" && o.After == round);
        var before = session.Current;
        Assert.Equal(beforeSeed, before.Battle.MainSeed);
        var spell = before.Battle.GetActor(actor).Definition.Spells.Single();
        Accept(session, new Confirm());
        Accept(session, new SelectSpell(spell));
        Accept(session, new SelectTarget(actor));
        Assert.Same(before.Battle, session.Current.Battle);
        var result = Accept(session, new Confirm());
        var healed = result.Snapshot.Battle.GetActor(actor);
        Assert.Equal((hp, mp, exp), ((int)healed.Hp, (int)healed.Mp, (int)healed.Exp));
        Assert.Equal(afterSeed, result.Snapshot.Battle.MainSeed);
        Assert.Equal(before.Battle.ThinkingSeed, result.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(new[] { "mp", "hp", "exp", "action-rng", "action-committed" },
            result.Observations.Take(5).Select(o => o.Kind));
        Assert.Equal(((long)beforeSeed, (long)afterSeed),
            (result.Observations[3].Before!.Value, result.Observations[3].After!.Value));
        Assert.Equal(SessionStopReason.PlayerInput, result.StopReason);
        Assert.NotEqual(actor, result.Snapshot.Selection!.Actor);
        Assert.Equal(Enumerable.Range(1, result.Observations.Count).Select(i => result.Observations[0].Sequence - 1 + i),
            result.Observations.Select(o => o.Sequence));
        Assert.Equal((ushort)(package == "practice-yard" ? 95 : 8), before.Battle.GetActor(actor).Hp);
    }

    [Theory]
    [InlineData("practice-yard")]
    [InlineData("garden-watch")]
    public void CancelDiscardsMovementAndTargetChoiceWithoutGameplayOrRngMutation(string package)
    {
        var session = Start(package);
        var before = session.Current;
        var actor = before.Selection!.Actor;
        Accept(session, new Move(ExplorationDirection.East));
        Assert.NotEqual(before.Battle.GetActor(actor).Position, session.Current.Selection!.Preview.Destination);
        Accept(session, new Confirm());
        Accept(session, new SelectSpell(before.Battle.GetActor(actor).Definition.Spells.Single()));
        Accept(session, new SelectTarget(actor));
        var result = Accept(session, new Cancel());
        Assert.Same(before.Battle, result.Snapshot.Battle);
        Assert.Equal(before.Battle.GetActor(actor).Position, result.Snapshot.Selection!.Preview.Destination);
        Assert.Equal(BattleSelectionStage.Movement, result.Snapshot.Selection.Stage);
        Assert.Null(result.Snapshot.Selection.Spell);
        Assert.True(result.Snapshot.Revision > before.Revision);
    }

    [Fact]
    public void StayCommitsProvisionalMovementThenAutomaticallyRunsAiAndTheNextRound()
    {
        var session = Start();
        var before = session.Current;
        var actor = before.Selection!.Actor;
        Accept(session, new Move(ExplorationDirection.East));
        Accept(session, new Confirm());
        Assert.Same(before.Battle, session.Current.Battle);
        Accept(session, new ChooseAction(SessionAction.Stay));
        var committed = Accept(session, new Confirm());
        Assert.Equal(new MapPosition(4, 3), committed.Snapshot.Battle.GetActor(actor).Position);
        Assert.Equal(new[] { "movement", "action-committed" }, committed.Observations.Take(2).Select(o => o.Kind));
        Assert.Equal(before.Battle.MainSeed, committed.Snapshot.Battle.MainSeed);
        var next = Stay(session);
        Assert.Equal(2, next.Snapshot.Battle.Round);
        Assert.Equal(actor, next.Snapshot.Selection!.Actor);
        Assert.Contains(committed.Observations.Concat(next.Observations), o => o.Kind == "ai-stay");
        Assert.Equal(0x0A061234u, next.Snapshot.Battle.MainSeed);
    }

    [Theory]
    [InlineData("revision")]
    [InlineData("session")]
    [InlineData("actor")]
    public void InvalidEnvelopeNeverPublishesState(string mismatch)
    {
        var session = Start();
        var before = session.Current;
        var result = session.Submit(new(mismatch == "session" ? Guid.NewGuid() : before.SessionId,
            mismatch == "revision" ? before.Revision - 1 : before.Revision,
            mismatch == "actor" ? new ActorRef("dummy-a") : before.Selection!.Actor, new Confirm()));
        Assert.Equal(mismatch == "actor" ? "wrong-actor" : "stale-input", result.Failure!.Code);
        Assert.Equal(SessionFailureKind.IllegalCommand, result.Failure.Kind);
        Assert.Same(before, session.Current);
        Assert.Same(before, result.Snapshot);
        Assert.Empty(result.Observations);
    }

    [Fact]
    public void InsufficientMpAndOutOfRangeArePreciseAtomicRejections()
    {
        var poor = Start(change: document => document["actors"]![0]!["mp"] = 2);
        Accept(poor, new Confirm());
        AssertRejectedWithoutMutation(poor, new SelectSpell(new("mend", 1)), "insufficient-mp", SessionFailureKind.IllegalCommand);
        var distant = Start();
        Accept(distant, new Confirm());
        Accept(distant, new SelectSpell(new("mend", 1)));
        AssertRejectedWithoutMutation(distant, new SelectTarget(new("guard-a")), "target-range", SessionFailureKind.IllegalCommand);
        AssertRejectedWithoutMutation(distant, new SelectTarget(new("missing")), "invalid-heal-target", SessionFailureKind.IllegalCommand);
    }

    [Fact]
    public void UnsupportedLevelUpRollsBackMovementCostHealingExperienceAndSeeds()
    {
        var session = Start(change: document => document["actors"]![0]!["exp"] = 99);
        Accept(session, new Move(ExplorationDirection.East));
        Accept(session, new Confirm());
        Accept(session, new SelectSpell(new("mend", 1)));
        Accept(session, new SelectTarget(new("medic-a")));
        AssertRejectedWithoutMutation(session, new Confirm(), "level-up", SessionFailureKind.UnsupportedCapability);
        Assert.Equal(new MapPosition(3, 3), session.Current.Battle.GetActor(new("medic-a")).Position);
        Assert.Equal((ushort)95, session.Current.Battle.GetActor(new("medic-a")).Hp);
    }

    [Fact]
    public void PhysicalActionReportsItsCapabilityBoundary()
    {
        var session = Start();
        Accept(session, new Confirm());
        AssertRejectedWithoutMutation(session, new ChooseAction(SessionAction.PhysicalAttack),
            "physical-attack", SessionFailureKind.UnsupportedCapability);
    }

    [Fact]
    public void AutomaticAdvanceSkipsADeadQueuedEntryWithoutAHistoryPredicate()
    {
        var definition = Definition();
        var initial = BattleTurnFlow.Start(definition.Battle, definition.MainSeed, definition.ThinkingSeed);
        var battle = initial.With(actors: initial.Actors.Select(a => a.Actor.Value == "guard-a" ? a.With(hp: 0) : a),
            round: 7, queue: [new(9, 9), new(129, 7), new(7, 5), new(255, 255)]);
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 20, 30, battle, null, SessionStopReason.SimulationWait);
        var result = BattleAdvancer.Advance(snapshot, []); // Unit seam only; no runtime queue/state injection.
        Assert.Equal(new[] { "dead-entry-skipped", "ai-stay", "player-control" }, result.Observations.Select(o => o.Kind));
        Assert.Equal(new ActorRef("medic-a"), result.Snapshot.Selection!.Actor);
        Assert.Equal(2, result.Snapshot.Battle.Cursor);
        Assert.Equal(7, result.Snapshot.Battle.Round);
        Assert.Equal(definition.MainSeed, result.Snapshot.Battle.MainSeed);
    }

    private static void AssertRejectedWithoutMutation(GameSession session, SessionCommand command,
        string code, SessionFailureKind kind)
    {
        var before = session.Current;
        var result = Send(session, command);
        Assert.Equal(code, result.Failure!.Code);
        Assert.Equal(kind, result.Failure.Kind);
        Assert.Same(before, session.Current);
        Assert.Same(before, result.Snapshot);
        Assert.Empty(result.Observations);
    }
}
