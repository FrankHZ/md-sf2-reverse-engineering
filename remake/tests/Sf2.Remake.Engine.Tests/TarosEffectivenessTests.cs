using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class TarosEffectivenessTests
{
    private static readonly ActorRef Apprentice = new("apprentice"), Champion = new("champion"), Guardian = new("guardian");

    [Theory]
    [InlineData("champion", 189, true)]
    [InlineData("champion", 445, true)] // Equipped plus broken; masked identity still61.
    [InlineData("champion", 61, false)]
    [InlineData("champion", 255, false)] // Equipped NOTHING is skipped.
    [InlineData("champion", 127, false)]
    [InlineData("apprentice", 189, false)]
    public void InitialConjunctionUsesBoundActorAndLiveEquippedWeapon(string identity, int item, bool effective)
    {
        var battle = Controlled(5000);
        var actor = new ActorRef(identity);
        // Definition starts with equipped61. This live state is deliberately different.
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == actor
            ? a.With(sourceLoadout: new([(ushort)item, 127, 127, 127], [63, 63, 63, 63])) : a));
        var action = Prepare(battle, actor);
        var reaction = action.Reactions[0];
        Assert.Equal(effective ? BattleReactionKind.Damage : BattleReactionKind.Ineffective, reaction.Kind);
        Assert.Equal(effective, reaction.HpAfter < reaction.HpBefore);
        Assert.Equal(effective, action.ConstructionEffects.Any(row => row.Kind == "rng-spread-1"));
        Assert.Equal(189, battle.GetActor(actor).Definition.SourceLoadout!.Items[0]);
    }

    [Fact]
    public void EquippedScanSkipsNonWeaponsAndStopsAtTheFirstEquippedWeapon()
    {
        var battle = Controlled(5000);
        var binding = new TarosEffectiveness(Champion, new Dictionary<ActorRef, byte> { [Guardian] = 88 }, [61, 1]);
        var champion = battle.GetActor(Champion);
        Assert.False(binding.PrepareInitial(champion.With(sourceLoadout: new([128, 255, 189, 127], [])), battle.GetActor(Guardian)));
        Assert.True(binding.PrepareInitial(champion.With(sourceLoadout: new([129, 189, 127, 127], [])), battle.GetActor(Guardian)));
        Assert.False(binding.PrepareInitial(champion.With(sourceLoadout: new([1, 189, 127, 127], [])), battle.GetActor(Guardian)));
    }

    [Fact]
    public void CurrentEquipmentCanEnableAnInitiallyCarriedWeapon()
    {
        var battle = Controlled(5000, d => d["actors"]![1]!["items"] = new JsonArray(61));
        Assert.Equal(BattleReactionKind.Ineffective, Prepare(battle, Champion).Reactions[0].Kind);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Champion
            ? a.With(sourceLoadout: new([189, 127, 127, 127], [])) : a));
        Assert.Equal(BattleReactionKind.Damage, Prepare(battle, Champion).Reactions[0].Kind);
        Assert.Equal(61, battle.GetActor(Champion).Definition.SourceLoadout!.Items[0]);
    }

    [Theory]
    [InlineData("different-definition")]
    [InlineData("unmapped-actor")]
    [InlineData("unbound-encounter")]
    [InlineData("reordered-slot")]
    [InlineData("renamed-encounter")]
    public void DefinitionBindingIsIndependentOfSlotNameAndOtherEncounters(string variation)
    {
        var battle = Controlled(5000, document =>
        {
            var encounter = document["encounters"]![0]!;
            switch (variation)
            {
                case "different-definition":
                    encounter["tarosEffectiveness"]!["enemyDefinitions"]![0]!["definition"] = 9;
                    encounter["tarosEffectiveness"]!["enemyDefinitions"]![1]!["definition"] = 88;
                    break;
                case "unmapped-actor": encounter["tarosEffectiveness"]!["enemyDefinitions"] =
                    JsonNode.Parse("""[{"actor":"reserve","definition":88}]"""); break;
                case "unbound-encounter":
                    var other = encounter.DeepClone(); other["id"] = "ordinary-practice";
                    other.AsObject().Remove("tarosEffectiveness");
                    document["encounters"]!.AsArray().Add(other);
                    document["start"]!["encounter"] = "ordinary-practice";
                    break;
                case "reordered-slot": encounter["placements"]![2]!["processingOrder"] = 0; break;
                case "renamed-encounter":
                    encounter["id"] = "another-practice"; document["start"]!["encounter"] = "another-practice"; break;
            }
        });
        var reaction = Prepare(battle, Apprentice).Reactions[0];
        Assert.Equal(variation is "reordered-slot" or "renamed-encounter"
            ? BattleReactionKind.Ineffective : BattleReactionKind.Damage, reaction.Kind);
    }

    [Fact]
    public void IneffectiveRetainsSurroundingDrawsAndMinimumEndExpWithoutDamageOrDeathCredit()
    {
        var before = Controlled(5000);
        var action = Prepare(before, Apprentice);
        Assert.Same(before.Definition, action.Prepared.Definition);
        Assert.Equal(500, action.Prepared.GetActor(Guardian).Hp);
        Assert.Equal(0x43DE1234u, action.Prepared.MainSeed);
        Assert.Equal(new[] { "rng-dodge", "rng-critical", "rng-double", "rng-counter", "rng-exp-plus", "rng-exp-minus" },
            action.ConstructionEffects.Where(row => row.Kind.StartsWith("rng-", StringComparison.Ordinal)).Select(row => row.Kind));
        Assert.Equal(new ushort?[] { 31, 28, 20, 9, 11, 4 },
            action.ConstructionEffects.Where(row => row.RandomRange is not null).Select(row => row.RandomValue));
        Assert.Equal(1, action.Reward!.Amount);
        var reaction = Assert.Single(action.Reactions);
        Assert.False(reaction.Critical); Assert.Null(reaction.StatusWrite); Assert.Null(reaction.InflictedSleep);
        var playback = BattleSceneRules.Reaction(action.Prepared, reaction);
        Assert.Same(action.Prepared, playback.Battle); Assert.Empty(playback.Motion); Assert.Empty(playback.Effects);
        var result = PhysicalBattleAction.ResolveSourceDefault(before, Apprentice, new(3, 2), Guardian);
        Assert.Equal(500, result.Battle.GetActor(Guardian).Hp);
        Assert.Equal(0, result.Battle.GetActor(Guardian).Status);
        Assert.Equal(1, result.Battle.GetActor(Apprentice).Exp!.Value);
        Assert.Equal(0, result.Battle.GetActor(Apprentice).Kills!.Value);
        Assert.Equal(0, result.Battle.GetActor(Guardian).Defeats!.Value);
        Assert.Equal(before.Gold, result.Battle.Gold);
        Assert.DoesNotContain(result.Effects, row => row.Kind is "hp" or "gold" or "death-cleanup" or "critical");
    }

    [Fact]
    public void DoubleRetainsInitialIneffectiveOperandAndDoesNotDispatchAThirdStrike()
    {
        var action = Prepare(Controlled(179), Apprentice);
        Assert.Equal(new[] { "physical-first", "physical-second" }, action.Reactions.Select(row => row.Action));
        Assert.All(action.Reactions, row => Assert.Equal(BattleReactionKind.Ineffective, row.Kind));
        Assert.Equal(0xE1951234u, action.Prepared.MainSeed);
        Assert.Equal(2, action.ConstructionEffects.Count(row => row.Kind == "rng-double" && row.RandomValue == 0));
        Assert.Equal(1, action.Reward!.Amount);
    }

    [Fact]
    public void CounterClearsIneffectiveAndTarosAsOriginalTargetCanCounter()
    {
        var action = Prepare(Controlled(183), Apprentice);
        Assert.Equal(new[] { "physical-first", "physical-counter" }, action.Reactions.Select(row => row.Action));
        Assert.Equal(BattleReactionKind.Ineffective, action.Reactions[0].Kind);
        var counter = action.Reactions[1];
        Assert.Equal(Guardian, counter.Actor); Assert.Equal(Apprentice, counter.Target);
        Assert.Equal(BattleReactionKind.Damage, counter.Kind);
        Assert.True(counter.Amount > 0);
    }

    [Fact]
    public void TarosOriginPreventsCounterDispatchWithoutRemovingItsCounterDraw()
    {
        // Seed169 selects a counter after the normal damage/spread draws.
        var battle = Controlled(169);
        battle = battle.With(actors: battle.Actors.Select(a => a.Actor == Apprentice ? a.With(position: new(3, 2)) : a));
        var action = PhysicalBattleAction.PrepareSourceDefault(battle, Guardian, new(2, 2), Apprentice);
        Assert.Contains(action.ConstructionEffects, row => row.Kind == "rng-counter" && row.RandomValue == 0);
        Assert.DoesNotContain(action.Reactions, row => row.Action == "physical-counter");
    }

    [Fact]
    public void DodgeWinsOverIneffectiveAndCriticalDrawDoesNotBecomeCriticalDamageFeedback()
    {
        var dodge = PhysicalStrikeRules.Resolve(30, 4, 500, 256, 62u << 16 | 0x1234, 32, 32, 1, ineffective: true);
        Assert.True(dodge.Dodged); Assert.False(dodge.Ineffective);
        Assert.Equal(new[] { "dodge", "double", "counter" }, dodge.Rolls.Select(row => row.Purpose));
        var critical = Prepare(Controlled(388), Apprentice);
        Assert.Contains(critical.ConstructionEffects, row => row.Kind == "rng-critical" && row.RandomValue == 0);
        Assert.All(critical.Reactions, row => { Assert.False(row.Critical); Assert.Equal(BattleReactionKind.Ineffective, row.Kind); });
        Assert.DoesNotContain(critical.ConstructionEffects, row => row.Kind == "critical");
    }

    [Fact]
    public void SourceSupportedSleepFollowsIneffectiveWithoutChangingLaterConstructionStatusReads()
    {
        var battle = WithSleepProfile(Controlled(388));
        var action = Prepare(battle, Apprentice);
        Assert.Equal(2, action.Reactions.Count);
        Assert.Equal((ushort)0xC0, action.Reactions[0].InflictedSleep);
        Assert.All(action.Reactions, row => Assert.Equal(BattleReactionKind.Ineffective, row.Kind));
        Assert.Equal(2, action.ConstructionEffects.Count(row => row.Kind == "rng-dodge"));
        var replay = action.ApplySleep(action.ApplyReaction(action.Prepared, action.Reactions[0]), action.Reactions[0]);
        Assert.Equal(500, replay.GetActor(Guardian).Hp); Assert.Equal(0xC0, replay.GetActor(Guardian).Status);
        var playback = BattleSceneRules.Reaction(replay, action.Reactions[0]);
        Assert.Empty(playback.Effects);
        Assert.DoesNotContain(action.Reactions, row => row.Action == "physical-counter");
    }

    [Fact]
    public void IneffectiveSleepDoesNotCancelAnAlreadyConstructedLegalCounter()
    {
        var action = Prepare(WithSleepProfile(Controlled(401)), Apprentice);
        Assert.Equal((ushort)0xC0, action.Reactions[0].InflictedSleep);
        Assert.Equal(BattleReactionKind.Ineffective, action.Reactions[0].Kind);
        var counter = action.Reactions[1];
        Assert.Equal("physical-counter", counter.Action);
        Assert.Equal(BattleReactionKind.Damage, counter.Kind);
        var after = action.ApplySleep(action.Prepared, action.Reactions[0]);
        after = action.ApplyReaction(after, counter);
        Assert.True(after.GetActor(Apprentice).Hp < 500);
        Assert.Equal(0xC0, after.GetActor(Guardian).Status);
    }

    [Fact]
    public void QueriesAndCancelNeverConstructAnActionOrSpendSeeds()
    {
        var session = Start("taros-effectiveness");
        var before = session.Current.Battle;
        _ = session.QueryBattleChoices(); _ = session.QueryBattleChoices();
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.North)));
        SelectAttack(session); FinishMovement(session, Accept(session, new Cancel()));
        Assert.Equal(before.MainSeed, session.Current.Battle.MainSeed);
        Assert.Equal(before.ThinkingSeed, session.Current.Battle.ThinkingSeed);
        Assert.Equal(before.GetActor(Apprentice).Position, session.Current.Battle.GetActor(Apprentice).Position);
        Assert.Equal(before.Gold, session.Current.Battle.Gold);
        Assert.Null(session.Current.BattleScene);
    }

    [Fact]
    public void RealScenePublishesIneffectiveOnceAndRejectsStaleOrDuplicateDelivery()
    {
        var session = Start("taros-effectiveness");
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.North)));
        SelectAttack(session);
        var result = Accept(session, new Confirm());
        Assert.DoesNotContain(result.Observations, row => row.Kind == "ineffective");
        var oldToken = session.Current.BattleScene!.Token;
        var command = SceneCommand(session.Current);
        Accept(session, command);
        var committed = session.Current;
        Assert.NotNull(Send(session, command).Failure); Assert.Same(committed, session.Current);
        Assert.NotNull(Send(session, new Acknowledge(oldToken)).Failure); Assert.Same(committed, session.Current);
        var completed = FinishBattleScenes(session, Step(session));
        Assert.Single(completed.Observations, row => row.Kind == "ineffective");
        Assert.DoesNotContain(completed.Observations, row => row.Kind.StartsWith("rng-reaction-", StringComparison.Ordinal));
        Assert.Equal(500, session.Current.Battle.GetActor(Guardian).Hp);
        Assert.True(session.Current.HasBattleControl); Assert.Equal(Champion, session.Current.Selection!.Actor);
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.North)));
        SelectAttack(session);
        completed = FinishBattleScenes(session, Accept(session, new Confirm()));
        Assert.True(session.Current.Battle.GetActor(Guardian).Hp < 500);
        Assert.Contains(completed.Observations, row => row.Kind == "hp" && row.Actor == Guardian);
        Assert.True(session.Current.HasBattleControl);
    }

    [Fact]
    public void LaterRewardFailurePreservesConstructionDeliveryAndRetriesOnlyThePendingCredit()
    {
        var progression = new FailingCredit();
        var rules = new SessionRules("taros-credit", new Sf2HealingRule(), RuleCompositions.SourcePhysical(),
            RuleCompositions.SourceItem(), RuleCompositions.SourceStay(), progression: progression);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(Document("taros-effectiveness")), rules)).Session;
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.North)));
        SelectAttack(session); Accept(session, new Confirm());
        while (session.Current.BattleScene!.Phase != BattleScenePhase.ResultMessage) Step(session);
        var prefix = session.Current;
        Assert.Equal("Ineffective", prefix.BattleScene!.ReactionKind);
        progression.Fail = true;
        var command = SceneCommand(prefix);
        for (int retry = 0; retry < 2; retry++)
        {
            var failure = Send(session, command);
            Assert.Equal(SessionFailureKind.InvariantFailure, failure.Failure!.Kind);
            Assert.Same(prefix, session.Current); Assert.Empty(failure.Observations);
            Assert.Equal(0, session.Current.Battle.GetActor(Apprentice).Exp!.Value);
        }
        progression.Fail = false;
        var result = FinishBattleScenes(session, Accept(session, command));
        Assert.Equal(1, session.Current.Battle.GetActor(Apprentice).Exp!.Value);
        Assert.Equal(500, session.Current.Battle.GetActor(Guardian).Hp);
        Assert.Single(result.Observations, row => row.Kind == "exp");
        Assert.DoesNotContain(result.Observations, row => row.Kind is "ineffective" or "rng-dodge" or "rng-exp-plus");
    }

    private static EngineBattleState Controlled(ushort seed, Action<JsonNode>? change = null) =>
        Start("taros-effectiveness", change).Current.Battle.With(mainSeed: (uint)seed << 16 | 0x1234);

    private static BattleActionResolution Prepare(EngineBattleState battle, ActorRef actor) =>
        PhysicalBattleAction.PrepareSourceDefault(battle, actor, actor == Champion ? new(1, 2) : new(3, 2), Guardian);

    private static EngineBattleState WithSleepProfile(EngineBattleState battle)
    {
        var original = battle.GetActor(Apprentice);
        var d = original.Definition;
        var sleeper = new BattleActorDefinition(d.Actor, d.ClassRule, d.Level, d.MaxHp, d.MaxMp,
            d.Attack, d.Defense, d.Agility, d.ExtraRoundAction, d.Move, d.Spells,
            d.Physical! with { Critical = PhysicalCriticalRule.Sleep }, d.Mover, d.SourceLoadout);
        var actor = new BattleActorState(original.Deployment with { Definition = sleeper }, original.Hp, original.Mp,
            original.Exp, original.Position, original.Kills, original.Defeats);
        return battle.With(actors: battle.Actors.Select(a => a.Actor == Apprentice ? actor : a));
    }

    private static void SelectAttack(GameSession session)
    {
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(Guardian));
    }

    private static SessionCommand SceneCommand(SessionSnapshot snapshot) => snapshot.BattleScene!.RequiresAcknowledgement
        ? new Acknowledge(snapshot.BattleScene.Token) : new CompletePresentation(snapshot.BattleScene.Token, snapshot.BattleScene.CompletionKind);

    private static SessionResult Step(GameSession session) => Accept(session, SceneCommand(session.Current));

    private sealed class FailingCredit : Sf2BattleProgressionRule, IBattleProgressionRule
    {
        internal bool Fail { get; set; }
        BattleActorState IBattleProgressionRule.Credit(BattleActorState actor, int amount, List<BattleEffect> effects) =>
            Fail ? throw new InvalidOperationException("credit failure") : Credit(actor, amount, effects);
    }
}
