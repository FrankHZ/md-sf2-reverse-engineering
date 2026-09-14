using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleMovementTests
{
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
        Accept(session, new Move(ExplorationDirection.South));
        Accept(session, new Move(ExplorationDirection.South));
        var result = Send(session, new Confirm());
        Assert.Equal("occupied-destination", result.Failure!.Code);
        Assert.Same(before, session.Current.Battle);
        Accept(session, new Move(ExplorationDirection.South));
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
