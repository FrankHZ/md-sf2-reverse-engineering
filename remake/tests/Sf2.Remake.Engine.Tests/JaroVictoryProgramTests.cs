using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.JaroVictoryTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class JaroVictoryProgramTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LaterPolicyFailureAndRetryDoNotReplayJoinedOrAiClearedPrefix(bool afterAi)
    {
        var policy = new TransientStory();
        var session = Enter(d =>
        {
            var program = d["world"]!["programs"]!.AsArray().Single(p => (string?)p!["id"] == "after")!;
            program["instructions"]!.AsArray().Insert(afterAi ? 5 : 2,
                JsonNode.Parse("""{"op":"retired-map3-entity-scratch"}"""));
        }, Rules(policy));
        var facts = Win(session).Observations.ToList();
        if (afterAi)
        {
            facts.AddRange(Accept(session, new Acknowledge(session.Current.Story.Wait!.Token)).Observations);
            UntilDialogue(session, facts);
        }
        var before = session.Current;
        var command = new Acknowledge(before.Story.Wait!.Token);
        var failure = Send(session, command); facts.AddRange(failure.Observations);
        Assert.Equal(SessionFailureKind.InvariantFailure, failure.Failure!.Kind);
        Assert.Equal(new ProgramLocation("after", afterAi ? 5 : 2), session.Current.Story.Cursor);
        Assert.Equal(JaroFrom(before), Jaro(session));
        Assert.Contains(23, session.Current.Story.Flags); Assert.Contains(55, session.Current.Story.Flags);
        Assert.DoesNotContain(532, session.Current.Story.Flags);
        Assert.Equal(afterAi ? (ushort)0x8001 : (ushort)0x8005, Jaro(session).ActivationWord);
        var prefix = session.Current;
        Assert.NotNull(Send(session, command).Failure); Assert.Same(prefix, session.Current);
        // A retry at the retained cursor can fail again without replaying join/text/AI writes.
        var repeated = Send(session, new AdvanceSimulation());
        Assert.NotNull(repeated.Failure);
        Assert.Equal(prefix.Story.Cursor, session.Current.Story.Cursor);
        Assert.Equal(JaroFrom(prefix), Jaro(session));
        Assert.DoesNotContain(repeated.Observations, e => e.Detail is "JoinPartyMember" or "ClearOutcomePartyAi");
        policy.Fail = false;
        facts.AddRange(Accept(session, new AdvanceSimulation()).Observations);
        Settle(session, facts);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Contains(23, session.Current.Story.PartyLists!.Reserve);
        Assert.Single(facts, e => e.Detail == "JoinPartyMember");
        Assert.Single(facts, e => e.Detail == "ClearOutcomePartyAi");
        Assert.Single(facts, e => e.Detail == "LeaveOutcomeBattleParty");
        Assert.Single(facts, e => e.Kind == "after-battle-join");
        Assert.Single(facts, e => e.Kind == "battle-completed-set");
        Assert.Equal((ushort)3, Jaro(session).Kills); Assert.Equal((byte)17, Jaro(session).Exp);
    }

    [Fact]
    public void FailureInCommonTailRetainsLeaveAndRetriesTailOnce()
    {
        var policy = new TransientReturn();
        var session = Enter(rules: Rules(outcome: policy));
        var facts = Win(session).Observations.ToList();
        facts.AddRange(Accept(session, new Acknowledge(session.Current.Story.Wait!.Token)).Observations);
        UntilDialogue(session, facts);
        facts.AddRange(Accept(session, new Acknowledge(session.Current.Story.Wait!.Token)).Observations);
        var failed = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.NotNull(failed.Failure); facts.AddRange(failed.Observations);
        Assert.Null(session.Current.Story.Cursor); Assert.Null(session.Current.Story.Wait);
        Assert.DoesNotContain(55, session.Current.Story.Flags);
        Assert.Contains(23, session.Current.Story.PartyLists!.Active); // still the older join23 cache
        Assert.Contains(432, session.Current.Story.Flags); Assert.DoesNotContain(532, session.Current.Story.Flags);
        var party = session.Current.Exploration!.Party;
        policy.Fail = false;
        facts.AddRange(Accept(session, new AdvanceSimulation()).Observations);
        Settle(session, facts);
        Assert.Equal(party.Actors, session.Current.Exploration!.Party.Actors);
        Assert.Contains(23, session.Current.Story.PartyLists!.Reserve);
        Assert.Single(facts, e => e.Detail == "LeaveOutcomeBattleParty");
        Assert.Single(facts, e => e.Kind == "after-battle-join");
        Assert.Single(facts, e => e.Kind == "battle-unlock-cleared");
        Assert.Single(facts, e => e.Kind == "battle-completed-set");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public void JoinItselfNeverInitializesOrRecoversRetainedActor(int hp)
    {
        var session = Start("jaro-victory", d =>
        {
            d["battle"]!["start"]!["actors"]![1]!["hp"] = hp;
            d["start"]!["program"] = JsonNode.Parse("""{"program":"join-only","instruction":0}""");
            d["world"]!["programs"]!.AsArray().Add(JsonNode.Parse("""
                {"id":"join-only","instructions":[{"op":"join-party","member":23},
                 {"op":"show-text","mode":"single","speaker":null,"text":101},
                 {"op":"join-party","member":23},{"op":"end"}]}
                """));
        });
        var actor = Jaro(session);
        Assert.Equal(hp, actor.Hp); Assert.Equal(1, actor.Mp);
        Assert.Equal((byte)17, actor.Exp); Assert.Equal((ushort)3, actor.Kills); Assert.Equal((ushort)2, actor.Defeats);
        Assert.Equal((ushort)0x8005, actor.ActivationWord);
        var party = session.Current.Exploration!.Party;
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Assert.Same(actor, Jaro(session)); Assert.Same(party, session.Current.Exploration!.Party);
    }

    [Theory]
    [InlineData("layout")]
    [InlineData("member")]
    [InlineData("enemy-member")]
    [InlineData("missing-word")]
    public void IncompleteOutcomePartyAdmissionFailsClosed(string shape)
    {
        var d = Document("jaro-victory");
        if (shape == "layout") d["world"]!["partyFlags"]!["capacity"] = 13;
        if (shape == "member") d["battle"]!["encounters"]![0]!["placements"]![1]!["initialization"]!.AsObject().Remove("allyPartyMember");
        if (shape == "enemy-member") d["battle"]!["encounters"]![0]!["placements"]![2]!["initialization"]!["allyPartyMember"] = 23;
        if (shape == "missing-word") d["battle"]!["start"]!["actors"]![1]!.AsObject().Remove("initialization");
        Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(d)));
    }

    [Fact]
    public void OutcomeOperationsConsumeTheBoundMemberRatherThanHardcodingJaro23()
    {
        var session = Enter(d =>
        {
            var encounter = d["battle"]!["encounters"]![0]!;
            encounter["placements"]![1]!["initialization"]!["allyPartyMember"] = 5;
            encounter["initialization"]!["sourceCombatants"]![1]!["slot"] = 5;
            d["start"]!["flags"] = new JsonArray(0, 5, 32, 37, 399, 432);
            var program = d["world"]!["programs"]!.AsArray().Single(p => (string?)p!["id"] == "after")!;
            foreach (var instruction in program["instructions"]!.AsArray())
                if (instruction!["member"] is not null) instruction["member"] = 5;
        });
        var facts = Win(session).Observations.ToList(); Settle(session, facts);
        Assert.Contains(5, session.Current.Story.PartyLists!.Reserve);
        Assert.DoesNotContain(37, session.Current.Story.Flags);
        Assert.DoesNotContain(23, session.Current.Story.Flags);
        Assert.Equal((ushort)0x8001, Jaro(session).ActivationWord);
    }

    private static BattleActorStartInput JaroFrom(SessionSnapshot snapshot) => snapshot.Exploration!.Party.Actors.Single(a => a.Actor.Value == "ally-jaro");
    private static SessionRules Rules(ISourceStoryPolicy? policy = null, IOutcomeReturnPolicy? outcome = null) =>
        new("jaro-failure", RuleCompositions.Sf2().Healing, RuleCompositions.SourcePhysical(),
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), sourceStory: policy, outcomeReturn: outcome);
    private sealed class TransientStory : ISourceStoryPolicy
    {
        public string Identity => "transient-story";
        internal bool Fail = true;
        public SourceStoryCandidate Apply(ScenarioDefinition definition, SessionSnapshot current,
            SourceStoryInstruction instruction, ProgramLocation location) => Fail ? throw new InvalidOperationException("transient") : new();
    }
    private sealed class TransientReturn : IOutcomeReturnPolicy
    {
        private readonly IOutcomeReturnPolicy _source = RuleCompositions.SourceOutcomeReturn();
        public string Identity => "transient-outcome";
        internal bool Fail = true;
        public OutcomeStart Start(ScenarioDefinition definition, EngineBattleState battle, StoryState story, BattleOutcomeKind kind, BattleStartInput party) =>
            _source.Start(definition, battle, story, kind, party);
        public OutcomeFinish Finish(ScenarioDefinition definition, StoryState story, BattleStartInput party) =>
            Fail ? throw new InvalidOperationException("transient") : _source.Finish(definition, story, party);
        public BattleStartInput Heal(ScenarioDefinition definition, BattleStartInput party, bool all) => _source.Heal(definition, party, all);
    }
}
