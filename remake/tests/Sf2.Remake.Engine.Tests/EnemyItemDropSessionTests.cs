using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class EnemyItemDropSessionTests
{
    private static readonly ActorRef Ally = new("leader"), Enemy = new("enemy"), Survivor = new("survivor");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OrdinaryCommandsCommitReceiptOrSilentDealsThenRetainCustodyThroughWalkingRetry(bool full)
    {
        var session = Enter(full);
        Guid identity = session.Current.SessionId;
        var before = session.Current.Battle;
        _ = session.QueryBattleChoices(); _ = session.QueryBattleChoices();
        Assert.Same(before, session.Current.Battle);
        var prepared = Attack(session);
        Assert.Null(prepared.Failure);
        var custody = session.Current.Battle.ItemCustody!;
        Assert.Equal(new byte[] { 24 }, custody.OnceFlags);
        Assert.Equal(full ? 15 : 14, custody.Deals[110]); Assert.Equal(3, custody.Deals[61]);
        Assert.Equal((ushort)1, session.Current.Battle.GetActor(Enemy).Hp);
        Assert.Equal(new ushort[] { 127, 127, 127, 127 }, session.Current.Battle.GetActor(Enemy).SourceLoadout!.Items);
        Assert.Contains(prepared.Observations, e => e.Kind == (full ? "item-to-deals" : "item-received"));
        Assert.Equal((ushort)61, session.Current.Battle.GetActor(Survivor).SourceLoadout!.Items[0]);
        Assert.Equal((byte)0, session.Current.Battle.GetActor(Ally).Exp);
        Assert.Contains(24, session.Current.Story.Flags); // This does not block drop once bit24.
        var phases = new List<BattleScenePhase>(); var events = prepared.Observations.ToList();
        while (session.Current.BattleScene is { } scene)
        {
            phases.Add(scene.Phase);
            var command = SceneCommand(session.Current);
            events.AddRange(Accept(session, command).Observations);
            var accepted = session.Current;
            Assert.NotNull(Send(session, command).Failure);
            Assert.Same(accepted, session.Current);
        }
        Assert.Equal(!full, phases.Contains(BattleScenePhase.ItemDroppedMessage));
        Assert.Equal(!full, phases.Contains(BattleScenePhase.ItemReceivedMessage));
        if (!full)
        {
            Assert.True(phases.IndexOf(BattleScenePhase.DeathMessage) < phases.IndexOf(BattleScenePhase.ItemDroppedMessage));
            Assert.True(phases.IndexOf(BattleScenePhase.ItemReceivedMessage) < phases.IndexOf(BattleScenePhase.Reward));
        }
        SettleField(session);
        Assert.Equal(SessionMode.Exploration, session.Current.Mode);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        var party = session.Current.Exploration!.Party;
        Assert.Same(custody, party.ItemCustody);
        var ally = party.Actors.Single(a => a.Actor == Ally);
        Assert.Equal(full ? new ushort[] { 0, 0, 0, 0 } : [110, 127, 127, 127], ally.SourceLoadout!.Items);
        Assert.Equal((ushort)2, ally.Kills); Assert.Equal((uint)84, party.Gold);
        Assert.DoesNotContain(party.Actors, a => a.Actor == Enemy || a.Actor == Survivor);
        Assert.Single(events, e => e.Kind == "rng-item-drop");
        Assert.Single(events, e => e.Kind == "drop-once-set");
        Assert.Single(events, e => e.Kind == "exp");
        Assert.DoesNotContain((byte)25, custody.OnceFlags); // Leader sweep never invokes action drops.
        Accept(session, new Move(ExplorationDirection.West)); SettleField(session);
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Equal(identity, session.Current.SessionId);
        Assert.Same(custody, session.Current.Battle.ItemCustody);
        Assert.Equal(ally.SourceLoadout.Items, session.Current.Battle.GetActor(Ally).SourceLoadout!.Items);
        Assert.Equal((ushort)494, session.Current.Battle.GetActor(Enemy).SourceLoadout!.Items[0]);
        Assert.Equal(party.Gold, session.Current.Battle.Gold);
        var repeat = Attack(session);
        Assert.Equal((ushort)0, Assert.Single(repeat.Observations, e => e.Kind == "rng-item-drop").RandomValue);
        Assert.DoesNotContain(repeat.Observations, e => e.Kind is "item-received" or "item-to-deals" or "enemy-item-removed" or "drop-once-set");
        Assert.Same(custody, session.Current.Battle.ItemCustody);
        FinishBattleScenes(session, repeat); SettleField(session);
        Assert.Equal(identity, session.Current.SessionId);
        Assert.Same(custody, session.Current.Exploration!.Party.ItemCustody);
        Assert.Equal(ally.SourceLoadout.Items, session.Current.Exploration.Party.Actors.Single(a => a.Actor == Ally).SourceLoadout!.Items);
    }

    [Fact]
    public void LiveCustodyAlsoCrossesEntryIntoAnAuthoredEncounterWithoutDrops()
    {
        var d = Document("battle-drops"); var b = d["battle"]!;
        var map = b["maps"]![0]!.DeepClone(); map["id"] = "annex"; b["maps"]!.AsArray().Add(map);
        var encounter = b["encounters"]![0]!.DeepClone();
        encounter["id"] = "annex-battle"; encounter["map"] = "annex"; encounter.AsObject().Remove("itemDrops");
        b["encounters"]!.AsArray().Add(encounter);
        var field = d["world"]!["maps"]![1]!.DeepClone(); field["id"] = "annex";
        field["battle"]!["encounter"] = "annex-battle"; field["events"] = new JsonArray();
        d["world"]!["maps"]!.AsArray().Add(field);
        var retry = d["world"]!["programs"]![0]!.DeepClone(); retry["id"] = "retry-annex";
        retry["instructions"]![1]!["map"] = "annex"; d["world"]!["programs"]!.AsArray().Add(retry);
        d["world"]!["maps"]![1]!["events"]![0]!["program"]!["program"] = "retry-annex";
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(d))).Session;
        Accept(session, new Interact(new("ferryman"))); SettleField(session);
        FinishBattleScenes(session, Attack(session)); SettleField(session);
        var custody = session.Current.Exploration!.Party.ItemCustody;
        Accept(session, new Move(ExplorationDirection.West)); SettleField(session);
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        Assert.Equal("annex-battle", session.Current.Battle.Definition.Encounter);
        Assert.Null(session.Current.Battle.Definition.ItemDrops);
        Assert.Same(custody, session.Current.Battle.ItemCustody);
        Assert.Equal((ushort)110, session.Current.Battle.GetActor(Ally).SourceLoadout!.Items[0]);
        Assert.Equal((ushort)494, session.Current.Battle.GetActor(Enemy).SourceLoadout!.Items[0]);
    }

    [Fact]
    public void ProgressionSeesTheAlreadyConstructedReceiptBeforeHpPlayback()
    {
        var progression = new ReceiptProgression();
        var rules = new SessionRules("drop-progression", new Sf2HealingRule(), RuleCompositions.SourcePhysical(),
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), progression: progression);
        var session = Enter(rules: rules);
        Attack(session);
        Assert.True(progression.SawReceipt);
    }

    private sealed class ReceiptProgression : Sf2BattleProgressionRule
    {
        public bool SawReceipt { get; private set; }
        public override int Award(EngineBattleState battle, BattleActorState recipient, BattleActionKind kind,
            IReadOnlyList<BattleReaction> reactions, ref uint seed, List<BattleEffect> effects)
        {
            SawReceipt = recipient.SourceLoadout!.Items[0] == 110 && battle.GetActor(Ally).SourceLoadout!.Items[0] == 110 &&
                battle.ItemCustody!.OnceFlags.Contains((byte)24) && battle.GetActor(Enemy).Hp == 1;
            return base.Award(battle, recipient, kind, reactions, ref seed, effects);
        }
    }

    [Fact]
    public void LaterGrowthFailurePreservesReceiptAndRetriesOnlyThePendingPlayback()
    {
        var progression = new FailingGrowth();
        var rules = new SessionRules("drop-failure", new Sf2HealingRule(), RuleCompositions.SourcePhysical(),
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), progression: progression);
        var session = Enter(rules: rules);
        var facts = Attack(session).Observations.ToList();
        while (session.Current.BattleScene?.Phase != BattleScenePhase.RewardMessage)
            facts.AddRange(Accept(session, SceneCommand(session.Current)).Observations);
        var before = session.Current; var custody = before.Battle.ItemCustody;
        var command = SceneCommand(before);
        progression.Fail = true;
        var failure = Send(session, command);
        Assert.Equal(SessionFailureKind.InvariantFailure, failure.Failure!.Kind);
        Assert.Same(before, session.Current); Assert.Empty(failure.Observations);
        Assert.Equal((ushort)110, session.Current.Battle.GetActor(Ally).SourceLoadout!.Items[0]);
        progression.Fail = false;
        var retried = Accept(session, command);
        Assert.DoesNotContain(retried.Observations, e => e.Kind is "rng-item-drop" or "item-received" or "enemy-item-removed" or "drop-once-set");
        Assert.Same(custody, session.Current.Battle.ItemCustody);
        FinishBattleScenes(session, retried); SettleField(session);
        Assert.Same(custody, session.Current.Exploration!.Party.ItemCustody);
        Assert.Single(facts, e => e.Kind == "item-received");
    }

    [Theory]
    [InlineData("custody", "drop-custody-unknown")]
    [InlineData("rarity", "drop-item-rarity")]
    [InlineData("unsupported-item", "drop-item-unsupported")]
    [InlineData("deals-overflow", "numeric-range")]
    [InlineData("duplicate-flag", "duplicate-drop-row")]
    [InlineData("ally-slot", "drop-enemy")]
    [InlineData("duplicate-slot", "duplicate-source-slot")]
    [InlineData("missing-item", "missing-drop-item")]
    [InlineData("conflicting-effect", "conflicting-item-type")]
    [InlineData("boolean-flag", "drop-once-flag")]
    public void IncompleteOrUnsupportedAdmissionFailsBeforeSessionPublication(string fault, string code)
    {
        var d = Document("battle-drops"); var b = d["battle"]!; var drops = b["encounters"]![0]!["itemDrops"]!;
        switch (fault)
        {
            case "custody": b["start"]!.AsObject().Remove("itemCustody"); break;
            case "rarity": b["dropItems"]![0]!["rare"] = false; break;
            case "unsupported-item": b["dropItems"]![0]!["id"] = 100; break;
            case "deals-overflow": b["start"]!["itemCustody"]!["deals"]![0]!["count"] = 16; break;
            case "duplicate-flag": drops["rows"]![1]!["onceFlag"] = 24; break;
            case "ally-slot": drops["enemySlots"]![0]!["actor"] = "leader"; break;
            case "duplicate-slot": drops["enemySlots"]![0]!["slot"] = 129; break;
            case "missing-item": drops["rows"]![0]!["item"] = 86; break;
            case "conflicting-effect": b["items"] = JsonNode.Parse("""[{"id":110,"name":"Wrong effect","effect":"consumable-healing","power":10,"minimumRange":0,"maximumRange":1}]"""); break;
            case "boolean-flag": b["start"]!["itemCustody"]!["onceFlags"]!.AsArray().Add(true); break;
        }
        Assert.Equal(code, Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(d))).Failure.Code);
    }

    private sealed class FailingGrowth : Sf2BattleProgressionRule, IBattleProgressionRule
    {
        public bool Fail { get; set; }
        public new BattleActorState Grow(BattleActorState actor, ref uint seed, List<BattleEffect> effects) =>
            Fail ? throw new InvalidOperationException("later delivery failed") : base.Grow(actor, ref seed, effects);
    }

    private static GameSession Enter(bool full = false, SessionRules? rules = null)
    {
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(Document(full ? "battle-drops-full" : "battle-drops")), rules ?? RuleCompositions.Sf2())).Session;
        Accept(session, new Interact(new("ferryman"))); SettleField(session);
        Assert.Equal(SessionMode.Battle, session.Current.Mode);
        return session;
    }
    private static SessionResult Attack(GameSession session)
    {
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(Enemy)); return Accept(session, new Confirm());
    }
    private static SessionCommand SceneCommand(SessionSnapshot current) => current.BattleScene!.RequiresAcknowledgement
        ? new Acknowledge(current.BattleScene.Token) : new CompletePresentation(current.BattleScene.Token, current.BattleScene.CompletionKind);
    private static void SettleField(GameSession session)
    {
        for (int i = 0; i < 2000 && session.Current.Mode == SessionMode.Exploration && session.Current.Story.Wait is { } wait; i++)
            Accept(session, wait switch
            {
                DialogueWait => new Acknowledge(wait.Token),
                FullFadeWait { LogicalDone: true } fade => new CompletePresentation(wait.Token, fade.Kind),
                _ => new AdvanceSimulation(wait.Token),
            });
    }
}
