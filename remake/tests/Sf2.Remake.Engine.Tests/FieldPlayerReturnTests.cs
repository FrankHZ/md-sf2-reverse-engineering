using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using System.Text.Json.Nodes;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldPlayerReturnTests
{
    [Theory]
    [InlineData("traveler", 96, false)]
    [InlineData("scout", 192, true)]
    [InlineData("entity-0", 128, false)]
    public void OutcomesRetainLivePlayerAcrossProgramsAndLaterTransfer(string id, ushort speed, bool direct)
    {
        foreach (var kind in new[] { BattleOutcomeKind.Victory, BattleOutcomeKind.Defeat, BattleOutcomeKind.Retreat })
        {
            var session = StartField(id, speed, direct);
            var entered = session.Current;
            Assert.True(entered.HasBattleControl);
            Assert.Equal(new FieldPlayerBinding(new(id), speed), entered.Story.FieldPlayer);
            var battle = entered.Battle;
            // Valid terminal battle states isolate projection from combat damage calculation.
            var terminal = battle.With(actors: battle.Actors.Select(actor =>
                kind == BattleOutcomeKind.Victory && !actor.IsAlly || kind == BattleOutcomeKind.Defeat && actor.IsAlly
                    ? actor.With(hp: 0) : actor));
            var snapshot = new SessionSnapshot(entered.SessionId, entered.Revision, entered.ObservationSequence,
                new ActiveBattle(terminal, null), entered.Story, SessionStopReason.SimulationWait);
            var result = kind == BattleOutcomeKind.Retreat
                ? BattleOutcome.ReturnFromRetreat(session.Definition, snapshot, [], session.Rules)
                : BattleOutcome.Begin(session.Definition, new(snapshot, [], snapshot.StopReason), session.Rules);
            Assert.Null(result.Failure);
            Assert.Equal(new EntityRef(id), result.Snapshot.Exploration!.Player);
            Assert.Equal(speed, result.Snapshot.Exploration.PlayerEntity.Motion.XSpeed);
            Assert.Equal(terminal.MainSeed, result.Snapshot.Exploration.Party.MainSeed);
            Assert.Equal(terminal.ThinkingSeed, result.Snapshot.Exploration.Party.ThinkingSeed);
            Assert.Equal(terminal.Gold, result.Snapshot.Exploration.Party.Gold);
            foreach (var actor in terminal.Actors.Where(actor => actor.IsAlly))
            {
                var carried = result.Snapshot.Exploration.Party.Actors.Single(row => row.Actor == actor.Actor);
                Assert.Equal(actor.Exp, carried.Exp);
                Assert.Equal(actor.Kills, carried.Kills);
                Assert.Equal(actor.Defeats, carried.Defeats);
                Assert.Equal(actor.Progress, carried.Progress);
                Assert.Equal(actor.SourceLoadout, carried.SourceLoadout);
            }
            var returned = Drain(session.Definition, result.Snapshot, session.Rules);
            Assert.True(returned.CanWaitAtInput);
            Assert.Equal(entered.SessionId, returned.SessionId);
            Assert.Null(returned.Story.EnteringBattle);
            Assert.Null(returned.Story.FieldPlayer);
            Assert.Contains(777, returned.Story.Flags);
            Assert.Equal(new EntityRef(id), returned.Exploration!.Player);
            Assert.Equal(speed, returned.Exploration.PlayerEntity.Motion.XSpeed);
            Assert.Equal(1, returned.Exploration.PlayerEntity.Motion.Facing);
            Assert.Equal(kind == BattleOutcomeKind.Defeat ? terminal.Gold / 2 : terminal.Gold,
                returned.Exploration.Party.Gold);
            Assert.Equal(terminal.MainSeed, returned.Exploration.Party.MainSeed);
            Assert.Equal(terminal.ThinkingSeed, returned.Exploration.Party.ThinkingSeed);
            Assert.Equal(kind == BattleOutcomeKind.Victory, returned.Story.Flags.Contains(502));
            var transferred = MapTransfer.Apply(session.Definition, returned, new("road"), new(2, 2), 1,
                MapLoadMode.Rebuild, returned.Story, [], session.Rules);
            var field = Drain(session.Definition, transferred, session.Rules).Exploration!;
            Assert.Equal(new EntityRef(id), field.Player);
            Assert.Equal(speed, field.PlayerEntity.Motion.XSpeed);
            Assert.Equal(speed, field.PlayerEntity.Motion.YSpeed);
            Assert.Equal(new MapPosition(2, 2), field.PlayerEntity.Position);
        }
    }

    [Theory]
    [InlineData("traveler", 96)]
    [InlineData("scout", 192)]
    public void PaidRetreatKeepsBindingThroughPaymentAndActualWaits(string id, ushort speed)
    {
        var session = StartField(id, speed, true);
        var before = session.Current.Battle;
        Accept(session, new Confirm());
        Accept(session, new SelectSpell(new("egress", 1)));
        Accept(session, new SelectTarget(new("medic-a")));
        var paid = FinishMovement(session, Accept(session, new Confirm()));
        Assert.Contains(paid.Observations, row => row.Kind == "retreat-mp-paid" && row.After == before.GetActor(new("medic-a")).Mp - 8);
        while (session.Current.BattleRetreat is { } retreat)
            Accept(session, retreat.Phase == BattleRetreatPhase.Text
                ? retreat.Revealed ? new Acknowledge(retreat.Token) : new CompleteTextReveal(retreat.Token)
                : new CompletePresentation(retreat.Token, retreat.CompletionKind));
        while (!session.Current.CanWaitAtInput)
        {
            var wait = session.Current.Story.Wait;
            Accept(session, wait is PresentationWait present ? new CompletePresentation(present.Token, present.Cue.Kind)
                : new AdvanceSimulation(wait?.Token));
        }
        Assert.Equal(new EntityRef(id), session.Current.Exploration!.Player);
        Assert.Equal(speed, session.Current.Exploration.PlayerEntity.Motion.XSpeed);
        Assert.Equal(before.Gold, session.Current.Exploration.Party.Gold);
        Assert.Equal(before.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(before.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        Assert.Null(session.Current.Story.FieldPlayer);
    }

    [Theory]
    [InlineData(BattleOutcomeKind.Victory)]
    [InlineData(BattleOutcomeKind.Defeat)]
    [InlineData(BattleOutcomeKind.Retreat)]
    public void SourcePopulationKeepsPhysicalPlayerAndEstablishedReturnSpeed(BattleOutcomeKind kind)
    {
        var session = StartField("entity-0", 192, true, source: true);
        Assert.Equal(new FieldPlayerBinding(new("entity-0"), 32), session.Current.Story.FieldPlayer);
        var result = Outcome(session, kind);
        Assert.Null(result.Failure);
        var returned = Drain(session.Definition, result.Snapshot, session.Rules);
        Assert.Equal(new EntityRef("entity-0"), returned.Exploration!.Player);
        Assert.Equal(0, returned.Exploration.PlayerEntity.Slot);
        Assert.Equal(32, returned.Exploration.PlayerEntity.Motion.XSpeed);
        Assert.NotNull(returned.Exploration.Population);
    }

    [Fact]
    public void FailedInitializationRetainsFieldPrefixWithoutPublishingBinding()
    {
        var session = StartField("scout", 192, true, invalidParty: true);
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        Assert.Equal(SessionStopReason.Faulted, session.Current.StopReason);
        Assert.Null(session.Current.Story.FieldPlayer);
        Assert.Equal(192, session.Current.Exploration!.PlayerEntity.Motion.XSpeed);
        Assert.NotNull(session.Current.Story.EnteringBattle);
    }

    [Fact]
    public void LaterFailureKeepsNamedPlayerCommittedReturnPrefixAndCursor()
    {
        var session = StartField("scout", 192, false, failReturn: true);
        var result = Outcome(session, BattleOutcomeKind.Victory);
        for (int count = 0; result.Failure is null && count < 500; count++)
        {
            var wait = result.Snapshot.Story.Wait;
            result = ExplorationDispatcher.Submit(session.Definition, result.Snapshot, wait switch {
                DialogueWait => new Acknowledge(wait.Token),
                PresentationWait present => new CompletePresentation(present.Token, present.Cue.Kind),
                _ => new AdvanceSimulation(wait?.Token) }, session.Rules);
        }
        Assert.Equal("program-opcode", result.Failure?.Code);
        Assert.Equal(new ProgramLocation("return", 4), result.Snapshot.Story.Cursor);
        Assert.Contains(777, result.Snapshot.Story.Flags);
        Assert.Contains(502, result.Snapshot.Story.Flags);
        Assert.Equal(new FieldPlayerBinding(new("scout"), 192), result.Snapshot.Story.FieldPlayer);
        Assert.Equal(new EntityRef("scout"), result.Snapshot.Exploration!.Player);
        Assert.Equal(1, result.Snapshot.Exploration.PlayerEntity.Motion.Facing);
        var stopped = ExplorationDispatcher.Submit(session.Definition, result.Snapshot, new AdvanceSimulation(), session.Rules);
        Assert.Same(result.Snapshot, stopped.Snapshot);
    }

    private static SessionResult Outcome(GameSession session, BattleOutcomeKind kind)
    {
        var entered = session.Current;
        var battle = entered.Battle.With(actors: entered.Battle.Actors.Select(actor =>
            kind == BattleOutcomeKind.Victory && !actor.IsAlly || kind == BattleOutcomeKind.Defeat && actor.IsAlly
                ? actor.With(hp: 0) : actor));
        var snapshot = new SessionSnapshot(entered.SessionId, entered.Revision, entered.ObservationSequence,
            new ActiveBattle(battle, null), entered.Story, SessionStopReason.SimulationWait);
        return kind == BattleOutcomeKind.Retreat
            ? BattleOutcome.ReturnFromRetreat(session.Definition, snapshot, [], session.Rules)
            : BattleOutcome.Begin(session.Definition, new(snapshot, [], snapshot.StopReason), session.Rules);
    }

    private static GameSession StartField(string id, ushort speed, bool direct, bool source = false,
        bool invalidParty = false, bool failReturn = false)
    {
        var document = Document();
        document["start"]!["gold"] = 101;
        document["start"]!["mainSeed"] = 0x12340000u + speed;
        document["start"]!["thinkingSeed"] = 0x98760000u + speed;
        document["start"]!["actors"]![0]!["exp"] = 17;
        document["start"]!["actors"]![0]!["kills"] = 4;
        document["start"]!["actors"]![0]!["defeats"] = 2;
        document["spells"] = JsonNode.Parse("""[{"id":"egress","level":1,"mpCost":8,"minimumRange":0,"maximumRange":0,"effect":{"kind":"retreat"}}]""");
        document["actors"]![0]!["spells"] = JsonNode.Parse("""[{"id":"egress","level":1}]""");
        var admitted = Assert.IsType<ScenarioReadAccepted>(Reader(document).Read());
        var d = admitted.Definition.Encounters[admitted.Start.Encounter];
        var encounter = new BattleDefinition(d.Encounter, d.Map, d.Width, d.Height, d.Terrain, d.Deployments,
            d.Spells.Values, d.Rewards, outcome: new(new("medic-a"), new("dummy-a")), retreatSpells: d.RetreatSpells.Values);
        var layout = new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]);
        var traversal = new OriginalMapTraversal([new(0, 0, 12, 12)]);
        var palette = new PalettePair(0x222, 0x444);
        var player = new EntityRef(id);
        var route = new ExplorationBattleRoute(d.Encounter, 402, 502, null, new("before", 0), null, Outcome:
            new(new("after", 0), 0, new("empty", 0), new("after", 0), new("return", 0),
                new("arena"), 3, new("camp"), new(1, 1), 0));
        ExplorationMapDefinition Map(string name, ExplorationBattleRoute? battle = null) =>
            new(new(name), layout, traversal, [], [], battle: battle, basePalette: palette,
                population: source ? new(1, 1, 0, []) : null);
        var programs = new StoryProgram[] {
            new("enter", [new TransferToMap(new("arena"), new(1, 1), 0, MapLoadMode.Rebuild), new EndProgram()]),
            // The initial speed differs: capture must follow legitimate pre-battle entity work.
            new("before", [new StartEntityMotion(player, new([new SetEntitySpeed(speed, speed), new StopEntityActions()]), true), new EndProgram()]),
            new("after", [new StartEntityMotion(player, new([new FaceEntity(2), new MoveEntityRelative(1, 0), new StopEntityActions()]), true),
                new ShowText(TextDisplayMode.Single, player, TextId: 100), new EndProgram()]),
            new("return", [new ReturnBattleMap(), new StartEntityMotion(player, new([new FaceEntity(1), new StopEntityActions()]), true),
                new PresentCue(PresentationCueKind.Sound, "return"), new WriteFlag(777, true),
                failReturn ? new UnsupportedInstruction("late-return", "test") : new EndProgram()]),
            new("empty", [new EndProgram()]) };
        var definition = new ScenarioDefinition("field-binding", [encounter], exploration:
            new([Map("road"), Map("arena", route), Map("camp")], programs,
                [new KeyValuePair<int, string>(100, "Returned")], partyFlags: new(2, 0, 32, 2)), encounterStarts: [admitted.Start]);
        var party = invalidParty ? new BattleStartInput(d.Encounter,
            admitted.Start.Actors.Select(actor => actor with { Hp = ushort.MaxValue }),
            admitted.Start.MainSeed, admitted.Start.ThinkingSeed, admitted.Start.Gold) : admitted.Start;
        var start = new ExplorationStartInput(new(direct ? "arena" : "road"), player, new(1, 1), 0, 48,
            [399, 402], party, direct ? null : new("enter", 0));
        var session = Assert.IsType<SessionStarted>(GameSession.Start(definition, start)).Session;
        for (int count = 0; !session.Current.HasBattleControl && count < 100; count++)
        {
            var result = Send(session, new AdvanceSimulation(session.Current.Story.Wait?.Token));
            if (invalidParty && result.Failure is not null) return session;
            Assert.Null(result.Failure);
        }
        return session;
    }

    private static SessionSnapshot Drain(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules)
    {
        for (int count = 0; !current.CanWaitAtInput && count < 500; count++)
        {
            var wait = current.Story.Wait;
            SessionCommand command = wait switch {
                DialogueWait => new Acknowledge(wait.Token),
                PresentationWait present => new CompletePresentation(present.Token, present.Cue.Kind),
                _ => new AdvanceSimulation(wait?.Token) };
            var result = ExplorationDispatcher.Submit(definition, current, command, rules);
            Assert.Null(result.Failure);
            current = result.Snapshot;
        }
        return current;
    }
}
