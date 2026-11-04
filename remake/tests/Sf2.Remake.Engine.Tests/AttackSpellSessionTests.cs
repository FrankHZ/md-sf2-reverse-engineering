using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.AttackSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class AttackSpellSessionTests
{
    internal static SessionCommand Next(SessionSnapshot state) => state.BattleScene!.RequiresAcknowledgement
        ? new Acknowledge(state.BattleScene.Token) : new CompletePresentation(state.BattleScene.Token, state.BattleScene.CompletionKind);
    internal static SessionResult Cast(GameSession session, string id = "blaze", byte level = 2)
    {
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new(id, level)));
        Accept(session, new SelectTarget(Center)); return Accept(session, new Confirm());
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void OnePaymentOrderedHitsDeathAccountingOneAwardThenUsableNextActor(bool bothDie)
    {
        var session = Start("attack-spells", d => { if (bothDie) d["start"]!["actors"]![2]!["hp"] = 1; }); var initial = session.Current;
        var events = Cast(session).Observations.ToList();
        Assert.Equal(20, session.Current.Battle.GetActor(Caster).Mp);
        Assert.Equal(4, session.Current.Battle.GetActor(Center).Hp);
        int messages = 0;
        var targets = new List<ActorRef>();
        while (session.Current.BattleScene is { } scene)
        {
            if (scene.Phase == BattleScenePhase.ActionMessage) messages++;
            if (scene.Phase == BattleScenePhase.ResultMessage)
            { targets.Add(scene.Target!.Value); Assert.Equal(scene.Target, scene.DisplayedEnemy); }
            var command = Next(session.Current); events.AddRange(Accept(session, command).Observations);
            var after = session.Current; Assert.NotNull(Send(session, command).Failure); Assert.Same(after, session.Current);
        }
        Assert.Equal(1, messages); Assert.Equal(new[] { Wing, Center }, targets);
        Assert.Single(events, e => e.Kind == "mp"); Assert.Single(events, e => e.Kind == "exp");
        Assert.Equal(new[] { Wing, Center }, events.Where(e => e.Kind == "hp").Select(e => e.Actor!.Value));
        Assert.Equal(14, session.Current.Battle.GetActor(Caster).Mp);
        Assert.Equal((ushort)(bothDie ? 2 : 1), session.Current.Battle.GetActor(Caster).Kills!.Value);
        Assert.Equal(0, session.Current.Battle.GetActor(Center).Hp); Assert.Null(session.Current.Battle.GetActor(Center).Position);
        Assert.Equal(bothDie ? 114u : 107u, session.Current.Battle.Gold);
        Assert.Equal(56, events.Count(e => e.RandomRange is not null)); // 6 damage +2 award +24 per damaged recipient.
        Assert.Equal(initial.SessionId, session.Current.SessionId); Assert.True(session.Current.HasBattleControl);
        Assert.Equal(new ActorRef("guard"), session.Current.Selection!.Actor);
        var origin = session.Current.Selection.Preview.Destination;
        Accept(session, new Move(ExplorationDirection.East)); Assert.NotEqual(origin, session.Current.Selection!.Preview.Destination);
    }

    [Fact]
    public void LearnedChoicesQueryConfirmCancellationAndInsufficientMpAreAtomic()
    {
        var session = Start("attack-spells"); var initial = session.Current;
        var choices = session.QueryBattleChoices(); Assert.Same(initial, session.Current);
        Assert.Equal(new[] { new SpellRef("blaze", 1), new("blaze", 2), new("blast", 1) }, choices.Spells.Select(s => s.Spell));
        Assert.All(choices.Spells.Take(2), s => Assert.True(s.Enabled));
        Assert.False(choices.Spells[2].Enabled); // BLAST1 cannot center two tiles away.
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("blaze", 2)));
        foreach (var target in choices.Spells[1].Targets)
        {
            var before = session.Current; var selected = Send(session, new SelectTarget(target.Actor));
            Assert.Equal(target.Enabled, selected.Failure is null);
            if (selected.Failure is not null) Assert.Same(before, session.Current);
        }
        var ready = session.Current;
        Assert.NotNull(Send(session, new SelectTarget(new("guard"))).Failure); Assert.Same(ready, session.Current);
        Assert.Equal("spell-not-known", Send(session, new SelectSpell(new("blaze", 3))).Failure!.Code);
        var stale = new CommandEnvelope(ready.SessionId, ready.Revision, Caster, new Confirm());
        Accept(session, new Cancel()); var canceled = session.Current;
        Assert.NotNull(session.Submit(stale).Failure); Assert.Same(canceled, session.Current);
        Assert.Same(initial.Battle, session.Current.Battle);
        var poor = Start("attack-spells", d => d["start"]!["actors"]![0]!["mp"] = 5);
        Assert.Equal("insufficient-mp", poor.QueryBattleChoices().Spells[1].Reason!.Code);
        Accept(poor, new Confirm()); var unchanged = poor.Current;
        Assert.Equal("insufficient-mp", Send(poor, new SelectSpell(new("blaze", 2))).Failure!.Code); Assert.Same(unchanged, poor.Current);
        var unlearned = Start("attack-spells", d => d["actors"]![0]!["spells"]!.AsArray().RemoveAt(1));
        Assert.DoesNotContain(unlearned.QueryBattleChoices().Spells, s => s.Spell == new SpellRef("blaze", 2));
    }

    [Fact]
    public void PaidPrefixSurvivesWrongCompletionAndLaterCreditFailureWithoutRecast()
    {
        var rule = new RefuseCredit();
        var rules = new SessionRules("spell-credit-fault", new Sf2HealingRule(), RuleCompositions.SourcePhysical(),
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), progression: rule);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(Document("attack-spells")), rules)).Session;
        Cast(session);
        while (session.Current.BattleScene is not { Phase: BattleScenePhase.SpellCost }) Accept(session, Next(session.Current));
        var paid = session.Current; Assert.Equal(14, paid.Battle.GetActor(Caster).Mp);
        Assert.Equal(4, paid.Battle.GetActor(Center).Hp); Assert.Empty(session.QueryBattleChoices().Actions);
        Assert.NotNull(Send(session, new Cancel()).Failure); Assert.Same(paid, session.Current);
        Assert.NotNull(Send(session, new CompletePresentation(paid.BattleScene!.Token, PresentationCueKind.SoundFade)).Failure);
        while (session.Current.BattleScene is not { Phase: BattleScenePhase.DeathMessage }) Accept(session, Next(session.Current));
        var dead = session.Current; rule.Refuse = true;
        var failed = Send(session, Next(dead)); Assert.Equal("battle-policy-failure", failed.Failure!.Code); Assert.Same(dead, session.Current);
        Assert.Equal(14, dead.Battle.GetActor(Caster).Mp); Assert.Equal(0, dead.Battle.GetActor(Center).Hp);
        rule.Refuse = false; var result = FinishBattleScenes(session, Accept(session, Next(dead)));
        Assert.DoesNotContain(result.Observations, e => e.Kind is "mp" or "hp");
        Assert.True(session.Current.HasBattleControl); Assert.Equal((ushort)1, session.Current.Battle.GetActor(Caster).Kills!.Value);
    }

    [Fact]
    public void SpellKillGrowsAfterAwardMessageAndLearnsBlazeTwoWithoutLevelCeiling()
    {
        var admitted = Admitted("attack-spells"); var original = admitted.Definition.Encounters.Values.Single();
        var growth = new BattleGrowthDefinition(3, 0,
            Enumerable.Repeat(new StatGrowth(1, 1, []), 5).ToArray(), [new LevelSpell(5, 75)],
            new Dictionary<byte, SpellRef> { [11] = new("blaze", 1), [75] = new("blaze", 2) });
        var encounter = original.WithDeployments(original.Deployments.Select(d => d.Actor == Caster
            ? d with { Definition = new BattleActorDefinition(Caster, BattleClassRule.UnpromotedMage, 4, 30, 20,
                10, 5, 80, false, 5, [new("blaze", 1)], d.Definition.Physical, BattleMover.Mage,
                new BattleSourceLoadout([127, 127, 127, 127], [11, 63, 63, 63]), growth) } : d));
        var start = admitted.Start;
        var session = Assert.IsType<SessionStarted>(GameSession.Start(new ScenarioDefinition("spell-growth", [encounter]),
            new BattleStartInput(start.Encounter, start.Actors.Select(a => a.Actor == Caster ? a with { Exp = 99 } : a),
                start.MainSeed, start.ThinkingSeed, start.Gold))).Session;
        Assert.DoesNotContain(session.Current.Battle.GetActor(Caster).Spells, s => s.Level == 2);
        Cast(session, level: 1);
        while (session.Current.BattleScene is not { Phase: BattleScenePhase.RewardMessage }) Accept(session, Next(session.Current));
        Assert.Equal(4, session.Current.Battle.GetActor(Caster).Level);
        var result = Accept(session, Next(session.Current));
        Assert.Equal(5, session.Current.Battle.GetActor(Caster).Level);
        Assert.Contains(new SpellRef("blaze", 2), session.Current.Battle.GetActor(Caster).Spells);
        Assert.Contains(result.Observations, e => e.Kind == "spell-learned");
        FinishBattleScenes(session, result); Assert.Equal(18, session.Current.Battle.GetActor(Caster).Mp);
    }

    private sealed class RefuseCredit : IBattleProgressionRule
    {
        private readonly Sf2BattleProgressionRule _source = new();
        public bool Refuse { get; set; }
        public string Identity => "spell-credit-refusal";
        public int Award(EngineBattleState b, BattleActorState a, BattleActionKind k, IReadOnlyList<BattleReaction> r,
            ref uint seed, List<BattleEffect> e) => _source.Award(b, a, k, r, ref seed, e);
        public uint Gold(uint a, uint b) => _source.Gold(a, b);
        public ushort Kills(ushort a) => _source.Kills(a);
        public ushort Defeats(ushort a) => _source.Defeats(a);
        public BattleActorState Credit(BattleActorState a, int n, List<BattleEffect> e) =>
            Refuse ? throw new InvalidOperationException("credit unavailable") : _source.Credit(a, n, e);
        public BattleActorState Grow(BattleActorState a, ref uint seed, List<BattleEffect> e) => _source.Grow(a, ref seed, e);
    }
}
