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
}
