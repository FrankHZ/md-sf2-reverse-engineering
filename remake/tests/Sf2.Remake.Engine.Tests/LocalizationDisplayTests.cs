using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class LocalizationDisplayTests
{
    [Theory]
    [InlineData("Name", 0)]
    [InlineData("A B-7", 3)]
    public void FieldDisplayCapturesNamesAndSourceRangesWithoutChangingGlyphWork(string name, int speed)
    {
        var session = ExplorationTextWaitTests.StartFieldText("{NAME;1} waits.{W1}{CLEAR}{N}{NAME;1}{D1}{W2}{W1}", name: name, speed: speed);
        var first = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        var captured = Assert.Single(first.DisplayNames!, binding => binding.SourceToken == 0);
        Assert.Equal(1, captured.Member); Assert.Equal(name, captured.CanonicalName);
        Assert.Equal(name + " waits.", first.Projection);
        Assert.Equal(name.Length, first.Units.Count(unit => unit.SourceToken == 0));
        Assert.Equal(0, first.DisplayStart); Assert.Equal(2, first.DisplayEnd);
        Assert.Equal(new[] { 0, 5 }, first.DisplayNames!.Select(binding => binding.SourceToken));
        ExplorationTextWaitTests.DrainTextWork(session);
        Assert.Equal(first.Units, Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Units);
        Accept(session, new CompleteTextReveal(session.Current.Story.Wait!.Token));
        Accept(session, new WaitForText(session.Current.Story.Wait!.Token));
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        ExplorationTextWaitTests.DrainTextWork(session);
        var cleared = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Assert.Equal(4, cleared.DisplayStart); Assert.Equal(7, cleared.DisplayEnd);
        Assert.Equal("\n" + name, cleared.Projection);
        Assert.Same(first.DisplayNames, cleared.DisplayNames);
        Assert.True(cleared.Wait2);
        Accept(session, new CompleteTextReveal(cleared.Token)); Accept(session, new WaitForText(cleared.Token));
        Accept(session, new Acknowledge(cleared.Token));
        var empty = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Assert.Equal(4, empty.DisplayStart); Assert.Equal(8, empty.DisplayEnd);
        Assert.NotEqual(cleared.Token, empty.Token);
    }

    [Theory]
    [InlineData("Leader")]
    [InlineData("a B-7")]
    public void CapturedCustomNameKeepsLiteralIdentityEvenWhenItMatchesTheDefault(string name)
    {
        var session = ExplorationTextWaitTests.StartFieldText("{NAME;0}{W1}");
        var definition = new Sf2.Remake.Application.Content.Scenarios.ScenarioDefinition("named-text",
            OpeningRestoreTests.Fixture().Definition.Encounters.Values, exploration: session.Definition.Exploration);
        var current = session.Current;
        var named = current.WithStory(new StoryState(cursor: current.Story.Cursor, textSettings: current.Story.TextSettings,
            logicalText: current.Story.LogicalText, logicalView: current.Story.LogicalView, portraitWindow: current.Story.PortraitWindow,
            names: new Dictionary<string, string> { ["ally-0"] = name }));
        var story = ExplorationTextRunner.Begin(definition.Exploration!, named,
            new Sf2.Remake.Application.Content.Scenarios.ShowText(Sf2.Remake.Application.Content.Scenarios.TextDisplayMode.Single,
                new("ferryman"), TextId: 100), new(999), definition);
        var binding = Assert.Single(Assert.IsType<FieldTextWait>(story.Wait).DisplayNames!);
        Assert.Equal("Leader", binding.CanonicalName); Assert.Equal(name, binding.CustomName);
        Assert.Equal("ally-0", binding.Actor!.Value.Value);
        Assert.Equal(name, Assert.IsType<FieldTextWait>(story.Wait).Projection);
        Assert.Empty(current.Story.Names);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HealRetainsAcceptedDisplayOperandsUntilSingleRecoveryCompletes(bool other)
    {
        var session = Start("healing-rule-demo", document => document["start"]!["actors"]![1]!["hp"] = 30);
        var target = new Sf2.Remake.Domain.Battles.ActorRef(other ? "guard-a" : "medic-a");
        Accept(session, new Confirm()); Accept(session, new SelectSpell(new("mend", 1)));
        Accept(session, new SelectTarget(target)); Accept(session, new Confirm());
        var display = session.Current.BattleScene!.Healing!.Display;
        Assert.Equal(target, display.Target); Assert.Equal(new("medic-a"), display.Caster);
        Assert.Equal(new("mend", 1), display.Spell); Assert.Null(display.ActionSourceText); Assert.Null(display.RecoverySourceText);
        Assert.Equal(other ? "Guard A" : "Medic A", display.TargetName);
        Assert.Contains(target.Value, session.Current.BattleScene.Healing.RecoveryText);
        Assert.Contains(display.RecoveredHp.ToString(), session.Current.BattleScene.Healing.RecoveryText);
        for (int step = 0; step < 5000 && session.Current.BattleScene is { } scene; step++)
        {
            Assert.Same(display, scene.Healing!.Display);
            Accept(session, scene.Healing is { LogicalComplete: false, AtTimedInput: false } ? new AdvanceSimulation(scene.Token) :
                scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind));
        }
        Assert.Null(session.Current.BattleScene);
        Assert.Equal(5, session.Current.Battle.GetActor(new("medic-a")).Mp);
        Assert.Equal(30 + display.RecoveredHp, session.Current.Battle.GetActor(target).Hp);
    }
}
