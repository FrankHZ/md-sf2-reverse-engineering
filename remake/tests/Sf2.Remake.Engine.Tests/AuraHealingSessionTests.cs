using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;
using static Sf2.Remake.Engine.Tests.AuraHealingTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class AuraHealingSessionTests
{
    private static SessionCommand Next(SessionSnapshot snapshot)
    {
        var scene = snapshot.BattleScene!;
        return scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token,scene.CompletionKind);
    }
    private static SessionResult Cast(GameSession s, byte level)
    {
        Accept(s,new Confirm()); Accept(s,new SelectSpell(new("aura",level)));
        Accept(s,new SelectTarget(level==4?Caster:Center)); return Accept(s,new Confirm());
    }

    [Theory]
    [InlineData(1, 7)] [InlineData(2, 11)] [InlineData(3, 15)] [InlineData(4, 20)]
    public void OnePaymentOrderedRecoveriesOneAwardAndSameSessionUsableNextTurn(byte level,int cost)
    {
        var s=Start("aura-healing"); var initial=s.Current; var events=Cast(s,level).Observations.ToList();
        var ordered=s.Current.BattleScene!.HealingTargets.ToArray();
        Assert.Equal(80,s.Current.Battle.GetActor(Caster).Mp); Assert.Equal(70,s.Current.Battle.GetActor(Caster).Hp);
        Assert.Null(s.Current.BattleScene.Healing); // Authored AURA does not claim the original HEAL fairy RNG.
        int announcements=0, recoveries=0, rewards=0;
        while(s.Current.BattleScene is { } scene)
        {
            if(scene.Phase==BattleScenePhase.ActionMessage) announcements++;
            if(scene.Phase==BattleScenePhase.RewardMessage) rewards++;
            if(scene.Phase==BattleScenePhase.ResultMessage)
            { Assert.Equal(ordered[recoveries++],scene.Target); Assert.Equal(scene.Target,scene.DisplayedAlly); }
            var before=s.Current; var command=Next(before);
            var result=Accept(s,command); events.AddRange(result.Observations);
            var after=s.Current;
            Assert.NotNull(Send(s,command).Failure); Assert.Same(after,s.Current);
        }
        Assert.Equal(1,announcements); Assert.Equal(1,rewards); Assert.Equal(ordered.Length,recoveries);
        Assert.Equal(ordered,events.Where(e=>e.Kind=="healing-recovery").Select(e=>e.Target!.Value));
        Assert.Equal(ordered,events.Where(e=>e.Kind=="healing-exp-contribution").Select(e=>e.Target!.Value));
        Assert.Single(events,e=>e.Kind=="mp"); Assert.Single(events,e=>e.Kind=="exp");
        Assert.Equal(2,events.Count(e=>e.RandomRange is not null));
        Assert.Equal(80-cost,s.Current.Battle.GetActor(Caster).Mp);
        Assert.Equal(initial.SessionId,s.Current.SessionId); Assert.True(s.Current.HasBattleControl);
        Assert.NotEqual(Caster,s.Current.Selection!.Actor);
        Assert.Equal(initial.Battle.ThinkingSeed,s.Current.Battle.ThinkingSeed); Assert.Equal(73u,s.Current.Battle.Gold);
        var old=s.Current.Selection.Preview.Destination;
        Accept(s,new Move(ExplorationDirection.West)); Assert.NotEqual(old,s.Current.Selection!.Preview.Destination);
    }

    [Fact]
    public void SelectionCancelQueriesUnavailableAndStaleEnvelopeNeverPayOrDraw()
    {
        var s=Start("aura-healing"); var initial=s.Current;
        Assert.All(s.QueryBattleChoices().Spells,choice=>Assert.True(choice.Enabled)); Assert.Same(initial,s.Current);
        Accept(s,new Confirm()); Accept(s,new SelectSpell(new("aura",2))); Accept(s,new SelectTarget(Center));
        Assert.Equal("invalid-aura-center",Send(s,new SelectTarget(new("n-neutral"))).Failure!.Code);
        Assert.Equal("spell-not-known",Send(s,new SelectSpell(new("other",1))).Failure!.Code);
        var stale=new CommandEnvelope(s.Current.SessionId,s.Current.Revision,s.Current.Selection!.Actor,new Confirm());
        Accept(s,new Cancel()); var after=s.Current;
        Assert.NotNull(s.Submit(stale).Failure); Assert.Same(after,s.Current);
        Assert.Same(initial.Battle,s.Current.Battle);
        var poor=Start("aura-healing",d=>d["start"]!["actors"]![0]!["mp"]=6);
        Assert.All(poor.QueryBattleChoices().Spells,choice=>Assert.Equal("insufficient-mp",choice.Reason!.Code));
    }

    [Fact]
    public void LaterAwardRefusalRetainsPaidAndRecoveredPrefixAndRetriesOnlyAward()
    {
        var progression=new RefuseCredit();
        var rules=new SessionRules("aura-refusal",new Sf2HealingRule(), RuleCompositions.SourcePhysical(), RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), progression:progression);
        var s=Assert.IsType<SessionStarted>(GameSession.Start(Reader(Document("aura-healing")),rules)).Session;
        Cast(s,2);
        while(s.Current.BattleScene is not {Phase:BattleScenePhase.SpellStop}) Accept(s,Next(s.Current));
        var before=s.Current; progression.Refuse=true;
        var failed=Send(s,Next(before)); Assert.Equal("battle-policy-failure",failed.Failure!.Code); Assert.Same(before,s.Current);
        Assert.Equal(69,s.Current.Battle.GetActor(Caster).Mp); Assert.Equal(85,s.Current.Battle.GetActor(Caster).Hp);
        Assert.Equal(16,s.Current.Battle.GetActor(new("a-edge")).Hp); Assert.Equal((byte)0,s.Current.Battle.GetActor(Caster).Exp);
        progression.Refuse=false; var result=Accept(s,Next(before));
        Assert.Equal(before.Battle.MainSeed,s.Current.Battle.MainSeed);
        Assert.DoesNotContain(result.Observations,e=>e.Kind is "mp" or "hp" or "healing-recovery" || e.RandomRange is not null);
        FinishBattleScenes(s,result); Assert.True(s.Current.HasBattleControl); Assert.Equal(69,s.Current.Battle.GetActor(Caster).Mp);
    }

    [Fact]
    public void PauseAfterPaymentPreservesEveryUnappliedTargetAndRejectsWrongCompletion()
    {
        var s=Start("aura-healing"); Cast(s,4);
        while(s.Current.BattleScene is not {Phase:BattleScenePhase.SpellCost}) Accept(s,Next(s.Current));
        var paid=s.Current;
        Assert.Equal(60,paid.Battle.GetActor(Caster).Mp); Assert.Equal(10,paid.Battle.GetActor(new("f-far")).Hp);
        Assert.NotNull(Send(s,new Cancel()).Failure); Assert.Same(paid,s.Current);
        Assert.NotNull(Send(s,new CompletePresentation(paid.BattleScene!.Token,PresentationCueKind.SoundFade)).Failure); Assert.Same(paid,s.Current);
        Assert.Empty(s.QueryBattleChoices().Actions); Assert.Same(paid,s.Current);
        var result=FinishBattleScenes(s,new(paid,[],paid.StopReason));
        Assert.Equal(600,s.Current.Battle.GetActor(new("f-far")).Hp);
        Assert.DoesNotContain(result.Observations,e=>e.Kind=="mp"); Assert.Equal(60,s.Current.Battle.GetActor(Caster).Mp);
    }

    [Fact]
    public void SharedAwardCrossesOneGrowthThresholdAfterAllRecoveries()
    {
        var admitted = Admitted("aura-healing");
        var definition = admitted.Definition.Encounters.Values.Single();
        var growth = new BattleGrowthDefinition(0, 0,
            Enumerable.Repeat(new StatGrowth(1, 31, Enumerable.Repeat(new StatGrowthFraction(128, 16), 30).ToArray()), 5).ToArray(),
            [], new Dictionary<byte, SpellRef>());
        var encounter = new BattleDefinition(definition.Encounter, definition.Map, definition.Width, definition.Height,
            definition.Terrain, definition.Deployments.Select(row => row.Actor == Caster
                ? row with { Definition = row.Definition.WithGrowth(growth) } : row),
            definition.Spells.Values, definition.Rewards, definition.Initialization);
        var input = admitted.Start;
        var s = Assert.IsType<SessionStarted>(GameSession.Start(new ScenarioDefinition("aura-growth", [encounter]),
            new BattleStartInput(input.Encounter, input.Actors.Select(row => row.Actor == Caster ? row with { Exp = 99 } : row),
                input.MainSeed, input.ThinkingSeed, input.Gold))).Session;
        var events = Cast(s, 2).Observations.ToList();
        while (s.Current.BattleScene is not { Phase: BattleScenePhase.RewardMessage })
            events.AddRange(Accept(s, Next(s.Current)).Observations);
        Assert.Equal(4, events.Count(e => e.Kind == "healing-recovery"));
        Assert.Single(events, e => e.Kind == "exp");
        Assert.Equal(1, s.Current.Battle.GetActor(Caster).Level);
        uint seed = s.Current.Battle.MainSeed;
        var grown = Accept(s, Next(s.Current));
        var draws = grown.Observations.Where(e => e.Kind.StartsWith("rng-growth-")).ToArray();
        Assert.Equal(10, draws.Length); Assert.Equal((long)seed, draws[0].Before);
        Assert.Equal(2, s.Current.Battle.GetActor(Caster).Level);
        Assert.Equal(69, s.Current.Battle.GetActor(Caster).Mp); Assert.Equal(85, s.Current.Battle.GetActor(Caster).Hp);
        Assert.False(s.Current.HasBattleControl);
        FinishBattleScenes(s, grown); Assert.True(s.Current.HasBattleControl);
    }

    private sealed class RefuseCredit : IBattleProgressionRule
    {
        private readonly Sf2BattleProgressionRule _source=new();
        public bool Refuse {get;set;}
        public string Identity=>"aura-credit-refusal";
        public int Award(EngineBattleState b,BattleActorState a,BattleActionKind k,IReadOnlyList<BattleReaction> r,ref uint seed,List<BattleEffect> e)=>_source.Award(b,a,k,r,ref seed,e);
        public uint Gold(uint a,uint b)=>_source.Gold(a,b);
        public ushort Kills(ushort a)=>_source.Kills(a);
        public ushort Defeats(ushort a)=>_source.Defeats(a);
        public BattleActorState Credit(BattleActorState a,int n,List<BattleEffect> e)=>Refuse?throw new InvalidOperationException("credit unavailable"):_source.Credit(a,n,e);
        public BattleActorState Grow(BattleActorState a,ref uint seed,List<BattleEffect> e)=>_source.Grow(a,ref seed,e);
    }
}
