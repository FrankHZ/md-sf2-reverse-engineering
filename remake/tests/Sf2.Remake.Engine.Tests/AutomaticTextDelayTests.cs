using System.Text.Json;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Localization;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class AutomaticTextDelayTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 0)]
    [InlineData(3, 1)]
    public void AutomaticDelayConsumes119ServicesAndReturnsWithoutAcknowledgement(int speed, int mouthControl)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{D2}", speed: speed,
            configure: document => document["start"]!["textSettings"]!["mouthControl"] = mouthControl);
        var originalParty = session.Current.Exploration!.Party;
        var token = session.Current.Story.Wait!.Token;
        Accept(session, new CompleteTextReveal(token));
        for (int i = 0; session.Current.Story.Wait is FieldTextWait { AutomaticDelay: false } && i < 40; i++)
            Accept(session, new AdvanceSimulation(token));
        var paused = session.Current;
        var delay = Assert.IsType<FieldTextWait>(paused.Story.Wait);
        Assert.True(delay.AutomaticDelay);
        Assert.False(delay.InterruptibleDelay);
        Assert.Equal(119, delay.Remaining);
        Assert.False(paused.Story.Typewriting);
        Assert.True(delay.SavedTypewriting);
        Assert.Equal("text-delay-unavailable", Send(session, new InterruptTextDelay(token)).Failure!.Code);
        Assert.Equal("text-delay-unavailable", Send(session, new InterruptTextDelay(token) { FromReadingConfirm = true }).Failure!.Code);
        Assert.Same(paused, session.Current);
        Accept(session, new AdvanceSimulation(token, 118));
        Assert.Equal(1, Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Remaining);
        Assert.Equal(paused.Story.SimulationTick + 118, session.Current.Story.SimulationTick);
        Accept(session, new AdvanceSimulation(token));
        Assert.IsType<TextCloseWait>(session.Current.Story.Wait);
        Assert.Equal(paused.Story.SimulationTick + 119, session.Current.Story.SimulationTick);
        for (int i = 0; !session.Current.CanWaitAtInput && i < 20; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.True(session.Current.CanWaitAtInput);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.Same(originalParty, session.Current.Exploration!.Party);
    }

    [Theory]
    [InlineData("{NAME;1}等待。{D2}", true)]
    [InlineData("{NAME;1}等待。", false)]
    [InlineData("{NAME;1}等待。{D3}", false)]
    [InlineData("等待。{D2}{NAME;1}", false)]
    public void LocalizedAutomaticTextPreservesDelayAndCapturedNameTopology(string translated, bool accepted)
    {
        var session = ExplorationTextWaitTests.StartFieldText("{NAME;1} waits.{D2}");
        var before = session.Current;
        var admission = DisplayCatalogReader.Read(JsonSerializer.SerializeToUtf8Bytes(new
        {
            package = session.Definition.Package, locale = "zh-CN",
            texts = new[] { new { @namespace = "world.text", id = 100, text = translated } },
            names = new[] { new { @namespace = "name.ally", index = 1, text = "访客" } },
        }), session.Definition);
        Assert.Null(admission.Error);
        var display = admission.Catalog!.Resolve(Assert.IsType<FieldTextWait>(before.Story.Wait), session.Definition.Exploration!);
        Assert.Equal(accepted ? "zh-CN" : "en", display.EffectiveLocale);
        Assert.Equal(accepted ? "访客等待。" : "Name waits.", display.Text);
        Assert.Equal(accepted ? null : "invalid-translation", display.Reason);
        Assert.Same(before, session.Current);
    }
}
