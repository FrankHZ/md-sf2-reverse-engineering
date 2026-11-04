using System.Text.Json.Nodes;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Xunit;
using static Sf2.Remake.Engine.Tests.SilenceSpellTests;

namespace Sf2.Remake.Engine.Tests;

public sealed class SupportDecisionTests
{
    private static BattleAutomaticAction? Support(EngineBattleState battle)
    {
        var rules = RuleCompositions.Sf2();
        return EnemySupportDecision.Resolve(battle, Smoke, rules.Actions);
    }

    [Fact]
    public void SupportScoresWholeAreaAndUsesLastHighestCenterWithoutRngOrLastTargetMutation()
    {
        var before = Battle(); var action = Assert.IsType<BattleAutomaticAction>(Support(before));
        Assert.Equal(Mage, action.Target); Assert.Equal(new BattleActionRef(BattleActionKind.SupportSpell, new("dispel", 1)), action.ChosenAction);
        Assert.Equal(2, action.Effects.Single(e => e.Kind == "ai-support-target").After);
        Assert.Equal(before.MainSeed, action.Battle.MainSeed); Assert.Equal(before.ThinkingSeed, action.Battle.ThinkingSeed);
        Assert.Equal(before.GetActor(Smoke).LastTarget, action.Battle.GetActor(Smoke).LastTarget);
        Assert.Equal(5, action.Battle.GetActor(Smoke).Mp); action.Validate(before, Smoke);
        // A second legal center at equal priority wins in source list order.
        var moved = before.With(actors: before.Actors.Select(a => a.Actor == Priest ? a.With(position: new(3, 1)) :
            a.Actor == Mage ? a.With(position: new(3, 2)) : a.Actor == Smoke ? a.With(position: new(4, 1)) : a));
        Assert.Equal(Priest, Support(moved)!.Target);
    }

    [Theory]
    [InlineData("single")] [InlineData("poor")] [InlineData("attack-mp")] [InlineData("out-of-area")]
    public void MpAloneNeverQualifiesWithoutTwoAreaContributors(string change)
    {
        var battle = Battle(d =>
        {
            if (change == "single") d["actors"]![1]!["spells"] = new JsonArray();
            if (change == "poor") d["start"]!["actors"]![2]!["mp"] = 4;
            if (change == "attack-mp") d["start"]!["actors"]![0]!["mp"] = 0;
            if (change == "out-of-area") d["encounters"]![0]!["placements"]![1]!["y"] = 5;
        });
        Assert.Null(Support(battle)); Assert.Equal(change == "poor" ? 4 : 5, battle.GetActor(Smoke).Mp);
    }

    [Fact]
    public void HealingContributorDoesNotNeedMpAndAlreadySilencedTargetsRemainCandidates()
    {
        var battle = Battle(d => d["start"]!["actors"]![1]!["mp"] = 0);
        battle = Status(Status(battle, Mage, 0x300), Priest, 0x300);
        Assert.NotNull(Support(battle));
        var rules = RuleCompositions.Sf2(); battle = Status(battle, Smoke, 0x300);
        var action = BattleDecisionRules.Decide(new SourceEnemyAi(), battle, Smoke, rules.Physical, rules.Progression, rules.Actions);
        Assert.Equal(BattleActionKind.SupportSpell, action.ChosenAction!.Value.Kind);
        var cast = Dispel(action.Battle); Assert.NotNull(cast.Silenced); Assert.Empty(cast.Reactions);
    }

    [Fact]
    public void EarlierPhysicalSuccessBypassesSupportAndChangedValidContentRetainsLegality()
    {
        var before = Battle();
        before = before.With(actors: before.Actors.Select(a => a.Actor == Smoke ? a.With(position: new(3, 1)) : a));
        var rules = RuleCompositions.Sf2();
        var decision = BattleDecisionRules.Decide(new SourceEnemyAi(), before, Smoke, rules.Physical, rules.Progression, rules.Actions);
        Assert.Null(decision.ChosenAction); Assert.NotNull(decision.Target);
        Assert.DoesNotContain(decision.Effects, e => e.Kind == "ai-command-support");
    }

    [Fact]
    public void SupportAttackGetterDoesNotSkipAnUnaffordableFirstFamilyForAnotherFamily()
    {
        var battle = Battle(d =>
        {
            d["spells"]![0]!["mpCost"] = 9;
            d["spells"]!.AsArray().Add(new JsonObject { ["id"] = "blast", ["level"] = 1, ["mpCost"] = 2,
                ["minimumRange"] = 1, ["maximumRange"] = 1,
                ["effect"] = new JsonObject { ["kind"] = "attack", ["power"] = 6, ["radius"] = 0, ["element"] = "wind" } });
            d["actors"]![0]!["spells"]!.AsArray().Add(new JsonObject { ["id"] = "blast", ["level"] = 1 });
            d["start"]!["actors"]![0]!["mp"] = 2;
        });
        Assert.Null(Support(battle));
    }
}
