using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class SpellSelectionTests
{
    internal static JsonNode TwoSpells()
    {
        var document = Document();
        var second = document["spells"]![0]!.DeepClone();
        second["id"] = "restore"; second["level"] = 2; second["mpCost"] = 5;
        second["maximumRange"] = 2; second["effect"]!["adjustedPower"] = 30;
        document["spells"]!.AsArray().Add(second);
        document["actors"]![0]!["spells"]!.AsArray().Add(new JsonObject { ["id"] = "restore", ["level"] = 2 });
        return document;
    }

    [Fact]
    public void SecondLearnedSpellUsesItsOwnCostAndPowerWithoutReorderingContent()
    {
        var document = TwoSpells(); document["start"]!["actors"]![0]!["hp"] = 60;
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(document))).Session;
        var actor = session.Current.Selection!.Actor;
        Assert.Equal(new[] { new SpellRef("mend", 1), new SpellRef("restore", 2) }, session.Current.Battle.GetActor(actor).Definition.Spells);
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("restore", 2)));
        Accept(session, new SelectTarget(actor)); var result = Accept(session, new Confirm());
        Assert.Equal((ushort)90, result.Snapshot.Battle.GetActor(actor).Hp);
        Assert.Equal((byte)15, result.Snapshot.Battle.GetActor(actor).Mp);
        Assert.Single(result.Observations, observation => observation.Kind == "after-turn" && observation.Actor == actor);
        Assert.NotEqual(actor, result.Snapshot.Selection!.Actor);
    }

    [Fact]
    public void SwitchingSpellsClearsTargetAndIllegalChoiceKeepsAcceptedSelectionAndRng()
    {
        var session = Assert.IsType<SessionStarted>(GameSession.Start(Reader(TwoSpells()))).Session;
        var actor = session.Current.Selection!.Actor;
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("mend", 1)));
        Accept(session, new SelectTarget(actor)); var before = session.Current;
        Assert.Equal("spell-not-known", Send(session, new SelectSpell(new("unknown", 1))).Failure!.Code);
        Assert.Same(before, session.Current);
        Accept(session, new SelectSpell(new("restore", 2)));
        Assert.Equal(BattleSelectionStage.TargetChoice, session.Current.Selection!.Stage);
        Assert.Null(session.Current.Selection.Target); Assert.Same(before.Battle, session.Current.Battle);
        Accept(session, new Cancel()); Assert.Null(session.Current.Selection!.Spell);
    }
}
