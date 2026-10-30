using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ChoiceIntentTests
{
    [Theory]
    [InlineData(true, false, 37)]
    [InlineData(false, true, 943)]
    public void ExplicitAnswerUsesClickedOptionAndRetainsClosingAndNextQuestion(bool selected, bool answer, int flag)
    {
        var session = Source(flag);
        var token = session.Current.Story.Wait!.Token;
        Accept(session, new ChoiceHeldInput(token, true));
        Accept(session, new AdvanceSimulation(token, 600));
        Assert.Equal(ChoicePhase.Release, Work(session).Phase);
        RejectAnswer(session, token, answer);
        Accept(session, new ChoiceHeldInput(token, false));
        Accept(session, new PollChoice(token, selected ? ChoiceInput.Left : ChoiceInput.Right));
        var accepted = Accept(session, new ChooseDialogue(token, answer));
        Assert.Equal(ChoicePhase.Closing, Work(session).Phase);
        Assert.Equal(answer, Work(session).Yes);
        Assert.DoesNotContain(flag, session.Current.Story.Flags);
        Assert.Single(accepted.Observations, row => row.Kind == "choice-accepted");
        RejectAnswer(session, token, !answer);
        Accept(session, new AdvanceSimulation(token, 5));
        Assert.Equal(ChoicePhase.ReturnDelay, Work(session).Phase);
        Assert.Equal(answer, session.Current.Story.Flags.Contains(flag));
        RejectAnswer(session, token, !answer);
        Accept(session, new AdvanceSimulation(token, 10));
        Assert.NotEqual(token, session.Current.Story.Wait!.Token);
        Assert.Equal(ChoicePhase.Entry, Work(session).Phase);
        RejectAnswer(session, token, answer);
        RejectAnswer(session, session.Current.Story.Wait.Token, answer);
        Assert.DoesNotContain(flag + 1, session.Current.Story.Flags);
    }

    [Fact]
    public void SourceAnswerRejectsEntryOpeningAndWrongToken()
    {
        var session = Source(51);
        var token = session.Current.Story.Wait!.Token;
        RejectAnswer(session, token, false);
        Accept(session, new ChoiceHeldInput(token, false));
        Assert.Equal(ChoicePhase.Opening, Work(session).Phase);
        RejectAnswer(session, token, true);
        Accept(session, new AdvanceSimulation(token, 5));
        RejectAnswer(session, new(token.Value + 1), true);
        Accept(session, new ChooseDialogue(token, true));
        Assert.Equal(ChoicePhase.Closing, Work(session).Phase);
        Assert.True(Work(session).Yes);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AuthoredAnswerStillCompletesDirectlyAndRejectsReplay(bool yes)
    {
        var session = Start("harbor-arrival");
        Accept(session, new Interact(new("ferryman")));
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        var wait = Assert.IsType<ChoiceWait>(session.Current.Story.Wait);
        Assert.Null(wait.Work);
        Accept(session, new ChooseDialogue(wait.Token, yes));
        Assert.Equal(yes, session.Current.Story.Flags.Contains(10));
        RejectAnswer(session, wait.Token, !yes);
    }

    private static ChoiceWork Work(GameSession session) => Assert.IsType<ChoiceWait>(session.Current.Story.Wait).Work!;
    private static void RejectAnswer(GameSession session, WaitToken token, bool yes)
    {
        var before = session.Current;
        Assert.NotNull(Send(session, new ChooseDialogue(token, yes)).Failure);
        Assert.Same(before, session.Current);
    }
    private static GameSession Source(int flag) => ExplorationTextWaitTests.StartFieldText("{W1}", configure: document =>
        document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse($$"""
            [{"op":"yes-no","flag":{{flag}}},{"op":"yes-no","flag":{{flag + 1}}},{"op":"end"}]
            """));
}
