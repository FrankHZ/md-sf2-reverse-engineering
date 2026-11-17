using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ServiceGreetingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LongTextDelayReturnsOnTimeoutOrInputWithoutConsumingTheFollowingWait(bool interrupt)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{D3}B{W2}");
        for (int i = 0; i < 40 && session.Current.Story.Wait is not FieldTextWait { InterruptibleDelay: true }; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var delay = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Assert.Equal(120, delay.Remaining);
        Assert.False(session.Current.Story.Typewriting);
        int ticks = interrupt ? 1 : 119;
        Accept(session, new AdvanceSimulation(delay.Token, ticks));
        Assert.Equal(120 - ticks, Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Remaining);
        long before = session.Current.Story.SimulationTick;
        if (interrupt) Accept(session, new InterruptTextDelay(delay.Token));
        else Accept(session, new AdvanceSimulation(delay.Token));
        Assert.Equal(before + (interrupt ? 0 : 1), session.Current.Story.SimulationTick);
        Assert.True(session.Current.Story.Typewriting);
        Accept(session, new AdvanceSimulation(delay.Token, 20));
        Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Accept(session, new CompleteTextReveal(delay.Token));
        Assert.True(session.Current.CanWaitForText);
        Assert.NotNull(Send(session, new InterruptTextDelay(delay.Token)).Failure);
        Assert.True(session.Current.CanWaitForText);
    }

    [Theory]
    [InlineData(false, 11)] [InlineData(true, 31)]
    public void CaravanMessagesSelectTheirOwnPortraitAndReturnThroughTheActualCaller(bool astral, int portrait)
    {
        var session = ExplorationTextWaitTests.StartFieldText("A{W1}", interaction: true, startEvent: false, configure: doc =>
        {
            var portraits = doc["world"]!["presentation"]!["portraits"]!.AsArray();
            foreach (int id in new[] { 11, 31 })
            { var copy = portraits[0]!.DeepClone(); copy["portrait"] = id; portraits.Add(copy); }
        });
        var source = session.Definition.Exploration!;
        var texts = source.Texts.ToDictionary(p => p.Key, p => p.Value);
        texts[2] = "A{W1}"; texts[10] = "B{W1}";
        List<StoryProgram> programs = [new("service", [new OpenCaravan(), new WriteFlag(998, true), new EndProgram()])];
        PrivateServicePrograms.Bind(programs, texts);
        var tokens = source.TextTokens.ToDictionary(p => p.Key, p => p.Value);
        tokens[2] = tokens[100]; tokens[10] = tokens[101];
        var world = new ExplorationDefinition(source.Maps.Values, programs, texts,
            partyFlags: new(2, 0, 32, 12), visuals: source.Visuals, memberNames: source.MemberNames, textTokens: tokens, textFont: source.TextFont);
        var definition = new ScenarioDefinition("service-greeting", session.Definition.Encounters.Values, exploration: world);
        var before = session.Current;
        var current = before.WithStory(before.Story.Copy(new("service", 0), flags: astral ? [70] : [],
            eventCaller: new EntityEventContext(new("ferryman"), 3, 0)));
        var run = ProgramRunner.Run(definition, current, []); Assert.Null(run.Failure); current = run.Snapshot;
        Assert.Equal(portrait, Assert.IsType<OpenPortraitWindow>(current.Story.PortraitWindow).Portrait);
        var shown = new HashSet<int>(); bool entered = false;
        for (int i = 0; i < 1000 && !current.CanWaitAtInput; i++)
        {
            if (current.Story.TextWindow is OpenTextWindow text) shown.Add(text.Text);
            if (current.Story.PortraitWindow is OpenPortraitWindow open) Assert.Equal(portrait, open.Portrait);
            SessionResult result;
            if (current.Story.Wait is CaravanWait caravan)
            { Assert.False(entered); entered = true; result = CaravanService.Submit(definition, current, new BackCaravan(caravan.Token), session.Rules); }
            else
            {
                SessionCommand command = current.CanWaitForText ? new Acknowledge(current.Story.Wait!.Token)
                    : current.Story.Wait is FieldTextWait { LogicalDone: true, Revealed: false } reveal ? new CompleteTextReveal(reveal.Token)
                    : new AdvanceSimulation(current.Story.Wait!.Token);
                result = ExplorationDispatcher.Submit(definition, current, command);
            }
            Assert.Null(result.Failure); current = result.Snapshot;
        }
        Assert.True(entered); Assert.True(current.CanWaitAtInput); Assert.Equal(new[] { 2, 10 }, shown.Order());
        Assert.Contains(998, current.Story.Flags); Assert.Empty(current.Story.Callers);
        Assert.IsType<ClosedPortraitWindow>(current.Story.PortraitWindow);
        Assert.Equal(before.Exploration!.Party.Actors, current.Exploration!.Party.Actors);
    }
}
