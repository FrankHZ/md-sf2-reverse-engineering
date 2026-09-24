using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleMovementTests
{
    [Theory]
    [InlineData("practice-yard")]
    [InlineData("garden-watch")]
    public void ANewStepOwnsExactlyOneDeliveryAndKeepsCommittedResources(string package)
    {
        var session = Start(package);
        var before = session.Current;
        var actor = before.Selection!.Actor;
        var from = before.Selection.Preview.Destination;
        var result = Accept(session, new Move(ExplorationDirection.East));
        var movement = Assert.IsType<BattleMovementState>(session.Current.BattleMovement);
        Assert.Equal(from, movement.From);
        Assert.Equal(new MapPosition(from.X + 1, from.Y), movement.To);
        Assert.Equal(BattleMovementPurpose.Player, movement.Purpose);
        Assert.Same(before.Battle, session.Current.Battle);
        Assert.False(session.Current.HasBattleControl);
        Assert.Single(result.Observations, e => e.Kind == "battle-movement-segment-started");
        var pending = session.Current;
        foreach (var command in new SessionCommand[] { new Move(ExplorationDirection.East), new Confirm(), new Cancel(),
            new AdvanceSimulation(), new CompletePresentation(new WaitToken(movement.Token.Value + 1), movement.CompletionKind),
            new CompletePresentation(movement.Token, PresentationCueKind.Sound) })
        {
            Assert.NotNull(Send(session, command).Failure);
            Assert.Same(pending, session.Current);
        }
        var completion = new CompletePresentation(movement.Token, movement.CompletionKind);
        var arrived = Accept(session, completion);
        Assert.Null(session.Current.BattleMovement);
        Assert.True(session.Current.HasBattleControl);
        Assert.Same(before.Battle, session.Current.Battle);
        Assert.Equal(actor, session.Current.Selection!.Actor);
        Assert.Single(arrived.Observations, e => e.Kind == "battle-movement-segment-arrived");
        Assert.NotNull(Send(session, completion).Failure);
    }

    [Fact]
    public void CancelWalksDecreasingCostsRatherThanReplayingTheInputHistory()
    {
        var session = Start(change: d =>
        {
            var rows = d["terrains"]![0]!["rows"]!.AsArray();
            for (int y = 0; y < rows.Count; y++) rows[y] = new string('g', rows[y]!.GetValue<string>().Length);
        });
        var original = session.Current.Battle;
        foreach (var direction in new[] { ExplorationDirection.East, ExplorationDirection.South,
            ExplorationDirection.West, ExplorationDirection.East, ExplorationDirection.East })
        {
            var result = Accept(session, new Move(direction));
            Assert.Equal(2, session.Current.BattleMovement!.Path.Count);
            FinishMovement(session, result);
        }
        Accept(session, new Confirm());
        var cancelled = Accept(session, new Cancel());
        var movement = session.Current.BattleMovement!;
        Assert.Equal(BattleMovementPurpose.Return, movement.Purpose);
        Assert.Equal(new[] { new MapPosition(5, 4), new MapPosition(5, 3), new MapPosition(4, 3), new MapPosition(3, 3) }, movement.Path);
        Assert.False(session.Current.HasBattleControl);
        Assert.Same(original, session.Current.Battle);
        var returned = FinishMovement(session, cancelled);
        Assert.Equal(3, returned.Observations.Count(e => e.Kind == "battle-movement-segment-started"));
        Assert.Same(original, session.Current.Battle);
        Assert.Equal(new MapPosition(3, 3), session.Current.Selection!.Preview.Destination);
        var empty = Accept(session, new Cancel());
        Assert.Null(session.Current.BattleMovement);
        Assert.DoesNotContain(empty.Observations, e => e.Kind.StartsWith("battle-movement-", StringComparison.Ordinal));
    }

    [Fact]
    public void FullWidthAuthoredMapDoesNotWrapFromOneRowEdgeToTheNext()
    {
        var session = Start(change: document =>
        {
            var rows = document["terrains"]![0]!["rows"]!.AsArray();
            for (int row = 0; row < rows.Count; row++) rows[row] = new string('g', 48);
            document["encounters"]![0]!["placements"]![0]!["x"] = 47;
            document["encounters"]![0]!["placements"]![0]!["y"] = 2;
        });
        var battle = session.Current.Battle;
        Assert.Equal("movement-range", Assert.Throws<BattleRuleException>(() =>
            BattleMovement.Preview(battle, new("medic-a"), new(0, 3))).Code);
        Assert.Equal(2, BattleMovement.Preview(battle, new("medic-a"), new(47, 3)).Cost);
    }

    [Fact]
    public void WeightedPreviewAccountsForForestCostsAndActorBudget()
    {
        var admitted = Admitted();
        var battle = BattleTurnFlow.Start(admitted.Definition.Encounters[admitted.Start.Encounter], admitted.Start);
        var preview = BattleMovement.Preview(battle, new("medic-a"), new(5, 3));
        Assert.Equal(6, preview.Cost);
        Assert.Equal(new MapPosition(3, 3), preview.Path[0]);
        Assert.Equal(new MapPosition(5, 3), preview.Destination);
        Assert.Equal("movement-range", Assert.Throws<BattleRuleException>(() =>
            BattleMovement.Preview(battle, new("medic-a"), new(10, 3))).Code);
    }

    [Fact]
    public void AllyCanBeTraversedButCannotBeACommittedDestination()
    {
        var session = Start();
        var before = session.Current.Battle;
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        var result = Send(session, new Confirm());
        Assert.Equal("occupied-destination", result.Failure!.Code);
        Assert.Same(before, session.Current.Battle);
        FinishMovement(session, Accept(session, new Move(ExplorationDirection.South)));
        Accept(session, new Confirm());
        Accept(session, new ChooseAction(SessionAction.Stay));
        Accept(session, new Confirm());
        Assert.Equal(new MapPosition(3, 6), session.Current.Battle.GetActor(new("medic-a")).Position);
    }

    [Fact]
    public void LivingOpponentBlocksTraversalWithoutChangingTheTerrainAuthority()
    {
        var session = Start(change: document =>
        {
            document["encounters"]![0]!["placements"]![2]!["x"] = 4;
            document["encounters"]![0]!["placements"]![2]!["y"] = 3;
        });
        var before = session.Current;
        var result = Send(session, new Move(ExplorationDirection.East));
        Assert.Equal("movement-range", result.Failure!.Code);
        Assert.Same(before, session.Current);
        Assert.Equal(new BattleTerrain(TerrainSurface.Brush, TerrainProtection.Heavy), before.Battle.Definition.Terrain[3 * 48 + 4]);
    }
}
