using System.Text.Json.Nodes;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class PrismLaserTests
{
    internal static readonly ActorRef Leader = new("leader"), Companion = new("companion"), Flower = new("flower"), Sentry = new("sentry");
    internal static EngineBattleState Battle(bool onRay = true)
    {
        var b = Start("prism-laser").Current.Battle;
        return b.With(mainSeed: 0x00001234, actors: b.Actors.Select(a => onRay && a.IsAlly ? a.With(position: new(a.Position!.X, 4)) : a));
    }
    internal static BattleActionResolution Prepare(EngineBattleState b, ActorRef? focus = null) =>
        BattleActionRules.Prepare(new PrismLaser(), b, Flower, b.GetActor(Flower).Position!,
            new(BattleActionKind.PrismLaser), focus ?? Leader, new Sf2BattleProgressionRule());
    private static BattleAutomaticAction Decide(EngineBattleState b) => BattleDecisionRules.Decide(new PrismLaserDecision(),
        b, Flower, new Sf2PhysicalAction(), new Sf2BattleProgressionRule());

    [Fact]
    public void FocusIsSpatialButHitsAreSourceOrderedAndOnlyConstructionRngChanges()
    {
        var before = Battle(); var decision = Decide(before); var action = Prepare(before);
        Assert.Equal(Leader, decision.Target);
        Assert.Equal(BattleActionKind.PrismLaser, decision.ChosenAction!.Value.Kind);
        Assert.Equal(new[] { Companion, Leader, Sentry }, action.Laser!.Targets);
        Assert.Equal(new[] { 16, 16, 14 }, action.Reactions.Select(r => r.Amount));
        Assert.Equal(new ushort[] { 0, 44, 0 }, action.Reactions.Select(r => r.HpAfter));
        Assert.Equal(16, action.Reactions[0].Amount); Assert.Equal(1, action.Reactions[0].HpBefore);
        Assert.Equal(6, action.ConstructionEffects.Count);
        Assert.All(action.ConstructionEffects, e => Assert.Equal((ushort)3, e.RandomRange));
        Assert.Equal(before.ThinkingSeed, action.Prepared.ThinkingSeed);
        Assert.Equal(before.Gold, action.Prepared.Gold); Assert.Same(before.ItemCustody, action.Prepared.ItemCustody);
        Assert.Null(action.Reward); Assert.All(action.Reactions, r => Assert.Null(r.Receipt));
        Assert.Equal(before.Actors.Select(a => (a.Hp, a.Mp, a.Kills, a.Defeats, a.Status)), action.Prepared.Actors.Select(a => (a.Hp, a.Mp, a.Kills, a.Defeats, a.Status)));
        Assert.Same(before, decision.Battle); Assert.Empty(decision.Effects);
        Assert.Equal(before.MainSeed, decision.Battle.MainSeed);
    }
    [Fact]
    public void EnemyOnlyRayStaysAndNeutralDeadOrDepartedAlliesCannotTrigger()
    {
        var b = Battle(false); Assert.Null(Decide(b).ChosenAction);
        foreach (int change in Enumerable.Range(0, 5))
        {
            var state = Battle().With(actors: Battle().Actors.Select(a => !a.IsAlly ? a : change switch
            {
                0 => a.With(hp: 0),
                1 => a.With(activationWord: 8),
                2 => a.With(clearPosition: true),
                3 => a.With(position: new(48, 4)),
                _ => a.With(position: new(a.Position!.X, 5))
            }));
            var decision = Decide(state); Assert.Null(decision.Target); Assert.Null(decision.ChosenAction);
            Assert.Equal(state.MainSeed, decision.Battle.MainSeed); Assert.Equal(state.ThinkingSeed, decision.Battle.ThinkingSeed);
        }
    }
    [Theory]
    [InlineData(0, 22, 20)]
    [InlineData(1, 20, 18)]
    [InlineData(2, 18, 20)]
    [InlineData(3, 20, 22)]
    public void CardinalDirectionIgnoresFacingAndOrdinaryTerrain(int direction, int x, int y)
    {
        var s = Start("prism-laser", d => d["encounters"]![0]!["prismLaser"]!["combatants"]![2]!["direction"] = direction);
        var b = s.Current.Battle;
        b = b.With(actors: b.Actors.Select(a => a.Actor == Flower ? a.With(position: new(20, 20), facing: (byte)((direction + 1) % 4)) :
            a.Actor == Leader ? a.With(position: new(x, y)) : a.With(clearPosition: true)));
        Assert.Equal(Leader, Decide(b).Target); Assert.Equal((byte)direction, Prepare(b).Laser!.Direction);
    }
    [Fact]
    public void RayUsesTactical48DomainBeyondCropAndDoesNotStopAtWalls()
    {
        var s = Start("prism-laser", d =>
        {
            d["terrains"]![0]!["rows"] = new JsonArray(Enumerable.Repeat<JsonNode?>(null, 10).Select(_ => JsonValue.Create(".........." as string)).ToArray());
            d["terrains"]![0]!["legend"]!["#"] = JsonNode.Parse("""{"surface":"barrier","protection":"none"}""");
            d["terrains"]![0]!["rows"]![4] = "...#......";
        });
        var b = s.Current.Battle.With(actors: s.Current.Battle.Actors.Select(a => a.Actor == Leader ? a.With(position: new(30, 4)) : a.IsAlly ? a.With(clearPosition: true) : a));
        Assert.Equal(Leader, Decide(b).Target); Assert.Contains(Leader, Prepare(b).Laser!.Targets);
    }
    [Fact]
    public void PreparationRebuildsWithoutRerunningDecisionEvenWhenFocusDiesOrEverythingLeaves()
    {
        var b = Battle(); var decision = Decide(b);
        var changed = b.With(actors: b.Actors.Select(a => a.IsAlly ? a.With(hp: 0) : a));
        var action = Prepare(changed, decision.Target); Assert.Equal(new[] { Sentry }, action.Laser!.Targets);
        changed = changed.With(actors: changed.Actors.Select(a => a.Actor == Sentry ? a.With(clearPosition: true) : a));
        action = Prepare(changed, decision.Target);
        Assert.Empty(action.Reactions); Assert.Empty(action.ConstructionEffects); Assert.Equal(changed.MainSeed, action.Prepared.MainSeed);
        Assert.Equal(Leader, action.Laser!.Focus);
    }
    [Theory]
    [InlineData("overlap")]
    [InlineData("edge")]
    [InlineData("status")]
    [InlineData("unknown-word")]
    [InlineData("hp")]
    public void UnsafeOrUnboundStatesRejectBeforePreparation(string invalid)
    {
        var b = Battle(); b = b.With(actors: b.Actors.Select(a => invalid switch
        {
            "overlap" when a.Actor == Sentry => a.With(position: b.GetActor(Leader).Position),
            "edge" when a.Actor == Flower => a.With(position: new(0, 4)),
            "status" when a.Actor == Leader => a.With(status: 1),
            "unknown-word" when a.Actor == Leader => new BattleActorState(a.Deployment, a.Hp, a.Mp, a.Exp, a.Position, a.Kills, a.Defeats),
            "hp" when a.Actor == Leader => a.With(hp: 32768),
            _ => a
        }));
        Assert.Throws<BattleRuleException>(() => Prepare(b));
    }
    [Theory]
    [InlineData("burst-rock")]
    [InlineData("zeon-guard")]
    [InlineData("debug")]
    public void SpecialKindsAreClosedAtContentAdmission(string kind)
    {
        var d = Document("prism-laser"); d["encounters"]![0]!["prismLaser"]!["combatants"]![3]!["kind"] = kind;
        Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(d)));
    }
    [Fact]
    public void RenamedActorsReorderedDeploymentAndChangedSourceSlotsStillDriveTheActualRule()
    {
        var d = Document("prism-laser");
        d["actors"]![0]!["id"] = "traveler"; d["start"]!["actors"]![0]!["actor"] = "traveler";
        d["encounters"]![0]!["placements"]![0]!["actor"] = "traveler";
        d["encounters"]![0]!["placements"]![0]!["processingOrder"] = 99;
        d["encounters"]![0]!["prismLaser"]!["combatants"]![0]!["actor"] = "traveler";
        d["encounters"]![0]!["prismLaser"]!["combatants"]![0]!["slot"] = 0;
        var b = Assert.IsType<SessionStarted>(GameSession.Start(Reader(d))).Session.Current.Battle;
        b = b.With(actors: b.Actors.Select(a => a.IsAlly ? a.With(position: new(a.Position!.X, 4)) : a));
        var chosen = Decide(b); Assert.Equal(new ActorRef("traveler"), chosen.Target);
        Assert.Equal(new ActorRef("traveler"), Prepare(b, chosen.Target).Reactions[0].Target);
    }
    [Fact]
    public void Source1234DamageIgnoresStatsMpAndRebuildsChangedNeutralAndHpState()
    {
        var b = Battle().With(mainSeed: 0x1234CAFE);
        b = b.With(actors: b.Actors.Select(a => a.Actor == Leader ? a.With(hp: 10, mp: 0, attack: 255) :
            a.Actor == Companion || a.Actor == Sentry ? a.With(activationWord: 8) : a.With(attack: 0)));
        var action = Prepare(b);
        var reaction = Assert.Single(action.Reactions);
        Assert.Equal(Leader, reaction.Target); Assert.Equal(14, reaction.Amount); Assert.Equal(0, reaction.HpAfter);
        Assert.Equal(0x04B6CAFEu, action.Prepared.MainSeed);
        Assert.Equal(new ushort?[] { 2, 0 }, action.ConstructionEffects.Select(e => e.RandomValue));
    }

    [Fact]
    public void Non15ActivationAndMissingBindingsRejectRatherThanInventActiveSourceState()
    {
        var d = Document("prism-laser");
        d["encounters"]![0]!["prismLaser"]!["primaryRegion"] = 2;
        Assert.Equal("prism-activation-regions", Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(d))).Failure.Code);
        d = Document("prism-laser");
        d["encounters"]![0]!["prismLaser"]!["combatants"]!.AsArray().RemoveAt(0);
        Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(d)));
    }

}
