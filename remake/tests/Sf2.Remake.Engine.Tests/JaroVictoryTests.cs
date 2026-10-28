using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class JaroVictoryTests
{
    [Theory]
    [InlineData(true, true, false, 2)]
    [InlineData(true, true, true, 11)]
    [InlineData(false, false, false, 11)]
    [InlineData(false, false, true, 12)]
    [InlineData(false, false, false, 13)]
    [InlineData(false, true, false, 12)]
    [InlineData(true, true, false, 12)]
    [InlineData(true, true, false, 13)]
    [InlineData(false, true, true, 1)]
    public void OrdinaryWinJoinsClearsOnlyAiThenLeavesAndReturns(bool converted, bool joined, bool dead, int activeCount)
    {
        var session = Enter(d => Configure(d, converted, joined, dead, activeCount));
        var before = session.Current.Battle.GetActor(new("ally-jaro"));
        var facts = Win(session).Observations.ToList();
        var first = session.Current;
        Assert.Equal(SessionMode.Exploration, first.Mode);
        Assert.IsType<DialogueWait>(first.Story.Wait);
        Assert.Equal(101, Assert.IsType<OpenTextWindow>(first.Story.TextWindow).Text);
        Assert.Equal(new MapPosition(2, 2), first.Story.OutcomeReturn!.Position);
        Assert.Contains(23, first.Story.Flags);
        Assert.Equal(converted || activeCount < 12, first.Story.Flags.Contains(55));
        Assert.Equal(converted, first.Story.PartyLists!.Active.Contains(23));
        Assert.Equal(!converted, first.Story.PartyLists.Reserve.Contains(23));
        var recovered = Jaro(session);
        Assert.Equal(dead ? 0 : 24, recovered.Hp);
        Assert.Equal(dead ? 1 : 3, recovered.Mp);
        Assert.Equal(before.ActivationWord, recovered.ActivationWord);
        Assert.Equal(before.Exp, recovered.Exp); Assert.Equal(before.Kills, recovered.Kills);
        Assert.Equal(before.Defeats, recovered.Defeats); Assert.Equal(before.Status, recovered.Status);
        Assert.Same(before.SourceLoadout, recovered.SourceLoadout); Assert.Same(before.Progress, recovered.Progress);
        var ack = new Acknowledge(first.Story.Wait!.Token);
        var envelope = new CommandEnvelope(first.SessionId, first.Revision, null, ack);
        facts.AddRange(Accept(session, ack).Observations);
        var ticking = session.Current;
        Assert.IsType<TickWait>(ticking.Story.Wait);
        Assert.NotNull(session.Submit(envelope).Failure);
        Assert.NotNull(Send(session, ack).Failure);
        Assert.NotNull(Send(session, new CompletePresentation(ticking.Story.Wait!.Token, Sf2.Remake.Application.Content.Scenarios.PresentationCueKind.FadeIn)).Failure);
        Assert.Same(ticking, session.Current);
        facts.AddRange(Accept(session, new AdvanceSimulation(ticking.Story.Wait.Token)).Observations);
        Assert.IsType<TickWait>(session.Current.Story.Wait);
        Assert.Equal(recovered, Jaro(session));
        UntilDialogue(session, facts);
        Assert.Equal(103, Assert.IsType<OpenTextWindow>(session.Current.Story.TextWindow).Text);
        Assert.Equal(recovered with { ActivationWord = (ushort)(recovered.ActivationWord!.Value & ~4) }, Jaro(session));
        Assert.Same(first.Story.PartyLists, session.Current.Story.PartyLists); // zero branch never joins/rebuilds
        facts.AddRange(Accept(session, new Acknowledge(session.Current.Story.Wait!.Token)).Observations);
        Assert.DoesNotContain(55, session.Current.Story.Flags);
        Assert.Contains(23, session.Current.Story.Flags);
        Assert.Same(first.Story.PartyLists, session.Current.Story.PartyLists); // leave keeps cached prefixes
        Assert.DoesNotContain(532, session.Current.Story.Flags);
        Assert.Contains(432, session.Current.Story.Flags);
        Settle(session, facts);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Contains(23, session.Current.Story.PartyLists!.Reserve);
        Assert.DoesNotContain(23, session.Current.Story.PartyLists.Active);
        Assert.Contains(532, session.Current.Story.Flags); Assert.DoesNotContain(432, session.Current.Story.Flags);
        Assert.Equal(recovered with { ActivationWord = (ushort)(recovered.ActivationWord!.Value & ~4) }, Jaro(session));
        Assert.Single(facts, x => x.Detail == nameof(Sf2.Remake.Application.Content.Scenarios.JoinPartyMember));
        Assert.Single(facts, x => x.Detail == "ClearOutcomePartyAi");
        Assert.Single(facts, x => x.Detail == "LeaveOutcomeBattleParty");
        Assert.True(facts.FindIndex(x => x.Kind == "after-battle-join") < facts.FindIndex(x => x.Kind == "battle-unlock-cleared"));
        Assert.True(facts.FindIndex(x => x.Kind == "battle-unlock-cleared") < facts.FindIndex(x => x.Kind == "battle-completed-set"));
        var position = session.Current.Exploration!.PlayerEntity.Position;
        Accept(session, new Move(ExplorationDirection.West)); Settle(session, facts);
        Assert.Equal(new MapPosition(position.X - 1, position.Y), session.Current.Exploration!.PlayerEntity.Position);
        Assert.DoesNotContain(session.Current.Exploration.Entities.Keys, e => e.Value == "ally-jaro");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void RepeatCompletionSkipsAllRecruitmentWithoutRepair(bool converted, bool joined)
    {
        var session = Enter(d =>
        {
            Configure(d, converted, joined, false, 2);
            // Explicit content writes completion during the before program, before the winning action.
            var before = d["world"]!["programs"]!.AsArray().Single(p => (string?)p!["id"] == "before")!;
            before["instructions"]!.AsArray().Insert(0, JsonNode.Parse("""{"op":"set-flag","flag":532,"value":true}"""));
        });
        var word = session.Current.Battle.GetActor(new("ally-jaro")).ActivationWord;
        var facts = Win(session).Observations.ToList(); Settle(session, facts);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal(joined, session.Current.Story.Flags.Contains(23));
        Assert.Equal(converted, session.Current.Story.Flags.Contains(55));
        Assert.Equal(word, Jaro(session).ActivationWord);
        Assert.DoesNotContain(facts, e => e.Detail is "JoinPartyMember" or "ClearOutcomePartyAi" or "LeaveOutcomeBattleParty");
        Assert.Single(facts, e => e.Kind == "after-battle-join" && e.Detail == "0");
        Assert.Contains(532, session.Current.Story.Flags); Assert.DoesNotContain(432, session.Current.Story.Flags);
    }

    [Fact]
    public void ReturnAnchorUsesPreSceneFirstActiveMemberBeforeJaroLeaves()
    {
        var session = Enter(d => d["start"]!["flags"] = new JsonArray(0, 23, 55, 399, 432));
        var facts = Win(session).Observations.ToList();
        Assert.Equal(new MapPosition(4, 4), session.Current.Story.OutcomeReturn!.Position);
        Settle(session, facts);
        Assert.Equal(new MapPosition(4, 4), session.Current.Exploration!.PlayerEntity.Position);
        Assert.DoesNotContain(55, session.Current.Story.Flags);
        Assert.Contains(32, session.Current.Story.Flags); // join0 after anchor selection
        Assert.Empty(session.Current.Story.PartyLists!.Active); // join0's own cache precedes active32
        Assert.Equal(new[] { 0, 23 }, session.Current.Story.PartyLists.Reserve);
    }

    internal static void Configure(JsonNode d, bool converted, bool joined, bool dead, int activeCount)
    {
        var flags = Enumerable.Range(0, activeCount - (converted ? 1 : 0)).SelectMany(i => new[] { i, i + 32 }).ToList();
        if (joined) flags.Add(23);
        if (converted) flags.Add(55);
        flags.AddRange([399, 432]);
        d["start"]!["flags"] = new JsonArray(flags.Distinct().Select(i => (JsonNode?)JsonValue.Create(i)).ToArray());
        var actor = d["battle"]!["start"]!["actors"]![1]!;
        actor["hp"] = dead ? 0 : 9; actor["absent"] = !converted;
        actor["initialization"]!["activationWord"] = converted ? 0x8005 : 0x8001;
    }
    internal static GameSession Enter(Action<JsonNode>? change = null, SessionRules? rules = null)
    {
        var d = Document("jaro-victory"); change?.Invoke(d);
        var outcome = GameSession.Start(Reader(d), rules ?? Sf2.Remake.Application.Gameplay.RuleCompositions.Sf2());
        Assert.True(outcome is SessionStarted, outcome.ToString());
        var started = (SessionStarted)outcome;
        Assert.Null(started.Result.Failure);
        var session = started.Session;
        Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Equal("leader", session.Current.Selection!.Actor.Value);
        return session;
    }
    internal static SessionResult Win(GameSession session)
    {
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(new("enemy")));
        return FinishBattleScenes(session, Accept(session, new Confirm()));
    }
    internal static BattleActorStartInput Jaro(GameSession session) => session.Current.Exploration!.Party.Actors.Single(a => a.Actor.Value == "ally-jaro");
    internal static void UntilDialogue(GameSession session, List<SessionObservation> facts)
    {
        for (int i = 0; i < 50 && session.Current.Story.Wait is TickWait wait; i++)
            facts.AddRange(Accept(session, new AdvanceSimulation(wait.Token)).Observations);
        Assert.IsType<DialogueWait>(session.Current.Story.Wait);
    }
    internal static void Settle(GameSession session, List<SessionObservation> facts)
    {
        for (int i = 0; i < 2000 && session.Current.Story.Wait is { } wait; i++)
            facts.AddRange(Accept(session, wait switch
            {
                DialogueWait => new Acknowledge(wait.Token),
                FullFadeWait { LogicalDone: true } fade => new CompletePresentation(wait.Token, fade.Kind),
                _ => new AdvanceSimulation(wait.Token),
            }).Observations);
        Assert.Null(session.Current.Story.Wait);
    }
}
