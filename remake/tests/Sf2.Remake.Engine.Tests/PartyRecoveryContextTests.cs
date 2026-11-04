using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PartyRecoveryContextTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ResetProgramRecoversLiveResourcesAndStillEntersPoisonEnabledBattle(bool changed)
    {
        var control = Enter(reset: false, changed);
        var reset = Enter(reset: true, changed);
        Assert.True(control.Session.Current.HasBattleControl);
        Assert.True(reset.Session.Current.HasBattleControl);
        var a = control.Session.Current.Battle; var b = reset.Session.Current.Battle;
        Assert.Equal(a.MainSeed, b.MainSeed); Assert.Equal(a.ThinkingSeed, b.ThinkingSeed);
        Assert.Equal(a.Gold, b.Gold); Assert.Equal(a.DeathCredit, b.DeathCredit);
        Assert.Equal(new ActorRef(changed ? "guard-a" : "medic-a"), b.DeathCredit!.Ally);
        Assert.Equal(control.Session.Current.Story.Flags, reset.Session.Current.Story.Flags);
        Assert.Single(reset.Facts, row => row.Detail == nameof(ResetPartyBattleStats));
        Assert.DoesNotContain(control.Facts, row => row.Detail == nameof(ResetPartyBattleStats));
        foreach (var actor in a.Actors)
        {
            var recovered = b.GetActor(actor.Actor);
            Assert.Equal(actor.IsAlly ? actor.MaxHp : actor.Hp, recovered.Hp);
            Assert.Equal(actor.IsAlly ? actor.MaxMp : actor.Mp, recovered.Mp);
            Assert.Equal(actor.Exp, recovered.Exp); Assert.Equal(actor.Kills, recovered.Kills);
            Assert.Equal(actor.Defeats, recovered.Defeats); Assert.Equal(actor.Status, recovered.Status);
            Assert.Equal(actor.SourceLoadout!.Items, recovered.SourceLoadout!.Items);
        }
    }

    private static (GameSession Session, List<SessionObservation> Facts) Enter(bool reset, bool changed)
    {
        var session = Start("shop-supplies", d =>
        {
            var start = d["battle"]!["start"]!;
            foreach (var actor in start["actors"]!.AsArray()) actor!["status"] = "none";
            if (changed)
            {
                start["mainSeed"] = 0xA1250412u; start["thinkingSeed"] = 0x71020304u;
                start["gold"] = 713; start["firstAlly"] = "guard-a";
                start["actors"]![0]!["hp"] = 27; start["actors"]![0]!["mp"] = 4;
                start["actors"]![0]!["exp"] = 37; start["actors"]![0]!["kills"] = 11;
                start["actors"]![0]!["defeats"] = 2; start["actors"]![2]!["hp"] = 19;
            }
            if (reset) d["world"]!["programs"]!.AsArray().Single(p => (string?)p!["id"] == "before-battle")!
                ["instructions"]!.AsArray().Insert(0, new JsonObject { ["op"] = "reset-party-battle-stats" });
        });
        List<SessionObservation> facts = [];
        foreach (var direction in new[] { ExplorationDirection.South, ExplorationDirection.East,
            ExplorationDirection.East, ExplorationDirection.East, ExplorationDirection.North })
        {
            facts.AddRange(Accept(session, new Move(direction)).Observations);
            for (int i = 0; i < 400 && session.Current.Mode == SessionMode.Exploration && !session.Current.CanWaitAtInput; i++)
            {
                var wait = session.Current.Story.Wait;
                facts.AddRange(Accept(session, wait is DialogueWait ? new Acknowledge(wait.Token) :
                    new AdvanceSimulation(wait?.Token, 1)).Observations);
            }
        }
        return (session, facts);
    }

    [Theory]
    [InlineData(false, 1)] [InlineData(false, 7)] [InlineData(false, 28)]
    [InlineData(true, 1)] [InlineData(true, 7)] [InlineData(true, 28)]
    public void RecoveryPreservesChangedMembershipAccountingAndRetainedContext(bool all, int member)
    {
        var session = Start("shop-supplies"); var admitted = session.Current.Exploration!.Party;
        var original = session.Definition.Encounters.Values.Single();
        var encounter = original.WithDeployments(original.Deployments.Select(row => row.Actor.Value == "guard-a"
            ? row with { Initialization = new(null, 0, 0, 0, 0, AllyPartyMember: member) } : row));
        var definition = new ScenarioDefinition("recovery", [encounter]);
        // Reordered, smaller live party with a dead member, depleted MP and nondefault accounting.
        var actors = admitted.Actors.Where(a => a.Actor.Value != "scout-a").Reverse().Select(a => a with
        {
            Status = 0, Hp = a.Actor.Value == "guard-a" ? (ushort)0 : (ushort)1,
            Mp = 0, Exp = 31, Kills = 9, Defeats = 4,
        });
        var party = Context(admitted, actors);
        var healed = OutcomeReturnRules.Heal(new Sf2OutcomeReturnPolicy(), definition, party, all);
        SameContext(party, healed);
        foreach (var before in party.Actors)
        {
            var after = healed.Actors.Single(a => a.Actor == before.Actor);
            bool ally = encounter.Deployments.Single(d => d.Actor == before.Actor).Faction == BattleFaction.Ally;
            bool recovers = ally && (all || before.Hp > 0 || member is 7 or 28);
            var actor = encounter.Deployments.Single(d => d.Actor == before.Actor).Definition;
            Assert.Equal(recovers ? actor.MaxHp : before.Hp, after.Hp);
            Assert.Equal(recovers ? actor.MaxMp : before.Mp, after.Mp);
            Assert.Equal(before, after with { Hp = before.Hp, Mp = before.Mp });
        }
    }

    [Theory]
    [InlineData(2)] [InlineData(0x300)] [InlineData(4)]
    public void UnsupportedRecoveryStatusKeepsTheWholePartyAndProgramCursor(int status)
    {
        var session = Start("shop-supplies"); var current = session.Current; var world = current.Exploration!;
        var party = Context(world.Party, world.Party.Actors.Select(a => a with
            { Status = a.Actor.Value == "guard-a" ? (ushort)status : (ushort)0 }));
        var programs = session.Definition.Exploration!;
        var program = new StoryProgram("reset-only", [new ResetPartyBattleStats(), new EndProgram()]);
        var exploration = new ExplorationDefinition(programs.Maps.Values, programs.Programs.Values.Append(program), programs.Texts);
        var definition = new ScenarioDefinition("reset", session.Definition.Encounters.Values, exploration: exploration);
        var input = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world.WithParty(party)), current.Story.Copy(new(program.Id, 0)), SessionStopReason.SimulationWait);
        var result = ProgramRunner.Run(definition, input, [], session.Rules);
        Assert.Equal("return-status-refresh", result.Failure!.Code);
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure.Kind);
        Assert.Same(input.Active, result.Snapshot.Active); Assert.Same(party, result.Snapshot.Exploration!.Party);
        Assert.Equal(input.Story.Cursor, result.Snapshot.Story.Cursor); Assert.Empty(result.Observations);
    }

    [Fact]
    public void JoiningNewMemberPreservesExistingLiveResourcesAndAllContextOnRetry()
    {
        var session = Start("jaro-victory"); var d = session.Definition; var world = session.Current.Exploration!;
        var initial = world.Party;
        var party = Context(initial, initial.Actors.Where(a => a.Actor.Value != "ally-jaro").Select(a => a with { Hp = 3 }));
        var definition = new ScenarioDefinition("joining", d.Encounters.Values, exploration: d.Exploration,
            encounterStarts: [initial]);
        SessionSnapshot Input(SessionSnapshot current, BattleStartInput resources) => new(current.SessionId,
            current.Revision, current.ObservationSequence, new ActiveExploration(world.WithParty(resources)),
            current.Story.Copy(new("after", 0)), SessionStopReason.SimulationWait);
        var result = ProgramRunner.Run(definition, Input(session.Current, party), [], session.Rules);
        Assert.Null(result.Failure); var joined = result.Snapshot.Exploration!.Party;
        SameContext(party, joined); Assert.Equal(party.Actors.Count + 1, joined.Actors.Count);
        Assert.All(party.Actors, a => Assert.Equal(a, joined.Actors.Single(b => b.Actor == a.Actor)));
        Assert.Equal(initial.Actors.Single(a => a.Actor.Value == "ally-jaro"), joined.Actors.Last());
        var repeated = ProgramRunner.Run(definition, Input(result.Snapshot, joined), [], session.Rules);
        Assert.Null(repeated.Failure); Assert.Same(joined, repeated.Snapshot.Exploration!.Party);
    }

    [Theory]
    [InlineData(0x8005)] [InlineData(0x8001)]
    public void OutcomeAiClearChangesOnlyTheSelectedActivationBit(int word)
    {
        var session = Start("jaro-victory"); var current = session.Current; var world = current.Exploration!;
        var party = Context(world.Party, world.Party.Actors.Select(a => a.Actor.Value == "ally-jaro"
            ? a with { ActivationWord = (ushort)word } : a));
        var route = world.Definition.Battles.Single();
        var story = current.Story.Copy(null, enteringBattle: route, continuation: ProgramContinuation.VictoryProgramFinished);
        var input = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world.WithParty(party)), story, SessionStopReason.SimulationWait);
        var result = OutcomePartyOperations.Apply(session.Definition, input, new ClearOutcomePartyAi(23), story);
        var after = Assert.IsType<ActiveExploration>(result.Active).World.Party;
        SameContext(party, after);
        Assert.Equal(party.Actors.Select(a => a.Actor.Value == "ally-jaro" ? a with { ActivationWord = (ushort)(word & ~4) } : a), after.Actors);
    }

    [Fact]
    public void DefeatRecoveryChangesOnlyLeaderVitalsAndGoldWithinTheSameContext()
    {
        var session = Start("jaro-victory"); var world = session.Current.Exploration!;
        var party = Context(world.Party, world.Party.Actors.Select(a => a with { Hp = 0, Status = 0 }));
        var story = session.Current.Story.Copy(null, enteringBattle: world.Definition.Battles.Single(),
            continuation: ProgramContinuation.DefeatProgramFinished);
        var finish = OutcomeReturnRules.Finish(new Sf2OutcomeReturnPolicy(), session.Definition, story, party);
        SameContext(party, finish.Recovered, gold: 359);
        SameContext(party, finish.Returned, gold: 359);
        Assert.Equal(40, finish.Recovered.Actors.Single(a => a.Actor.Value == "leader").Hp);
        Assert.Equal(0, finish.Returned.Actors.Single(a => a.Actor.Value == "ally-jaro").Hp);
        Assert.Equal(party.Actors.Select(a => a.Actor.Value == "leader" ? a with { Hp = 40 } : a), finish.Recovered.Actors);
    }

    [Theory]
    [InlineData("flags")] [InlineData("visuals")] [InlineData("retained")] [InlineData("credit")]
    public void SelectedRecoveryPolicyCannotPublishUnownedContextChanges(string field)
    {
        var session = Start("shop-supplies"); var current = session.Current; var world = current.Exploration!;
        var party = Context(world.Party, world.Party.Actors.Select(a => a with { Status = 0 }));
        var exploration = session.Definition.Exploration!;
        var program = new StoryProgram("reset-policy", [new ResetPartyBattleStats(), new EndProgram()]);
        var definition = new ScenarioDefinition("policy", session.Definition.Encounters.Values,
            exploration: new ExplorationDefinition(exploration.Maps.Values, exploration.Programs.Values.Append(program), exploration.Texts));
        var rules = new SessionRules("bad-recovery", session.Rules.Healing,
            RuleCompositions.SourcePhysical(), RuleCompositions.SourceItem(), RuleCompositions.SourceStay(),
            outcomeReturn: new ContextChangingRecovery(field));
        var input = new SessionSnapshot(current.SessionId, current.Revision, current.ObservationSequence,
            new ActiveExploration(world.WithParty(party)), current.Story.Copy(new(program.Id, 0)), SessionStopReason.SimulationWait);
        var result = ProgramRunner.Run(definition, input, [], rules);
        Assert.Equal(SessionFailureKind.InvariantFailure, result.Failure!.Kind);
        Assert.Same(input.Active, result.Snapshot.Active); Assert.Same(party, result.Snapshot.Exploration!.Party);
        Assert.Equal(input.Story.Cursor, result.Snapshot.Story.Cursor); Assert.Empty(result.Observations);
    }

    private sealed class ContextChangingRecovery(string field) : IOutcomeReturnPolicy
    {
        private readonly Sf2OutcomeReturnPolicy _source = new();
        public string Identity => "context-changing-recovery";
        public OutcomeStart Start(ScenarioDefinition definition, EngineBattleState battle, StoryState story,
            BattleOutcomeKind kind, BattleStartInput party) => _source.Start(definition, battle, story, kind, party);
        public OutcomeFinish Finish(ScenarioDefinition definition, StoryState story, BattleStartInput party) =>
            _source.Finish(definition, story, party);
        public BattleStartInput Heal(ScenarioDefinition definition, BattleStartInput party, bool all)
        {
            var healed = _source.Heal(definition, party, all);
            return new(healed.Encounter, healed.Actors, healed.MainSeed, healed.ThinkingSeed, healed.Gold,
                healed.NewBattle, healed.ActiveAllies, healed.ItemCustody,
                field == "flags" ? [] : healed.StoryFlags, field == "visuals" ? null : healed.Visuals,
                field == "retained" ? [] : healed.RetainedSourceSlots, field == "credit" ? null : healed.DeathCredit);
        }
    }

    private static BattleStartInput Context(BattleStartInput basis, IEnumerable<BattleActorStartInput> actors) =>
        new(basis.Encounter, actors, 0x12345678u, 0x10203040u, 719,
            new("controlled", "test", "test", 0, true, true, true, false, false, null),
            [basis.Actors[0].Actor], new([2, 9], new Dictionary<byte, byte> { [61] = 2 }), [13, 29],
            new(new Dictionary<ActorRef, int>(), [new(19, basis.Actors[0].Actor, new(2, 3), 1)]),
            [19, 23], new(basis.Actors[0].Actor));

    private static void SameContext(BattleStartInput before, BattleStartInput after, uint? gold = null)
    {
        Assert.Equal(before.Encounter, after.Encounter); Assert.Equal(before.MainSeed, after.MainSeed);
        Assert.Equal(before.ThinkingSeed, after.ThinkingSeed); Assert.Equal(gold ?? before.Gold, after.Gold);
        Assert.Equal(before.NewBattle, after.NewBattle); Assert.Equal(before.ActiveAllies, after.ActiveAllies);
        Assert.Same(before.ItemCustody, after.ItemCustody); Assert.Same(before.Visuals, after.Visuals);
        Assert.Equal(before.StoryFlags, after.StoryFlags); Assert.Equal(before.RetainedSourceSlots, after.RetainedSourceSlots);
        Assert.Equal(before.DeathCredit, after.DeathCredit);
    }
}
